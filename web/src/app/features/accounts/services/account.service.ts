import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AccountResponse } from '../models/account-response.interface';
import { UpdateProfileRequest } from '../models/update-profile-request.interface';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/accounts`;

  getProfile(): Observable<AccountResponse> {
    return this.http.get<AccountResponse>(this.baseUrl);
  }

  updateProfile(request: UpdateProfileRequest): Observable<AccountResponse> {
    return this.http.put<AccountResponse>(this.baseUrl, request);
  }
}
