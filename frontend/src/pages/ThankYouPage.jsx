import { Link, useLocation } from "react-router-dom";

function ThankYouPage() {
  const { state } = useLocation();
  const orderId = state?.order?.id;
  return <section className="container py-5"><div className="thank-you-card text-center mx-auto py-5 px-4">
    <span className="success-mark"><i className="bi bi-check2"/></span>
    <p className="eyebrow mt-4">تم تأكيد الطلب</p><h1 className="display-5 fw-bold">يعطيك العافية!</h1>
    <p className="text-muted fs-5">طلبك تأكد، وبنجهزه لك بأسرع وقت.</p>
    {orderId && <p className="small text-muted">رقم الطلب <strong>#{orderId}</strong></p>}
    <Link to="/" className="btn btn-dark mt-3">رجوع للتسوّق <i className="bi bi-arrow-left ms-2"/></Link>
  </div></section>;
}

export default ThankYouPage;
