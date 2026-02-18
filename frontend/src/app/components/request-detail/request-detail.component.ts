import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService, User } from '../../services/auth.service';
import { RequestResponseDto, RequestHistoryDto } from '../../models/api.models';

@Component({
  selector: 'app-request-detail',
  templateUrl: './request-detail.component.html',
  styleUrls: ['./request-detail.component.css']
})
export class RequestDetailComponent implements OnInit {
  request: RequestResponseDto | null = null;
  history: RequestHistoryDto[] = [];
  currentUser: User | null = null;
  isLoading = true;
  comment: string = '';
  actionError: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => this.currentUser = user);
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadData(id);
    } else {
      this.router.navigate(['/requests']);
    }
  }

  loadData(id: string): void {
    this.isLoading = true;
    this.apiService.getRequestById(id).subscribe({
      next: (req) => {
        this.request = req;
        this.loadHistory(id);
      },
      error: (err) => {
        console.error('Error loading request', err);
        this.isLoading = false;
      }
    });
  }

  loadHistory(id: string): void {
    this.apiService.getRequestHistory(id).subscribe({
      next: (hist) => {
        this.history = hist;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading history', err);
        this.isLoading = false;
      }
    });
  }

  approve(): void {
    if (!this.request) return;
    this.apiService.approveRequest(this.request.id, this.comment).subscribe({
      next: () => this.reload(),
      error: (err) => this.actionError = 'Failed to approve request.'
    });
  }

  reject(): void {
    if (!this.request) return;
    if (!this.comment) {
      this.actionError = 'Comment is required to reject.';
      return;
    }
    this.apiService.rejectRequest(this.request.id, this.comment).subscribe({
      next: () => this.reload(),
      error: (err) => this.actionError = 'Failed to reject request.'
    });
  }

  reload(): void {
    this.comment = '';
    this.actionError = '';
    if (this.request) this.loadData(this.request.id);
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