import { NavLink, Link, useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import { isAdmin } from "../auth";
import SARPrice from "./SARPrice";
import { api, normalizeProduct } from "../api";

function Navbar({ cartCount, cartItems = [], updateQuantity, removeFromCart, user, logout, searchTerm, setSearchTerm, products = [], notifications = [], onNotificationsRead, onClearNotifications, theme = "light", onToggleTheme }) {
  const navigate = useNavigate();
  const [notificationsOpen, setNotificationsOpen] = useState(false);
  const [cartOpen, setCartOpen] = useState(false);
  const [cartClosing, setCartClosing] = useState(false);
  const [cartSide, setCartSide] = useState("left");
  const [cartDragging, setCartDragging] = useState(false);
  const [cartHasDragged, setCartHasDragged] = useState(false);
  const cartDragStart = useRef(null);
  const cartCloseTimer = useRef(null);
  const [searchFocused, setSearchFocused] = useState(false);
  const query = searchTerm.trim().toLocaleLowerCase("ar");
  const [suggestions, setSuggestions] = useState([]);
  const [suggestionsLoading, setSuggestionsLoading] = useState(false);
  const unreadCount = notifications.filter((notification) => !notification.read).length;
  const cartTotal = cartItems.reduce((total, item) => total + Number(item.price || 0) * Number(item.quantity || 0), 0);

  useEffect(() => {
    let active = true;
    const term = searchTerm.trim();
    if (!term) {
      setSuggestions([]);
      setSuggestionsLoading(false);
      return () => { active = false; };
    }
    setSuggestions([]);
    setSuggestionsLoading(true);

    const timer = window.setTimeout(() => {
      api.productPage(1, 6, term)
        .then((data) => {
          if (active) {
            setSuggestions((data?.items ?? data?.Items ?? []).map(normalizeProduct));
            setSuggestionsLoading(false);
          }
        })
        .catch(() => {
          if (active) {
            setSuggestions([]);
            setSuggestionsLoading(false);
          }
        });
    }, 250);

    return () => {
      active = false;
      window.clearTimeout(timer);
    };
  }, [searchTerm]);

  useEffect(() => {
    if (!cartOpen) return undefined;
    const closeOnEscape = (event) => { if (event.key === "Escape") closeCart(); };
    window.addEventListener("keydown", closeOnEscape);
    return () => {
      window.removeEventListener("keydown", closeOnEscape);
    };
  }, [cartOpen]);
  const closeCart = () => {
    if (!cartOpen || cartCloseTimer.current) return;
    setCartClosing(true);
    cartCloseTimer.current = window.setTimeout(() => {
      setCartOpen(false);
      setCartClosing(false);
      setCartDragging(false);
      cartCloseTimer.current = null;
    }, 280);
  };
  const startCartDrag = (event) => {
    if (event.button != null && event.button !== 0) return;
    if (!event.target.closest?.(".cart-drawer-header")) return;
    if (event.target.closest?.(".cart-drawer-control")) return;
    cartDragStart.current = { x: event.clientX, y: event.clientY, horizontal: false, pointerId: event.pointerId };
  };
  const moveCartDrag = (event) => {
    const start = cartDragStart.current;
    if (!start) return;
    const deltaX = event.clientX - start.x;
    const deltaY = event.clientY - start.y;
    if (!start.horizontal && Math.abs(deltaX) > 8 && Math.abs(deltaX) > Math.abs(deltaY)) {
      start.horizontal = true;
      event.currentTarget.setPointerCapture?.(start.pointerId);
    }
    if (start.horizontal) {
      event.preventDefault();
      setCartDragging(true);
    }
  };
  const endCartDrag = (event) => {
    const start = cartDragStart.current;
    if (!start) return;
    const deltaX = event.clientX - start.x;
    cartDragStart.current = null;
    setCartDragging(false);
    if (start.horizontal && Math.abs(deltaX) > 50) {
      setCartHasDragged(true);
      setCartSide(deltaX > 0 ? "right" : "left");
    }
  };
  const openNotification = (notification) => {
    const fallback = notification.type === "new-order"
      ? `/dashboard/orders?orderId=${notification.orderId}`
      : `/profile?orderId=${notification.orderId}`;
    const destination = notification.link?.startsWith("/") && !notification.link.startsWith("//")
      ? notification.link
      : fallback;
    setNotificationsOpen(false);
    navigate(destination);
  };
  const submitSearch = (event) => {
    event.preventDefault();
    setSearchFocused(false);
    navigate("/#products");
  };
  const openProduct = (product) => {
    setSearchFocused(false);
    navigate(`/product/${product.id}`);
  };

  return (
    <nav className="navbar navbar-expand-lg bg-dark navbar-dark sticky-top shadow-sm">
      <div className="container">
        <Link className="navbar-brand site-logo" to="/" aria-label="سوق، الصفحة الرئيسية">
          <span>سوق</span>
          <i className="bi bi-bag-fill" aria-hidden="true"></i>
        </Link>

        <form className="navbar-search" role="search" onSubmit={submitSearch} onBlur={(event) => { if (!event.currentTarget.contains(event.relatedTarget)) setSearchFocused(false); }}>
          <input type="search" aria-label="ابحث عن المنتجات" placeholder="وش تدور عليه؟" value={searchTerm} onFocus={() => setSearchFocused(true)} onChange={(event) => setSearchTerm(event.target.value)} onKeyDown={(event) => { if (event.key === "Escape") setSearchFocused(false); if (event.key === "ArrowDown" && suggestions.length) { event.preventDefault(); event.currentTarget.form?.querySelector(".search-suggestion")?.focus(); } }} />
          {searchTerm && <button type="button" className="navbar-search-clear" aria-label="مسح البحث" onClick={() => setSearchTerm("")}><i className="bi bi-x-lg" /></button>}
          <button type="submit" className="navbar-search-submit" aria-label="بحث"><i className="bi bi-search" /></button>
          {searchFocused && query && <div className="search-suggestions" role="listbox" aria-label="اقتراحات المنتجات">
            {suggestions.length ? suggestions.map((product) => <button type="button" role="option" aria-selected="false" className="search-suggestion" key={product.id} onClick={() => openProduct(product)}>
              <img src={product.image} alt="" loading="lazy" />
              <span className="search-suggestion-copy"><strong>{product.title}</strong><small>{product.category}</small></span>
              <i className="bi bi-arrow-up-left" aria-hidden="true" />
            </button>) : <p className="search-suggestions-empty">{suggestionsLoading ? "جاري البحث..." : "ما لقينا منتجات تطابق بحثك"}</p>}
          </div>}
        </form>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#mainNavbar"
          aria-controls="mainNavbar"
          aria-expanded="false"
          aria-label="فتح قائمة التنقل"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="mainNavbar">
          <div className="navbar-nav me-auto">
            <NavLink className="nav-link" to="/">
              تسوّق
            </NavLink>
            {user && (isAdmin()
              ? <NavLink className="nav-link" to="/dashboard">لوحة التحكم</NavLink>
              : <NavLink className="nav-link" to="/profile"><i className="bi bi-person-circle me-1" aria-hidden="true" />حسابي</NavLink>)}
          </div>

          <div className="d-flex align-items-center gap-2 nav-actions">
            <button type="button" className="theme-toggle" onClick={onToggleTheme} aria-label={theme === "dark" ? "تفعيل الوضع الفاتح" : "تفعيل الوضع الداكن"} title={theme === "dark" ? "الوضع الفاتح" : "الوضع الداكن"}>
              <i className={`bi ${theme === "dark" ? "bi-sun-fill" : "bi-moon-stars-fill"}`} aria-hidden="true" />
            </button>
            <div className="notification-menu">
              <button type="button" className="notification-trigger" aria-label={`الإشعارات${unreadCount ? `، ${unreadCount} غير مقروءة` : ""}`} aria-expanded={notificationsOpen} aria-controls="notification-panel" onClick={() => { setNotificationsOpen((open) => !open); if (!notificationsOpen) onNotificationsRead?.(); }}>
                <i className="bi bi-bell" aria-hidden="true" />
                {unreadCount > 0 && <span className="notification-count">{unreadCount > 99 ? "99+" : unreadCount}</span>}
              </button>
              {notificationsOpen && <div className="notification-panel" id="notification-panel" role="dialog" aria-label="الإشعارات">
                <div className="notification-panel-header"><strong>الإشعارات</strong>{notifications.length > 0 && <button type="button" onClick={onClearNotifications}>مسح الكل</button>}</div>
                {notifications.length ? <div className="notification-list">{notifications.map((notification) => <button type="button" className={`notification-item ${notification.read ? "" : "is-unread"}`} key={notification.id} onClick={() => openNotification(notification)}>
                  <span className="notification-item-icon"><i className={`bi ${notification.type === "new-order" ? "bi-bag-check" : "bi-arrow-repeat"}`} aria-hidden="true" /></span>
                  <span className="notification-item-copy"><strong>{notification.title}</strong><small>{notification.message}</small><time>{new Intl.DateTimeFormat("ar-SA", { dateStyle: "medium", timeStyle: "short" }).format(new Date(notification.createdAt))}</time></span>
                </button>)}</div> : <div className="notification-empty"><span className="notification-panel-icon"><i className="bi bi-bell-slash" /></span><strong>ما عندك إشعارات الحين</strong><small>إذا وصلك شي جديد بنعلمك هنا.</small></div>}
              </div>}
            </div>
            {user ? (
              <>
                <span className="text-white small me-2">
                  يا هلا، {user.name}
                </span>

                <button
                  className="btn btn-outline-light btn-sm"
                  onClick={logout}
                >
                  تسجيل خروج
                </button>
              </>
            ) : (
              <>
              <Link className="btn btn-outline-light btn-sm" to="/login">
                  دخول
                </Link>

              <Link className="btn btn-light btn-sm" to="/signup">
                  حساب جديد
                </Link>
              </>
            )}

            <button type="button" className="btn btn-warning btn-sm position-relative" onClick={() => { window.clearTimeout(cartCloseTimer.current); cartCloseTimer.current = null; setCartClosing(false); setCartHasDragged(false); setCartOpen(true); }} aria-haspopup="dialog" aria-expanded={cartOpen && !cartClosing} aria-controls="cart-drawer">
              <i className="bi bi-cart3 me-1" aria-hidden="true"></i>السلة
              {cartCount > 0 && <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">{cartCount}</span>}
            </button>
          </div>
        </div>
      </div>
      {cartOpen && <div className={`cart-drawer-backdrop ${cartClosing ? "is-closing" : ""}`}>
        <aside className={`cart-drawer ${cartSide === "right" ? "is-on-right" : ""} ${cartClosing ? "is-closing" : ""} ${cartClosing && cartSide === "right" ? "is-closing-on-right" : ""} ${cartDragging ? "is-dragging" : ""} ${cartHasDragged ? "has-dragged" : ""}`} id="cart-drawer" role="dialog" aria-modal="false" aria-labelledby="cart-drawer-title" dir="rtl" onPointerDown={startCartDrag} onPointerMove={moveCartDrag} onPointerUp={endCartDrag} onPointerCancel={endCartDrag}>
          <header className="cart-drawer-header" title="اسحب لتحريك السلة"><div className="cart-drawer-heading"><span className="cart-drawer-eyebrow">Sooq</span><h2 id="cart-drawer-title">سلة التسوق <small>({cartCount})</small></h2></div><div className="cart-drawer-header-controls"><span className="cart-drawer-move-hint">حرك الناحية الثانية لتشوف</span><button type="button" className="cart-drawer-side-toggle cart-drawer-control" onClick={() => { setCartHasDragged(true); setCartSide((side) => side === "left" ? "right" : "left"); }} aria-label="انقل السلة إلى الجهة الأخرى" title="انقل السلة إلى الجهة الأخرى"><i className="bi bi-arrow-left-right" aria-hidden="true" /></button><button type="button" className="cart-drawer-close cart-drawer-control" onClick={closeCart} aria-label="إغلاق السلة"><i className="bi bi-x-lg" /></button></div></header>
          {cartItems.length ? <>
            <div className="cart-drawer-items">{cartItems.map((item) => <article className="cart-drawer-item" key={item.id}>
              <img src={item.image} alt={item.title} />
              <div className="cart-drawer-item-copy"><Link to={`/product/${item.id}`} onClick={closeCart}>{item.title}</Link><strong><SARPrice value={item.price} /></strong><div className="cart-drawer-quantity"><button type="button" onClick={() => updateQuantity(item.id, item.quantity - 1)} aria-label={`تقليل كمية ${item.title}`}>−</button><span>{item.quantity}</span><button type="button" onClick={() => updateQuantity(item.id, item.quantity + 1)} aria-label={`زيادة كمية ${item.title}`} disabled={item.stockQuantity != null && item.quantity >= Number(item.stockQuantity)}>+</button><button type="button" className="cart-drawer-remove" onClick={() => removeFromCart(item.id)} aria-label={`حذف ${item.title} من السلة`}><i className="bi bi-trash3" /></button></div></div>
            </article>)}</div>
            <footer className="cart-drawer-footer"><div className="cart-drawer-total"><span>الإجمالي</span><strong><SARPrice value={cartTotal} /></strong></div><Link className="cart-drawer-checkout" to="/checkout" onClick={closeCart}>إتمام الطلب<i className="bi bi-arrow-left" aria-hidden="true" /></Link><Link className="cart-drawer-view-cart" to="/cart" onClick={closeCart}>عرض السلة كاملة</Link></footer>
          </> : <div className="cart-drawer-empty"><i className="bi bi-bag" aria-hidden="true" /><strong>سلتك فاضية</strong><span>اكتشف المنتجات وأضف ما يعجبك.</span><Link to="/#products" onClick={closeCart}>تسوّق المنتجات</Link></div>}
        </aside>
      </div>}
    </nav>
  );
}

export default Navbar;
