<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import emitter from "../../utils/evenBus.js"; // Import emitter từ eventBus.js

const notifications = ref([]); // Chứa tất cả các thông báo

const handler = (payload) => {
	// Use the specified duration or default to 3000ms
	const duration = payload.duration || 3000;

	const notification = {
		id: Date.now(), // Sử dụng id duy nhất cho từng thông báo
		status: payload.status,
		message: payload.message,
		show: true,
		position: payload.position || "bottom-right", // Default position
	};
	notifications.value.push(notification);

	// Tự động ẩn thông báo sau khoảng thời gian duration
	setTimeout(() => {
		notification.show = false;
	}, duration);

	// Xóa thông báo khỏi danh sách khi nó đã ẩn
	setTimeout(() => {
		notifications.value = notifications.value.filter(
			(n) => n.id !== notification.id
		);
	}, duration + 500); // Thời gian để hoàn tất việc ẩn
};

onMounted(() => {
	emitter.on("show-notification", handler);
});

onUnmounted(() => {
	emitter.off("show-notification", handler);
});
</script>

<template>
	<!-- Group notifications by position -->
	<div
		v-for="position in [
			'top-left',
			'top-center',
			'top-right',
			'bottom-left',
			'bottom-center',
			'bottom-right',
		]"
		:key="position"
		class="notificationBox"
		:class="position"
	>
		<transition-group name="slide-fade" tag="div">
			<div
				v-for="notification in notifications.filter(
					(n) => n.position === position
				)"
				:key="notification.id"
				class="notification"
				:class="notification.status"
			>
				<i
					class="fa-solid"
					:class="{
						'fa-circle-check': notification.status === 'success',
						'fa-circle-xmark': notification.status === 'error',
						'fa-circle-exclamation':
							notification.status === 'warning',
					}"
				></i>
				<div>{{ notification.message }}</div>
				<div class="progress-bar" :class="notification.status"></div>
			</div>
		</transition-group>
	</div>
</template>

<style>
.notificationBox {
	position: fixed;
	display: flex;
	flex-direction: column;
	z-index: 9999;
}

/* Position classes */
.notificationBox.top-left {
	top: 10px;
	left: 10px;
	align-items: flex-start;
}

.notificationBox.top-center {
	top: 10px;
	left: 50%;
	transform: translateX(-50%);
	align-items: center;
}

.notificationBox.top-right {
	top: 10px;
	right: 10px;
	align-items: flex-end;
}

.notificationBox.bottom-left {
	bottom: 10px;
	left: 10px;
	align-items: flex-start;
}

.notificationBox.bottom-center {
	bottom: 10px;
	left: 50%;
	transform: translateX(-50%);
	align-items: center;
}

.notificationBox.bottom-right {
	bottom: 10px;
	right: 10px;
	align-items: flex-end;
}

.notification {
	width: 300px;
	background: #fff;
	font-weight: 500;
	margin: 10px 0;
	box-shadow: 0 0 20px rgba(0, 0, 0, 0.3);
	display: flex;
	align-items: center;
	position: relative;
	padding: 15px;
	gap: 10px;
	transition: all 0.5s ease;
}

.notification.success {
	background: #d4edda;
	color: #155724;
}
.notification.error {
	background: #f8d7da;
	color: #721c24;
}
.notification.warning {
	background: #fff3cd;
	color: #856404;
}

.notification i {
	font-size: 35px;
}

.notification.success i {
	color: green;
}

.notification.error i {
	color: red;
}

.notification.warning i {
	color: orange;
}

.progress-bar {
	position: absolute;
	bottom: 0;
	left: 0;
	height: 5px;
	width: 100%;
	animation: progress 2s linear forwards;
}

.progress-bar.success {
	background: green;
}
.progress-bar.error {
	background: red;
}
.progress-bar.warning {
	background: orange;
}

@keyframes progress {
	100% {
		width: 0;
	}
}

/* --------- TRANSITION ----------- */
.slide-fade-enter-active,
.slide-fade-leave-active {
	transition: all 0.8s ease;
}

.slide-fade-enter-from {
	opacity: 0;
	transform: translateX(100%);
}
.slide-fade-enter-to {
	opacity: 1;
	transform: translateX(0);
}

.slide-fade-leave-from {
	opacity: 1;
	transform: translateX(0);
}
.slide-fade-leave-to {
	opacity: 0;
	transform: translateX(100%);
}
</style>
