import { Link } from "react-router-dom";

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
            Your cart is empty
          </h2>

          <p className="text-muted">
            Add some products before viewing your cart.
          </p>

          <Link to="/" className="btn btn-dark">
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-5">
      <h1 className="fw-bold mb-4">
        Shopping Cart
      </h1>

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
                        ${Number(item.price).toFixed(2)}
                    </span>
                  </div>

                  <div className="col-6 col-md-3">
                    <div className="input-group">
                      <button
                        className="btn btn-outline-secondary"
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
                    $
                    {(
                      item.price * item.quantity
                    ).toFixed(2)}
                  </div>

                  <div className="col-2 col-md-1 text-end">
                    <button
                      className="btn btn-outline-danger btn-sm"
                      onClick={() =>
                        removeFromCart(item.id)
                      }
                      title="Remove item"
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
              <h4 className="fw-bold">
                Cart Summary
              </h4>

              <hr />

              <div className="d-flex justify-content-between mb-2">
                <span>Subtotal</span>
                <span>${total.toFixed(2)}</span>
              </div>

              <div className="d-flex justify-content-between mb-3">
                <span>Shipping</span>
                <span className="text-success">
                  Free
                </span>
              </div>

              <hr />

              <div className="d-flex justify-content-between fw-bold fs-5 mb-4">
                <span>Total</span>
                <span>${total.toFixed(2)}</span>
              </div>

              <Link
                className="btn btn-dark w-100 btn-lg"
                to="/checkout"
              >
                Checkout
              </Link>

              

              <Link
                to="/"
                className="btn btn-outline-secondary w-100 mt-3"
              >
                Continue Shopping
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default CartPage;
