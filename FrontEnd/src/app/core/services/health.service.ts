import { Injectable } from '@angular/core';

import { ApiService } from './api.service';

export type HealthResponse = string | { message?: string };

@Injectable({
  providedIn: 'root'
})
export class HealthService {
  constructor(private readonly api: ApiService) { }

  getHealth() {
    return this.api.get<HealthResponse>('health');
  }
}
