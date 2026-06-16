export interface Credentials {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface RegisterResponse {
  id: string;
  username: string;
}
