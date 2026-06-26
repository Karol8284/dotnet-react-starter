import { HttpError } from '../../services/api';
import { getApiErrorMessage } from '../../utils/helpers';

describe('getApiErrorMessage', () => {
  it('returns a friendly rate-limit message for 429 responses', () => {
    const error = new HttpError(429, 'Too many requests', null);

    expect(
      getApiErrorMessage(error, {
        defaultMessage: 'Default message',
        rateLimitMessage: 'Please wait before retrying.',
      })
    ).toBe('Please wait before retrying.');
  });

  it('returns the API payload message when available', () => {
    const error = new HttpError(400, 'Bad request', {
      message: 'Validation failed',
    });

    expect(
      getApiErrorMessage(error, {
        defaultMessage: 'Default message',
      })
    ).toBe('Validation failed');
  });

  it('falls back to the error message for generic errors', () => {
    expect(
      getApiErrorMessage(new Error('Network offline'), {
        defaultMessage: 'Default message',
      })
    ).toBe('Network offline');
  });

  it('falls back to the provided default message for unknown failures', () => {
    expect(
      getApiErrorMessage(null, {
        defaultMessage: 'Default message',
      })
    ).toBe('Default message');
  });
});