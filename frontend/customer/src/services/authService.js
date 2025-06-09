import instance from "./axiosConfig";
import emitter from "../utils/evenBus.js";
class AuthService {
	async verifyUser() {
		try {
			const response = await instance.get("/user/auth/verify");
			if (response.data && response.data.success) {
				localStorage.setItem("isLogin", "true");
			}
			return response;
		} catch (error) {
			localStorage.removeItem("isLogin");
			throw error;
		}
	}

	async login(phoneNumber, password) {
		const response = await instance.post(
			"/user/auth/login",
			{ phoneNumber, password },
			{
				isLoginRequest: true, // Flag to handle login-specific errors
			}
		);

		if (response.data && response.data.success) {
			localStorage.setItem("isLogin", "true");
		}
		return response;
	}

	async googleLogin(tokenData) {
		const response = await instance.post(
			"/user/auth/google-login",
			{
				Token: tokenData.Token,
			},
			{
				isLoginRequest: true, // Flag to handle login-specific errors
			}
		);

		if (response.data && response.data.success) {
			localStorage.setItem("isLogin", "true");
		}
		return response;
	}

	async register(userInfo) {
		const response = await instance.post("/user/auth/register", userInfo, {
			isLoginRequest: true, // Handle registration like login for error handling
		});

		if (response.data && response.data.success) {
			localStorage.setItem("isLogin", "true");
		}
		return response;
	}

	async refreshToken() {
		return await instance.post(
			"/auth/refresh-token",
			{},
			{
				isRefreshRequest: true,
			}
		);
	}

	async forgotPassword(data) {
		return await instance.post("/auth/forgot-password", data);
	}

	async resetPassword(resetData) {
		return await instance.post("/auth/reset-password", resetData);
	}

	async logout() {
		emitter.emit("logout");
		localStorage.removeItem("isLogin");
		localStorage.removeItem("userData");
		return await instance.post("/auth/logout");
	}

	async sendVerificationEmail() {
		return await instance.post("/user/auth/send-verification-email");
	}

	async verifyEmail(verificationData) {
		return await instance.post("/user/auth/verify-email", verificationData);
	}
}
export default new AuthService();
