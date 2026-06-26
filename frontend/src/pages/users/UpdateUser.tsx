import { Link, useParams } from 'react-router-dom';

export default function UpdateUser() {
  const { id } = useParams();

  return (
    <section className="page-shell">
      <div className="card stack">
        <p className="eyebrow">Not active</p>
        <h1>Update user</h1>
        <p className="page-note">
          Full profile editing for user <strong>{id ?? 'unknown'}</strong> is not exposed by the current backend contract.
        </p>
        <p className="page-note">
          The backend currently supports reading users, deleting users as admin, changing display name and updating roles through dedicated endpoints.
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
