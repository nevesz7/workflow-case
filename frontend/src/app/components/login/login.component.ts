import { Component } from '@angular/core';
import { LoginRequest } from '../../models/api.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  credentials: LoginRequest = {
    username: '',
    password: ''
  };
  errorMessage: string = '';

  constructor(private authService: AuthService) {}

  login(): void {
    if (!this.credentials.username || !this.credentials.password) {
      this.errorMessage = 'Username and password are required.';
      return;
    }
    
    this.authService.login(this.credentials).subscribe({
      error: (err) => {
        console.error('Login failed', err);
        if (err.status === 0) {
          this.errorMessage = 'Connection refused. Is the backend running? Check API URL.';
        } else if (err.status === 401) {
          this.errorMessage = 'Invalid username or password.';
        } else {
          this.errorMessage = 'Login failed. Please try again later.';
        }
      }
    });
  }
}