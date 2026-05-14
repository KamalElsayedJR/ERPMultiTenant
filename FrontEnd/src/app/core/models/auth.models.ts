export interface UserProfile {
  id?: string;
  fullName?: string;
  email?: string;
  role?: string;
  tenantId?: string;
  permissions?: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  companyName: string;
  email: string;
  password: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

export type AuthResponse = ApiResponse<LoginResponse>;

export type RefreshResponse = ApiResponse<LoginResponse>;

export type UserProfileResponse = ApiResponse<UserProfile>;

export type ChangePasswordResponse = ApiResponse<null>;
