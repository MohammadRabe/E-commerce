import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api, getApiErrorMessage, normalizeProduct } from "../api";

const blank = { title: "", description: "", price: "", categoryId: "", rating: "0", ratingCount: "0", stockQuantity: "0" };
const MAX_PRODUCT_IMAGES = 7;

function AdminProductForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const editing = Boolean(id);
  const [form, setForm] = useState(blank);
  const [categories, setCategories] = useState([]);
  const [images, setImages] = useState([]);
  const [currentImage, setCurrentImage] = useState("");
  const [loading, setLoading] = useState(editing);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    Promise.all([api.categories(), editing ? api.getProductById(id) : Promise.resolve(null)])
      .then(([categoryResult, productResult]) => {
        if (!active) return;
        setCategories(categoryResult?.items ?? categoryResult ?? []);
        if (productResult) {
          const product = normalizeProduct(productResult);
          setForm({ title: product.title || "", description: product.description || "", price: String(product.price ?? ""), categoryId: String(product.categoryId ?? productResult.categoryId ?? ""), rating: String(product.rating?.rate ?? 0), ratingCount: String(product.rating?.count ?? 0), stockQuantity: String(product.stockQuantity ?? 0) });
          setCurrentImage(product.image || "");
        }
      })
      .catch((reason) => { if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل بيانات النموذج.")); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [editing, id]);

  const updateField = (event) => setForm((current) => ({ ...current, [event.target.name]: event.target.value }));

  const selectImages = (event) => {
    const selected = Array.from(event.target.files || []);
    if (selected.length > MAX_PRODUCT_IMAGES) {
      setError(`تقدر ترفع ${MAX_PRODUCT_IMAGES} صور كحد أقصى للمنتج.`);
      event.target.value = "";
      setImages([]);
      return;
    }
    setError("");
    setImages(selected);
  };

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    const data = new FormData();
    data.append("Title", form.title.trim());
    data.append("Description", form.description.trim());
    data.append("Price", form.price);
    data.append("CategoryId", form.categoryId);
    data.append("Rating", form.rating || "0");
    data.append("RatingCount", form.ratingCount || "0");
    data.append("StockQuantity", form.stockQuantity || "0");
    images.forEach((image) => data.append("Images", image));
    try {
      if (editing) await api.updateProduct(id, data);
      else await api.createProduct(data);
      navigate("/", { replace: true });
    } catch (reason) {
      setError(getApiErrorMessage(reason, "ما قدرنا نحفظ المنتج. راجع البيانات وحاول مرة ثانية."));
    } finally {
      setSaving(false);
    }
  };

  return <main className="container admin-form-page">
    <nav className="admin-customers-breadcrumb" aria-label="مسار التنقل"><Link to="/dashboard">لوحة التحكم</Link><i className="bi bi-chevron-left separator" /><span>{editing ? "تعديل منتج" : "إضافة منتج"}</span></nav>
    <header className="admin-form-heading"><span><i className={`bi ${editing ? "bi-pencil-square" : "bi-plus-square"}`} /></span><div><p className="admin-customers-eyebrow">إدارة المنتجات</p><h1>{editing ? "تعديل المنتج" : "إضافة منتج جديد"}</h1><p>أدخل تفاصيل المنتج ليظهر في المتجر.</p></div></header>
    {error && <div className="alert alert-danger" role="alert">{error}</div>}
    {loading ? <div className="admin-customers-loading"><span className="spinner-border spinner-border-sm" /> جاري تحميل البيانات…</div> : <form className="admin-form-card" onSubmit={submit}>
      <div className="admin-form-grid">
        <label className="admin-field wide"><span>اسم المنتج</span><input required maxLength="200" name="title" value={form.title} onChange={updateField} placeholder="مثال: حقيبة يومية أنيقة" /></label>
        <label className="admin-field wide"><span>وصف المنتج</span><textarea required rows="5" maxLength="4000" name="description" value={form.description} onChange={updateField} placeholder="اكتب وصفاً واضحاً للمنتج" /></label>
        <label className="admin-field"><span>السعر</span><input required type="number" min="0.01" step="0.01" name="price" value={form.price} onChange={updateField} /></label>
        <label className="admin-field"><span>الفئة</span><select required name="categoryId" value={form.categoryId} onChange={updateField}><option value="">اختر الفئة</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
        <label className="admin-field"><span>المخزون</span><input required type="number" min="0" step="1" name="stockQuantity" value={form.stockQuantity} onChange={updateField} /></label>
        <label className="admin-field"><span>التقييم</span><input type="number" min="0" max="5" step="0.1" name="rating" value={form.rating} onChange={updateField} /></label>
        <label className="admin-field"><span>عدد التقييمات</span><input type="number" min="0" step="1" name="ratingCount" value={form.ratingCount} onChange={updateField} /></label>
        <label className="admin-field wide"><span>صور المنتج — الحد الأقصى {MAX_PRODUCT_IMAGES} صور {editing && "(اختياري لتغيير الصور)"}</span><input type="file" accept="image/*" multiple max={MAX_PRODUCT_IMAGES} required={!editing} onChange={selectImages} />{images.length > 0 && <small className="admin-image-count">تم اختيار {images.length} من {MAX_PRODUCT_IMAGES} صور</small>}{currentImage && images.length === 0 && <img className="admin-current-product-image" src={currentImage} alt="الصورة الحالية للمنتج" />}</label>
      </div>
      <div className="admin-form-actions"><Link to="/dashboard" className="btn btn-outline-secondary">إلغاء</Link><button className="btn btn-dark" type="submit" disabled={saving}>{saving ? "جاري الحفظ…" : editing ? "حفظ التعديلات" : "إضافة المنتج"}</button></div>
    </form>}
  </main>;
}

export default AdminProductForm;
