const API_BASE = import.meta.env.VITE_API_URL || "https://e-commerce-sooq-gzgdczg3g8gvg8du.polandcentral-01.azurewebsites.net/api/v1";

export function connectOrderNotifications(onNotification) {
  let stopped = false;
  let socket = null;
  let reconnectTimer = null;
  let pingTimer = null;
  const hubUrl = API_BASE
    .replace(/\/api\/v\d+\/?$/i, "")
    .replace(/^https:/i, "wss:")
    .replace(/^http:/i, "ws:");

  const connect = () => {
    if (stopped) return;
    const token = localStorage.getItem("accessToken");
    if (!token) return;

    socket = new WebSocket(`${hubUrl}/hubs/orders?access_token=${encodeURIComponent(token)}`);
    let buffer = "";
    let handshakeComplete = false;

    socket.onopen = () => socket.send(`${JSON.stringify({ protocol: "json", version: 1 })}\x1e`);
    socket.onmessage = (event) => {
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
            socket.close();
            return;
          }
          pingTimer = window.setInterval(() => {
            if (socket?.readyState === WebSocket.OPEN) socket.send('{"type":6}\x1e');
          }, 15000);
          continue;
        }

        if (message.type === 1 && ["NewOrder", "OrderStatusChanged"].includes(message.target)) {
          onNotification(message.arguments?.[0]);
        } else if (message.type === 7) {
          socket.close();
          return;
        }
      }
    };

    socket.onclose = () => {
      window.clearInterval(pingTimer);
      if (!stopped) reconnectTimer = window.setTimeout(connect, 3000);
    };
    socket.onerror = () => socket.close();
  };

  connect();
  return () => {
    stopped = true;
    window.clearTimeout(reconnectTimer);
    window.clearInterval(pingTimer);
    socket?.close();
  };
}
