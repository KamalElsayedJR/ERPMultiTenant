import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HealthResponse, HealthService } from '../../../../core/services/health.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  totalUsers = 128;
  totalTenants = 12;

  apiLoading = false;
  apiStatus = 'offline';
  apiMessage = '';
  apiError = '';

  constructor(private readonly healthService: HealthService) { }

  ngOnInit(): void {
    this.loadApiHealth();
  }

  private loadApiHealth(): void {
    this.apiLoading = true;
    this.apiError = '';

    this.healthService.getHealth().subscribe({
      next: (response) => {
        this.apiLoading = false;
        this.apiStatus = 'online';
        this.apiMessage = this.extractMessage(response) || 'ERP API Running';
      },
      error: () => {
        this.apiLoading = false;
        this.apiStatus = 'offline';
        this.apiError = 'Unable to reach the ERP API.';
      }
    });
  }

  private extractMessage(response: HealthResponse): string {
    if (typeof response === 'string') {
      return response;
    }

    return response?.message ?? '';
  }
}
