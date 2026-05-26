import { useAuth } from '../hooks/useAuth';

export default function Profile() {
  const { user, tokens } = useAuth();

  return (
    <section className="page-shell">
      <h1>Profile</h1>
      {user ? (
        <div className="card">
          <p><strong>ID:</strong> {user.id}</p>
          <p><strong>Name:</strong> {user.displayName}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>Role:</strong> {user.role}</p>
          <p><strong>Access expires in:</strong> {tokens?.expiresIn ?? 0}s</p>
        </div>
      ) : (
        <p>Brak sesji użytkownika.</p>
      )}
    </section>
  );
}
