import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api, getApiErrorMessage, normalizeProduct } from "../api";
import SARPrice from "../components/SARPrice";

const categoryNames = { electronics: "إلكترونيات", jewelery: "مجوهرات", "men's clothing": "أزياء رجالية", "women's clothing": "أزياء نسائية" };

function ProductPage({ addToCart, cart, likedProductIds = [], toggleLike }) {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState("");
  const [displayImage, setDisplayImage] = useState("");
  const [imageVisible, setImageVisible] = useState(true);
  const [zoom, setZoom] = useState({ active: false, x: 50, y: 50, left: 0, top: 0 });
  const [galleryHovered, setGalleryHovered] = useState(false);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setProduct(null);
    setLoadError("");
    setQuantity(1);
    setSelectedImage("");
    setZoom({ active: false, x: 50, y: 50, left: 0, top: 0 });
    api.getProductById(id).then((productDto) => { if (active) { const normalized = normalizeProduct(productDto); setProduct(normalized); setSelectedImage(normalized.images[0] || normalized.image); } })
      .catch((reason) => { if (active) { setProduct(null); setLoadError(getApiErrorMessage(reason, "ما قدرنا نحمّل تفاصيل المنتج.")); } })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [id]);

  const productImages = product?.images?.length ? product.images : product?.image ? [product.image] : [];

  useEffect(() => {
    if (!selectedImage || selectedImage === displayImage) return undefined;
    setImageVisible(false);
    const transition = window.setTimeout(() => {
      setDisplayImage(selectedImage);
      requestAnimationFrame(() => setImageVisible(true));
    }, 140);
    return () => window.clearTimeout(transition);
  }, [selectedImage, displayImage]);

  useEffect(() => {
    if (!product || galleryHovered || productImages.length < 2) return undefined;
    const carousel = window.setInterval(() => {
      setSelectedImage((current) => {
        const currentIndex = productImages.indexOf(current);
        return productImages[(currentIndex + 1) % productImages.length];
      });
    }, 3000);
    return () => window.clearInterval(carousel);
  }, [product, productImages, galleryHovered]);

  if (loading) return <main className="product-page container"><div className="product-page-loading"><div className="spinner-border" role="status"/><p>جاري تحميل المنتج…</p></div></main>;

  if (!product) {
    return <main className="product-page container"><div className="product-page-empty"><div className="product-page-empty-icon"><i className="bi bi-box-seam" /></div><h1>ما لقينا المنتج</h1><p>{loadError || "يمكن المنتج مب متوفر أو الرابط غير صحيح."}</p><Link to="/" className="btn btn-dark"><i className="bi bi-arrow-right" /> رجوع للمنتجات</Link></div></main>;
  }

  const rating = Number(product.rating?.rate) || 0;
  const ratingCount = Number(product.rating?.count) || 0;
  const stockQuantity = product.stockQuantity == null ? null : Math.max(0, Number(product.stockQuantity));
  const alreadyInCart = cart.find((item) => Number(item.id) === Number(product.id))?.quantity ?? 0;
  const maxQuantity = stockQuantity == null ? 99 : Math.max(0, stockQuantity - alreadyInCart);
  const inStock = maxQuantity > 0;
  const liked = likedProductIds.includes(Number(product.id));
  const updateZoom = (event) => {
    const image = event.currentTarget.querySelector(".product-detail-image");
    const bounds = image?.getBoundingClientRect();
    if (!bounds) return;
    const wrapper = event.currentTarget.getBoundingClientRect();
    const lensSize = 240;
    setZoom({
      active: true,
      x: Math.max(0, Math.min(100, ((event.clientX - bounds.left) / bounds.width) * 100)),
      y: Math.max(0, Math.min(100, ((event.clientY - bounds.top) / bounds.height) * 100)),
      left: Math.max(0, Math.min(wrapper.width - lensSize, event.clientX - wrapper.left - lensSize / 2)),
      top: Math.max(0, Math.min(wrapper.height - lensSize, event.clientY - wrapper.top - lensSize / 2)),
    });
  };

  return (
    <main className="product-page container">
      <nav className="product-breadcrumb" aria-label="مسار الصفحة">
        <Link to="/">الرئيسية</Link><i className="bi bi-chevron-left" aria-hidden="true" />
        <Link to="/#products">المنتجات</Link><i className="bi bi-chevron-left" aria-hidden="true" />
        <span>{categoryNames[product.category?.toLowerCase()] || product.category}</span>
      </nav>

      <section className="product-detail-layout" aria-labelledby="product-title">
        <div className="product-gallery">
          <div className={`product-detail-image-wrap ${zoom.active ? "is-zoomed" : ""}`} onMouseEnter={() => setGalleryHovered(true)} onMouseMove={updateZoom} onMouseLeave={() => { setZoom((current) => ({ ...current, active: false })); setGalleryHovered(false); }}>
            <img src={displayImage || selectedImage || product.image} alt={product.title} className={`product-detail-image ${imageVisible ? "is-visible" : "is-transitioning"}`} />
            {zoom.active && <span className="product-image-zoom-preview" style={{ left: zoom.left, top: zoom.top, backgroundImage: `url("${selectedImage || product.image}")`, backgroundPosition: `${zoom.x}% ${zoom.y}%` }} aria-hidden="true" />}
          </div>
          {productImages.length > 1 && <div className="product-image-thumbnails" role="group" aria-label="صور المنتج">{productImages.map((image, index) => <button type="button" className={`product-image-thumbnail ${selectedImage === image ? "is-selected" : ""}`} key={`${image}-${index}`} onClick={() => { setSelectedImage(image); setZoom({ active: false, x: 50, y: 50 }); }} aria-label={`عرض الصورة ${index + 1}`} aria-pressed={selectedImage === image}><img src={image} alt="" loading="lazy" /></button>)}</div>}
          <p className="product-gallery-caption"><i className="bi bi-search" aria-hidden="true" /> مرّر المؤشر فوق الصورة للتكبير</p>
        </div>

        <div className="product-detail-content">
          <span className="product-detail-category">{categoryNames[product.category?.toLowerCase()] || product.category}</span>
          <h1 id="product-title" className="product-detail-title">{product.title}</h1>
          <button type="button" className={`product-detail-like ${liked ? "is-liked" : ""}`} onClick={() => toggleLike?.(product)} aria-pressed={liked}><i className={`bi ${liked ? "bi-heart-fill" : "bi-heart"}`} aria-hidden="true" /> {liked ? "أزل من المفضلة" : "أضف للمفضلة"}</button>

          <div className="product-detail-rating" aria-label={`التقييم ${rating} من 5`}>
            <span className="rating-stars" aria-hidden="true">{"★".repeat(Math.round(rating))}{"☆".repeat(5 - Math.round(rating))}</span>
            <span className="product-rating-value">{rating.toFixed(1)}</span>
            <span className="product-rating-count">({ratingCount} تقييم)</span>
          </div>

          <div className="product-detail-price-block">
            <span className="product-price-caption">السعر</span>
            <span className="product-detail-price"><SARPrice value={product.price} /></span>
          </div>

          <p className="product-detail-description">{product.description}</p>

          <div className={`product-stock ${inStock ? "is-available" : "is-unavailable"}`}>
            <i className={`bi ${inStock ? "bi-check-circle-fill" : "bi-x-circle-fill"}`} aria-hidden="true" />
            {inStock ? "متوفر وجاهز للطلب" : stockQuantity > 0 ? "أضفت كل الكمية المتوفرة للسلة" : "نفد من المخزون"}
          </div>

          <div className="product-detail-actions">
            <div className="product-quantity" aria-label="تحديد الكمية">
              <button type="button" aria-label="تقليل الكمية" disabled={quantity <= 1} onClick={() => setQuantity((current) => Math.max(1, current - 1))}><i className="bi bi-dash-lg" /></button>
              <output aria-live="polite" aria-label="الكمية المطلوبة">{quantity}</output>
              <button type="button" aria-label="زيادة الكمية" disabled={quantity >= maxQuantity} onClick={() => setQuantity((current) => Math.min(maxQuantity, current + 1))}><i className="bi bi-plus-lg" /></button>
            </div>
            <button className="btn btn-dark btn-lg product-detail-add" disabled={!inStock} onClick={() => { addToCart(product, quantity); setQuantity(1); }}>
              <i className="bi bi-cart-plus" aria-hidden="true" /> أضف للسلة
            </button>
            <Link to="/#products" className="product-back-link"><i className="bi bi-arrow-right" aria-hidden="true" /> متابعة التسوق</Link>
          </div>

          <div className="product-service-notes">
            <div><i className="bi bi-truck" aria-hidden="true" /><span><strong>توصيل موثوق</strong><small>تفاصيل الشحن تظهر عند إتمام الطلب</small></span></div>
            <div><i className="bi bi-shield-check" aria-hidden="true" /><span><strong>تسوّق بثقة</strong><small>دعمنا معك من الطلب إلى الاستلام</small></span></div>
          </div>
        </div>
      </section>
    </main>
  );
}

export default ProductPage;
