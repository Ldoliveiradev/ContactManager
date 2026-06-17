import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ToastComponent } from './toast.component';
import { ToastService } from '../../../core/services/toast.service';

describe('ToastComponent', () => {
  let fixture: ComponentFixture<ToastComponent>;
  let service: ToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastComponent);
    service = TestBed.inject(ToastService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render a toast when service shows one', () => {
    // Arrange / Act
    service.show('Contact created successfully.', 'success');
    fixture.detectChanges();

    // Assert
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.toast--success')?.textContent).toContain('Contact created successfully.');
  });

  it('should render an error toast', () => {
    // Arrange / Act
    service.show('Something went wrong.', 'error');
    fixture.detectChanges();

    // Assert
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.toast--error')?.textContent).toContain('Something went wrong.');
  });

  it('should dismiss a toast when close button is clicked', () => {
    // Arrange
    service.show('Hello', 'success');
    fixture.detectChanges();

    // Act
    const closeBtn: HTMLButtonElement = fixture.nativeElement.querySelector('.toast__close');
    closeBtn.click();
    fixture.detectChanges();

    // Assert
    expect(fixture.nativeElement.querySelectorAll('.toast').length).toBe(0);
  });

  it('should auto-dismiss after duration', fakeAsync(() => {
    // Arrange / Act
    service.show('Auto gone', 'success', 1000);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.toast').length).toBe(1);

    // Assert
    tick(1000);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.toast').length).toBe(0);
  }));
});
