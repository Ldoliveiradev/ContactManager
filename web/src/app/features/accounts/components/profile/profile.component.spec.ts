import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { Profile } from './profile';

const mockAccount = {
  id: '1', username: 'demo', firstName: 'Jane', lastName: 'Doe', email: 'jane@example.com',
};

describe('Profile', () => {
  let fixture: ComponentFixture<Profile>;
  let component: Profile;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Profile],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Profile);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/accounts`).flush({ isSuccess: true, error: null, data: mockAccount });
  });

  it('populates profile form from loaded account', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/accounts`).flush({ isSuccess: true, error: null, data: mockAccount });
    fixture.detectChanges();

    expect(component['profileForm'].value.firstName).toBe('Jane');
    expect(component['profileForm'].value.email).toBe('jane@example.com');
  });

  it('shows username on profile page', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/accounts`).flush({ isSuccess: true, error: null, data: mockAccount });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('@demo');
  });

  it('password form is invalid when empty', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/accounts`).flush({ isSuccess: true, error: null, data: mockAccount });

    expect(component['passwordForm'].invalid).toBeTrue();
  });
});
