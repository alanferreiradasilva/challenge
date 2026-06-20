<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useAuthStore } from "@/stores/auth.store";
import { imagesService } from "@/services/images.service";
import type { CollectionItemDto } from "@/types";

const auth = useAuthStore();

const items = ref<CollectionItemDto[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);

onMounted(async () => {
  if (!auth.userId) return;
  try {
    items.value = await imagesService.getTimeline(auth.userId);
  } catch {
    error.value = "Failed to load timeline.";
  } finally {
    loading.value = false;
  }
});

// Group items by year-month
const grouped = computed(() => {
  const groups: Record<string, CollectionItemDto[]> = {};
  for (const item of items.value) {
    const key = item.earthDate
      ? new Date(item.earthDate).toLocaleDateString("en-US", {
          year: "numeric",
          month: "long",
        })
      : "Unknown date";
    if (!groups[key]) groups[key] = [];
    groups[key].push(item);
  }
  return groups;
});

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
    <header class="top-bar">
      <span class="logo">🚀 Space Explorer</span>
      <nav>
        <router-link :to="{ name: 'search' }">Search</router-link>
        <router-link :to="{ name: 'collections' }">Collections</router-link>
      </nav>
    </header>

    <main class="content">
      <h1>Timeline</h1>
      <p class="subtitle">Your images ordered by capture date</p>

      <div v-if="loading" class="state-msg">Loading timeline…</div>
      <div v-else-if="error" class="state-msg error">{{ error }}</div>

      <div v-else-if="items.length === 0" class="empty-state">
        <p>No dated images yet.</p>
        <router-link :to="{ name: 'search' }"
          >Search and add images to your collections.</router-link
        >
      </div>

      <div v-else class="timeline">
        <div v-for="(group, month) in grouped" :key="month" class="group">
          <div class="month-label">
            <span class="dot"></span>
            {{ month }}
          </div>

          <div class="cards-row">
            <div v-for="item in group" :key="item.id" class="card">
              <img :src="item.nasaImageUrl" :alt="item.title" loading="lazy" />
              <div class="card-body">
                <h3>{{ item.title }}</h3>
                <p class="date">{{ formatDate(item.earthDate) }}</p>
                <p v-if="item.aiDescription" class="ai-desc">
                  <span class="ai-badge">✨ AI</span>
                  {{ item.aiDescription.slice(0, 100) }}…
                </p>
                <div v-if="item.tags.length" class="tags">
                  <span v-for="tag in item.tags" :key="tag" class="tag">{{
                    tag
                  }}</span>
                </div>
                <router-link
                  :to="{
                    name: 'collection-detail',
                    params: { id: item.collectionId },
                  }"
                  class="view-link"
                  >View collection →</router-link
                >
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
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
h1 {
  margin-bottom: 0.25rem;
}
.subtitle {
  color: #a0a0c0;
  margin-bottom: 2rem;
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
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.empty-state a {
  color: #7c6af5;
}
.timeline {
  display: flex;
  flex-direction: column;
  gap: 2.5rem;
}
.group {
}
.month-label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #7c6af5;
  font-size: 1.1rem;
  font-weight: 600;
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #2a2a4a;
}
.dot {
  width: 10px;
  height: 10px;
  background: #7c6af5;
  border-radius: 50%;
  flex-shrink: 0;
}
.cards-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 1rem;
}
.card {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 10px;
  overflow: hidden;
}
.card img {
  width: 100%;
  height: 160px;
  object-fit: cover;
  display: block;
}
.card-body {
  padding: 1rem;
}
.card-body h3 {
  font-size: 0.9rem;
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
  margin-right: 0.3rem;
}
.ai-desc {
  color: #c0c0e0;
  font-size: 0.78rem;
  line-height: 1.5;
  margin-bottom: 0.6rem;
}
.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  margin-bottom: 0.6rem;
}
.tag {
  background: #2a2a4a;
  color: #a0a0c0;
  font-size: 0.72rem;
  padding: 0.15rem 0.45rem;
  border-radius: 4px;
}
.view-link {
  color: #7c6af5;
  font-size: 0.8rem;
  text-decoration: none;
}
.view-link:hover {
  text-decoration: underline;
}
</style>
