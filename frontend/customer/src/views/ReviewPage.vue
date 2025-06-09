<template>
	<div class="review-page">
		<h2 class="page-title">Đánh giá sản phẩm</h2>

		<!-- Modal thông báo thành công -->
		<div v-if="showSuccessModal" class="success-modal">
			<div class="success-modal-content">
				<div class="success-icon">
					<i class="fas fa-check-circle"></i>
				</div>
				<h3>Đánh giá thành công!</h3>
				<p>
					Cảm ơn bạn đã đánh giá sản phẩm. Đánh giá của bạn sẽ giúp
					người dùng khác có lựa chọn tốt hơn.
				</p>
				<div class="success-actions">
					<button class="btn btn-secondary" @click="goToOrderHistory">
						<i class="fas fa-list-alt"></i> Quay lại đơn hàng
					</button>
					<button class="btn btn-primary" @click="viewRatedProducts">
						<i class="fas fa-eye"></i> Xem sản phẩm đã đánh giá
					</button>
				</div>
			</div>
		</div>

		<div v-if="loading" class="loading-state">
			<Loading />
		</div>

		<div v-else-if="error" class="error-state">
			<i class="fas fa-exclamation-circle"></i>
			{{
				errorMessage ||
				"Đã xảy ra lỗi khi tải thông tin đơn hàng. Vui lòng thử lại sau."
			}}
		</div>

		<div v-else class="review-container">
			<div class="order-info">
				<h3>Thông tin đơn hàng #{{ getOrderID(order?.id) }}</h3>
				<p>
					<strong>Ngày đặt:</strong>
					{{ format.formatDate(order?.orderDate) }}
				</p>
				<p>
					<strong>Tổng tiền:</strong>
					{{ format.formatPrice(order?.totalAmount) }} ₫
				</p>
			</div>

			<div class="products-section">
				<h3>Sản phẩm</h3>
				<div
					v-for="item in order?.orderItems"
					:key="item.id"
					class="product-review-card"
				>
					<div class="product-info">
						<img
							:src="productService.getUrlImage(item.colorImage)"
							:alt="item.product.name"
							class="product-thumb"
						/>
						<div class="product-details">
							<h4>{{ item.product.name }}</h4>
							<p>Màu: {{ item.colorName }}</p>
							<p>Số lượng: {{ item.quantity }}</p>
							<p>Giá: {{ format.formatPrice(item.price) }} ₫</p>
						</div>
					</div>
					<div class="review-form">
						<h4>Đánh giá của bạn</h4>

						<!-- Thông báo cảnh báo đánh giá sao -->
						<div class="rating-info" style="margin-bottom: 15px">
							<div class="rating-once-info">
								<i class="fas fa-exclamation-circle"></i>
								<span
									><strong>Lưu ý:</strong> Bạn chỉ được đánh
									giá sao cho mỗi sản phẩm một lần để đảm bảo
									hệ thống đánh giá công bằng và chính
									xác</span
								>
							</div>
						</div>

						<div class="rating">
							<span>Chọn số sao:</span>
							<div class="stars">
								<div
									class="star-container"
									@mouseleave="
										clearHoverRating(item.product.id)
									"
								>
									<template v-for="star in 5" :key="star">
										<i
											class="fas fa-star"
											:style="{
												color: getStar(
													star,
													item.product.id
												)
													? '#ffc107'
													: '#ddd',
											}"
											@click="
												setRating(item.product.id, star)
											"
											@mouseenter="
												setHoverRating(
													item.product.id,
													star
												)
											"
										></i>
									</template>
								</div>
							</div>
						</div>

						<div class="comment">
							<label for="comment">Nhận xét của bạn:</label>
							<textarea
								:id="`comment-${item.product.id}`"
								v-model="comments[item.product.id]"
								rows="4"
								placeholder="Chia sẻ trải nghiệm của bạn về sản phẩm này..."
							></textarea>
						</div>
					</div>
				</div>
			</div>

			<div class="action-buttons">
				<button @click="goBack" class="btn btn-secondary">
					Quay lại
				</button>
				<button
					@click="submitReviews"
					class="btn btn-primary"
					:disabled="submitting"
				>
					{{ submitting ? "Đang gửi..." : "Gửi đánh giá" }}
				</button>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted, reactive } from "vue";
import { useRoute, useRouter } from "vue-router";
import Loading from "@/components/common/Loading.vue";
import productService from "@/services/productService";
import format from "@/utils/format";

const route = useRoute();
const router = useRouter();
const orderId = route.params.id;

const order = ref(null);
const loading = ref(true);
const error = ref(false);
const errorMessage = ref("");
const submitting = ref(false);
const showSuccessModal = ref(false);
const ratedProductIds = ref([]);

// Store ratings and comments for each product
const ratings = reactive({});
const comments = reactive({});
const hoverRatings = reactive({}); // Lưu trạng thái hover của các ngôi sao

const getOrderID = (id) => {
	return id ? id.toString().substring(0, 8) : "";
};

const setRating = (productId, rating) => {
	ratings[productId] = rating;
};

const setHoverRating = (productId, rating) => {
	hoverRatings[productId] = rating;
};

const clearHoverRating = (productId) => {
	hoverRatings[productId] = 0;
};

// Kiểm tra xem ngôi sao có cần hiển thị màu vàng không
const getStar = (star, productId) => {
	// Nếu đang hover, hiện sao vàng từ 1 đến số sao đang hover
	if (hoverRatings[productId] > 0) {
		return star <= hoverRatings[productId];
	}
	// Nếu không hover, hiện theo đánh giá đã chọn
	return star <= ratings[productId];
};

const goBack = () => {
	router.push({ name: "orders-history" });
};

const goToOrderHistory = () => {
	router.push({ name: "orders-history" });
};

const viewRatedProducts = () => {
	// Lấy ID sản phẩm đầu tiên đã đánh giá để chuyển hướng
	if (ratedProductIds.value.length > 0) {
		const firstProductId = ratedProductIds.value[0];
		router.push({ name: "product-detail", params: { id: firstProductId } });
	} else {
		// Nếu không có sản phẩm nào được đánh giá, quay lại trang đơn hàng
		router.push({ name: "orders-history" });
	}
};

const fetchOrderDetails = async () => {
	loading.value = true;
	error.value = false;

	try {
		const response = await productService.getOrderById(orderId);

		if (response.status === "Hoàn thành") {
			order.value = response;

			// Initialize ratings and comments
			order.value.orderItems.forEach((item) => {
				ratings[item.product.id] = 0;
				comments[item.product.id] = "";
				hoverRatings[item.product.id] = 0;
			});
		} else {
			error.value = true;
			errorMessage.value =
				"Chỉ có thể đánh giá các đơn hàng đã hoàn thành.";
		}
	} catch (err) {
		console.error("Lỗi khi lấy dữ liệu đơn hàng:", err);
		error.value = true;
	} finally {
		loading.value = false;
	}
};

const submitReviews = async () => {
	submitting.value = true;

	try {
		const productReviews = Object.entries(ratings)
			.filter(([productId, rating]) => rating > 0)
			.map(([productId, rating]) => ({
				productId: parseInt(productId),
				content: comments[productId] || "",
				rating: rating,
			}));

		if (productReviews.length === 0) {
			alert("Vui lòng đánh giá ít nhất một sản phẩm");
			submitting.value = false;
			return;
		}

		// Submit each review
		for (const review of productReviews) {
			await productService.createComment(review);
			ratedProductIds.value.push(review.productId);
		}

		// Hiển thị modal thông báo thành công thay vì alert
		showSuccessModal.value = true;
	} catch (err) {
		console.error("Lỗi khi gửi đánh giá:", err);
		alert("Đã xảy ra lỗi khi gửi đánh giá. Vui lòng thử lại sau.");
		submitting.value = false;
	}
};

onMounted(() => {
	fetchOrderDetails();
});
</script>

<style scoped>
.review-page {
	max-width: 1200px;
	margin: 0 auto;
	padding: 2rem;
	color: #333;
	background-color: white;
	border-radius: 10px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.page-title {
	font-weight: 600;
	text-shadow: 1px 1px 2px rgba(248, 110, 211, 0.1);
	position: relative;
	margin-bottom: 1.5rem;
	padding-bottom: 1rem;
	border-bottom: 1px solid #eee;
}

.page-title::after {
	content: "";
	position: absolute;
	bottom: 0;
	left: 0;
	width: 7rem;
	height: 2px;
	background-color: var(--primary-color);
	border-radius: 10px;
}

.loading-state,
.error-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem;
	text-align: center;
	min-height: 300px;
}

.error-state i {
	font-size: 3rem;
	margin-bottom: 1rem;
	color: var(--danger);
}

.order-info {
	margin-bottom: 2rem;
	padding: 1rem;
	background-color: #f9f0ff;
	border-radius: 8px;
	border-left: 3px solid var(--primary-color);
}

.product-review-card {
	margin-bottom: 2rem;
	padding: 1.5rem;
	border: 1px solid #eee;
	border-radius: 8px;
	background-color: #fff;
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
	transition: all 0.3s ease;
}

.product-review-card:hover {
	box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
	transform: translateY(-2px);
}

.product-info {
	display: flex;
	margin-bottom: 1.5rem;
	padding-bottom: 1rem;
	border-bottom: 1px solid #eee;
}

.product-thumb {
	width: 120px;
	height: 120px;
	object-fit: cover;
	border-radius: 4px;
	margin-right: 1.5rem;
}

.product-details {
	flex: 1;
}

.product-details h4 {
	margin-top: 0;
	margin-bottom: 0.5rem;
	font-size: 1.1rem;
}

.review-form {
	padding-top: 1rem;
}

.rating {
	margin-bottom: 1.5rem;
}

.stars {
	margin-top: 0.5rem;
	display: flex;
}

.star-container {
	display: flex;
}

.stars i {
	font-size: 1.5rem;
	margin-right: 0.5rem;
	cursor: pointer;
	transition: color 0.2s;
}

.comment {
	margin-bottom: 1rem;
}

.comment label {
	display: block;
	margin-bottom: 0.5rem;
	font-weight: 500;
}

.comment textarea {
	width: 100%;
	padding: 0.75rem;
	border: 1px solid #ddd;
	border-radius: 4px;
	resize: vertical;
	font-family: inherit;
	transition: all 0.3s ease;
}

.comment textarea:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.2);
	outline: none;
}

/* Styling cho thông báo cảnh báo đánh giá sao */
.rating-info {
	margin-top: 5px;
	animation: fadeIn 0.8s;
}

.rating-once-info {
	display: flex;
	align-items: center;
	padding: 15px 20px;
	background-color: #fff3cd;
	border: 1px solid #ffeeba;
	border-radius: 4px;
	color: #856404;
	font-size: 14px;
	position: relative;
	overflow: hidden;
	margin-top: 10px;
	margin-bottom: 15px;
	transition: all 0.3s ease;
	width: 100%;
}

.rating-once-info:before {
	content: "";
	position: absolute;
	left: 0;
	top: 0;
	bottom: 0;
	width: 4px;
	background-color: #ffc107;
}

.rating-once-info i {
	margin-right: 10px;
	font-size: 16px;
	color: #856404;
}

@keyframes fadeIn {
	from {
		opacity: 0;
		transform: translateY(-10px);
	}
	to {
		opacity: 1;
		transform: translateY(0);
	}
}

@keyframes shimmer {
	0% {
		transform: translateX(-100%);
	}
	100% {
		transform: translateX(100%);
	}
}

/* Modal thông báo thành công */
.success-modal {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background-color: rgba(0, 0, 0, 0.5);
	display: flex;
	justify-content: center;
	align-items: center;
	z-index: 1000;
	animation: fadeIn 0.3s;
}

.success-modal-content {
	background-color: white;
	border-radius: 10px;
	padding: 2rem;
	text-align: center;
	width: 90%;
	max-width: 500px;
	box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
	animation: modalSlideIn 0.4s;
}

.success-icon {
	display: flex;
	justify-content: center;
	margin-bottom: 1rem;
}

.success-icon i {
	font-size: 4rem;
	color: #4caf50;
	animation: successPulse 2s infinite;
}

.success-modal-content h3 {
	font-size: 1.5rem;
	margin-bottom: 1rem;
	color: #333;
}

.success-modal-content p {
	color: #666;
	margin-bottom: 1.5rem;
	line-height: 1.5;
}

.success-actions {
	display: flex;
	gap: 1rem;
	justify-content: center;
}

@keyframes successPulse {
	0% {
		transform: scale(1);
	}
	50% {
		transform: scale(1.1);
	}
	100% {
		transform: scale(1);
	}
}

@keyframes modalSlideIn {
	from {
		transform: translateY(-30px);
		opacity: 0;
	}
	to {
		transform: translateY(0);
		opacity: 1;
	}
}

.action-buttons {
	display: flex;
	justify-content: space-between;
	margin-top: 2rem;
}

.btn {
	padding: 0.75rem 1.5rem;
	border: none;
	border-radius: 4px;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.3s ease;
}

.btn:active {
	transform: translateY(1px);
}

.btn-primary {
	background-color: var(--primary-color);
	color: white;
}

.btn-primary:hover {
	background-color: var(--hover-color);
	transform: translateY(-2px);
}

.btn-primary:disabled {
	background-color: #9e9e9e;
	cursor: not-allowed;
	transform: none;
}

.btn-secondary {
	background-color: #e0e0e0;
	color: #333;
}

.btn-secondary:hover {
	background-color: #d5d5d5;
	transform: translateY(-2px);
}

@media (max-width: 768px) {
	.product-info {
		flex-direction: column;
	}

	.product-thumb {
		margin-right: 0;
		margin-bottom: 1rem;
		width: 100%;
		height: auto;
		max-height: 200px;
	}

	.action-buttons {
		flex-direction: column;
		gap: 1rem;
	}

	.btn {
		width: 100%;
	}

	.success-actions {
		flex-direction: column;
	}

	.success-modal-content {
		padding: 1.5rem;
	}
}
</style>
