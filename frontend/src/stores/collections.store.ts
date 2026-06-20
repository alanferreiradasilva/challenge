import { defineStore } from "pinia";
import { ref } from "vue";
import type { CollectionDto } from "@/types";
import { collectionsService } from "@/services/collections.service";

export const useCollectionsStore = defineStore("collections", () => {
  const collections = ref<CollectionDto[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchByUser(userId: string) {
    loading.value = true;
    error.value = null;
    try {
      collections.value = await collectionsService.getByUser(userId);
    } catch (e) {
      error.value = "Failed to load collections.";
    } finally {
      loading.value = false;
    }
  }

  async function create(name: string, description?: string) {
    const created = await collectionsService.create({ name, description });
    collections.value.push(created);
    return created;
  }

  async function remove(id: string) {
    await collectionsService.remove(id);
    collections.value = collections.value.filter((c) => c.id !== id);
  }

  return { collections, loading, error, fetchByUser, create, remove };
});
