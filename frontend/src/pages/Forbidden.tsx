import { Link, useLocation } from 'react-router-dom';

export default function Forbidden() {
  const location = useLocation();
  const attemptedPath = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? null;

  return (
    <section className="page-shell">
      <div className="card stack">
        <p className="eyebrow">403</p>
        <h1>Access denied</h1>
        <p>
          Your current role does not allow access to this area.
          {attemptedPath ? ` Requested route: ${attemptedPath}.` : ''}
        </p>
        <div className="hero__actions">
          <Link className="button" to="/dashboard">
            Back to dashboard
          </Link>
          <Link className="button button--ghost" to="/profile">
            View profile
          </Link>
        </div>
      </div>
    </section>
  );
}