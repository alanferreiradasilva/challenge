import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "@/stores/auth.store";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      redirect: "/search",
    },
    {
      path: "/login",
      name: "login",
      component: () => import("@/views/LoginView.vue"),
      meta: { requiresGuest: true },
    },
    {
      path: "/register",
      name: "register",
      component: () => import("@/views/RegisterView.vue"),
      meta: { requiresGuest: true },
    },
    {
      path: "/search",
      name: "search",
      component: () => import("@/views/SearchView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/collections",
      name: "collections",
      component: () => import("@/views/CollectionsView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/collections/:id",
      name: "collection-detail",
      component: () => import("@/views/CollectionDetailView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/timeline",
      name: "timeline",
      component: () => import("@/views/TimelineView.vue"),
      meta: { requiresAuth: true },
    },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: "login" };
  }

  if (to.meta.requiresGuest && auth.isAuthenticated) {
    return { name: "search" };
  }
});

export default router;
