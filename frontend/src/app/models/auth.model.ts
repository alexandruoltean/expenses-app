export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  username: string;
  userId: string;
  expiresAt: string;
}

export interface User {
  id: string;
  email: string;
  username: string;
}