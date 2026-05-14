import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, filter, finalize, map, shareReplay, switchMap, take, tap } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  AuthResponse,
  ChangePasswordRequest,
  ChangePasswordResponse,
  LoginRequest,
  RefreshResponse,
  RegisterRequest,
  UserProfile,
  UserProfileResponse
} from '../models/auth.models';

interface TokenClaims {
  role?: string;
  tenantId?: string;
  tenant_id?: string;
  permissions?: string[] | string;
  perms?: string[] | string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey = 'erp_auth_token';
  private readonly refreshTokenKey = 'erp_refresh_token';
  private readonly authBaseUrl = `${environment.apiUrl.replace(/\/+$/, '')}/auth`;
  private readonly profileUrl = `${environment.apiUrl.replace(/\/+$/, '')}/users/profile`;
  private readonly authStateSubject = new BehaviorSubject<boolean>(false);
  private readonly currentUserSubject = new BehaviorSubject<UserProfile | null>(null);
  private readonly authInitializingSubject = new BehaviorSubject<boolean>(true);
  private readonly authReadySubject = new BehaviorSubject<boolean>(false);
  private readonly refreshInProgressSubject = new BehaviorSubject<boolean>(false);
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private profileRequest$: Observable<UserProfile | null> | null = null;
  private refreshRequest$: Observable<string | null> | null = null;
  private tokensRestored = false;

  readonly isAuthenticated$ = this.authStateSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isInitializingAuth$ = this.authInitializingSubject.asObservable();
  readonly authReady$ = this.authReadySubject.asObservable();
  readonly isRefreshingToken$ = this.refreshInProgressSubject.asObservable();
  readonly role$ = this.currentUser$.pipe(
    map((user) => user?.role ?? this.getRoleFromToken() ?? null),
    distinctUntilChanged()
  );
  readonly tenantId$ = this.currentUser$.pipe(
    map((user) => user?.tenantId ?? this.getTenantIdFromToken()),
    distinctUntilChanged()
  );
  readonly permissions$ = this.currentUser$.pipe(
    map((user) => user?.permissions ?? this.getPermissionsFromToken()),
    distinctUntilChanged((previous, next) => previous.join('|') === next.join('|'))
  );

  constructor(private readonly http: HttpClient) { }

  initializeSession(): void {
    this.authInitializingSubject.next(true);
    this.restoreTokens();
    const isAuthenticated = Boolean(this.accessToken);
    this.authStateSubject.next(isAuthenticated);
    if (isAuthenticated) {
      this.loadCurrentUser(true).pipe(
        finalize(() => this.markAuthReady())
      ).subscribe();
      return;
    }
    this.markAuthReady();
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.authBaseUrl}/login`, payload).pipe(
      tap((response) => {
        const token = response.data?.accessToken ?? null;
        const refreshToken = response.data?.refreshToken ?? null;
        if (!token) {
          return;
        }

        this.setTokens(token, refreshToken);
      }),
      switchMap((response) =>
        this.loadCurrentUser(true).pipe(map(() => response))
      )
    );
  }

  register(payload: RegisterRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.authBaseUrl}/register`, payload);
  }

  changePassword(payload: ChangePasswordRequest): Observable<ChangePasswordResponse> {
    return this.http.put<ChangePasswordResponse>(`${this.authBaseUrl}/change-password`, payload);
  }

  loadCurrentUser(force = false): Observable<UserProfile | null> {
    if (!this.accessToken) {
      return of(null);
    }

    if (!force && this.currentUserSubject.value) {
      return of(this.currentUserSubject.value);
    }

    if (this.profileRequest$) {
      return this.profileRequest$;
    }

    this.profileRequest$ = this.http.get<UserProfileResponse>(this.profileUrl).pipe(
      map((response) => this.enrichUserWithToken(response.data ?? null)),
      tap((user) => this.currentUserSubject.next(user)),
      catchError(() => {
        this.currentUserSubject.next(null);
        return of(null);
      }),
      finalize(() => {
        this.profileRequest$ = null;
      }),
      shareReplay(1)
    );

    return this.profileRequest$;
  }

  refreshAccessToken(): Observable<string | null> {
    if (this.refreshRequest$) {
      return this.refreshRequest$;
    }

    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of(null);
    }

    this.refreshInProgressSubject.next(true);
    this.refreshRequest$ = this.http.post<RefreshResponse>(`${this.authBaseUrl}/refresh-token`, { refreshToken }).pipe(
      tap((response) => {
        const token = response.data?.accessToken ?? null;
        if (!token) {
          return;
        }
        const newRefreshToken = response.data?.refreshToken ?? refreshToken;
        this.setTokens(token, newRefreshToken);
      }),
      map((response) => response.data?.accessToken ?? null),
      catchError(() => of(null)),
      finalize(() => {
        this.refreshRequest$ = null;
        this.refreshInProgressSubject.next(false);
      }),
      shareReplay(1)
    );

    return this.refreshRequest$;
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.clearSession();
      return of(void 0);
    }

    return this.http.post<ApiResponse<unknown>>(`${this.authBaseUrl}/logout`, { refreshToken }).pipe(
      catchError(() => of(null)),
      finalize(() => {
        this.clearSession();
      }),
      map(() => void 0)
    );
  }

  forceLogout(): void {
    this.clearSession();
  }

  isAuthenticated(): boolean {
    return this.authStateSubject.value;
  }

  isInitializingAuth(): boolean {
    return this.authInitializingSubject.value;
  }

  isAuthReady(): boolean {
    return this.authReadySubject.value;
  }

  isRefreshingToken(): boolean {
    return this.refreshInProgressSubject.value;
  }

  waitForAuthReady(): Observable<boolean> {
    return this.authReady$.pipe(
      filter(Boolean),
      take(1)
    );
  }

  getCurrentUserSnapshot(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  getToken(): string | null {
    this.restoreTokens();
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    this.restoreTokens();
    return this.refreshToken;
  }

  hasPermission(permission: string | string[], user: UserProfile | null = this.currentUserSubject.value): boolean {
    if (this.isAdmin(user)) {
      return true;
    }
    const permissions = this.getPermissions(user);
    if (!permissions.length) {
      return false;
    }
    if (Array.isArray(permission)) {
      return permission.every((value) => permissions.includes(value));
    }
    return permissions.includes(permission);
  }

  isAdmin(user: UserProfile | null = this.currentUserSubject.value): boolean {
    const role = user?.role ?? this.getRoleFromToken();
    return (role ?? '').toLowerCase() === 'admin';
  }

  private hasToken(): boolean {
    return Boolean(this.getToken());
  }

  private setTokens(accessToken: string, refreshToken: string | null): void {
    this.accessToken = accessToken;
    if (refreshToken) {
      this.refreshToken = refreshToken;
    }
    if (!this.isStorageAvailable()) {
      return;
    }
    localStorage.setItem(this.tokenKey, accessToken);
    if (refreshToken) {
      localStorage.setItem(this.refreshTokenKey, refreshToken);
    }
    this.authStateSubject.next(true);
  }

  private clearSession(): void {
    this.accessToken = null;
    this.refreshToken = null;
    this.tokensRestored = true;
    this.authStateSubject.next(false);
    this.currentUserSubject.next(null);
    this.authInitializingSubject.next(false);
    this.authReadySubject.next(true);
    if (!this.isStorageAvailable()) {
      return;
    }
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }

  private markAuthReady(): void {
    this.authInitializingSubject.next(false);
    this.authReadySubject.next(true);
  }

  private restoreTokens(): void {
    if (this.tokensRestored || !this.isStorageAvailable()) {
      return;
    }
    this.tokensRestored = true;
    this.accessToken = localStorage.getItem(this.tokenKey);
    this.refreshToken = localStorage.getItem(this.refreshTokenKey);
  }

  private enrichUserWithToken(user: UserProfile | null): UserProfile | null {
    if (!user) {
      return null;
    }

    const claims = this.getTokenClaims();
    if (!claims) {
      return user;
    }

    const permissions = user.permissions?.length
      ? user.permissions
      : this.normalizePermissions(claims.permissions ?? claims.perms);

    return {
      ...user,
      role: user.role ?? claims.role,
      tenantId: user.tenantId ?? claims.tenantId ?? claims.tenant_id,
      permissions
    };
  }

  private getPermissions(user: UserProfile | null): string[] {
    if (!user) {
      return this.getPermissionsFromToken();
    }

    return user.permissions?.length ? user.permissions : this.getPermissionsFromToken();
  }

  private getPermissionsFromToken(): string[] {
    const claims = this.getTokenClaims();
    return this.normalizePermissions(claims?.permissions ?? claims?.perms);
  }

  private getRoleFromToken(): string | undefined {
    return this.getTokenClaims()?.role;
  }

  private getTenantIdFromToken(): string | null {
    const claims = this.getTokenClaims();
    return claims?.tenantId ?? claims?.tenant_id ?? null;
  }

  private normalizePermissions(value?: string[] | string | null): string[] {
    if (!value) {
      return [];
    }
    if (Array.isArray(value)) {
      return value.filter((permission) => permission.trim().length > 0);
    }
    return value
      .split(',')
      .map((permission) => permission.trim())
      .filter((permission) => permission.length > 0);
  }

  private getTokenClaims(): TokenClaims | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }
    return this.decodeTokenPayload<TokenClaims>(token);
  }

  private decodeTokenPayload<T>(token: string): T | null {
    const parts = token.split('.');
    if (parts.length < 2) {
      return null;
    }

    const rawPayload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const paddedPayload = rawPayload + '==='.slice((rawPayload.length + 3) % 4);
    try {
      const decoded = typeof atob === 'function' ? atob(paddedPayload) : null;
      if (!decoded) {
        return null;
      }
      return JSON.parse(decoded) as T;
    } catch {
      return null;
    }
  }

  private isStorageAvailable(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
