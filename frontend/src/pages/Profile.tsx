import { FormEvent, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { getApiErrorMessage } from '../utils/helpers';

export default function Profile() {
  const { user, tokens, updateDisplayName } = useAuth();
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const nextDisplayName = displayName.trim();

    if (!nextDisplayName) {
      setError('Display name is required.');
      return;
    }

    setSaving(true);
    setMessage(null);
    setError(null);

    try {
      await updateDisplayName(nextDisplayName);
      setMessage('Display name updated.');
    } catch (caughtError) {
      setError(
        getApiErrorMessage(caughtError, {
          defaultMessage: 'Unable to update display name right now.',
        }),
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="page-shell">
      <div className="page-shell__header">
        <div className="stack stack--tight">
          <p className="eyebrow">Account</p>
          <h1>Profile</h1>
          <p className="page-note">Manage the authenticated user session and public display name.</p>
        </div>
      </div>
      {user ? (
        <div className="grid grid--2">
          <article className="card stack stack--tight">
            <h2>Session details</h2>
            <p><strong>ID:</strong> {user.id}</p>
            <p><strong>Name:</strong> {user.displayName}</p>
            <p><strong>Email:</strong> {user.email}</p>
            <p><strong>Role:</strong> {user.role}</p>
            <p><strong>Access expires in:</strong> {tokens?.expiresIn ?? 0}s</p>
          </article>

          <article className="card stack">
            <div className="stack stack--tight">
              <h2>Display name</h2>
              <p className="page-note">This calls the backend display-name endpoint and updates the local session.</p>
            </div>
            <form className="form" onSubmit={handleSubmit}>
              <label className="field">
                <span className="field__label">Display name</span>
                <input
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                  autoComplete="name"
                  required
                />
              </label>
              {message ? <p className="form__success">{message}</p> : null}
              {error ? <p className="form__error">{error}</p> : null}
              <button className="button" type="submit" disabled={saving}>
                {saving ? 'Saving...' : 'Save display name'}
              </button>
            </form>
          </article>
        </div>
      ) : (
        <p>Brak sesji użytkownika.</p>
      )}
    </section>
  );
}
