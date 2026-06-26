/**
 * AUTH TYPES - Mapowanie z backendu C# DTOs
 * 
 * C# → TypeScript Mapping Rules:
 * - Guid → string (UUID format)
 * - DateTime → string (ISO 8601 format)
 * - UserRole enum → 'User' | 'Admin'
 * - string? (nullable) → string | null
 * - long → number
 */

// ============================================
// 1️⃣ REQUESTS (Frontend → Backend)
// ============================================

/** Login request - maps to LoginUserDto.cs */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Register request - maps to RegisterUserDto.cs */
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
  address: string;
}

/** Refresh token request */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/** Logout request */
export interface LogoutRequest {
  refreshToken: string;
}

/** Verify token request */
export interface VerifyTokenRequest {
  token: string;
}

// ============================================
// 2️⃣ RESPONSES (Backend → Frontend)
// ============================================

/** JWT token pair - maps to JwtTokens from User.cs */
export interface JwtTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // seconds (900 = 15 minutes)
}

/** Current authenticated user - maps to /api/auth/me response */
export interface AuthUser {
  id: string; // Guid → UUID string
  email: string;
  displayName: string;
  role: 'User' | 'Admin'; // UserRole enum
}

/** Login response from backend */
export interface LoginResponse {
  statusCode: number;
  message: string;
  data: JwtTokens;
  errors: ErrorDetail[] | null;
  timestamp: string;
}

/** Register response from backend */
export interface RegisterResponse {
  statusCode: number;
  message: string;
  data: JwtTokens;
  errors: ErrorDetail[] | null;
  timestamp: string;
}

/** Get current user (Me) response */
export interface MeResponse {
  statusCode: number;
  message: string;
  data: AuthUser;
  errors: ErrorDetail[] | null;
  timestamp: string;
}

/** Verify token response */
export interface VerifyTokenResponse {
  isValid: boolean;
  expiresAt?: string;
}

/** Logout response */
export interface LogoutResponse {
  statusCode: number;
  message: string;
  data: null;
  errors: ErrorDetail[] | null;
  timestamp: string;
}

// ============================================
// 3️⃣ ERROR HANDLING
// ============================================

/** Error detail from ApiResponse - maps to Shared/ErrorDetail.cs */
export interface ErrorDetail {
  field?: string;
  message: string;
  code?: string;
}

/** Generic API error response */
export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  data: null;
  errors: ErrorDetail[];
  timestamp: string;
}

// ============================================
// 4️⃣ CONTEXT STATE
// ============================================

/** Auth context state */
export interface AuthState {
  isAuthenticated: boolean;
  user: AuthUser | null;
  tokens: JwtTokens | null;
  loading: boolean;
  error: string | null;
}

/** Auth context value */
export interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<void>;
  updateDisplayName: (displayName: string) => Promise<void>;
  clearError: () => void;
}
