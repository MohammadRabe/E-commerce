import { NavLink, Link } from "react-router-dom";

function Navbar({ cartCount, user, logout }) {
  return (
    <nav className="navbar navbar-expand-lg bg-dark navbar-dark sticky-top shadow-sm">
      <div className="container">
        <Link className="navbar-brand" to="/">
          <i className="bi bi-flower1 me-2"></i>
          Shoply
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#mainNavbar"
          aria-controls="mainNavbar"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="mainNavbar">
          <div className="navbar-nav me-auto">
            <NavLink className="nav-link" to="/">
              Home
            </NavLink>
          </div>

          <div className="d-flex align-items-center gap-2">
            {user ? (
              <>
                <span className="text-white small">
                  Hi, {user.name}
                </span>

                <button
                  className="btn btn-outline-light btn-sm"
                  onClick={logout}
                >
                  Logout
                </button>
              </>
            ) : (
              <>
              <Link className="btn btn-outline-light btn-sm" to="/login">
                  Login
                </Link>

              <Link className="btn btn-light btn-sm" to="/signup">
                  Sign Up
                </Link>
              </>
            )}

            <Link
              className="btn btn-warning btn-sm position-relative"
              to="/cart"
            >
              <i className="bi bi-cart3 me-1"></i>
              Cart

              {cartCount > 0 && (
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                  {cartCount}
                </span>
              )}
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
