import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { httpClient } from '../../services/api';
import { subscribeToApiNotices, type ApiNotice } from '../../services/api/apiEvents';

export function AppNoticeCenter() {
  const { isAuthenticated, loading, refreshToken, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [notice, setNotice] = useState<ApiNotice | null>(null);

  useEffect(() => {
    if (loading) {
      return undefined;
    }

    httpClient.setUnauthorizedHandler(async () => {
      if (!isAuthenticated) {
        return false;
      }

      try {
        await refreshToken();
        return true;
      } catch {
        return false;
      }
    });

    return () => {
      httpClient.setUnauthorizedHandler(undefined);
    };
  }, [isAuthenticated, loading, refreshToken]);

  useEffect(() => {
    const unsubscribe = subscribeToApiNotices((nextNotice) => {
      setNotice(nextNotice);

      if (nextNotice.code === 'session-expired') {
        void logout().catch(() => undefined).finally(() => {
          navigate('/login', {
            replace: true,
            state: {
              from: location,
              reason: 'session-expired',
            },
          });
        });
      }
    });

    return unsubscribe;
  }, [location, logout, navigate]);

  if (!notice || notice.code === 'session-expired') {
    return null;
  }

  return (
    <div className="global-notice global-notice--warning" role="alert">
      <span>{notice.message}</span>
      <button className="button button--ghost" type="button" onClick={() => setNotice(null)}>
        Dismiss
      </button>
    </div>
  );
}