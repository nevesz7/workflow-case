import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LoginRequest,
  LoginResponse,
  CreateRequestDto,
  RequestResponseDto,
  RequestHistoryDto,
  DecisionDto
} from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  // Adjust this URL to your backend's address
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // --- Auth ---
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials);
  }

  // --- Requests ---
  getRequests(status?: string): Observable<RequestResponseDto[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<RequestResponseDto[]>(`${this.apiUrl}/requests`, { params });
  }

  getRequestById(id: string): Observable<RequestResponseDto> {
    return this.http.get<RequestResponseDto>(`${this.apiUrl}/requests/${id}`);
  }

  createRequest(data: CreateRequestDto): Observable<RequestResponseDto> {
    return this.http.post<RequestResponseDto>(`${this.apiUrl}/requests`, data);
  }

  approveRequest(id: string, comment?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/requests/${id}/approve`, { comment } as DecisionDto);
  }

  rejectRequest(id: string, comment: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/requests/${id}/reject`, { comment } as DecisionDto);
  }

  getRequestHistory(id: string): Observable<RequestHistoryDto[]> {
    return this.http.get<RequestHistoryDto[]>(`${this.apiUrl}/requests/${id}/history`);
  }
}
