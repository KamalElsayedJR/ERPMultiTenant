export interface UserProfile {
    id?: string;
    fullName?: string;
    email?: string;
    role?: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    fullName: string;
    email: string;
    password: string;
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
