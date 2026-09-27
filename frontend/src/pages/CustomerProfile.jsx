import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api, getApiErrorMessage, normalizeProduct } from "../api";
import { formatSAR } from "../formatCurrency";
import ProductCard from "../components/ProductCard/ProductCard";

const orderStatuses = ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"];
const statusLabels = {
  Pending: "بانتظار التأكيد",
  Processing: "قيد التجهيز",
  Shipped: "طلع للتوصيل",
  Delivered: "تم التوصيل",
  Cancelled: "ملغي",
};

function getStatus(status) {
  return typeof status === "number" ? orderStatuses[status] ?? "Pending" : status;
}

function formatDate(value) {
  return new Intl.DateTimeFormat("ar-SA-u-ca-gregory", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
}

function CustomerProfile({ user, logout }) {
  const [activeSection, setActiveSection] = useState("");
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busyOrder, setBusyOrder] = useState(null);
  const [searchParams] = useSearchParams();
  const [highlightedOrderId, setHighlightedOrderId] = useState(null);
  const [likedProducts, setLikedProducts] = useState([]);
  const [likesLoading, setLikesLoading] = useState(true);
  const [likesError, setLikesError] = useState("");
  const requestedOrderId = searchParams.get("orderId");

  const loadOrders = useCallback(async () => {
    setError("");
    try {
      setOrders(await api.myOrders());
    } catch (reason) {
      setError(getApiErrorMessage(reason, "ما قدرنا نحمّل طلباتك الحين. جرّب مرة ثانية."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadOrders(); }, [loadOrders]);

  const loadLikes = useCallback(async () => {
    setLikesError("");
    setLikesLoading(true);
    try {
      const ids = await api.myLikes();
      const products = await api.products();
      const likedIds = new Set((ids || []).map(Number));
      setLikedProducts(products.filter((product) => likedIds.has(Number(product.id ?? product.productId))).map(normalizeProduct));
    } catch (reason) {
      setLikesError(getApiErrorMessage(reason, "ما قدرنا نحمّل المنتجات التي أعجبتك."));
    } finally {
      setLikesLoading(false);
    }
  }, []);

  useEffect(() => { loadLikes(); }, [loadLikes]);

  const removeLikedProduct = async (product) => {
    setLikedProducts((current) => current.filter((item) => Number(item.id) !== Number(product.id)));
    try {
      await api.removeLike(product.id);
    } catch (reason) {
      setLikesError(getApiErrorMessage(reason, "ما قدرنا نحذف المنتج من المفضلة."));
      await loadLikes();
    }
  };

  useEffect(() => {
    if (loading || !requestedOrderId || !orders.some((order) => Number(order.id) === Number(requestedOrderId))) return;
    const target = document.getElementById(`profile-order-${requestedOrderId}`);
    if (!target) return;
    setHighlightedOrderId(Number(requestedOrderId));
    target.scrollIntoView({ behavior: "smooth", block: "center" });
    target.focus({ preventScroll: true });
  }, [loading, orders, requestedOrderId]);

  const orderSummary = useMemo(() => ({
    total: orders.length,
    active: orders.filter((order) => ["Pending", "Processing", "Shipped"].includes(getStatus(order.status))).length,
    delivered: orders.filter((order) => getStatus(order.status) === "Delivered").length,
  }), [orders]);

  const cancelOrder = async (order) => {
    if (!window.confirm(`متأكد تبي تلغي الطلب رقم ${order.id}؟`)) return;
    setBusyOrder(order.id);
    setError("");
    try {
      await api.updateOrderStatus(order.id, "Cancelled");
      await loadOrders();
    } catch (reason) {
      setError(getApiErrorMessage(reason, "ما قدرنا نلغي الطلب. يمكن حالته تغيّرت، حدّث الصفحة وحاول مرة ثانية."));
    } finally {
      setBusyOrder(null);
    }
  };

  return (
    <main className="customer-profile container">
      <header className="profile-heading">
        <div>
          <p className="eyebrow mb-2">مساحتك في سوق</p>
          <h1>حسابي</h1>
          <p>تابع طلباتك وتفاصيل حسابك من مكان واحد.</p>
        </div>
        <button type="button" className="btn btn-outline-secondary profile-logout" onClick={logout}><i className="bi bi-box-arrow-left" aria-hidden="true" /> تسجيل خروج</button>
      </header>

      <section className="profile-account-card" aria-label="معلومات الحساب">
        <span className="profile-avatar"><i className="bi bi-person-fill" aria-hidden="true" /></span>
        <div className="profile-account-info"><span>بيانات الحساب</span><strong>{user.name || user.userName || "عميل سوق"}</strong><small>{user.email || "حسابك في سوق"}</small></div>
        <i className="bi bi-shield-check profile-account-check" title="حساب مسجل" aria-hidden="true" />
      </section>

      <section className="profile-hub-grid" aria-label="أقسام حسابك">
        <button type="button" className={`profile-hub-card orders ${activeSection === "orders" ? "is-active" : ""}`} aria-expanded={activeSection === "orders"} onClick={() => setActiveSection((current) => current === "orders" ? "" : "orders")}>
          <span className="profile-hub-icon"><i className="bi bi-receipt-cutoff" aria-hidden="true" /></span>
          <span className="profile-hub-copy"><strong>طلباتي</strong><small>تابع حالة مشترياتك وتفاصيلها</small></span>
          <span className="profile-hub-count">{loading ? "—" : orderSummary.total}</span>
          <i className={`bi ${activeSection === "orders" ? "bi-chevron-up" : "bi-chevron-down"} profile-hub-chevron`} aria-hidden="true" />
        </button>
        <button type="button" className={`profile-hub-card likes ${activeSection === "likes" ? "is-active" : ""}`} aria-expanded={activeSection === "likes"} onClick={() => setActiveSection((current) => current === "likes" ? "" : "likes")}>
          <span className="profile-hub-icon"><i className="bi bi-heart" aria-hidden="true" /></span>
          <span className="profile-hub-copy"><strong>منتجات أعجبتني</strong><small>كل المنتجات التي حفظتها للمفضلة</small></span>
          <span className="profile-hub-count">{likesLoading ? "—" : likedProducts.length}</span>
          <i className={`bi ${activeSection === "likes" ? "bi-chevron-up" : "bi-chevron-down"} profile-hub-chevron`} aria-hidden="true" />
        </button>
      </section>

      {activeSection === "orders" && <section className="profile-orders profile-content-panel" aria-labelledby="profile-orders-title">
        <div className="profile-orders-heading"><div><p className="eyebrow mb-1">سجلّك</p><h2 id="profile-orders-title">طلباتي</h2></div><Link to="/#products" className="profile-shop-link">تابع التسوق <i className="bi bi-arrow-left" aria-hidden="true" /></Link></div>

        {error && <div className="alert alert-danger" role="alert">{error}</div>}
        {loading ? <div className="profile-orders-state" role="status"><span className="spinner-border spinner-border-sm" /> جاري تحميل طلباتك…</div>
          : orders.length === 0 && !error ? <div className="profile-orders-empty"><span><i className="bi bi-bag" /></span><h3>ما عندك طلبات للحين</h3><p>أول ما تطلب، بتلقى تفاصيل طلبك وحالته هنا.</p><Link to="/#products" className="btn btn-dark">تسوّق الحين</Link></div>
            : <div className="profile-order-list">{orders.map((order) => {
              const status = getStatus(order.status);
              const itemCount = order.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0;
              return <article id={`profile-order-${order.id}`} tabIndex={-1} className={`profile-order-card ${highlightedOrderId === Number(order.id) ? "is-highlighted" : ""}`} key={order.id}>
                <div className="profile-order-topline"><div><span className="profile-order-number">طلب رقم <strong>#{order.id}</strong></span><time>{formatDate(order.createdAt)}</time></div><span className={`profile-order-status status-${status.toLowerCase()}`}>{statusLabels[status] || status}</span></div>
                <div className="profile-order-meta"><span><i className="bi bi-bag" aria-hidden="true" /> {itemCount} قطعة</span><strong>{formatSAR(order.totalAmount)}</strong></div>
                <details className="profile-order-details"><summary>تفاصيل الطلب <i className="bi bi-chevron-down" aria-hidden="true" /></summary>
                  <div className="profile-order-lines">{order.items?.map((item) => <div key={item.productId}><span>منتج #{item.productId} <small>× {item.quantity}</small></span><strong>{formatSAR(item.lineTotal)}</strong></div>)}</div>
                  <div className="profile-delivery-address"><small>عنوان التوصيل</small><span>{order.shippingAddress}</span></div>
                  {order.shippingPhoneNumber && <div className="profile-delivery-address"><small>رقم الجوال</small><span>{order.shippingPhoneNumber}</span></div>}
                </details>
                {status === "Pending" && <div className="profile-order-actions"><button className="btn btn-sm btn-outline-danger" type="button" disabled={busyOrder !== null} onClick={() => cancelOrder(order)}>{busyOrder === order.id ? "جاري الإلغاء…" : "إلغاء الطلب"}</button></div>}
              </article>;
            })}</div>}
      </section>}

      {activeSection === "likes" && <section className="profile-likes profile-content-panel" aria-labelledby="profile-likes-title">
        <div className="profile-orders-heading"><div><p className="eyebrow mb-1">قائمتك الخاصة</p><h2 id="profile-likes-title">منتجات أعجبتني</h2></div><Link to="/#products" className="profile-shop-link">اكتشف منتجات أكثر <i className="bi bi-arrow-left" aria-hidden="true" /></Link></div>
        {likesError && <div className="alert alert-danger" role="alert">{likesError}</div>}
        {likesLoading ? <div className="profile-orders-state" role="status"><span className="spinner-border spinner-border-sm" /> جاري تحميل المفضلة…</div>
          : likedProducts.length === 0 ? <div className="profile-orders-empty"><span><i className="bi bi-heart" /></span><h3>قائمتك فارغة حالياً</h3><p>اضغط على رمز القلب في أي منتج لحفظه هنا.</p><Link to="/#products" className="btn btn-dark">اكتشف المنتجات</Link></div>
            : <div className="row g-3 g-xl-4">{likedProducts.map((product) => <div className="col-6 col-md-4 col-xl-3" key={product.id}><ProductCard product={product} addToCart={() => {}} liked onToggleLike={removeLikedProduct} /></div>)}</div>}
      </section>}
    </main>
  );
}

export default CustomerProfile;
