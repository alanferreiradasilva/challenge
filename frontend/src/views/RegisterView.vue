<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth.store";

const router = useRouter();
const auth = useAuthStore();

const name = ref("");
const email = ref("");
const password = ref("");
const error = ref<string | null>(null);
const loading = ref(false);

async function handleRegister() {
  error.value = null;
  loading.value = true;
  try {
    await auth.register(name.value, email.value, password.value);
    router.push({ name: "search" });
  } catch {
    error.value = "Registration failed. Email may already be in use.";
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1>Space Explorer</h1>
      <h2>Create account</h2>

      <form @submit.prevent="handleRegister">
        <div class="field">
          <label for="name">Name</label>
          <input
            id="name"
            v-model="name"
            type="text"
            required
            autocomplete="name"
            placeholder="Your name"
          />
        </div>

        <div class="field">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            autocomplete="email"
            placeholder="you@example.com"
          />
        </div>

        <div class="field">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            minlength="8"
            autocomplete="new-password"
            placeholder="Min. 8 characters"
          />
        </div>

        <p v-if="error" class="error">{{ error }}</p>

        <button type="submit" :disabled="loading">
          {{ loading ? "Creating account…" : "Create account" }}
        </button>
      </form>

      <p class="switch-link">
        Already have an account?
        <router-link :to="{ name: 'login' }">Sign in</router-link>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f0f1a;
}
.auth-card {
  background: #1a1a2e;
  border: 1px solid #2a2a4a;
  border-radius: 12px;
  padding: 2.5rem;
  width: 100%;
  max-width: 420px;
}
h1 {
  color: #7c6af5;
  margin-bottom: 0.25rem;
  font-size: 1.5rem;
}
h2 {
  color: #e0e0ff;
  margin-bottom: 1.5rem;
  font-weight: 400;
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
  transition: border-color 0.2s;
}
input:focus {
  border-color: #7c6af5;
}
button {
  width: 100%;
  margin-top: 0.5rem;
  padding: 0.75rem;
  background: #7c6af5;
  border: none;
  border-radius: 6px;
  color: #fff;
  font-size: 1rem;
  cursor: pointer;
  transition: opacity 0.2s;
}
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.error {
  color: #f87171;
  font-size: 0.875rem;
  margin: 0.5rem 0;
}
.switch-link {
  color: #a0a0c0;
  margin-top: 1.25rem;
  text-align: center;
  font-size: 0.875rem;
}
.switch-link a {
  color: #7c6af5;
}
</style>
