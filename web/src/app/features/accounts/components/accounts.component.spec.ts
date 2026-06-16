import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { AccountsComponent } from './accounts.component';

const mockAccount = {
  id: '1', username: 'demo', firstName: 'Jane', lastName: 'Doe', email: 'jane@example.com',
};

describe('AccountsComponent', () => {
  let fixture: ComponentFixture<AccountsComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(AccountsComponent);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/accounts`).flush({ isSuccess: true, error: null, data: mockAccount });
    expect(fixture.componentInstance).toBeTruthy();
  });
});
