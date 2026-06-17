import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Login } from './login.component';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders the sign-in heading', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Contact Manager');
    expect(compiled.querySelector('.auth-card__subtitle')?.textContent).toContain('Sign in');
  });

  it('only shows username and password fields', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[formControlName="username"]')).toBeTruthy();
    expect(compiled.querySelector('[formControlName="password"]')).toBeTruthy();
    expect(compiled.querySelector('[formControlName="firstName"]')).toBeNull();
  });

  it('marks form invalid on empty submit', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(component['form'].invalid).toBeTrue();
  });

  it('emits switchToRegister when the register link is clicked', () => {
    let switched = false;
    component.switchToRegister.subscribe(() => (switched = true));

    const ghostBtn = fixture.nativeElement.querySelectorAll('button')[1] as HTMLButtonElement;
    ghostBtn.click();

    expect(switched).toBeTrue();
  });
});
