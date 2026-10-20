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
  age?: number | null;
}

export interface UpdateUserProfilePayload {
  firstName: string;
  lastName: string;
  city: string;
  address: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
}
