<script setup lang="ts">
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth.store";
import { useCollectionsStore } from "@/stores/collections.store";
import { imagesService } from "@/services/images.service";
import { collectionsService } from "@/services/collections.service";
import type { NasaImageDto, PagedResult } from "@/types";

const router = useRouter();
const auth = useAuthStore();
const collectionsStore = useCollectionsStore();

const query = ref("");
const startDate = ref("");
const endDate = ref("");
const currentPage = ref(1);

const result = ref<PagedResult<NasaImageDto> | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const selectedImage = ref<NasaImageDto | null>(null);
const addingToCollection = ref(false);
const addError = ref<string | null>(null);

async function search(page = 1) {
  if (!query.value.trim() && !startDate.value) return;
  loading.value = true;
  error.value = null;
  currentPage.value = page;
  try {
    result.value = await imagesService.search({
      query: query.value || undefined,
      startDate: startDate.value || undefined,
      endDate: endDate.value || undefined,
      page,
    });
  } catch {
    error.value = "Failed to fetch images. Please try again.";
  } finally {
    loading.value = false;
  }
}

async function openAddModal(image: NasaImageDto) {
  selectedImage.value = image;
  addError.value = null;
  if (auth.userId && collectionsStore.collections.length === 0) {
    await collectionsStore.fetchByUser(auth.userId);
  }
}

async function addToCollection(collectionId: string) {
  if (!selectedImage.value) return;
  addingToCollection.value = true;
  addError.value = null;
  try {
    await collectionsService.addItem(collectionId, {
      nasaImageId: selectedImage.value.nasaId,
      nasaImageUrl: selectedImage.value.imageUrl,
      title: selectedImage.value.title,
      description: selectedImage.value.description,
      earthDate: selectedImage.value.date,
    });
    selectedImage.value = null;
  } catch {
    addError.value = "Failed to add image to collection.";
  } finally {
    addingToCollection.value = false;
  }
}

function formatDate(date?: string) {
  if (!date) return "—";
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}
</script>

<template>
  <div class="page">
    <!-- Header -->
    <header class="top-bar">
      <span class="logo">🚀 Space Explorer</span>
      <nav>
        <router-link :to="{ name: 'collections' }">Collections</router-link>
        <router-link :to="{ name: 'timeline' }">Timeline</router-link>
        <button
          class="logout-btn"
          @click="
            auth.logout();
            $router.push({ name: 'login' });
          "
        >
          Logout
        </button>
      </nav>
    </header>

    <main class="content">
      <h1>Search NASA Images</h1>

      <!-- Search form -->
      <form class="search-form" @submit.prevent="search(1)">
        <input
          v-model="query"
          type="text"
          placeholder="Search images… (e.g. Mars, Apollo, nebula)"
        />
        <input v-model="startDate" type="date" title="Start date" />
        <input v-model="endDate" type="date" title="End date" />
        <button type="submit" :disabled="loading">
          {{ loading ? "Searching…" : "Search" }}
        </button>
      </form>

      <!-- States -->
      <div v-if="loading" class="state-msg">Loading images…</div>
      <div v-else-if="error" class="state-msg error">{{ error }}</div>
      <div v-else-if="result && result.items.length === 0" class="state-msg">
        No images found. Try a different search term.
      </div>

      <!-- Results -->
      <div v-else-if="result" class="results">
        <p class="result-count">
          {{ result.totalCount.toLocaleString() }} results — page
          {{ result.page }} of {{ result.totalPages }}
        </p>

        <div class="grid">
          <div v-for="image in result.items" :key="image.nasaId" class="card">
            <img :src="image.imageUrl" :alt="image.title" loading="lazy" />
            <div class="card-body">
              <h3>{{ image.title }}</h3>
              <p v-if="image.date" class="date">{{ formatDate(image.date) }}</p>
              <p v-if="image.description" class="desc">
                {{ image.description?.slice(0, 120) }}…
              </p>
              <button class="add-btn" @click="openAddModal(image)">
                + Add to collection
              </button>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div class="pagination">
          <button
            :disabled="!result.hasPreviousPage"
            @click="search(currentPage - 1)"
          >
            ← Prev
          </button>
          <span>{{ result.page }} / {{ result.totalPages }}</span>
          <button
            :disabled="!result.hasNextPage"
            @click="search(currentPage + 1)"
          >
            Next →
          </button>
        </div>
      </div>
    </main>

    <!-- Add to collection modal -->
    <div
      v-if="selectedImage"
      class="modal-backdrop"
      @click.self="selectedImage = null"
    >
      <div class="modal">
        <h2>Add to collection</h2>
        <p class="modal-title">{{ selectedImage.title }}</p>

        <div v-if="collectionsStore.collections.length === 0" class="state-msg">
          No collections yet.
          <router-link :to="{ name: 'collections' }"
            >Create one first.</router-link
          >
        </div>

        <ul v-else class="collection-list">
          <li
            v-for="col in collectionsStore.collections"
            :key="col.id"
            @click="addToCollection(col.id)"
          >
            <span>{{ col.name }}</span>
            <span class="count">{{ col.itemCount }} items</span>
          </li>
        </ul>

        <p v-if="addError" class="error">{{ addError }}</p>
        <button class="close-btn" @click="selectedImage = null">Cancel</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  background: #0f0f1a;
  color: #e0e0ff;
}
.top-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 2rem;
  background: #1a1a2e;
  border-bottom: 1px solid #2a2a4a;
}
.logo {
  color: #7c6af5;
  font-size: 1.25rem;
  font-weight: 600;
}
nav {
  display: flex;
  gap: 1.5rem;
  align-items: center;
}
nav a {
  color: #a0a0c0;
  text-decoration: none;
}
nav a:hover {
  color: #e0e0ff;
}
.logout-btn {
  background: none;
  border: 1px solid #2a2a4a;
  color: #a0a0c0;
  padding: 0.3rem 0.8rem;
  border-radius: 6px;
  cursor: pointer;
}
.content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
h1 {
  margin-bottom: 1.5rem;
}
.search-form {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}
.search-form input {
  flex: 1;
  min-width: 180px;
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 6px;
  color: #e0e0ff;
  padding: 0.6rem 0.9rem;
  font-size: 1rem;
}
.search-form button {
  background: #7c6af5;
  border: none;
  border-radius: 6px;
  color: #fff;
  padding: 0.6rem 1.5rem;
  font-size: 1rem;
  cursor: pointer;
}
.search-form button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.state-msg {
  color: #a0a0c0;
  padding: 2rem 0;
}
.state-msg.error {
  color: #f87171;
}
.result-count {
  color: #a0a0c0;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.25rem;
}
.card {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 10px;
  overflow: hidden;
}
.card img {
  width: 100%;
  height: 180px;
  object-fit: cover;
  display: block;
}
.card-body {
  padding: 1rem;
}
.card-body h3 {
  font-size: 0.95rem;
  margin-bottom: 0.4rem;
}
.date {
  color: #7c6af5;
  font-size: 0.8rem;
  margin-bottom: 0.4rem;
}
.desc {
  color: #a0a0c0;
  font-size: 0.8rem;
  margin-bottom: 0.75rem;
  line-height: 1.4;
}
.add-btn {
  background: transparent;
  border: 1px solid #7c6af5;
  color: #7c6af5;
  border-radius: 6px;
  padding: 0.4rem 0.9rem;
  cursor: pointer;
  font-size: 0.875rem;
  transition:
    background 0.2s,
    color 0.2s;
}
.add-btn:hover {
  background: #7c6af5;
  color: #fff;
}
.pagination {
  display: flex;
  gap: 1rem;
  align-items: center;
  justify-content: center;
  margin-top: 2rem;
}
.pagination button {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  color: #e0e0ff;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
}
.pagination button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.modal {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 12px;
  padding: 2rem;
  width: 100%;
  max-width: 440px;
}
.modal h2 {
  margin-bottom: 0.5rem;
}
.modal-title {
  color: #a0a0c0;
  font-size: 0.875rem;
  margin-bottom: 1.25rem;
}
.collection-list {
  list-style: none;
  padding: 0;
  margin: 0 0 1rem;
}
.collection-list li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  border: 1px solid #2a2a4a;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 0.5rem;
  transition: background 0.15s;
}
.collection-list li:hover {
  background: #2a2a4a;
}
.count {
  color: #a0a0c0;
  font-size: 0.8rem;
}
.close-btn {
  background: #2a2a4a;
  border: none;
  color: #e0e0ff;
  padding: 0.5rem 1.25rem;
  border-radius: 6px;
  cursor: pointer;
}
.error {
  color: #f87171;
  font-size: 0.875rem;
  margin-bottom: 0.75rem;
}
</style>
