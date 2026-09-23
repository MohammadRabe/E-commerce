import { Link } from "react-router-dom";

function NotFound() {
  return (
    <div className="container empty-state">
      <div className="text-center">
        <h1 className="display-1 fw-bold">
          404
        </h1>

        <h2>Page Not Found</h2>

        <p className="text-muted">
          The page you are looking for does not exist.
        </p>

        <Link to="/" className="btn btn-dark">
          Go Home
        </Link>
      </div>
    </div>
  );
}

export default NotFound;
