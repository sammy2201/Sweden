import { Injectable, signal } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import type {
  AuthResponse,
  LoginPayload,
  RegisterPayload,
  UpdateUserProfilePayload,
  UserProfile,
} from "../models/auth.models";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly accessTokenStorageKey = "sweden-start-token";
  private logoutTimeoutId: number | null = null;

  readonly isAuthenticated = signal(false);
  readonly user = signal<UserProfile | null>(null);

  constructor() {
    this.restoreAuthenticatedSession();
  }

  async login(loginPayload: LoginPayload): Promise<AuthResponse> {
    try {
      const response = await axios.post<AuthResponse>(
        `${environment.apiUrl}/auth/login`,
        loginPayload,
        this.requestConfig(),
      );
      this.storeAuthenticatedSession(response.data);
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async register(registrationPayload: RegisterPayload): Promise<void> {
    try {
      await axios.post<void>(
        `${environment.apiUrl}/auth/register`,
        registrationPayload,
        this.requestConfig(),
      );
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async loadUserProfile(): Promise<UserProfile> {
    try {
      const response = await axios.get<UserProfile>(
        `${environment.apiUrl}/auth/user`,
        this.requestConfig(),
      );
      this.user.set(response.data);
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  updateUserProfile(
    profileUpdatePayload: UpdateUserProfilePayload,
  ): Promise<UserProfile> {
    return axios
      .put<UserProfile>(
        `${environment.apiUrl}/auth/user`,
        profileUpdatePayload,
        this.requestConfig(),
      )
      .then((response) => {
        this.user.set(response.data);
        return response.data;
      })
      .catch((error: unknown) => this.handleUnauthorized(error));
  }

  logout(): void {
    this.clearLogoutTimeout();
    localStorage.removeItem(this.accessTokenStorageKey);
    this.isAuthenticated.set(false);
    this.user.set(null);
  }

  getValidAccessToken(): string | null {
    const accessToken = localStorage.getItem(this.accessTokenStorageKey);

    if (!accessToken) {
      this.isAuthenticated.set(false);
      return null;
    }

    const accessTokenExpirationTime =
      this.getAccessTokenExpirationTime(accessToken);

    if (
      accessTokenExpirationTime === null ||
      Date.now() >= accessTokenExpirationTime
    ) {
      this.logout();
      return null;
    }

    this.isAuthenticated.set(true);
    this.scheduleAutomaticLogout(accessTokenExpirationTime);

    return accessToken;
  }

  private storeAuthenticatedSession(
    authenticationResponse: AuthResponse,
  ): void {
    const accessToken = authenticationResponse.accessToken;

    localStorage.setItem(this.accessTokenStorageKey, accessToken);
    this.isAuthenticated.set(true);

    const accessTokenExpirationTime =
      this.getAccessTokenExpirationTime(accessToken);

    if (accessTokenExpirationTime !== null) {
      this.scheduleAutomaticLogout(accessTokenExpirationTime);
    } else {
      this.logout();
    }
  }

  private requestConfig() {
    const token = this.getValidAccessToken();
    return token ? { headers: { Authorization: `Bearer ${token}` } } : {};
  }

  private handleUnauthorized(error: unknown): never {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      this.logout();
    }
    throw error;
  }

  private restoreAuthenticatedSession(): void {
    this.getValidAccessToken();
  }

  private scheduleAutomaticLogout(accessTokenExpirationTime: number): void {
    this.clearLogoutTimeout();

    const millisecondsUntilTokenExpiration =
      accessTokenExpirationTime - Date.now();

    if (millisecondsUntilTokenExpiration <= 0) {
      this.logout();
      return;
    }

    this.logoutTimeoutId = window.setTimeout(() => {
      this.logout();
    }, millisecondsUntilTokenExpiration);
  }

  private clearLogoutTimeout(): void {
    if (this.logoutTimeoutId !== null) {
      window.clearTimeout(this.logoutTimeoutId);
      this.logoutTimeoutId = null;
    }
  }

  private getAccessTokenExpirationTime(accessToken: string): number | null {
    try {
      const accessTokenParts = accessToken.split(".");

      if (accessTokenParts.length !== 3) {
        return null;
      }

      const encodedTokenPayload = accessTokenParts[1];
      const decodedTokenPayload = this.decodeBase64Url(encodedTokenPayload);

      const tokenPayload = JSON.parse(decodedTokenPayload) as {
        exp?: number;
      };

      if (typeof tokenPayload.exp !== "number") {
        return null;
      }

      // JWT expiration is expressed in seconds; JavaScript timestamps use milliseconds.
      return tokenPayload.exp * 1000;
    } catch {
      return null;
    }
  }

  private decodeBase64Url(encodedValue: string): string {
    const base64Value = encodedValue.replace(/-/g, "+").replace(/_/g, "/");

    const paddedBase64Value = base64Value.padEnd(
      Math.ceil(base64Value.length / 4) * 4,
      "=",
    );

    return atob(paddedBase64Value);
  }
}
