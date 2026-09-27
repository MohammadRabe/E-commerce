import { Link } from "react-router-dom";
import { formatSAR } from "../formatCurrency";

function CartPage({ cart, updateQuantity, removeFromCart }) {
  const total = cart.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  if (cart.length === 0) {
    return (
      <div className="container empty-state">
        <div className="text-center">
          <i className="bi bi-cart-x display-1 text-muted"></i>

          <h2 className="fw-bold mt-3">
            سلتك فاضية
          </h2>

          <p className="text-muted">
            يمكن طلبك الجاي هو المفضل عندك.
          </p>

          <Link to="/" className="btn btn-dark">
            تسوّق المنتجات
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-5">
      <p className="eyebrow mb-2">اختياراتك</p>
      <h1 className="section-title mb-4">سلة التسوق</h1>

      <div className="row g-4">
        <div className="col-lg-8">
          {cart.map((item) => (
            <div
              className="card border-0 shadow-sm mb-3"
              key={item.id}
            >
              <div className="card-body">
                <div className="row align-items-center g-3">
                  <div className="col-3 col-md-2">
                    <img
                      src={item.image}
                      alt={item.title}
                      className="cart-image"
                    />
                  </div>

                  <div className="col-9 col-md-4">
                    <h6 className="fw-bold">
                      {item.title}
                    </h6>

                    <span className="text-muted">
                        {formatSAR(item.price)}
                    </span>
                  </div>

                  <div className="col-6 col-md-3">
                    <div className="input-group">
                      <button
                        className="btn btn-outline-secondary"
                        disabled={item.stockQuantity != null && item.quantity >= Number(item.stockQuantity)}
                        aria-label={`زيادة كمية ${item.title}`}
                        onClick={() =>
                          updateQuantity(
                            item.id,
                            item.quantity - 1
                          )
                        }
                      >
                        -
                      </button>

                      <span className="form-control text-center">
                        {item.quantity}
                      </span>

                      <button
                        className="btn btn-outline-secondary"
                        onClick={() =>
                          updateQuantity(
                            item.id,
                            item.quantity + 1
                          )
                        }
                      >
                        +
                      </button>
                    </div>
                  </div>

                  <div className="col-4 col-md-2 text-end fw-bold">
                    {formatSAR(
                      item.price * item.quantity
                    )}
                  </div>

                  <div className="col-2 col-md-1 text-end">
                    <button
                      className="btn btn-outline-danger btn-sm"
                      onClick={() =>
                        removeFromCart(item.id)
                      }
                      title="احذف المنتج"
                    >
                      <i className="bi bi-trash"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        <div className="col-lg-4">
          <div className="card border-0 shadow-sm">
            <div className="card-body">
              <p className="eyebrow">باقي خطوة وتتهنى فيها</p>
              <h4 className="fw-bold">ملخص الطلب</h4>

              <hr />

              <div className="d-flex justify-content-between mb-2">
                <span>المجموع</span>
                <span>{formatSAR(total)}</span>
              </div>

              <div className="d-flex justify-content-between mb-3">
                <span>التوصيل</span>
                <span className="text-success">
                  علينا
                </span>
              </div>

              <hr />

              <div className="d-flex justify-content-between fw-bold fs-5 mb-4">
                <span>الإجمالي</span>
                <span>{formatSAR(total)}</span>
              </div>

              <Link
                className="btn btn-dark w-100 btn-lg"
                to="/checkout"
              >
                كمّل الطلب
              </Link>

              

              <Link
                to="/"
                className="btn btn-outline-secondary w-100 mt-3"
              >
                كمل تسوّق
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default CartPage;
