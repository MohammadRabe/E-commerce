import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

const statusLabels = {
  Pending: "بانتظار التأكيد",
  Processing: "قيد التجهيز",
  Shipped: "طلع للتوصيل",
  Delivered: "تم التوصيل",
  Cancelled: "ملغي",
};

const statusClasses = {
  Pending: "pending",
  Processing: "processing",
  Shipped: "shipped",
  Delivered: "delivered",
  Cancelled: "cancelled",
};

const number = (value) => new Intl.NumberFormat("ar-SA").format(Number(value || 0));

function AdminSalesStatistics() {
  const [statistics, setStatistics] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    api.salesStatistics()
      .then((data) => { if (active) setStatistics(data); })
      .catch((reason) => { if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل إحصائيات المبيعات.")); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  const statusBreakdown = statistics?.statusBreakdown ?? [];
  const successPercentage = Number(statistics?.succeededProcessPercentage ?? 0);
  const activePercentage = Number(statistics?.activeCustomerPercentage ?? 0);

  return (
    <section className="container admin-statistics-page">
      <header className="admin-statistics-heading">
        <div>
          <Link to="/dashboard" className="admin-statistics-back"><i className="bi bi-arrow-right" aria-hidden="true" /> لوحة التحكم</Link>
          <p className="eyebrow">إدارة Sooq</p>
          <h1>إحصائيات المبيعات</h1>
          <p>صورة واضحة عن العملاء والطلبات ونسب الإنجاز.</p>
        </div>
        <span className="admin-statistics-heading-icon"><i className="bi bi-bar-chart-line" aria-hidden="true" /></span>
      </header>

      {error && <div className="alert alert-danger" role="alert">{error}</div>}
      {loading ? <div className="admin-statistics-loading" role="status"><span className="spinner-border spinner-border-sm" /> جاري تحميل الإحصائيات…</div>
        : statistics && <>
          <div className="admin-statistics-metrics">
            <Link to="/dashboard/customers" className="admin-stat-card admin-stat-card-link">
              <span className="admin-stat-icon customers"><i className="bi bi-people" aria-hidden="true" /></span>
              <span className="admin-stat-label">عدد العملاء</span>
              <strong>{number(statistics.customerCount)}</strong>
              <small>الحسابات المسجّلة كعملاء</small>
            </Link>
            <Link to="/dashboard/orders" className="admin-stat-card admin-stat-card-link">
              <span className="admin-stat-icon purchases"><i className="bi bi-bag-check" aria-hidden="true" /></span>
              <span className="admin-stat-label">عمليات الشراء</span>
              <strong>{number(statistics.purchasingProcessCount)}</strong>
              <small>كل الطلبات المسجّلة</small>
            </Link>
            <Link to="/dashboard/orders?status=Delivered" className="admin-stat-card admin-stat-card-link">
              <span className="admin-stat-icon succeeded"><i className="bi bi-check2-circle" aria-hidden="true" /></span>
              <span className="admin-stat-label">عمليات ناجحة</span>
              <strong>{number(statistics.succeededProcessCount)}</strong>
              <small>الطلبات التي تم توصيلها</small>
            </Link>
            <article className="admin-stat-card">
              <span className="admin-stat-icon active"><i className="bi bi-person-check" aria-hidden="true" /></span>
              <span className="admin-stat-label">عملاء نشطون</span>
              <strong>{number(statistics.activeCustomerCount)}</strong>
              <small>اشتروا خلال آخر {number(statistics.activeCustomerWindowDays)} يوم</small>
            </article>
          </div>

          <div className="admin-statistics-charts">
            <article className="admin-chart-card admin-success-chart">
              <div className="admin-chart-heading"><div><h2>نسبة نجاح الطلبات</h2><p>الطلبات التي وصلت للعملاء من إجمالي الطلبات</p></div><i className="bi bi-pie-chart" aria-hidden="true" /></div>
              <div className="admin-success-chart-content">
                <div className="admin-donut" style={{ "--chart-percent": `${successPercentage}%` }} role="img" aria-label={`نسبة الطلبات الناجحة ${number(successPercentage)} بالمئة`}>
                  <div><strong>{number(successPercentage)}٪</strong><span>نسبة النجاح</span></div>
                </div>
                <div className="admin-chart-legend">
                  <div><span className="legend-dot delivered" /><span>تم التوصيل</span><strong>{number(statistics.succeededProcessCount)} طلب</strong></div>
                  <div><span className="legend-dot remaining" /><span>حالات أخرى</span><strong>{number(Math.max(0, statistics.purchasingProcessCount - statistics.succeededProcessCount))} طلب</strong></div>
                </div>
              </div>
            </article>

            <article className="admin-chart-card">
              <div className="admin-chart-heading"><div><h2>نشاط العملاء</h2><p>نسبة العملاء اللي اشتروا خلال آخر 30 يوم</p></div><i className="bi bi-activity" aria-hidden="true" /></div>
              <div className="admin-active-rate"><strong>{number(activePercentage)}٪</strong><span>من إجمالي العملاء</span></div>
              <div className="admin-horizontal-track"><span className="active-customer-fill" style={{ width: `${Math.min(100, activePercentage)}%` }} /></div>
              <div className="admin-active-foot"><span>{number(statistics.activeCustomerCount)} عميل نشط</span><span>{number(statistics.customerCount)} عميل</span></div>
            </article>

            <article className="admin-chart-card admin-status-chart">
              <div className="admin-chart-heading"><div><h2>توزيع حالات الطلبات</h2><p>نسبة كل حالة من إجمالي عمليات الشراء</p></div><i className="bi bi-bar-chart" aria-hidden="true" /></div>
              <div className="admin-status-list">
                {statusBreakdown.map((item) => <div className="admin-status-row" key={item.status}>
                  <div className="admin-status-label"><span>{statusLabels[item.status] || item.status}</span><strong>{number(item.percentage)}٪</strong></div>
                  <div className="admin-horizontal-track"><span className={`admin-status-fill ${statusClasses[item.status] || ""}`} style={{ width: `${Math.min(100, Number(item.percentage || 0))}%` }} /></div>
                  <small>{number(item.count)} طلب</small>
                </div>)}
              </div>
            </article>
          </div>
        </>}
    </section>
  );
}

export default AdminSalesStatistics;
