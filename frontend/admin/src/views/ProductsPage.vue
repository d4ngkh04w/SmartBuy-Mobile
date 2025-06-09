<template>
    <div class="products-management">
        <div class="page-header">
            <div class="header-top">
                <button class="back-button" @click="goBack">
                    <i class="fas fa-arrow-left"></i> Quay lại Dashboard
                </button>
                <h1>Quản lý Sản phẩm</h1>
                <button class="invisible-button" style="visibility: hidden">
                    <i class="fas fa-sync-alt"></i> Nút ẩn
                </button>
            </div>
        </div>

        <div class="product-content">
            <ProductManagement />
        </div>
    </div>
</template>

<script setup>
import { useRouter } from "vue-router";
import ProductManagement from "@/components/product/ProductManagement.vue";

const router = useRouter();

function goBack() {
    router.push("/dashboard");
}
</script>

<style scoped>
.products-management {
    min-height: 100vh;
    background-color: #f9f9f9;
    padding: 2rem;
}

.page-header {
    display: flex;
    flex-direction: column;
    margin-bottom: 2rem;
    background-color: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.header-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    margin-bottom: 0.5rem;
}

.back-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #fff;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.back-button i {
    color: var(--primary-color);
}

.back-button:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

.invisible-button {
    background-color: transparent;
    border: none;
    cursor: default;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.page-header h1 {
    margin: 0;
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
}

.product-content {
    background-color: white;
    border-radius: 12px;
    padding: 1rem;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

@media (max-width: 768px) {
    .products-management {
        padding: 1rem;
    }

    .page-header {
        padding: 1.5rem;
    }

    .header-top {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }
}
</style>
