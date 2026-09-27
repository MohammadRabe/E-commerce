import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

const PAGE_SIZE = 10;
const formatNumber = (value) => new Intl.NumberFormat("ar-SA").format(value);

function AdminCustomers() {
  const [searchParams, setSearchParams] = useSearchParams();
  const pageNumber = Math.max(1, Number(searchParams.get("page")) || 1);
  const [searchInput, setSearchInput] = useState(searchParams.get("search") || "");
  const search = searchParams.get("search") || "";
  const [result, setResult] = useState({ items: [], totalCount: 0, next: false, prev: false });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [deletingUserId, setDeletingUserId] = useState("");

  useEffect(() => setSearchInput(search), [search]);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    api.customers({ pageNumber, pageSize: PAGE_SIZE, search })
      .then((data) => { if (active) setResult(data); })
      .catch((reason) => { if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل قائمة العملاء.")); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [pageNumber, search]);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      const nextSearch = searchInput.trim();
      if (nextSearch === search) return;
      setSearchParams(nextSearch ? { search: nextSearch } : {}, { replace: true });
    }, 300);
    return () => window.clearTimeout(timeout);
  }, [searchInput, search, setSearchParams]);

  const start = result.totalCount === 0 ? 0 : (pageNumber - 1) * PAGE_SIZE + 1;
  const end = Math.min(pageNumber * PAGE_SIZE, result.totalCount);
  const pageCount = Math.max(1, Math.ceil(result.totalCount / PAGE_SIZE));
  const pagination = useMemo(() => Array.from({ length: pageCount }, (_, index) => index + 1), [pageCount]);

  const goToPage = (page) => {
    const next = {};
    if (search) next.search = search;
    if (page > 1) next.page = String(page);
    setSearchParams(next);
  };

  const submitSearch = (event) => {
    event.preventDefault();
    const nextSearch = searchInput.trim();
    setSearchParams(nextSearch ? { search: nextSearch } : {}, { replace: true });
  };

  const deleteCustomer = async (customer) => {
    const label = customer.userName || customer.email || "هذا العميل";
    if (!window.confirm(`هل تريد حذف حساب «${label}»؟ لا يمكن التراجع عن هذا الإجراء.`)) return;
    setDeletingUserId(customer.id);
    setError("");
    try {
      await api.deleteCustomer(customer.id);
      if (result.items.length === 1 && pageNumber > 1) {
        goToPage(pageNumber - 1);
      } else {
        setResult((current) => ({
          ...current,
          items: current.items.filter((item) => item.id !== customer.id),
          totalCount: Math.max(0, current.totalCount - 1),
        }));
      }
    } catch (reason) {
      setError(getApiErrorMessage(reason, "ما قدرنا نحذف حساب العميل."));
    } finally {
      setDeletingUserId("");
    }
  };

  return (
    <section className="container admin-customers-page">
      <nav className="admin-customers-breadcrumb" aria-label="مسار التنقل">
        <Link to="/dashboard"><i className="bi bi-grid-1x2" aria-hidden="true" /> لوحة التحكم</Link>
        <i className="bi bi-chevron-left separator" aria-hidden="true" />
        <Link to="/dashboard/statistics">إحصائيات المبيعات</Link>
        <i className="bi bi-chevron-left separator" aria-hidden="true" />
        <span aria-current="page">العملاء</span>
      </nav>

      <header className="admin-customers-hero">
        <div className="admin-customers-hero-copy">
          <span className="admin-customers-eyebrow"><i className="bi bi-people" aria-hidden="true" /> إدارة العملاء</span>
          <h1>عملاء متجرك</h1>
          <p>ابحث بسرعة بين الحسابات المسجّلة، وتصفّح القائمة صفحة بصفحة.</p>
        </div>
        <div className="admin-customers-total"><span>إجمالي العملاء</span><strong>{formatNumber(result.totalCount)}</strong><i className="bi bi-person-vcard" aria-hidden="true" /></div>
      </header>

      <div className="admin-customers-toolbar">
        <div><h2>قائمة العملاء</h2><p>{loading ? "جاري تحديث النتائج…" : `عرض ${formatNumber(start)}–${formatNumber(end)} من ${formatNumber(result.totalCount)} عميل`}</p></div>
        <form className="admin-customers-search" role="search" onSubmit={submitSearch}>
          <i className="bi bi-search" aria-hidden="true" />
          <input type="search" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="ابحث بالاسم أو البريد أو الجوال" aria-label="ابحث عن عميل" />
          {searchInput && <button type="button" className="admin-customers-search-clear" aria-label="مسح البحث" onClick={() => setSearchInput("")}><i className="bi bi-x-lg" /></button>}
          <button type="submit" className="admin-customers-search-submit">بحث</button>
        </form>
      </div>

      <div className="admin-customers-card">
        {error ? <div className="alert alert-danger m-4" role="alert">{error}</div>
          : loading ? <div className="admin-customers-loading" role="status"><span className="spinner-border spinner-border-sm" /> جاري تحميل العملاء…</div>
            : result.items.length === 0 ? <div className="admin-customers-empty"><span><i className="bi bi-person-exclamation" aria-hidden="true" /></span><strong>{search ? "ما لقينا عميل بهذا البحث" : "ما فيه عملاء للعرض حالياً"}</strong><p>{search ? "جرّب اسم أو بريد أو رقم جوال مختلف." : "سيظهر العملاء المسجّلون هنا."}</p></div>
              : <div className="table-responsive"><table className="table admin-customers-table align-middle mb-0">
                <thead><tr><th>العميل</th><th>البريد الإلكتروني</th><th>رقم الجوال</th><th className="text-end">الحالة</th><th className="text-end">إجراء</th></tr></thead>
                <tbody>{result.items.map((customer, index) => <tr key={customer.id}>
                  <td><div className="admin-customer-identity"><span className="admin-customer-avatar">{(customer.userName || customer.email || "؟").slice(0, 1).toLocaleUpperCase("ar")}</span><div><strong>{customer.userName || "بدون اسم"}</strong><small>عميل #{formatNumber((pageNumber - 1) * PAGE_SIZE + index + 1)}</small></div></div></td>
                  <td>{customer.email || <span className="text-muted">غير مضاف</span>}</td>
                  <td dir="ltr" className="admin-customer-phone">{customer.phoneNumber || <span dir="rtl" className="text-muted">غير مضاف</span>}</td>
                  <td className="text-end"><span className="admin-customer-status"><i /> نشط</span></td>
                  <td className="text-end"><button type="button" className="btn btn-outline-danger btn-sm admin-customer-delete" onClick={() => deleteCustomer(customer)} disabled={deletingUserId === customer.id} aria-label={`حذف حساب ${customer.userName || customer.email || "العميل"}`} title="حذف العميل"><i className={`bi ${deletingUserId === customer.id ? "bi-hourglass-split" : "bi-trash3"}`} aria-hidden="true" />{deletingUserId === customer.id ? " جارٍ الحذف" : " حذف"}</button></td>
                </tr>)}</tbody>
              </table></div>}
      </div>

      {!loading && !error && result.items.length > 0 && <nav className="admin-customers-pagination" aria-label="صفحات العملاء">
        <span>صفحة {formatNumber(pageNumber)} من {formatNumber(pageCount)}</span>
        <div>
          <button type="button" onClick={() => goToPage(pageNumber - 1)} disabled={!result.prev} aria-label="الصفحة السابقة"><i className="bi bi-chevron-right" /> السابقة</button>
          {pagination.map((page) => <button key={page} type="button" className={page === pageNumber ? "active" : ""} onClick={() => goToPage(page)} aria-current={page === pageNumber ? "page" : undefined}>{formatNumber(page)}</button>)}
          <button type="button" onClick={() => goToPage(pageNumber + 1)} disabled={!result.next} aria-label="الصفحة التالية">التالية <i className="bi bi-chevron-left" /></button>
        </div>
      </nav>}
    </section>
  );
}

export default AdminCustomers;
