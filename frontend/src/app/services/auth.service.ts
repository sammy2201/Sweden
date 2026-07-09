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
  password: string;
}

interface AuthResponse {
  accessToken: string;
  expiresAt: string;
}

interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly storageKey = "sweden-start-token";
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

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.isAuthenticated.set(false);
    this.user.set(null);
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(this.storageKey, response.accessToken);
    this.isAuthenticated.set(true);
  }

  private restoreSession(): void {
    const token = localStorage.getItem(this.storageKey);
    if (!token) {
      this.isAuthenticated.set(false);
      return;
    }

    this.isAuthenticated.set(true);
  }
}
