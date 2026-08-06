export interface County {
  id: string;
  name: string;
  description: string;
  imageUrl: string;
}

export interface Attraction {
  id: string;
  name: string;
  city: string;
  category: string;
  imageUrl: string;
  rating: number;
}

export interface AttractionDetail extends Attraction {
  description: string;
  address: string;
  website: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
