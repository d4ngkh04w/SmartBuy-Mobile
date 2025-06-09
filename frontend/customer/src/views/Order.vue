<template>
  <div class="order-container">
    <h1 class="order">Đặt hàng</h1>
    <!-- Thanh lộ trình -->
    <div class="steps">
      <div
        v-for="(label, index) in stepLabels"
        :key="index"
        :class="['step-item', step >= index + 1 ? 'active' : '', step === index + 1 ? 'current' : '']"
        @click="handleStepClick(index + 1)"
        :style="{ cursor: step === 3 ? 'not-allowed' : 'pointer' }"
      >
        {{ index + 1 }}. {{ label }}
      </div>
    </div>

    <!-- Bước 1: Thông tin -->
    <div v-if="step === 1" class="step-content">
      <form @submit.prevent="nextStep">
        <div>
          <h2>Sản phẩm</h2>
          <div class="product" v-for="product in products" :key="product.productId" :class="{ 'disabled-item': !product.isActive }">
            <img :src="productService.getUrlImage(product.imagePath)" alt="product" />
            <div class="product-info">
              <h3>{{ product.productName }}</h3>
              <button>{{ product.colorName }}</button>
              <p style="color: red">{{ format.formatPrice(product.price) }}₫</p>
              <span>Số lượng: {{ product.quantity }}</span>
            </div>
          </div>
          <div class="total-line">
            <strong>Tổng tiền:</strong>
            <span>{{ format.formatPrice(totalAmount) }}</span>
          </div>
        </div>
        <div>
          <h2>Thông tin khách hàng</h2>
          <input v-model="form.name" type="text" placeholder="Họ và tên" required />
          <input v-model="form.phone" type="tel" placeholder="Số điện thoại" required />
        </div>
        <AddressForm
          v-model="form.address"
          :initial-address="userInfo.address"
          @address-status="handleAddressStatus"
          @update:address="updateAddress"
        />
        <button type="submit" class="btn">Tiếp theo</button>
      </form>
    </div>

		<!-- Bước 2: Thanh toán -->
		<div v-else-if="step === 2" class="step-content">
			<h2>Phương thức thanh toán</h2>
			<form @submit.prevent="submitOrder">
				<div class="payment-options">
					<label
						v-for="option in paymentOptions"
						:key="option.value"
						class="payment-option"
					>
						<input
							type="radio"
							v-model="form.payment"
							:value="option.value"
							required
						/>
						<span class="payment-label">{{ option.label }}</span>
					</label>
				</div>
				<button type="submit" class="btn">Xác nhận đơn hàng</button>
			</form>
		</div>

		<!-- Bước 3: Hoàn tất -->
		<div v-else-if="step === 3" class="step-content success-step">
			<div class="success-animation">
				<svg
					class="checkmark"
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 52 52"
				>
					<circle
						class="checkmark__circle"
						cx="26"
						cy="26"
						r="25"
						fill="none"
					/>
					<path
						class="checkmark__check"
						fill="none"
						d="M14.1 27.2l7.1 7.2 16.7-16.8"
					/>
				</svg>
				<p class="success-message">Đặt hàng thành công</p>
				<p class="success-message">
					Cảm ơn bạn đã mua sắm tại cửa hàng chúng tôi
				</p>
			</div>

			<div class="invoice-card">
				<div class="invoice-header">
					<h3>Thông tin đơn hàng</h3>
				</div>

        <div class="products-summary">
          <h4>Sản phẩm đã đặt:</h4>
          <div v-for="(product, index) in products" :key="index" class="product-item">
            <span class="product-name">{{ product.productName }} (x{{ product.quantity }})</span>
            <span>{{ format.formatPrice(product.price * product.quantity) }}₫</span>
          </div>
        </div>
      </div>

      <div class="success-actions">
        <button @click="backToHome" class="btn btn-home">
          <i class="fas fa-home"></i> Về trang chủ
        </button>
        <button @click="goToOrders" class="btn btn-order">
          <i class="fas fa-receipt"></i> Xem chi tiết đơn hàng
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AddressForm from '../components/account/AddressForm.vue';
import productService from '@/services/productService';
import meService from '@/services/meService';
import format from '@/utils/format';
import emitter from '../utils/evenBus.js';

const router = useRouter();
const route = useRoute();
const products = ref([]);
const isAddressValid = ref(false);
const previousAddress = ref('');
const totalAmount = computed(() => {
	return products.value.reduce((sum, product) => {
		return sum + product.price * product.quantity;
	}, 0);
});

const userInfo = ref({
	name: "",
	phone: "",
	address: "",
});

const step = ref(1);
const stepLabels = ["Thông tin", "Thanh toán", "Hoàn tất"];

const form = reactive({
  name: '',
  phone: '',
  address: '',
  payment: '',
});

const paymentOptions = [
	{ label: "Thanh toán khi nhận hàng (COD)", value: "cod" },
	{ label: "Chuyển khoản Ngân hàng", value: "bank" },
	{ label: "MoMo / VNPay / ZaloPay", value: "momo" },
];

const paymentText = computed(() => {
	return (
		paymentOptions.find((opt) => opt.value === form.payment)?.label || ""
	);
});

// Theo dõi thay đổi địa chỉ từ userInfo
watch(() => userInfo.value.address, (newVal) => {
  if (newVal && newVal !== form.address) {
    form.address = newVal;
    previousAddress.value = newVal;
  }
});

const handleAddressStatus = async (isComplete) => {
  isAddressValid.value = isComplete;
};
const updateAddress = async (newAddress) => {
  try {
    emitter.emit('show-loading', true);

    // Gọi API cập nhật thông tin user (chỉ cập nhật địa chỉ)
    const updatedUser = await meService.updateProfile({
      ...userInfo.value,
      address: newAddress,
    });

    // Nếu backend trả về user mới, đồng bộ lại dữ liệu
    if (updatedUser && updatedUser.address) {
      userInfo.value.address = updatedUser.address;
      form.address = updatedUser.address;
      previousAddress.value = updatedUser.address;
    } else {
      // Nếu không trả về, vẫn đồng bộ theo newAddress
      userInfo.value.address = newAddress;
      form.address = newAddress;
      previousAddress.value = newAddress;
    }

  } catch (error) {
    // Nếu lỗi, rollback lại địa chỉ cũ
    form.address = previousAddress.value;
  } finally {
    emitter.emit('show-loading', false);
  }
};
const goToOrders = () => {
  router.push({ path: "/account", query: { section: "orders" } });
};

const nextStep = () => {
  if (form.name && form.phone && isAddressValid.value) {
    step.value = 2;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  } else {
    emitter.emit('show-notification', {
      status: 'error',
      message: 'Vui lòng điền đầy đủ thông tin địa chỉ'
    });
  }
};

const submitOrder = () => {
  if (form.payment) {
    const orderData = {
      paymentMethod: form.payment,
      items: products.value.map(product => ({
        productId: product.productId,
        quantity: product.quantity,
        colorId: product.colorId,
      })),
    };

    productService.createOrder(orderData)
      .then(response => {
        console.log('Đặt hàng thành công:', response);
        step.value = 3;
        window.scrollTo({ top: 0, behavior: 'smooth' });
        emitter.emit('cart-updated');
      })
      .catch(error => {
        emitter.emit('show-notification', {
          status: 'error',
          message: 'Đặt hàng thất bại. Vui lòng thử lại sau.',
        });
        console.error('Lỗi khi đặt hàng:', error);
      });
  } else {
    emitter.emit('show-notification', {
      status: 'error',
      message: 'Vui lòng chọn phương thức thanh toán',
    });
  }
};

const handleStepClick = (targetStep) => {
  if(step.value === 3) return;
  if (targetStep < step.value) {
    step.value = targetStep;
  }
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const backToHome = () => {
  router.push('/');
};

onMounted(async() => {
  try {
    // Lấy thông tin user
    const user = await meService.getMe();
    if (user) {
      userInfo.value = {
        name: user.name || '',
        phone: user.phoneNumber || '',
        address: user.address || ''
      };
      
      // Khởi tạo form và previousAddress
      form.name = userInfo.value.name;
      form.phone = userInfo.value.phone;
      form.address = userInfo.value.address;
      previousAddress.value = userInfo.value.address;
    }
  } catch (error) {
    console.error('Lỗi khi lấy thông tin người dùng:', error);
  }

  // Xử lý sản phẩm từ URL
  if (route.query.direct === 'true' && route.query.products) {
    try {
      const decodedProducts = decodeURIComponent(route.query.products);
      products.value = JSON.parse(decodedProducts).map(p => ({
        ...p,
        isActive: true,
        price: p.price || 0 
      }));
    } catch (error) {
      console.error('Lỗi khi xử lý sản phẩm:', error);
    }
  }

  if (!products.value.length) {
    emitter.emit('show-notification', {
      status: 'warning',
      message: 'Bạn chưa chọn sản phẩm nào.',
    });
    router.push('/');
  }
});
</script>


<style scoped>
.order {
	text-align: center;
	margin-bottom: 20px;
}
.order-container {
	max-width: 1200px;
	margin: 0 auto;
	padding: 2rem;
	font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
	color: #2d3748;
}

/* Steps styling */
.steps {
	display: flex;
	justify-content: space-between;
	margin-bottom: 3rem;
	position: relative;
}

.steps::before {
	content: "";
	position: absolute;
	top: 15px;
	left: 0;
	right: 0;
	height: 2px;
	background-color: #e2e8f0;
	z-index: 1;
}

.step-item {
	position: relative;
	padding: 0.5rem 1rem;
	background-color: #f8fafc;
	border-radius: 8px;
	text-align: center;
	z-index: 2;
	cursor: pointer;
	transition: all 0.3s ease;
	border: 1px solid #e2e8f0;
	font-weight: 500;
}

.step-item:hover {
	background-color: #edf2f7;
}

.step-item.active {
	background-color: var(--primary-color);
	color: white;
	border-color: #e67ad6;
}

.step-item.current {
	box-shadow: 0 0 0 3px rgba(161, 138, 245, 0.3);
	font-weight: 600;
}

/* Step content styling */
.step-content {
	background-color: #fff;
	border-radius: 12px;
	padding: 2rem;
	box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
	margin-bottom: 2rem;
}

.step-content h2 {
	font-size: 1.3rem;
	margin-bottom: 1.8rem;
	color: #2c3e50;
	text-align: left;
	font-weight: 600;
	position: relative;
	padding-bottom: 0.8rem;
	display: flex;
	align-items: center;
}

.step-content h2::before {
	content: "";
	display: inline-block;
	width: 4px;
	height: 20px;
	background: linear-gradient(to bottom, #f86ed3, #f5b0f8);
	border-radius: 2px;
	margin-right: 12px;
}

/* Product styling */
.product {
	display: flex;
	gap: 1.5rem;
	/* padding: 1.5rem; */
	border: 1px solid #eaeaea;
	border-radius: 8px;
	margin-bottom: 1.5rem;
	transition: all 0.3s ease;
}

/* Thêm style cho phần tổng tiền */
.total-line {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-top: 1.5rem;
	margin-bottom: 2rem;
	padding: 1rem 1.5rem;
	background-color: #f8f8f8;
	border-radius: 8px;
	border-left: 4px solid #fa46c1;
	font-size: 1.2rem;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.total-line strong {
	font-weight: 600;
	color: #333;
}

.total-line span {
	font-weight: 700;
	color: #e91e63;
	font-size: 1.3rem;
}

.product:hover {
	border-color: #cbd5e0;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.product.disabled-item {
	opacity: 0.6;
	background-color: #f8fafc;
}

.product img {
	width: 120px;
	height: 140px;
	object-fit: contain;
	border-radius: 6px;
	border: 1px solid #e2e8f0;
}

.product-info {
	flex: 1;
}

.product-info h3 {
	margin: 0 0 0.5rem 0;
	font-size: 1.1rem;
}

.product-info button {
	background-color: #f1f5f9;
	border: none;
	padding: 0.25rem 0.75rem;
	border-radius: 4px;
	font-size: 0.85rem;
	margin-bottom: 0.5rem;
}

.product-info p {
	font-size: 1.1rem;
	margin: 0.5rem 0;
	font-weight: 600;
}

/* Form input styling */
input[type="text"],
input[type="tel"] {
	width: 100%;
	padding: 0.85rem 1.2rem;
	margin-bottom: 1.5rem;
	border: 1px solid #e2e8f0;
	border-radius: 8px;
	font-size: 1rem;
	transition: all 0.3s ease;
	background-color: #f8fafc;
	color: #2d3748;
}

input[type="text"]:hover,
input[type="tel"]:hover {
	border-color: #cbd5e0;
	background-color: #fff;
}

input[type="text"]:focus,
input[type="tel"]:focus {
	border-color: #a18af5;
	box-shadow: 0 0 0 3px rgba(161, 138, 245, 0.2);
	outline: none;
	background-color: #fff;
}

/* Payment options */
.payment-options {
	margin-bottom: 2rem;
}

.payment-option {
	display: block;
	padding: 1.2rem;
	margin-bottom: 1rem;
	border: 1px solid #e2e8f0;
	border-radius: 8px;
	cursor: pointer;
	transition: all 0.3s ease;
	background-color: #f8fafc;
}

.payment-option:hover {
	background-color: #fff;
	border-color: #cbd5e0;
}

.payment-label {
	margin-left: 0.75rem;
	font-weight: 500;
}

/* Invoice styling */
/* Success Step Styles */
.success-step {
	text-align: center;
	padding: 2rem;
}

.success-animation {
	margin-bottom: 2.5rem;
	animation: fadeIn 0.6s ease;
}

.checkmark {
	width: 100px;
	height: 100px;
	margin: 0 auto;
}

.checkmark__circle {
	stroke: #4caf50;
	stroke-width: 2;
	stroke-miterlimit: 10;
	animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
	fill: none;
}

.checkmark__check {
	transform-origin: 50% 50%;
	stroke: #4caf50;
	stroke-width: 2;
	stroke-linecap: round;
	stroke-miterlimit: 10;
	stroke-dasharray: 48;
	stroke-dashoffset: 48;
	animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.6s forwards;
}

@keyframes stroke {
	100% {
		stroke-dashoffset: 0;
	}
}

.success-message {
	font-size: 1.2rem;
	color: #4caf50;
	margin-top: 1rem;
	font-weight: 500;
}

.invoice-card {
	background: white;
	border-radius: 12px;
	box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
	padding: 1.5rem;
	margin: 1.5rem 0;
	text-align: left;
	animation: slideUp 0.5s ease;
}

.invoice-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 1.5rem;
	padding-bottom: 1rem;
	border-bottom: 1px solid #f0f0f0;
}

.invoice-header h3 {
	margin: 0;
	color: #333;
	font-size: 1.25rem;
}

.order-number {
	background: #f8f9fa;
	padding: 0.4rem 0.8rem;
	border-radius: 20px;
	font-size: 0.85rem;
	font-weight: 600;
	color: #6c757d;
}

.detail-row {
	display: flex;
	justify-content: space-between;
	margin-bottom: 0.8rem;
	padding-bottom: 0.8rem;
	border-bottom: 1px dashed #f0f0f0;
}

.detail-row:last-child {
	border-bottom: none;
}

.detail-label {
	color: #6c757d;
	font-weight: 500;
}

.detail-value {
	color: #333;
	font-weight: 600;
}

.detail-row.highlight .detail-value {
	color: #e91e63;
	font-size: 1.3rem;
	font-weight: 700;
}

.detail-row.highlight {
	background-color: #f8f8f8;
	margin: 1rem -1.5rem;
	padding: 1rem 1.5rem;
	border-left: 4px solid #fa46c1;
	border-radius: 4px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.products-summary {
	margin-top: 1.5rem;
	padding-top: 1.5rem;
	border-top: 1px solid #f0f0f0;
}

.products-summary h4 {
	margin-top: 0;
	margin-bottom: 1rem;
	color: #333;
	font-size: 1rem;
}

.product-item {
	display: flex;
	justify-content: space-between;
	margin-bottom: 0.5rem;
	font-size: 0.9rem;
}

.product-name {
	color: #555;
}

.success-actions {
	display: flex;
	gap: 1rem;
	margin-top: 2rem;
}

.btn-home {
	background: linear-gradient(to right, #4caf50, #8bc34a);
	flex: 1;
}

.btn-order {
	background: linear-gradient(to right, #2196f3, #03a9f4);
	flex: 1;
}

/* Animations */
@keyframes fadeIn {
	from {
		opacity: 0;
	}
	to {
		opacity: 1;
	}
}

@keyframes slideUp {
	from {
		opacity: 0;
		transform: translateY(20px);
	}
	to {
		opacity: 1;
		transform: translateY(0);
	}
}

/* Font Awesome icons */
.fas {
	margin-right: 8px;
}
/* Button styling */
.btn {
	background: linear-gradient(to right, #ef9dd9, #fa46c1);
	color: white;
	border: none;
	padding: 0.9rem 2rem;
	border-radius: 8px;
	font-size: 1rem;
	font-weight: 600;
	cursor: pointer;
	transition: all 0.3s ease;
	width: 100%;
}

.btn:hover {
	opacity: 0.9;
	box-shadow: 0 4px 12px rgba(161, 138, 245, 0.3);
}

/* Responsive design */
@media (max-width: 768px) {
	.order-container {
		padding: 1rem;
	}

	.steps {
		flex-direction: column;
		gap: 1rem;
	}

	.steps::before {
		display: none;
	}

	.step-item {
		width: 100%;
		text-align: left;
	}

	.step-content {
		padding: 1.5rem;
	}

	/* .product {
    flex-direction: column;
    align-items: center;
    text-align: center;
  } */

	/* .product img {
    width: 80px;
    height: 80px;
  } */
}
</style>
