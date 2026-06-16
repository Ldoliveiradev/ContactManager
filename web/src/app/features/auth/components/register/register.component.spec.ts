import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { Register } from './register.component';

describe('Register', () => {
  let fixture: ComponentFixture<Register>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { register: () => of({}), login: () => of({}) } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Register);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
