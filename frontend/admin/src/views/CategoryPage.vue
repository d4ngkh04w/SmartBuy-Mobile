<template>
	<div class="category-management">
		<div class="page-header">
			<div class="header-top">
				<button class="back-button" @click="goBack">
					<i class="fas fa-arrow-left"></i> Quay lại Dashboard
				</button>
				<h1>Quản lý Danh mục</h1>
				<button class="invisible-button" style="visibility: hidden">
					<i class="fas fa-sync-alt"></i> Nút ẩn
				</button>
			</div>
		</div>
		<!-- Tabs for different category management sections -->
		<div class="category-tabs">
			<button
				@click="activeTab = 'brands'"
				:class="['tab-btn', activeTab === 'brands' ? 'active-tab' : '']"
			>
				<i class="fas fa-trademark"></i> Thương hiệu
			</button>
			<button
				@click="activeTab = 'productLines'"
				:class="[
					'tab-btn',
					activeTab === 'productLines' ? 'active-tab' : '',
				]"
			>
				<i class="fas fa-mobile-alt"></i> Dòng sản phẩm
			</button>
		</div>
		<!-- Content based on active tab -->
		<div class="tab-content">
			<transition name="fade" mode="out-in">
				<BrandManagement v-if="activeTab === 'brands'" />
				<ProductLineManagement
					v-else-if="activeTab === 'productLines'"
				/>
			</transition>
		</div>
	</div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import BrandManagement from "../components/category/BrandManagement.vue";
import ProductLineManagement from "../components/category/ProductLineManagement.vue";

const router = useRouter();
const activeTab = ref("brands");

function goBack() {
	router.push("/dashboard");
}
</script>

<style scoped>
.category-management {
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

.category-tabs {
	display: flex;
	border-bottom: 1px solid #e2e8f0;
	margin-bottom: 1.5rem;
	background-color: white;
	border-radius: 12px 12px 0 0;
	box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
	overflow: hidden;
}

.tab-btn {
	padding: 1rem 1.5rem;
	background-color: white;
	border: none;
	cursor: pointer;
	color: #666;
	font-weight: 500;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	transition: all 0.3s;
	position: relative;
}

.tab-btn i {
	font-size: 1.2rem;
}

.tab-btn:hover {
	color: var(--primary-color);
	background-color: #fff5fc;
}

.active-tab {
	color: var(--primary-color);
	border-bottom: 3px solid var(--primary-color);
	font-weight: 600;
}

.tab-content {
	background-color: white;
	border-radius: 0 0 12px 12px;
	padding: 1.5rem;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.fade-enter-active,
.fade-leave-active {
	transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
	opacity: 0;
}
</style>
