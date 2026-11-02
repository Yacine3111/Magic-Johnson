<template>
  <article class="cart-item">
    <div class="image-container">
      <img :src="item.image" alt="" />
      <div v-if="item.isFoil" class="foil-overlay"></div>
    </div>
    <div class="info">
      <strong>{{ item.name }}</strong>
      <small v-if="item.number" class="muted">No de série : {{ item.number }}</small>
      <small v-if="item.language" class="muted"
        >Langue : {{ translateLanguage(item.language) }}</small
      >
      <small v-if="item.isFoil" class="foil-text">FOIL</small>
    </div>

    <div class="qty-price-container">
      <div class="qty-block">
        <span class="grid-label">Quantité :</span>
        <span class="grid-value">{{ item.quantity }}</span>
      </div>

      <div class="price-block">
        <span class="grid-label">Prix :</span>
        <span v-if="hasDiscount" class="grid-value price-original">
          {{ (originalPrice * item.quantity).toFixed(2) }} $
        </span>
        <span class="grid-value" :class="{ 'price-new': hasDiscount }">
          {{ (currentPrice * item.quantity).toFixed(2) }} $
        </span>
      </div>
    </div>

    <div class="info-icon-container">
      <i class="bi bi-info-circle">
        <q-tooltip class="text-body2">
          Prix unitaire : <span>{{ currentPrice.toFixed(2) }} $</span>
        </q-tooltip>
      </i>
    </div>
  </article>
</template>

<script setup>
import { TranslationService } from '@/services/translationService'
import { computed } from 'vue'

const props = defineProps({
  item: {
    type: Object,
    required: true,
  },
})

const translateLanguage = (language) => {
  return TranslationService.translateLanguage(language)
}

const hasDiscount = computed(() => {
  return props.item.discount && props.item.discount > 0
})

const originalPrice = computed(() => {
  return props.item.price || 0
})

const currentPrice = computed(() => {
  const original = originalPrice.value
  const discount = props.item.discount || 0
  if (discount > 0) {
    return original * (1 - discount / 100)
  }
  return original
})
</script>

<style scoped>
.cart-item {
  display: grid;
  grid-template-columns: 60px 1fr auto 30px;
  gap: 10px;
  align-items: center;
  background: #2a344f;
  border-radius: 10px;
  padding: 8px;
  border: 1px solid #3e4c79;
}

.image-container {
  position: relative;
  width: 60px;
  height: 80px;
  border-radius: 8px;
  overflow: hidden;
  flex-shrink: 0;
}

.image-container img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 8px;
  display: block;
}

.foil-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 8px;
  background: linear-gradient(
    45deg,
    rgba(255, 0, 255, 0.3),
    rgba(0, 255, 255, 0.3),
    rgba(255, 255, 0, 0.3),
    rgba(255, 0, 255, 0.3)
  );
  background-size: 400% 400%;
  animation: foilShine 3s ease infinite;
  pointer-events: none;
  z-index: 2;
}

.foil-overlay::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 20%;
  height: 100%;
  background: linear-gradient(to right, transparent, rgba(255, 255, 255, 0.4) 50%, transparent);
  animation: lightSweep 4s ease-in-out infinite;
  pointer-events: none;
}

@keyframes foilShine {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

@keyframes lightSweep {
  0% {
    left: 0%;
    opacity: 1;
  }
  25% {
    left: 80%;
    opacity: 1;
  }
  100% {
    left: 80%;
    opacity: 0;
  }
}

.cart-item .info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.foil-text {
  font-weight: bold;
  background: linear-gradient(90deg, #ffdb58, #ffffff, #ffdb58);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  width: fit-content;
}

.muted {
  color: rgb(255, 255, 255);
}
body:not(.body--dark) .muted {
  color: #000000;
}
.qty-price-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: flex-end;
  justify-content: center;
}

.qty-block,
.price-block {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 8px;
}

.info-icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.grid-label {
  text-align: right;
  font-size: medium;
}

.grid-value {
  text-align: right;
  font-size: medium;
}

.price-new {
  color: red;
  font-weight: bold;
}

.price-original {
  text-decoration: line-through;
  color: black;
  margin-right: 5px;
}
</style>
