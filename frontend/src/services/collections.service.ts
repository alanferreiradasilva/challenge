import api from "./api";
import type {
  CollectionDto,
  CreateCollectionRequest,
  CollectionItemDto,
  AddItemRequest,
} from "@/types";

export const collectionsService = {
  async getByUser(userId: string): Promise<CollectionDto[]> {
    const { data } = await api.get<CollectionDto[]>(
      `/collections?userId=${userId}`,
    );
    return data;
  },

  async getById(id: string): Promise<CollectionDto> {
    const { data } = await api.get<CollectionDto>(`/collections/${id}`);
    return data;
  },

  async create(request: CreateCollectionRequest): Promise<CollectionDto> {
    const { data } = await api.post<CollectionDto>("/collections", request);
    return data;
  },

  async remove(id: string): Promise<void> {
    await api.delete(`/collections/${id}`);
  },

  async getItems(collectionId: string): Promise<CollectionItemDto[]> {
    const { data } = await api.get<CollectionItemDto[]>(
      `/collections/${collectionId}/items`,
    );
    return data;
  },

  async addItem(
    collectionId: string,
    request: AddItemRequest,
  ): Promise<CollectionItemDto> {
    const { data } = await api.post<CollectionItemDto>(
      `/collections/${collectionId}/items`,
      request,
    );
    return data;
  },

  async removeItem(collectionId: string, itemId: string): Promise<void> {
    await api.delete(`/collections/${collectionId}/items/${itemId}`);
  },
};
