import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ContactDto } from '../../models/contact-dto.interface';
import { ContactList } from './contact-list.component';

const mockContacts: ContactDto[] = [
  { id: '1', name: 'Jane Doe', email: 'jane@example.com', phone: null },
];

describe('ContactList', () => {
  let fixture: ComponentFixture<ContactList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactList],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactList);
    fixture.componentRef.setInput('items', mockContacts);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders contacts from the items input', () => {
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Jane Doe');
  });

  it('emits editContact with the contact id', () => {
    let editedId: string | undefined;
    fixture.componentInstance.editContact.subscribe((id) => (editedId = id));
    fixture.detectChanges();

    const editBtn = fixture.nativeElement.querySelector('[aria-label^="Edit"]') as HTMLButtonElement;
    editBtn.click();

    expect(editedId).toBe('1');
  });
});
