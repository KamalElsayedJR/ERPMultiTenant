import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { finalize, map } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { DepartmentsApiService } from '../../services/departments.api.service';
import {
  CreateDepartmentRequestDto,
  Department,
  DepartmentListItemResponseDto,
  DepartmentsQueryParams,
  UpdateDepartmentRequestDto
} from '../../models/departments.models';

@Component({
  selector: 'app-departments-list',
  standalone: true,
  imports: [
    CommonModule,
    AsyncPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './departments-list.component.html',
  styleUrl: './departments-list.component.scss'
})
export class DepartmentsListComponent implements OnInit {
  private readonly duplicateNameMessage = 'department name already exists.';

  readonly departmentForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly canManageDepartments$ = this.authService.currentUser$.pipe(
    map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageDepartments', user))
  );

  departments: Department[] = [];
  departmentsLoading = false;
  departmentsError = '';
  totalCount = 0;
  totalPages = 1;
  pageNumber = 1;
  pageSize = 25;

  modalOpen = false;
  deleteOpen = false;
  editingDepartment: Department | null = null;
  deletingDepartment: Department | null = null;
  saveLoading = false;
  deleteLoading = false;

  constructor(
    private readonly departmentsApi: DepartmentsApiService,
    private readonly authService: AuthService,
    private readonly fb: FormBuilder,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
  }

  get nameControl() {
    return this.departmentForm.controls.name;
  }

  get descriptionControl() {
    return this.departmentForm.controls.description;
  }

  openCreate(): void {
    if (!this.canManageDepartments()) {
      this.toastr.error('You do not have permission to create departments.');
      return;
    }
    this.editingDepartment = null;
    this.departmentForm.reset({ name: '', description: '' });
    this.modalOpen = true;
  }

  openEdit(department: Department): void {
    if (!this.canManageDepartments()) {
      this.toastr.error('You do not have permission to edit departments.');
      return;
    }
    this.editingDepartment = department;
    this.departmentForm.reset({
      name: department.name,
      description: department.description ?? ''
    });
    this.modalOpen = true;
  }

  closeModal(): void {
    this.modalOpen = false;
    this.editingDepartment = null;
    this.departmentForm.reset({ name: '', description: '' });
  }

  openDelete(department: Department): void {
    if (!this.canManageDepartments()) {
      this.toastr.error('You do not have permission to delete departments.');
      return;
    }
    this.deletingDepartment = department;
    this.deleteOpen = true;
  }

  closeDelete(): void {
    this.deleteOpen = false;
    this.deletingDepartment = null;
  }

  submitForm(): void {
    if (!this.canManageDepartments()) {
      this.toastr.error('You do not have permission to update departments.');
      return;
    }
    if (this.departmentForm.invalid || this.saveLoading) {
      return;
    }

    this.saveLoading = true;
    this.clearDuplicateNameError();

    const payload: CreateDepartmentRequestDto = {
      Name: this.nameControl.value.trim(),
      Description: this.normalizeOptionalText(this.descriptionControl.value)
    };

    const request$ = (this.editingDepartment
      ? this.submitUpdate(payload, this.editingDepartment)
      : this.submitCreate(payload)) as Observable<unknown>;

    request$.pipe(
      finalize(() => {
        this.saveLoading = false;
      })
    ).subscribe({
      next: () => {
        this.closeModal();
        this.loadDepartments(true);
      },
      error: (error: unknown) => {
        const message = this.getErrorMessage(error, 'Unable to save department.');
        if (this.isDuplicateNameMessage(message)) {
          this.applyDuplicateNameError();
        }
        this.toastr.error(message);
      }
    });
  }

  confirmDelete(): void {
    if (!this.canManageDepartments()) {
      this.toastr.error('You do not have permission to delete departments.');
      return;
    }
    if (!this.deletingDepartment || this.deleteLoading) {
      return;
    }

    this.deleteLoading = true;

    this.departmentsApi.delete(this.deletingDepartment.departmentId).pipe(
      finalize(() => {
        this.deleteLoading = false;
      })
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success('Department deleted.');
          this.closeDelete();
          this.loadDepartments(true);
          return;
        }

        const message = response.message || 'Unable to delete department.';
        this.toastr.error(message);
      },
      error: (error: unknown) => {
        const message = this.getErrorMessage(error, 'Unable to delete department.');
        this.toastr.error(message);
      }
    });
  }

  loadDepartments(syncPage = false): void {
    if (this.departmentsLoading) {
      return;
    }

    this.departmentsLoading = true;
    this.departmentsError = '';

    const params: DepartmentsQueryParams = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.departmentsApi.getAll(params).pipe(
      finalize(() => {
        this.departmentsLoading = false;
      })
    ).subscribe({
      next: (response) => {
        if (!response.success) {
          this.departmentsError = response.message || 'Unable to load departments.';
          this.departments = [];
          return;
        }

        const items = response.data?.items ?? [];
        this.departments = items.map((item) => this.mapDepartment(item));
        this.totalCount = response.data?.totalCount ?? items.length;
        this.totalPages = response.data?.totalPages ?? 1;
        if (syncPage && this.pageNumber > this.totalPages) {
          this.pageNumber = this.totalPages;
          this.departmentsLoading = false;
          this.loadDepartments();
        }
      },
      error: (error: unknown) => {
        this.departmentsError = this.getErrorMessage(error, 'Unable to load departments.');
      }
    });
  }

  trackByDepartmentId(_index: number, department: Department): string {
    return department.departmentId;
  }

  clearDuplicateNameError(): void {
    if (!this.nameControl.hasError('nameExists')) {
      return;
    }

    const errors = { ...(this.nameControl.errors ?? {}) } as Record<string, boolean>;
    delete errors['nameExists'];
    this.nameControl.setErrors(Object.keys(errors).length ? errors : null);
  }

  private submitCreate(payload: CreateDepartmentRequestDto) {
    return this.departmentsApi.create(payload).pipe(
      map((response) => {
        if (!response.success) {
          throw response;
        }
        this.toastr.success('Department created.');
        return response;
      })
    );
  }

  private submitUpdate(payload: CreateDepartmentRequestDto, department: Department) {
    const updatePayload: UpdateDepartmentRequestDto = {
      DepartmentId: department.departmentId,
      Name: payload.Name,
      Description: payload.Description
    };

    return this.departmentsApi.update(department.departmentId, updatePayload).pipe(
      map((response) => {
        if (!response.success) {
          throw response;
        }
        this.toastr.success('Department updated.');
        return response;
      })
    );
  }

  private applyDuplicateNameError(): void {
    const errors = { ...(this.nameControl.errors ?? {}) } as Record<string, boolean>;
    errors['nameExists'] = true;
    this.nameControl.setErrors(errors);
  }

  private canManageDepartments(): boolean {
    return this.authService.isAdmin() || this.authService.hasPermission('ManageDepartments');
  }

  private isDuplicateNameMessage(message: string): boolean {
    return message.trim().toLowerCase() === this.duplicateNameMessage;
  }

  private normalizeOptionalText(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  }

  private mapDepartment(item: DepartmentListItemResponseDto): Department {
    return {
      departmentId: item.departmentId,
      name: item.name,
      description: item.description ?? null,
      createdAt: item.createdAt,
      updatedAt: null
    };
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
