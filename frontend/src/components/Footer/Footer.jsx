import { Link } from "react-router-dom";

function Footer() {
  return (
    <footer className="bg-dark text-white mt-5 pt-5 pb-4">
      <div className="container">
        <div className="row gy-4">

          <div className="col-lg-5 col-md-6">
            <h2 className="fw-bold mb-3">
              <i className="bi bi-bag-check-fill me-2"></i>
              Sooq
            </h2>

            <p className="text-white-50 mb-4 footer-intro">
              أشياء مختارة بعناية لبيتك ويومك. جودة تعيش معك وتفاصيل تفرّحك.
            </p>

            <div className="d-flex gap-3">
              <a
                href="#"
                className="text-white fs-4"
                aria-label="فيسبوك"
              >
                <i className="bi bi-facebook"></i>
              </a>

              <a
                href="#"
                className="text-white fs-4"
                aria-label="إكس"
              >
                <i className="bi bi-twitter-x"></i>
              </a>

              <a
                href="#"
                className="text-white fs-4"
                aria-label="واتساب"
              >
                <i className="bi bi-whatsapp"></i>
              </a>

              <a
                href="#"
                className="text-white fs-4"
                aria-label="إنستغرام"
              >
                <i className="bi bi-instagram"></i>
              </a>

              <a
                target="_blank"
                href="https://github.com/MohammadRabe/Projects/tree/main/finalProject"
                className="text-white fs-4"
                aria-label="جيت هب"
              >
                <i className="bi bi-github"></i>
              </a>
            </div>
          </div>

          <div className="col-lg-3 col-md-6">
            <h5 className="fw-bold mb-3">روابط تهمك</h5>

            <ul className="list-unstyled">
              <li className="mb-3">
              <Link
                  to="/"
                  className="footer-link text-decoration-none"
                >
                  الرئيسية
                </Link>
              </li>

              <li className="mb-3">
                <a
                  href="/#products"
                  className="footer-link text-decoration-none"
                >
                  المنتجات
                </a>
              </li>

              <li className="mb-3">
                <Link
                  to="/cart"
                  className="footer-link text-decoration-none"
                >
                  السلة
                </Link>
              </li>
            </ul>
          </div>

          <div className="col-lg-4 col-md-6">
            <h5 className="fw-bold mb-3">عن Sooq</h5>

            <p className="text-white-50 mb-2">
              اختيارات مميزة، وأشياء تحبها كل يوم.
            </p>

            <p className="text-white-50 mb-0">
              تسوّق براحتك واختر اللي يناسبك.
            </p>
          </div>
        </div>

        <hr className="border-secondary my-4" />

        <div className="footer-bottom d-flex flex-column flex-md-row justify-content-between align-items-center gap-2" dir="ltr">
          <small className="footer-credit" dir="ltr">
            Built by Eng. Mohammed Rabie
          </small>
          <small className="text-white-50">
            © {new Date().getFullYear()} Sooq. جميع الحقوق محفوظة.
          </small>

          
        </div>
      </div>
    </footer>
  );
}

export default Footer;
