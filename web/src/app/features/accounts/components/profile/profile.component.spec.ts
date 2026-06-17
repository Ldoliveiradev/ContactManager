import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AccountDto } from '../../models/account-dto.interface';
import { Profile } from './profile.component';

const mockAccount: AccountDto = {
  id: '1', firstName: 'Jane', lastName: 'Doe', email: 'jane@example.com',
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
    fixture.componentRef.setInput('account', mockAccount);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('populates profile form from the account input', () => {
    fixture.detectChanges();

    expect(component['profileForm'].value.firstName).toBe('Jane');
    expect(component['profileForm'].value.email).toBe('jane@example.com');
  });

  it('password form is invalid when empty', () => {
    fixture.detectChanges();

    expect(component['passwordForm'].invalid).toBeTrue();
  });
});
