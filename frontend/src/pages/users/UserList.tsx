import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { userApi } from '../../services/api';
import type { UserDto } from '../../types';

export default function UserList() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;

    const loadUsers = async () => {
      try {
        const response = await userApi.getUsers();
        if (mounted) {
          setUsers(response.data ?? []);
        }
      } catch (caughtError) {
        if (mounted) {
          setError(caughtError instanceof Error ? caughtError.message : 'Failed to load users');
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    void loadUsers();

    return () => {
      mounted = false;
    };
  }, []);

  const handleDelete = async (id: string) => {
    await userApi.deleteUser(id);
    setUsers((current) => current.filter((user) => user.id !== id));
  };

  return (
    <section className="page-shell">
      <div className="page-shell__header">
        <div>
          <h1>Users</h1>
          <p>CRUD view pod przyszły backend UsersController.</p>
        </div>
        <Link className="button" to="/users/new">
          Create user
        </Link>
      </div>

      {loading ? <div className="page-state">Loading users...</div> : null}
      {error ? <p className="form__error">{error}</p> : null}

      <div className="grid grid--cards">
        {users.map((user) => (
          <article className="card" key={user.id}>
            <h2>
              {user.firstName} {user.lastName}
            </h2>
            <p>{user.email}</p>
            <p>{user.phoneNumber}</p>
            <p>{user.address}</p>
            <div className="hero__actions">
              <Link className="button button--ghost" to={`/users/${user.id}/edit`}>
                Edit
              </Link>
              <button className="button button--danger" type="button" onClick={() => void handleDelete(user.id)}>
                Delete
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
