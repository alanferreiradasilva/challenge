<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";
import { collectionsService } from "@/services/collections.service";
import { imagesService } from "@/services/images.service";
import type {
  CollectionDto,
  CollectionItemDto,
  EnrichItemResponse,
  TagSuggestionResponse,
} from "@/types";
import api from "@/services/api";

const route = useRoute();
const collectionId = route.params.id as string;

const collection = ref<CollectionDto | null>(null);
const items = ref<CollectionItemDto[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);

// AI enrichment modal
const enrichTarget = ref<CollectionItemDto | null>(null);
const enrichResult = ref<EnrichItemResponse | null>(null);
const enriching = ref(false);

// Tag suggestions modal
const tagTarget = ref<CollectionItemDto | null>(null);
const tagSuggestions = ref<string[]>([]);
const loadingTags = ref(false);
const newTag = ref("");
const addingTag = ref(false);

onMounted(async () => {
  try {
    const [col, its] = await Promise.all([
      collectionsService.getById(collectionId),
      collectionsService.getItems(collectionId),
    ]);
    collection.value = col;
    items.value = its;
  } catch {
    error.value = "Failed to load collection.";
  } finally {
    loading.value = false;
  }
});

async function removeItem(itemId: string) {
  if (!confirm("Remove this image from the collection?")) return;
  await collectionsService.removeItem(collectionId, itemId);
  items.value = items.value.filter((i) => i.id !== itemId);
}

async function enrichItem(item: CollectionItemDto) {
  enrichTarget.value = item;
  enrichResult.value = null;
  enriching.value = true;
  try {
    enrichResult.value = await imagesService.enrich(item.id);
    // Update item in place
    const idx = items.value.findIndex((i) => i.id === item.id);
    const found = items.value[idx];
    if (idx !== -1 && found)
      found.aiDescription = enrichResult.value.aiDescription;
  } finally {
    enriching.value = false;
  }
}

async function openTagModal(item: CollectionItemDto) {
  tagTarget.value = item;
  tagSuggestions.value = [];
  newTag.value = "";
  loadingTags.value = true;
  try {
    const { data } = await api.get<TagSuggestionResponse>(
      `/items/${item.id}/tags/suggestions`,
    );
    tagSuggestions.value = data.suggestions;
  } finally {
    loadingTags.value = false;
  }
}

async function addTag(tagName: string) {
  if (!tagTarget.value || !tagName.trim()) return;
  addingTag.value = true;
  try {
    await api.post(`/items/${tagTarget.value.id}/tags`, {
      name: tagName.trim(),
    });
    const idx = items.value.findIndex((i) => i.id === tagTarget.value!.id);
    const found = items.value[idx];
    if (idx !== -1 && found && !found.tags.includes(tagName)) {
      found.tags.push(tagName.trim());
    }
    newTag.value = "";
  } finally {
    addingTag.value = false;
  }
}

async function removeTag(
  item: CollectionItemDto,
  tagId: string,
  tagName: string,
) {
  await api.delete(`/items/${item.id}/tags/${tagId}`);
  const idx = items.value.findIndex((i) => i.id === item.id);
  const found = items.value[idx];
  if (idx !== -1 && found) {
    found.tags = found.tags.filter((t) => t !== tagName);
  }
}

function formatDate(date?: string) {
  if (!date) return null;
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}
</script>

<template>
  <div class="page">
    <header class="top-bar">
      <span class="logo">🚀 Space Explorer</span>
      <nav>
        <router-link :to="{ name: 'search' }">Search</router-link>
        <router-link :to="{ name: 'collections' }">Collections</router-link>
        <router-link :to="{ name: 'timeline' }">Timeline</router-link>
      </nav>
    </header>

    <main class="content">
      <div v-if="loading" class="state-msg">Loading…</div>
      <div v-else-if="error" class="state-msg error">{{ error }}</div>

      <template v-else>
        <div class="page-header">
          <div>
            <h1>{{ collection?.name }}</h1>
            <p v-if="collection?.description" class="subtitle">
              {{ collection.description }}
            </p>
          </div>
          <router-link :to="{ name: 'collections' }" class="back-link"
            >← Back</router-link
          >
        </div>

        <div v-if="items.length === 0" class="empty-state">
          No images yet.
          <router-link :to="{ name: 'search' }"
            >Search and add some.</router-link
          >
        </div>

        <div v-else class="grid">
          <div v-for="item in items" :key="item.id" class="card">
            <img :src="item.nasaImageUrl" :alt="item.title" loading="lazy" />
            <div class="card-body">
              <h3>{{ item.title }}</h3>
              <p v-if="item.earthDate" class="date">
                {{ formatDate(item.earthDate) }}
              </p>

              <!-- AI description -->
              <p v-if="item.aiDescription" class="ai-desc">
                <span class="ai-badge">✨ AI</span>
                {{ item.aiDescription }}
              </p>

              <!-- Tags -->
              <div v-if="item.tags.length" class="tags">
                <span v-for="tag in item.tags" :key="tag" class="tag">{{
                  tag
                }}</span>
              </div>

              <div class="card-actions">
                <button class="action-btn" @click="enrichItem(item)">
                  ✨ Enrich
                </button>
                <button class="action-btn" @click="openTagModal(item)">
                  🏷 Tags
                </button>
                <button class="remove-btn" @click="removeItem(item.id)">
                  Remove
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
    </main>

    <!-- Enrich modal -->
    <div
      v-if="enrichTarget"
      class="modal-backdrop"
      @click.self="enrichTarget = null"
    >
      <div class="modal">
        <h2>✨ AI Enrichment</h2>
        <p class="modal-subtitle">{{ enrichTarget.title }}</p>
        <div v-if="enriching" class="state-msg">Generating…</div>
        <template v-else-if="enrichResult">
          <p class="enrich-desc">{{ enrichResult.aiDescription }}</p>
          <ul class="curiosities">
            <li v-for="(c, i) in enrichResult.curiosities" :key="i">{{ c }}</li>
          </ul>
        </template>
        <button class="primary-btn" @click="enrichTarget = null">Close</button>
      </div>
    </div>

    <!-- Tags modal -->
    <div v-if="tagTarget" class="modal-backdrop" @click.self="tagTarget = null">
      <div class="modal">
        <h2>🏷 Tags</h2>
        <p class="modal-subtitle">{{ tagTarget.title }}</p>

        <div v-if="loadingTags" class="state-msg">Loading suggestions…</div>
        <template v-else>
          <p class="label">AI Suggestions</p>
          <div class="tag-suggestions">
            <button
              v-for="s in tagSuggestions"
              :key="s"
              class="suggestion-tag"
              @click="addTag(s)"
            >
              + {{ s }}
            </button>
          </div>
        </template>

        <div class="tag-input-row">
          <input
            v-model="newTag"
            type="text"
            placeholder="Add custom tag…"
            @keyup.enter="addTag(newTag)"
          />
          <button
            class="primary-btn"
            :disabled="addingTag"
            @click="addTag(newTag)"
          >
            Add
          </button>
        </div>

        <button
          class="secondary-btn"
          style="margin-top: 0.75rem"
          @click="tagTarget = null"
        >
          Done
        </button>
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
}
nav a {
  color: #a0a0c0;
  text-decoration: none;
}
nav a:hover {
  color: #e0e0ff;
}
.content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}
.subtitle {
  color: #a0a0c0;
  margin-top: 0.25rem;
}
.back-link {
  color: #7c6af5;
  text-decoration: none;
  font-size: 0.9rem;
}
.state-msg {
  color: #a0a0c0;
  padding: 2rem 0;
}
.state-msg.error {
  color: #f87171;
}
.empty-state {
  color: #a0a0c0;
  padding: 3rem 0;
}
.empty-state a {
  color: #7c6af5;
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
  margin-bottom: 0.5rem;
}
.ai-badge {
  background: #7c6af5;
  color: #fff;
  font-size: 0.7rem;
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
  margin-right: 0.4rem;
}
.ai-desc {
  color: #c0c0e0;
  font-size: 0.8rem;
  line-height: 1.5;
  margin-bottom: 0.75rem;
}
.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  margin-bottom: 0.75rem;
}
.tag {
  background: #2a2a4a;
  color: #a0a0c0;
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}
.card-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.action-btn {
  background: transparent;
  border: 1px solid #2a2a4a;
  color: #a0a0c0;
  padding: 0.3rem 0.7rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.8rem;
}
.action-btn:hover {
  border-color: #7c6af5;
  color: #7c6af5;
}
.remove-btn {
  background: transparent;
  border: 1px solid #f87171;
  color: #f87171;
  padding: 0.3rem 0.7rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.8rem;
  margin-left: auto;
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
  max-width: 500px;
}
.modal h2 {
  margin-bottom: 0.5rem;
}
.modal-subtitle {
  color: #a0a0c0;
  font-size: 0.875rem;
  margin-bottom: 1.25rem;
}
.enrich-desc {
  color: #c0c0e0;
  line-height: 1.6;
  margin-bottom: 1rem;
}
.curiosities {
  color: #a0a0c0;
  padding-left: 1.25rem;
  margin-bottom: 1.25rem;
}
.curiosities li {
  margin-bottom: 0.4rem;
  line-height: 1.5;
}
.label {
  color: #a0a0c0;
  font-size: 0.8rem;
  margin-bottom: 0.5rem;
}
.tag-suggestions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.suggestion-tag {
  background: #2a2a4a;
  border: none;
  color: #7c6af5;
  padding: 0.3rem 0.7rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.8rem;
}
.suggestion-tag:hover {
  background: #7c6af5;
  color: #fff;
}
.tag-input-row {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.75rem;
}
.tag-input-row input {
  flex: 1;
  background: #0f0f1a;
  border: 1px solid #2a2a4a;
  border-radius: 6px;
  color: #e0e0ff;
  padding: 0.5rem 0.75rem;
  font-size: 0.9rem;
  outline: none;
}
.tag-input-row input:focus {
  border-color: #7c6af5;
}
.primary-btn {
  background: #7c6af5;
  border: none;
  border-radius: 6px;
  color: #fff;
  padding: 0.5rem 1.25rem;
  cursor: pointer;
}
.primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.secondary-btn {
  background: #2a2a4a;
  border: none;
  border-radius: 6px;
  color: #e0e0ff;
  padding: 0.5rem 1.25rem;
  cursor: pointer;
}
</style>
