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

function handleImageError(event) {
  event.target.style.display = 'none'
}

// Import des images
const images = import.meta.glob('@/assets/images/*.{png,jpg,jpeg,svg}', {
  eager: true,
  query: '?url',
  import: 'default',
})

const getImageUrl = (imageName) => {
  const entry = Object.entries(images).find(([path]) => path.endsWith(imageName))
  return entry ? entry[1] : ''
}
</script>

<template>
  <div class="promotion-card-content" :class="{ 'foil-card': card.isFoil }">
    <div class="image-wrapper">
      <img
        :src="card.image_src"
        :alt="card.alt_text"
        class="promotion-card-image"
        @error="handleImageError"
        @click="goToRoute"
      />
      <div v-if="card.isFoil" class="foil-overlay"></div>
    </div>
    <div class="promotion-card-info">
      <p class="nom_carte">{{ card.nom_carte }}</p>
      <p class="nom_set">{{ card.nom_set }}</p>
      <div class="price-container">
        <span class="label_prix" :class="{ 'text-green': isStartOfPromotion }"
          >{{ card.prix }}$CAD</span
        >
        <span class="label_ancien_prix">{{ card.ancien_prix }}$CAD</span>
      </div>
      <p class="countDown_label_text">
        {{ isStartOfPromotion ? 'Début de la promotion dans :' : 'Fin de la promotion dans :' }}
        <span class="countDown_text" :class="{ 'text-green': isStartOfPromotion }">
          {{ timeCountDown.days }}j {{ timeCountDown.hours }}h {{ timeCountDown.minutes }}m
          {{ timeCountDown.secondes }}s
        </span>
      </p>
      <div class="buttons-container">
        <button type="button" class="btn btn-secondary see_card_btn" @click="goToRoute">
          <span class="material-symbols-outlined">playing_cards</span>Voir la carte
        </button>
        <button
          type="button"
          :disabled="(!isCardAvailable() || !hasStock()) && !isCardInCart()"
          :class="[
            'btn',
            'see_card_btn',
            isCardInCart()
              ? 'btn-danger'
              : !isCardAvailable() || !hasStock()
                ? 'btn-secondary'
                : 'btn-primary',
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
          <span class="material-symbols-outlined" v-if="isCardInCart()">remove_shopping_cart</span>
          <span class="material-symbols-outlined" v-else-if="!hasStock()"
            >production_quantity_limits</span
          >
          <span class="material-symbols-outlined" v-else-if="!isCardAvailable()">block</span>
          <span class="material-symbols-outlined" v-else>add_shopping_cart</span>
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
  </div>
</template>

<style scoped>
.promotion-card-content {
  display: flex;
  flex-direction: row;
  justify-content: space-evenly;
  align-items: center;
  width: 100%;
}

.promotion-card-image {
  width: 100%;
  height: 100%;
  object-fit: fill;
  display: block;
}

.promotion-card-info {
  text-align: center;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 0;
  max-height: 100%;
  overflow: hidden;
}

.nom_carte {
  font-size: 120%;
  font-weight: bold;
  color: black;
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  max-height: 2.4em;
  line-height: 1.2em;
}

.nom_set {
  font-size: 90%;
  opacity: 0.8;
  color: black;
  margin: 0.2rem 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.label_prix {
  font-size: 130%;
  font-weight: bold;
  color: red;
}

.label_ancien_prix {
  font-size: 75%;
  text-decoration: line-through;
  color: black;
  font-weight: bold;
  margin-left: 10px;
}

.countDown_text {
  color: red;
  font-weight: bold;
}

.text-green {
  color: green !important;
}

.countDown_label_text {
  color: black;
  font-size: 85%;
  line-height: 1.2;
  margin: 0.3rem 0;
}

/* Effet foil */
.foil-card {
  position: relative;
  overflow: hidden;
}

.image-wrapper {
  position: relative;
  width: 130px;
  height: auto;
  display: inline-block;
  cursor: pointer;
}

.foil-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  z-index: 2;
  mix-blend-mode: color-dodge;
  opacity: 0.6;
}

.foil-overlay::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, transparent 30%, rgba(255, 215, 0, 0.6) 50%, transparent 70%);
  background-size: 200% 200%;
  animation: foil-shine 3s ease-in-out infinite;
}

.foil-overlay::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.4) 0%,
    transparent 50%,
    rgba(255, 215, 0, 0.3) 100%
  );
  animation: foil-glow 2s ease-in-out infinite alternate;
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
    opacity: 0.7;
  }
}

.price-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  gap: 15px;
  width: 100%;
  margin: 0.3rem 0;
  flex-shrink: 0;
}

.buttons-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  width: 100%;
  margin-top: 5px;
}

.see_card_btn {
  width: 60%;
  height: 28px;
  font-size: 95%;
  overflow: hidden;
  transition:
    width 0.3s ease-in-out,
    height 0.3s ease-in-out;
  background-color: #2d3c49;
  color: white;
  border: none;
  border-radius: 500px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin: 0 auto;
}

.see_card_btn > span {
  margin-right: 5px;
  font-size: medium;
}

.see_card_btn:hover {
  width: 65%;
  height: 35px;
  background-color: #0079ea;
  transform: scale(1.04);
}
</style>

<style>
body.body--dark .nom_carte {
  color: white !important;
}

body.body--dark .nom_set {
  color: #ccc !important;
}

body.body--dark .label_ancien_prix {
  color: white !important;
}

body.body--dark .countDown_text {
  color: red !important;
}

body.body--dark .text-green {
  color: #4caf50 !important;
}

body.body--dark .label_prix,
body.body--dark .promotion-card-content .label_prix {
  color: red !important;
}

body.body--dark .countDown_label_text {
  color: white !important;
}
</style>
