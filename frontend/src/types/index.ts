// ── Auth ───────────────────────────────────────────────────────────────────────
export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  userId: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// ── Collections ────────────────────────────────────────────────────────────────
export interface CollectionDto {
  id: string;
  name: string;
  description?: string;
  userId: string;
  createdAt: string;
  itemCount: number;
}

export interface CreateCollectionRequest {
  name: string;
  description?: string;
}

// ── Images ─────────────────────────────────────────────────────────────────────
export interface NasaImageDto {
  nasaId: string;
  title: string;
  description?: string;
  imageUrl: string;
  photographer?: string;
  date?: string;
  location?: string;
}

export interface CollectionItemDto {
  id: string;
  collectionId: string;
  nasaImageId: string;
  nasaImageUrl: string;
  title: string;
  description?: string;
  earthDate?: string;
  aiDescription?: string;
  createdAt: string;
  tags: string[];
}

export interface AddItemRequest {
  nasaImageId: string;
  nasaImageUrl: string;
  title: string;
  description?: string;
  earthDate?: string;
}

export interface EnrichItemResponse {
  aiDescription: string;
  curiosities: string[];
}

export interface NasaSearchParams {
  query?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
}

// ── Tags ───────────────────────────────────────────────────────────────────────
export interface TagDto {
  id: string;
  name: string;
  userId: string;
}

export interface TagSuggestionResponse {
  suggestions: string[];
}

// ── Pagination ─────────────────────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
