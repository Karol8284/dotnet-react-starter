import { Link } from 'react-router-dom';

export default function CreateUser() {
  return (
    <section className="page-shell">
      <div className="card stack">
        <p className="eyebrow">Not active</p>
        <h1>Create user</h1>
        <p className="page-note">
          This screen is intentionally disabled. The current backend does not expose a user-creation endpoint for the admin area yet.
        </p>
        <div className="hero__actions">
          <Link className="button" to="/users">
            Back to users
          </Link>
          <Link className="button button--ghost" to="/admin">
            Open admin panel
          </Link>
        </div>
      </div>
    </section>
  );
}
