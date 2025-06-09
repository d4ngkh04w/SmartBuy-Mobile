import axiosInstance from "./axiosConfig";

class ProductService {
	async getProducts(page, limit, filters = {}) {
		try {
			const {
				search = "",
				sortBy = "newest",
				brandName = null,
				productLineId = null,
				minPrice = null,
				maxPrice = null,
			} = filters;

			const params = new URLSearchParams({
				page,
				pageSize: limit,
			});

			if (search) params.append("search", search);
			if (sortBy) params.append("sortBy", sortBy);
			if (brandName) params.append("brandName", brandName);
			if (productLineId) params.append("productLineId", productLineId);
			if (minPrice !== null) params.append("minPrice", minPrice);
			if (maxPrice !== null) params.append("maxPrice", maxPrice);
			const response = await axiosInstance.get(
				`/product/page?${params.toString()}`
			);
			if (response) {
				return response.data;
			}
		} catch (error) {
			console.error("Error fetching products:", error);
			throw error;
		}
	}
	async getProductById(id) {
		return await axiosInstance
			.get(`/product/${id}`)
			.then((response) => {
				if (response) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching product by ID:", error);
				throw error;
			});
	}
	async getQuantityProduct(productId, colorId) {
		const product = await this.getProductById(productId);
		if (product) {
			const productColorData = product.colors.find(
				(color) => color.id === colorId
			);
			if (productColorData) {
				return productColorData.quantity;
			}
		}
	}
	async createOrder(orderData) {
		return await axiosInstance
			.post("/order", orderData)
			.then((response) => {
				return response.data.data;
			})
			.catch((error) => {
				console.error(
					"Error creating order:",
					error.response?.data || error
				);
				throw error;
			});
	}

	async getBrands() {
		return await axiosInstance
			.get("/brand")
			.then((response) => {
				if (response) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching brands:", error);
				throw error;
			});
	}
	async getProductLinesByBrand(brandId) {
		return await axiosInstance
			.get(`/product-line/by-brand/${brandId}`)
			.then((response) => {
				if (response) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching product lines by brand:", error);
				throw error;
			});
	}
	getUrlImage(url) {
		const baseUrl =
			import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
		return url?.startsWith("http") ? url : `${baseUrl}${url}`;
	}
	async getCarts() {
		return await axiosInstance
			.get("/cart")
			.then((response) => {
				if (response) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching carts:", error);
				throw error;
			});
	}
	async removeCartItem(cartId) {
		return await axiosInstance
			.delete(`/cart/items/${cartId}`)
			.then((response) => {
				if (response) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error removing cart item:", error);
				throw error;
			});
	}
	async updateCartItem(cartId, quantity) {
		return await axiosInstance
			.put(`/cart/items/${cartId}`, { quantity })
			.then((response) => {
				if (response) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error updating cart item:", error);
				throw error;
			});
	}
	async addToCart(productId, quantity, colorId) {
		return await axiosInstance
			.post("/cart/add", {
				productId,
				quantity,
				colorId,
			})
			.then((response) => {
				if (response && response.data) {
					return response.data;
				}
			})
			.catch((error) => {
				console.error("Error adding product to cart:", error);
				throw error;
			});
	}
	async checkQuantityToToggleStatus(productId) {
		const product = await this.getProductById(productId);
		if (product) {
			if (product.stock === 0) {
				await this.deactivateProduct(productId);
			} else {
				await this.activateProduct(productId);
			}
		}
		return;
	}
	async getAllOrders() {
		return await axiosInstance
			.get("/order/me")
			.then((response) => {
				if (response) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching all products:", error);
				throw error;
			});
	}
	async getAllCurrentOrders() {
		return await axiosInstance
			.get("/order/me-current")
			.then((response) => {
				if (response && response.data) {
					return response.data.data || [];
				}
				return [];
			})
			.catch((error) => {
				console.error("Error fetching current orders:", error);
				throw error;
			});
	}
	async cancelOrder(orderId) {
		return await axiosInstance
			.put(`/order/${orderId}/cancel`)
			.then((response) => {
				if (response) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error cancelling order:", error);
				throw error;
			});
	}
	async updateStatusOrder(orderId, status) {
		return await axiosInstance
			.put(`/order/${orderId}/status`, { status })
			.then((response) => {
				if (response) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error updating order status:", error);
				throw error;
			});
	}
	async searchProducts(searchQuery, limit = 10) {
		try {
			if (!searchQuery || searchQuery.trim().length === 0) {
				return { data: { items: [], totalItems: 0 } };
			}

			const params = new URLSearchParams({
				search: searchQuery.trim(),
				pageSize: limit,
				page: 1,
				isActive: true,
			});

			const response = await axiosInstance.get(
				`/product/page?${params.toString()}`
			);

			if (response && response.data) {
				return response.data;
			}
			return { data: { items: [], totalItems: 0 } };
		} catch (error) {
			console.error("Error searching products:", error);
			return { data: { items: [], totalItems: 0 } };
		}
	}
	async getOrderById(orderId) {
		return await axiosInstance
			.get(`/order/${orderId}`)
			.then((response) => {
				if (response && response.data) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching order details:", error);
				throw error;
			});
	}
	async createComment(commentData) {
		return await axiosInstance
			.post("/comment", commentData)
			.then((response) => {
				if (response && response.data) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error creating comment:", error);
				throw error;
			});
	}
}
export default new ProductService();
