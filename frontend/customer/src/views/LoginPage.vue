<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import authService from "@/services/authService";
import emitter from "@/utils/evenBus";
import { GoogleLogin } from "vue3-google-login";

const phoneNumber = ref("");
const password = ref("");
const showPassword = ref(false);
const isLoading = ref(false);
const errorMessage = ref("");
const phoneNumberError = ref("");
const router = useRouter();

const handleLogin = async () => {
	// Reset errors
	errorMessage.value = "";
	phoneNumberError.value = "";

	// Validate phone number
	if (!isValidPhoneNumber(phoneNumber.value)) {
		errorMessage.value = "Số điện thoại không hợp lệ";
		return;
	}

	if (!password.value) {
		errorMessage.value = "Vui lòng nhập mật khẩu";
		return;
	}

	try {
		isLoading.value = true;

		// Call login API
		await authService.login(phoneNumber.value, password.value);

		emitter.emit("show-notification", {
			status: "success",
			message: "Đăng nhập thành công",
		});

		// Reset form
		phoneNumber.value = "";
		password.value = "";
		showPassword.value = false;

		// Điều hướng về trang trước nếu có redirect, ngược lại về trang chủ
		const redirect = router.currentRoute.value.query.redirect;
		if (redirect) {
			router.replace(redirect);
		} else {
			router.push("/");
		}
	} catch (error) {
		console.error("Error logging in:", error);

		if (error.response) {
			const { status, data } = error.response;

			emitter.emit("show-notification", {
				status: "error",
				message: data.message || "Đăng nhập thất bại",
			});
		} else {
			emitter.emit("show-notification", {
				status: "error",
				message: "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.",
			});
		}
	} finally {
		isLoading.value = false;
	}
};

/**
 * Validate phone number input (only allow digits and limit to 10 characters)
 */
const validatePhoneInput = (event) => {
	const input = event.target.value.replace(/\D/g, "").slice(0, 10);
	phoneNumber.value = input;
	phoneNumberError.value =
		input.length != 10 ? "Số điện thoại không hợp lệ" : "";
};

const handleGoogleLogin = async (response) => {
	try {
		console.log("Client ID:", import.meta.env.VITE_GOOGLE_CLIENT_ID);
		isLoading.value = true;

		// Lấy token từ Google response
		const { credential, ...otherData } = response;

		console.log("Google response data:", otherData);

		// Debug: Log token information
		console.log("Google credential received:", credential);
		console.log("Token data to be sent:", { Token: credential });

		// Gọi API đăng nhập Google từ backend với đúng định dạng yêu cầu
		await authService.googleLogin({ Token: credential });

		emitter.emit("show-notification", {
			status: "success",
			message: "Đăng nhập với Google thành công",
		});

		// Chuyển đến trang chủ
		router.push("/");
		// Điều hướng về trang trước nếu có redirect, ngược lại về trang chủ
		const redirect = router.currentRoute.value.query.redirect;
		if (redirect) {
			router.replace(redirect);
		} else {
			router.push("/");
		}
	} catch (error) {
		console.error("Error during Google login:", error);

		if (error.response) {
			const { data } = error.response;
			console.error("Error response data:", data);
			emitter.emit("show-notification", {
				status: "error",
				message: data.message || "Đăng nhập thất bại",
			});
		} else {
			emitter.emit("show-notification", {
				status: "error",
				message:
					error.message ||
					"Không thể kết nối đến máy chủ. Vui lòng thử lại sau.",
			});
		}
	} finally {
		isLoading.value = false;
	}
};

const handleGoogleLoginError = (error) => {
	console.error("Google login error:", error);
	emitter.emit("show-notification", {
		status: "error",
		message: "Đăng nhập với Google thất bại",
	});
};

/**
 * Check if phone number is valid (10 digits)
 */
const isValidPhoneNumber = (phone) => {
	return /^[0-9]{10}$/.test(phone);
};
</script>

<template>
	<div class="login-page">
		<div class="login-container">
			<div class="logo-section">
				<router-link to="/" class="logo">
					<div class="logo-text">SmartBuy Mobile</div>
				</router-link>
				<h2 class="tagline">
					Công nghệ chính hãng, dịch vụ tận tâm, giao nhanh từng phút.
				</h2>
			</div>

			<div class="form-section">
				<div class="login-card">
					<form @submit.prevent="handleLogin" class="login-form">
						<h2 class="form-title">Đăng nhập</h2>
						<div v-if="errorMessage" class="error-message">
							{{ errorMessage }}
						</div>

						<div class="form-group">
							<div class="input-container">
								<i class="fas fa-phone input-icon"></i>
								<input
									type="text"
									id="phoneNumber"
									v-model="phoneNumber"
									placeholder="Số điện thoại"
									@input="validatePhoneInput"
									:class="{ 'input-error': phoneNumberError }"
									required
								/>
							</div>
							<p class="error-text" v-if="phoneNumberError">
								{{ phoneNumberError }}
							</p>
						</div>

						<div class="form-group">
							<div class="password-input">
								<i class="fas fa-lock input-icon"></i>
								<input
									:type="showPassword ? 'text' : 'password'"
									id="password"
									v-model="password"
									placeholder="Mật khẩu"
									required
								/>
								<button
									type="button"
									class="toggle-password"
									@click="showPassword = !showPassword"
								>
									<i
										:class="
											showPassword
												? 'fas fa-eye-slash'
												: 'fas fa-eye'
										"
									></i>
								</button>
							</div>
						</div>

						<button
							type="submit"
							class="btn-login"
							:disabled="isLoading"
						>
							{{ isLoading ? "Đang đăng nhập..." : "Đăng nhập" }}
						</button>

						<div class="login-links">
							<router-link
								to="/forgot-password"
								class="auth-link"
							>
								Quên mật khẩu?
							</router-link>
							<router-link to="/register" class="auth-link">
								Đăng ký
							</router-link>
						</div>

						<div class="divider">
							<span>hoặc</span>
						</div>

						<div class="google-button-container">
							<!-- Sử dụng nút đăng nhập của Google không có chữ -->
							<GoogleLogin
								:callback="handleGoogleLogin"
								:error-callback="handleGoogleLoginError"
								:popup-type="false"
								type="icon"
								theme="outline"
								size="large"
							/>
						</div>
					</form>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
.login-page {
	min-height: 100vh;
	background-color: #f0f2f5;
	background-image: linear-gradient(
		to right bottom,
		#ffffff,
		#fdf3fa,
		#fce7f6,
		#fbdaf2,
		#f9ceee
	);
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 0 1rem;
	font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
		Helvetica, Arial, sans-serif;
}

.login-container {
	display: flex;
	align-items: center;
	max-width: 980px;
	width: 100%;
	margin: 0 auto;
	padding: 20px 0;
}

/* Logo & Tagline Section */
.logo-section {
	flex: 1;
	margin-right: 32px;
	padding-right: 32px;
	display: flex;
	flex-direction: column;
	justify-content: center;
}

.logo-text {
	font-size: 42px;
	font-weight: bold;
	color: #f86ed3;
	margin-bottom: 20px;
	line-height: 1;
	letter-spacing: -0.5px;
}

.tagline {
	font-size: 28px;
	font-weight: normal;
	line-height: 32px;
	color: #1c1e21;
	padding-right: 20px;
}

/* Form Section */
.form-section {
	width: 396px;
}

.form-title {
	text-align: center;
	margin-bottom: 1.5rem;
	color: #f86ed3;
	font-size: 1.5rem;
}

.login-card {
	background-color: #fff;
	border-radius: 8px;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 0 8px 16px rgba(0, 0, 0, 0.1);
	padding: 1.5rem;
	margin-bottom: 28px;
}

.login-form {
	display: flex;
	flex-direction: column;
}

.form-group {
	margin-bottom: 12px;
}

/* Input styles */
.input-container {
	position: relative;
	width: 100%;
}

.input-icon {
	position: absolute;
	top: 50%;
	left: 12px;
	transform: translateY(-50%);
	color: #888;
	font-size: 1rem;
}

.input-container input,
.password-input input {
	width: 100%;
	padding: 14px 16px 14px 40px;
	font-size: 17px;
	border: 1px solid #dddfe2;
	border-radius: 6px;
	color: #1c1e21;
	height: 50px;
	line-height: 50px;
}

input:focus {
	border-color: #f86ed3;
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.2);
	outline: none;
}

.password-input {
	position: relative;
}

.toggle-password {
	position: absolute;
	right: 12px;
	top: 50%;
	transform: translateY(-50%);
	background: none;
	border: none;
	color: #606770;
	cursor: pointer;
	font-size: 16px;
}

.btn-login {
	background-color: #f86ed3;
	border: none;
	border-radius: 6px;
	font-size: 20px;
	line-height: 48px;
	padding: 0 16px;
	width: 100%;
	color: #fff;
	font-weight: bold;
	margin-top: 8px;
	cursor: pointer;
	transition: background-color 0.3s;
}

.btn-login:hover {
	background-color: #e05cb6;
}

.btn-login:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

.login-links {
	display: flex;
	justify-content: space-between;
	margin: 16px 0;
}

.auth-link {
	color: #f86ed3;
	font-size: 17px;
	text-decoration: none;
}

.auth-link:hover {
	text-decoration: underline;
}

.divider {
	display: flex;
	align-items: center;
	margin: 20px 0;
}

.divider::before,
.divider::after {
	content: "";
	flex: 1;
	height: 1px;
	background-color: #dadde1;
}

.divider span {
	color: #606770;
	font-size: 14px;
	margin: 0 16px;
}

/* Google Button Styling */
.google-button-container {
	display: flex;
	justify-content: center;
	align-items: center;
	margin: 16px 0;
}

.btn-google {
	display: flex;
	align-items: center;
	background-color: white;
	border: 1px solid #dadce0;
	border-radius: 4px;
	box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
	padding: 0;
	height: 46px;
	width: 100%;
	cursor: pointer;
	transition: all 0.2s;
	margin-bottom: 8px;
	position: relative;
	overflow: hidden;
}

.btn-google:hover {
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.15);
	background-color: #f8f8f8;
}

.btn-google:active {
	background-color: #f1f1f1;
}

.btn-google:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

.google-icon-wrapper {
	display: flex;
	justify-content: center;
	align-items: center;
	width: 46px;
	height: 46px;
	background-color: white;
	border-radius: 2px;
}

.google-icon {
	width: 20px;
	height: 20px;
}

.btn-google-text {
	flex: 1;
	text-align: center;
	font-family: "Roboto", sans-serif;
	font-size: 16px;
	font-weight: 500;
	color: #3c4043;
	margin-right: 46px;
}

.error-message {
	background-color: #ffebee;
	color: #d32f2f;
	padding: 12px;
	border-radius: 6px;
	margin-bottom: 16px;
	font-size: 14px;
	text-align: center;
}

.error-text {
	color: #d32f2f;
	font-size: 12px;
	margin: 4px 0 0 4px;
}

.input-error {
	border-color: #d32f2f;
	border-width: 2px;
}

/* Responsive */
@media (max-width: 900px) {
	.login-container {
		flex-direction: column;
		text-align: center;
	}

	.logo-section {
		margin-right: 0;
		margin-bottom: 32px;
		padding-right: 0;
	}

	.logo-text {
		margin-left: auto;
		margin-right: auto;
	}

	.form-section {
		width: 100%;
		max-width: 396px;
	}
}

@media (max-width: 480px) {
	.login-card {
		box-shadow: none;
		background-color: transparent;
		padding: 0;
	}

	.logo-text {
		font-size: 32px;
	}

	.tagline {
		font-size: 20px;
		line-height: 26px;
	}
}
</style>
