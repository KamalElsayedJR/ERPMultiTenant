import { ApiResponse } from './auth.models';

export type UserRole = 'Admin' | 'Manager' | 'Employee' | 'Accountant';

export interface TenantUser {
    userId: string;
    fullName: string;
    email: string;
    role: UserRole;
}

export interface InviteUserRequest {
    fullName: string;
    email: string;
    role: UserRole;
}

export interface UpdateUserRoleRequest {
    userId: string;
    role: UserRole;
}

export interface InviteUserData {
    user?: TenantUser;
    temporaryPassword?: string;
    tempPassword?: string;
}

export type TenantUsersResponse = ApiResponse<TenantUser[]>;

export type InviteUserResponse = ApiResponse<InviteUserData>;

export type UpdateUserRoleResponse = ApiResponse<TenantUser>;

export type DeleteUserResponse = ApiResponse<null>;
