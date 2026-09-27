import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

function AdminCategoryForm() {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      const normalizedName = name.trim();
      const categories = await api.categories();
      const exists = (categories?.items ?? categories ?? []).some((category) =>
        String(category.name || "").trim().localeCompare(normalizedName, undefined, { sensitivity: "accent" }) === 0,
      );
      if (exists) {
        setError("هذه الفئة موجودة بالفعل. اختر اسماً مختلفاً.");
        return;
      }
      await api.createCategory({ name: normalizedName });
      navigate("/dashboard", { replace: true });
    } catch (reason) {
      setError(getApiErrorMessage(reason, "ما قدرنا نضيف الفئة. يمكن الاسم مستخدم من قبل."));
    } finally {
      setSaving(false);
    }
  };

  return <main className="container admin-form-page admin-category-form-page">
    <nav className="admin-customers-breadcrumb" aria-label="مسار التنقل"><Link to="/dashboard">لوحة التحكم</Link><i className="bi bi-chevron-left separator" /><span>إضافة فئة</span></nav>
    <header className="admin-form-heading"><span><i className="bi bi-folder-plus" /></span><div><p className="admin-customers-eyebrow">إدارة التصنيفات</p><h1>إضافة فئة جديدة</h1><p>نظّم المنتجات ضمن أقسام واضحة.</p></div></header>
    {error && <div className="alert alert-danger" role="alert">{error}</div>}
    <form className="admin-form-card admin-category-form-card" onSubmit={submit}>
      <label className="admin-field"><span>اسم الفئة</span><input autoFocus required maxLength="100" value={name} onChange={(event) => setName(event.target.value)} placeholder="مثال: إكسسوارات" /></label>
      <div className="admin-form-actions"><Link to="/dashboard" className="btn btn-outline-secondary">إلغاء</Link><button className="btn btn-dark" type="submit" disabled={saving}>{saving ? "جاري الإضافة…" : "إضافة الفئة"}</button></div>
    </form>
  </main>;
}

export default AdminCategoryForm;
