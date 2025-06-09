<template>
    <div class="order-management">
        <h2 class="page-title">Đơn hàng</h2>

        <div v-if="loading && !(orders.length === 0)" class="loading-state">
            <Loading />
        </div>

        <div v-else-if="error" class="error-state">
            <i class="fas fa-exclamation-circle"></i> Đã xảy ra lỗi khi tải đơn
            hàng. Vui lòng thử lại sau.
        </div>

        <div v-else-if="orders.length === 0" class="empty-state">
            <i class="fas fa-box-open"></i> Hiện tại bạn không có đơn hàng nào.
        </div>

        <div v-else class="order-list">
            <div v-for="order in orders" :key="order.id" class="order-card">
                <div class="order-header">
                    <span class="order-id"
                        >Mã đơn hàng: #{{ getOrderID(order.id) }}</span
                    >
                    <span
                        :class="['order-status', getStatusClass(order.status)]"
                        ><i
                            :class="['fa-solid', getStatusIcon(order.status)]"
                        ></i>
                        {{ order.status }}
                    </span>
                </div>
                <div class="order-body">
                    <div class="product-summary">
                        <div
                            v-for="item in order.orderItems.slice(0, 2)"
                            :key="item.id"
                            class="product-item-preview"
                            @click="goToProductDetail(item.product.id)"
                        >

                            <img
                                :src="
                                    productService.getUrlImage(item.colorImage)
                                "
                                :alt="item.name"
                                class="product-thumbnail"
                            />
                            <div class="product-info-preview">
                                <p class="product-name">
                                    {{ item.product.name }}
                                </p>
                                <p class="product-color">
                                    {{ item.colorName }}
                                </p>
                                <p class="product-quantity">
                                    x{{ item.quantity }}
                                </p>
                            </div>
                        </div>
                        <p
                            v-if="order.orderItems.length > 2"
                            class="more-items"
                        >
                            và {{ order.orderItems.length - 2 }} sản phẩm
                            khác...
                        </p>
                    </div>
                    <div class="order-details">
                        <p class="order-date">
                            Ngày đặt: {{ format.formatDate(order.orderDate) }}
                        </p>
                        <p class="order-total">
                            Tổng tiền:
                            {{ format.formatPrice(order.totalAmount) }}₫
                        </p>
                    </div>
                </div>
                <div class="order-actions">
                    <button @click="showPopup(order)" class="btn btn-primary">
                        Xem chi tiết
                    </button>
                    <button
                        v-if="canCancel(order.status)"
                        @click="cancelOrderConfirm(order)"
                        class="btn btn-secondary"
                    >
                        Hủy đơn hàng
                    </button>
                    <button
                        v-if="canConfirmOrReturn(order.status)"
                        @click="confirmDelivery(order.id)"
                        class="btn btn-confirm"
                    >
                        Đã nhận hàng
                    </button>
                    <button
                        v-if="canConfirmOrReturn(order.status)"
                        @click="returnOrderConfirm(order)"
                        class="btn btn-return"
                    >
                        Hoàn hàng
                    </button>
                </div>
            </div>
        </div>

        <!-- Popup chi tiết đơn hàng -->
        <div v-if="popupVisible" class="popup-overlay" @click.self="closePopup">
            <div class="popup-content">
                <h3>Chi tiết đơn hàng #{{ getOrderID(selectedOrder.id) }}</h3>
                <p>
                    <strong>Ngày đặt:</strong>
                    {{ format.formatDate(selectedOrder.orderDate) }}
                </p>
                <p><strong>Trạng thái:</strong> {{ selectedOrder.status }}</p>
                <p>
                    <strong>Tổng tiền:</strong>
                    {{ format.formatPrice(selectedOrder.totalAmount) }}₫
                </p>

                <h4>Sản phẩm:</h4>
                <ul class="popup-product-list">
                    <li
                        v-for="item in selectedOrder.orderItems"
                        :key="item.id"
                        class="popup-product-item"
                        @click="goToProductDetail(item.product.id)"
                    >
                        <img
                            :src="productService.getUrlImage(item.colorImage)"
                            :alt="item.product.name"
                            class="popup-product-thumbnail"
                        />
                        <div>
                            <p class="popup-product-name">
                                {{ item.product.name }}
                            </p>
                            <p>Màu sắc: {{ item.colorName }}</p>
                            <p>Số lượng: {{ item.quantity }}</p>
                            <p>Giá: {{ format.formatPrice(item.price) }}</p>
                        </div>
                    </li>
                </ul>
                <div class="container-btn">
                    <button @click="closePopup" class="btn btn-close">
                        Đóng
                    </button>
                </div>
            </div>
        </div>
        <!-- popup Cancer Order -->
        <div v-if="popupCancel" class="popup-overlay" @click.self="closePopup">
            <div class="confirm-cancel popup-content">
                <i class="fa-solid fa-circle-exclamation"></i>
                <p>
                    Bạn có chắc chắn muốn hủy đơn hàng #{{
                        getOrderID(selectedOrder.id)
                    }}
                    không?
                </p>
                <p>Hành động này sẽ không thể hoàn tác.</p>
                <div class="order-actions">
                    <button
                        @click="cancelOrder(selectedOrder.id)"
                        class="btn btn-secondary"
                    >
                        Xác nhận hủy
                    </button>
                    <button @click="closePopup" class="btn btn-close">
                        Quay lại
                    </button>
                </div>
            </div>
        </div>
        <!-- popup Return Order -->
        <div v-if="popupReturn" class="popup-overlay" @click.self="closePopup">
            <div class="confirm-return popup-content">
                <i class="fa-solid fa-circle-exclamation"></i>
                <p>
                    Bạn có chắc chắn muốn hoàn đơn hàng #{{
                        getOrderID(selectedOrder.id)
                    }}
                    không?
                </p>
                <p>Hành động này sẽ không thể hoàn tác.</p>
                <div class="order-actions">
                    <button
                        @click="returnOrder(selectedOrder.id)"
                        class="btn btn-secondary"
                    >
                        Xác nhận hoàn
                    </button>
                    <button @click="closePopup" class="btn btn-close">
                        Quay lại
                    </button>
                </div>
            </div>
        </div>
        <!-- Pagination - chỉ hiển thị khi có đơn hàng -->
        <Pagi
            v-if="orders.length > 0"
            :totalProducts="totalOrders"
            :currentPage="currentPage"
            :pageSize="pageSize"
            @pageChanged="getOrdersInPage"
        />
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import Loading from "../components/common/Loading.vue";
import format from "@/utils/format";
import productService from "@/services/productService";
import emitter from "../utils/evenBus.js";
import Pagi from "../components/common/Pagination.vue";
const router = useRouter();
const orders = ref([]);
const allOrders = ref([]);
const loading = ref(true);
const error = ref(false);

const popupVisible = ref(false);
const selectedOrder = ref(null);
const popupCancel = ref(false);
const popupReturn = ref(false);
const pageSize = ref(5);
const currentPage = ref(1);
const totalOrders = ref(0);

const fetchOrders = async () => {
    loading.value = true;
    error.value = false;
    try {
        const data = await productService.getAllCurrentOrders();
        // Nếu API trả về null hoặc undefined, coi như mảng rỗng, không phải lỗi
        allOrders.value = Array.isArray(data) ? data : [];
        loading.value = false;

        // Cập nhật dữ liệu hiển thị
        orders.value = allOrders.value.slice(0, pageSize.value);
        totalOrders.value = allOrders.value.length;
    } catch (err) {
        console.error("Error fetching orders:", err);
        // Đặt mảng rỗng để hiển thị không có đơn hàng thay vì hiển thị lỗi
        // khi lỗi liên quan đến không tìm thấy dữ liệu (404)
        if (err.response && err.response.status === 404) {
            allOrders.value = [];
            orders.value = [];
            totalOrders.value = 0;
            error.value = false;
        } else {
            error.value = true;
        }
        loading.value = false;
    }
};

const getOrdersInPage = (page = 1) => {
    currentPage.value = page;
    const start = (currentPage.value - 1) * pageSize.value;
    const end = start + pageSize.value;
    orders.value = allOrders.value.slice(start, end);
    totalOrders.value = allOrders.value.length;
    window.scrollTo({
        top: 0,
        behavior: "smooth",
    });
};

const getOrderID = (order) => {
    return order.toString().substring(0, 8);
};
const getStatusClass = (status) => {
    switch (status) {
        case "Chờ xác nhận":
            return "status-pending";
        case "Đã xác nhận":
            return "status-processing";
        case "Đang giao hàng":
            return "status-shipping";
        case "Đã giao hàng":
            return "status-delivered";
        default:
            return "";
    }
};
const getStatusIcon = (status) => {
    switch (status) {
        case "Chờ xác nhận":
            return "fa-circle-question";
        case "Đã xác nhận":
            return "fa-hourglass-half";
        case "Đang giao hàng":
            return "fa-truck-fast";
        case "Đã giao hàng":
            return "fa-circle-check";
        default:
            return "";
    }
};

const canCancel = (status) => ["Chờ xác nhận", "Đã xác nhận"].includes(status);
const canConfirmOrReturn = (status) => ["Đã giao hàng"].includes(status);

const showPopup = (order) => {
    selectedOrder.value = order;
    popupVisible.value = true;
};

const closePopup = () => {
    popupVisible.value = false;
    popupCancel.value = false;
    selectedOrder.value = null;
    popupReturn.value = false;
};

const cancelOrderConfirm = (order) => {
    selectedOrder.value = order;
    popupCancel.value = true;
};
const returnOrderConfirm = (order) => {
    selectedOrder.value = order;
    popupReturn.value = true;
};
const confirmDelivery = (orderId) => {
    var status = "Hoàn thành";
    updateOrderStatus(orderId, status);
};
const updateOrderStatus = (orderId, status) => {
    productService
        .updateStatusOrder(orderId, status)
        .then(() => {
            closePopup();
            emitter.emit("toPurchaseHistory");
            emitter.emit("show-notification", {
                status: "success",
                message: `Đã cập nhật trạng thái đơn hàng thành ${status}`,
            });
        })
        .catch((err) => {
            emitter.emit("show-notification", {
                status: "error",
                message:
                    "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng. Vui lòng thử lại sau.",
            });
            console.error("Error updating order status:", err);
            error.value = true;
        });
};
const returnOrder = (orderId) => {
    var status = "Đã trả hàng";
    updateOrderStatus(orderId, status);
};
const goToProductDetail = (productId) => {
    router.push(`/product/${productId}`);
};

const cancelOrder = (orderId) => {
    productService
        .cancelOrder(orderId)
        .then(() => {
            emitter.emit("toPurchaseHistory");
            fetchOrders();
            closePopup();
            emitter.emit("show-notification", {
                status: "success",
                message: "Đã hủy đơn hàng",
            });
        })
        .catch((err) => {
            emitter.emit("show-notification", {
                status: "error",
                message:
                    "Đã xảy ra lỗi khi hủy đơn hàng. Vui lòng thử lại sau.",
            });
            error.value = true;
        });
};

onMounted(() => {
    fetchOrders();
    emitter.on('order-updated', fetchOrders);
});
</script>

<style scoped>
.order-management {
    min-height: calc(100vh - 100px);
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.page-title {
    font-weight: 600;
    text-shadow: 1px 1px 2px rgba(214, 51, 132, 0.1);
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
    background-color: #060606;
    border-radius: 10px;
}

.loading-state,
.error-state,
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 200px; /* hoặc chiều cao theo vùng bạn muốn */
    color: var(--primary-color);
    font-size: 16px;
    text-align: center;
    background-color: #f9f9f9;
    border-radius: 8px;
    padding: 20px;
}

.empty-state i {
    font-size: 40px;
    color: var(--primary-color);
    margin-bottom: 12px;
}


.order-list {
    display: flex;
    flex-direction: column;
    gap: 1.8rem;
}

.order-card {
    background-color: #fff;
    padding: 1.5rem;
    border-radius: 16px;
    box-shadow: 0 3px 7px rgba(241, 175, 215, 0.25);
    border: 1px solid #f9c3d7;
    transition: box-shadow 0.3s ease;
    user-select: none;
}

.order-card:hover {
    box-shadow: 0 6px 16px rgba(241, 175, 215, 0.5);
}

.order-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 1rem;
    font-weight: 600;
    font-size: 1.2rem;
}

.order-status {
    padding: 0.3rem 0.7rem;
    border-radius: 12px;
    font-weight: 600;
    font-size: 1rem;
    color: #fff;
    text-transform: uppercase;
    user-select: none;
}

.status-pending {
    background-color: #fff3f9;
    color: #f98ad8;
}

.status-processing {
    background-color: #fdeef7;
    color: #ea5cbf;
}

.status-shipping {
    background-color: #fce5f2;
    color: #e408a2;
}

.status-delivered {
    background-color: #f8e2f1;
    color: #c1118c;
}

.order-body {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 1rem;
}

.product-summary {
    flex: 1 1 50%;
    display: flex;
    align-items: center;
    gap: 1rem;
    flex-wrap: wrap;
}

.product-item-preview {
    display: flex;
    align-items: center;
    gap: 0.7rem;
    /* background-color: #f7d3e0; */
    padding: 0.3rem 0.6rem;
    border-radius: 10px;
    user-select: none;
    cursor: pointer;
}

.product-thumbnail {
    width: 50px;
    height: 50px;
    object-fit: contain;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(220, 38, 127, 0.3);
}

.product-info-preview {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    font-size: 0.9rem;
    color: #6b7280;
    user-select: none;
}

.product-name {
    font-weight: 600;
    font-size: 1rem;
    /* color: #ec4899; */
    text-shadow: 1px 1px 3px #ffe4ef;
    user-select: none;
}

.product-quantity {
    font-weight: 400;
    font-size: 0.85rem;
    user-select: none;
}

.more-items {
    font-size: 0.85rem;
    font-style: italic;
    color: #9ca3af;
    user-select: none;
}

.order-details {
    flex: 1 1 40%;
    display: flex;
    flex-direction: column;
    justify-content: end;
    align-items: end;
    gap: 0.4rem;
    font-weight: 600;
    font-size: 1.1rem;
    color: #4b5563;
    user-select: none;
}

.order-actions {
    margin-top: 1rem;
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    flex-wrap: wrap;
}

.btn {
    cursor: pointer;
    border: none;
    padding: 0.5rem 1.2rem;
    border-radius: 20px;
    font-weight: 600;
    font-size: 1rem;
    user-select: none;
    transition: background-color 0.3s ease, color 0.3s ease;
}

.btn-primary {
    background-color: #f47da4;  
    color: var(--primary-color);
    color: white;
}
.btn-primary:hover {
    background-color: #e65082; 
}   

.btn-secondary {
    background-color: #8e96a7;
    color: white;
}

.btn-confirm {
    background-color: #cef7d1;
    color: #2e7d32;
}
.btn-confirm:hover {
    background-color: #76f5cd;
}
.btn-return {
    background-color: #fff9c4;  
    color: #f57f17;             

}
.btn-return:hover {
    background-color: #e6f582;
}

.btn-secondary:hover {
    background-color: #616d7d;
}

.btn-info {
    background-color: #3b82f6;
    color: white;
}

.btn-info:hover {
    background-color: #2563eb;
}

/* Popup modal */

.popup-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background-color: rgba(0, 0, 0, 0.4);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
}

.popup-content {
    background-color: #fff;
    padding: 2rem 2.5rem;
    border-radius: 16px;
    width: 90%;
    max-width: 600px;
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.25);
    overflow-y: auto;
    max-height: 80vh;
}

.popup-content h3 {
    margin-bottom: 1rem;
    color: var(--primary-color);
    text-align: center;
    user-select: none;
}

.popup-content p {
    margin-bottom: 0.8rem;
    font-size: 1.1rem;
    user-select: none;
}

.popup-product-list {
    list-style: none;
    padding: 0;
    margin: 1rem 0 1.5rem;
}

.popup-product-item {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1rem;
    border-bottom: 1px solid #f3f4f6;
    padding-bottom: 1rem;
    user-select: none;
    cursor: pointer;
}

.popup-product-thumbnail {
    width: 60px;
    height: 60px;
    object-fit: contain;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(214, 51, 132, 0.2);
}

.popup-product-name {
    font-weight: 600;
    margin-bottom: 0.2rem;
    user-select: none;
}
.container-btn {
    text-align: end;
}

.btn-close {
    text-align: end;
    background-color: var(--primary-color);
    color: white;
    padding: 0.6rem 1.6rem;
    border-radius: 30px;
    font-weight: 600;
    font-size: 1.1rem;
    cursor: pointer;
    user-select: none;
    transition: background-color 0.3s ease;
}

.btn-close:hover {
    background-color: #eb2ddb;
}
.fa-circle-exclamation {
    font-size: 3rem;
    color: #f59e0b;
    margin-bottom: 1rem;
}
.confirm-cancel {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}
.confirm-return {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}
.popup-content {
    animation: popupFadeIn 0.3s ease-out;
}

@keyframes popupFadeIn {
    from {
        opacity: 0;
        transform: translateY(-20px) scale(0.95);
    }
    to {
        opacity: 1;
        transform: translateY(0) scale(1);
    }
}
.order-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
    flex-wrap: wrap;
}
</style>
