<template>
    <transition name="modal">
        <div class="modal-backdrop" @click.self="$emit('cancel')">
            <div class="modal">
                <div class="modal-header">
                    <h2>{{ title }}</h2>
                    <button @click="$emit('cancel')" class="close-button" aria-label="Close modal">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <line x1="18" y1="6" x2="6" y2="18"></line>
                            <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                    </button>
                </div>
                <div class="modal-body">
                    <p class="confirmation-message">{{ message }}</p>
                    <div class="form-actions">
                        <button type="button" class="cancel-button" @click="$emit('cancel')">Hủy</button>
                        <button type="button" class="confirm-button" @click="$emit('confirm')">Xác nhận</button>
                    </div>
                </div>
            </div>
        </div>
    </transition>
</template>

<script setup>
defineProps({
    title: {
        type: String,
        default: "Xác nhận",
    },
    message: {
        type: String,
        required: true,
    },
});

defineEmits(["confirm", "cancel"]);
</script>

<style scoped>
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
    backdrop-filter: blur(2px);
}

.modal {
    background-color: white;
    border-radius: 12px;
    width: min(400px, 95vw);
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.modal-enter-active,
.modal-leave-active {
    transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
    opacity: 0;
}

.modal-enter-active .modal,
.modal-leave-active .modal {
    transition: all 0.3s ease;
}

.modal-enter-from .modal,
.modal-leave-to .modal {
    transform: translateY(-20px);
    opacity: 0;
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid var(--border-color, #eee);
    position: relative;
}

.modal-header h2 {
    margin: 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--text-primary, #333);
}

.close-button {
    background-color: transparent;
    border: none;
    color: var(--text-secondary, #666);
    cursor: pointer;
    transition: all 0.2s;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
}

.close-button:hover {
    background-color: pink;
    color: var(--text-primary, #333);
}

.close-button svg {
    width: 16px;
    height: 16px;
}

.modal-body {
    padding: 1.5rem;
}

.confirmation-message {
    margin: 0 0 1.5rem 0;
    line-height: 1.6;
    color: var(--text-secondary, #555);
    font-size: 0.95rem;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 0.75rem;
}

.cancel-button,
.confirm-button {
    padding: 0.625rem 1.25rem;
    border-radius: 8px;
    font-size: 0.9375rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 1px solid transparent;
}

.cancel-button {
    background-color: var(--bg-secondary, #f5f5f5);
    color: var(--text-secondary, #666);
    border-color: var(--border-color, #ddd);
}

.cancel-button:hover {
    background-color: pink;
}

.confirm-button {
    background-color: var(--primary-color);
    color: white;
}

.confirm-button:hover {
    background-color: rgb(243, 66, 96);
    box-shadow: 0 2px 8px rgba(var(--primary-rgb), 0.2);
}

/* Responsive adjustments */
@media (max-width: 480px) {
    .modal-header {
        padding: 1rem 1.25rem;
    }
    
    .modal-body {
        padding: 1.25rem;
    }
    
    .form-actions {
        flex-direction: column-reverse;
        gap: 0.75rem;
    }
    
    .cancel-button,
    .confirm-button {
        width: 100%;
    }
}
</style>