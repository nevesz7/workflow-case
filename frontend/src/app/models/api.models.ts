export interface LoginRequest {
  username: string;
  password?: string;
}

export interface LoginResponse {
  token: string;
  username:string;
  role: 'User' | 'Manager';
  id: string;
}

export interface CreateRequestDto {
  title: string;
  description: string;
  category: string;
  priority: 'Low' | 'Medium' | 'High';
}

export interface RequestResponseDto {
  id: string;
  title: string;
  description: string;
  category: string;
  priority: 'Low' | 'Medium' | 'High';
  status: 'Pending' | 'Approved' | 'Rejected';
  userId: string;
  createdAt: string; // ISO date string
  updatedAt: string; // ISO date string
}

export interface DecisionDto {
  comment?: string;
}

export interface RequestHistoryDto {
  id: string;
  fromStatus: string | null;
  toStatus: string;
  changedByName: string;
  changedAt: string; // ISO date string
  comment: string | null;
}