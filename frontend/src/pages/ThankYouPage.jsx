import { Link, useLocation } from "react-router-dom";

function ThankYouPage() {
  const { state } = useLocation();
  const orderId = state?.order?.id;
  return <section className="container py-5"><div className="thank-you-card text-center mx-auto py-5 px-4">
    <span className="success-mark"><i className="bi bi-check2"/></span>
    <p className="eyebrow mt-4">ORDER CONFIRMED</p><h1 className="display-5 fw-bold">Thank you.</h1>
    <p className="text-muted fs-5">Your order is in. We’ll take it from here.</p>
    {orderId && <p className="small text-muted">Order number <strong>#{orderId}</strong></p>}
    <Link to="/" className="btn btn-dark mt-3">Back to the collection <i className="bi bi-arrow-right ms-2"/></Link>
  </div></section>;
}

export default ThankYouPage;
