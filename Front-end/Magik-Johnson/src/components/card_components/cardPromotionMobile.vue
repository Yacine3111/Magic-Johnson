<script setup>
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { cardService } from '@/services/CardService'

const router = useRouter()
const cartStore = useCartStore()

const props = defineProps({
  card: {
    type: Object,
    required: true,
  },
  timeCountDown: {
    type: Object,
    required: true,
  },
  isStartOfPromotion: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['add-to-cart'])

function goToRoute() {
  cardService.updateCardClickCount(props.card.id)
  router.push({
    name: 'DetailsCarte',
    params: {
      langage: 'en',
      id: props.card.id,
      name: props.card.nom_carte.toLowerCase().replace(/\s+/g, '-').replace(/'/g, ''),
    },
  })
}

function addToCart() {
  emit('add-to-cart', props.card)
}

function removeFromCart() {
  let lang = props.card.language
  if (!lang) {
    const item = cartStore.items.find((i) => i.id === props.card.id)
    if (item) {
      lang = item.language
    }
  }
  cartStore.removeItem(props.card.id, lang)
}

function isCardInCart() {
  return cartStore.isInCart(props.card.id)
}

function isCardAvailable() {
  const price = parseFloat(props.card.price || 0)
  const foilPrice = parseFloat(props.card.foilPrice || 0)

  return price > 0 || foilPrice > 0
}

function hasStock() {
  return (props.card.stock || 0) > 0
}
</script>

<template>
  <div class="mobile-card-content" :class="{ 'foil-card': card.isFoil }">
    <img
      :src="card.image_src"
      :alt="card.alt_text"
      class="mobile-card-image"
      :class="{ 'foil-effect': card.isFoil }"
      draggable="false"
      @error="handleImageError"
    />
    <div class="mobile-card-info">
      <p class="mobile-card-name">{{ card.nom_carte }}</p>
      <p class="mobile-card-set">{{ card.nom_set }}</p>

      <div class="mobile-prices">
        <span class="mobile-price" :class="{ 'text-green': isStartOfPromotion }"
          >{{ card.prix }}$CAD</span
        >
        <span class="mobile-old-price">{{ card.ancien_prix }}$CAD</span>
      </div>
    </div>

    <div class="mobile-countdown">
      <span class="countdown-text">
        {{ isStartOfPromotion ? 'Début de la promotion dans :' : 'Fin de la promotion dans :' }}
      </span>
      <span class="countdown-time" :class="{ 'text-green': isStartOfPromotion }">
        {{ timeCountDown.days }}j {{ timeCountDown.hours }}h {{ timeCountDown.minutes }}m
        {{ timeCountDown.secondes }}s
      </span>
    </div>

    <div class="mobile-card-buttons">
      <button type="button" class="mobile-btn mobile-btn-view" @click="goToRoute">
        <span class="material-symbols-outlined">playing_cards</span>
        Voir la carte
      </button>
      <button
        type="button"
        :disabled="(!isCardAvailable() || !hasStock()) && !isCardInCart()"
        :class="[
          'mobile-btn',
          'mobile-btn-cart',
          isCardInCart() ? 'mobile-btn-danger' : '',
          (!isCardAvailable() || !hasStock()) && !isCardInCart() ? 'mobile-btn-disabled' : '',
        ]"
        :style="
          isCardInCart()
            ? { backgroundColor: '#dc3545', borderColor: '#dc3545', color: 'white' }
            : !isCardAvailable() || !hasStock()
              ? {
                  backgroundColor: '#6c757d',
                  borderColor: '#6c757d',
                  color: 'white',
                  cursor: 'not-allowed',
                }
              : {}
        "
        @click="isCardInCart() ? removeFromCart() : addToCart()"
      >
        <span class="material-symbols-outlined">{{
          isCardInCart()
            ? 'remove_shopping_cart'
            : !hasStock()
              ? 'production_quantity_limits'
              : isCardAvailable()
                ? 'add_shopping_cart'
                : 'cancel'
        }}</span>
        {{
          isCardInCart()
            ? 'Retirer du panier'
            : !hasStock()
              ? 'Stock épuisé'
              : isCardAvailable()
                ? 'Ajouter au panier'
                : 'Non disponible'
        }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.mobile-card-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  gap: 0.8rem;
  width: 100%;
  max-width: 100%;
  height: auto;
  min-height: auto;
  padding: 1rem 0.5rem;
  margin: 0 auto;
  margin-bottom: 0.5rem;
  box-sizing: border-box;
  overflow: visible;
  transition: all 0.3s ease-in-out;
  animation: slideInUp 0.3s ease-out;
}

@keyframes slideInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Effet foil */
.foil-card {
  position: relative;
  overflow: hidden;
}

.foil-effect {
  position: relative;
  filter: brightness(1.3) contrast(1.2) saturate(1.4);
}

.foil-effect::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, transparent 30%, rgba(255, 215, 0, 0.4) 50%, transparent 70%);
  background-size: 200% 200%;
  animation: foil-shine 2s ease-in-out infinite;
  pointer-events: none;
  z-index: 1;
}

.foil-effect::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.3) 0%,
    transparent 50%,
    rgba(255, 215, 0, 0.2) 100%
  );
  animation: foil-glow 1.5s ease-in-out infinite alternate;
  pointer-events: none;
  z-index: 2;
}

@keyframes foil-shine {
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

@keyframes foil-glow {
  0% {
    opacity: 0.3;
  }
  100% {
    opacity: 0.8;
  }
}

/* Style pour le compteur */
.mobile-countdown {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  margin-bottom: 0.5rem;
  flex-shrink: 0;
  text-align: center;
  animation: pulse 2s infinite;
  padding: 0 0.5rem;
  box-sizing: border-box;
}

@keyframes pulse {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
  100% {
    transform: scale(1);
  }
}

.countdown-text {
  font-size: 0.75rem;
  color: black;
  font-weight: 500;
  margin-bottom: 0.3rem;
  white-space: normal;
  text-align: center;
  line-height: 1.4;
  word-wrap: break-word;
  overflow-wrap: break-word;
  padding: 0 0.5rem;
  width: 100%;
  box-sizing: border-box;
}

.countdown-time {
  font-size: 0.75rem;
  color: red;
  font-weight: bold;
  white-space: normal;
  text-align: center;
  line-height: 1.4;
  word-wrap: break-word;
  overflow-wrap: break-word;
  padding: 0 0.5rem;
  width: 100%;
  box-sizing: border-box;
}

.text-green {
  color: green !important;
}

.mobile-card-image {
  width: auto;
  max-width: 100%;

  min-height: 150px;
  max-height: none;
  object-fit: contain;
  border-radius: 6px;
  flex-shrink: 0;
  transition: all 0.3s ease-in-out;
  animation: fadeInScale 0.8s ease-out;
  margin-bottom: 0.5rem;
}

@keyframes fadeInScale {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.mobile-card-info {
  text-align: center;
  width: 100%;

  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0 0.3rem;
  box-sizing: border-box;
}

.mobile-card-name {
  font-size: 0.95rem;
  font-weight: bold;
  margin: 0 0 0.3rem 0;
  color: #333;
  white-space: normal;
  text-align: center;
  word-wrap: break-word;
  overflow-wrap: break-word;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  line-height: 1.4em;
  display: block;
  overflow: visible;
}

.mobile-card-set {
  font-size: 0.8rem;
  color: #666;
  margin: 0 0 0.5rem 0;
  word-wrap: break-word;
  overflow-wrap: break-word;
  width: 100%;
  box-sizing: border-box;
}

.mobile-prices {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.mobile-price {
  font-size: 1.1rem;
  font-weight: bold;
  color: red;
  animation: priceGlow 3s infinite;
}

@keyframes priceGlow {
  0%,
  100% {
    text-shadow: 0 0 5px rgba(255, 0, 0, 0.3);
  }
  50% {
    text-shadow: 0 0 15px rgba(255, 0, 0, 0.6);
  }
}

.mobile-old-price {
  font-size: 0.9rem;
  color: black;
  text-decoration: line-through;
}

/* Conteneur des boutons */
.mobile-card-buttons {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: stretch;
  gap: 0.6rem;
  width: 100%;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
  flex-wrap: wrap;
  padding: 0.5rem;
  box-sizing: border-box;
}

/* Boutons */
.mobile-btn {
  padding: 0.6rem 0.8rem;
  border: none;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease-in-out;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.2rem;
  min-width: 120px;
  max-width: 100%;
  white-space: normal;
  word-wrap: break-word;
  overflow-wrap: break-word;
  position: relative;
  overflow: visible;
  flex: 1 1 auto;
  box-sizing: border-box;
}

.mobile-btn:active {
  transform: scale(0.95);
}

.mobile-btn .material-symbols-outlined {
  font-size: 0.85rem;
}

.mobile-btn-view {
  background-color: #586573;
  color: #ffffff;
}

.mobile-btn-cart {
  background-color: #586573;
  color: #ffffff;
}

.mobile-card-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  gap: 0.8rem;
  width: 110%;
  max-width: 100%;
  height: 400px;
  min-height: 100%;
  padding: 0.5rem 1rem 1rem 1rem;
  margin: 0 auto;
  margin-bottom: 0.5rem;
  box-sizing: border-box;
  overflow: visible;
  transition: all 0.3s ease-in-out;
  animation: slideInUp 0.6s ease-out;
}

/* Pour les écrans moyens et petits */
@media (max-width: 767px) {
  .mobile-card-content {
    max-width: 100%;
    padding: 1.5rem 0.8rem 1rem 0.8rem;
    gap: 0.7rem;
  }

  .carousel-spacer {
    height: 3vh;
    min-height: 18px;
  }

  .mobile-btn {
    min-width: 110px;
    padding: 0.5rem 0.6rem;
    font-size: 0.7rem;
    flex: 1 1 45%;
  }

  .mobile-btn .material-symbols-outlined {
    font-size: 0.75rem;
  }

  .countdown-text,
  .countdown-time {
    font-size: 0.7rem;
    padding: 0 0.3rem;
  }

  .mobile-card-info {
    padding: 0 0.2rem;
  }

  .mobile-card-name {
    font-size: 0.9rem;
    line-height: 1.35em;
  }
}

/* Pour les très petits écrans */
@media (max-width: 480px) {
  .mobile-card-content {
    max-width: 100%;
    padding: 1.2rem 0.6rem 0.8rem 0.6rem;
    gap: 0.6rem;
  }

  .mobile-card-image {
    height: 220px;
    min-height: 220px;
    max-width: 100%;
  }

  .carousel-spacer {
    height: 2.5vh;
    min-height: 15px;
  }

  .mobile-btn {
    min-width: 100px;
    padding: 0.5rem 0.5rem;
    font-size: 0.65rem;
    flex: 1 1 45%;
  }

  .mobile-btn .material-symbols-outlined {
    font-size: 0.7rem;
  }

  .mobile-card-buttons {
    gap: 0.5rem;
    padding: 0 0.3rem;
  }

  .countdown-text,
  .countdown-time {
    font-size: 0.65rem;
    padding: 0 0.2rem;
  }

  .mobile-card-info {
    padding: 0 0.15rem;
  }

  .mobile-card-name {
    font-size: 0.9rem;
    line-height: 1.3em;
  }

  .mobile-card-set {
    font-size: 0.75rem;
  }
}

@media (max-width: 360px) {
  .mobile-card-content {
    max-width: 240px;
    padding: 0.7rem;
    gap: 0.5rem;
  }

  .mobile-card-image {
    height: 200px;
    min-height: 200px;
  }

  .mobile-card-name {
    font-size: 0.9rem;
  }

  .mobile-card-set {
    font-size: 0.75rem;
  }

  .mobile-prices {
    gap: 0.4rem;
  }

  .mobile-price {
    font-size: 1rem;
  }

  .mobile-old-price {
    font-size: 0.85rem;
  }

  .carousel-spacer {
    height: 2vh;
    min-height: 12px;
  }

  .mobile-btn {
    min-width: 110px;
    padding: 0.5rem 0.6rem;
    font-size: 0.65rem;
  }

  .mobile-btn .material-symbols-outlined {
    font-size: 0.7rem;
  }
}

/* Pour les écrans extrêmement petits */
@media (max-width: 320px) {
  .mobile-card-content {
    max-width: 220px;
    padding: 0.6rem;
    gap: 0.4rem;
  }

  .mobile-card-image {
    height: 180px;
    min-height: 180px;
  }

  .mobile-card-name {
    font-size: 0.85rem;
    margin-bottom: 0.15rem;
  }

  .mobile-card-set {
    font-size: 0.7rem;
    margin-bottom: 0.4rem;
  }

  .mobile-prices {
    gap: 0.3rem;
    margin-bottom: 0.4rem;
  }

  .mobile-price {
    font-size: 0.95rem;
  }

  .mobile-old-price {
    font-size: 0.8rem;
  }

  .carousel-spacer {
    height: 1.5vh;
    min-height: 10px;
  }

  .mobile-card-buttons {
    gap: 0.5rem;
    margin-top: 0.3rem;
  }

  .mobile-btn {
    min-width: 100px;
    padding: 0.45rem 0.5rem;
    font-size: 0.6rem;
  }

  .mobile-btn .material-symbols-outlined {
    font-size: 0.65rem;
  }
}
</style>

<style>
body.body--dark .mobile-card-name {
  color: white !important;
}

body.body--dark .mobile-card-set {
  color: #ccc !important;
}

body.body--dark .mobile-old-price {
  color: white !important;
}

body.body--dark .countdown-text {
  color: red !important;
}

body.body--dark .countdown-time {
  color: red !important;
}

body.body--dark .text-green {
  color: #4caf50 !important;
}

body.body--dark .mobile-price {
  color: red !important;
}
</style>
