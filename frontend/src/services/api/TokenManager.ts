import type { AuthUser, JwtTokens } from '../../types';

const STORAGE_KEYS = {
  accessToken: 'drs.auth.accessToken',
  refreshToken: 'drs.auth.refreshToken',
  user: 'drs.auth.user',
} as const;

export class TokenManager {
  private readonly storage: Storage;

  constructor(storage: Storage = window.localStorage) {
    this.storage = storage;
  }

  setSession(tokens: JwtTokens, user?: AuthUser | null): void {
    this.storage.setItem(STORAGE_KEYS.accessToken, tokens.accessToken);
    this.storage.setItem(STORAGE_KEYS.refreshToken, tokens.refreshToken);
    if (user) {
      this.storage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
    }
  }

  getAccessToken(): string | null {
    return this.storage.getItem(STORAGE_KEYS.accessToken);
  }

  getRefreshToken(): string | null {
    return this.storage.getItem(STORAGE_KEYS.refreshToken);
  }

  getUser(): AuthUser | null {
    const rawUser = this.storage.getItem(STORAGE_KEYS.user);
    if (!rawUser) {
      return null;
    }

    try {
      return JSON.parse(rawUser) as AuthUser;
    } catch {
      this.storage.removeItem(STORAGE_KEYS.user);
      return null;
    }
  }

  getSession(): JwtTokens | null {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();

    if (!accessToken || !refreshToken) {
      return null;
    }

    return {
      accessToken,
      refreshToken,
      expiresIn: this.getAccessTokenExpiresInSeconds(accessToken),
    };
  }

  clearSession(): void {
    this.storage.removeItem(STORAGE_KEYS.accessToken);
    this.storage.removeItem(STORAGE_KEYS.refreshToken);
    this.storage.removeItem(STORAGE_KEYS.user);
  }

  hasSession(): boolean {
    return Boolean(this.getAccessToken() && this.getRefreshToken());
  }

  isAccessTokenExpired(skewSeconds = 30): boolean {
    const accessToken = this.getAccessToken();
    if (!accessToken) {
      return true;
    }

    const expiresAt = this.getAccessTokenExpiresAt(accessToken);
    if (!expiresAt) {
      return false;
    }

    return expiresAt.getTime() <= Date.now() + skewSeconds * 1000;
  }

  getAccessTokenExpiresAt(token = this.getAccessToken() ?? ''): Date | null {
    const payload = this.decodePayload(token);
    if (!payload || typeof payload.exp !== 'number') {
      return null;
    }

    return new Date(payload.exp * 1000);
  }

  getAccessTokenExpiresInSeconds(token = this.getAccessToken() ?? ''): number {
    const expiresAt = this.getAccessTokenExpiresAt(token);
    if (!expiresAt) {
      return 0;
    }

    return Math.max(0, Math.floor((expiresAt.getTime() - Date.now()) / 1000));
  }

  private decodePayload(token: string): Record<string, unknown> | null {
    if (!token) {
      return null;
    }

    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }

    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
      return JSON.parse(window.atob(padded)) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}

export const tokenManager = new TokenManager();