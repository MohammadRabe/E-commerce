import { Link } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import SARPrice from "../SARPrice";
const categoryNames = { electronics: "إلكترونيات", jewelery: "مجوهرات", "men's clothing": "أزياء رجالية", "women's clothing": "أزياء نسائية" };
function ProductCard({ product, addToCart, liked = false, onToggleLike, isAdmin = false, onDeleteProduct }) {
  const navigate = useNavigate();
  const productImage = product.image || product.imageUrl || product.ImageUrl || product.images?.[0] || product.ImageUrls?.[0] || product.Images?.[0] || "";
  return (
    <div className="card product-card h-100 shadow-sm">
      <div className="product-visual">
        <button className="product-image-button" type="button" onClick={() => navigate(`/product/${product.id}`)} aria-label={`عرض ${product.title}`}>
          <img src={productImage} className="card-img-top product-image" alt={product.title} loading="lazy" />
        </button>
        <button type="button" className="product-quick-add" onClick={() => addToCart(product)} aria-label={`أضف ${product.title} للسلة`}><i className="bi bi-cart3" aria-hidden="true" /></button>
        <button type="button" className={`product-like-button ${liked ? "is-liked" : ""}`} onClick={() => onToggleLike?.(product)} aria-label={liked ? `إزالة ${product.title} من المفضلة` : `إضافة ${product.title} للمفضلة`} aria-pressed={liked} title={liked ? "إزالة من المفضلة" : "إضافة للمفضلة"}><i className={`bi ${liked ? "bi-heart-fill" : "bi-heart"}`} aria-hidden="true" /></button>
        {isAdmin && <div className="product-admin-actions" aria-label="إجراءات الإدارة">
          <button type="button" className="product-admin-edit" onClick={() => navigate(`/dashboard/products/${product.id}/edit`)} aria-label={`تعديل ${product.title}`} title="تعديل المنتج"><i className="bi bi-pencil-square" aria-hidden="true" /></button>
          <button type="button" className="product-admin-delete" onClick={() => onDeleteProduct?.(product)} aria-label={`حذف ${product.title}`} title="حذف المنتج"><i className="bi bi-trash3" aria-hidden="true" /></button>
        </div>}
      </div>

      <div className="card-body d-flex flex-column">
        <span className="product-category mb-2">
          {categoryNames[product.category?.toLowerCase()] || product.category}
        </span>

        <h5 className="card-title">
          {product.title.length > 45
            ? `${product.title.substring(0, 45)}...`
            : product.title}
        </h5>

        <div className="product-rating"><span><i className="bi bi-star-fill" /> {Number(product.rating?.rate || 0).toFixed(1)}</span><span>{product.rating?.count || 0} تقييم</span></div>

        <div className="d-flex justify-content-between align-items-center mt-auto pt-3 gap-2">
          <span className="price"><SARPrice value={product.price} /></span>

          <Link
            to={`/product/${product.id}`}
            className="btn btn-outline-dark btn-sm"
          >
            التفاصيل
          </Link>
        </div>

        <button
          className="btn btn-dark w-100 mt-2 product-add-button"
          onClick={() => addToCart(product)}
        >
          <i className="bi bi-cart-plus me-1"></i>
          أضف للسلة
        </button>
      </div>
    </div>
  );
}

export default ProductCard;
