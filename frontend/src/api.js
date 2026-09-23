const API_BASE = import.meta.env.VITE_API_URL || "https://localhost:7280/api/v1";

let refreshPromise = null;

function saveTokens(tokens) {
  if (tokens?.accessToken) localStorage.setItem("accessToken", tokens.accessToken);
  if (tokens?.refreshToken) localStorage.setItem("refreshToken", tokens.refreshToken);
  return tokens;
}

function clearSession() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");
  window.dispatchEvent(new Event("auth:logout"));
}

async function readPayload(response) {
  const payload = await response.json().catch(() => null);
  if (!response.ok || payload?.isSuccess === false) {
    throw new Error(payload?.errors?.join(" ") || payload?.message || `Request failed (${response.status})`);
  }
  return payload?.data ?? payload;
}

async function refreshAccessToken() {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return null;

  if (!refreshPromise) {
    refreshPromise = fetch(`${API_BASE}/auth/refreshToken`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    })
      .then(readPayload)
      .then(saveTokens)
      .catch(() => {
        clearSession();
        return null;
      })
      .finally(() => { refreshPromise = null; });
  }

  return refreshPromise;
}

async function request(path, options = {}) {
  const send = () => {
    const token = localStorage.getItem("accessToken");
    return fetch(`${API_BASE}${path}`, {
      ...options,
      headers: {
        ...(options.body ? { "Content-Type": "application/json" } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...options.headers,
      },
    });
  };

  let response = await send();
  if (response.status === 401 && !path.startsWith("/auth/")) {
    const tokens = await refreshAccessToken();
    if (tokens?.accessToken) response = await send();
  }
  return readPayload(response);
}

export const api = {
  products: async (pageNumber = 1, pageSize = 100) => {
    const query = new URLSearchParams({ pageNumber, pageSize });
    const data = await request(`/product/getPagedProducts?${query}`);
    return data?.items ?? data ?? [];
  },
  product: (id) => request(`/product/getProductById/${id}`),
  categories: () => request("/category"),
  signIn: async (userName, password) => saveTokens(await request("/auth/signIn", { method: "POST", body: JSON.stringify({ userName, password }) })),
  signUp: (userName, email, password) => request("/auth/signUp", { method: "POST", body: JSON.stringify({ userName, email, password }) }),
  signOut: clearSession,
  placeOrder: (order) => request("/order", { method: "POST", body: JSON.stringify(order) }),
};

export function normalizeProduct(product) {
  const image = product.imageUrl || product.image || product.images?.[0] || "";
  return {
    ...product,
    id: product.id ?? product.productId,
    title: product.title ?? "Untitled product",
    price: Number(product.price ?? 0),
    category: product.category ?? product.categoryName ?? "General",
    image,
    rating: typeof product.rating === "object" ? product.rating : { rate: Number(product.rating ?? 0), count: Number(product.ratingCount ?? 0) },
  };
}
