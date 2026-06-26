import type {
  ApiResponse,
  AuthUser,
  JwtTokens,
  LoginRequest,
  MeResponse,
  RegisterRequest,
  VerifyTokenRequest,
  VerifyTokenResponse,
} from '../../types';
import { httpClient, type HttpClient } from './HttpClient';

export class AuthApi {
  constructor(private readonly client: HttpClient = httpClient) {}

  login(request: LoginRequest): Promise<ApiResponse<JwtTokens>> {
    return this.client.post<ApiResponse<JwtTokens>, LoginRequest>('/auth/login', request, {
      skipAuth: true,
    });
  }

  register(request: RegisterRequest): Promise<ApiResponse<JwtTokens>> {
    return this.client.post<ApiResponse<JwtTokens>, RegisterRequest>('/auth/register', request, {
      skipAuth: true,
    });
  }

  refreshToken(): Promise<ApiResponse<JwtTokens>> {
    return this.client.post<ApiResponse<JwtTokens>, undefined>('/auth/refresh-token', undefined, {
      skipAuth: true,
    });
  }

  logout(): Promise<ApiResponse<null>> {
    return this.client.post<ApiResponse<null>, undefined>('/auth/logout');
  }

  me(): Promise<ApiResponse<AuthUser>> {
    return this.client.get<MeResponse>('/auth/me');
  }

  verifyToken(request: VerifyTokenRequest): Promise<ApiResponse<VerifyTokenResponse>> {
    return this.client.post<ApiResponse<VerifyTokenResponse>, VerifyTokenRequest>('/auth/verify-token', request, {
      skipAuth: true,
    });
  }
}

export const authApi = new AuthApi();