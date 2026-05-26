import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function Dashboard() {
  const { user, tokens, isAuthenticated } = useAuth();

  return (
    <section className="page-shell">
      <h1>Dashboard</h1>
      <p>To jest protected view dla zalogowanego użytkownika.</p>

      <div className="grid grid--2">
        <article className="card">
          <h2>Session</h2>
          <p>Status: {isAuthenticated ? 'authenticated' : 'anonymous'}</p>
          <p>Access token expires in: {tokens?.expiresIn ?? 0}s</p>
        </article>

        <article className="card">
          <h2>User</h2>
          {user ? (
            <>
              <p>{user.displayName}</p>
              <p>{user.email}</p>
              <p>{user.role}</p>
            </>
          ) : (
            <p>Brak załadowanego usera.</p>
          )}
        </article>
      </div>

      <div className="hero__actions">
        <Link className="button" to="/profile">
          Profile
        </Link>
        <Link className="button button--ghost" to="/users">
          Users CRUD
        </Link>
      </div>
    </section>
  );
}
