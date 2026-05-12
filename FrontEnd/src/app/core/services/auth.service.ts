import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, finalize, map, shareReplay, switchMap, tap } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, AuthResponse, LoginRequest, RefreshResponse, RegisterRequest, UserProfile, UserProfileResponse } from '../models/auth.models';

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
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private profileRequest$: Observable<UserProfile | null> | null = null;
  private refreshRequest$: Observable<string | null> | null = null;
  private tokensRestored = false;

  readonly isAuthenticated$ = this.authStateSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly role$ = this.currentUser$.pipe(
    map((user) => user?.role ?? null),
    distinctUntilChanged()
  );

  constructor(private readonly http: HttpClient) { }

  initializeSession(): void {
    this.restoreTokens();
    const isAuthenticated = Boolean(this.accessToken);
    this.authStateSubject.next(isAuthenticated);
    if (isAuthenticated) {
      this.loadCurrentUser();
    }
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
      map((response) => response.data ?? null),
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

  getToken(): string | null {
    this.restoreTokens();
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    this.restoreTokens();
    return this.refreshToken;
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
    if (!this.isStorageAvailable()) {
      return;
    }
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }

  private restoreTokens(): void {
    if (this.tokensRestored || !this.isStorageAvailable()) {
      return;
    }
    this.tokensRestored = true;
    this.accessToken = localStorage.getItem(this.tokenKey);
    this.refreshToken = localStorage.getItem(this.refreshTokenKey);
  }

  private isStorageAvailable(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
