import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  CreateDepartmentRequestDto,
  CreateDepartmentResponse,
  DeleteDepartmentResponse,
  DepartmentListResponseDto,
  DepartmentsQueryParams,
  UpdateDepartmentRequestDto,
  UpdateDepartmentResponse
} from '../models/departments.models';

@Injectable({
  providedIn: 'root'
})
export class DepartmentsApiService {
  private readonly baseUrl = this.normalizeApiUrl(environment.apiUrl);
  private readonly departmentsUrl = `${this.baseUrl}/departments`;

  constructor(private readonly http: HttpClient) {}

  getAll(params?: DepartmentsQueryParams): Observable<DepartmentListResponseDto> {
    const httpParams = this.buildQueryParams(params);
    return this.http.get<DepartmentListResponseDto>(this.departmentsUrl, { params: httpParams });
  }

  create(payload: CreateDepartmentRequestDto): Observable<CreateDepartmentResponse> {
    return this.http.post<CreateDepartmentResponse>(this.departmentsUrl, payload);
  }

  update(departmentId: string, payload: UpdateDepartmentRequestDto): Observable<UpdateDepartmentResponse> {
    return this.http.put<UpdateDepartmentResponse>(`${this.departmentsUrl}/${departmentId}`, payload);
  }

  delete(departmentId: string): Observable<DeleteDepartmentResponse> {
    return this.http.delete<DeleteDepartmentResponse>(`${this.departmentsUrl}/${departmentId}`);
  }

  private normalizeApiUrl(rawUrl: string): string {
    const trimmed = rawUrl.replace(/\/+$/, '');
    return trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
  }

  private buildQueryParams(params?: DepartmentsQueryParams): HttpParams {
    let httpParams = new HttpParams();
    if (!params) {
      return httpParams;
    }

    if (params.pageNumber !== undefined) {
      httpParams = httpParams.set('pageNumber', String(params.pageNumber));
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }

    return httpParams;
  }
}
