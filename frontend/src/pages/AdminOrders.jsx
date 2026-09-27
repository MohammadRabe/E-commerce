import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";
import { formatSAR } from "../formatCurrency";

const statuses = ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"];
const statusLabels = { Pending: "قيد الانتظار", Processing: "قيد التجهيز", Shipped: "تم الشحن", Delivered: "تم التوصيل", Cancelled: "ملغي" };

function formatDate(value) {
  return new Intl.DateTimeFormat("ar-SA-u-ca-gregory", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function AdminOrders() {
  const [status, setStatus] = useState("Pending");
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [updatingOrderId, setUpdatingOrderId] = useState(null);
  const [actionError, setActionError] = useState("");
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState("");
  const [searchParams, setSearchParams] = useSearchParams();

  const openOrderDetails = async (orderId) => {
    setSelectedOrder(null);
    setDetailError("");
    setDetailLoading(true);
    try {
      setSelectedOrder(await api.orderDetails(orderId));
    } catch (reason) {
      setDetailError(getApiErrorMessage(reason, "ما قدرنا نحمّل تفاصيل الطلب."));
    } finally {
      setDetailLoading(false);
    }
  };

  const closeOrderDetails = () => {
    setSelectedOrder(null);
    setDetailError("");
  };

  useEffect(() => {
    const orderId = searchParams.get("orderId");
    if (!orderId) return;
    setSearchParams((current) => {
      const next = new URLSearchParams(current);
      next.delete("orderId");
      return next;
    }, { replace: true });
    openOrderDetails(orderId);
  }, []);

  useEffect(() => {
    if (!selectedOrder && !detailLoading && !detailError) return undefined;
    const closeOnEscape = (event) => {
      if (event.key === "Escape") closeOrderDetails();
    };
    window.addEventListener("keydown", closeOnEscape);
    return () => window.removeEventListener("keydown", closeOnEscape);
  }, [selectedOrder, detailLoading, detailError]);

  const loadOrders = () => {
    setLoading(true);
    setError("");
    return api.ordersByStatus(status)
      .then((result) => setOrders(result))
      .catch((reason) => setError(getApiErrorMessage(reason, "ما قدرنا نحمّل الطلبات.")))
      .finally(() => setLoading(false));
  };

  const updateOrderStatus = async (orderId, nextStatus) => {
    setUpdatingOrderId(orderId);
    setActionError("");
    try {
      await api.updateOrderStatus(orderId, nextStatus);
      await loadOrders();
    } catch (reason) {
      setActionError(getApiErrorMessage(reason, "ما قدرنا نحدّث الطلب. حاول مرة ثانية."));
    } finally {
      setUpdatingOrderId(null);
    }
  };

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    api.ordersByStatus(status)
      .then((result) => { if (active) setOrders(result); })
      .catch((reason) => { if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل الطلبات.")); })
      .finally(() => { if (active) setLoading(false); });

    return () => { active = false; };
  }, [status]);

  return (
    <section className="container py-5">
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3 mb-4">
        <div>
          <Link to="/dashboard" className="small text-muted text-decoration-none"><i className="bi bi-arrow-right me-1" aria-hidden="true" />لوحة التحكم</Link>
          <p className="text-uppercase text-muted small fw-semibold mb-1 mt-2">إدارة Sooq</p>
          <h1 className="h2 fw-bold mb-0">الطلبات</h1>
        </div>
        <label className="d-flex align-items-center gap-2">
          <span className="fw-semibold">حالة الطلب</span>
          <select className="form-select w-auto" value={status} onChange={(event) => setStatus(event.target.value)}>
            {statuses.map((item) => <option key={item} value={item}>{statusLabels[item]}</option>)}
          </select>
        </label>
      </div>

      <div className="card border-0 shadow-sm">
        <div className="card-header bg-white border-0 px-4 pt-4 pb-2">
          <div className="d-flex justify-content-between align-items-center">
            <h2 className="h5 fw-bold mb-0">طلبات {statusLabels[status]}</h2>
            {!loading && <span className="badge text-bg-light">{orders.length} طلب</span>}
          </div>
        </div>
        <div className="card-body p-0">
          {(actionError || error) ? (
            <div className="alert alert-danger m-4" role="alert">{actionError || error}</div>
          ) : loading ? (
            <div className="text-center py-5" role="status"><div className="spinner-border text-dark" /><p className="text-muted mt-3 mb-0">جاري تحميل الطلبات…</p></div>
          ) : orders.length === 0 ? (
            <p className="text-muted text-center py-5 mb-0">ما فيه طلبات {statusLabels[status]} حالياً.</p>
          ) : (
            <div className="table-responsive">
              <table className="table align-middle mb-0">
                <thead className="table-light">
                  <tr><th className="ps-4">الطلب</th><th>التاريخ</th><th>المنتجات</th><th>الإجمالي</th><th>عنوان التوصيل</th><th>الإجراءات</th></tr>
                </thead>
                <tbody>
                  {orders.map((order) => (
                    <tr key={order.id}>
                      <th className="ps-4"><button className="btn btn-link p-0 fw-bold text-dark" type="button" onClick={() => openOrderDetails(order.id)} aria-label={`عرض الطلب ${order.id}`}>#{order.id}</button></th>
                      <td className="text-nowrap">{formatDate(order.createdAt)}</td>
                      <td>{order.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0}</td>
                      <td className="text-nowrap">{formatSAR(order.totalAmount)}</td>
                      <td className="text-truncate" style={{ maxWidth: 280 }}>{order.shippingAddress}</td>
                      <td className="text-nowrap">
                        {status === "Pending" && <div className="d-flex gap-2">
                          <button className="btn btn-sm btn-success" type="button" disabled={updatingOrderId !== null} onClick={() => updateOrderStatus(order.id, "Processing")}>
                            {updatingOrderId === order.id ? "جاري التحديث…" : "قبول"}
                          </button>
                          <button className="btn btn-sm btn-outline-danger" type="button" disabled={updatingOrderId !== null} onClick={() => updateOrderStatus(order.id, "Cancelled")}>
                            إلغاء
                          </button>
                        </div>}
                        {status === "Processing" && <div className="d-flex gap-2">
                          <button className="btn btn-sm btn-success" type="button" disabled={updatingOrderId !== null} onClick={() => updateOrderStatus(order.id, "Shipped")}>
                            {updatingOrderId === order.id ? "جاري التحديث…" : "تم الشحن"}
                          </button>
                          <button className="btn btn-sm btn-outline-secondary" type="button" disabled={updatingOrderId !== null} onClick={() => updateOrderStatus(order.id, "Pending")}>
                            إرجاع للانتظار
                          </button>
                        </div>}
                        {status === "Delivered" && <button className="btn btn-sm btn-outline-secondary" type="button" disabled={updatingOrderId !== null} onClick={() => updateOrderStatus(order.id, "Processing")}>
                          {updatingOrderId === order.id ? "جاري التحديث…" : "إرجاع للتجهيز"}
                        </button>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {(selectedOrder || detailLoading || detailError) && <>
        <div className="modal-backdrop fade show" onClick={closeOrderDetails} />
        <div className="modal fade show d-block" role="dialog" aria-modal="true" aria-labelledby="order-detail-title" onMouseDown={(event) => { if (event.target === event.currentTarget) closeOrderDetails(); }}>
          <div className="modal-dialog modal-dialog-centered modal-lg modal-dialog-scrollable">
            <div className="modal-content border-0 shadow">
              <div className="modal-header px-4 py-3">
                <div>
                  <p className="eyebrow mb-1">تفاصيل الطلب</p>
                  <h2 className="modal-title h4 fw-bold mb-0" id="order-detail-title">{selectedOrder ? `الطلب #${selectedOrder.id}` : "الطلب"}</h2>
                </div>
                <button type="button" className="btn-close" aria-label="إغلاق" onClick={closeOrderDetails} />
              </div>
              <div className="modal-body p-4">
                {detailLoading ? <div className="text-center py-5" role="status"><div className="spinner-border text-dark" /><p className="text-muted mt-3 mb-0">جاري تحميل الطلب…</p></div>
                  : detailError ? <div className="alert alert-danger mb-0" role="alert">{detailError}</div>
                    : selectedOrder && <>
                      <div className="row g-3 mb-4">
                        <div className="col-sm-6"><div className="small text-muted">تاريخ الطلب</div><div className="fw-semibold">{formatDate(selectedOrder.createdAt)}</div></div>
                        <div className="col-sm-6"><div className="small text-muted">الحالة</div><div className="fw-semibold">{statusLabels[typeof selectedOrder.status === "number" ? statuses[selectedOrder.status] : selectedOrder.status] || selectedOrder.status}</div></div>
                        <div className="col-sm-6"><div className="small text-muted">عنوان التوصيل</div><div className="fw-semibold">{selectedOrder.shippingAddress}</div></div>
                        {selectedOrder.shippingPhoneNumber && <div className="col-sm-6"><div className="small text-muted">رقم الجوال</div><div className="fw-semibold">{selectedOrder.shippingPhoneNumber}</div></div>}
                      </div>
                      <h3 className="h6 fw-bold mb-3">المنتجات</h3>
                      <div className="table-responsive">
                        <table className="table align-middle">
                          <thead className="table-light"><tr><th>المنتج</th><th>الكمية</th><th>سعر الوحدة</th><th className="text-end">المجموع</th></tr></thead>
                          <tbody>{selectedOrder.items?.map((item) => <tr key={item.productId}><td>منتج #{item.productId}</td><td>{item.quantity}</td><td>{formatSAR(item.unitPrice)}</td><td className="text-end">{formatSAR(item.lineTotal)}</td></tr>)}</tbody>
                          <tfoot><tr><th colSpan="3" className="text-end">الإجمالي</th><th className="text-end">{formatSAR(selectedOrder.totalAmount)}</th></tr></tfoot>
                        </table>
                      </div>
                    </>}
              </div>
            </div>
          </div>
        </div>
      </>}
    </section>
  );
}

export default AdminOrders;
