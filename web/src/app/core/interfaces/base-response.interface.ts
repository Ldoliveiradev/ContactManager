export interface BaseResponse<T> {
  data: T | null;
  isSuccess: boolean;
  error: string | null;
}
