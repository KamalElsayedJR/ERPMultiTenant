import { ApiResponseDto, PaginatedResponseDto } from '../../../shared/models/api.models';

export type EmployeeSortBy = 'employeeNumber' | 'name' | 'hireDate' | 'salary' | 'department';
export type SortDirection = 'asc' | 'desc';

export interface EmployeeListItemResponseDto {
  employeeId: string;
  employeeNumber: string;
  fullName: string;
  email: string;
  departmentId: string;
  departmentName: string;
  jobTitle: string | null;
  hireDate: string;
  salary: number;
}

export interface EmployeeDetailsResponseDto {
  EmployeeId: string;
  EmployeeNumber: string;
  FullName: string;
  Email: string;
  DepartmentId: string;
  DepartmentName: string;
  ApplicationUserId: string;
  JobTitle: string | null;
  HireDate: string;
  Salary: number;
  CreatedAt: string;
  UpdatedAt: string | null;
}

export interface CreateEmployeeRequestDto {
  JobTitle?: string | null;
  HireDate: string;
  Salary: number;
  DepartmentId: string;
  ApplicationUserId: string;
}

export interface CreateEmployeeResponseDto {
  EmployeeId: string;
  EmployeeNumber: string;
  DepartmentId: string;
  ApplicationUserId: string;
  JobTitle: string | null;
  HireDate: string;
  Salary: number;
  CreatedAt: string;
}

export interface UpdateEmployeeRequestDto {
  EmployeeId: string;
  JobTitle?: string | null;
  HireDate: string;
  Salary: number;
  DepartmentId: string;
  ApplicationUserId: string;
}

export interface UpdateEmployeeResponseDto {
  EmployeeId: string;
  EmployeeNumber: string;
  DepartmentId: string;
  ApplicationUserId: string;
  JobTitle: string | null;
  HireDate: string;
  Salary: number;
  UpdatedAt: string;
}

export interface DeleteEmployeeResponseDto {
  EmployeeId: string;
}

export type EmployeeListResponseDto = ApiResponseDto<
  PaginatedResponseDto<EmployeeListItemResponseDto>
>;

export type EmployeeDetailsResponse = ApiResponseDto<EmployeeDetailsResponseDto>;
export type CreateEmployeeResponse = ApiResponseDto<CreateEmployeeResponseDto>;
export type UpdateEmployeeResponse = ApiResponseDto<UpdateEmployeeResponseDto>;
export type DeleteEmployeeResponse = ApiResponseDto<DeleteEmployeeResponseDto>;

export interface Employee {
  employeeId: string;
  employeeNumber: string;
  fullName: string;
  email: string;
  departmentId: string;
  departmentName: string;
  applicationUserId: string;
  jobTitle: string | null;
  hireDate: string;
  salary: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface EmployeesQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  departmentId?: string;
  minSalary?: number;
  maxSalary?: number;
  sortBy?: EmployeeSortBy;
  sortDirection?: SortDirection;
}
