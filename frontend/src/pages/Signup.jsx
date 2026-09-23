import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";

function Signup({ onLogin }) {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    name: "",
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
      !form.name ||
      !form.email ||
      !form.password ||
      !form.confirmPassword
    ) {
      setError("Please fill in all fields.");
      return;
    }

    if (form.password !== form.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setSubmitting(true);
    try {
      const result = await api.signUp(form.name, form.email, form.password);
      onLogin({ ...result, name: result.userName || form.name, email: form.email });
      navigate("/");
    } catch (reason) { setError(reason.message || "Could not create your account."); }
    finally { setSubmitting(false); }
  };

  return (
    <div className="container">
      <div className="card auth-card shadow-sm border-0">
        <div className="card-body p-4 p-md-5">
          <div className="text-center mb-4">
            <i className="bi bi-person-plus display-4"></i>

            <h2 className="fw-bold mt-2">
              Create Account
            </h2>

            <p className="text-muted">
              Join us for a more considered everyday.
            </p>
          </div>

          {error && (
            <div className="alert alert-danger">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">
                Full Name
              </label>

              <input
                name="name"
                className="form-control"
                value={form.name}
                onChange={handleChange}
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                Email
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
                Password
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
                Confirm Password
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
              {submitting ? "Creating account…" : "Sign Up"}
            </button>
          </form>

          <p className="text-center mt-4 mb-0">
            Already have an account?{" "}
            <Link to="/login">
              Login
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

export default Signup;
