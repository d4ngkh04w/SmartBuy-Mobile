// router/index.js
import { createRouter, createWebHistory } from "vue-router";
import { routes } from "./routes";
import authService from "../services/authService.js";

// Khởi tạo router
const router = createRouter({
	history: createWebHistory(import.meta.env.BASE_URL),
	routes,
	scrollBehavior(to, from, savedPosition) {
		if (to.hash) {
			return { el: to.hash, behavior: "smooth" };
		}
		return savedPosition || { top: 0 };
	},
});
router.beforeEach(async (to, from, next) => {
  document.title = to.meta.title || "SmartBuy Mobile";
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const isAuthRoute = ['/login', '/register', '/not-logged-in'].includes(to.path);

  if (!requiresAuth) return next();

  try {
    await authService.verifyUser({ skipAuthRedirect: true });
    next();
  } catch (error) {
    if ([401, 403].includes(error.response?.status)) {
      try {
        await authService.logout();
      } catch (logoutError) {
        console.error("Logout error:", logoutError);
      }
      if (!isAuthRoute) {
        // Lưu lại đường dẫn hiện tại để redirect sau đăng nhập
        return next({
          path: '/login',
          query: { redirect: to.fullPath }
        });
      }
    }
    next('/'); // Fallback
  }
});



export default router;