import axios from "axios";
import authService from "./authService";
const apiUrl = import.meta.env.VITE_API_URL;
const instance = axios.create({
	baseURL: apiUrl,
	timeout: 10000,
	withCredentials: true, // Enables sending cookies with cross-origin requests
	headers: {
		"Content-Type": "application/json",
	},
});

// Add request interceptor to correctly merge headers
instance.interceptors.request.use(
	(config) => {
		// Save custom options from config to handle properly in the response interceptor
		if (config.skipAuthRedirect) {
			config.skipAuthRedirect = true;
		}

		// Ensure headers are preserved
		config.headers = {
			...config.headers,
		};

		return config;
	},
	(error) => Promise.reject(error)
);

// Add a response interceptor to handle token refresh
instance.interceptors.response.use(
	(response) => response,
	async (error) => {
		const originalRequest = error.config;
		if (originalRequest?.isLoginRequest) {
			return Promise.reject(error); // để catch trong login() xử lý
		}

		if (originalRequest?.isRefreshRequest === true) {
			console.error("Lỗi xác thực:", error);
			window.location.href = "/";
			return Promise.reject(error);
		}

		// Kiểm tra nếu request đến từ header, không chuyển hướng
		if (
			originalRequest?.headers &&
			originalRequest.headers["X-From-Header"] === "true"
		) {
			return Promise.reject(error);
		}
		if (error.response?.status === 401 && !originalRequest._retry) {
			originalRequest._retry = true;
			try {
				await authService.refreshToken();
				console.log("refreshToken thành công");

				// Clone the original request to ensure all headers are preserved
				const newRequest = {
					...originalRequest,
					headers: { ...originalRequest.headers },
				};

				return instance(newRequest);
			} catch (refreshError) {
				console.error("Lỗi khi làm mới token:", refreshError);
				if (!originalRequest?.skipAuthRedirect) {
					if (window.location.pathname !== "/not-logged-in") {
						window.location.href = "/not-logged-in";
						console.error("Lỗi khi làm mới token:", refreshError);
					}
				}
				return Promise.reject(refreshError);
			}
		}

		return Promise.reject(error);
	}
);

export default instance;
