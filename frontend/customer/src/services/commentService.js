import axiosInstance from "./axiosConfig";

class CommentService {
	// Get comments by product ID with pagination
	async getCommentsByProductId(productId, page = 1, pageSize = 10) {
		try {
			const response = await axiosInstance.get(
				`/comment/product/${productId}?page=${page}&pageSize=${pageSize}`
			);
			return response.data;
		} catch (error) {
			console.error("Error fetching comments:", error);
			throw error;
		}
	}
	// Get product rating statistics
	async getProductAverageRating(productId) {
		try {
			const response = await axiosInstance.get(
				`/comment/product/${productId}/rating`
			);
			return response.data;
		} catch (error) {
			console.error("Error fetching product rating statistics:", error);
			throw error;
		}
	}

	// Get just the average rating number
	async getProductAverageRatingOnly(productId) {
		try {
			const response = await axiosInstance.get(
				`/comment/product/${productId}/average-rating`
			);
			return response.data;
		} catch (error) {
			console.error("Error fetching product average rating:", error);
			throw error;
		}
	}
	// Check if the current user has already rated this product
	async hasUserRatedProduct(productId) {
		try {
			const response = await axiosInstance.get(
				`/comment/check-user-rating/${productId}`
			);
			console.log("Has user rated response:", response);
			return response.data.data; // Returns true if user has already rated
		} catch (error) {
			console.error("Error checking if user has rated product:", error);
			if (error.response && error.response.status === 401) {
				// User not logged in
				return false;
			}
			throw error; // Rethrow other errors
		}
	}
	// Check if user has purchased a product
	async hasUserPurchasedProduct(productId) {
		try {
			const response = await axiosInstance.get(
				`/order/check-purchase/${productId}`
			);
			console.log("Has user purchased response:", response);
			return response.data.data; // Returns true if user has purchased the product
		} catch (error) {
			console.error("Error checking product purchase:", error);
			if (error.response && error.response.status === 401) {
				// User not logged in
				return false;
			}
			throw error; // Rethrow other errors to be handled by the component
		}
	}

	// Create a new comment
	async createComment(productId, content, rating, parentId = null) {
		try {
			const commentData = {
				productId,
				content,
				rating,
				parentId,
			};

			const response = await axiosInstance.post(`/comment`, commentData);
			return response.data;
		} catch (error) {
			console.error("Error creating comment:", error);
			throw error;
		}
	}

	// Update an existing comment
	async updateComment(commentId, content, rating) {
		try {
			const commentData = {
				content,
				rating,
			};

			const response = await axiosInstance.put(
				`/comment/${commentId}`,
				commentData
			);
			return response.data;
		} catch (error) {
			console.error("Error updating comment:", error);
			throw error;
		}
	}
	// Delete a comment
	async deleteComment(commentId) {
		try {
			const response = await axiosInstance.delete(
				`/comment/${commentId}`
			);
			return response.data;
		} catch (error) {
			console.error("Error deleting comment:", error);
			throw error;
		}
	}
}

export default new CommentService();
