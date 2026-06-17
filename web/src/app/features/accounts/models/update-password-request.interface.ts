export interface UpdatePasswordRequest {
  userId: string;
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
