import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  CreateEmployeeRequestDto,
  CreateEmployeeResponse,
  DeleteEmployeeResponse,
  EmployeeDetailsResponse,
  EmployeeListResponseDto,
  EmployeesQueryParams,
  UpdateEmployeeRequestDto,
  UpdateEmployeeResponse
} from '../models/employees.models';

@Injectable({
  providedIn: 'root'
})
export class EmployeesApiService {
  private readonly baseUrl = this.normalizeApiUrl(environment.apiUrl);
  private readonly employeesUrl = `${this.baseUrl}/employees`;

  constructor(private readonly http: HttpClient) {}

  getAll(params?: EmployeesQueryParams): Observable<EmployeeListResponseDto> {
    const httpParams = this.buildQueryParams(params);
    return this.http.get<EmployeeListResponseDto>(this.employeesUrl, { params: httpParams });
  }

  getById(employeeId: string): Observable<EmployeeDetailsResponse> {
    return this.http.get<EmployeeDetailsResponse>(`${this.employeesUrl}/${employeeId}`);
  }

  create(payload: CreateEmployeeRequestDto): Observable<CreateEmployeeResponse> {
    return this.http.post<CreateEmployeeResponse>(this.employeesUrl, payload);
  }

  update(employeeId: string, payload: UpdateEmployeeRequestDto): Observable<UpdateEmployeeResponse> {
    return this.http.put<UpdateEmployeeResponse>(`${this.employeesUrl}/${employeeId}`, payload);
  }

  delete(employeeId: string): Observable<DeleteEmployeeResponse> {
    return this.http.delete<DeleteEmployeeResponse>(`${this.employeesUrl}/${employeeId}`);
  }

  private normalizeApiUrl(rawUrl: string): string {
    const trimmed = rawUrl.replace(/\/+$/, '');
    return trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
  }

  private buildQueryParams(params?: EmployeesQueryParams): HttpParams {
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
    if (params.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params.departmentId) {
      httpParams = httpParams.set('departmentId', params.departmentId);
    }
    if (params.minSalary !== undefined) {
      httpParams = httpParams.set('minSalary', String(params.minSalary));
    }
    if (params.maxSalary !== undefined) {
      httpParams = httpParams.set('maxSalary', String(params.maxSalary));
    }
    if (params.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }
    if (params.sortDirection) {
      httpParams = httpParams.set('sortDirection', params.sortDirection);
    }

    return httpParams;
  }
}
