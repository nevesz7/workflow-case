import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { AuthService, User } from '../../services/auth.service';
import { RequestResponseDto } from '../../models/api.models';

@Component({
  selector: 'app-request-list',
  templateUrl: './request-list.component.html',
  styleUrls: ['./request-list.component.css']
})
export class RequestListComponent implements OnInit {
  requests: RequestResponseDto[] = [];
  statusFilter: string = '';
  currentUser: User | null = null;
  isLoading = false;

  constructor(
    private apiService: ApiService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;
    // If statusFilter is empty string, pass undefined to get all
    const status = this.statusFilter || undefined;
    
    this.apiService.getRequests(status).subscribe({
      next: (data) => {
        this.requests = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading requests', err);
        this.isLoading = false;
      }
    });
  }

  onFilterChange(): void {
    this.loadRequests();
  }
  
  getStatusClass(status: string): string {
    switch (status) {
      case 'Approved': return 'status-approved';
      case 'Rejected': return 'status-rejected';
      case 'Pending': return 'status-pending';
      default: return '';
    }
  }
}