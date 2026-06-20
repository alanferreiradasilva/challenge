import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { AuthResponse } from "@/types";
import { authService } from "@/services/auth.service";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<AuthResponse | null>(
    JSON.parse(localStorage.getItem("auth_user") ?? "null"),
  );

  const isAuthenticated = computed(() => !!user.value?.token);
  const token = computed(() => user.value?.token ?? null);
  const userId = computed(() => user.value?.userId ?? null);

  async function login(email: string, password: string) {
    const response = await authService.login({ email, password });
    user.value = response;
    localStorage.setItem("auth_user", JSON.stringify(response));
  }

  async function register(name: string, email: string, password: string) {
    const response = await authService.register({ name, email, password });
    user.value = response;
    localStorage.setItem("auth_user", JSON.stringify(response));
  }

  function logout() {
    user.value = null;
    localStorage.removeItem("auth_user");
  }

  return { user, isAuthenticated, token, userId, login, register, logout };
});
