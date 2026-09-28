import { refreshSession } from "./api";

const API_BASE = import.meta.env.VITE_API_URL || "https://e-commerce-sooq-gzgdczg3g8gvg8du.polandcentral-01.azurewebsites.net/api/v1";
const RECONNECT_BASE_DELAY = 3000;
const RECONNECT_MAX_DELAY = 30000;
const TOKEN_REFRESH_SKEW_SECONDS = 60;

function isTokenNearExpiry(token) {
  try {
    const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const claims = JSON.parse(window.atob(payload));
    return !Number.isFinite(claims.exp) || claims.exp <= Date.now() / 1000 + TOKEN_REFRESH_SKEW_SECONDS;
  } catch {
    return true;
  }
}

export function connectOrderNotifications(onNotification) {
  let stopped = false;
  let socket = null;
  let reconnectTimer = null;
  let pingTimer = null;
  let reconnectDelay = RECONNECT_BASE_DELAY;
  let connecting = false;
  const hubUrl = API_BASE
    .replace(/\/api\/v\d+\/?$/i, "")
    .replace(/^https:/i, "wss:")
    .replace(/^http:/i, "ws:");

  const scheduleReconnect = () => {
    if (stopped || reconnectTimer !== null) return;
    reconnectTimer = window.setTimeout(() => {
      reconnectTimer = null;
      void connect();
    }, reconnectDelay);
    reconnectDelay = Math.min(reconnectDelay * 2, RECONNECT_MAX_DELAY);
  };

  const connect = async () => {
    if (stopped || connecting) return;
    connecting = true;
    let token = localStorage.getItem("accessToken");
    if (!token) {
      connecting = false;
      return;
    }

    if (isTokenNearExpiry(token)) {
      token = await refreshSession();
      if (!token) {
        connecting = false;
        return;
      }
    }

    if (stopped) {
      connecting = false;
      return;
    }

    const currentSocket = new WebSocket(`${hubUrl}/hubs/orders?access_token=${encodeURIComponent(token)}`);
    socket = currentSocket;
    connecting = false;
    let buffer = "";
    let handshakeComplete = false;

    currentSocket.onopen = () => {
      reconnectDelay = RECONNECT_BASE_DELAY;
      currentSocket.send(`${JSON.stringify({ protocol: "json", version: 1 })}\x1e`);
    };
    currentSocket.onmessage = (event) => {
      buffer += event.data;
      const frames = buffer.split("\x1e");
      buffer = frames.pop() ?? "";

      for (const frame of frames) {
        if (!frame) continue;
        let message;
        try { message = JSON.parse(frame); } catch { continue; }

        if (!handshakeComplete) {
          handshakeComplete = true;
          if (message.error) {
            currentSocket.close();
            return;
          }
          pingTimer = window.setInterval(() => {
            if (currentSocket.readyState === WebSocket.OPEN) currentSocket.send('{"type":6}\x1e');
          }, 15000);
          continue;
        }

        if (message.type === 1 && ["NewOrder", "OrderStatusChanged"].includes(message.target)) {
          onNotification(message.arguments?.[0]);
        } else if (message.type === 7) {
          currentSocket.close();
          return;
        }
      }
    };

    currentSocket.onclose = () => {
      window.clearInterval(pingTimer);
      if (socket === currentSocket) socket = null;
      scheduleReconnect();
    };
    currentSocket.onerror = () => currentSocket.close();
  };

  void connect();
  return () => {
    stopped = true;
    window.clearTimeout(reconnectTimer);
    window.clearInterval(pingTimer);
    socket?.close();
  };
}
