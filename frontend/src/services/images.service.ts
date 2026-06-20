import api from "./api";
import type {
  NasaImageDto,
  PagedResult,
  NasaSearchParams,
  CollectionItemDto,
  EnrichItemResponse,
} from "@/types";

export const imagesService = {
  async search(params: NasaSearchParams): Promise<PagedResult<NasaImageDto>> {
    const { data } = await api.get<PagedResult<NasaImageDto>>(
      "/images/search",
      { params },
    );
    return data;
  },

  async enrich(itemId: string): Promise<EnrichItemResponse> {
    const { data } = await api.post<EnrichItemResponse>(
      `/images/${itemId}/enrich`,
    );
    return data;
  },

  async getTimeline(userId: string): Promise<CollectionItemDto[]> {
    const { data } = await api.get<CollectionItemDto[]>(
      `/images/timeline?userId=${userId}`,
    );
    return data;
  },
};
