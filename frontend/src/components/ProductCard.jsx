import { Link } from "react-router-dom";

function ProductCard({ product, addToCart }) {
  return (
    <div className="card product-card h-100 shadow-sm">
      <img
        src={product.image}
        className="card-img-top product-image"
        alt={product.title}
      />

      <div className="card-body d-flex flex-column">
        <span className="badge bg-secondary align-self-start mb-2">
          {product.category}
        </span>

        <h5 className="card-title">
          {product.title.length > 45
            ? `${product.title.substring(0, 45)}...`
            : product.title}
        </h5>

        <div className="d-flex justify-content-between align-items-center mt-auto pt-3 gap-2">
          <span className="price">${product.price.toFixed(2)}</span>

          <Link
            to={`/product/${product.id}`}
            className="btn btn-outline-dark btn-sm"
          >
            View Details
          </Link>
        </div>

        <button
          className="btn btn-dark w-100 mt-2"
          onClick={() => addToCart(product)}
        >
          <i className="bi bi-cart-plus me-1"></i>
          Add to Cart
        </button>
      </div>
    </div>
  );
}

export default ProductCard;
