import React, { createContext, useContext, useEffect, useState } from 'react';
import type { AuthContextType, AuthState, AuthUser, JwtTokens, RegisterRequest } from '../types';
import { authApi } from '../services/api';
import { tokenManager } from '../services/api/TokenManager';

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  tokens: null,
  loading: true,
  error: null,
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>(initialState);

  useEffect(() => {
    let isMounted = true;

    const bootstrap = async () => {
      const session = tokenManager.getSession();

      if (!session) {
        if (isMounted) {
          setState((current) => ({ ...current, loading: false }));
        }
        return;
      }

      const storedUser = tokenManager.getUser();

      if (!tokenManager.isAccessTokenExpired() && storedUser) {
        if (isMounted) {
          setState({
            isAuthenticated: true,
            user: storedUser,
            tokens: session,
            loading: false,
            error: null,
          });
        }
        return;
      }

      try {
        const refreshed = await authApi.refreshToken({ refreshToken: session.refreshToken });
        if (!refreshed.data) {
          throw new Error('Refresh response missing token payload');
        }

        tokenManager.setSession(refreshed.data, storedUser ?? undefined);

        const currentUserResponse = await authApi.me();
        if (isMounted) {
          setState({
            isAuthenticated: true,
            user: currentUserResponse.data,
            tokens: refreshed.data,
            loading: false,
            error: null,
          });
        }
      } catch {
        tokenManager.clearSession();
        if (isMounted) {
          setState({ ...initialState, loading: false });
        }
      }
    };

    void bootstrap();

    return () => {
      isMounted = false;
    };
  }, []);

  const persistSession = (tokens: JwtTokens, user: AuthUser) => {
    tokenManager.setSession(tokens, user);
    setState({
      isAuthenticated: true,
      user,
      tokens,
      loading: false,
      error: null,
    });
  };

  const clearSession = () => {
    tokenManager.clearSession();
    setState({ ...initialState, loading: false });
  };

  const loadCurrentUser = async (): Promise<AuthUser> => {
    const response = await authApi.me();
    if (!response.data) {
      throw new Error('Current user payload missing');
    }

    tokenManager.setSession(tokenManager.getSession() as JwtTokens, response.data);
    return response.data;
  };

  const login = async (email: string, password: string) => {
    setState((current) => ({ ...current, loading: true, error: null }));

    try {
      const response = await authApi.login({ email, password });
      if (!response.data) {
        throw new Error('Login response missing tokens');
      }

      tokenManager.setSession(response.data);
      const currentUser = await loadCurrentUser();
      persistSession(response.data, currentUser);
    } catch (error) {
      clearSession();
      setState((current) => ({
        ...current,
        loading: false,
        error: error instanceof Error ? error.message : 'Login failed',
      }));
      throw error;
    }
  };

  const register = async (request: RegisterRequest) => {
    setState((current) => ({ ...current, loading: true, error: null }));

    try {
      const response = await authApi.register(request);
      if (!response.data) {
        throw new Error('Register response missing tokens');
      }

      tokenManager.setSession(response.data);
      const currentUser = await loadCurrentUser();
      persistSession(response.data, currentUser);
    } catch (error) {
      clearSession();
      setState((current) => ({
        ...current,
        loading: false,
        error: error instanceof Error ? error.message : 'Registration failed',
      }));
      throw error;
    }
  };

  const refreshToken = async () => {
    const refreshTokenValue = tokenManager.getRefreshToken();
    if (!refreshTokenValue) {
      throw new Error('Refresh token not available');
    }

    const response = await authApi.refreshToken({ refreshToken: refreshTokenValue });
    if (!response.data) {
      throw new Error('Refresh response missing tokens');
    }

    const nextUser = tokenManager.getUser();
    tokenManager.setSession(response.data, nextUser ?? undefined);
    setState((current) => ({
      ...current,
      isAuthenticated: Boolean(nextUser),
      tokens: response.data,
      user: nextUser,
      loading: false,
      error: null,
    }));
  };

  const logout = async () => {
    const refreshTokenValue = tokenManager.getRefreshToken();

    try {
      if (refreshTokenValue) {
        await authApi.logout({ refreshToken: refreshTokenValue });
      }
    } finally {
      clearSession();
    }
  };

  const clearError = () => setState((current) => ({ ...current, error: null }));

  const value: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    refreshToken,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuthContext(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuthContext must be used within AuthProvider');
  }

  return context;
}
