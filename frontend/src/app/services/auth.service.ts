import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";
import { environment } from "../../environments/environment";

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  city: string;
  address: string;
  password: string;
}

export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  city: string;
  address: string;
}

interface AuthResponse {
  accessToken: string;
  expiresAt: string;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly storageKey = "sweden-start-token";
  private logoutTimerId: number | null = null;
  readonly isAuthenticated = signal(false);
  readonly user = signal<UserProfile | null>(null);

  constructor(private readonly http: HttpClient) {
    this.restoreSession();
  }

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, payload)
      .pipe(tap((response) => this.storeSession(response)));
  }

  register(payload: RegisterPayload): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/register`, payload);
  }

  loadUserProfile(): Observable<UserProfile> {
    return this.http
      .get<UserProfile>(`${environment.apiUrl}/auth/user`)
      .pipe(tap((profile) => this.user.set(profile)));
  }

  logout(): void {
    this.clearLogoutTimer();
    localStorage.removeItem(this.storageKey);
    this.isAuthenticated.set(false);
    this.user.set(null);
  }

  getValidToken(): string | null {
    const token = localStorage.getItem(this.storageKey);
    if (!token) {
      this.isAuthenticated.set(false);
      return null;
    }

    const expiresAtMs = this.getTokenExpiryMs(token);
    if (!expiresAtMs || Date.now() >= expiresAtMs) {
      this.logout();
      return null;
    }

    this.isAuthenticated.set(true);
    this.scheduleAutoLogout(expiresAtMs);
    return token;
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(this.storageKey, response.accessToken);
    this.isAuthenticated.set(true);
    const expiresAtMs = this.getTokenExpiryMs(response.accessToken);
    if (expiresAtMs) {
      this.scheduleAutoLogout(expiresAtMs);
    }
  }

  private restoreSession(): void {
    this.getValidToken();
  }

  private scheduleAutoLogout(expiresAtMs: number): void {
    this.clearLogoutTimer();

    const msUntilExpiry = expiresAtMs - Date.now();
    if (msUntilExpiry <= 0) {
      this.logout();
      return;
    }

    this.logoutTimerId = window.setTimeout(() => {
      this.logout();
    }, msUntilExpiry);
  }

  private clearLogoutTimer(): void {
    if (this.logoutTimerId !== null) {
      window.clearTimeout(this.logoutTimerId);
      this.logoutTimerId = null;
    }
  }

  private getTokenExpiryMs(token: string): number | null {
    try {
      const tokenParts = token.split(".");
      if (tokenParts.length !== 3) {
        return null;
      }

      const payloadJson = this.decodeBase64Url(tokenParts[1]);
      const payload = JSON.parse(payloadJson) as { exp?: number };
      if (typeof payload.exp !== "number") {
        return null;
      }

      return payload.exp * 1000;
    } catch {
      return null;
    }
  }

  private decodeBase64Url(value: string): string {
    const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
    return atob(padded);
  }
}
