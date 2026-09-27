import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";
import { formatSAR } from "../formatCurrency";

function CheckoutPage({ cart, user, setCart }) {
  const navigate = useNavigate();
  const [address, setAddress] = useState("");
  const [phone, setPhone] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [paymentOrderId, setPaymentOrderId] = useState(null);
  const total = useMemo(() => cart.reduce((sum, item) => sum + item.price * item.quantity, 0), [cart]);

  if (!cart.length) return <div className="container py-5 text-center"><h1 className="fw-bold">سلتك تنتظرك.</h1><p className="text-muted">اختار لك كم شي حلو وارجع لنا.</p><Link className="btn btn-dark" to="/">تسوّق المنتجات</Link></div>;

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    if (!user || !localStorage.getItem("accessToken")) { navigate("/login", { state: { from: "/checkout" } }); return; }

    const overStockItem = cart.find((item) => item.stockQuantity != null && item.quantity > Number(item.stockQuantity));
    if (overStockItem) {
      setError(`من «${overStockItem.title}» طلبت ${overStockItem.quantity}، والمتوفر ${Number(overStockItem.stockQuantity)} بس. عدّل الكمية من سلتك عشان تكمل طلبك.`);
      return;
    }

    setSubmitting(true);
    let currentOrderId = paymentOrderId;
    try {
      const order = paymentOrderId
        ? { id: paymentOrderId }
        : await api.placeOrder({ shippingAddress: address, shippingPhoneNumber: phone, items: cart.map((item) => ({ productId: Number(item.id), quantity: item.quantity })) });
      currentOrderId = order.id;
      setPaymentOrderId(order.id);
      const payment = await api.createOrderPayment(order.id);
      setCart([]);
      window.location.assign(payment.url);
    } catch (requestError) {
      const stockError = requestError.message.match(/insufficient stock for product\s+(\d+)/i);
      if (stockError) {
        const item = cart.find((cartItem) => Number(cartItem.id) === Number(stockError[1]));
        setError(item
          ? `الكمية المطلوبة من «${item.title}» ما عادت متوفرة بالكامل. قلّل الكمية من سلتك وحاول مرة ثانية.`
          : "الكمية المطلوبة من أحد المنتجات ما عادت متوفرة بالكامل. راجع كميات سلتك وحاول مرة ثانية.");
      } else {
        setError(currentOrderId
          ? `طلبك رقم #${currentOrderId} محفوظ، لكن تعذر فتح بوابة الدفع: ${getApiErrorMessage(requestError, "خطأ غير معروف من بوابة الدفع")}`
          : getApiErrorMessage(requestError, "تعذّر تأكيد الطلب الحين. حاول مرة ثانية بعد ما تتأكد من بيانات التوصيل."));
      }
    }
    finally { setSubmitting(false); }
  };

  return <section className="container py-5 checkout-page">
    <Link className="text-muted d-inline-block mb-4" to="/cart"><i className="bi bi-arrow-right me-2"/>رجوع للسلة</Link>
    <div className="mb-4"><p className="eyebrow">باقي خطوة</p><h1 className="display-5 fw-bold">إتمام الطلب</h1><p className="text-muted">عطنا تفاصيل التوصيل ونجهز طلبك.</p></div>
    <div className="row g-4">
      <div className="col-lg-7"><form className="card checkout-card border-0 p-4 p-lg-5" onSubmit={submit}>
        <h2 className="h4 fw-bold mb-4">بيانات التوصيل</h2>
        {!user && <div className="alert alert-info">سجّل دخولك قبل تأكيد الطلب. <Link to="/login">دخول</Link></div>}
        {error && <div className="alert alert-danger">{error}</div>}
        <label className="form-label" htmlFor="address">عنوان التوصيل</label><textarea id="address" className="form-control mb-4" rows="3" required value={address} onChange={(e) => setAddress(e.target.value)} placeholder="الشارع، الحي، المدينة"/>
        <label className="form-label" htmlFor="phone">رقم الجوال</label><input id="phone" className="form-control mb-4" type="tel" autoComplete="tel" required value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="05xxxxxxxx"/>
        <button className="btn btn-dark btn-lg" disabled={submitting}>{submitting ? "جاري تحويلك للدفع…" : paymentOrderId ? "إعادة محاولة الدفع" : `ادفع وأكد الطلب · ${formatSAR(total)}`}</button>
      </form></div>
      <div className="col-lg-5"><aside className="checkout-card p-4 p-lg-5"><h2 className="h4 fw-bold mb-4">سلتك <span className="text-muted fw-normal">({cart.reduce((sum, item) => sum + item.quantity, 0)})</span></h2>
        {cart.map((item) => <div className="d-flex justify-content-between gap-3 mb-3" key={item.id}><span>{item.title} <small className="text-muted">× {item.quantity}</small></span><strong>{formatSAR(item.price * item.quantity)}</strong></div>)}
        <hr/><div className="d-flex justify-content-between mb-2"><span>التوصيل</span><span className="text-success">علينا</span></div><div className="d-flex justify-content-between fs-5 fw-bold mt-3"><span>الإجمالي</span><span>{formatSAR(total)}</span></div>
      </aside></div>
    </div>
  </section>;
}

export default CheckoutPage;
