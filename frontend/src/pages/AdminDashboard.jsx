import { Link } from "react-router-dom";

function AdminDashboard() {
  return (
    <section className="container py-5">
      <div className="mb-4">
        <p className="text-uppercase text-muted small fw-semibold mb-1">إدارة Sooq</p>
        <h1 className="h2 fw-bold mb-0">لوحة التحكم</h1>
      </div>

      <div className="row g-3">
        <div className="col-12 col-sm-6 col-lg-4 col-xl-3">
          <Link to="/dashboard/orders" className="dashboard-control card border-0 shadow-sm h-100 text-decoration-none">
            <span className="dashboard-control-icon"><i className="bi bi-receipt-cutoff" aria-hidden="true" /></span>
            <span className="h5 fw-bold mb-2">الطلبات</span>
            <span className="text-muted small">راجع طلبات العملاء وحدّث حالتها</span>
            <span className="dashboard-control-arrow"><i className="bi bi-arrow-up-right" aria-hidden="true" /></span>
          </Link>
        </div>
        <div className="col-12 col-sm-6 col-lg-4 col-xl-3">
          <Link to="/dashboard/statistics" className="dashboard-control card border-0 shadow-sm h-100 text-decoration-none">
            <span className="dashboard-control-icon"><i className="bi bi-bar-chart-line" aria-hidden="true" /></span>
            <span className="h5 fw-bold mb-2">إحصائيات المبيعات</span>
            <span className="text-muted small">نظرة على العملاء والطلبات ونسب الإنجاز</span>
            <span className="dashboard-control-arrow"><i className="bi bi-arrow-up-right" aria-hidden="true" /></span>
          </Link>
        </div>
        <div className="col-12 col-sm-6 col-lg-4 col-xl-3">
          <Link to="/dashboard/products/new" className="dashboard-control card border-0 shadow-sm h-100 text-decoration-none">
            <span className="dashboard-control-icon"><i className="bi bi-plus-square" aria-hidden="true" /></span>
            <span className="h5 fw-bold mb-2">إضافة منتج</span>
            <span className="text-muted small">أضف منتجاً جديداً إلى المتجر</span>
            <span className="dashboard-control-arrow"><i className="bi bi-arrow-up-right" aria-hidden="true" /></span>
          </Link>
        </div>
        <div className="col-12 col-sm-6 col-lg-4 col-xl-3">
          <Link to="/dashboard/categories/new" className="dashboard-control card border-0 shadow-sm h-100 text-decoration-none">
            <span className="dashboard-control-icon"><i className="bi bi-folder-plus" aria-hidden="true" /></span>
            <span className="h5 fw-bold mb-2">إضافة فئة</span>
            <span className="text-muted small">أنشئ قسماً جديداً للمنتجات</span>
            <span className="dashboard-control-arrow"><i className="bi bi-arrow-up-right" aria-hidden="true" /></span>
          </Link>
        </div>
        <div className="col-12 col-sm-6 col-lg-4 col-xl-3">
          <Link to="/dashboard/customers" className="dashboard-control card border-0 shadow-sm h-100 text-decoration-none">
            <span className="dashboard-control-icon"><i className="bi bi-people" aria-hidden="true" /></span>
            <span className="h5 fw-bold mb-2">العملاء</span>
            <span className="text-muted small">اعرض حسابات العملاء وابحث بينها</span>
            <span className="dashboard-control-arrow"><i className="bi bi-arrow-up-right" aria-hidden="true" /></span>
          </Link>
        </div>
      </div>
    </section>
  );
}

export default AdminDashboard;
