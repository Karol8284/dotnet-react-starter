import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useAuth } from '../hooks/useAuth';
import type { RegisterFormValues } from '../utils/authSchemas';
import { registerSchema } from '../utils/authSchemas';
import { getApiErrorMessage } from '../utils/helpers';

export default function Register() {
  const { register, loading, error, clearError } = useAuth();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const navigate = useNavigate();
  const {
    register: registerField,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      address: '',
      password: '',
    },
  });

  const onSubmit = async (values: RegisterFormValues) => {
    clearError();
    setSubmitError(null);

    try {
      await register({
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        phoneNumber: values.phoneNumber,
        address: values.address,
      });

      navigate('/dashboard', { replace: true });
    } catch (caughtError) {
      setSubmitError(
        getApiErrorMessage(caughtError, {
          defaultMessage: 'Unable to create the account right now. Please try again.',
          rateLimitMessage: 'Registration is temporarily rate limited. Please wait and try again.',
        }),
      );
    }
  };

  return (
    <section className="auth-layout auth-layout--wide">
      <article className="auth-callout">
        <p className="eyebrow">Professional onboarding</p>
        <h1>Create a workspace account.</h1>
        <p>
          Register against the backend JWT API and continue directly into the protected application shell.
        </p>
        <ul className="auth-callout__list">
          <li>Collects the same profile fields required by the backend contract.</li>
          <li>Prepares the account for role-based navigation and admin workflows.</li>
          <li>Keeps the first-run experience aligned with the production auth flow.</li>
        </ul>
      </article>

      <article className="auth-panel">
        <div className="auth-panel__header">
          <p className="eyebrow">New account</p>
          <h2>Register</h2>
          <p>Fill in the fields below to create a JWT-backed account.</p>
        </div>

        <form className="form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid--2">
            <label className="field">
              <span className="field__label">First name</span>
              <input
                autoComplete="given-name"
                placeholder="Karol"
                aria-invalid={errors.firstName ? 'true' : 'false'}
                {...registerField('firstName')}
              />
              {errors.firstName ? <span className="field__error">{errors.firstName.message}</span> : null}
            </label>
            <label className="field">
              <span className="field__label">Last name</span>
              <input
                autoComplete="family-name"
                placeholder="Kowalski"
                aria-invalid={errors.lastName ? 'true' : 'false'}
                {...registerField('lastName')}
              />
              {errors.lastName ? <span className="field__error">{errors.lastName.message}</span> : null}
            </label>
          </div>
          <label className="field">
            <span className="field__label">Email</span>
            <input
              type="email"
              autoComplete="email"
              placeholder="name@company.com"
              aria-invalid={errors.email ? 'true' : 'false'}
              {...registerField('email')}
            />
            {errors.email ? <span className="field__error">{errors.email.message}</span> : null}
          </label>
          <div className="grid grid--2">
            <label className="field">
              <span className="field__label">Phone</span>
              <input
                autoComplete="tel"
                placeholder="+48 123 456 789"
                aria-invalid={errors.phoneNumber ? 'true' : 'false'}
                {...registerField('phoneNumber')}
              />
              {errors.phoneNumber ? <span className="field__error">{errors.phoneNumber.message}</span> : null}
            </label>
            <label className="field">
              <span className="field__label">Address</span>
              <input
                autoComplete="street-address"
                placeholder="Warsaw, Poland"
                aria-invalid={errors.address ? 'true' : 'false'}
                {...registerField('address')}
              />
              {errors.address ? <span className="field__error">{errors.address.message}</span> : null}
            </label>
          </div>
          <label className="field">
            <span className="field__label">Password</span>
            <input
              type="password"
              autoComplete="new-password"
              placeholder="Choose a secure password"
              aria-invalid={errors.password ? 'true' : 'false'}
              {...registerField('password')}
            />
            {errors.password ? <span className="field__error">{errors.password.message}</span> : null}
            <span className="field__hint">This password is sent to the backend register endpoint and used for the first JWT session.</span>
          </label>
          {submitError ?? error ? <p className="form__error">{submitError ?? error}</p> : null}
          <button className="button button--block" type="submit" disabled={loading}>
            {loading ? 'Creating...' : 'Create account'}
          </button>
        </form>

        <p className="auth-panel__footer">
          Already registered? <Link to="/login">Sign in instead</Link>.
        </p>
      </article>
    </section>
  );
}
