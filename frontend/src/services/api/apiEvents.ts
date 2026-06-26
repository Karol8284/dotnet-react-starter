export type ApiNoticeCode = 'session-expired' | 'forbidden';

export interface ApiNotice {
  code: ApiNoticeCode;
  message: string;
}

type ApiNoticeListener = (notice: ApiNotice) => void;

const listeners = new Set<ApiNoticeListener>();

export function emitApiNotice(notice: ApiNotice) {
  listeners.forEach((listener) => listener(notice));
}

export function subscribeToApiNotices(listener: ApiNoticeListener) {
  listeners.add(listener);

  return () => {
    listeners.delete(listener);
  };
}