export interface Contact {
  id: string;
  name: string;
  email: string;
  phone: string | null;
}

export interface ContactInput {
  name: string;
  email: string;
  phone: string | null;
}
