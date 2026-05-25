export interface ApiResponseDto<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface PaginatedResponseDto<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type ApiErrorResponseDto = ApiResponseDto<string>;
