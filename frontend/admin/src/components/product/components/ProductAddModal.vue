<template>
    <div v-if="showModal" class="modal-backdrop">
        <div class="modal-container product-modal">
            <div class="modal-header">
                <h3>Thêm sản phẩm mới</h3>
                <button @click="$emit('close')" class="close-button">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="modal-form-container">
                <div class="modal-form">
                    <!-- Basic Information Section -->
                    <div class="form-section">
                        <div class="section-header">
                            <h4 class="section-title">
                                <i class="fas fa-info-circle"></i> Thông tin cơ
                                bản
                            </h4>
                        </div>

                        <!-- Category Selection Section -->
                        <div class="form-row">
                            <div class="form-group">
                                <label for="brandSelect">
                                    Thương hiệu <span class="required">*</span>
                                </label>
                                <div class="select-wrapper">
                                    <select
                                        id="brandSelect"
                                        v-model="productForm.brandId"
                                        @change="handleBrandChange"
                                    >
                                        <option
                                            v-for="brand in brands"
                                            :key="brand.id"
                                            :value="brand.id"
                                        >
                                            {{ brand.name }}
                                        </option>
                                    </select>
                                </div>
                                <span
                                    v-if="formErrors.brandId"
                                    class="error-message"
                                >
                                    {{ formErrors.brandId }}
                                </span>
                            </div>

                            <div class="form-group">
                                <label for="productLineSelect">
                                    Dòng sản phẩm
                                    <span class="required">*</span>
                                </label>
                                <div class="select-wrapper">
                                    <select
                                        id="productLineSelect"
                                        v-model="productForm.productLineId"
                                        :disabled="
                                            !productForm.brandId ||
                                            !productLines.length
                                        "
                                    >
                                        <option
                                            v-if="!productLines.length"
                                            :value="null"
                                        >
                                            {{
                                                !productForm.brandId
                                                    ? "Vui lòng chọn thương hiệu trước"
                                                    : "Không có dòng sản phẩm"
                                            }}
                                        </option>
                                        <option
                                            v-for="line in productLines"
                                            :key="line.id"
                                            :value="line.id"
                                        >
                                            {{ line.name }}
                                        </option>
                                    </select>
                                </div>
                                <span
                                    v-if="formErrors.productLineId"
                                    class="error-message"
                                >
                                    {{ formErrors.productLineId }}
                                </span>
                            </div>
                        </div>

                        <!-- Product Name -->
                        <div class="form-row">
                            <div class="form-group full-width">
                                <label for="productName">
                                    Tên sản phẩm <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="productName"
                                    v-model="productForm.name"
                                    placeholder="Nhập tên sản phẩm (tối đa 100 ký tự)"
                                    maxlength="100"
                                />
                                <span
                                    v-if="formErrors.name"
                                    class="error-message"
                                >
                                    {{ formErrors.name }}
                                </span>
                            </div>
                        </div>

                        <!-- Prices -->
                        <div class="form-row">
                            <div class="form-group">
                                <label for="importPrice">
                                    Giá nhập <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="importPrice"
                                    v-model="importPriceDisplay"
                                    placeholder="Giá nhập (lớn hơn 0)"
                                    inputmode="numeric"
                                    @input="onImportPriceInput"
                                    autocomplete="off"
                                />
                                <span
                                    v-if="formErrors.importPrice"
                                    class="error-message"
                                >
                                    {{ formErrors.importPrice }}
                                </span>
                            </div>

                            <div class="form-group">
                                <label for="salePrice">
                                    Giá bán <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="salePrice"
                                    v-model="salePriceDisplay"
                                    placeholder="Giá bán (lớn hơn 0)"
                                    inputmode="numeric"
                                    @input="onSalePriceInput"
                                    autocomplete="off"
                                />
                                <span
                                    v-if="formErrors.salePrice"
                                    class="error-message"
                                >
                                    {{ formErrors.salePrice }}
                                </span>
                            </div>
                        </div>

                        <!-- Description -->
                        <div class="form-row">
                            <div class="form-group full-width">
                                <label for="description">Mô tả sản phẩm</label>
                                <textarea
                                    id="description"
                                    v-model="productForm.description"
                                    rows="4"
                                    placeholder="Mô tả chi tiết về sản phẩm (tối đa 2000 ký tự)"
                                    maxlength="2000"
                                ></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Technical Specs Section -->
                    <div class="form-section">
                        <div class="section-header">
                            <h4 class="section-title">
                                <i class="fas fa-microchip"></i> Thông số kỹ
                                thuật
                            </h4>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="warranty">Bảo hành (tháng)</label>
                                <input
                                    type="number"
                                    id="warranty"
                                    v-model="productForm.warranty"
                                    placeholder="1-60 tháng"
                                    min="1"
                                    max="60"
                                />
                            </div>

                            <div class="form-group">
                                <label for="ram">RAM (GB)</label>
                                <input
                                    type="number"
                                    id="ram"
                                    v-model="productForm.ram"
                                    placeholder="1-32 GB"
                                    min="1"
                                    max="32"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="storage">Bộ nhớ trong (GB)</label>
                                <input
                                    type="number"
                                    id="storage"
                                    v-model="productForm.storage"
                                    placeholder="8-2048 GB"
                                    min="8"
                                    max="2048"
                                />
                            </div>

                            <div class="form-group">
                                <label for="screenSize"
                                    >Kích thước màn hình (inch)</label
                                >
                                <input
                                    type="number"
                                    id="screenSize"
                                    v-model="productForm.screenSize"
                                    placeholder="3.0-15.0 inch"
                                    min="3"
                                    max="15"
                                    step="0.1"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="screenResolution"
                                    >Độ phân giải màn hình</label
                                >
                                <input
                                    type="text"
                                    id="screenResolution"
                                    v-model="productForm.screenResolution"
                                    placeholder="Định dạng: 1234x567"
                                    pattern="\d{3,4}x\d{3,4}"
                                />
                            </div>

                            <div class="form-group">
                                <label for="battery"
                                    >Dung lượng pin (mAh)</label
                                >
                                <input
                                    type="number"
                                    id="battery"
                                    v-model="productForm.battery"
                                    placeholder="1000-10000 mAh"
                                    min="1000"
                                    max="10000"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="processor">Vi xử lý</label>
                                <input
                                    type="text"
                                    id="processor"
                                    v-model="productForm.processor"
                                    placeholder="Tên vi xử lý (tối đa 100 ký tự)"
                                    maxlength="100"
                                />
                            </div>

                            <div class="form-group">
                                <label for="os">Hệ điều hành</label>
                                <input
                                    type="text"
                                    id="os"
                                    v-model="productForm.os"
                                    placeholder="Tên hệ điều hành (tối đa 50 ký tự)"
                                    maxlength="50"
                                />
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="simSlots">Số khe SIM</label>
                                <select
                                    id="simSlots"
                                    v-model="productForm.simSlots"
                                >
                                    <option :value="1">1 SIM</option>
                                    <option :value="2">2 SIM</option>
                                    <option :value="3">3 SIM</option>
                                    <option :value="4">4 SIM</option>
                                </select>
                            </div>
                        </div>
                    </div>

                    <!-- First Product Color Section -->
                    <div class="form-section">
                        <div class="section-header">
                            <h4 class="section-title">
                                <i class="fas fa-palette"></i> Màu sắc đầu tiên
                                (Tùy chọn)
                            </h4>
                            <div class="section-subtitle">
                                <i class="fas fa-info-circle"></i> Có thể thêm
                                màu sau khi tạo sản phẩm
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label for="colorName">
                                    Tên màu <span class="required">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="colorName"
                                    v-model="colorForm.name"
                                    placeholder="Nhập tên màu (ví dụ: Xanh, Đỏ, Vàng)"
                                />
                                <span
                                    v-if="formErrors.colorName"
                                    class="error-message"
                                >
                                    {{ formErrors.colorName }}
                                </span>
                            </div>

                            <div class="form-group">
                                <label for="colorQuantity"> Số lượng </label>
                                <input
                                    type="number"
                                    id="colorQuantity"
                                    v-model="colorForm.quantity"
                                    placeholder="Nhập số lượng (từ 0 trở lên)"
                                    min="0"
                                />
                                <span
                                    v-if="formErrors.colorQuantity"
                                    class="error-message"
                                >
                                    {{ formErrors.colorQuantity }}
                                </span>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-group full-width">
                                <label for="colorImages"> Hình ảnh </label>
                                <div class="file-upload-container">
                                    <input
                                        type="file"
                                        id="colorImages"
                                        ref="colorImageInput"
                                        @change="handleColorImageUpload"
                                        multiple
                                        accept="image/png,image/jpeg,image/jpg"
                                        class="file-input"
                                    />
                                    <button
                                        type="button"
                                        @click="$refs.colorImageInput.click()"
                                        class="file-upload-button"
                                    >
                                        <i class="fas fa-upload"></i> Chọn ảnh
                                    </button>
                                    <span class="file-name">
                                        {{ colorForm.images.length }} ảnh được
                                        chọn
                                    </span>
                                </div>
                                <span
                                    v-if="formErrors.colorImages"
                                    class="error-message"
                                >
                                    {{ formErrors.colorImages }}
                                </span>
                            </div>
                        </div>

                        <div
                            v-if="colorForm.imagePreviews.length > 0"
                            class="image-preview-container"
                        >
                            <div
                                v-for="(
                                    preview, index
                                ) in colorForm.imagePreviews"
                                :key="index"
                                class="image-preview-item"
                                :class="{
                                    'main-image':
                                        index === colorForm.mainImageIndex,
                                }"
                            >
                                <img :src="preview" alt="Ảnh màu sắc" />
                                <div class="image-preview-actions">
                                    <button
                                        type="button"
                                        @click="setMainImage(index)"
                                        :disabled="
                                            index === colorForm.mainImageIndex
                                        "
                                        :class="{
                                            active:
                                                index ===
                                                colorForm.mainImageIndex,
                                        }"
                                        title="Đặt làm ảnh chính"
                                    >
                                        <i class="fas fa-star"></i>
                                    </button>
                                    <button
                                        type="button"
                                        @click="removeImage(index)"
                                        title="Xóa ảnh"
                                    >
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Submit Button Section -->
                    <div class="form-actions">
                        <button
                            type="button"
                            @click="$emit('close')"
                            class="cancel-button"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                        <button
                            type="button"
                            @click="saveProduct"
                            class="submit-button"
                            :disabled="saving"
                        >
                            <span v-if="saving" class="spinner small"></span>
                            <span v-else
                                ><i class="fas fa-save"></i> Lưu sản phẩm</span
                            >
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, defineEmits, computed, watch } from "vue";
import productService from "../../../services/productService.js";
import emitter from "../../../utils/evenBus.js";

// Utility functions for notifications
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

const props = defineProps({
    showModal: {
        type: Boolean,
        required: true,
    },
    brands: {
        type: Array,
        default: () => [],
    },
    productLines: {
        type: Array,
        default: () => [],
    },
});

const emit = defineEmits(["close", "brand-change", "product-created"]);

// Form states
const productForm = ref({
    name: "",
    brandId: null,
    productLineId: null,
    importPrice: "",
    salePrice: "",
    description: "",
    warranty: "12",
    ram: "",
    storage: "",
    screenSize: "",
    screenResolution: "",
    battery: "",
    processor: "",
    os: "",
    simSlots: 1,
});

const colorForm = ref({
    name: "",
    quantity: 0,
    images: [],
    imagePreviews: [],
    mainImageIndex: 0,
});

// Watch for changes in props.brands to set a default brand if needed
watch(
    () => props.brands,
    (newBrands) => {
        if (newBrands && newBrands.length > 0 && !productForm.value.brandId) {
            // If no brand is selected but brands are available, select the first one
            productForm.value.brandId = newBrands[0].id;
            emit("brand-change", productForm.value.brandId);
        }
    },
    { immediate: true }
);

// Watch for modal visibility
watch(
    () => props.showModal,
    (isVisible) => {
        if (isVisible) {
            if (productForm.value.brandId) {                
                emit("brand-change", productForm.value.brandId);
            } else if (props.brands && props.brands.length > 0) {               
                productForm.value.brandId = props.brands[0].id;
                emit("brand-change", productForm.value.brandId);
            }
        }
    }
);

// Watch for changes in productLines to select the first one by default if needed
watch(
    () => props.productLines,
    (newProductLines) => {
        if (
            newProductLines &&
            newProductLines.length > 0 &&
            !productForm.value.productLineId
        ) {            
            productForm.value.productLineId = newProductLines[0].id;
        }
    },
    { immediate: true }
);

// Form errors
const formErrors = ref({
    name: "",
    brandId: "",
    productLineId: "",
    importPrice: "",
    salePrice: "",
    colorName: "",
    colorQuantity: "",
    colorImages: "",
});

// Loading states
const saving = ref(false);

const handleBrandChange = () => {
    productForm.value.productLineId = null;
    emit("brand-change", productForm.value.brandId);
};

const validateForm = () => {
    let isValid = true;

    // Reset errors
    Object.keys(formErrors.value).forEach((key) => {
        formErrors.value[key] = "";
    }); // Validate product basic info
    if (!productForm.value.name.trim()) {
        formErrors.value.name = "Tên sản phẩm không được để trống";
        isValid = false;
    }

    if (!productForm.value.brandId) {
        formErrors.value.brandId = "Vui lòng chọn thương hiệu";
        isValid = false;
    }

    if (!productForm.value.productLineId) {
        formErrors.value.productLineId = "Vui lòng chọn dòng sản phẩm";
        isValid = false;
    }
    if (!productForm.value.importPrice) {
        formErrors.value.importPrice = "Giá nhập không được để trống";
        isValid = false;
    } else if (
        isNaN(productForm.value.importPrice) ||
        Number(productForm.value.importPrice) <= 0
    ) {
        formErrors.value.importPrice = "Giá nhập phải lớn hơn 0";
        isValid = false;
    }
    if (!productForm.value.salePrice) {
        formErrors.value.salePrice = "Giá bán không được để trống";
        isValid = false;
    } else if (
        isNaN(productForm.value.salePrice) ||
        Number(productForm.value.salePrice) <= 0
    ) {
        formErrors.value.salePrice = "Giá bán phải lớn hơn 0";
        isValid = false;
    }

    // Validate product color if name is provided (i.e., user wants to add a color)
    if (colorForm.value.name.trim()) {
        // Validate quantity
        if (colorForm.value.quantity < 0) {
            formErrors.value.colorQuantity = "Số lượng không được âm";
            isValid = false;
        }
    }

    return isValid;
};

const handleColorImageUpload = (event) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    // Validate files
    let allValid = true;
    for (const file of files) {
        const validation = productService.validateImageFile(file);
        if (!validation.valid) {
            showErrorNotification(validation.message);
            allValid = false;
            break;
        }
    }

    if (!allValid) {
        return;
    }

    // Add files to color form
    for (const file of files) {
        colorForm.value.images.push(file);
        const reader = new FileReader();
        reader.onload = (e) => {
            colorForm.value.imagePreviews.push(e.target.result);
        };
        reader.readAsDataURL(file);
    }
};

const setMainImage = (index) => {
    colorForm.value.mainImageIndex = index;
};

const removeImage = (index) => {
    colorForm.value.images.splice(index, 1);
    colorForm.value.imagePreviews.splice(index, 1);

    // Adjust main image index if necessary
    if (index === colorForm.value.mainImageIndex) {
        colorForm.value.mainImageIndex = 0;
    } else if (index < colorForm.value.mainImageIndex) {
        colorForm.value.mainImageIndex--;
    }
};

const saveProduct = async () => {
    if (!validateForm()) {
        return;
    }

    saving.value = true;
    try {
        // Create FormData object for the product
        const formData = new FormData();

        // Add basic info
        formData.append("Name", productForm.value.name.trim());
        formData.append(
            "ImportPrice",
            productForm.value.importPrice.toString()
        );
        formData.append("SalePrice", productForm.value.salePrice.toString());
        formData.append("ProductLineId", productForm.value.productLineId);
        formData.append("Description", productForm.value.description || "");

        // Add technical specs
        if (productForm.value.warranty)
            formData.append("Warranty", productForm.value.warranty);
        if (productForm.value.ram)
            formData.append("RAM", productForm.value.ram);
        if (productForm.value.storage)
            formData.append("Storage", productForm.value.storage);
        if (productForm.value.screenSize)
            formData.append("ScreenSize", productForm.value.screenSize);
        if (productForm.value.screenResolution)
            formData.append(
                "ScreenResolution",
                productForm.value.screenResolution
            );
        if (productForm.value.battery)
            formData.append("Battery", productForm.value.battery);
        if (productForm.value.os) formData.append("OS", productForm.value.os);
        if (productForm.value.processor)
            formData.append("Processor", productForm.value.processor);
        if (productForm.value.simSlots)
            formData.append("SimSlots", productForm.value.simSlots);

        // Create product first
        const response = await productService.createProduct(formData);
        const productId = response.data.data.id;

        // Only create color if name is provided
        if (colorForm.value.name.trim()) {
            const colorData = {
                name: colorForm.value.name.trim(),
                quantity: parseInt(colorForm.value.quantity),
                images: colorForm.value.images,
                mainImageIndex: colorForm.value.mainImageIndex,
            };

            await productService.createProductColor(productId, colorData);
        }

        showSuccessNotification("Sản phẩm đã được tạo thành công!");

        // Reset form data
        productForm.value = {
            name: "",
            brandId: null,
            productLineId: null,
            importPrice: "",
            salePrice: "",
            description: "",
            warranty: "12",
            ram: "",
            storage: "",
            screenSize: "",
            screenResolution: "",
            battery: "",
            processor: "",
            os: "",
            simSlots: 1,
        };
        colorForm.value = {
            name: "",
            quantity: 0,
            images: [],
            imagePreviews: [],
            mainImageIndex: 0,
        };

        // Notify parent component
        emit("product-created");
        emit("close");
    } catch (error) {
        console.error("Error creating product:", error);

        let errorMessage = "Có lỗi xảy ra khi tạo sản phẩm.";
        if (error.response?.data?.message) {
            errorMessage = error.response.data.message;
        }

        showErrorNotification(errorMessage);
    } finally {
        saving.value = false;
    }
};

// Format currency function
function formatCurrency(value) {
    if (value === null || value === undefined || value === "") return "";
    const number = Number(value.toString().replace(/[^\d]/g, ""));
    if (isNaN(number)) return "";
    return number.toLocaleString("vi-VN");
}

const importPriceDisplay = ref("");
const salePriceDisplay = ref("");

watch(
    () => productForm.value.importPrice,
    (val) => {
        importPriceDisplay.value = formatCurrency(val);
    }
);
watch(
    () => productForm.value.salePrice,
    (val) => {
        salePriceDisplay.value = formatCurrency(val);
    }
);

const onImportPriceInput = (e) => {
    let raw = e.target.value.replace(/[^\d]/g, "");
    importPriceDisplay.value = formatCurrency(raw);
    productForm.value.importPrice = raw ? Number(raw) : "";
};
const onSalePriceInput = (e) => {
    let raw = e.target.value.replace(/[^\d]/g, "");
    salePriceDisplay.value = formatCurrency(raw);
    productForm.value.salePrice = raw ? Number(raw) : "";
};
</script>

<style scoped>
/* Modal Styling */
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    backdrop-filter: blur(3px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    width: 90%;
    max-width: 1000px;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
    position: sticky;
    top: 0;
    background-color: white;
    z-index: 10;
    border-radius: 12px 12px 0 0;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: #333;
}

.close-button {
    background: none;
    border: none;
    font-size: 1rem;
    color: #666;
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

.modal-form-container {
    padding: 1.5rem;
}

/* Form Section Styling */
.form-section {
    margin-bottom: 2rem;
    padding: 1.5rem;
    border-radius: 8px;
    background-color: #f8f9fa;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.form-section:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.section-title {
    margin: 0;
    font-weight: 600;
    font-size: 1.1rem;
    color: #444;
}

.section-title i {
    margin-right: 8px;
    color: var(--primary-color, #f86ed3);
}

.section-subtitle {
    font-size: 0.85rem;
    color: #666;
    margin-top: 0.25rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.section-subtitle i {
    color: var(--primary-color, #f86ed3);
    font-size: 0.9rem;
}

.save-section-button {
    padding: 0.5rem 1rem;
    background-color: var(--primary-color, #f86ed3);
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 0.85rem;
    cursor: pointer;
    display: flex;
    align-items: center;
}

.save-section-button:hover {
    background-color: #e94e9c;
}

.save-section-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.save-section-button i {
    margin-right: 6px;
}

/* Form Fields Styling */
.form-row {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-bottom: 1rem;
}

.form-group {
    flex: 1;
    min-width: 200px;
}

.form-group.full-width {
    flex-basis: 100%;
}

label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    font-weight: 500;
    color: #555;
}

input,
textarea,
select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 0.95rem;
    transition: border-color 0.2s;
}

input:focus,
textarea:focus,
select:focus {
    border-color: var(--primary-color, #f86ed3);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.select-wrapper {
    position: relative;
}

.select-wrapper:after {
    content: "\f107";
    font-family: "Font Awesome 5 Free";
    font-weight: 900;
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

select {
    appearance: none;
    cursor: pointer;
}

textarea {
    resize: vertical;
    min-height: 100px;
}

.required {
    color: #ef4444;
}

.error-message {
    display: block;
    color: #ef4444;
    font-size: 0.8rem;
    margin-top: 0.3rem;
}

/* File Upload Styling */
.file-upload-container {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.file-input {
    display: none;
}

.file-upload-button {
    padding: 0.75rem 1.25rem;
    background-color: var(--primary-color, #f86ed3);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 0.9rem;
}

.file-upload-button:hover {
    background-color: #e94e9c;
}

.file-name {
    font-size: 0.9rem;
    color: #666;
}

/* Image Preview Styling */
.image-preview-container {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-top: 1rem;
}

.image-preview-item {
    position: relative;
    width: 100px;
    height: 100px;
    border-radius: 8px;
    overflow: hidden;
    border: 2px solid #eee;
}

.image-preview-item.main-image {
    border-color: var(--primary-color, #f86ed3);
}

.image-preview-item img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.image-preview-actions {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    display: flex;
    justify-content: space-between;
    padding: 5px;
    background-color: rgba(0, 0, 0, 0.5);
}

.image-preview-actions button {
    background: none;
    border: none;
    color: white;
    cursor: pointer;
    font-size: 0.8rem;
    padding: 3px;
    border-radius: 3px;
}

.image-preview-actions button:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

.image-preview-actions button.active {
    color: var(--primary-color, #f86ed3);
}

/* Submit Button Styling */
.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 2rem;
}

.cancel-button,
.submit-button {
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
    background-color: var(--primary-color, #f86ed3);
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

.submit-button i {
    margin-right: 8px;
}

/* Loading Spinner */
.spinner {
    display: inline-block;
    width: 20px;
    height: 20px;
    border: 2px solid rgba(248, 110, 211, 0.1);
    border-radius: 50%;
    border-top-color: white;
    animation: spin 1s linear infinite;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
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
</style>
