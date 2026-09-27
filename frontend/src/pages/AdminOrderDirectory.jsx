import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";
import { formatSAR } from "../formatCurrency";

const PAGE_SIZE = 10;
const labels = { Pending: "بانتظار التأكيد", Processing: "قيد التجهيز", Shipped: "طلع للتوصيل", Delivered: "تم التوصيل", Cancelled: "ملغي" };
const formatNumber = (value) => new Intl.NumberFormat("ar-SA").format(Number(value || 0));

function formatDate(value) {
  return new Intl.DateTimeFormat("ar-SA-u-ca-gregory", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
}

function AdminOrderDirectory() {
  const [searchParams, setSearchParams] = useSearchParams();
  const pageNumber = Math.max(1, Number(searchParams.get("page")) || 1);
  const status = searchParams.get("status") || "";
  const search = searchParams.get("search") || "";
  const [searchInput, setSearchInput] = useState(search);
  const [result, setResult] = useState({ items: [], totalCount: 0, next: false, prev: false });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => setSearchInput(search), [search]);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    api.adminOrders({ pageNumber, pageSize: PAGE_SIZE, search, status })
      .then((data) => { if (active) setResult(data); })
      .catch((reason) => { if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل قائمة الطلبات.")); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [pageNumber, search, status]);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      const nextSearch = searchInput.trim();
      if (nextSearch === search) return;
      const next = {};
      if (status) next.status = status;
      if (nextSearch) next.search = nextSearch;
      setSearchParams(next, { replace: true });
    }, 300);
    return () => window.clearTimeout(timeout);
  }, [searchInput, search, status, setSearchParams]);

  const pageCount = Math.max(1, Math.ceil(result.totalCount / PAGE_SIZE));
  const pages = useMemo(() => Array.from({ length: pageCount }, (_, index) => index + 1), [pageCount]);
  const start = result.totalCount ? (pageNumber - 1) * PAGE_SIZE + 1 : 0;
  const end = Math.min(pageNumber * PAGE_SIZE, result.totalCount);
  const title = status === "Delivered" ? "الطلبات الناجحة" : "كل الطلبات";

  const updateParams = (page, nextSearch = search, nextStatus = status, replace = false) => {
    const next = {};
    if (nextStatus) next.status = nextStatus;
    if (nextSearch) next.search = nextSearch;
    if (page > 1) next.page = String(page);
    setSearchParams(next, { replace });
  };

  const submitSearch = (event) => {
    event.preventDefault();
    updateParams(1, searchInput.trim(), status, true);
  };

  return (
    <section className="container admin-customers-page admin-order-directory-page">
      <nav className="admin-customers-breadcrumb" aria-label="مسار التنقل">
        <Link to="/dashboard"><i className="bi bi-grid-1x2" aria-hidden="true" /> لوحة التحكم</Link>
        <i className="bi bi-chevron-left separator" aria-hidden="true" />
        <Link to="/dashboard/statistics">إحصائيات المبيعات</Link>
        <i className="bi bi-chevron-left separator" aria-hidden="true" />
        <span aria-current="page">{title}</span>
      </nav>

      <header className="admin-customers-hero admin-orders-hero">
        <div className="admin-customers-hero-copy">
          <span className="admin-customers-eyebrow"><i className="bi bi-receipt" aria-hidden="true" /> إدارة الطلبات</span>
          <h1>{title}</h1>
          <p>{status === "Delivered" ? "كل الطلبات التي وصلت إلى أصحابها، مع إمكانية البحث والتصفّح." : "تابع جميع طلبات متجرك وابحث عنها بسهولة."}</p>
        </div>
        <div className="admin-customers-total"><span>{status ? labels[status] : "إجمالي الطلبات"}</span><strong>{formatNumber(result.totalCount)}</strong><i className={`bi ${status === "Delivered" ? "bi-check2-circle" : "bi-bag-check"}`} aria-hidden="true" /></div>
      </header>

      <div className="admin-customers-toolbar">
        <div><h2>{status ? `طلبات ${labels[status]}` : "سجل الطلبات"}</h2><p>{loading ? "جاري تحديث النتائج…" : `عرض ${formatNumber(start)}–${formatNumber(end)} من ${formatNumber(result.totalCount)} طلب`}</p></div>
        <form className="admin-customers-search" role="search" onSubmit={submitSearch}>
          <i className="bi bi-search" aria-hidden="true" />
          <input type="search" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="ابحث برقم الطلب أو العميل أو الجوال" aria-label="ابحث عن طلب" />
          {searchInput && <button type="button" className="admin-customers-search-clear" aria-label="مسح البحث" onClick={() => setSearchInput("")}><i className="bi bi-x-lg" /></button>}
          <button type="submit" className="admin-customers-search-submit">بحث</button>
        </form>
      </div>

      <div className="admin-customers-card">
        {error ? <div className="alert alert-danger m-4" role="alert">{error}</div>
          : loading ? <div className="admin-customers-loading" role="status"><span className="spinner-border spinner-border-sm" /> جاري تحميل الطلبات…</div>
            : result.items.length === 0 ? <div className="admin-customers-empty"><span><i className="bi bi-bag-x" aria-hidden="true" /></span><strong>{search ? "ما لقينا طلب بهذا البحث" : "ما فيه طلبات للعرض حالياً"}</strong><p>{search ? "جرّب رقم طلب أو اسم عميل أو رقم جوال مختلف." : "ستظهر الطلبات المسجّلة هنا."}</p></div>
              : <div className="table-responsive"><table className="table admin-customers-table admin-orders-table align-middle mb-0">
                <thead><tr><th>الطلب</th><th>العميل</th><th>التاريخ</th><th>المنتجات</th><th>الإجمالي</th><th>الحالة</th></tr></thead>
                <tbody>{result.items.map((order) => <tr key={order.id}>
                  <td><strong className="admin-order-number">#{formatNumber(order.id)}</strong></td>
                  <td><div className="admin-order-customer"><strong>{order.customerName || "عميل"}</strong><small>{order.customerEmail || order.shippingPhoneNumber || ""}</small></div></td>
                  <td className="text-nowrap">{formatDate(order.createdAt)}</td>
                  <td>{formatNumber(order.itemCount)}</td>
                  <td className="text-nowrap fw-semibold">{formatSAR(order.totalAmount)}</td>
                  <td><span className={`admin-order-status ${String(order.status).toLowerCase()}`}>{labels[order.status] || order.status}</span></td>
                </tr>)}</tbody>
              </table></div>}
      </div>

      {!loading && !error && result.items.length > 0 && <nav className="admin-customers-pagination" aria-label="صفحات الطلبات">
        <span>صفحة {formatNumber(pageNumber)} من {formatNumber(pageCount)}</span>
        <div>
          <button type="button" onClick={() => updateParams(pageNumber - 1)} disabled={!result.prev} aria-label="الصفحة السابقة"><i className="bi bi-chevron-right" /> السابقة</button>
          {pages.map((page) => <button key={page} type="button" className={page === pageNumber ? "active" : ""} onClick={() => updateParams(page)} aria-current={page === pageNumber ? "page" : undefined}>{formatNumber(page)}</button>)}
          <button type="button" onClick={() => updateParams(pageNumber + 1)} disabled={!result.next} aria-label="الصفحة التالية">التالية <i className="bi bi-chevron-left" /></button>
        </div>
      </nav>}
    </section>
  );
}

export default AdminOrderDirectory;
