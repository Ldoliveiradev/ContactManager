import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ContactForm } from './contact-form.component';

describe('ContactForm', () => {
  let fixture: ComponentFixture<ContactForm>;
  let component: ContactForm;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows "New contact" heading when not editing', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('New contact');
  });

  it('form is invalid when empty', () => {
    expect(component['form'].invalid).toBeTrue();
  });

  it('form is valid with name and email', () => {
    component['form'].setValue({ name: 'Jane Doe', email: 'jane@example.com', phone: '' });
    expect(component['form'].valid).toBeTrue();
  });

  it('marks touched on invalid submit', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(component['form'].controls.name.touched).toBeTrue();
  });
});
