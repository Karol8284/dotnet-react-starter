import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { userApi } from '../services/api';

export default function AdminPanel() {
  const [userCount, setUserCount] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;

    const loadOverview = async () => {
      try {
        const response = await userApi.getUserCount();
        if (mounted) {
          setUserCount(response.data ?? 0);
        }
      } catch (caughtError) {
        if (mounted) {
          setError(caughtError instanceof Error ? caughtError.message : 'Failed to load admin overview');
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    void loadOverview();

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <section className="page-shell">
      <div className="page-shell__header">
        <div className="stack stack--tight">
          <p className="eyebrow">Admin only</p>
          <h1>Admin panel</h1>
          <p className="page-note">This area is routed only for users with the Admin role.</p>
        </div>
        <Link className="button button--ghost" to="/users">
          Open user directory
        </Link>
      </div>

      {loading ? <div className="page-state">Loading admin overview...</div> : null}
      {error ? <p className="form__error">{error}</p> : null}

      <div className="grid grid--cards">
        <article className="card stack stack--tight">
          <p className="eyebrow">Users</p>
          <h2>{userCount ?? 0}</h2>
          <p className="page-note">Live count from the backend admin endpoint.</p>
        </article>

        <article className="card stack stack--tight">
          <p className="eyebrow">Access model</p>
          <h2>Role-gated routes</h2>
          <p className="page-note">Navigation and route guards now follow the JWT role claim returned by the API.</p>
        </article>
      </div>
    </section>
  );
}