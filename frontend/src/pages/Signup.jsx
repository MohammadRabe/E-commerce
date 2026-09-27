import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api, getApiErrorMessage } from "../api";

function Signup({ onLogin }) {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    userName: "",
    fullName: "",
    email: "",
    password: "",
    confirmPassword: ""
  });

  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (
      !form.userName ||
      !form.fullName ||
      !form.email ||
      !form.password ||
      !form.confirmPassword
    ) {
      setError("عبي كل الخانات عشان نكمل.");
      return;
    }

    if (form.password !== form.confirmPassword) {
      setError("كلمتا المرور مو متطابقتين.");
      return;
    }

    if (form.fullName.trim().split(/\s+/).length !== 2) {
      setError("اكتب الاسم الأول واسم العائلة فقط.");
      return;
    }

    setSubmitting(true);
    try {
      const result = await api.signUp(form.userName.trim(), form.fullName.trim(), form.email, form.password);
      onLogin({ ...result, name: result.fullName || form.fullName.trim(), email: form.email });
      navigate("/");
    } catch (reason) { setError(getApiErrorMessage(reason, "ما قدرنا نسوي حسابك الحين. جرّب مرة ثانية.")); }
    finally { setSubmitting(false); }
  };

  return (
    <div className="container">
      <div className="card auth-card shadow-sm border-0">
        <div className="card-body p-4 p-md-5">
          <div className="text-center mb-4">
            <i className="bi bi-person-plus display-4"></i>

            <h2 className="fw-bold mt-2">
              حساب جديد
            </h2>

            <p className="text-muted">
              حيّاك معنا، تسوّق كل اللي تحتاجه بمكان واحد.
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
                اسم المستخدم
              </label>

              <input
                name="userName"
                className="form-control"
                value={form.userName}
                onChange={handleChange}
                autoComplete="username"
                required
              />
              <div className="form-text">اكتب اسم مستخدم بدون مسافات، مثل Mohammed123.</div>
            </div>

            <div className="mb-3">
              <label className="form-label">
                الاسم الكامل
              </label>

              <input
                name="fullName"
                className="form-control"
                value={form.fullName}
                onChange={handleChange}
                autoComplete="name"
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                البريد الإلكتروني
              </label>

              <input
                name="email"
                type="email"
                className="form-control"
                value={form.email}
                onChange={handleChange}
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                كلمة المرور
              </label>

              <input
                name="password"
                type="password"
                className="form-control"
                value={form.password}
                onChange={handleChange}
              />
            </div>

            <div className="mb-4">
              <label className="form-label">
                تأكيد كلمة المرور
              </label>

              <input
                name="confirmPassword"
                type="password"
                className="form-control"
                value={form.confirmPassword}
                onChange={handleChange}
              />
            </div>

            <button className="btn btn-dark w-100 btn-lg">
              {submitting ? "جاري إنشاء الحساب…" : "إنشاء حساب"}
            </button>
          </form>

          <p className="text-center mt-4 mb-0">
            عندك حساب؟{" "}
            <Link to="/login">
              سجّل دخولك
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

export default Signup;
