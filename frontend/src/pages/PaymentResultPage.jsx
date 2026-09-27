import { useCallback, useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api } from "../api";

function PaymentResultPage() {
  const [params] = useSearchParams();
  const orderId = params.get("orderId");
  const [state, setState] = useState("checking");
  const verify = useCallback(async () => {
    if (!orderId) { setState("invalid"); return; }
    setState("checking");
    try {
      const result = await api.verifyOrderPayment(orderId);
      setState(result.isPaid ? "paid" : "pending");
    } catch { setState("error"); }
  }, [orderId]);
  useEffect(() => { verify(); }, [verify]);

  const messages = {
    checking: ["نتحقق من الدفع", "لحظات ونحدّث حالة طلبك."],
    paid: ["تم الدفع بنجاح", `تم تأكيد دفعة الطلب #${orderId}.`],
    pending: ["الدفع قيد المعالجة", `لم يؤكد مزود الدفع إتمام الطلب #${orderId} بعد.`],
    error: ["تعذر التحقق من الدفع", "حاول التحقق مرة ثانية بعد قليل."],
    invalid: ["رابط الدفع غير مكتمل", "تعذر العثور على رقم الطلب."],
  };
  const [title, description] = messages[state];
  return <section className="container py-5"><div className="thank-you-card text-center mx-auto py-5 px-4">
    <p className="eyebrow mt-4">الدفع الإلكتروني</p><h1 className="display-5 fw-bold">{title}</h1>
    <p className="text-muted fs-5">{description}</p>
    {(state === "pending" || state === "error") && <button className="btn btn-dark mt-3 me-2" onClick={verify}>تحقق مرة أخرى</button>}
    <Link to="/" className="btn btn-outline-dark mt-3">العودة للتسوق</Link>
  </div></section>;
}

export default PaymentResultPage;
