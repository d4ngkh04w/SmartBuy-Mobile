<script setup>
import { ref, onMounted, computed, watch } from "vue";
import productLineService from "../../services/productLineService.js";
import brandService from "../../services/brandService.js";
import emitter from "../../utils/evenBus.js";

// State
const productLines = ref([]);
const allBrands = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showStatusModal = ref(false);
const isEditing = ref(false);
const hasProducts = ref(false);

const formData = ref(productLineService.getEmptyProductLineForm());
const imagePreview = ref("");
const fileInput = ref(null);

const searchQuery = ref("");
const statusFilter = ref("all"); // "all", "active", "inactive"
const productLineToToggle = ref(null);

// Computed
const filteredProductLines = computed(() => {
    const query = searchQuery.value.toLowerCase().trim();
    return productLines.value.filter(
        (pl) =>
            !query ||
            pl.name?.toLowerCase().includes(query) ||
            pl.description?.toLowerCase().includes(query) ||
            pl.brandName?.toLowerCase().includes(query)
    );
});

const brandsFromProductLines = computed(() => {
    const uniqueBrands = new Map();

    productLines.value.forEach((pl) => {
        if (pl.brandName && pl.brandId) {
            uniqueBrands.set(pl.brandId, {
                id: pl.brandId,
                name: pl.brandName,
                logo: pl.brandLogo || null,
            });
        }
    });

    return Array.from(uniqueBrands.values());
});

const availableBrands = computed(() => {
    // If we have brands from API, use those. Otherwise use extracted brands from product lines
    return allBrands.value.length > 0
        ? allBrands.value
        : brandsFromProductLines.value;
});



const activeCount = computed(
    () => productLines.value.filter((pl) => pl.isActive).length
);

// Methods
const fetchBrands = async () => {
    try {
        const response = await brandService.getBrands();
        allBrands.value = response.data;       
    } catch (error) {
        console.error("Error fetching brands:", error);
    }
};

const getProductLineProductsCount = (productLineId) => {
    return productLineService.getProductLineProductsCount(
        productLines.value,
        productLineId
    );
};

// Fetch all product lines
const fetchProductLines = async () => {
    loading.value = true;
    try {
        const params = {
            includeProducts: true,
            ...(statusFilter.value !== "all" && {
                isActive: statusFilter.value === "active",
            }),
        };

        const response = await productLineService.getProductLines(params);
        productLines.value = response.data;       
    } catch (error) {
        console.error("Error fetching product lines:", error);
        productLines.value = [];
    } finally {
        loading.value = false;
    }
};

// Open modal to add new product line
const openAddProductLineModal = async () => {
    loading.value = true;
    try {
        // Fetch brands first to ensure we have the complete list
        await fetchBrands();

        isEditing.value = false;
        formData.value = productLineService.getEmptyProductLineForm();
        imagePreview.value = "";
        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
        alert("Có lỗi xảy ra khi chuẩn bị form thêm mới. Vui lòng thử lại.");
    } finally {
        loading.value = false;
    }
};

// Edit product line
const editProductLine = async (productLine) => {
    loading.value = true;
    try {       
        isEditing.value = true;

        formData.value = productLineService.getEditProductLineForm(productLine);
        imagePreview.value = productLine.image
            ? productLineService.getUrlImage(productLine.image)
            : "";

        await fetchBrands();		
		const matchingBrand = allBrands.value.find(
			(b) => b.name.toLowerCase() === productLine.brandName?.toLowerCase()
		);

		console.log("Found matching brand:", matchingBrand);
		const brandId = matchingBrand ? matchingBrand.id : productLine.brandId;

        formData.value.brandId = brandId;    

        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
        emitter.emit("show-notification", {
            status: "error",
            message:
                "Có lỗi xảy ra khi chuẩn bị form chỉnh sửa. Vui lòng thử lại.",
        });
    } finally {
        loading.value = false;
    }
};

// Handle file upload
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const validation = productLineService.validateImageFile(file);
    if (!validation.valid) {
        alert(validation.message);
        return;
    }

    formData.value.imageFile = file;

    const reader = new FileReader();
    reader.onload = (e) => {
        imagePreview.value = e.target.result;
    };
    reader.readAsDataURL(file);
};

// Clear image
const clearImage = (e) => {
    e?.stopPropagation();
    formData.value.imageFile = null;
    imagePreview.value = "";
    if (fileInput.value) fileInput.value.value = "";
};

// Submit form (create or update)
const submitForm = async () => {
    loading.value = true;
    try {
        const data = productLineService.prepareProductLineFormData(
            formData.value
        );

        if (!formData.value.brandId) {
            emitter.emit("show-notification", {
                status: "error",
                message: "Vui lòng chọn thương hiệu cho dòng sản phẩm",
            });
            loading.value = false;
            return;
        }

        const response = isEditing.value
            ? await productLineService.updateProductLine(
                  formData.value.id,
                  data
              )
            : await productLineService.createProductLine(data);

        emitter.emit("show-notification", {
            status: "success",
            message: isEditing.value
                ? "Cập nhật dòng sản phẩm thành công!"
                : "Thêm dòng sản phẩm thành công!",
        });

        await fetchProductLines();
        closeModal();
    } catch (error) {
        console.error("Error submitting form:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra trong quá trình xử lý yêu cầu.",
        });
    } finally {
        loading.value = false;
    }
};

// Close modal
const closeModal = () => {
    showModal.value = false;
    formData.value = productLineService.getEmptyProductLineForm();
    imagePreview.value = "";
    if (fileInput.value) fileInput.value.value = "";
};

// Confirm status toggle
const confirmToggleStatus = (productLine) => {
    if (!productLine) return;
    productLineToToggle.value = productLine;
    hasProducts.value = getProductLineProductsCount(productLine.id) > 0;
    showStatusModal.value = true;
};

// Cancel status toggle
const cancelToggleStatus = () => {
    showStatusModal.value = false;
    productLineToToggle.value = null;
    hasProducts.value = false;
};

// Toggle product line status
const toggleProductLineStatus = async () => {
    if (!productLineToToggle.value) return;

    loading.value = true;
    try {
        await productLineService.toggleProductLineStatus(
            productLineToToggle.value.id,
            productLineToToggle.value.isActive
        );

        emitter.emit("show-notification", {
            status: "success",
            message: productLineToToggle.value.isActive
                ? "Hủy kích hoạt dòng sản phẩm thành công"
                : "Kích hoạt dòng sản phẩm thành công",
        });

        await fetchProductLines();
        showStatusModal.value = false;
        productLineToToggle.value = null;
    } catch (error) {
        console.error("Error toggling product line status:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra khi thay đổi trạng thái dòng sản phẩm",
        });
    } finally {
        loading.value = false;
    }
};

// Watch for statusFilter changes and reload data
watch(statusFilter, async () => {
    await fetchProductLines();
});

// Load data when component mounts
onMounted(async () => {
    await fetchProductLines();
});
</script>
<template>
    <div class="product-line-management">
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-mobile-alt"></i> Quản lý Dòng sản phẩm</h2>
                <p>Quản lý các dòng sản phẩm theo thương hiệu</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm dòng sản phẩm..."
                    />
                </div>

                <!-- Status Filter -->
                <div class="status-filter">
                    <i class="fas fa-filter"></i>
                    <select v-model="statusFilter">
                        <option value="all">Tất cả trạng thái</option>
                        <option value="active">Đang kích hoạt</option>
                        <option value="inactive">Chưa kích hoạt</option>
                    </select>
                </div>

                <button @click="openAddProductLineModal" class="add-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm
                </button>
            </div>
        </div>

        <!-- Status Cards -->
        <div class="status-cards">
            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-mobile-alt"></i>
                </div>
                <div class="status-content">
                    <h3>{{ productLines.length }}</h3>
                    <p>Tổng số dòng sản phẩm</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeCount }}</h3>
                    <p>Đang kích hoạt</p>
                </div>
            </div>
        </div>

        <!-- Product Lines hierarchical view -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div
                v-else-if="filteredProductLines.length === 0"
                class="empty-state"
            >
                <i class="fas fa-th-list"></i>
                <p v-if="searchQuery">
                    Không tìm thấy dòng sản phẩm phù hợp với tìm kiếm
                </p>
                <p v-else>Không có dòng sản phẩm nào</p>
                <button @click="openAddProductLineModal" class="action-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm mới
                </button>
            </div>
            <table v-else class="data-table">
                <thead>
                    <tr>
                        <th>Dòng sản phẩm</th>
                        <th>Ảnh</th>
                        <th>Mô tả</th>
                        <th>Thương hiệu</th>
                        <th>Trạng thái</th>
                        <th>Số SP</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="productLine in filteredProductLines"
                        :key="productLine.id"
                        class="data-row"
                    >
                        <td class="name-cell">{{ productLine.name }}</td>
                        <td class="logo-cell">
                            <div class="logo-wrapper">
                                <img
                                    v-if="productLine.image"
                                    :src="
                                        productLineService.getUrlImage(
                                            productLine.image
                                        )
                                    "
                                    :alt="productLine.name"
                                />
                                <div v-else class="no-logo">
                                    <i class="fas fa-mobile-alt"></i>
                                </div>
                            </div>
                        </td>
                        <td class="description-cell">
                            {{ productLine.description || "Không có mô tả" }}
                        </td>
                        <td class="brand-cell">
                            {{
                                availableBrands.find((b) => b.id === productLine.brandId)?.name ||
                                productLine.brandName ||
                                "N/A"
                            }}
                        </td>
                        <td class="status-cell">
                            <span
                                :class="[
                                    'status-badge',
                                    productLine.isActive
                                        ? 'active'
                                        : 'inactive',
                                ]"
                            >
                                {{
                                    productLine.isActive
                                        ? "Đang kích hoạt"
                                        : "Không kích hoạt"
                                }}
                            </span>
                        </td>
                        <td class="count-cell">
                            {{ getProductLineProductsCount(productLine.id) }}
                        </td>
                        <td class="actions-cell">
                            <button
                                @click="editProductLine(productLine)"
                                class="edit-button"
                                title="Chỉnh sửa"
                            >
                                <i class="fas fa-edit"></i>
                            </button>
                            <button
                                @click="confirmToggleStatus(productLine)"
                                :class="[
                                    productLine.isActive
                                        ? 'deactivate-button'
                                        : 'activate-button',
                                ]"
                                :title="
                                    productLine.isActive
                                        ? 'Hủy kích hoạt'
                                        : 'Kích hoạt'
                                "
                            >
                                <i
                                    class="fas"
                                    :class="
                                        productLine.isActive
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

        <!-- Add/Edit Product Line Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa dòng sản phẩm"
                                : "Thêm dòng sản phẩm mới"
                        }}
                    </h3>
                    <button @click="closeModal" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <form @submit.prevent="submitForm" class="modal-form">
                    <div class="form-group">
                        <label
                            >Thương hiệu <span class="required">*</span></label
                        >
                        <div class="select-wrapper">
                            <select
                                v-model="formData.brandId"
                                class="form-select"
                                required
                            >
                                <option value="" disabled selected>
                                    Chọn thương hiệu
                                </option>
                                <option
                                    v-for="brand in availableBrands"
                                    :key="brand.id"
                                    :value="brand.id"
                                >
                                    {{ brand.name }}
                                </option>
                            </select>
                            <i class="fas fa-chevron-down select-icon"></i>
                        </div>
                    </div>

                    <div class="form-group">
                        <label
                            >Tên dòng sản phẩm
                            <span class="required">*</span></label
                        >
                        <input
                            v-model="formData.name"
                            type="text"
                            placeholder="Nhập tên dòng sản phẩm"
                            required
                        />
                    </div>

                    <div class="form-group">
                        <label>Ảnh minh họa</label>
                        <div
                            class="upload-area"
                            @click="$refs.fileInput.click()"
                        >
                            <img
                                v-if="imagePreview"
                                :src="
                                    imagePreview.startsWith('data:') ||
                                    imagePreview.startsWith('blob:')
                                        ? imagePreview
                                        : productLineService.getUrlImage(
                                              imagePreview
                                          )
                                "
                                alt="Preview"
                                class="image-preview"
                            />
                            <div v-else class="upload-placeholder">
                                <i class="fas fa-cloud-upload-alt"></i>
                                <span>Nhấp để tải lên ảnh (SVG, PNG, JPG)</span>
                            </div>
                        </div>
                        <input
                            ref="fileInput"
                            type="file"
                            @change="handleFileChange"
                            accept="image/*"
                            class="hidden-input"
                        />
                        <div v-if="imagePreview" class="preview-actions">
                            <button
                                type="button"
                                @click="clearImage"
                                class="remove-button"
                            >
                                <i class="fas fa-trash-alt"></i> Xóa ảnh
                            </button>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Mô tả</label>
                        <textarea
                            v-model="formData.description"
                            placeholder="Nhập mô tả dòng sản phẩm"
                            rows="3"
                        ></textarea>
                    </div>

                    <div class="form-actions">
                        <button
                            type="button"
                            @click="closeModal"
                            class="cancel-button"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                        <button
                            type="submit"
                            class="submit-button"
                            :disabled="loading"
                        >
                            <span v-if="loading" class="spinner small"></span>
                            <i
                                v-else
                                class="fas"
                                :class="isEditing ? 'fa-save' : 'fa-plus'"
                            ></i>
                            {{ isEditing ? "Cập nhật" : "Thêm mới" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Status Toggle Confirmation Modal -->
        <div v-if="showStatusModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div
                    class="modal-header"
                    :class="{ warning: productLineToToggle?.isActive }"
                >
                    <h3>
                        {{
                            productLineToToggle?.isActive
                                ? "Xác nhận hủy kích hoạt dòng sản phẩm"
                                : "Xác nhận kích hoạt dòng sản phẩm"
                        }}
                    </h3>
                    <button @click="cancelToggleStatus" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div
                        class="warning-icon"
                        :class="{
                            'activate-icon': !productLineToToggle?.isActive,
                        }"
                    >
                        <i
                            class="fas"
                            :class="
                                productLineToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn
                        {{
                            productLineToToggle?.isActive
                                ? "vô hiệu hóa"
                                : "kích hoạt"
                        }}
                        dòng sản phẩm
                        <strong>"{{ productLineToToggle?.name }}"</strong>?
                    </p>

                    <!-- Status change impact details -->
                    <div
                        class="warning-details"
                        :class="{
                            'activate-details': !productLineToToggle?.isActive,
                        }"
                    >
                        <i class="fas fa-info-circle"></i>
                        <div class="warning-text">
                            <p v-if="hasProducts" class="warning-count">
                                <span>Dòng sản phẩm này có</span>
                                <strong>{{
                                    getProductLineProductsCount(
                                        productLineToToggle?.id
                                    )
                                }}</strong>
                                <span>sản phẩm.</span>
                            </p>
                            <p
                                v-if="productLineToToggle?.isActive"
                                class="warning-impact"
                            >
                                <strong>Lưu ý:</strong> Vô hiệu hóa dòng sản
                                phẩm sẽ ẩn tất cả sản phẩm thuộc dòng sản phẩm
                                này khỏi trang người dùng.
                            </p>
                            <p v-else class="warning-impact">
                                <strong>Lưu ý:</strong> Kích hoạt dòng sản phẩm
                                sẽ hiển thị tất cả sản phẩm thuộc dòng sản phẩm
                                này (trừ những sản phẩm đã bị ngừng bán thủ
                                công).
                            </p>
                        </div>
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelToggleStatus" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="toggleProductLineStatus"
                        :class="
                            productLineToToggle?.isActive
                                ? 'deactivate-confirm-button'
                                : 'activate-confirm-button'
                        "
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i
                            v-else
                            class="fas"
                            :class="
                                productLineToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                        {{
                            productLineToToggle?.isActive
                                ? "Vô hiệu hóa"
                                : "Kích hoạt"
                        }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.product-line-management {
    width: 100%;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.left-section h2 {
    font-size: 1.25rem;
    color: #333;
    margin: 0 0 0.25rem 0;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.left-section p {
    color: #666;
    margin: 0;
    font-size: 0.9rem;
}

.right-section {
    display: flex;
    gap: 1rem;
    align-items: center;
}

.search-box {
    position: relative;
    width: 250px;
}

.search-box i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
}

.search-box input {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;
}

.search-box input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.add-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s;
}

.add-button:hover {
    background-color: #e94e9c;
}

.status-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.status-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.status-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background-color: #fff5fc;
    color: var(--primary-color);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.status-content h3 {
    font-size: 1.5rem;
    margin: 0 0 0.25rem 0;
    color: #333;
}

.status-content p {
    margin: 0;
    color: #666;
    font-size: 0.85rem;
}

.data-card {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
    overflow: hidden;
}

.loading-state,
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
    color: #666;
}

.spinner {
    width: 40px;
    height: 40px;
    border: 3px solid rgba(248, 110, 211, 0.1);
    border-top: 3px solid var(--primary-color);
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
    margin: 0;
    margin-right: 0.5rem;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

.empty-state i {
    font-size: 3rem;
    color: #ddd;
    margin-bottom: 1rem;
}

.empty-state p {
    margin-bottom: 1rem;
}

.action-button {
    padding: 0.5rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.no-logo {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
}

.brand-name {
    margin: 0;
    font-size: 1.1rem;
    color: #333;
}

/* Data Table Styles */
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
    width: 15%;
} /* DÒNG SẢN PHẨM */
.data-table th:nth-child(2) {
    width: 10%;
} /* ẢNH */
.data-table th:nth-child(3) {
    width: 35%;
} /* MÔ TẢ */
.data-table th:nth-child(4) {
    width: 15%;
} /* THƯƠNG HIỆU */
.data-table th:nth-child(5) {
    width: 12%;
} /* TRẠNG THÁI */
.data-table th:nth-child(6) {
    width: 5%;
} /* SỐ SP */
.data-table th:nth-child(7) {
    width: 8%;
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

.name-cell {
    font-weight: 500;
}

.logo-cell {
    width: 10%;
}

.logo-wrapper {
    width: 40px;
    height: 40px;
    border-radius: 8px;
    overflow: hidden;
    background-color: #f9f9f9;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #eee;
}

.logo-wrapper img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.description-cell {
    max-width: 250px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.brand-cell {
    font-weight: 500;
}

.status-badge {
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.count-cell {
    text-align: center;
    font-weight: 500;
}

.actions-cell {
    display: flex;
    gap: 0.5rem;
}

.edit-button,
.activate-button,
.deactivate-button {
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

.activate-button {
    background-color: #e6f7ea;
    color: #22c55e;
}

.activate-button:hover {
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

/* Modal Styles */
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    backdrop-filter: blur(3px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow-y: auto;
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-header.warning {
    border-bottom-color: #fee2e2;
    color: #ef4444;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    color: #333;
}

.close-button {
    background: none;
    border: none;
    color: #666;
    font-size: 1rem;
    cursor: pointer;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.close-button:hover {
    background-color: #f9f9f9;
    color: #333;
}

.modal-form {
    padding: 1.5rem;
}

.form-group {
    margin-bottom: 1.25rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #333;
}

.form-group input[type="text"],
.form-group textarea,
.form-select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s;
}

.form-group input[type="text"]:focus,
.form-group textarea:focus,
.form-select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.select-wrapper {
    position: relative;
}

.select-icon {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.form-select {
    appearance: none;
    padding-right: 2.5rem;
}

.required {
    color: #ef4444;
}

.upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 2rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s;
}

.upload-area:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    color: #666;
}

.upload-placeholder i {
    font-size: 2rem;
    color: #999;
}

.image-preview {
    max-width: 100%;
    max-height: 150px;
    margin: 0 auto;
    display: block;
}

.preview-actions {
    display: flex;
    justify-content: center;
    margin-top: 0.75rem;
}

.remove-button {
    background: none;
    border: none;
    color: #ef4444;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.hidden-input {
    display: none;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
}

.cancel-button,
.submit-button,
.activate-confirm-button,
.deactivate-confirm-button {
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.cancel-button {
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    color: #666;
}

.cancel-button:hover {
    background-color: #eee;
}

.submit-button {
    background-color: var(--primary-color);
    border: none;
    color: white;
}

.submit-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.modal-body {
    padding: 1.5rem;
}

.text-center {
    text-align: center;
}

.warning-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background-color: #fee2e2;
    color: #ef4444;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2rem;
    margin: 0 auto 1.5rem auto;
}

.warning-icon.activate-icon {
    background-color: #e6f7ea;
    color: #22c55e;
}

.warning-message {
    font-size: 1.1rem;
    color: #333;
    margin-bottom: 1.5rem;
}

.warning-details {
    display: flex;
    align-items: flex-start;
    gap: 0.8rem;
    padding: 1.25rem;
    background-color: #ffeeee;
    border: 1px solid #ffcccc;
    border-radius: 8px;
    color: #d32f2f;
    text-align: left;
    font-size: 1rem;
    margin-top: 1.5rem;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.warning-details.activate-details {
    background-color: #e8f5e9;
    border: 1px solid #c8e6c9;
    color: #2e7d32;
}

.warning-text {
    flex: 1;
}

.warning-count {
    margin: 0 0 0.75rem 0;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.35rem;
}

.warning-impact {
    margin-top: 0.75rem;
    font-size: 1rem;
}

.warning-impact p {
    margin: 0;
    font-weight: 500;
    line-height: 1.5;
}

.modal-actions {
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.activate-confirm-button {
    background-color: #22c55e;
    border: none;
    color: white;
    min-width: 140px;
}

.activate-confirm-button:hover {
    background-color: #16a34a;
}

.deactivate-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
    min-width: 140px;
}

.deactivate-confirm-button:hover {
    background-color: #dc2626;
}

.status-filter {
    position: relative;
    width: 200px;
}

.status-filter i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.status-filter select {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23666' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 10px center;
    background-size: 16px;
    transition: all 0.3s;
}

.status-filter select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

/* Responsive tweaks */
@media (max-width: 768px) {
    .section-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }

    .right-section {
        width: 100%;
        flex-direction: column;
    }

    .search-box {
        width: 100%;
    }

    .add-button {
        width: 100%;
        justify-content: center;
    }

    .status-cards {
        grid-template-columns: 1fr;
    }

    .data-table {
        display: block;
        overflow-x: auto;
    }
}
</style>
