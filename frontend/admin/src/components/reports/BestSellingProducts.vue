<script setup>
import { ref, computed, watch } from "vue";
import { formatCurrency, formatDate } from "../../utils/dateTimeUtils.js";

const props = defineProps({
    products: {
        type: Array,
        required: true,
    },
    loading: {
        type: Boolean,
        default: false,
    },
    timeFilter: {
        type: String,
        default: "month",
    },
    startDate: {
        type: String,
        default: "",
    },
    endDate: {
        type: String,
        default: "",
    },
});

const sortBy = ref("quantity");
const formatID = (id) => {
    return "SP" + id?.toString().padStart(4, "0") || "N/A";
};
const sortedProducts = computed(() => {
    if (!props.products || props.products.length === 0) return [];

    return [...props.products].sort((a, b) => {
        if (sortBy.value === "quantity") {
            return b.quantity - a.quantity;
        } else if (sortBy.value === "revenue") {
            return b.revenue - a.revenue;
        }
        return 0;
    });
});

function sortProducts() {
    // Sorting is handled by the computed property
}

// Format number with commas
function formatNumber(num) {
    return num?.toLocaleString("vi-VN") || "0";
}

// Get CSS class for rank badges
function getRankClass(index) {
    if (index === 0) return "rank-first";
    if (index === 1) return "rank-second";
    if (index === 2) return "rank-third";
    return "rank-default";
}

// Watch for changes in props to update if needed
watch(
    () => props.timeFilter,
    () => {
        // In a real implementation, you would fetch new data here
        // based on the time filter changes
    }
);
</script>

<template>
    <div class="best-selling-products">
        <div class="section-header">
            <div class="left-section">
                <h2>
                    <i class="fas fa-trophy"></i>
                    Sản phẩm bán chạy
                </h2>
                <p>Danh sách các sản phẩm có doanh số bán hàng cao nhất</p>
            </div>
            <div class="right-section">
                <div class="filter-section">
                    <div class="filter-group">
                        <i class="fas fa-sort"></i>
                        <select v-model="sortBy" @change="sortProducts">
                            <option value="quantity">Số lượng bán</option>
                            <option value="revenue">Doanh thu</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>

            <div v-else-if="sortedProducts.length === 0" class="empty-state">
                <i class="fas fa-inbox"></i>
                <p>Không có dữ liệu sản phẩm trong khoảng thời gian này</p>
            </div>

            <div v-else>
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>STT</th>
                            <th>Tên sản phẩm</th>
                            <th>Mã SP</th>
                            <th>Số lượng bán</th>
                            <th>Doanh thu</th>
                            <th>Ngày bắt đầu bán</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="(product, index) in sortedProducts"
                            :key="product.id"
                        >
                            <td>
                                <div
                                    class="rank-badge"
                                    :class="getRankClass(index)"
                                >
                                    {{ index + 1 }}
                                </div>
                            </td>
                            <td>
                                <div class="product-name">
                                    {{ product.name }}
                                </div>
                            </td>
                            <td>
                                <div class="product-sku">{{ formatID(product.id) }}</div>
                            </td>
                            <td>
                                <div class="quantity-cell">
                                    <i class="fas fa-boxes"></i>
                                    {{ formatNumber(product.quantity) }}
                                </div>
                            </td>
                            <td>
                                <div class="revenue-cell">
                                    <i class="fas fa-dollar-sign"></i>
                                    {{ formatCurrency(product.revenue) }}
                                </div>
                            </td>
                            <td>
                                <div class="date-cell">
                                    {{ formatDate(product.firstSoldDate) }}
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<style scoped>
.best-selling-products {
    width: 100%;
    margin-bottom: 1.5rem;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
    flex-wrap: wrap;
    gap: 1rem;
}

.left-section h2 {
    font-size: 1.25rem;
    color: #333;
    margin: 0 0 0.25rem 0;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.left-section h2 i {
    color: #ffd700;
}

.left-section p {
    color: #666;
    margin: 0;
    font-size: 0.9rem;
}

.right-section {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    align-items: center;
}

.filter-section {
    display: flex;
    gap: 0.5rem;
    align-items: center;
}

.filter-group {
    position: relative;
}

.filter-group i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.filter-group select {
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%236e6e6e' viewBox='0 0 16 16'%3E%3Cpath d='M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 8px center;
    padding-right: 28px;
    transition: all 0.3s;
}

.filter-group select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

/* Data Card */
.data-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
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

/* Data Table */
.data-table {
    width: 100%;
    border-collapse: collapse;
}

.data-table th {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid #eee;
    text-align: left;
    font-weight: 600;
    color: #333;
    font-size: 0.9rem;
    white-space: nowrap;
}

.data-table td {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid #f5f5f5;
    font-size: 0.9rem;
    color: #555;
    vertical-align: middle;
}

.data-table tr:hover td {
    background-color: #f9f9f9;
}

.rank-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    font-weight: 700;
    font-size: 0.9rem;
}

.rank-first {
    background: linear-gradient(135deg, #ffd700, #ffed4e);
    color: #744210;
    box-shadow: 0 2px 8px rgba(255, 215, 0, 0.3);
}

.rank-second {
    background: linear-gradient(135deg, #c0c0c0, #e2e8f0);
    color: #4a5568;
    box-shadow: 0 2px 8px rgba(192, 192, 192, 0.3);
}

.rank-third {
    background: linear-gradient(135deg, #cd7f32, #d69e2e);
    color: white;
    box-shadow: 0 2px 8px rgba(205, 127, 50, 0.3);
}

.rank-default {
    background: linear-gradient(135deg, #e2e8f0, #cbd5e0);
    color: #4a5568;
}

.product-name {
    font-weight: 600;
    color: #333;
}

.product-sku {
    font-family: monospace;
    font-size: 0.85rem;
    color: #718096;
    background: #f7fafc;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    display: inline-block;
}

.quantity-cell,
.revenue-cell {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-weight: 500;
}

.quantity-cell i {
    color: #3182ce;
}

.revenue-cell i {
    color: #38a169;
}

.date-cell {
    color: #718096;
    font-size: 0.9rem;
}

@media (max-width: 768px) {
    .section-header {
        flex-direction: column;
        align-items: stretch;
        text-align: center;
    }

    .data-table {
        font-size: 0.8rem;
    }

    .data-table th,
    .data-table td {
        padding: 0.5rem;
    }
}

</style>
