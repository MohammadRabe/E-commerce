import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api, normalizeProduct } from "../api";

function ProductPage({ addToCart }) {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    api.product(id).then((item) => { if (active) setProduct(normalizeProduct(item)); })
      .catch(() => { if (active) setProduct(null); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [id]);

  if (loading) return <div className="container py-5 text-center"><div className="spinner-border" role="status"/><p className="mt-3 text-muted">Loading product…</p></div>;

  if (!product) {
    return (
      <div className="container py-5">
        <div className="alert alert-danger">
          Product not found.
        </div>

        <Link to="/" className="btn btn-dark">
          Back to Products
        </Link>
      </div>
    );
  }

  return (
    <>
      <div className="container py-5">
        <Link
          to="/"
          className="btn btn-outline-secondary mb-4"
        >
          <i className="bi bi-arrow-left me-2"></i>
          Back to Products
        </Link>

        <div className="row g-5 align-items-center">
          <div className="col-lg-6">
            <img
              src={product.image}
              alt={product.title}
              className="product-detail-image shadow-sm"
            />
          </div>

          <div className="col-lg-6">
            <span className="badge bg-secondary mb-3">
              {product.category}
            </span>

            <h1 className="fw-bold">
              {product.title}
            </h1>

            <div className="mb-3">
              <span className="rating-stars me-2">
                {"★".repeat(Math.round(product.rating.rate))}
                {"☆".repeat(
                  5 - Math.round(product.rating.rate)
                )}
              </span>

              <span className="text-muted">
                {product.rating.rate} ({product.rating.count} reviews)
              </span>
            </div>

            <h2 className="fw-bold mb-4">
              ${product.price.toFixed(2)}
            </h2>

            <p className="text-muted lh-lg">
              {product.description}
            </p>

            <button
              className="btn btn-dark btn-lg mt-3"
              onClick={() => addToCart(product)}
            >
              <i className="bi bi-cart-plus me-2"></i>
              Add to Cart
            </button>
          </div>
        </div>
      </div>

    </>
  );
}

export default ProductPage;
