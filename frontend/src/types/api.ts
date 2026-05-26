/**
 * GENERIC API TYPES
 * 
 * Generyczne wrappery dla wszystkich API responses
 * Mapuje się z backendu Shared/Responses/ApiResponse.cs
 */

// ============================================
// 1️⃣ GENERIC API RESPONSE WRAPPER
// ============================================

/** Generic API Response - maps to ApiResponse<T> from backend */
export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T | null;
  errors: ErrorDetail[] | null;
  timestamp: string;
}

/** Generic API Error Response */
export interface ApiError {
  statusCode: number;
  message: string;
  data: null;
  errors: ErrorDetail[];
  timestamp: string;
}

// ============================================
// 2️⃣ ERROR DETAIL
// ============================================

/** Error detail - maps to Shared/ErrorDetail.cs */
export interface ErrorDetail {
  /** Field name that caused the error (for form validation) */
  field?: string;
  /** Error message */
  message: string;
  /** Error code (e.g., "INVALID_EMAIL", "USER_NOT_FOUND") */
  code?: string;
}

// ============================================
// 3️⃣ HTTP STATUS CODES
// ============================================

export enum HttpStatusCode {
  // Success
  OK = 200,
  CREATED = 201,
  NO_CONTENT = 204,

  // Client Errors
  BAD_REQUEST = 400,
  UNAUTHORIZED = 401,
  FORBIDDEN = 403,
  NOT_FOUND = 404,
  CONFLICT = 409,
  UNPROCESSABLE_ENTITY = 422,

  // Server Errors
  INTERNAL_SERVER_ERROR = 500,
  SERVICE_UNAVAILABLE = 503,
}

// ============================================
// 4️⃣ AXIOS ERROR RESPONSE
// ============================================

/** Axios error response structure */
export interface AxiosErrorResponse<T = any> {
  status: number;
  statusText: string;
  data: T;
  headers: Record<string, string>;
}

// ============================================
// 5️⃣ REQUEST/RESPONSE HELPERS
// ============================================

/** Helper type for API request with loading state */
export interface AsyncRequest<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

/** Helper type for API request with pagination */
export interface PaginatedRequest<T> extends AsyncRequest<T[]> {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

// ============================================
// 6️⃣ VALIDATION TYPES
// ============================================

/** Validation rules for form fields */
export interface ValidationRule {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  pattern?: RegExp;
  custom?: (value: any) => boolean;
  message: string;
}

export type ValidationRules = Record<string, ValidationRule[]>;

/** Form errors map */
export type FormErrors = Record<string, string | undefined>;
