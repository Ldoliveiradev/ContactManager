import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginationResponse } from '../../../core/interfaces/pagination-response.interface';
import { ContactDto } from '../models/contact-dto.interface';
import { ContactListResponse } from '../models/contact-list-response.interface';
import { ContactResponse } from '../models/contact-response.interface';
import { CreateContactRequest } from '../models/create-contact-request.interface';
import { UpdateContactRequest } from '../models/update-contact-request.interface';

@Injectable({ providedIn: 'root' })
export class ContactService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/contacts`;

  getAll(): Observable<ContactDto[]> {
    // The DataView does search/sort/pagination client-side, so fetch the full
    // book of business in one request. The API paginates server-side (default
    // PageSize=10), which would otherwise silently drop contacts past page 1.
    const params = new HttpParams().set('Page', 1).set('PageSize', 1000);
    return this.http
      .get<PaginationResponse<ContactListResponse>>(this.baseUrl, { params })
      .pipe(map((res) => res.data?.data ?? []));
  }

  getById(id: string): Observable<ContactDto | null> {
    return this.http
      .get<ContactResponse>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data));
  }

  create(input: CreateContactRequest): Observable<ContactDto | null> {
    return this.http
      .post<ContactResponse>(this.baseUrl, input)
      .pipe(map((res) => res.data));
  }

  update(id: string, input: UpdateContactRequest): Observable<ContactDto | null> {
    return this.http
      .put<ContactResponse>(`${this.baseUrl}/${id}`, input)
      .pipe(map((res) => res.data));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
