import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { LoginRequest, RegisterRequest, AuthResponse, User } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5050/api/auth';
  private tokenKey = 'auth_token';
  private userKey = 'current_user';
  
  private currentUserSubject = new BehaviorSubject<User | null>(this.getCurrentUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient) {
    this.checkTokenExpiration();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(error => {
          console.error('Login error:', error);
          throw error;
        })
      );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(error => {
          console.error('Registration error:', error);
          throw error;
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    
    const user: User = {
      id: response.userId,
      email: response.email,
      username: response.username
    };
    
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private getCurrentUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.userKey);
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch (error) {
        console.error('Error parsing user from storage:', error);
        localStorage.removeItem(this.userKey);
      }
    }
    return null;
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expirationTime = payload.exp * 1000;
      const currentTime = Date.now();
      
      if (currentTime >= expirationTime) {
        this.logout();
        return false;
      }
      
      return true;
    } catch (error) {
      console.error('Error validating token:', error);
      this.logout();
      return false;
    }
  }

  private checkTokenExpiration(): void {
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const expirationTime = payload.exp * 1000;
        const currentTime = Date.now();
        const timeUntilExpiration = expirationTime - currentTime;

        if (timeUntilExpiration > 0) {
          setTimeout(() => {
            this.logout();
            console.log('Token expired, user logged out');
          }, timeUntilExpiration);
        }
      } catch (error) {
        console.error('Error checking token expiration:', error);
        this.logout();
      }
    }
  }
}