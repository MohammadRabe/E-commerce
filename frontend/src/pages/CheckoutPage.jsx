import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";

function CheckoutPage({ cart, user, setCart }) {
  const navigate = useNavigate();
  const [address, setAddress] = useState("");
  const [phone, setPhone] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const total = useMemo(() => cart.reduce((sum, item) => sum + item.price * item.quantity, 0), [cart]);

  if (!cart.length) return <div className="container py-5 text-center"><h1 className="fw-bold">Your bag is waiting.</h1><p className="text-muted">Add a few things you love, then come back here.</p><Link className="btn btn-dark" to="/">Explore the collection</Link></div>;

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    if (!user || !localStorage.getItem("accessToken")) { navigate("/login", { state: { from: "/checkout" } }); return; }
    setSubmitting(true);
    try {
      const order = await api.placeOrder({ shippingAddress: address, shippingPhoneNumber: phone, items: cart.map((item) => ({ productId: Number(item.id), quantity: item.quantity })) });
      setCart([]);
      navigate("/thank-you", { state: { order } });
    } catch (reason) { setError(reason.message || "We couldn't place your order."); }
    finally { setSubmitting(false); }
  };

  return <section className="container py-5 checkout-page">
    <Link className="text-muted d-inline-block mb-4" to="/cart"><i className="bi bi-arrow-left me-2"/>Back to bag</Link>
    <div className="mb-4"><p className="eyebrow">ALMOST YOURS</p><h1 className="display-5 fw-bold">Checkout</h1><p className="text-muted">A few details and we’ll get your order on its way.</p></div>
    <div className="row g-4">
      <div className="col-lg-7"><form className="card checkout-card border-0 p-4 p-lg-5" onSubmit={submit}>
        <h2 className="h4 fw-bold mb-4">Delivery details</h2>
        {!user && <div className="alert alert-info">Please sign in before placing your order. <Link to="/login">Sign in</Link></div>}
        {error && <div className="alert alert-danger">{error}</div>}
        <label className="form-label" htmlFor="address">Shipping address</label><textarea id="address" className="form-control mb-4" rows="3" required value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Street, city, postal code"/>
        <label className="form-label" htmlFor="phone">Phone number <span className="text-muted">(optional)</span></label><input id="phone" className="form-control mb-4" type="tel" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="+20 …"/>
        <button className="btn btn-dark btn-lg" disabled={submitting}>{submitting ? "Placing your order…" : "Place order · $" + total.toFixed(2)}</button>
      </form></div>
      <div className="col-lg-5"><aside className="checkout-card p-4 p-lg-5"><h2 className="h4 fw-bold mb-4">Your bag <span className="text-muted fw-normal">({cart.reduce((sum, item) => sum + item.quantity, 0)})</span></h2>
        {cart.map((item) => <div className="d-flex justify-content-between gap-3 mb-3" key={item.id}><span>{item.title} <small className="text-muted">× {item.quantity}</small></span><strong>${(item.price * item.quantity).toFixed(2)}</strong></div>)}
        <hr/><div className="d-flex justify-content-between mb-2"><span>Shipping</span><span className="text-success">Complimentary</span></div><div className="d-flex justify-content-between fs-5 fw-bold mt-3"><span>Total</span><span>${total.toFixed(2)}</span></div>
      </aside></div>
    </div>
  </section>;
}

export default CheckoutPage;
