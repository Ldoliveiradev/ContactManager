import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { AccountDto } from '../models/account-dto.interface';
import { AccountResponse } from '../models/account-response.interface';
import { AccountService } from './account.service';

const baseUrl = `${environment.apiUrl}/accounts`;

const mockAccount: AccountDto = {
  id: '1', firstName: 'Jane', lastName: 'Doe', email: 'jane@example.com',
};

describe('AccountService', () => {
  let service: AccountService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AccountService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getProfile returns AccountResponse', () => {
    let result: AccountResponse | undefined;
    service.getProfile().subscribe((a) => (result = a));

    const req = http.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush({ isSuccess: true, error: null, data: mockAccount });

    expect(result?.data).toEqual(mockAccount);
  });

  it('updateProfile sends PUT and returns AccountResponse', () => {
    const input = { firstName: 'Jane', lastName: 'Smith', email: 'jane@example.com' };
    let result: AccountResponse | undefined;
    service.updateProfile(input).subscribe((a) => (result = a));

    const req = http.expectOne(baseUrl);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(input);
    req.flush({ isSuccess: true, error: null, data: { ...mockAccount, lastName: 'Smith' } });

    expect(result?.data?.lastName).toBe('Smith');
  });


});
