import { describe, it, expect, beforeEach, vi } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import { useAuthStore } from "@/stores/auth.store";

// Mock the auth service
vi.mock("@/services/auth.service", () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
  },
}));

import { authService } from "@/services/auth.service";

const mockAuthResponse = {
  token: "mock-jwt-token",
  name: "Alan Smith",
  email: "alan@test.com",
  userId: "123e4567-e89b-12d3-a456-426614174000",
};

describe("useAuthStore", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  it("should start unauthenticated when no stored user", () => {
    const store = useAuthStore();
    expect(store.isAuthenticated).toBe(false);
    expect(store.token).toBeNull();
  });

  it("should set user and token after successful login", async () => {
    vi.mocked(authService.login).mockResolvedValueOnce(mockAuthResponse);

    const store = useAuthStore();
    await store.login("alan@test.com", "password");

    expect(store.isAuthenticated).toBe(true);
    expect(store.token).toBe("mock-jwt-token");
    expect(store.user?.email).toBe("alan@test.com");
  });

  it("should persist user to localStorage after login", async () => {
    vi.mocked(authService.login).mockResolvedValueOnce(mockAuthResponse);

    const store = useAuthStore();
    await store.login("alan@test.com", "password");

    const stored = JSON.parse(localStorage.getItem("auth_user") ?? "null");
    expect(stored?.token).toBe("mock-jwt-token");
  });

  it("should clear user and localStorage after logout", async () => {
    vi.mocked(authService.login).mockResolvedValueOnce(mockAuthResponse);

    const store = useAuthStore();
    await store.login("alan@test.com", "password");
    store.logout();

    expect(store.isAuthenticated).toBe(false);
    expect(store.token).toBeNull();
    expect(localStorage.getItem("auth_user")).toBeNull();
  });

  it("should hydrate from localStorage on store creation", () => {
    localStorage.setItem("auth_user", JSON.stringify(mockAuthResponse));

    const store = useAuthStore();
    expect(store.isAuthenticated).toBe(true);
    expect(store.token).toBe("mock-jwt-token");
  });
});
