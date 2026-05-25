import { ApiResponseDto, PaginatedResponseDto } from '../../../shared/models/api.models';

export interface DepartmentListItemResponseDto {
  departmentId: string;
  name: string;
  description: string | null;
  createdAt: string;
}

export interface CreateDepartmentRequestDto {
  Name: string;
  Description?: string | null;
}

export interface CreateDepartmentResponseDto {
  DepartmentId: string;
  Name: string;
  Description: string | null;
  CreatedAt: string;
}

export interface UpdateDepartmentRequestDto {
  DepartmentId: string;
  Name: string;
  Description?: string | null;
}

export interface UpdateDepartmentResponseDto {
  DepartmentId: string;
  Name: string;
  Description: string | null;
  UpdatedAt: string;
}

export interface DeleteDepartmentResponseDto {
  DepartmentId: string;
}

export type DepartmentListResponseDto = ApiResponseDto<
  PaginatedResponseDto<DepartmentListItemResponseDto>
>;

export type CreateDepartmentResponse = ApiResponseDto<CreateDepartmentResponseDto>;
export type UpdateDepartmentResponse = ApiResponseDto<UpdateDepartmentResponseDto>;
export type DeleteDepartmentResponse = ApiResponseDto<DeleteDepartmentResponseDto>;

export interface Department {
  departmentId: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface DepartmentsQueryParams {
  pageNumber?: number;
  pageSize?: number;
}
