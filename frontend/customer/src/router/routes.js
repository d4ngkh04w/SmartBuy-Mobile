import MainLayout from "../layouts/MainLayout.vue";
import BlankLayout from "../layouts/BlankLayout.vue";
import HomePage from "../views/HomePage.vue";
import ProductDetail from "../components/product/ProductDetail.vue";
import Cart from "../views/CartPage.vue";
import NotLoggedIn from "@/components/common/NotLoggedIn.vue";

export const routes = [
	// 📦 Layout chính: Có header và footer
	{
		path: "/",
		component: MainLayout,
		children: [
			{
				path: "",
				name: "home",
				component: HomePage,
				meta: {
					title: "SmartBuy Mobile - Hệ thống bán lẻ điện thoại di động",
					requiresAuth: false,
				},
			},
			{
				path: "product/:id",
				name: "product-detail",
				component: ProductDetail,
				props: true,
				meta: {
					title: "Chi tiết sản phẩm - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "cart",
				name: "cart",
				component: Cart,
				meta: {
					title: "Giỏ hàng - SmartBuy Mobile",
					requiresAuth: true,
				},
			},
			{
				path: "order",
				name: "order",
				component: () => import("../views/Order.vue"),
				meta: {
					title: "Đặt hàng - SmartBuy Mobile",
					requiresAuth: true,
				},
			},
			{
				path: "account",
				name: "account",
				component: () => import("../views/AccountPage.vue"),
				meta: {
					title: "Tài khoản của tôi - SmartBuy Mobile",
					requiresAuth: true,
				},
			},
			{
				path: "review/:id",
				name: "review",
				component: () => import("../views/ReviewPage.vue"),
				meta: {
					title: "Đánh giá sản phẩm - SmartBuy Mobile",
					requiresAuth: true,
				},
			},
		],
	},

	// Layout trắng: Không có header/footer
	{
		path: "/",
		component: BlankLayout,
		children: [
			{
				path: "login",
				name: "login",
				component: () => import("../views/LoginPage.vue"),
				meta: {
					title: "Đăng nhập - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "register",
				name: "register",
				component: () => import("../views/RegisterPage.vue"),
				meta: {
					title: "Đăng ký - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "not-logged-in",
				name: "not-logged-in",
				component: NotLoggedIn,
				meta: {
					title: "Chưa đăng nhập - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "profile-setup",
				name: "profile-setup",
				component: () => import("../views/ProfileSetupPage.vue"),
				meta: {
					title: "Hoàn tất thông tin - SmartBuy Mobile",
					requiresAuth: true,
				},
			},
			{
				path: "forgot-password",
				name: "forgot-password",
				component: () => import("../views/ForgotPasswordPage.vue"),
				meta: {
					title: "Quên mật khẩu - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "reset-password",
				name: "reset-password",
				component: () => import("../views/ResetPasswordPage.vue"),
				meta: {
					title: "Đặt lại mật khẩu - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "verify-email",
				name: "verify-email",
				component: () => import("../views/VerifyEmailPage.vue"),
				meta: {
					title: "Xác thực email - SmartBuy Mobile",
					requiresAuth: false,
				},
			},
			{
				path: "/:pathMatch(.*)*",
				name: "not-found",
				component: () => import("../components/common/NotFound.vue"),
				meta: {
					title: "404 - Không tìm thấy trang",
					requiresAuth: false,
				},
			},
		],
	},
];
