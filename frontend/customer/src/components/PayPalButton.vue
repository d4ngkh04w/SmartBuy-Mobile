<!-- src/components/PayPalButton.vue -->
<template>
	<div ref="paypalRef"></div>
</template>

<script setup>
import { onMounted, ref } from "vue";

const props = defineProps({
	amount: Number,
});

const emit = defineEmits(["payment-success", "payment-error"]);
const paypalRef = ref(null);

onMounted(async () => {
	if (!window.paypal) {
		await loadPayPalScript();
	}

	window.paypal
		.Buttons({
			createOrder: (data, actions) => {
				return actions.order.create({
					purchase_units: [
						{
							amount: {
								value: props.amount.toFixed(2),
								currency_code: "USD",
							},
						},
					],
				});
			},
			onApprove: (data, actions) => {
				return actions.order.capture().then((details) => {
					emit("payment-success", details);
				});
			},
			onError: (err) => {
				console.error("PayPal payment error:", err);
				emit("payment-error", err);
			},
		})
		.render(paypalRef.value);
});

const loadPayPalScript = () => {
	return new Promise((resolve, reject) => {
		const script = document.createElement("script");
		script.src = `https://www.paypal.com/sdk/js?client-id=${
			import.meta.env.VITE_PAYPAL_CLIENT_ID
		}&currency=USD`;
		script.onload = resolve;
		script.onerror = reject;
		document.head.appendChild(script);
	});
};
</script>
