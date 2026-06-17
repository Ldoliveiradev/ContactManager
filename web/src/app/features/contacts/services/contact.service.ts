import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginationFilter } from '../../../core/interfaces/pagination-filter.interface';
import { PaginationResponse } from '../../../core/interfaces/pagination-response.interface';
import { ContactDto } from '../models/contact-dto.interface';
import { ContactResponse } from '../models/contact-response.interface';
import { CreateContactRequest } from '../models/create-contact-request.interface';
import { UpdateContactRequest } from '../models/update-contact-request.interface';

@Injectable({ providedIn: 'root' })
export class ContactService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/contacts`;

  getAll(filter: PaginationFilter): Observable<PaginationResponse<ContactDto[]>> {
    let params = new HttpParams()
      .set('Page', filter.page ?? 1)
      .set('PageSize', filter.pageSize ?? 10);
    if (filter.search?.trim()) params = params.set('Search', filter.search.trim());
    if (filter.sortBy?.trim()) params = params.set('SortBy', filter.sortBy.trim());
    if (typeof filter.sortDesc === 'boolean') params = params.set('SortDesc', filter.sortDesc);
    return this.http.get<PaginationResponse<ContactDto[]>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<ContactResponse> {
    return this.http.get<ContactResponse>(`${this.baseUrl}/${id}`);
  }

  create(input: CreateContactRequest): Observable<ContactResponse> {
    return this.http.post<ContactResponse>(this.baseUrl, input);
  }

  update(id: string, input: UpdateContactRequest): Observable<ContactResponse> {
    return this.http.put<ContactResponse>(`${this.baseUrl}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
