<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useAuthStore } from "@/stores/auth.store";
import { useCollectionsStore } from "@/stores/collections.store";

const auth = useAuthStore();
const collectionsStore = useCollectionsStore();

const showModal = ref(false);
const newName = ref("");
const newDescription = ref("");
const createError = ref<string | null>(null);
const creating = ref(false);

onMounted(() => {
  if (auth.userId) {
    collectionsStore.fetchByUser(auth.userId);
  }
});

async function createCollection() {
  createError.value = null;
  creating.value = true;
  try {
    await collectionsStore.create(
      newName.value,
      newDescription.value || undefined,
    );
    showModal.value = false;
    newName.value = "";
    newDescription.value = "";
  } catch {
    createError.value = "Failed to create collection.";
  } finally {
    creating.value = false;
  }
}

async function deleteCollection(id: string) {
  if (!confirm("Delete this collection and all its items?")) return;
  await collectionsStore.remove(id);
}
</script>

<template>
  <div class="page">
    <header class="top-bar">
      <span class="logo">🚀 Space Explorer</span>
      <nav>
        <router-link :to="{ name: 'search' }">Search</router-link>
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
      <div class="page-header">
        <h1>My Collections</h1>
        <button class="primary-btn" @click="showModal = true">
          + New collection
        </button>
      </div>

      <div v-if="collectionsStore.loading" class="state-msg">Loading…</div>
      <div v-else-if="collectionsStore.error" class="state-msg error">
        {{ collectionsStore.error }}
      </div>

      <div
        v-else-if="collectionsStore.collections.length === 0"
        class="empty-state"
      >
        <p>No collections yet.</p>
        <button class="primary-btn" @click="showModal = true">
          Create your first collection
        </button>
      </div>

      <div v-else class="grid">
        <div
          v-for="col in collectionsStore.collections"
          :key="col.id"
          class="card"
        >
          <router-link
            :to="{ name: 'collection-detail', params: { id: col.id } }"
            class="card-link"
          >
            <div class="card-body">
              <h3>{{ col.name }}</h3>
              <p v-if="col.description" class="desc">{{ col.description }}</p>
              <p class="meta">
                {{ col.itemCount }} image{{ col.itemCount !== 1 ? "s" : "" }}
              </p>
            </div>
          </router-link>
          <button
            class="delete-btn"
            @click="deleteCollection(col.id)"
            title="Delete collection"
          >
            ✕
          </button>
        </div>
      </div>
    </main>

    <!-- Create modal -->
    <div
      v-if="showModal"
      class="modal-backdrop"
      @click.self="showModal = false"
    >
      <div class="modal">
        <h2>New Collection</h2>
        <form @submit.prevent="createCollection">
          <div class="field">
            <label for="col-name">Name *</label>
            <input
              id="col-name"
              v-model="newName"
              type="text"
              required
              placeholder="e.g. Mars Rovers"
            />
          </div>
          <div class="field">
            <label for="col-desc">Description</label>
            <input
              id="col-desc"
              v-model="newDescription"
              type="text"
              placeholder="Optional description"
            />
          </div>
          <p v-if="createError" class="error">{{ createError }}</p>
          <div class="modal-actions">
            <button
              type="button"
              class="secondary-btn"
              @click="showModal = false"
            >
              Cancel
            </button>
            <button type="submit" class="primary-btn" :disabled="creating">
              {{ creating ? "Creating…" : "Create" }}
            </button>
          </div>
        </form>
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
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
.primary-btn {
  background: #7c6af5;
  border: none;
  border-radius: 6px;
  color: #fff;
  padding: 0.6rem 1.25rem;
  cursor: pointer;
  font-size: 0.95rem;
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
  padding: 0.6rem 1.25rem;
  cursor: pointer;
  font-size: 0.95rem;
}
.state-msg {
  color: #a0a0c0;
  padding: 2rem 0;
}
.state-msg.error {
  color: #f87171;
}
.empty-state {
  text-align: center;
  padding: 4rem 0;
  color: #a0a0c0;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 1rem;
}
.card {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 10px;
  position: relative;
}
.card-link {
  display: block;
  text-decoration: none;
  color: inherit;
  padding: 1.25rem;
}
.card-link:hover .card-body h3 {
  color: #7c6af5;
}
.card-body h3 {
  margin-bottom: 0.4rem;
  transition: color 0.2s;
}
.desc {
  color: #a0a0c0;
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
}
.meta {
  color: #7c6af5;
  font-size: 0.8rem;
}
.delete-btn {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  background: none;
  border: none;
  color: #a0a0c0;
  cursor: pointer;
  font-size: 0.9rem;
  padding: 0.2rem 0.4rem;
  border-radius: 4px;
}
.delete-btn:hover {
  color: #f87171;
  background: rgba(248, 113, 113, 0.1);
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
  margin-bottom: 1.25rem;
}
.field {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
label {
  color: #a0a0c0;
  font-size: 0.875rem;
}
input {
  background: #0f0f1a;
  border: 1px solid #2a2a4a;
  border-radius: 6px;
  color: #e0e0ff;
  padding: 0.6rem 0.8rem;
  font-size: 1rem;
  outline: none;
}
input:focus {
  border-color: #7c6af5;
}
.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1rem;
}
.error {
  color: #f87171;
  font-size: 0.875rem;
}
</style>
