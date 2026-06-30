import { FormEvent, useEffect, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import type { AuthUser, UpdateUserRequest } from '../types';
import { getApiErrorMessage } from '../utils/helpers';

function splitDisplayName(displayName: string) {
  const parts = displayName.trim().split(/\s+/).filter(Boolean);
  return {
    firstName: parts[0] ?? '',
    lastName: parts.slice(1).join(' '),
  };
}

function createProfileState(user: AuthUser | null) {
  const parsedName = splitDisplayName(user?.displayName ?? '');

  return {
    firstName: user?.firstName ?? parsedName.firstName,
    lastName: user?.lastName ?? parsedName.lastName,
    email: user?.email ?? '',
    avatarUrl: user?.avatarUrl ?? '',
  };
}

export default function Profile() {
  const { user, tokens, updateProfile, changePassword } = useAuth();
  const [profile, setProfile] = useState(() => createProfileState(user));
  const [passwords, setPasswords] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [savingProfile, setSavingProfile] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [profileMessage, setProfileMessage] = useState<string | null>(null);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);

  useEffect(() => {
    setProfile(createProfileState(user));
  }, [user]);

  const handleProfileSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const firstName = profile.firstName.trim();
    const lastName = profile.lastName.trim();
    const email = profile.email.trim();
    const avatarUrl = profile.avatarUrl.trim();

    if (!firstName || !lastName) {
      setProfileError('First name and last name are required.');
      return;
    }

    if (!email) {
      setProfileError('Email is required.');
      return;
    }

    setSavingProfile(true);
    setProfileMessage(null);
    setProfileError(null);

    try {
      const request: UpdateUserRequest = {
        firstName,
        lastName,
        email,
        avatarUrl: avatarUrl || null,
      };

      await updateProfile(request);
      setProfileMessage('Profile updated.');
    } catch (caughtError) {
      setProfileError(
        getApiErrorMessage(caughtError, {
          defaultMessage: 'Unable to update profile right now.',
        }),
      );
    } finally {
      setSavingProfile(false);
    }
  };

  const handlePasswordSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!passwords.currentPassword || !passwords.newPassword) {
      setPasswordError('Current password and new password are required.');
      return;
    }

    if (passwords.newPassword.length < 8) {
      setPasswordError('New password must be at least 8 characters long.');
      return;
    }

    if (passwords.newPassword !== passwords.confirmPassword) {
      setPasswordError('New password confirmation does not match.');
      return;
    }

    setChangingPassword(true);
    setPasswordMessage(null);
    setPasswordError(null);

    try {
      await changePassword(passwords.currentPassword, passwords.newPassword);
      setPasswords({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setPasswordMessage('Password changed.');
    } catch (caughtError) {
      setPasswordError(
        getApiErrorMessage(caughtError, {
          defaultMessage: 'Unable to change password right now.',
        }),
      );
    } finally {
      setChangingPassword(false);
    }
  };

  return (
    <section className="page-shell">
      <div className="page-shell__header">
        <div className="stack stack--tight">
          <p className="eyebrow">Account</p>
          <h1>Profile</h1>
          <p className="page-note">Manage your account details, avatar URL, and password from the authenticated session.</p>
        </div>
      </div>
      {user ? (
        <div className="grid grid--2">
          <article className="card stack stack--tight">
            <h2>Session details</h2>
            <p><strong>ID:</strong> {user.id}</p>
            <p><strong>Name:</strong> {user.displayName}</p>
            <p><strong>Email:</strong> {user.email}</p>
            <p><strong>Avatar URL:</strong> {user.avatarUrl || 'Not set'}</p>
            <p><strong>Role:</strong> {user.role}</p>
            <p><strong>Access expires in:</strong> {tokens?.expiresIn ?? 0}s</p>
          </article>

          <article className="card stack">
            <div className="stack stack--tight">
              <h2>Profile details</h2>
              <p className="page-note">This updates your profile through the dedicated `users/me` endpoint and keeps the local session in sync.</p>
            </div>
            <form className="form" onSubmit={handleProfileSubmit}>
              <label className="field">
                <span className="field__label">First name</span>
                <input
                  value={profile.firstName}
                  onChange={(event) => setProfile((current) => ({ ...current, firstName: event.target.value }))}
                  autoComplete="given-name"
                  required
                />
              </label>
              <label className="field">
                <span className="field__label">Last name</span>
                <input
                  value={profile.lastName}
                  onChange={(event) => setProfile((current) => ({ ...current, lastName: event.target.value }))}
                  autoComplete="family-name"
                  required
                />
              </label>
              <label className="field">
                <span className="field__label">Email</span>
                <input
                  type="email"
                  value={profile.email}
                  onChange={(event) => setProfile((current) => ({ ...current, email: event.target.value }))}
                  autoComplete="email"
                  required
                />
              </label>
              <label className="field">
                <span className="field__label">Avatar URL</span>
                <input
                  type="url"
                  value={profile.avatarUrl}
                  onChange={(event) => setProfile((current) => ({ ...current, avatarUrl: event.target.value }))}
                  placeholder="https://example.com/avatar.png"
                  autoComplete="url"
                />
              </label>
              {profileMessage ? <p className="form__success">{profileMessage}</p> : null}
              {profileError ? <p className="form__error">{profileError}</p> : null}
              <button className="button" type="submit" disabled={savingProfile}>
                {savingProfile ? 'Saving...' : 'Save profile'}
              </button>
            </form>
          </article>

          <article className="card stack">
            <div className="stack stack--tight">
              <h2>Change password</h2>
              <p className="page-note">Your current password is required before the backend accepts a new one.</p>
            </div>
            <form className="form" onSubmit={handlePasswordSubmit}>
              <label className="field">
                <span className="field__label">Current password</span>
                <input
                  type="password"
                  value={passwords.currentPassword}
                  onChange={(event) => setPasswords((current) => ({ ...current, currentPassword: event.target.value }))}
                  autoComplete="current-password"
                  required
                />
              </label>
              <label className="field">
                <span className="field__label">New password</span>
                <input
                  type="password"
                  value={passwords.newPassword}
                  onChange={(event) => setPasswords((current) => ({ ...current, newPassword: event.target.value }))}
                  autoComplete="new-password"
                  required
                />
              </label>
              <label className="field">
                <span className="field__label">Confirm new password</span>
                <input
                  type="password"
                  value={passwords.confirmPassword}
                  onChange={(event) => setPasswords((current) => ({ ...current, confirmPassword: event.target.value }))}
                  autoComplete="new-password"
                  required
                />
              </label>
              {passwordMessage ? <p className="form__success">{passwordMessage}</p> : null}
              {passwordError ? <p className="form__error">{passwordError}</p> : null}
              <button className="button" type="submit" disabled={changingPassword}>
                {changingPassword ? 'Saving...' : 'Change password'}
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
