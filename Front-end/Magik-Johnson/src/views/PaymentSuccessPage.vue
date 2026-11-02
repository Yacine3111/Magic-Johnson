<script setup>
import { useCartStore } from '@/stores/cartStore'
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { CreateOrder } from '@/api/payment/PaymentApi'
import { useCheckoutStore } from '@/stores/Checkout'
import { Loading, Notify } from 'quasar'

import { useAuthStore } from '@/stores/authStore'
import { ref } from 'vue'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()
const createdOrderId = ref(null)

onMounted(async () => {
  const checkout = useCheckoutStore()

  try {
    Loading.show()

    if (checkout.paymentMethod === 'card') {
      const response = await CreateOrder(checkout.email)
      createdOrderId.value = response.data.id || response.data.Id
    }
    checkout.reset()
    checkout.reset()
    await cartStore.clearCart()
  } catch {
    Notify.create({
      type: 'negative',
      message: 'Erreur lors de la confirmation de la commande. Veuillez contacter le support.',
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
  } finally {
    Loading.hide()
  }
})
</script>

<template>
  <div class="success-page">
    <div class="success-icon">
      <svg viewBox="0 0 24 24" class="checkmark" aria-hidden="true">
        <circle class="circle" cx="12" cy="12" r="10" />
        <path class="check" d="M7 12l3 3 7-7" />
      </svg>
    </div>

    <h1 class="success-title">Paiement réussi</h1>
    <p class="success-message">Merci pour votre achat !</p>

    <div class="actions">
      <template v-if="authStore.role?.includes('employee') || authStore.role?.includes('manager')">
        <button class="btn primary" @click="router.push('/Catalogue')">
          Continuer de magasiner
        </button>
        <button class="btn primary" @click="router.push('/GestionMagasin/gestionCommandes')">
          Voir les commandes
        </button>
        <button
          v-if="createdOrderId"
          class="btn secondary"
          @click="router.push(`/details-commande/${createdOrderId}`)"
        >
          Voir la commande
        </button>
      </template>
      <template v-else>
        <button class="btn primary" @click="router.push('/Catalogue')">
          Continuer de magasiner
        </button>
        <button class="btn secondary" @click="router.push('/GestionCompte/HistoriqueCommande')">
          Voir les commandes
        </button>
      </template>
    </div>
  </div>
</template>

<style scoped>
.success-page {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100vh;
  text-align: center;
  font-family:
    system-ui,
    -apple-system,
    'Segoe UI',
    Roboto,
    sans-serif;
  transition:
    background-color 0.3s ease,
    color 0.3s ease;
}

/* Light Mode  */
.success-page {
  background: #f5f0e8;
  color: #2c3e50;
}

.success-title {
  font-size: 28px;
  color: #2e7d32;
  margin: 0;
}

.success-message {
  font-size: 16px;
  color: #555;
  margin-top: 10px;
}

/* Dark Mode */
body.body--dark .success-page {
  background: #232a38;
  color: #e5e7eb;
}

body.body--dark .success-title {
  color: #4ade80;
}

body.body--dark .success-message {
  color: #e5e7eb;
}

.success-icon {
  width: 100px;
  height: 100px;
  background: rgba(22, 163, 74, 0.1);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20px;
}

.checkmark {
  width: 60px;
  height: 60px;
}

.circle {
  fill: none;
  stroke: #4ade80;
  stroke-width: 2.5;
  stroke-dasharray: 63;
  stroke-dashoffset: 63;
  animation: drawCircle 0.6s ease-out forwards;
}

.check {
  fill: none;
  stroke: #22c55e;
  stroke-width: 3;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-dasharray: 20;
  stroke-dashoffset: 20;
  animation: drawCheck 0.4s 0.4s ease-out forwards;
}

.actions {
  display: flex;
  gap: 12px;
  margin-top: 30px;
}

.btn {
  padding: 10px 18px;
  border-radius: 8px;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition:
    background 0.2s ease,
    transform 0.15s ease;
  color: white;
}

/* Light Mode */
.btn.primary {
  background: #8d6e63;
}

.btn.primary:hover {
  background: #6d4c41;
}

.btn.secondary {
  background: #7f8c8d;
}

.btn.secondary:hover {
  background: #607d8b;
}

/* Dark Mode */
body.body--dark .btn.primary {
  background: #666a70;
}

body.body--dark .btn.primary:hover {
  background: #6b7280;
}

body.body--dark .btn.secondary {
  background: #374151;
}

body.body--dark .btn.secondary:hover {
  background: #4b5563;
}

.btn:active {
  transform: translateY(1px);
}

@keyframes drawCircle {
  to {
    stroke-dashoffset: 0;
  }
}

@keyframes drawCheck {
  to {
    stroke-dashoffset: 0;
  }
}
</style>
