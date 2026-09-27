import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

function Login({ onLogin, adminOnly = false }) {
  const navigate = useNavigate();

  const [identifier, setIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!identifier.trim() || !password) {
      setError("عبي كل الخانات عشان نكمل.");
      return;
    }

    setSubmitting(true);
    try {
      const result = adminOnly
        ? await api.adminSignIn(identifier.trim(), password)
        : await api.signIn(identifier.trim(), password);
      onLogin({ ...result, name: result.userName || identifier.trim().split("@")[0], email: identifier.includes("@") ? identifier.trim() : "" });
      navigate(adminOnly ? "/dashboard" : "/");
    } catch (reason) { setError(getApiErrorMessage(reason, "ما قدرنا ندخّلك. تأكد من بياناتك وحاول مرة ثانية.")); }
    finally { setSubmitting(false); }
  };

  return (
    <div className="container">
      <div className="card auth-card shadow-sm border-0">
        <div className="card-body p-4 p-md-5">
          <div className="text-center mb-4">
            <i className="bi bi-person-circle display-4"></i>

            <h2 className="fw-bold mt-2">
              {adminOnly ? "دخول المشرفين" : "يا هلا فيك"}
            </h2>

            <p className="text-muted">
              {adminOnly ? "هالصفحة مخصصة لإدارة متجر Sooq." : "سجّل دخولك لحسابك في Sooq."}
            </p>
          </div>

          {error && (
            <div className="alert alert-danger auth-error-message" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">
                اسم المستخدم أو البريد الإلكتروني
              </label>

              <input
                type="text"
                autoComplete="username"
                className="form-control"
                value={identifier}
                onChange={(e) =>
                  setIdentifier(e.target.value)
                }
                required
              />
            </div>

            <div className="mb-4">
              <label className="form-label">
                كلمة المرور
              </label>

              <input
                type="password"
                autoComplete="current-password"
                className="form-control"
                value={password}
                onChange={(e) =>
                  setPassword(e.target.value)
                }
              />
            </div>

            <button className="btn btn-dark w-100 btn-lg" disabled={submitting}>
              {submitting ? "جاري تسجيل الدخول…" : "دخول"}
            </button>
          </form>

          <p className="text-center mt-4 mb-0">
            ما عندك حساب؟{" "}
            {!adminOnly && <Link to="/signup">سوّ حسابك</Link>}
          </p>
        </div>
      </div>
    </div>
  );
}

export default Login;
