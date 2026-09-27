import { Link } from "react-router-dom";

function NotFound() {
  return (
    <div className="container empty-state">
      <div className="text-center">
        <h1 className="display-1 fw-bold">
          404
        </h1>

        <h2>الصفحة مو موجودة</h2>

        <p className="text-muted">
          شكلك وصلت لرابط غلط. خلنا نرجعك للمكان الصحيح.
        </p>

        <Link to="/" className="btn btn-dark">
          للصفحة الرئيسية
        </Link>
      </div>
    </div>
  );
}

export default NotFound;
