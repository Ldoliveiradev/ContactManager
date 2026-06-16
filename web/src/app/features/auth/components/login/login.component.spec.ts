import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Login } from './login';

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

  it('starts in login mode', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Contact Manager');
    expect(compiled.querySelector('[aria-label="First name"]')).toBeNull();
  });

  it('shows register fields after toggle', () => {
    const toggleBtn = fixture.nativeElement.querySelectorAll('button')[1] as HTMLButtonElement;
    toggleBtn.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[aria-label="First name"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[aria-label="Last name"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[aria-label="Email"]')).toBeTruthy();
  });

  it('marks form invalid on empty submit', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(component['form'].invalid).toBeTrue();
  });
});
