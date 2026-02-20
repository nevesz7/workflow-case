import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { CreateRequestDto } from '../../models/api.models';

@Component({
  selector: 'app-new-request',
  templateUrl: './new-request.component.html',
  styleUrls: ['./new-request.component.css']
})
export class NewRequestComponent {
  request: CreateRequestDto = {
    title: '',
    description: '',
    category: 'TI',
    priority: 'Medium'
  };
  errorMessage: string = '';
  isLoading: boolean = false;

  categories = ['TI', 'Compras', 'Reembolso', 'Outros'];
  priorities = ['Low', 'Medium', 'High'];

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {}

  onSubmit(): void {
    if (!this.request.title || !this.request.description) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isLoading = true;
    this.apiService.createRequest(this.request).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/requests']);
      },
      error: (err) => {
        console.error('Error creating request', err);
        this.errorMessage = 'Failed to create request. Please try again.';
        this.isLoading = false;
      }
    });
  }
}