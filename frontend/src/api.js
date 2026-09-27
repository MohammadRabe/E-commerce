const API_BASE = import.meta.env.VITE_API_URL || "https://localhost:7280/api/v1";
const orderStatusValues = { Pending: 0, Processing: 1, Shipped: 2, Delivered: 3, Cancelled: 4 };

let refreshPromise = null;

export class ApiError extends Error {
  constructor(message, { status, errors = [], payload } = {}) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
    this.payload = payload;
  }
}

function collectErrorMessages(value, fieldName = "") {
  if (Array.isArray(value)) return value.flatMap((item) => collectErrorMessages(item, fieldName));
  if (typeof value === "string") return value.trim() ? [fieldName ? `${fieldName}: ${value.trim()}` : value.trim()] : [];
  if (value && typeof value === "object") {
    if (value.message || value.description || value.errorMessage) {
      return collectErrorMessages(value.message || value.description || value.errorMessage, fieldName);
    }
    return Object.entries(value).flatMap(([field, messages]) => collectErrorMessages(messages, fieldName || field));
  }
  return [];
}

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
    const details = collectErrorMessages(payload?.errors ?? payload?.validationErrors ?? payload?.ModelState ?? payload?.modelState);
    const summary = collectErrorMessages(payload?.message ?? payload?.detail ?? payload?.title ?? payload?.errorMessage ?? payload?.error);
    const usefulSummary = summary.filter((message) => !/^(bad request|validation failed|request validation failed)$/i.test(message));
    const errors = [...new Set([...details, ...usefulSummary])];
    const errorMessage = errors.join("\n") || `Request failed (${response.status})`;
    throw new ApiError(errorMessage, { status: response.status, errors, payload });
  }
  return payload?.data ?? payload;
}

function translateApiMessage(message) {
  const value = message.replace(/^(username|user name|email|password)\s*:\s*/i, "").trim();
  const username = value.match(/username\s+['\"](.+?)['\"]\s+is already taken\.?/i);
  if (username || /username.{0,40}(already taken|already exists|in use)/i.test(value)) {
    return username ? `اسم المستخدم «${username[1]}» مستخدم من قبل. جرّب اسم ثاني.` : "اسم المستخدم مستخدم من قبل. جرّب اسم ثاني.";
  }
  const email = value.match(/email\s+['\"](.+?)['\"]\s+is already taken\.?/i);
  if (email || /email.{0,40}(already taken|already exists|in use)/i.test(value)) {
    return email ? `البريد الإلكتروني «${email[1]}» مسجل من قبل. استخدم بريد ثاني أو سجّل دخولك.` : "البريد الإلكتروني مسجل من قبل. استخدم بريد ثاني أو سجّل دخولك.";
  }
  if (/invalid username or password/i.test(value)) return "اسم المستخدم أو البريد الإلكتروني أو كلمة المرور غير صحيحة.";
  if (/^unauthorized\.?$/i.test(value)) return "سجّل دخولك عشان تكمل هالعملية.";
  if (/^forbidden\.?$/i.test(value)) return "ما عندك صلاحية لتنفيذ هالعملية.";
  if (/^(not found|resource not found)\.?$/i.test(value)) return "ما لقينا المطلوب. يمكن تغيّر أو انحذف.";
  if (/product.{0,50}(doesn't exist|does not exist|not found)/i.test(value)) return "المنتج غير موجود أو انحذف من المتجر.";
  if (/^conflict\.?$/i.test(value)) return "الطلب يتعارض مع الحالة الحالية. حدّث الصفحة وحاول مرة ثانية.";
  if (/category.{0,50}(already exists|already taken|duplicate)/i.test(value)) return "هذه الفئة موجودة بالفعل. اختر اسماً مختلفاً.";
  if (/^(bad request|validation failed|request validation failed)\.?$/i.test(value)) return "راجع البيانات المدخلة وتأكد إنها صحيحة.";
  if (/failed to fetch|networkerror|fetch failed/i.test(value)) return "ما قدرنا نتصل بالخادم. تأكد من اتصالك وحاول مرة ثانية.";
  if (/username.{0,50}(must not be empty|required)/i.test(value)) return "اكتب اسم المستخدم.";
  if (/email.{0,50}(not a valid email|invalid email)/i.test(value)) return "اكتب بريد إلكتروني بصيغة صحيحة.";
  if (/email.{0,50}(must not be empty|required)/i.test(value)) return "اكتب البريد الإلكتروني.";
  if (/password.{0,60}(at least 6|minimum length|must be at least 6)/i.test(value)) return "كلمة المرور لازم تكون 6 خانات على الأقل.";
  if (/password.{0,60}(at least one digit|digit)/i.test(value)) return "أضف رقم واحد على الأقل لكلمة المرور.";
  if (/password.{0,60}(non.?alphanumeric|special character)/i.test(value)) return "أضف رمز مثل ! أو @ لكلمة المرور.";
  if (/password.{0,50}(must not be empty|required)/i.test(value)) return "اكتب كلمة المرور.";
  if (/^request failed \(\d+\)$/i.test(value)) return "صار خطأ في الطلب. جرّب مرة ثانية.";
  return value;
}

export function getApiErrorMessage(error, fallback) {
  const friendlyFallback = fallback || "صار خطأ غير متوقع. جرّب مرة ثانية.";
  if (error instanceof ApiError && error.status >= 500) {
    return "واجهنا مشكلة مؤقتة في الخادم. جرّب مرة ثانية بعد قليل.";
  }
  const rawMessages = error instanceof ApiError ? error.errors : [error?.message].filter(Boolean);
  const messages = [...new Set(rawMessages.map(translateApiMessage).filter(Boolean))];
  return messages.length ? messages.join("\n") : friendlyFallback;
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
    const isFormData = typeof FormData !== "undefined" && options.body instanceof FormData;
    return fetch(`${API_BASE}${path}`, {
      ...options,
      headers: {
        ...(options.body && !isFormData ? { "Content-Type": "application/json" } : {}),
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
  productPage: async (pageNumber = 1, pageSize = 12) => {
    const query = new URLSearchParams({ pageNumber, pageSize });
    return request(`/product/getPagedProducts?${query}`);
  },
  products: async (pageNumber = 1, pageSize = 100) => {
    const query = new URLSearchParams({ pageNumber, pageSize });
    const data = await request(`/product/getPagedProducts?${query}`);
    return data?.items ?? data ?? [];
  },
  createProduct: (formData) => request("/product/create", { method: "POST", body: formData }),
  updateProduct: (id, formData) => request(`/product/${encodeURIComponent(id)}`, { method: "PUT", body: formData }),
  deleteProduct: (id) => request(`/product/${encodeURIComponent(id)}`, { method: "DELETE" }),
  getProductById: (id) => request(`/product/getProductById/${encodeURIComponent(id)}`),
  categories: () => request("/category"),
  createCategory: (category) => request("/category", { method: "POST", body: JSON.stringify(category) }),
  signIn: async (userName, password) => saveTokens(await request("/auth/signIn", { method: "POST", body: JSON.stringify({ userName, password }) })),
  adminSignIn: async (userName, password) => saveTokens(await request("/auth/signIn", { method: "POST", body: JSON.stringify({ userName, password, adminOnly: true }) })),
  signUp: (userName, fullName, email, password) => request("/auth/signUp", { method: "POST", body: JSON.stringify({ userName, fullName, email, password }) }),
  signOut: clearSession,
  placeOrder: (order) => request("/order", { method: "POST", body: JSON.stringify(order) }),
  createOrderPayment: (orderId) => request(`/order/${encodeURIComponent(orderId)}/payment`, { method: "POST" }),
  verifyOrderPayment: (orderId) => request(`/order/${encodeURIComponent(orderId)}/payment/verify`, { method: "POST" }),
  ordersByStatus: async (status) => {
    const query = new URLSearchParams({ status });
    return (await request(`/order/status?${query}`)) ?? [];
  },
  myOrders: async () => (await request("/order")) ?? [],
  orderDetails: (orderId) => request(`/order/${orderId}`),
  updateOrderStatus: (orderId, status) => request(`/order/${orderId}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status: orderStatusValues[status] }),
  }),
  myNotifications: async () => (await request("/notifications/mine")) ?? [],
  markAllNotificationsRead: () => request("/notifications/read", { method: "PATCH" }),
  clearMyNotifications: () => request("/notifications/mine", { method: "DELETE" }),
  salesStatistics: () => request("/admin/getStatistics"),
  customers: async ({ pageNumber = 1, pageSize = 10, search = "" } = {}) => {
    const query = new URLSearchParams({ pageNumber, pageSize, search });
    return request(`/admin/getCustomers?${query}`);
  },
  deleteCustomer: (userId) => request(`/admin/customers/${encodeURIComponent(userId)}`, { method: "DELETE" }),
  adminOrders: async ({ pageNumber = 1, pageSize = 10, search = "", status = "" } = {}) => {
    const query = new URLSearchParams({ pageNumber, pageSize, search, status });
    return request(`/admin/getOrders?${query}`);
  },
  myLikes: async () => (await request("/userLikes")) ?? [],
  addLike: (productId) => request(`/userLikes/${encodeURIComponent(productId)}`, { method: "PUT" }),
  removeLike: (productId) => request(`/userLikes/${encodeURIComponent(productId)}`, { method: "DELETE" }),
};

export function normalizeProduct(product) {
  const rawImages = product.imageUrls ?? product.ImageUrls ?? product.images ?? product.Images ?? product.imagePaths ?? product.ImagePaths ?? [];
  const imageList = (Array.isArray(rawImages) ? rawImages : [rawImages])
    .map((item) => typeof item === "string" ? item : item?.url ?? item?.Url ?? item?.path ?? item?.Path)
    .filter(Boolean);
  const rawImage = product.imageUrl ?? product.ImageUrl ?? imageList[0] ?? product.image ?? product.Image ?? "";
  const resolveImageUrl = (value) => {
    if (typeof value !== "string") return "";
    const url = value.trim();
    if (!url) return "";
    if (/^https?:\/\//i.test(url)) {
      return typeof window !== "undefined" && window.location.protocol === "https:" && url.startsWith("http:")
        ? url.replace(/^http:/i, "https:")
        : url;
    }
    try {
      return new URL(url, new URL(API_BASE).origin).href;
    } catch {
      return url;
    }
  };
  const image = resolveImageUrl(rawImage);
  const images = imageList.map(resolveImageUrl);
  return {
    ...product,
    id: product.id ?? product.productId,
    title: product.title ?? "Untitled product",
    price: Number(product.price ?? 0),
    category: product.category ?? product.categoryName ?? "General",
    image,
    images: [...new Set([image, ...images].filter(Boolean))],
    rating: typeof product.rating === "object" ? product.rating : { rate: Number(product.rating ?? 0), count: Number(product.ratingCount ?? 0) },
  };
}
