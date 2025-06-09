<template>
    <div class="account-page">
        <div class="container">
            <div class="header-section">
                <button class="back-btn" @click="goToHome">
                    <i class="fas fa-chevron-left"></i> Quay lại
                </button>
                <h1 class="page-title">Tài khoản của tôi</h1>
            </div>

            <div class="account-layout">
                <!-- Sidebar menu -->
                <div class="sidebar">
                    <sidebar-menu
                        v-model:active-menu="activeMenu"
                        @logout="showLogoutConfirm = true"
                    />
                </div>

                <!-- Main content area -->
                <div class="main-content">
                    <!-- User Profile Tab -->
                    <profile-info
                        v-if="activeMenu === 'profile'"
                        v-model:user-data="userData"
                        @change-password="activeMenu = 'change-password'"
                    />
                    <!-- Password Change Tab -->
                    <password-change
                        v-else-if="activeMenu === 'change-password'"
                        @back="activeMenu = 'profile'"
                    />

                    <OrderManagement
                        v-else-if="activeMenu === 'orders'"
                        @back="activeMenu = 'profile'"
                    />
                    <PurchaseHistory
                        v-else-if="activeMenu === 'purchase-history'"
                        @back="activeMenu = 'profile'"
                    />
                </div>
            </div>
        </div>
        <ConfirmationModal
            v-if="showLogoutConfirm"
            title="Xác nhận đăng xuất"
            message="Bạn có chắc chắn muốn đăng xuất không?"
            @confirm="confirmLogout"
            @cancel="showLogoutConfirm = false"
        />
    </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import authService from "@/services/authService";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";
import SidebarMenu from "@/components/account/SidebarMenu.vue";
import ProfileInfo from "@/components/account/ProfileInfo.vue";
import PasswordChange from "@/components/account/PasswordChange.vue";
import OrderManagement from "./OrderManagement.vue";
import PurchaseHistory from "./PurchaseHistory.vue";
import ConfirmationModal from "@/components/common/ConfirmationModal.vue";

const route = useRoute();
const router = useRouter();
const activeMenu = ref("profile");
const loading = ref(false);
const showLogoutConfirm = ref(false);

// Hàm xử lý quay về trang chủ
const goToHome = () => {
    router.push("/");
};

// Single source of truth for user data
const userData = ref({
    name: "",
    email: "",
    emailConfirmed: false,
    phoneNumber: "",
    phoneNumberConfirmed: false,
    createdAt: "",
    avatar: "",
    gender: "",
    address: "",
    dateOfBirth: "",
    avatarFile: null,
});

// Fetch user data
const fetchUserData = async () => {
    loading.value = true;
    try {
        const data = await meService.getMe();

        if (data) {
            // Update user data
            userData.value = {
                ...data,
                avatarFile: null,
            };
        }
    } catch (error) {
        console.error("Error fetching user data:", error);
        if (error.response && error.response.status === 401) {
            router.push("/login");
        }
    } finally {
        loading.value = false;
    }
};

// Hàm xử lý đăng xuất
const handleLogout = async () => {
    try {
        await authService.logout();
        emitter.emit("user-logged-out");
        router.push("/");
    } catch (error) {
        console.error("Error during logout:", error);
    }
};

const confirmLogout = () => {
    showLogoutConfirm.value = false;
    handleLogout();
};

watch(activeMenu, () => {
    router.replace({ query: {} }); // Xoá hết query trên URL
});
onMounted(() => {
    fetchUserData();
    if (route.query.section === "orders") {
        activeMenu.value = "orders";
    }
    emitter.on("toPurchaseHistory", () => {
        activeMenu.value = "purchase-history";
    });
});
onBeforeUnmount(() => {
    emitter.off("toPurchaseHistory");
});
</script>

<style scoped>
.account-page {
    padding: 1rem 0;
    min-height: 100vh;
    width: 100%;
}

.header-section {
    margin-bottom: 1.5rem;
    position: relative;
    text-align: center;
    padding: 0.5rem 0;
}

.back-btn {
    display: inline-flex;
    align-items: center;
    background-color: transparent;
    border: none;
    color: #333;
    padding: 0.5rem 1rem;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.2s ease;
    position: absolute;
    left: 0;
    top: 50%;
    transform: translateY(-50%);
}

.back-btn:hover {
    opacity: 0.8;
}

.back-btn i {
    margin-right: 0.5rem;
    color: #ff69b4; /* Màu hồng cho mũi tên */
    font-size: 1.3rem;
}

.page-title {
    font-size: 2rem;
    margin: 0 auto;
    color: #333;
    font-weight: 600;
    display: inline-block;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 15px;
}

.account-layout {
    display: flex;
    gap: 2rem;
}

.sidebar {
    width: 250px;
    flex-shrink: 0;
}

.main-content {
    flex: 1;
    padding: 0;
    background-color: #ffffff;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

@media (max-width: 992px) {
    .account-layout {
        flex-direction: column;
    }

    .sidebar {
        width: 100%;
        margin-bottom: 1.5rem;
    }
}
</style>
