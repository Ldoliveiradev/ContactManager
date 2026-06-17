export interface PaginationFilter {
  search?: string | null;
  sortBy?: string | null;
  sortDesc?: boolean;
  page?: number;
  pageSize?: number;
}
