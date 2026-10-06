import { useCallback, useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

function PaymentResultPage() {
  const [params] = useSearchParams();
  const orderId = params.get("orderId");
  const [state, setState] = useState("checking");
  const [verification, setVerification] = useState(null);
  const [errorMessage, setErrorMessage] = useState("حاول التحقق مرة ثانية بعد قليل.");
  const verify = useCallback(async () => {
    if (!orderId) { setState("invalid"); return; }
    setState("checking");
    try {
      const result = await api.verifyOrderPayment(orderId);
      setVerification(result);
      setState(result.outcome || "unknown");
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error, "حاول التحقق مرة ثانية بعد قليل."));
      setState("error");
    }
  }, [orderId]);
  useEffect(() => { verify(); }, [verify]);

  const messages = {
    checking: ["نتحقق من الدفع", "لحظات ونحدّث حالة طلبك."],
    paid: ["تم الدفع بنجاح", `تم تأكيد دفعة الطلب #${orderId}.`],
    pending: ["الدفع لم يكتمل بعد", `أفاد فواتيرك أن الدفعة غير مدفوعة حتى الآن للطلب #${orderId}.`],
    amount_mismatch: ["تم استلام مبلغ مختلف", `أفاد فواتيرك أن الدفعة مدفوعة، لكن المبلغ لا يطابق إجمالي الطلب #${orderId}.`],
    currency_mismatch: ["عملة الدفعة غير مطابقة", `أفاد فواتيرك أن الدفعة مدفوعة، لكن العملة لا تطابق عملة الطلب #${orderId}.`],
    unknown: ["تعذر تحديد حالة الدفع", `لا تتوفر لدى فواتيرك بيانات كافية للتحقق من الطلب #${orderId}.`],
    error: ["تعذر التحقق من الدفع", errorMessage],
    invalid: ["رابط الدفع غير مكتمل", "تعذر العثور على رقم الطلب."],
  };
  const [title, description] = messages[state];
  return <section className="container py-5"><div className="thank-you-card text-center mx-auto py-5 px-4">
    <p className="eyebrow mt-4">الدفع الإلكتروني</p><h1 className="display-5 fw-bold">{title}</h1>
    <p className="text-muted fs-5">{description}</p>
    {verification && <div className="small text-muted mt-3" dir="rtl">
      <p className="mb-1">حالة الدفع لدى فواتيرك: {verification.providerPaid === 1 ? "مدفوعة" : verification.providerPaid === 0 ? "غير مدفوعة" : "غير متاحة"}</p>
      {verification.providerTotal != null && <p className="mb-1">المبلغ المبلغ عنه: {verification.providerTotal} {verification.providerCurrency || ""}</p>}
      {verification.reason && <p className="mb-0">التفصيل: {({ provider_reports_unpaid: "مزود الدفع لم يؤكد السداد", amount_mismatch: "المبلغ المبلغ عنه لا يطابق إجمالي الطلب", currency_mismatch: "العملة المبلغ عنها لا تطابق عملة الطلب", missing_provider_intent: "لا يوجد رقم عملية لدى مزود الدفع", already_recorded: "الدفعة مسجلة مسبقاً" })[verification.reason] || verification.reason}</p>}
    </div>}
    {["pending", "error", "unknown", "amount_mismatch", "currency_mismatch"].includes(state) && <button className="btn btn-dark mt-3 me-2" onClick={verify}>تحقق مرة أخرى</button>}
    <Link to="/" className="btn btn-outline-dark mt-3">العودة للتسوق</Link>
  </div></section>;
}

export default PaymentResultPage;
