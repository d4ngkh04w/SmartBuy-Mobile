<template>
	<div class="comments-section">
		<!-- Rating Summary -->
		<div
			class="reviews-summary"
			v-if="comments.length > 0"
			:key="`rating-${productRating.average}-${productRating.total}`"
		>
			<div class="average-rating">
				<div class="rating-number">
					{{
						productRating.total > 0
							? averageRating.toFixed(1)
							: "0.0"
					}}
				</div>
				<div class="stars" v-if="productRating.total > 0">
					<i
						v-for="i in 5"
						:key="i"
						:class="getStarClass(i, averageRating)"
						:style="{
							fontSize: '20px',
							color:
								i <= Math.floor(averageRating) ||
								(i === Math.ceil(averageRating) &&
									averageRating % 1 >= 0.5)
									? '#ffc107'
									: '#ccc',
						}"
					></i>
				</div>
				<div class="no-rating-stars" v-else>
					<span>Chưa có đánh giá</span>
				</div>
				<div class="total-reviews">
					{{ productRating.total }} đánh giá
				</div>
			</div>

			<div class="rating-bars">
				<div class="rating-bar" v-for="star in 5" :key="star">
					<div class="star-label">{{ star }} sao</div>
					<div class="bar-container">
						<div
							class="bar-fill"
							:style="{ width: ratingPercentages[star] + '%' }"
						></div>
					</div>
					<div class="percentage">
						{{ ratingPercentages[star].toFixed(0) }}%
					</div>
				</div>
			</div>
		</div>
		<!-- Filter Section -->
		<div class="filter-section">
			<div class="filter-options">
				<button
					class="filter-option all-filter"
					:class="{ active: currentFilter === 'all' }"
					@click="filterComments('all')"
				>
					Tất cả ({{ productRating.total }})</button
				><button
					v-for="star in 5"
					:key="star"
					class="filter-option"
					:class="{ active: currentFilter === star }"
					@click="filterComments(star)"
				>
					<div class="filter-stars">
						<i v-for="i in star" :key="i" class="fas fa-star"></i>
						<i
							v-for="i in 5 - star"
							:key="i + star"
							class="far fa-star"
						></i>
					</div>
					<span>({{ getStarCount(star) }})</span>
				</button>
			</div>
		</div>
		<!-- Add Comment Form for logged-in users -->
		<div v-if="isLoggedIn" class="add-comment-form">
			<h4>Viết bình luận của bạn</h4>

			<!-- Thông báo cảnh báo đánh giá sao -->
			<div class="rating-info" style="margin-bottom: 15px">
				<div class="rating-once-info">
					<i class="fas fa-exclamation-circle"></i>
					<span
						><strong>Lưu ý:</strong> Bạn chỉ được đánh giá sao cho
						mỗi sản phẩm một lần để đảm bảo hệ thống đánh giá công
						bằng và chính xác</span
					>
				</div>
			</div>

			<div v-if="userHasRated" class="user-rated-alert">
				<i class="fas fa-check-circle"></i>
				<span>
					<strong>Đã đánh giá:</strong> Bạn đã đánh giá sao cho sản
					phẩm này. Bạn vẫn có thể thêm hoặc chỉnh sửa nội dung bình
					luận của mình.
				</span>
			</div>

			<div class="rating-selector">
				<span
					>Đánh giá của bạn<span v-if="newComment.content.trim()">
						(tùy chọn)</span
					>:</span
				>
				<div
					class="star-rating"
					:class="{ disabled: userHasRated }"
					:title="
						userHasRated ? 'Bạn đã đánh giá sản phẩm này rồi' : ''
					"
				>
					<i
						v-for="star in 5"
						:key="star"
						:class="
							star <= (hoverRating || newComment.rating)
								? 'fas fa-star'
								: 'far fa-star'
						"
						@click="
							userHasRated
								? showRatingExistsWarning()
								: (newComment.rating = star)
						"
						@mouseover="userHasRated ? null : (hoverRating = star)"
						@mouseleave="userHasRated ? null : (hoverRating = 0)"
						:style="{
							fontSize: '28px',
							color:
								star <= (hoverRating || newComment.rating)
									? '#ffc107'
									: '#ccc',
							marginRight: '8px',
							cursor: userHasRated ? 'not-allowed' : 'pointer',
							transition: 'color 0.2s, transform 0.1s',
							opacity: userHasRated ? '0.7' : '1',
						}"
					></i>
					<button
						v-if="newComment.rating > 0"
						class="clear-rating-btn"
						@click="clearRating"
						title="Xóa đánh giá sao"
					>
						<i class="fas fa-times"></i>
					</button>
				</div>
				<div v-if="newComment.rating > 0" class="rating-hint">
					<small
						>Bấm <i class="fas fa-times"></i> để xóa đánh giá
						sao</small
					>
				</div>
			</div>
			<textarea
				v-model="newComment.content"
				placeholder="Nhập nội dung đánh giá của bạn... (tùy chọn nếu đã đánh giá sao)"
				rows="3"
			></textarea
			><button
				class="submit-comment-btn"
				@click="submitComment"
				:disabled="!newComment.content.trim() && !newComment.rating"
			>
				<i class="fas fa-paper-plane"></i> Gửi
			</button>
		</div> 
		<!-- Login message for non-logged-in users -->
		<div v-else class="login-message">
			<i class="fas fa-user-lock login-icon"></i>
			<p>
				Vui lòng
				<a href="/login" @click.prevent="redirectToLogin">đăng nhập</a>
				để bình luận.
			</p>
			<button class="login-btn" @click="redirectToLogin">
				<i class="fas fa-sign-in-alt"></i> Đăng nhập ngay
			</button>
		</div>

		<!-- Comments List -->
		<div v-if="displayedComments.length > 0" class="comments-list">
			<div
				v-for="comment in displayedComments"
				:key="comment.id"
				class="comment-item"
			>
				<div class="comment-header">
					<img
						:src="
							comment.userAvatar
								? apiBaseUrl + comment.userAvatar
								: defaultAvatar
						"
						:alt="comment.userName"
						class="comment-avatar"
					/>
					<div class="comment-user">
						<span class="user-name">{{
							comment.userName ||
							getDefaultUsername(comment.id, comment.userId)
						}}</span>
						<div class="comment-rating" v-if="comment.rating">
							<i
								v-for="star in 5"
								:key="star"
								:class="getStarClass(star, comment.rating)"
								:style="{
									fontSize: '16px',
									marginRight: '2px',
									color:
										star <= Math.floor(comment.rating) ||
										(star === Math.ceil(comment.rating) &&
											comment.rating % 1 >= 0.5)
											? '#ffc107'
											: '#ccc',
								}"
							></i>
						</div>
					</div>
					<span class="comment-date">{{
						comment.createdAtString
					}}</span>
				</div>
				<div class="comment-content">{{ comment.content }}</div>

				<div class="comment-actions" v-if="isLoggedIn">
					<button
						class="reply-btn"
						@click="showReplyForm(comment.id)"
						v-if="!comment.isReplying"
					>
						<i class="fas fa-reply"></i> Trả lời
					</button>
					<div
						v-if="
							isLoggedIn &&
							currentUserId &&
							comment.userId === currentUserId
						"
						class="owner-actions"
					>
						<button class="edit-btn" @click="editComment(comment)">
							<i class="fas fa-edit"></i> Sửa
						</button>
						<button
							class="delete-btn"
							@click="deleteCommentConfirm(comment.id)"
						>
							<i class="fas fa-trash"></i> Xóa
						</button>
					</div>
				</div>

				<!-- Reply Form -->
				<div v-if="comment.isReplying" class="reply-form">
					<textarea
						v-model="replyContent"
						placeholder="Nhập phản hồi của bạn..."
						rows="2"
					></textarea>
					<div class="reply-buttons">
						<button
							class="submit-reply-btn"
							@click="submitReply(comment.id)"
						>
							Gửi
						</button>
						<button class="cancel-reply-btn" @click="cancelReply">
							Hủy
						</button>
					</div>
				</div>
				<!-- Edit Form -->
				<div v-if="comment.isEditing" class="edit-form">
					<div
						class="rating-info"
						v-if="!comment.parentId"
						style="margin-bottom: 15px"
					>
						<div class="rating-once-info">
							<i class="fas fa-exclamation-circle"></i>
							<span
								><strong>Lưu ý:</strong> Bạn chỉ được đánh giá
								sao cho mỗi sản phẩm một lần để đảm bảo hệ thống
								đánh giá công bằng và chính xác</span
							>
						</div>
					</div>
					<textarea v-model="editContent" rows="2"></textarea>
					<div class="edit-rating" v-if="!comment.parentId">
						<span>Đánh giá:</span>
						<div class="star-rating">
							<i
								v-for="star in 5"
								:key="star"
								:class="
									star <= (hoverEditRating || editRating)
										? 'fas fa-star'
										: 'far fa-star'
								"
								@click="editRating = star"
								@mouseover="hoverEditRating = star"
								@mouseleave="hoverEditRating = 0"
								:style="{
									fontSize: '24px',
									color:
										star <= (hoverEditRating || editRating)
											? '#ffc107'
											: '#ccc',
									marginRight: '8px',
									cursor: 'pointer',
									transition: 'color 0.2s, transform 0.1s',
								}"
							></i>
							<button
								v-if="editRating > 0"
								class="clear-rating-btn"
								@click="clearEditRating"
								title="Xóa đánh giá sao"
							>
								<i class="fas fa-times"></i>
							</button>
						</div>
						<div v-if="editRating > 0" class="rating-hint">
							<small
								>Bấm <i class="fas fa-times"></i> để xóa đánh
								giá sao</small
							>
						</div>
					</div>
					<div class="edit-buttons">
						<button
							class="submit-edit-btn"
							@click="submitEdit(comment.id)"
						>
							Lưu
						</button>
						<button class="cancel-edit-btn" @click="cancelEdit">
							Hủy
						</button>
					</div>
				</div>

				<!-- Replies -->
				<div
					v-if="comment.replies && comment.replies.length > 0"
					class="replies-section"
				>
					<div
						v-for="reply in comment.replies"
						:key="reply.id"
						class="reply-item"
					>
						<div class="reply-header">
							<img
								:src="
									reply.userAvatar
										? apiBaseUrl + reply.userAvatar
										: defaultAvatar
								"
								:alt="reply.userName"
								class="reply-avatar"
							/>
							<div class="reply-user">
								<span class="user-name">{{
									reply.userName ||
									getDefaultUsername(reply.id, reply.userId)
								}}</span>
							</div>
							<span class="reply-date">{{
								reply.createdAtString
							}}</span>
						</div>
						<div class="reply-content">{{ reply.content }}</div>
						<!-- Reply Actions -->
						<div
							class="reply-actions"
							v-if="
								isLoggedIn &&
								currentUserId &&
								reply.userId === currentUserId
							"
						>
							<button
								class="edit-btn"
								@click="editComment(reply)"
							>
								<i class="fas fa-edit"></i> Sửa
							</button>
							<button
								class="delete-btn"
								@click="deleteCommentConfirm(reply.id)"
							>
								<i class="fas fa-trash"></i> Xóa
							</button>
						</div>

						<!-- Reply Edit Form -->
						<div v-if="reply.isEditing" class="edit-form">
							<textarea v-model="editContent" rows="2"></textarea>
							<div class="edit-buttons">
								<button
									class="submit-edit-btn"
									@click="submitEdit(reply.id)"
								>
									Lưu
								</button>
								<button
									class="cancel-edit-btn"
									@click="cancelEdit"
								>
									Hủy
								</button>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<!-- No Comments Message -->
		<div v-else-if="comments.length === 0" class="no-comments">
			<p>
				Chưa có đánh giá nào cho sản phẩm này. Hãy là người đầu tiên
				đánh giá!
			</p>
		</div>
		<div
			v-else-if="displayedComments.length === 0"
			class="no-filtered-comments"
		>
			<p>Không có đánh giá nào ở mức {{ currentFilter }} sao.</p>
		</div>

		<!-- Pagination -->
		<div class="pagination-container" v-if="totalPages > 1">
			<button
				class="pagination-btn"
				@click="changePage(currentPage - 1)"
				:disabled="currentPage === 1"
			>
				<i class="fas fa-chevron-left"></i>
			</button>

			<button
				v-for="page in displayedPages"
				:key="page"
				class="pagination-btn"
				:class="{ active: page === currentPage }"
				@click="changePage(page)"
			>
				{{ page }}
			</button>

			<button
				class="pagination-btn"
				@click="changePage(currentPage + 1)"
				:disabled="currentPage === totalPages"
			>
				<i class="fas fa-chevron-right"></i>
			</button>
		</div>

		<!-- Confirmation Dialog -->
		<div v-if="showConfirmDialog" class="confirm-dialog">
			<div class="confirm-dialog-content">
				<h3>Xác nhận xóa</h3>
				<p>Bạn có chắc chắn muốn xóa đánh giá này?</p>
				<div class="confirm-buttons">
					<button class="confirm-yes" @click="confirmDelete">
						Xóa
					</button>
					<button class="confirm-no" @click="cancelDelete">
						Hủy
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import commentService from "@/services/commentService";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";
import MD5 from "crypto-js/md5";

const apiBaseUrl = import.meta.env.VITE_API_URL.replace("/api/v1", "");

const props = defineProps({
	productId: {
		type: Number,
		required: true,
	},
	isLoggedIn: {
		type: Boolean,
		default: false,
	},
});

const emit = defineEmits(["ratingUpdated"]);

const router = useRouter();

// State variables
const comments = ref([]);
const totalItems = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const currentFilter = ref("all");
const defaultAvatar = ref(
	"https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg"
);
const showLoginMessage = ref(false);
// User reactions state removed
const isLoading = ref(true);
const currentUserId = ref(null); // Add this line to define currentUserId
const productRating = ref({
	average: 0,
	total: 0,
	distribution: {
		1: 0,
		2: 0,
		3: 0,
		4: 0,
		5: 0,
	},
});

// New comment form
const newComment = ref({
	content: "",
	rating: 0,
});
const hoverRating = ref(0);
const userHasRated = ref(false); // Add this to track if user has already rated

// Reply state
const replyContent = ref("");
const activeReplyId = ref(null);

// Check if user has already rated this product when component mounts
async function checkUserRatingStatus() {
	if (!props.isLoggedIn || !currentUserId.value) return;

	try {
		userHasRated.value = await commentService.hasUserRatedProduct(
			props.productId
		);
		console.log("User has already rated:", userHasRated.value);

		// Show notification if user has already rated
		// if (userHasRated.value) {
		// 	// Use setTimeout to ensure this appears after page load
		// 	setTimeout(() => {
		// 		emitter.emit("show-notification", {
		// 			status: "warning",
		// 			message:
		// 				"Bạn đã đánh giá sản phẩm này rồi! Bạn vẫn có thể thêm hoặc chỉnh sửa bình luận.",
		// 			duration: 5000,
		// 		});
		// 	}, 1000);
		// }
	} catch (error) {
		console.error("Error checking user rating status:", error);
	}
}

// Edit state
const editContent = ref("");
const editRating = ref(0);
const hoverEditRating = ref(0);
const activeEditId = ref(null);

// Delete confirmation
const showConfirmDialog = ref(false);
const commentToDelete = ref(null);

// Filtered comments
const displayedComments = computed(() => {
	if (currentFilter.value === "all") {
		return comments.value;
	}
	return comments.value.filter(
		(comment) => comment.rating === currentFilter.value
	);
});

// Star rating percentages
const ratingPercentages = computed(() => {
	const result = {};
	const total = productRating.value.total;

	if (total === 0) {
		for (let i = 1; i <= 5; i++) {
			result[i] = 0;
		}
		return result;
	}

	for (let i = 1; i <= 5; i++) {
		result[i] = (productRating.value.distribution[i] / total) * 100;
	}

	return result;
});

// Tính toán lớp CSS cho hiển thị sao dựa trên điểm đánh giá
function getStarClass(position, rating) {
	if (position <= Math.floor(rating)) {
		// Sao đầy đủ khi vị trí <= phần nguyên của rating
		return "fas fa-star";
	} else if (position === Math.ceil(rating) && rating % 1 >= 0.5) {
		// Nửa sao khi vị trí = phần nguyên + 1 và phần thập phân >= 0.5
		return "fas fa-star-half-alt";
	} else {
		// Sao trống
		return "far fa-star";
	}
}

// Average rating
const averageRating = computed(() => {
	return productRating.value.average;
});

// Pagination display logic
const displayedPages = computed(() => {
	const pages = [];
	const maxVisible = 5;

	if (totalPages.value <= maxVisible) {
		for (let i = 1; i <= totalPages.value; i++) {
			pages.push(i);
		}
	} else {
		const halfMax = Math.floor(maxVisible / 2);

		if (currentPage.value <= halfMax + 1) {
			for (let i = 1; i <= maxVisible; i++) {
				pages.push(i);
			}
		} else if (currentPage.value >= totalPages.value - halfMax) {
			for (
				let i = totalPages.value - maxVisible + 1;
				i <= totalPages.value;
				i++
			) {
				pages.push(i);
			}
		} else {
			for (
				let i = currentPage.value - halfMax;
				i <= currentPage.value + halfMax;
				i++
			) {
				pages.push(i);
			}
		}
	}

	return pages;
});

// Methods
function getStarCount(star) {
	return productRating.value.distribution[star] || 0;
}

function getDefaultUsername(id, userId) {
	// Use userId if available (better for uniqueness), otherwise use comment id
	const sourceId = userId ? userId.toString() : id.toString();

	const hash = MD5(sourceId).toString().slice(0, 6); // lấy 6 ký tự đầu
	return `Người dùng ${hash}`;
}

async function fetchProductRating(forceRefresh = false) {
	try {
		console.log(
			`Fetching product rating for product ${props.productId}, forceRefresh: ${forceRefresh}`
		);
		const response = await commentService.getProductAverageRating(
			props.productId,
			forceRefresh
		);
		if (response && response.data) {
			// Transform API response to match our component's expected structure
			const ratingDistribution = {
				1: 0,
				2: 0,
				3: 0,
				4: 0,
				5: 0,
			};

			// Extract rating counts from the new API structure
			if (response.data.ratingDistribution) {
				// Our new API returns object with objects that have count and percentage
				Object.entries(response.data.ratingDistribution).forEach(
					([rating, details]) => {
						ratingDistribution[rating] = details.count || 0;
					}
				);
			}

			const newRatingData = {
				average: response.data.averageRating || 0,
				total: response.data.totalRatings || 0,
				distribution: ratingDistribution,
			};

			console.log("Updated rating data:", newRatingData);
			productRating.value = newRatingData;

			// Emit event to parent component
			emit("ratingUpdated", newRatingData);
		}
	} catch (error) {
		console.error("Error fetching product rating:", error);
		// Không hiển thị thông báo lỗi cho người dùng khi API này thất bại
		// Thay vào đó, đảm bảo hiển thị giá trị mặc định
		const defaultRating = {
			average: 0,
			total: 0,
			distribution: {
				1: 0,
				2: 0,
				3: 0,
				4: 0,
				5: 0,
			},
		};
		productRating.value = defaultRating;

		// Emit default rating to parent component
		emit("ratingUpdated", defaultRating);
	}
}

async function fetchComments() {
	try {
		isLoading.value = true;
		const response = await commentService.getCommentsByProductId(
			props.productId,
			currentPage.value,
			pageSize.value
		);
		comments.value = response.data.comments.map((comment) => ({
			...comment,
			isReplying: false,
			isEditing: false,
		}));
		totalItems.value = response.data.totalItems;
		totalPages.value = response.data.totalPages;
		isLoading.value = false;
	} catch (error) {
		console.error("Error fetching comments:", error);
		isLoading.value = false;
	}
}

// This function is already defined above

function filterComments(filter) {
	currentFilter.value = filter;
	if (filter !== "all") {
		currentPage.value = 1;
	}
}

function changePage(page) {
	if (page < 1 || page > totalPages.value) return;
	currentPage.value = page;
	fetchComments();
}

function redirectToLogin() {
	// Store the current URL to redirect back after login
	localStorage.setItem("redirectAfterLogin", window.location.pathname);
	router.push("/login");
}

function clearRating() {
	newComment.value.rating = 0;
	hoverRating.value = 0;
}

function showRatingExistsWarning() {
	emitter.emit("show-notification", {
		status: "warning",
		message:
			"Bạn đã đánh giá sao cho sản phẩm này rồi. Mỗi người dùng chỉ được đánh giá sao một lần cho mỗi sản phẩm.",
		duration: 4000,
	});
}

function showReplyForm(commentId) {
	comments.value = comments.value.map((c) => ({
		...c,
		isReplying: c.id === commentId,
	}));
	replyContent.value = "";
	activeReplyId.value = commentId;
}

function cancelReply() {
	comments.value = comments.value.map((c) => ({
		...c,
		isReplying: false,
	}));
	replyContent.value = "";
	activeReplyId.value = null;
}

function editComment(comment) {
	// Reset all other comments' editing state
	comments.value = comments.value.map((c) => {
		if (c.id === comment.id) {
			return {
				...c,
				isEditing: true,
			};
		}

		if (c.replies) {
			c.replies = c.replies.map((r) => {
				if (r.id === comment.id) {
					return {
						...r,
						isEditing: true,
					};
				}
				return {
					...r,
					isEditing: false,
				};
			});
		}

		return {
			...c,
			isEditing: false,
		};
	});

	editContent.value = comment.content;
	editRating.value = comment.rating || 0;
	activeEditId.value = comment.id;
}

function clearEditRating() {
	editRating.value = 0;
	hoverEditRating.value = 0;
}

function cancelEdit() {
	// Reset all edit states
	comments.value = comments.value.map((c) => {
		const updatedComment = {
			...c,
			isEditing: false,
		};

		if (updatedComment.replies) {
			updatedComment.replies = updatedComment.replies.map((r) => ({
				...r,
				isEditing: false,
			}));
		}

		return updatedComment;
	});

	editContent.value = "";
	editRating.value = 0;
	activeEditId.value = null;
}

// Helper function to find a comment by its ID
function findCommentById(commentId) {
	// First check in top-level comments
	const topLevelComment = comments.value.find((c) => c.id === commentId);
	if (topLevelComment) {
		return topLevelComment;
	}

	// If not found, check in replies
	for (const comment of comments.value) {
		if (comment.replies) {
			const reply = comment.replies.find((r) => r.id === commentId);
			if (reply) {
				return reply;
			}
		}
	}

	return null;
}

function deleteCommentConfirm(commentId) {
	commentToDelete.value = commentId;
	showConfirmDialog.value = true;
}

function cancelDelete() {
	commentToDelete.value = null;
	showConfirmDialog.value = false;
}

async function confirmDelete() {
	if (commentToDelete.value) {
		try {
			await commentService.deleteComment(commentToDelete.value);

			// Remove the comment from the list
			const deletedCommentId = commentToDelete.value;

			// Check if it's a top-level comment
			const topLevelComment = comments.value.find(
				(c) => c.id === deletedCommentId
			);

			if (topLevelComment) {
				comments.value = comments.value.filter(
					(c) => c.id !== deletedCommentId
				);
			} else {
				// It's a reply, find parent and remove from replies
				comments.value = comments.value.map((c) => {
					if (
						c.replies &&
						c.replies.some((r) => r.id === deletedCommentId)
					) {
						return {
							...c,
							replies: c.replies.filter(
								(r) => r.id !== deletedCommentId
							),
						};
					}
					return c;
				});
			}

			// Update total counts
			totalItems.value -= 1;

			// Close the dialog
			showConfirmDialog.value = false;
			commentToDelete.value = null;

			// Update product rating after deleting comment
			if (topLevelComment && topLevelComment.rating > 0) {
				fetchProductRating();
			}

			// Show success message
			emitter.emit("show-notification", {
				status: "success",
				message: "Đã xóa bình luận thành công",
			});
		} catch (error) {
			console.error("Error deleting comment:", error);
			emitter.emit("show-notification", {
				status: "error",
				message: "Không thể xóa bình luận. Vui lòng thử lại sau.",
			});
		}
	}
}

async function submitComment() {
	if (!props.isLoggedIn) {
		emitter.emit("show-notification", {
			status: "warning",
			message: "Vui lòng đăng nhập để bình luận",
		});
		redirectToLogin();
		return;
	}
	try {
		// Check if user has a rating and needs purchase verification
		if (newComment.value.rating > 0) {
			try {
				// First check if user has already rated this product
				const hasRated = await commentService.hasUserRatedProduct(
					props.productId
				);
				if (hasRated) {
					emitter.emit("show-notification", {
						status: "warning",
						message:
							"Bạn đã đánh giá sao cho sản phẩm này rồi. Mỗi người dùng chỉ được đánh giá sao một lần cho mỗi sản phẩm.",
						duration: 6000, // Show for longer (6 seconds)
						position: "top-center", // Show at the top center for better visibility
					});
					// Clear the rating but keep the content so user can still comment without rating
					newComment.value.rating = 0;
					hoverRating.value = 0;
					return;
				}

				const hasPurchased =
					await commentService.hasUserPurchasedProduct(
						props.productId
					);
				if (!hasPurchased) {
					emitter.emit("show-notification", {
						status: "warning",
						message:
							"Bạn cần mua sản phẩm này trước khi đánh giá. Bạn vẫn có thể bình luận mà không đánh giá sao.",
					});
					return;
				}
			} catch (error) {
				console.error(
					"Error checking purchase or rating status:",
					error
				);
				// Display the specific error message from the backend if available
				const errorMessage =
					error.response?.data?.message ||
					"Có lỗi xảy ra khi kiểm tra trạng thái đánh giá. Vui lòng thử lại sau.";

				emitter.emit("show-notification", {
					status: "error",
					message: errorMessage,
					duration: 6000, // Show for longer (6 seconds)
				});

				// Clear the rating to prevent further errors
				newComment.value.rating = 0;
				hoverRating.value = 0;
				return;
			}
		}
		// Cho phép tạo bình luận nếu có nội dung hoặc có đánh giá sao
		if (newComment.value.content.trim() || newComment.value.rating) {
			const result = await commentService.createComment(
				props.productId,
				newComment.value.content,
				newComment.value.rating || null
			);

			// Add the new comment to the list
			if (result.data) {
				comments.value.unshift({
					...result.data,
					isReplying: false,
					isEditing: false,
					replies: [],
				});

				totalItems.value += 1; // Reset form
				newComment.value = {
					content: "",
					rating: 0,
				};
				hoverRating.value = 0;
				emitter.emit("show-notification", {
					status: "success",
					message:
						"Đã đăng bình luận thành công! Cảm ơn bạn đã bình luận về sản phẩm.",
				});

				// Update product rating after new comment with a longer delay to ensure backend processing
				console.log(
					"Scheduling rating refresh after comment submission"
				);
				setTimeout(() => {
					console.log("Refreshing rating data after timeout");
					fetchProductRating(true); // Force refresh
				}, 1000);
			}
		}
	} catch (error) {
		console.error("Error submitting comment:", error);
		// Extract specific error message from the backend response if available
		const errorMessage =
			error.response?.data?.message ||
			"Không thể đăng bình luận. Vui lòng thử lại sau.";

		emitter.emit("show-notification", {
			status: "error",
			message: errorMessage,
			duration: 6000, // Show for longer (6 seconds)
		});
	}
}

async function submitReply(parentId) {
	if (!props.isLoggedIn) {
		showLoginMessage.value = true;
		return;
	}

	try {
		if (replyContent.value.trim()) {
			const result = await commentService.createComment(
				props.productId,
				replyContent.value,
				null,
				parentId
			);

			// Add the reply to the parent comment
			if (result.data) {
				comments.value = comments.value.map((c) => {
					if (c.id === parentId) {
						return {
							...c,
							isReplying: false,
							replies: [
								...(c.replies || []),
								{
									...result.data,
									isEditing: false,
								},
							],
						};
					}
					return c;
				});

				// Reset form
				replyContent.value = "";
				activeReplyId.value = null;
				emitter.emit("show-notification", {
					status: "success",
					message: "Đã trả lời bình luận thành công",
				});
			}
		}
	} catch (error) {
		console.error("Error submitting reply:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể trả lời bình luận. Vui lòng thử lại sau.",
		});
	}
}

async function submitEdit(commentId) {
	try {
		// Find the current comment to check if it had a rating before
		const currentComment = findCommentById(commentId);
		const hadRatingBefore = currentComment && currentComment.rating > 0;

		// Check if user is trying to add or change a rating
		if (editRating.value > 0) {
			// If the comment didn't have a rating before and user is trying to add one now
			if (!hadRatingBefore) {
				try {
					// First check if user has already rated this product in another comment					const hasRated = await commentService.hasUserRatedProduct(props.productId);
					if (hasRated) {
						emitter.emit("show-notification", {
							status: "warning",
							message:
								"Bạn đã đánh giá sao cho sản phẩm này trong một bình luận khác. Mỗi người dùng chỉ được đánh giá sao một lần cho mỗi sản phẩm.",
							duration: 6000, // Show for longer (6 seconds)
							position: "top-center", // Show at the top center for better visibility
						});
						// Clear the rating but keep the content so user can still edit without rating
						editRating.value = 0;
						hoverEditRating.value = 0;
						return;
					}
				} catch (error) {
					console.error("Error checking rating status:", error);
				}
			}

			try {
				const hasPurchased =
					await commentService.hasUserPurchasedProduct(
						props.productId
					);
				if (!hasPurchased) {
					emitter.emit("show-notification", {
						status: "warning",
						message:
							"Bạn cần mua sản phẩm này trước khi đánh giá. Bạn vẫn có thể bình luận mà không đánh giá sao.",
					});
					return;
				}
			} catch (error) {
				console.error("Error checking purchase status:", error);
			}
		}
		const result = await commentService.updateComment(
			commentId,
			editContent.value,
			editRating.value || null
		);

		if (result.data) {
			// Update the comment in the list
			let commentFound = false;
			let ratingChanged = false;
			let isTopLevelComment = false;

			// Check if it's a top-level comment
			comments.value = comments.value.map((c) => {
				if (c.id === commentId) {
					commentFound = true;
					isTopLevelComment = true;
					// Check if rating changed
					if (c.rating !== result.data.rating) {
						ratingChanged = true;
					}
					return {
						...c,
						content: result.data.content,
						rating: result.data.rating,
						isEditing: false,
					};
				}

				// Check if it's in replies
				if (c.replies) {
					const updatedReplies = c.replies.map((r) => {
						if (r.id === commentId) {
							commentFound = true;
							return {
								...r,
								content: result.data.content,
								isEditing: false,
							};
						}
						return r;
					});

					return {
						...c,
						replies: updatedReplies,
					};
				}

				return c;
			});

			// Reset edit form
			editContent.value = "";
			editRating.value = 0;
			activeEditId.value = null;

			// Update product rating if the comment's rating has changed
			if (commentFound && isTopLevelComment && ratingChanged) {
				fetchProductRating();
			}

			if (commentFound) {
				emitter.emit("show-notification", {
					status: "success",
					message: "Đã cập nhật bình luận thành công",
				});
			}
		}
	} catch (error) {
		console.error("Error updating comment:", error);
		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể cập nhật bình luận. Vui lòng thử lại sau.",
		});
	}
}

// Reaction function removed

// Watchers
watch(
	() => props.productId,
	() => {
		currentPage.value = 1;
		currentFilter.value = "all";
		fetchComments();
		fetchProductRating(true);
	}
);

// Watch for login state changes
watch(
	() => props.isLoggedIn,
	async (newValue) => {
		if (newValue && !currentUserId.value) {
			// User just logged in, fetch their ID
			try {
				const userInfo = await meService.getMe();
				if (userInfo && userInfo.id) {
					currentUserId.value = userInfo.id;
					localStorage.setItem("userData", JSON.stringify(userInfo));
				}
			} catch (error) {
				console.error("Error fetching user data after login:", error);
			}
		} else if (!newValue) {
			// User logged out
			currentUserId.value = null;
		}
	}
);

// Initialize
onMounted(async () => {
	fetchComments();
	fetchProductRating(true);

	// Try to get user ID from localStorage first (faster)
	try {
		// In some applications, user data might be stored in localStorage
		const userData = localStorage.getItem("userData");
		if (userData) {
			const parsedUserData = JSON.parse(userData);
			if (parsedUserData && parsedUserData.id) {
				currentUserId.value = parsedUserData.id;
			}
		}

		// If we're logged in but don't have the ID yet, fetch it from the API
		if (props.isLoggedIn && !currentUserId.value) {
			try {
				const userInfo = await meService.getMe();
				if (userInfo && userInfo.id) {
					currentUserId.value = userInfo.id;
					// Optionally store for future use
					localStorage.setItem("userData", JSON.stringify(userInfo));
				}
			} catch (error) {
				console.error("Error fetching user data:", error);
			}
		}

		// Check if user has already rated this product
		if (props.isLoggedIn && currentUserId.value) {
			await checkUserRatingStatus();
		}
	} catch (error) {
		console.error("Error processing user data:", error);
	}

	// Check user rating status on mount
	checkUserRatingStatus();
});
</script>

<style scoped>
.comments-section {
	margin: 0px 0;
	position: relative;
}

/* Rating Summary Styles */
.reviews-summary {
	display: flex;
	gap: 40px;
	margin-bottom: 30px;
	padding: 20px;
	background-color: #f9f9f9;
	border-radius: 8px;
}

.average-rating {
	display: flex;
	flex-direction: column;
	align-items: center;
	min-width: 150px;
}

.rating-number {
	font-size: 48px;
	font-weight: 700;
	color: var(--primary-color); /* Sử dụng màu chính từ biến CSS */
	line-height: 1;
	margin-bottom: 8px;
}

.stars {
	margin-bottom: 8px;
	display: flex;
	justify-content: center;
	align-items: center;
}

.stars .fa-star,
.stars .fa-star-half-alt {
	color: #ffc107; /* Màu vàng cho sao đã chọn và nửa sao */
}

.stars .far.fa-star {
	color: #ccc; /* Màu xám cho sao chưa chọn */
}

.no-rating-stars {
	height: 25px;
	display: flex;
	align-items: center;
	margin-bottom: 8px;
	color: #999;
	font-style: italic;
	font-size: 14px;
}

.total-reviews {
	font-size: 14px;
	color: #666;
}

.rating-bars {
	flex: 1;
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.rating-bar {
	display: flex;
	align-items: center;
	gap: 10px;
}

.star-label {
	width: 60px;
	font-size: 14px;
	color: #666;
}

.bar-container {
	flex: 1;
	height: 8px;
	background-color: #e0e0e0;
	border-radius: 4px;
	overflow: hidden;
}

.bar-fill {
	height: 100%;
	background-color: var(--primary-color);
	border-radius: 4px;
}

.percentage {
	width: 40px;
	font-size: 14px;
	color: #666;
}

/* Filter Styles */
.filter-section {
	margin-bottom: 30px;
	margin-top: 20px;
}

.filter-options {
	display: flex;
	flex-wrap: wrap;
	gap: 12px;
}

.filter-option {
	padding: 10px 18px;
	border: 1px solid #ddd;
	border-radius: 25px;
	background: none;
	cursor: pointer;
	transition: all 0.3s;
	font-weight: 500;
	font-size: 14px;
	display: flex;
	align-items: center;
	gap: 5px;
}

.filter-option:hover {
	background-color: #f5f5f5;
	border-color: #ccc;
	transform: translateY(-2px);
	box-shadow: 0 4px 8px rgba(0, 0, 0, 0.08);
	transition: all 0.3s ease;
}

.filter-option.active {
	background-color: var(--primary-color);
	color: white;
	border-color: var(--primary-color);
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.filter-option.active .fa-star,
.filter-option.active .far.fa-star {
	color: #fff !important;
}

.filter-stars {
	display: flex;
	align-items: center;
	margin-right: 5px;
}

.filter-stars .fa-star {
	color: #ffc107;
	font-size: 14px;
}

.filter-stars .far.fa-star {
	color: #ccc;
	font-size: 14px;
}

/* Đảm bảo nút "Tất cả" không có sao */
/* Ensure consistent width for star filters */
.filter-option {
	min-width: 100px;
	justify-content: center;
}

/* Special styling for "All" button */
.filter-option.all-filter {
	min-width: 70px;
}

/* Comment Form Styles */
.add-comment-form {
	background-color: #f9f9f9;
	padding: 20px;
	border-radius: 8px;
	margin-bottom: 30px;
}

.add-comment-form h4 {
	margin-bottom: 15px;
	font-size: 18px;
}

.user-rated-alert {
	display: flex;
	align-items: center;
	margin-bottom: 15px;
	padding: 12px 15px;
	border-radius: 8px;
	background-color: #e7f4fd;
	border: 1px solid #b3e0ff;
	color: #0277bd;
	box-shadow: 0 3px 6px rgba(0, 0, 0, 0.08);
	position: relative;
	overflow: hidden;
	transition: all 0.3s ease;
}

.user-rated-alert:hover {
	background-color: #e1f5fe;
	transform: translateY(-1px);
	box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
}

.user-rated-alert::before {
	content: "";
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: linear-gradient(
		90deg,
		transparent,
		rgba(255, 255, 255, 0.6),
		transparent
	);
	animation: shimmer 3s infinite;
}

.user-rated-alert i {
	font-size: 18px;
	margin-right: 10px;
	color: #29b6f6;
	animation: pulse 2s infinite;
}

/* We can remove the pulseAlert keyframe as we're now using the pulse animation */

.rating-selector {
	display: flex;
	align-items: center;
	margin-bottom: 15px;
}

.rating-selector span {
	margin-right: 10px;
}

.star-rating {
	color: #ccc; /* Mặc định màu xám cho sao chưa chọn */
	cursor: pointer;
	display: flex;
	align-items: center;
	margin: 10px 0;
	position: relative;
}

.star-rating i {
	margin-right: 8px;
	transition: color 0.3s ease;
}

.star-rating .fa-star {
	color: #ffc107; /* Màu vàng cho sao đã chọn */
}

.star-rating.disabled {
	position: relative;
}

.star-rating.disabled::after {
	content: "";
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	cursor: not-allowed;
}

.clear-rating-btn {
	background: none;
	border: none;
	color: #6c757d;
	font-size: 18px;
	cursor: pointer;
	padding: 5px;
	margin-left: 5px;
	border-radius: 50%;
	display: inline-flex;
	align-items: center;
	justify-content: center;
	transition: all 0.2s ease;
	background-color: #f0f0f0;
	width: 28px;
	height: 28px;
}

.clear-rating-btn:hover {
	background-color: #e0e0e0;
	color: #dc3545;
	transform: scale(1.1);
}

.rating-hint {
	font-size: 14px;
	color: #6c757d;
	margin-top: 5px;
	margin-left: 2px;
	animation: fadeIn 0.3s;
}

.rating-hint i {
	color: #6c757d;
	font-size: 12px;
}

/* Rating notification styling */
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

/* Disabled rating notification */
.rating-disabled-info {
	display: flex;
	align-items: center;
	padding: 8px 12px;
	background-color: #f8d7da;
	border-left: 3px solid #dc3545;
	border-radius: 4px;
	color: #721c24;
	font-size: 14px;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.rating-disabled-info i {
	margin-right: 8px;
	font-size: 16px;
	color: #dc3545;
}

@keyframes shimmer {
	0% {
		transform: translateX(-100%);
	}
	100% {
		transform: translateX(100%);
	}
}

textarea {
	width: 100%;
	padding: 12px;
	border: 1px solid #ddd;
	border-radius: 4px;
	resize: vertical;
	margin-bottom: 15px;
	font-family: inherit;
}

.submit-comment-btn {
	padding: 12px 24px;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 4px;
	cursor: pointer;
	transition: all 0.3s;
	font-weight: 500;
	font-size: 15px;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
	display: flex;
	align-items: center;
	gap: 8px;
}

.submit-comment-btn:hover {
	background-color: var(--hover-color);
	transform: translateY(-1px);
	box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.submit-comment-btn:disabled {
	background-color: #cccccc;
	cursor: not-allowed;
	box-shadow: none;
	transform: none;
}

.form-validation-feedback {
	margin-bottom: 15px;
}

.validation-message {
	color: #e74c3c;
	font-size: 14px;
	margin-bottom: 5px;
	display: flex;
	align-items: center;
	gap: 5px;
}

/* Login Message */
.login-message {
	background-color: #f8f9fa;
	padding: 30px;
	border-radius: 8px;
	margin-bottom: 30px;
	text-align: center;
	border: 1px dashed #ddd;
	box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 15px;
}

.login-icon {
	font-size: 42px;
	color: #aaa;
	margin-bottom: 10px;
}

.login-message p {
	font-size: 16px;
	margin: 0;
	color: #666;
}

.login-message a {
	color: var(--primary-color);
	text-decoration: underline;
	font-weight: 500;
	transition: color 0.2s;
}

.login-message a:hover {
	color: var(--hover-color);
}

.login-btn {
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 4px;
	padding: 12px 20px;
	font-size: 15px;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.3s;
	display: flex;
	align-items: center;
	gap: 8px;
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.login-btn:hover {
	background-color: var(--hover-color);
	transform: translateY(-2px);
	box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

/* Comments List */
.comments-list {
	display: flex;
	flex-direction: column;
	gap: 30px;
}

.comment-item {
	padding: 20px;
	border: 1px solid #eee;
	border-radius: 8px;
}

.comment-header {
	display: flex;
	align-items: center;
	gap: 15px;
	margin-bottom: 15px;
}

.comment-avatar {
	width: 40px;
	height: 40px;
	border-radius: 50%;
	object-fit: cover;
}

.comment-user {
	flex: 1;
}

.user-name {
	font-weight: 500;
	display: block;
	margin-bottom: 3px;
}

.comment-rating {
	margin-top: 3px;
	display: flex;
	align-items: center;
}

.comment-rating .fa-star,
.comment-rating .fa-star-half-alt {
	color: #ffc107;
}

.comment-rating .far.fa-star {
	color: #ccc;
}

.comment-date {
	font-size: 14px;
	color: #999;
}

.comment-content {
	margin-bottom: 15px;
	line-height: 1.6;
}

/* Comment Actions */
.comment-actions {
	display: flex;
	align-items: center;
	gap: 15px;
}

.reply-btn,
.edit-btn,
.delete-btn {
	background: none;
	border: none;
	color: #666;
	cursor: pointer;
	display: flex;
	align-items: center;
	gap: 5px;
	font-size: 14px;
	padding: 5px;
}

.reply-btn:hover {
	color: var(--primary-color);
}

.edit-btn:hover {
	color: #28a745;
}

.delete-btn:hover {
	color: #dc3545;
}

/* Reaction styles removed */

.owner-actions {
	margin-left: auto;
	display: flex;
	gap: 10px;
}

/* Reply Form Styles */
.reply-form,
.edit-form {
	margin-top: 15px;
	padding: 15px;
	background-color: #f9f9f9;
	border-radius: 8px;
}

.reply-buttons,
.edit-buttons {
	display: flex;
	gap: 10px;
}

.submit-reply-btn,
.cancel-reply-btn,
.submit-edit-btn,
.cancel-edit-btn {
	padding: 8px 16px;
	border: none;
	border-radius: 4px;
	cursor: pointer;
}

.submit-reply-btn,
.submit-edit-btn {
	background-color: var(--primary-color);
	color: white;
}

.cancel-reply-btn,
.cancel-edit-btn {
	background-color: #6c757d;
	color: white;
}

.edit-rating {
	display: flex;
	align-items: center;
	margin: 10px 0;
}

.edit-rating span {
	margin-right: 10px;
}

/* Replies Section */
.replies-section {
	margin-top: 20px;
	padding-left: 20px;
	border-left: 2px solid #eee;
}

.reply-item {
	padding: 15px;
	background-color: #f9f9f9;
	border-radius: 8px;
	margin-bottom: 10px;
}

.reply-header {
	display: flex;
	align-items: center;
	gap: 10px;
	margin-bottom: 10px;
}

.reply-avatar {
	width: 30px;
	height: 30px;
	border-radius: 50%;
	object-fit: cover;
}

.reply-user {
	flex: 1;
}

.reply-date {
	font-size: 12px;
	color: #999;
}

.reply-content {
	margin-bottom: 10px;
	line-height: 1.5;
}

.reply-actions {
	display: flex;
	justify-content: flex-end;
	gap: 10px;
}

/* Empty States */
.no-comments,
.no-filtered-comments {
	text-align: center;
	padding: 30px;
	background-color: #f9f9f9;
	border-radius: 8px;
	margin-bottom: 20px;
}

/* Pagination */
.pagination-container {
	display: flex;
	justify-content: center;
	align-items: center;
	gap: 5px;
	margin-top: 30px;
}

.pagination-btn {
	width: 36px;
	height: 36px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: 1px solid #ddd;
	background-color: white;
	border-radius: 4px;
	cursor: pointer;
	transition: all 0.2s;
}

.pagination-btn:hover:not(:disabled) {
	background-color: #f5f5f5;
}

.pagination-btn.active {
	background-color: var(--primary-color);
	color: white;
	border-color: var(--primary-color);
}

.pagination-btn:disabled {
	opacity: 0.5;
	cursor: not-allowed;
}

/* Confirmation Dialog */
.confirm-dialog {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background-color: rgba(0, 0, 0, 0.5);
	display: flex;
	align-items: center;
	justify-content: center;
	z-index: 1000;
}

.confirm-dialog-content {
	background-color: white;
	padding: 30px;
	border-radius: 8px;
	max-width: 400px;
	width: 90%;
	text-align: center;
}

.confirm-dialog-content h3 {
	margin-bottom: 15px;
}

.confirm-buttons {
	display: flex;
	justify-content: center;
	gap: 15px;
	margin-top: 20px;
}

.confirm-yes,
.confirm-no {
	padding: 10px 20px;
	border: none;
	border-radius: 4px;
	cursor: pointer;
}

.confirm-yes {
	background-color: #dc3545;
	color: white;
}

.confirm-no {
	background-color: #6c757d;
	color: white;
}

@media (max-width: 768px) {
	.reviews-summary {
		flex-direction: column;
		gap: 20px;
	}

	.comment-header,
	.reply-header {
		flex-wrap: wrap;
	}

	.comment-date,
	.reply-date {
		margin-left: auto;
	}

	.comment-actions {
		flex-wrap: wrap;
		gap: 10px;
	}

	.owner-actions {
		margin-top: 10px;
		margin-left: 0;
	}

	.filter-options {
		gap: 8px;
	}
	.filter-option {
		padding: 8px 10px;
		font-size: 13px;
		min-width: 60px;
	}

	.filter-stars .fas.fa-star,
	.filter-stars .far.fa-star {
		font-size: 12px;
	}
}

@keyframes pulse {
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

@keyframes shimmer {
	0% {
		transform: translateX(-100%);
	}
	100% {
		transform: translateX(100%);
	}
}
</style>
