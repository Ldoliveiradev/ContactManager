import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { ContactFormPageComponent } from './contact-form-page.component';

describe('ContactFormPageComponent', () => {
  let fixture: ComponentFixture<ContactFormPageComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactFormPageComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '123' }) } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactFormPageComponent);
    router = TestBed.inject(Router);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('reads edit id from route params', () => {
    expect(fixture.componentInstance['editId']).toBe('123');
  });

  it('navigates back to contacts on done', () => {
    const navigateSpy = spyOn(router, 'navigate');

    fixture.componentInstance['onDone']();

    expect(navigateSpy).toHaveBeenCalledWith(['/contacts']);
  });
});
