<script setup>
import { defineEmits, computed } from "vue";
import productService from "../../../services/productService.js";

const props = defineProps({
	products: {
		type: Array,
		required: true,
	},
	loading: {
		type: Boolean,
		default: false,
	},
	searchQuery: {
		type: String,
		default: "",
	},
});

const emit = defineEmits([
	"edit-product",
	"view-product-detail",
	"toggle-status",
	"add-product",
]);

// Format currency function
const formatCurrency = (value) => {
	if (!value) return "0 ₫";
	return new Intl.NumberFormat("vi-VN", {
		style: "currency",
		currency: "VND",
	}).format(value);
};

// Get product image URL
const getProductMainImage = (product) => {
	const url = productService.getProductMainImage(product);
	return url || "https://via.placeholder.com/150"; // Fallback image if no
};

// Computed properties for filtered products
const filteredProducts = computed(() => {
	if (!props.searchQuery) return props.products;

	const query = props.searchQuery.toLowerCase().trim();
	return props.products.filter(
		(product) =>
			product.name.toLowerCase().includes(query) ||
			(product.productLineName &&
				product.productLineName.toLowerCase().includes(query))
	);
});
</script>

<template>
	<div class="data-card">
		<div v-if="loading" class="loading-state">
			<div class="spinner"></div>
			<p>Đang tải dữ liệu...</p>
		</div>
		<div v-else-if="filteredProducts.length === 0" class="empty-state">
			<i class="fas fa-box-open"></i>
			<p v-if="searchQuery">Không tìm thấy sản phẩm phù hợp</p>
			<p v-else>Không có sản phẩm nào</p>
			<button @click="$emit('add-product')" class="action-button">
				<i class="fas fa-plus"></i> Thêm sản phẩm mới
			</button>
		</div>
		<table v-else class="data-table">
			<thead>
				<tr>
					<th>Sản phẩm</th>
					<th>Hình ảnh</th>
					<th>Giá nhập</th>
					<th>Giá bán</th>
					<th>Số lượng</th>
					<th>Dòng sản phẩm</th>
					<th>Trạng thái</th>
					<th>Hành động</th>
				</tr>
			</thead>
			<tbody>
				<tr
					v-for="product in filteredProducts"
					:key="product.id"
					class="data-row"
				>
					<td class="name-cell">{{ product.name }}</td>
					<td class="image-cell">
						<div class="logo-wrapper">
							<img
								v-if="getProductMainImage(product)"
								:src="getProductMainImage(product)"
								:alt="product.name"
							/>
							<div v-else class="no-logo">
								<i class="fas fa-mobile-alt"></i>
							</div>
						</div>
					</td>
					<td>{{ formatCurrency(product.importPrice) }}</td>
					<td>{{ formatCurrency(product.salePrice) }}</td>
					<td class="quantity-cell">
						<span
							:class="{
								'low-stock': product.stock < 10,
								'out-of-stock': product.stock <= 0,
							}"
						>
							{{ product.stock }}
						</span>
					</td>
					<td>{{ product.productLineName }}</td>
					<td class="status-cell">
						<span
							:class="[
								'status-badge',
								product.isActive ? 'active' : 'inactive',
							]"
						>
							{{ product.isActive ? "Đang bán" : "Ngừng bán" }}
						</span>
					</td>
					<td class="actions-cell">
						<button
							@click="$emit('edit-product', product)"
							class="edit-button"
							title="Chỉnh sửa"
						>
							<i class="fas fa-edit"></i>
						</button>
						<button
							@click="$emit('view-product-detail', product)"
							class="detail-button"
							title="Xem chi tiết"
						>
							<i class="fas fa-eye"></i>
						</button>
						<button
							@click="$emit('toggle-status', product)"
							:class="[
								'status-button',
								product.isActive
									? 'deactivate-button'
									: 'activate-button',
							]"
							:title="
								product.isActive ? 'Ngừng bán' : 'Kích hoạt'
							"
						>
							<i
								class="fas"
								:class="
									product.isActive
										? 'fa-toggle-off'
										: 'fa-toggle-on'
								"
							></i>
						</button>
					</td>
				</tr>
			</tbody>
		</table>
	</div>
</template>

<style scoped>
.data-card {
	background-color: white;
	border-radius: 12px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	padding: 1.5rem;
	overflow: hidden;
}

.data-table {
	width: 100%;
	border-collapse: collapse;
	table-layout: fixed;
}

.data-table th {
	text-align: left;
	padding: 1rem;
	color: #666;
	border-bottom: 1px solid #eee;
	font-weight: 600;
	font-size: 0.85rem;
	text-transform: uppercase;
}

.data-table th:nth-child(1) {
	width: 20%;
} /* SẢN PHẨM */
.data-table th:nth-child(2) {
	width: 10%;
} /* HÌNH ẢNH */
.data-table th:nth-child(3) {
	width: 12%;
} /* GIÁ NHẬP */
.data-table th:nth-child(4) {
	width: 12%;
} /* GIÁ BÁN */
.data-table th:nth-child(5) {
	width: 10%;
} /* SỐ LƯỢNG */
.data-table th:nth-child(6) {
	width: 15%;
} /* DÒNG SẢN PHẨM */
.data-table th:nth-child(7) {
	width: 12%;
} /* TRẠNG THÁI */
.data-table th:nth-child(8) {
	width: 9%;
} /* HÀNH ĐỘNG */

.data-row {
	border-bottom: 1px solid #eee;
	transition: background-color 0.2s;
}

.data-row:hover {
	background-color: #f9f9f9;
}

.data-row td {
	padding: 1rem;
	color: #333;
	vertical-align: middle;
}

/* Image cell styling */
.image-cell {
	width: 80px;
}

.logo-wrapper {
	width: 60px;
	height: 60px;
	border-radius: 12px;
	overflow: hidden;
	display: flex;
	align-items: center;
	justify-content: center;
	background-color: #f9f9f9;
	border: 1px solid #eee;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.logo-wrapper img {
	max-width: 100%;
	max-height: 100%;
	object-fit: contain;
}

.no-logo {
	width: 100%;
	height: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
	color: #ddd;
	font-size: 1.5rem;
}

.name-cell {
	max-width: 200px;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}

.quantity-cell span {
	padding: 0.25rem 0.5rem;
	border-radius: 4px;
	font-size: 0.85rem;
}

.low-stock {
	background-color: #fff3e0;
	color: #e65100;
}

.out-of-stock {
	background-color: #ffebee;
	color: #c62828;
}

.status-badge {
	padding: 0.25rem 0.5rem;
	border-radius: 12px;
	font-size: 0.75rem;
	font-weight: 500;
	display: inline-block;
}

.status-badge.active {
	background-color: #e6f7ea;
	color: #22c55e;
}

.status-badge.inactive {
	background-color: #fee2e2;
	color: #ef4444;
}

.actions-cell {
	display: flex;
	gap: 0.5rem;
}

.edit-button,
.status-button,
.detail-button {
	width: 32px;
	height: 32px;
	border-radius: 8px;
	display: flex;
	align-items: center;
	justify-content: center;
	border: none;
	cursor: pointer;
	transition: all 0.2s;
}

.edit-button {
	background-color: #e6f7ea;
	color: #22c55e;
}

.edit-button:hover {
	background-color: #22c55e;
	color: white;
}

.deactivate-button {
	background-color: #fee2e2;
	color: #ef4444;
}

.deactivate-button:hover {
	background-color: #ef4444;
	color: white;
}

.activate-button {
	background-color: #e6f7ea;
	color: #22c55e;
}

.activate-button:hover {
	background-color: #22c55e;
	color: white;
}

.detail-button {
	background-color: #e6f7ea;
	color: #22c55e;
}

.detail-button:hover {
	background-color: #22c55e;
	color: white;
}

/* No data states */
.empty-state,
.loading-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 3rem;
	text-align: center;
}

.empty-state i,
.loading-state .spinner {
	font-size: 3rem;
	color: #ddd;
	margin-bottom: 1rem;
}

.empty-state p,
.loading-state p {
	color: #888;
	margin-bottom: 1.5rem;
}

.action-button {
	padding: 0.6rem 1rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	cursor: pointer;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	font-size: 0.9rem;
}

.action-button:hover {
	background-color: #e94e9c;
}

.spinner {
	width: 50px;
	height: 50px;
	border: 4px solid rgba(0, 0, 0, 0.1);
	border-radius: 50%;
	border-top: 4px solid var(--primary-color);
	animation: spin 1s linear infinite;
}

@keyframes spin {
	0% {
		transform: rotate(0deg);
	}
	100% {
		transform: rotate(360deg);
	}
}
</style>
