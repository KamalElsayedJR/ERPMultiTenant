import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import {
    DeleteUserResponse,
    InviteUserRequest,
    InviteUserResponse,
    TenantUser,
    TenantUsersResponse,
    UserRole,
    UpdateUserRoleRequest,
    UpdateUserRoleResponse
} from '../models/user-management.models';

@Injectable({
    providedIn: 'root'
})
export class UsersService {
    private readonly usersUrl = this.normalizeApiUrl(environment.apiUrl);

    constructor(private readonly http: HttpClient) { }

    getTenantUsers(): Observable<TenantUser[]> {
        return this.http.get<TenantUsersResponse>(`${this.usersUrl}/users`).pipe(
            map((response) => (response.data ?? [])
                .map((user) => this.normalizeTenantUser(user))
                .filter((user): user is TenantUser => Boolean(user))
            )
        );
    }

    inviteUser(payload: InviteUserRequest): Observable<InviteUserResponse> {
        return this.http.post<InviteUserResponse>(`${this.usersUrl}/users/invite`, payload);
    }

    updateUserRole(userId: string, payload: UpdateUserRoleRequest): Observable<UpdateUserRoleResponse> {
        return this.http.put<UpdateUserRoleResponse>(`${this.usersUrl}/users/${userId}/role`, payload);
    }

    deleteUser(userId: string): Observable<DeleteUserResponse> {
        return this.http.delete<DeleteUserResponse>(`${this.usersUrl}/users/${userId}`);
    }

    private normalizeApiUrl(rawUrl: string): string {
        const trimmed = rawUrl.replace(/\/+$/, '');
        return trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
    }

    private normalizeTenantUser(rawUser: Partial<TenantUser> & { id?: string; userId?: string }): TenantUser | null {
        const userId = rawUser.userId ?? rawUser.id;
        if (!userId) {
            console.error('Invalid user object missing userId.', rawUser);
            return null;
        }

        return {
            userId,
            fullName: rawUser.fullName ?? '',
            email: rawUser.email ?? '',
            role: (rawUser.role ?? 'Employee') as UserRole
        };
    }
}
