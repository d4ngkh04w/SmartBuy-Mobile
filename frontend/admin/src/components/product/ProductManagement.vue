<script setup>
import { ref, onMounted, computed, watch } from "vue";
import productService from "../../services/productService.js";
import productLineService from "../../services/productLineService.js";
import brandService from "../../services/brandService.js";
import emitter from "../../utils/evenBus.js";

// Import component parts
import ProductManagementHeader from "./components/ProductManagementHeader.vue";
import ProductStatCards from "./components/ProductStatCards.vue";
import ProductTable from "./components/ProductTable.vue";
import ProductStatusModal from "./components/ProductStatusModal.vue";
import ProductDetailModal from "./components/ProductDetailModal.vue";
import ProductEditModal from "./components/ProductEditModal.vue";
import ProductAddModal from "./components/ProductAddModal.vue";

// ======== STATE ========
// Main data
const products = ref([]);
const brands = ref([]);
const productLines = ref([]);

// UI state
const loading = ref(false);
const searchQuery = ref("");
const statusFilter = ref("all");

// Utility functions
const showErrorNotification = (message) => {
	emitter.emit("show-notification", {
		status: "error",
		message,
	});
};

const showSuccessNotification = (message) => {
	emitter.emit("show-notification", {
		status: "success",
		message,
	});
};

// Modal state
const showStatusModal = ref(false);
const showDetailModal = ref(false);
const showEditModal = ref(false);
const showAddModal = ref(false);

// Selected data
const selectedProduct = ref(null);
const productToToggle = ref(null);

// Default empty product object
const defaultProductData = {
	id: null,
	name: "",
	importPrice: "",
	salePrice: "",
	description: "",
	productLineId: null,
	detail: {},
	colors: [],
};

// Reset product to default empty state
const resetProductToEdit = () => {
	productToEdit.value = { ...defaultProductData };
};

const productToEdit = ref({ ...defaultProductData });

// ======== COMPUTED PROPERTIES ========
// Stats for cards
const totalProducts = computed(() => products.value.length);
const activeProducts = computed(
	() => products.value.filter((p) => p.isActive).length
);
const totalValue = computed(() => {
	return products.value.reduce((sum, product) => {
		return sum + product.stock * product.salePrice;
	}, 0);
});

// ======== DATA FETCHING METHODS ========
// Fetch all products
const fetchProducts = async () => {
	loading.value = true;
	try {
		const filters = {};

		// Add status filter only
		if (statusFilter.value !== "all") {
			filters.isActive = statusFilter.value === "active";
		}

		const response = await productService.getProducts(filters);
		if (response.data && response.data.data) {
			products.value = response.data.data;
		} else {
			products.value = [];
		}
	} catch (error) {
		products.value = [];
		showErrorNotification("Có lỗi xảy ra khi tải danh sách sản phẩm");
	} finally {
		loading.value = false;
	}
};

// Fetch all active brands
const fetchBrands = async () => {
	try {
		const response = await brandService.getBrands({
			includeProductLines: true,
			isActive: true,
		});

		brands.value = response.data;
	} catch (error) {
		brands.value = [];
		// showErrorNotification("Có lỗi xảy ra khi tải danh sách thương hiệu");
	}
};

// Fetch product lines for selected brand
const fetchProductLinesForBrand = async (brandId) => {
	if (!brandId) {
		productLines.value = [];
		return;
	}

	try {
		const response = await productLineService.getProductLinesByBrand(
			brandId
		);
		if (response.data) {
			productLines.value = response.data;
		} else {
			productLines.value = [];
		}
	} catch (error) {
		productLines.value = [];
		showErrorNotification("Có lỗi xảy ra khi tải dòng sản phẩm");
	}
};

// ======== PRODUCT DETAIL OPERATIONS ========
// View product detail
const viewProductDetail = async (product) => {
	try {
		// Fetch detailed information if needed
		const response = await productService.getProductById(product.id);
		if (response.data && response.data.data) {
			selectedProduct.value = response.data.data;
		} else {
			selectedProduct.value = product;
		}

		// Get brand name from product line
		const productLine = await productLineService.getProductLineById(
			selectedProduct.value.productLineId
		);
		selectedProduct.value.brandName = productLine.data.brandName;

		showDetailModal.value = true;
	} catch (error) {
		console.error("Error fetching product details:", error);
		showErrorNotification("Không thể tải chi tiết sản phẩm");
	}
};

// Close product detail modal
const closeDetailModal = () => {
	showDetailModal.value = false;
	selectedProduct.value = null;
};

// ======== STATUS OPERATIONS ========
// Toggle product status modal
const toggleStatusModal = (product) => {
	productToToggle.value = product;
	showStatusModal.value = true;
};

// Cancel status toggle
const cancelStatusToggle = () => {
	showStatusModal.value = false;
	productToToggle.value = null;
};

// Confirm status toggle and call the API
const confirmStatusToggle = async () => {
	if (!productToToggle.value) return;

	loading.value = true;
	try {
		await productService.toggleProductStatus(
			productToToggle.value.id,
			productToToggle.value.isActive
		);

		showSuccessNotification(
			productToToggle.value.isActive
				? "Ngừng bán sản phẩm thành công!"
				: "Kích hoạt bán lại sản phẩm thành công!"
		);
		// Refresh products list
		await fetchProducts();
		showStatusModal.value = false;
		productToToggle.value = null;
	} catch (error) {
		console.error("Error toggling product status:", error);
		showErrorNotification(
			"Có lỗi xảy ra khi cập nhật trạng thái sản phẩm."
		);
	} finally {
		loading.value = false;
	}
};

// ======== PRODUCT EDIT OPERATIONS ========
// Open product edit modal
const editProduct = async (product) => {
	try {
		// Fetch detailed product information
		const response = await productService.getProductById(product.id);
		if (response.data && response.data.data) {
			productToEdit.value = response.data.data;
		} else {
			productToEdit.value = product;
		}

		// Find brand ID and fetch product lines
		let brandIdToUse = null;

		if (productToEdit.value.brandId) {
			// If product already has brandId, use it
			brandIdToUse = productToEdit.value.brandId;
		} else if (productToEdit.value.productLineId) {
			// If product only has productLineId, find the brand ID from product line
			try {
				const productLineResponse =
					await productLineService.getProductLineById(
						productToEdit.value.productLineId
					);
				if (productLineResponse.data && productLineResponse.data.data) {
					brandIdToUse = productLineResponse.data.data.brandId;
					// Also set the brandId on the product for consistency
					productToEdit.value.brandId = brandIdToUse;
				}
			} catch (error) {
				console.error("Error fetching product line details:", error);
			}
		}

		// Fetch product lines for the selected brand
		if (brandIdToUse) {
			await fetchProductLinesForBrand(brandIdToUse);
		}

		// Show the edit modal
		showEditModal.value = true;
	} catch (error) {
		console.error("Error fetching product details for editing:", error);
		showErrorNotification("Không thể tải thông tin sản phẩm để chỉnh sửa");
	}
};

// Handle brand change in edit form
const handleBrandChange = async (brandId) => {
	await fetchProductLinesForBrand(brandId);
};

// Handle product update
const handleProductUpdated = async () => {
	// Refresh the products list
	await fetchProducts();

	// If a product is being edited, refresh its data to show updated images/colors
	if (productToEdit.value && productToEdit.value.id) {
		try {
			const response = await productService.getProductById(
				productToEdit.value.id
			);
			if (response.data && response.data.data) {
				// Update the product data while keeping the modal open
				productToEdit.value = response.data.data;
			}
		} catch (error) {
			console.error("Error refreshing product details:", error);
			showErrorNotification(
				"Không thể tải thông tin sản phẩm sau khi cập nhật"
			);
		}
	}
};

// Close edit modal
const closeEditModal = () => {
	showEditModal.value = false;
	resetProductToEdit();
};

// ======== PRODUCT ADD OPERATIONS ========
// Open product add modal
const openAddProductModal = () => {
	showAddModal.value = true;
};

// Close add modal
const closeAddModal = () => {
	showAddModal.value = false;
};

// Handle product creation
const handleProductCreated = async () => {
	// Refresh the products list
	await fetchProducts();
};

// ======== LIFECYCLE HOOKS ========
// Load data when component mounts
onMounted(async () => {
	await fetchProducts();
	await fetchBrands();
});

// Watch for changes in status filter only
watch(statusFilter, async () => {
	await fetchProducts();
});
</script>

<template>
	<div class="product-management">
		<!-- Header Section -->
		<ProductManagementHeader
			v-model:searchQuery="searchQuery"
			v-model:statusFilter="statusFilter"
			@add-product="openAddProductModal"
		/>

		<!-- Status Cards -->
		<ProductStatCards
			:totalProducts="totalProducts"
			:activeProducts="activeProducts"
			:totalValue="totalValue"
		/>
		<!-- Products table -->
		<ProductTable
			:products="products"
			:loading="loading"
			:searchQuery="searchQuery"
			@view-product-detail="viewProductDetail"
			@toggle-status="toggleStatusModal"
			@edit-product="editProduct"
			@add-product="openAddProductModal"
		/>

		<!-- Status Toggle Confirmation Modal -->
		<ProductStatusModal
			:showModal="showStatusModal"
			:product="productToToggle"
			:loading="loading"
			@confirm="confirmStatusToggle"
			@cancel="cancelStatusToggle"
		/>

		<!-- Product Detail Modal -->
		<ProductDetailModal
			:showModal="showDetailModal"
			:product="selectedProduct"
			@close="closeDetailModal"
		/>

		<!-- Product Edit Modal -->
		<ProductEditModal
			:showModal="showEditModal"
			:product="productToEdit"
			:brands="brands"
			:productLines="productLines"
			@close="closeEditModal"
			@brand-change="handleBrandChange"
			@product-updated="handleProductUpdated"
		/>

		<!-- Product Add Modal -->
		<ProductAddModal
			:showModal="showAddModal"
			:brands="brands"
			:productLines="productLines"
			@close="closeAddModal"
			@brand-change="handleBrandChange"
			@product-created="handleProductCreated"
		/>
	</div>
</template>

<style scoped>
.product-management {
	padding: 2rem;
	width: 100%;
}
</style>
