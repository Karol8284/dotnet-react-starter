import { HttpError } from '../services/api';
import type { ApiError } from '../types';

interface ApiErrorMessageOptions {
	defaultMessage: string;
	rateLimitMessage?: string;
}

export function getApiErrorMessage(error: unknown, options: ApiErrorMessageOptions): string {
	if (error instanceof HttpError) {
		const payload = error.payload as ApiError | null;

		if (error.status === 429 && options.rateLimitMessage) {
			return options.rateLimitMessage;
		}

		if (payload?.message) {
			return payload.message;
		}
	}

	if (error instanceof Error && error.message.trim()) {
		return error.message;
	}

	return options.defaultMessage;
}
