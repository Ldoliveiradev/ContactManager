/** Column definition for the generic data grid. */
export interface DataViewColumn<T> {
  /** Property key on the row object this column displays. */
  key: keyof T & string;
  /** Header label. */
  header: string;
  /** Whether the column can be sorted (default false). */
  sortable?: boolean;
  /** Whether the column's text is included in the search filter (default false). */
  searchable?: boolean;
  /** Optional custom-cell template id, matched to a [uiDataViewCell] directive. */
  cell?: string;
}

export type SortDirection = 'asc' | 'desc';

export interface SortState<T> {
  key: keyof T & string;
  direction: SortDirection;
}
