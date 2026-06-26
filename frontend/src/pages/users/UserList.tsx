import { useEffect, useState } from 'react';
import { userApi } from '../../services/api';
import { useAuth } from '../../hooks/useAuth';
import type { UserDto } from '../../types';

export default function UserList() {
  const { user } = useAuth();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [roleAction, setRoleAction] = useState<string | null>(null);
  const [roleMessage, setRoleMessage] = useState<string | null>(null);
  const isAdmin = user?.role === 'Admin';

  useEffect(() => {
    let mounted = true;

    const loadUsers = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await userApi.getUsers(pageNumber, pageSize);
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
  }, [pageNumber, pageSize]);

  const handleDelete = async (id: string) => {
    setDeleteError(null);

    try {
      await userApi.deleteUser(id);
      setUsers((current) => current.filter((currentUser) => currentUser.id !== id));
    } catch (caughtError) {
      setDeleteError(caughtError instanceof Error ? caughtError.message : 'Failed to delete user');
    }
  };

  const handleRoleChange = async (id: string, role: 'User' | 'Admin') => {
    setRoleAction(`${id}:${role}`);
    setRoleMessage(null);
    setDeleteError(null);

    try {
      await userApi.updateUserRole(id, role);
      setRoleMessage(`Role update request saved as ${role}.`);
    } catch (caughtError) {
      setDeleteError(caughtError instanceof Error ? caughtError.message : 'Failed to update role');
    } finally {
      setRoleAction(null);
    }
  };

  return (
    <section className="page-shell">
      <div className="page-shell__header">
        <div className="stack stack--tight">
          <h1>Users</h1>
          <p className="page-note">
            Authenticated directory view. Administrative actions appear only for users with the Admin role.
          </p>
        </div>
        {isAdmin ? <div className="role-badge role-badge--admin">Admin access</div> : <div className="role-badge">Read-only</div>}
      </div>

      <div className="toolbar">
        <div className="toolbar__group">
          <button className="button button--ghost" type="button" disabled={pageNumber === 1 || loading} onClick={() => setPageNumber((page) => Math.max(1, page - 1))}>
            Previous
          </button>
          <span className="role-badge">Page {pageNumber}</span>
          <button className="button button--ghost" type="button" disabled={loading || users.length < pageSize} onClick={() => setPageNumber((page) => page + 1)}>
            Next
          </button>
        </div>
        <label className="toolbar__select">
          Page size
          <select value={pageSize} onChange={(event) => { setPageNumber(1); setPageSize(Number(event.target.value)); }}>
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </select>
        </label>
      </div>

      {loading ? <div className="page-state">Loading users...</div> : null}
      {error ? <p className="form__error">{error}</p> : null}
      {deleteError ? <p className="form__error">{deleteError}</p> : null}
      {roleMessage ? <p className="form__success">{roleMessage}</p> : null}

      <div className="grid grid--cards">
        {users.map((user) => (
          <article className="card" key={user.id}>
            <h2>
              {user.firstName} {user.lastName}
            </h2>
            <p>{user.email}</p>
            <p>{user.phoneNumber}</p>
            <p>{user.address}</p>
            {isAdmin ? (
              <div className="hero__actions">
                <button
                  className="button button--ghost"
                  type="button"
                  disabled={roleAction === `${user.id}:User`}
                  onClick={() => void handleRoleChange(user.id, 'User')}
                >
                  Set User
                </button>
                <button
                  className="button button--ghost"
                  type="button"
                  disabled={roleAction === `${user.id}:Admin`}
                  onClick={() => void handleRoleChange(user.id, 'Admin')}
                >
                  Set Admin
                </button>
                <button className="button button--danger" type="button" onClick={() => void handleDelete(user.id)}>
                  Delete
                </button>
              </div>
            ) : null}
          </article>
        ))}
      </div>
      {!loading && users.length === 0 ? <div className="page-state">No users found on this page.</div> : null}
    </section>
  );
}
