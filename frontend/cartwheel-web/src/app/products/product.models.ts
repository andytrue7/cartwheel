export interface ProductResponse {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly price: number;
  readonly stockQuantity: number;
  readonly categoryId: string;
  readonly categoryName: string;
}

export interface PagedResponse<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}
