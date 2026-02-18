import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode'; // npm install jwt-decode
import { ApiService } from './api.service';
import { LoginRequest, LoginResponse } from '../models/api.models';

// This interface represents the decoded user data from the JWT
export interface User {
  userId: string;
  name: string;
  role: 'User' | 'Manager';
  exp: number; // Expiration time (Unix timestamp)
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {
    this.loadUserFromToken();
  }

  private loadUserFromToken(): void {
    const token = this.getToken();
    if (token) {
      try {
        const decodedUser = jwtDecode<User>(token);
        // Check if token is expired
        if (decodedUser.exp * 1000 > Date.now()) {
          this.currentUserSubject.next(decodedUser);
        } else {
          this.logout(); // Token is expired, clear session
        }
      } catch (error) {
        this.logout(); // Invalid token
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.apiService.login(credentials).pipe(
      tap(response => {
        localStorage.setItem('authToken', response.token);
        const decodedUser = jwtDecode<User>(response.token);
        this.currentUserSubject.next(decodedUser);
        this.router.navigate(['/requests']);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  public getToken(): string | null {
    return localStorage.getItem('authToken');
  }
}