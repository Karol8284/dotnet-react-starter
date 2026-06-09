import type {
  ApiResponse,
  CreateUserRequest,
  CreateUserResponse,
  GetAllUsersResponse,
  GetUserResponse,
  UpdateUserRequest,
  UpdateUserResponse,
} from '../../types';
import { httpClient, type HttpClient } from './HttpClient';

export class UserApi {
  constructor(private readonly client: HttpClient = httpClient) {}

  getUsers(): Promise<GetAllUsersResponse> {
    return this.client.get<GetAllUsersResponse>('/users');
  }

  getUser(id: string): Promise<GetUserResponse> {
    return this.client.get<GetUserResponse>(`/users/${id}`);
  }

  createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
    return this.client.post<CreateUserResponse, CreateUserRequest>('/users', request);
  }

  updateUser(id: string, request: UpdateUserRequest): Promise<UpdateUserResponse> {
    return this.client.put<UpdateUserResponse, UpdateUserRequest>(`/users/${id}`, request);
  }

  deleteUser(id: string): Promise<ApiResponse<null>> {
    return this.client.delete<ApiResponse<null>>(`/users/${id}`);
  }
}

export const userApi = new UserApi();