import { useCallback, useState } from 'react';

export function useApi<TResult, TArgs extends unknown[]>(operation: (...args: TArgs) => Promise<TResult>) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(
    async (...args: TArgs): Promise<TResult> => {
      setLoading(true);
      setError(null);

      try {
        return await operation(...args);
      } catch (caughtError) {
        const message = caughtError instanceof Error ? caughtError.message : 'Operation failed';
        setError(message);
        throw caughtError;
      } finally {
        setLoading(false);
      }
    },
    [operation],
  );

  return { execute, loading, error, clearError: () => setError(null) };
}
