<script setup>
import { useRouter } from 'vue-router'
const router = useRouter()
</script>

<template>
  <div class="success-page">
    <div class="success-icon">
      <svg viewBox="0 0 24 24" class="checkmark" aria-hidden="true">
        <circle class="circle" cx="12" cy="12" r="10" />
        <path class="check" d="M7 12l3 3 7-7" />
      </svg>
    </div>

    <h1 class="success-title">Demande de remboursement envoyée</h1>
    <p class="success-message">Merci de votre confiance !</p>

    <div class="actions">
      <button class="btn primary" @click="router.push('/Catalogue')">Continuer de magasiner</button>
      <button class="btn secondary" @click="router.push('/GestionCompte/HistoriqueCommande')">
        Voir mes commandes
      </button>
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
  background: #232a38;
  text-align: center;
  font-family:
    system-ui,
    -apple-system,
    'Segoe UI',
    Roboto,
    sans-serif;
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

.success-title {
  font-size: 28px;
  color: #4ade80;
  margin: 0;
}

.success-message {
  font-size: 16px;
  color: #e5e7eb;
  margin-top: 10px;
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
  color: #f3f4f6;
}

.btn.primary {
  background: #666a70;
}

.btn.primary:hover {
  background: #6b7280;
}

.btn.secondary {
  background: #374151;
}

.btn.secondary:hover {
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
