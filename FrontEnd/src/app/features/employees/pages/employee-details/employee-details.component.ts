import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

import { EmployeesApiService } from '../../services/employees.api.service';
import { EmployeeDetailsResponseDto } from '../../models/employees.models';

@Component({
  selector: 'app-employee-details',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './employee-details.component.html',
  styleUrl: './employee-details.component.scss'
})
export class EmployeeDetailsComponent implements OnInit {
  employee: EmployeeDetailsResponseDto | null = null;
  loading = false;
  errorMessage = '';

  constructor(
    private readonly employeesApi: EmployeesApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const employeeId = this.route.snapshot.paramMap.get('employeeId');
    if (!employeeId) {
      this.router.navigate(['/employees']);
      return;
    }
    this.loadEmployee(employeeId);
  }

  private loadEmployee(employeeId: string): void {
    if (this.loading) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.employeesApi.getById(employeeId).pipe(
      finalize(() => {
        this.loading = false;
      })
    ).subscribe({
      next: (response) => {
        if (!response.success) {
          this.errorMessage = response.message || 'Unable to load employee details.';
          return;
        }
        this.employee = response.data ?? null;
      },
      error: (error: unknown) => {
        this.errorMessage = this.getErrorMessage(error, 'Unable to load employee details.');
        if (this.errorMessage.toLowerCase().includes('not found')) {
          this.toastr.error(this.errorMessage);
        }
      }
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error && typeof error === 'object') {
      const responseError = error as { error?: { Message?: string; message?: string } };
      const message = responseError.error?.Message ?? responseError.error?.message;
      if (typeof message === 'string' && message.trim().length > 0) {
        return message;
      }

      const maybeMessage = (error as { Message?: string }).Message;
      if (typeof maybeMessage === 'string' && maybeMessage.trim().length > 0) {
        return maybeMessage;
      }
    }

    return fallback;
  }
}
