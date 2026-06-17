import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SkeletonComponent } from './skeleton.component';

describe('SkeletonComponent', () => {
  let fixture: ComponentFixture<SkeletonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkeletonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SkeletonComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should default to line variant', () => {
    // Assert
    expect(fixture.componentInstance.variant()).toBe('line');
  });

  it('should render a span with the correct variant class', () => {
    // Arrange / Act
    fixture.componentRef.setInput('variant', 'circle');
    fixture.detectChanges();

    // Assert
    const span: HTMLElement = fixture.nativeElement.querySelector('.skeleton');
    expect(span.classList).toContain('skeleton--circle');
  });

  it('should apply width and height styles to the inner span', () => {
    // Arrange / Act
    fixture.componentRef.setInput('variant', 'rect');
    fixture.componentRef.setInput('width', '120px');
    fixture.componentRef.setInput('height', '40px');
    fixture.detectChanges();

    // Assert
    const span: HTMLElement = fixture.nativeElement.querySelector('.skeleton');
    expect(span.style.width).toBe('120px');
    expect(span.style.height).toBe('40px');
  });

  it('should render line variant by default', () => {
    // Assert
    const span: HTMLElement = fixture.nativeElement.querySelector('.skeleton');
    expect(span.classList).toContain('skeleton--line');
  });
});
