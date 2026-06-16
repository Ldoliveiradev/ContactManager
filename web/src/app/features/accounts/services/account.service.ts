import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AccountDto } from '../models/account-dto.interface';
import { AccountResponse } from '../models/account-response.interface';
import { UpdatePasswordRequest } from '../models/update-password-request.interface';
import { UpdateProfileRequest } from '../models/update-profile-request.interface';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/accounts`;

  getProfile(): Observable<AccountDto | null> {
    return this.http
      .get<AccountResponse>(this.baseUrl)
      .pipe(map((res) => res.data));
  }

  updateProfile(request: UpdateProfileRequest): Observable<AccountDto | null> {
    return this.http
      .put<AccountResponse>(this.baseUrl, request)
      .pipe(map((res) => res.data));
  }

  updatePassword(request: UpdatePasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/password`, request);
  }
}
