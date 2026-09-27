import { useEffect, useState } from "react";
import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import Navbar from "./components/Navbar";
import Home from "./pages/Home";
import ProductPage from "./pages/ProductPage";
import CartPage from "./pages/CartPage";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import NotFound from "./pages/NotFound";
import CheckoutPage from "./pages/CheckoutPage";
import ThankYouPage from "./pages/ThankYouPage";
import PaymentResultPage from "./pages/PaymentResultPage";
import Footer from "./components/Footer/Footer";
import { api, getApiErrorMessage, normalizeProduct } from "./api";
import AdminDashboard from "./pages/AdminDashboard";
import AdminOrders from "./pages/AdminOrders";
import AdminOrderDirectory from "./pages/AdminOrderDirectory";
import AdminSalesStatistics from "./pages/AdminSalesStatistics";
import AdminCustomers from "./pages/AdminCustomers";
import AdminProductForm from "./pages/AdminProductForm";
import AdminCategoryForm from "./pages/AdminCategoryForm";
import CustomerProfile from "./pages/CustomerProfile";
import { getAccessTokenSubject, isAdmin } from "./auth";
import { connectOrderNotifications } from "./orderNotifications";

function ScrollToPageLocation() {
  const { pathname, search, hash } = useLocation();

  useEffect(() => {
    let frame = 0;
    if (hash) {
      frame = window.requestAnimationFrame(() => {
        const target = document.getElementById(decodeURIComponent(hash.slice(1)));
        if (target) target.scrollIntoView({ behavior: "smooth", block: "start" });
        else window.scrollTo({ top: 0, left: 0, behavior: "auto" });
      });
    } else {
      window.scrollTo({ top: 0, left: 0, behavior: "auto" });
    }

    return () => window.cancelAnimationFrame(frame);
  }, [pathname, search, hash]);

  return null;
}

function App() {
  const [cart, setCart] = useState(() => {
    return JSON.parse(localStorage.getItem("cart")) || [];
  });

  const [user, setUser] = useState(() => {
    return JSON.parse(localStorage.getItem("user")) || null;
  });
  const [searchTerm, setSearchTerm] = useState("");
  const [theme, setTheme] = useState(() => localStorage.getItem("sooq-theme") || "light");
  const [likedProductIds, setLikedProductIds] = useState([]);
  const [catalog, setCatalog] = useState(() => {
    try { return (JSON.parse(localStorage.getItem("catalog")) || []).map(normalizeProduct); } catch { return []; }
  });
  const notificationStorageKey = user
    ? `sooq:notifications:${getAccessTokenSubject() || user.id || user.email || "account"}`
    : null;
  const [notifications, setNotifications] = useState(() => {
    if (!notificationStorageKey) return [];
    try { return JSON.parse(localStorage.getItem(notificationStorageKey)) || []; } catch { return []; }
  });

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    localStorage.setItem("sooq-theme", theme);
  }, [theme]);

  useEffect(() => {
    api.products().then((products) => {
      localStorage.setItem("catalog", JSON.stringify(products));
      setCatalog(products.map(normalizeProduct));
      const stockById = new Map(products.map((product) => [Number(product.id ?? product.productId), Number(product.stockQuantity)]));
      setCart((currentCart) => currentCart.map((item) => stockById.has(Number(item.id))
        ? { ...item, stockQuantity: stockById.get(Number(item.id)) }
        : item));
    }).catch(() => {});
  }, []);

  useEffect(() => {
    localStorage.setItem("cart", JSON.stringify(cart));
  }, [cart]);

  const addToCart = (product, amount = 1) => {
    const requestedQuantity = Math.max(1, Math.floor(Number(amount) || 1));
    setCart((currentCart) => {
      const existingProduct = currentCart.find(
        (item) => item.id === product.id
      );
      const stockQuantity = product.stockQuantity == null ? Infinity : Math.max(0, Number(product.stockQuantity));
      const availableQuantity = Math.max(0, stockQuantity - (existingProduct?.quantity ?? 0));
      const quantityToAdd = Math.min(requestedQuantity, availableQuantity);
      if (quantityToAdd === 0) return currentCart;

      if (existingProduct) {
        return currentCart.map((item) =>
          item.id === product.id
            ? { ...item, stockQuantity, quantity: item.quantity + quantityToAdd }
            : item
        );
      }

      return [...currentCart, { ...product, quantity: quantityToAdd }];
    });
  };

  const removeFromCart = (id) => {
    setCart((currentCart) =>
      currentCart.filter((item) => item.id !== id)
    );
  };

  const updateQuantity = (id, quantity) => {
    if (quantity < 1) {
      removeFromCart(id);
      return;
    }

    setCart((currentCart) => currentCart.map((item) => {
      if (item.id !== id) return item;
      if (quantity > item.quantity && item.stockQuantity != null) {
        const maximum = Math.max(0, Number(item.stockQuantity));
        if (maximum <= item.quantity) return item;
        return { ...item, quantity: Math.min(quantity, maximum) };
      }
      return { ...item, quantity };
    }));
  };

  const login = (userData) => {
    localStorage.setItem("accessToken", userData.accessToken);
    localStorage.setItem("refreshToken", userData.refreshToken);
    localStorage.setItem("user", JSON.stringify(userData));
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    setUser(null);
  };

  useEffect(() => {
    const handleSessionExpired = () => setUser(null);
    window.addEventListener("auth:logout", handleSessionExpired);
    return () => window.removeEventListener("auth:logout", handleSessionExpired);
  }, []);

  useEffect(() => {
    let active = true;
    if (!user) {
      setLikedProductIds([]);
      return () => { active = false; };
    }
    api.myLikes().then((ids) => {
      if (active && Array.isArray(ids)) setLikedProductIds(ids.map(Number));
    }).catch(() => {});
    return () => { active = false; };
  }, [user]);

  const toggleLike = async (product) => {
    if (!localStorage.getItem("accessToken")) {
      window.location.assign("/login?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search + window.location.hash));
      return;
    }
    const productId = Number(product.id ?? product.productId);
    const wasLiked = likedProductIds.includes(productId);
    setLikedProductIds((current) => wasLiked ? current.filter((id) => id !== productId) : [...current, productId]);
    try {
      if (wasLiked) await api.removeLike(productId);
      else await api.addLike(productId);
    } catch (error) {
      setLikedProductIds((current) => wasLiked ? [...current, productId] : current.filter((id) => id !== productId));
      if (error?.status === 401) {
        window.location.assign("/login?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search + window.location.hash));
      }
    }
  };

  const deleteProduct = async (product) => {
    if (!window.confirm(`هل تريد حذف «${product.title}»؟`)) return;
    try {
      await api.deleteProduct(product.id);
      setCatalog((current) => {
        const next = current.filter((item) => Number(item.id) !== Number(product.id));
        localStorage.setItem("catalog", JSON.stringify(next));
        return next;
      });
      return true;
    } catch (error) {
      window.alert(getApiErrorMessage(error, "ما قدرنا نحذف المنتج. إذا كان ضمن طلب سابق، يبقى محفوظاً في سجل الطلبات."));
      return false;
    }
  };

  useEffect(() => {
    if (!notificationStorageKey) {
      setNotifications([]);
      return undefined;
    }
    try { setNotifications(JSON.parse(localStorage.getItem(notificationStorageKey)) || []); } catch { setNotifications([]); }
    let active = true;
    api.myNotifications().then((saved) => {
      if (!active || !Array.isArray(saved)) return;
      setNotifications((current) => {
        const merged = new Map();
        [...current, ...saved.map((notification) => ({ ...notification, read: Boolean(notification.isRead ?? notification.read) }))]
          .forEach((notification) => merged.set(String(notification.id), notification));
        const next = [...merged.values()].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 30);
        localStorage.setItem(notificationStorageKey, JSON.stringify(next));
        return next;
      });
    }).catch(() => {});
    const disconnect = connectOrderNotifications((notification) => {
      if (!notification) return;
      setNotifications((current) => {
        const normalized = { ...notification, read: false };
        const next = [normalized, ...current.filter((item) => String(item.id) !== String(notification.id))].slice(0, 30);
        localStorage.setItem(notificationStorageKey, JSON.stringify(next));
        return next;
      });
    });
    return () => { active = false; disconnect?.(); };
  }, [notificationStorageKey]);

  const markNotificationsRead = () => {
    if (!notificationStorageKey) return;
    setNotifications((current) => {
      const next = current.map((notification) => ({ ...notification, read: true }));
      localStorage.setItem(notificationStorageKey, JSON.stringify(next));
      return next;
    });
    api.markAllNotificationsRead().catch(() => {});
  };

  const clearNotifications = () => {
    if (notificationStorageKey) localStorage.removeItem(notificationStorageKey);
    setNotifications([]);
    api.clearMyNotifications().catch(() => {});
  };

  const cartCount = cart.reduce(
    (total, item) => total + item.quantity,
    0
  );

  return (
    <>
      <ScrollToPageLocation />
      <Navbar
        cartCount={cartCount}
        notifications={notifications}
        onNotificationsRead={markNotificationsRead}
        onClearNotifications={clearNotifications}
        user={user}
        logout={logout}
        searchTerm={searchTerm}
        setSearchTerm={setSearchTerm}
        products={catalog}
        cartItems={cart}
        updateQuantity={updateQuantity}
        removeFromCart={removeFromCart}
        theme={theme}
        onToggleTheme={() => setTheme((current) => current === "dark" ? "light" : "dark")}
      />

      <main>
        <Routes>
          <Route
            path="/"
            element={<Home addToCart={addToCart} search={searchTerm} setSearch={setSearchTerm} likedProductIds={likedProductIds} toggleLike={toggleLike} isAdmin={Boolean(user && isAdmin())} onDeleteProduct={deleteProduct} />}
          />

          <Route
            path="/product/:id"
            element={<ProductPage addToCart={addToCart} cart={cart} likedProductIds={likedProductIds} toggleLike={toggleLike} />}
          />

          <Route
            path="/cart"
            element={
              <CartPage
                cart={cart}
                updateQuantity={updateQuantity}
                removeFromCart={removeFromCart}
              />
            }
          />

          <Route
            path="/checkout"
            element={<CheckoutPage cart={cart} user={user} setCart={setCart} />} 
          />

          <Route
          path="/thank-you"
          element={<ThankYouPage setCart={setCart}/>} 
          />
          <Route path="/payment/result" element={<PaymentResultPage />} />
          <Route
            path="/login"
            element={<Login onLogin={login} />}
          />

          <Route
            path="/admin/login"
            element={<Login onLogin={login} adminOnly />}
          />

          <Route
            path="/signup"
            element={<Signup onLogin={login} />}
          />

          <Route
            path="/dashboard"
            element={user && isAdmin() ? <AdminDashboard /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/orders"
            element={user && isAdmin() ? <AdminOrderDirectory /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/statistics"
            element={user && isAdmin() ? <AdminSalesStatistics /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/customers"
            element={user && isAdmin() ? <AdminCustomers /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/products/new"
            element={user && isAdmin() ? <AdminProductForm /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/products/:id/edit"
            element={user && isAdmin() ? <AdminProductForm /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/dashboard/categories/new"
            element={user && isAdmin() ? <AdminCategoryForm /> : <Navigate to={user ? "/" : "/admin/login"} replace />}
          />

          <Route
            path="/profile"
            element={user && !isAdmin() ? <CustomerProfile user={user} logout={logout} /> : <Navigate to={user ? "/dashboard" : "/login"} replace />}
          />

          <Route path="*" element={<NotFound />} />
        </Routes>
        <Footer />
      </main>
    </>
  );
}

export default App;
