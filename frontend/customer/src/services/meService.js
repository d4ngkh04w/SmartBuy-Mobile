import instance from "./axiosConfig";

class MeService {
	// Utility methods
	getHeaders = (data) =>
		data instanceof FormData
			? { headers: { "Content-Type": "multipart/form-data" } }
			: {};

	getUrlImage(url) {
		const baseUrl =
			import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
		return url?.startsWith("http") ? url : `${baseUrl}${url}`;
	}

	// API methods
	async getMe(options = {}) {
		const config = { ...options };
		try {
			const response = await instance.get("/user/me", config);
			return response.data.data;
		} catch (error) {
			console.error("Error fetching user data:", error);
			throw error;
		}
	}

	async changePassword(passwordData) {
		return await instance.post("/auth/change-password", passwordData);
	}

	async updateUserProfile(userInfo) {
		return (
			await instance.put("/user/me", userInfo, this.getHeaders(userInfo))
		).data.data;
	}

    async sendVerificationEmail() {
        return await instance.post("/user/auth/send-verification-email");
    }
    async updateProfile(userInfo) {
    const formData = new FormData();
    Object.keys(userInfo).forEach(key => {
        if (userInfo[key] !== undefined && userInfo[key] !== null) {
            formData.append(key, userInfo[key]);
        }
    });

    const response = await instance.put("/user/me", formData, {
        headers: {
            'Content-Type': 'multipart/form-data'
        }
    });
    return response.data.data;
}
}

const meService = new MeService();
export default meService;
