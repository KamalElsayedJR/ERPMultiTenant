import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl.replace(/\/+$/, '');

  constructor(private readonly http: HttpClient) { }

  get<T>(path: string) {
    const normalizedPath = path.replace(/^\/+/, '');
    return this.http.get<T>(`${this.baseUrl}/${normalizedPath}`);
  }
}
