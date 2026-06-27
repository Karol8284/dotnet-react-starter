import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useAuth } from '../hooks/useAuth';
import type { LoginFormValues } from '../utils/authSchemas';
import { loginSchema } from '../utils/authSchemas';
import { getApiErrorMessage } from '../utils/helpers';

export default function Login() {
  const { login, loading, error, clearError } = useAuth();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location } | null)?.from?.pathname ?? '/dashboard';
  const reason = (location.state as { reason?: string } | null)?.reason;
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  const onSubmit = async (values: LoginFormValues) => {
    clearError();
    setSubmitError(null);

    try {
      await login(values.email, values.password);
      navigate(from, { replace: true });
    } catch (caughtError) {
      setSubmitError(
        getApiErrorMessage(caughtError, {
          defaultMessage: 'Unable to sign in right now. Please try again.',
          rateLimitMessage: 'Too many login attempts. Please wait a moment and try again.',
        }),
      );
    }
  };

  return (
    <section className="auth-layout">
      <article className="auth-callout">
        <p className="eyebrow">JWT access</p>
        <h1>Sign in to the control panel.</h1>
        <p>
          Access the authenticated area, bootstrap the current session from JWT and continue where you left off.
        </p>
        <ul className="auth-callout__list">
          <li>Automatic redirect back to the protected route you requested.</li>
          <li>Refresh-token based session recovery after page reload.</li>
          <li>Consistent error handling for invalid credentials and expired sessions.</li>
        </ul>
      </article>

      <article className="auth-panel">
        <div className="auth-panel__header">
          <p className="eyebrow">Welcome back</p>
          <h2>Log in</h2>
          <p>Use the account created through the backend JWT flow.</p>
        </div>

        {reason === 'session-expired' ? <p className="form__warning">Your session expired. Please sign in again.</p> : null}

        <form className="form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <label className="field">
            <span className="field__label">Email</span>
            <input
              type="email"
              autoComplete="email"
              placeholder="name@company.com"
              aria-invalid={errors.email ? 'true' : 'false'}
              {...register('email')}
            />
            {errors.email ? <span className="field__error">{errors.email.message}</span> : null}
          </label>
          <label className="field">
            <span className="field__label">Password</span>
            <input
              type="password"
              autoComplete="current-password"
              placeholder="Enter your password"
              aria-invalid={errors.password ? 'true' : 'false'}
              {...register('password')}
            />
            {errors.password ? <span className="field__error">{errors.password.message}</span> : null}
          </label>
          {submitError ?? error ? <p className="form__error">{submitError ?? error}</p> : null}
          <button className="button button--block" type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <p className="auth-panel__footer">
          No account yet? <Link to="/register">Create one here</Link>.
        </p>
      </article>
    </section>
  );
}
