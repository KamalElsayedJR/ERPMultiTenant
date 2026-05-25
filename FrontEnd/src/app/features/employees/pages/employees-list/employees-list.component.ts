import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { finalize, map, startWith } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { UsersService } from '../../../../core/services/users.service';
import { TenantUser } from '../../../../core/models/user-management.models';
import { DepartmentsApiService } from '../../../departments/services/departments.api.service';
import { DepartmentListItemResponseDto } from '../../../departments/models/departments.models';
import { EmployeesApiService } from '../../services/employees.api.service';
import {
  CreateEmployeeRequestDto,
  EmployeeListItemResponseDto,
  EmployeeSortBy,
  EmployeesQueryParams,
  SortDirection,
  UpdateEmployeeRequestDto
} from '../../models/employees.models';

interface EmployeeListItemView {
  employeeId: string;
  employeeNumber: string;
  fullName: string;
  email: string;
  departmentId: string;
  departmentName: string;
  jobTitle: string | null;
  hireDate: string;
  salary: number;
  status: string;
}

@Component({
  selector: 'app-employees-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './employees-list.component.html',
  styleUrl: './employees-list.component.scss'
})
export class EmployeesListComponent implements OnInit {
  private readonly duplicateUserMessage = 'application user is already linked to an employee.';

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    departmentId: [''],
    minSalary: [''],
    maxSalary: [''],
    sortBy: ['employeeNumber' as EmployeeSortBy],
    sortDirection: ['asc' as SortDirection]
  }, { validators: [this.salaryRangeValidator] });

  readonly employeeForm = this.fb.nonNullable.group({
    applicationUserId: ['', [Validators.required, (control: AbstractControl) => this.userSelectionValidator(control)]],
    departmentId: ['', [Validators.required]],
    jobTitle: ['', [Validators.maxLength(150)]],
    hireDate: ['', [Validators.required]],
    salary: [0, [Validators.required, Validators.min(0)]]
  });

  readonly canManageEmployees$ = this.authService.currentUser$.pipe(
    map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageEmployees', user))
  );

  employees: EmployeeListItemView[] = [];
  departments: DepartmentListItemResponseDto[] = [];
  users: TenantUser[] = [];
  filteredUsers: TenantUser[] = [];
  usersLoading = false;
  employeesLoading = false;
  employeesError = '';
  departmentsLoading = false;
  totalCount = 0;
  totalPages = 1;
  pageNumber = 1;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];

  modalOpen = false;
  deleteOpen = false;
  editingEmployee: EmployeeListItemView | null = null;
  deletingEmployee: EmployeeListItemView | null = null;
  saveLoading = false;
  deleteLoading = false;

  constructor(
    private readonly employeesApi: EmployeesApiService,
    private readonly departmentsApi: DepartmentsApiService,
    private readonly authService: AuthService,
    private readonly usersService: UsersService,
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadEmployees();
    this.loadUsers();
    this.setupUserFilter();
  }

  get applicationUserIdControl() {
    return this.employeeForm.controls.applicationUserId;
  }

  get departmentIdControl() {
    return this.employeeForm.controls.departmentId;
  }

  get jobTitleControl() {
    return this.employeeForm.controls.jobTitle;
  }

  get hireDateControl() {
    return this.employeeForm.controls.hireDate;
  }

  get salaryControl() {
    return this.employeeForm.controls.salary;
  }

  get salaryRangeInvalid(): boolean {
    return this.filterForm.hasError('salaryRangeInvalid');
  }

  openCreate(): void {
    if (!this.canManageEmployees()) {
      this.toastr.error('You do not have permission to create employees.');
      return;
    }
    this.editingEmployee = null;
    this.employeeForm.reset({
      applicationUserId: '',
      departmentId: '',
      jobTitle: '',
      hireDate: '',
      salary: 0
    });
    this.modalOpen = true;
  }

  openEdit(employee: EmployeeListItemView): void {
    if (!this.canManageEmployees()) {
      this.toastr.error('You do not have permission to edit employees.');
      return;
    }
    this.editingEmployee = employee;
    this.employeeForm.reset({
      applicationUserId: '',
      departmentId: employee.departmentId,
      jobTitle: employee.jobTitle ?? '',
      hireDate: employee.hireDate,
      salary: employee.salary
    });
    this.modalOpen = true;
  }

  closeModal(): void {
    this.modalOpen = false;
    this.editingEmployee = null;
    this.employeeForm.reset({
      applicationUserId: '',
      departmentId: '',
      jobTitle: '',
      hireDate: '',
      salary: 0
    });
    this.clearDuplicateErrors();
  }

  openDelete(employee: EmployeeListItemView): void {
    if (!this.canManageEmployees()) {
      this.toastr.error('You do not have permission to delete employees.');
      return;
    }
    this.deletingEmployee = employee;
    this.deleteOpen = true;
  }

  closeDelete(): void {
    this.deleteOpen = false;
    this.deletingEmployee = null;
  }

  applyFilters(): void {
    if (this.filterForm.invalid) {
      return;
    }
    this.pageNumber = 1;
    this.loadEmployees();
  }

  resetFilters(): void {
    this.filterForm.reset({
      searchTerm: '',
      departmentId: '',
      minSalary: '',
      maxSalary: '',
      sortBy: 'employeeNumber',
      sortDirection: 'asc'
    });
    this.pageNumber = 1;
    this.loadEmployees();
  }

  changePageSize(): void {
    this.pageNumber = 1;
    this.loadEmployees();
  }

  goToPreviousPage(): void {
    if (this.pageNumber <= 1) {
      return;
    }
    this.pageNumber -= 1;
    this.loadEmployees();
  }

  goToNextPage(): void {
    if (this.pageNumber >= this.totalPages) {
      return;
    }
    this.pageNumber += 1;
    this.loadEmployees();
  }

  openDetails(employee: EmployeeListItemView): void {
    this.router.navigate(['/employees', employee.employeeId]);
  }

  submitForm(): void {
    if (!this.canManageEmployees()) {
      this.toastr.error('You do not have permission to update employees.');
      return;
    }
    if (this.employeeForm.invalid || this.saveLoading) {
      return;
    }

    this.saveLoading = true;
    this.clearDuplicateErrors();

    const payload: CreateEmployeeRequestDto = {
      JobTitle: this.normalizeOptionalText(this.jobTitleControl.value),
      HireDate: this.hireDateControl.value,
      Salary: Number(this.salaryControl.value),
      DepartmentId: this.departmentIdControl.value,
      ApplicationUserId: this.applicationUserIdControl.value.trim()
    };

    const request$ = (this.editingEmployee
      ? this.submitUpdate(payload, this.editingEmployee)
      : this.submitCreate(payload)) as Observable<unknown>;

    request$.pipe(
      finalize(() => {
        this.saveLoading = false;
      })
    ).subscribe({
      next: () => {
        this.closeModal();
        this.loadEmployees(true);
      },
      error: (error: unknown) => {
        const message = this.getErrorMessage(error, 'Unable to save employee.');
        if (this.isDuplicateUserMessage(message)) {
          this.applyApplicationUserExistsError();
        }
        this.toastr.error(message);
      }
    });
  }

  confirmDelete(): void {
    if (!this.canManageEmployees()) {
      this.toastr.error('You do not have permission to delete employees.');
      return;
    }
    if (!this.deletingEmployee || this.deleteLoading) {
      return;
    }

    this.deleteLoading = true;

    this.employeesApi.delete(this.deletingEmployee.employeeId).pipe(
      finalize(() => {
        this.deleteLoading = false;
      })
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success('Employee deleted.');
          this.closeDelete();
          this.loadEmployees(true);
          return;
        }

        const message = response.message || 'Unable to delete employee.';
        this.toastr.error(message);
      },
      error: (error: unknown) => {
        const message = this.getErrorMessage(error, 'Unable to delete employee.');
        this.toastr.error(message);
      }
    });
  }

  loadEmployees(syncPage = false): void {
    if (this.employeesLoading) {
      return;
    }

    this.employeesLoading = true;
    this.employeesError = '';

    const filters = this.filterForm.getRawValue();
    const params: EmployeesQueryParams = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: this.normalizeOptionalText(filters.searchTerm),
      departmentId: filters.departmentId || undefined,
      minSalary: this.parseOptionalNumber(filters.minSalary),
      maxSalary: this.parseOptionalNumber(filters.maxSalary),
      sortBy: filters.sortBy,
      sortDirection: filters.sortDirection
    };

    this.employeesApi.getAll(params).pipe(
      finalize(() => {
        this.employeesLoading = false;
      })
    ).subscribe({
      next: (response) => {
        if (!response.success) {
          this.employeesError = response.message || 'Unable to load employees.';
          this.employees = [];
          return;
        }

        const items = response.data?.items ?? [];
        this.employees = items.map((item) => this.mapEmployee(item));
        this.totalCount = response.data?.totalCount ?? items.length;
        this.totalPages = response.data?.totalPages ?? 1;
        if (syncPage && this.pageNumber > this.totalPages) {
          this.pageNumber = this.totalPages;
          this.employeesLoading = false;
          this.loadEmployees();
        }
      },
      error: (error: unknown) => {
        this.employeesError = this.getErrorMessage(error, 'Unable to load employees.');
      }
    });
  }

  loadDepartments(): void {
    if (this.departmentsLoading) {
      return;
    }

    this.departmentsLoading = true;

    this.departmentsApi.getAll({ pageNumber: 1, pageSize: 200 }).pipe(
      finalize(() => {
        this.departmentsLoading = false;
      })
    ).subscribe({
      next: (response) => {
        this.departments = response.data?.items ?? [];
      },
      error: () => {
        this.departments = [];
      }
    });
  }

  trackByEmployeeId(_index: number, employee: EmployeeListItemView): string {
    return employee.employeeId;
  }

  clearDuplicateErrors(): void {
    if (this.applicationUserIdControl.hasError('applicationUserExists')) {
      const errors = { ...(this.applicationUserIdControl.errors ?? {}) } as Record<string, boolean>;
      delete errors['applicationUserExists'];
      this.applicationUserIdControl.setErrors(Object.keys(errors).length ? errors : null);
    }
  }

  private submitCreate(payload: CreateEmployeeRequestDto) {
    return this.employeesApi.create(payload).pipe(
      map((response) => {
        if (!response.success) {
          throw response;
        }
        this.toastr.success('Employee created.');
        return response;
      })
    );
  }

  private submitUpdate(payload: CreateEmployeeRequestDto, employee: EmployeeListItemView) {
    const updatePayload: UpdateEmployeeRequestDto = {
      EmployeeId: employee.employeeId,
      JobTitle: payload.JobTitle,
      HireDate: payload.HireDate,
      Salary: payload.Salary,
      DepartmentId: payload.DepartmentId,
      ApplicationUserId: payload.ApplicationUserId
    };

    return this.employeesApi.update(employee.employeeId, updatePayload).pipe(
      map((response) => {
        if (!response.success) {
          throw response;
        }
        this.toastr.success('Employee updated.');
        return response;
      })
    );
  }

  private applyApplicationUserExistsError(): void {
    const errors = { ...(this.applicationUserIdControl.errors ?? {}) } as Record<string, boolean>;
    errors['applicationUserExists'] = true;
    this.applicationUserIdControl.setErrors(errors);
  }

  private canManageEmployees(): boolean {
    return this.authService.isAdmin() || this.authService.hasPermission('ManageEmployees');
  }

  private isDuplicateUserMessage(message: string): boolean {
    return message.trim().toLowerCase() === this.duplicateUserMessage;
  }

  private loadUsers(): void {
    if (this.usersLoading) {
      return;
    }

    this.usersLoading = true;

    this.usersService.getTenantUsers().pipe(
      finalize(() => {
        this.usersLoading = false;
      })
    ).subscribe({
      next: (users) => {
        this.users = users;
        this.filteredUsers = this.filterUsers(this.applicationUserIdControl.value);
        this.applicationUserIdControl.updateValueAndValidity({ emitEvent: false });
      },
      error: () => {
        this.users = [];
        this.filteredUsers = [];
      }
    });
  }

  private setupUserFilter(): void {
    this.applicationUserIdControl.valueChanges.pipe(
      startWith(this.applicationUserIdControl.value)
    ).subscribe((value) => {
      this.filteredUsers = this.filterUsers(value);
    });
  }

  displayUserLabel(userId: string): string {
    if (!userId) {
      return '';
    }
    const match = this.users.find((user) => user.userId === userId);
    return match ? this.formatUserLabel(match) : userId;
  }

  private filterUsers(value: string | null): TenantUser[] {
    if (!this.users.length) {
      return [];
    }
    const normalized = this.normalizeUserSearch(value ?? '');
    if (!normalized) {
      return [...this.users];
    }
    return this.users.filter((user) => {
      const label = this.formatUserLabel(user).toLowerCase();
      return label.includes(normalized);
    });
  }

  private normalizeUserSearch(value: string): string {
    const trimmed = value.trim();
    if (!trimmed) {
      return '';
    }
    const match = this.users.find((user) => user.userId === trimmed);
    return (match ? this.formatUserLabel(match) : trimmed).toLowerCase();
  }

  private formatUserLabel(user: TenantUser): string {
    const fullName = user.fullName?.trim() ?? '';
    const email = user.email?.trim() ?? '';
    if (fullName && email) {
      return `${fullName} (${email})`;
    }
    return fullName || email || user.userId;
  }

  private userSelectionValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value as string;
    if (!value) {
      return null;
    }
    return this.users.some((user) => user.userId === value) ? null : { userSelectionInvalid: true };
  }

  private normalizeOptionalText(value: string): string | undefined {
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : undefined;
  }

  private parseOptionalNumber(value: string): number | undefined {
    if (value === '' || value === null || value === undefined) {
      return undefined;
    }
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : undefined;
  }

  private mapEmployee(item: EmployeeListItemResponseDto): EmployeeListItemView {
    return {
      employeeId: item.employeeId,
      employeeNumber: item.employeeNumber,
      fullName: item.fullName,
      email: item.email,
      departmentId: item.departmentId,
      departmentName: item.departmentName,
      jobTitle: item.jobTitle ?? null,
      hireDate: item.hireDate,
      salary: item.salary,
      status: 'Active'
    };
  }

  private salaryRangeValidator(control: AbstractControl): ValidationErrors | null {
    const group = control as { get: (key: string) => AbstractControl | null };
    const minSalaryRaw = group.get('minSalary')?.value;
    const maxSalaryRaw = group.get('maxSalary')?.value;
    if (minSalaryRaw === '' || maxSalaryRaw === '') {
      return null;
    }
    const minSalary = Number(minSalaryRaw);
    const maxSalary = Number(maxSalaryRaw);
    if (Number.isNaN(minSalary) || Number.isNaN(maxSalary)) {
      return null;
    }
    return maxSalary >= minSalary ? null : { salaryRangeInvalid: true };
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
