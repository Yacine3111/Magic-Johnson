<template>
  <div class="article-panier" :class="{ 'stock-issue-highlight': item.hasStockIssue }">
    <!-- Message d'erreur stock -->
    <div v-if="item.hasStockIssue" class="stock-issue-message">
      <q-icon name="warning" color="red" size="20px" />
      <span>{{ item.stockIssueMessage }}</span>
    </div>

    <!-- Section principale avec image, détails et contrôles -->
    <div class="article-main">
      <!-- Image de la carte -->
      <div class="article-image">
        <RouterLink :to="`/cards/${item.id}`">
          <q-img
            :src="item.image"
            alt="Image du produit"
            fit="cover"
            class="card-image cursor-pointer"
          >
            <div v-if="item.isFoil" class="foil-overlay"></div>
            <template v-slot:error>
              <div class="image-error">
                <q-icon name="image" size="40px" color="grey-5" />
              </div>
            </template>
          </q-img>
        </RouterLink>
      </div>

      <!-- Informations de la carte -->
      <div class="article-info">
        <h3 class="article-name">{{ item.name }}</h3>

        <div class="article-details">
          <div class="detail-line">
            <span class="detail-label">Nom série :</span>
            <span class="detail-value">{{ getSetName }}</span>
          </div>
          <div class="detail-line">
            <span class="detail-label">Numero série :</span>
            <span class="detail-value">{{ getSerialNumber }}</span>
          </div>
          <div class="detail-line">
            <span class="detail-label">Rareté :</span>
            <span class="detail-value">{{ getRarity }}</span>
          </div>
          <div class="detail-line">
            <span class="detail-label">Langue :</span>
            <span class="detail-value">{{ getLanguage }}</span>
          </div>
          <div v-if="item.isFoil" class="detail-line foil-line">
            <span class="foil-text">FOIL</span>
          </div>
          <div v-if="hasDiscount" class="detail-line discount-line">
            <span class="detail-label">Rabais :</span>
            <span class="detail-value discount-value">{{ getDiscountText }}</span>
          </div>
        </div>
      </div>

      <!-- Ligne de prix  -->
      <div class="price-row">
        <span :class="hasDiscount ? 'article-price-discounted' : 'article-price-normal'">
          {{ (currentPrice * quantity).toFixed(2) }}$
        </span>
        <span v-if="hasDiscount && originalPrice > currentPrice" class="article-price-old">
          {{ (originalPrice * quantity).toFixed(2) }}$
        </span>
      </div>

      <!-- Icône supprimer-->
      <div class="delete-section">
        <q-btn flat round icon="delete" size="md" @click="removeItem" class="delete-btn" />
      </div>

      <!-- Boutons quantité -->
      <div class="quantity-controls" role="group" aria-label="Changer la quantité">
        <q-btn
          flat
          round
          dense
          icon="remove"
          @click="decreaseQuantity"
          :disabled="quantity <= 1"
          class="qty-btn minus-btn"
          aria-label="Diminuer"
        />
        <span class="quantity-display" aria-live="polite">{{ quantity }}</span>
        <q-btn
          flat
          round
          dense
          icon="add"
          @click="increaseQuantity"
          :disabled="quantity >= item.limit"
          class="qty-btn plus-btn"
          aria-label="Augmenter"
        />
      </div>

      <!-- Message limite -->
      <div class="limit-slot">
        <div class="limit-message" :class="{ 'is-hidden': quantity < item.limit }">
          Vous avez atteint la limite pour ce produit
        </div>
      </div>

      <!-- fin section principale -->
    </div>

    <!-- Informations de stock et limite -->
    <div class="limite-client">
      <div :class="{ 'stock-epuise': (item.stock || 0) === 0 }">
        <span v-if="(item.stock || 0) === 0">Stock épuisé</span>
        <template v-else>
          <div>Limite client: {{ item.clientLimit || 4 }}</div>
          <div>Stock disponible: {{ item.stock || 0 }}</div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Notify } from 'quasar'

import { TranslationService } from '@/services/translationService'

const props = defineProps({
  item: {
    type: Object,
    required: true,
  },
})

const emit = defineEmits(['update-quantity', 'remove-item'])

// Utilise directement la quantité de l'item
const quantity = computed(() => props.item.quantity)

const increaseQuantity = () => {
  if (quantity.value < props.item.limit) {
    emit(
      'update-quantity',
      props.item.id,
      props.item.language,
      props.item.isFoil,
      quantity.value + 1,
    )
  } else {
    const currentStock = props.item.isFoil ? props.item.stockFoil : props.item.stockNonFoil
    const reason =
      currentStock < props.item.clientLimit ? 'Stock insuffisant' : 'Limite client atteinte'
    const maxValue = currentStock < props.item.clientLimit ? currentStock : props.item.clientLimit

    Notify.create({
      type: 'warning',
      message: `${reason}. Maximum ${maxValue} exemplaire(s) autorisé(s)`,
      position: 'top',
      badgeStyle: 'display: none',
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
  }
}

const decreaseQuantity = () => {
  if (quantity.value > 1) {
    emit(
      'update-quantity',
      props.item.id,
      props.item.language,
      props.item.isFoil,
      quantity.value - 1,
    )
  } else {
    removeItem()
  }
}

const removeItem = () => {
  emit('remove-item', props.item.id, props.item.language, props.item.isFoil)
}

// Computed properties pour les détails
const getSetName = computed(() => {
  return props.item.setName || 'Série inconnue'
})

const getSerialNumber = computed(() => {
  const number = props.item.number || props.item.serialNumber
  return number && number !== 'N/A' ? number : ''
})

const getRarity = computed(() => {
  const rarity = props.item.rarity
  if (!rarity) return 'Commune'

  return TranslationService.translateRarity(rarity)
})

const getLanguage = computed(() => {
  const language = props.item.language
  if (!language) return 'Anglais'

  return TranslationService.translateLanguage(language)
})

const hasDiscount = computed(() => {
  return props.item.discount && props.item.discount > 0
})

const getDiscountText = computed(() => {
  if (!hasDiscount.value) return ''
  return `-${props.item.discount}%`
})

// Prix original (avant discount)
const originalPrice = computed(() => {
  return props.item.price || 0
})

// Prix actuel, calculé à partir du prix original
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
.stock-issue-highlight {
  border: 3px solid #f44336 !important;

  animation: stockIssuePulse 2s ease-in-out infinite;
}

.stock-issue-message {
  background-color: #f44336;
  color: white;
  padding: 10px 15px;
  border-radius: 8px 8px 0 0;
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 500;
  font-size: 14px;
}

@keyframes stockIssuePulse {
  0% {
    box-shadow: 0 0 0 0 rgba(244, 67, 54, 0.7);
  }
  70% {
    box-shadow: 0 0 0 10px rgba(244, 67, 54, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(244, 67, 54, 0);
  }
}

.foil-indicator {
  color: #ff6b6b;
  font-weight: bold;
  text-shadow: 0 0 5px rgba(255, 107, 107, 0.5);
}

.article-image {
  position: relative;
}

.stock-epuise {
  color: #ff4444;
  font-weight: bold;
  background-color: rgba(255, 68, 68, 0.1);
  padding: 5px 10px;
  border-radius: 5px;
  border: 1px solid #ff4444;
  animation: pulse-red 2s infinite;
}

@keyframes pulse-red {
  0% {
    box-shadow: 0 0 0 0 rgba(255, 68, 68, 0.4);
  }
  70% {
    box-shadow: 0 0 0 10px rgba(255, 68, 68, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(255, 68, 68, 0);
  }
}

.foil-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 4px;
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
.article-panier {
  background: #a89c8d;
  border-radius: 20px;
  margin-bottom: 1rem;
  overflow: hidden;
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.25);
  transition: all 0.3s ease;
}

.article-panier:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
}

/* Section principale */
.article-main {
  display: grid;
  grid-template-columns: 150px 5fr 1fr;
  column-gap: 1rem;
  row-gap: 0.75rem;
  align-items: start;
  padding: 1.75rem;
  position: relative;
  max-width: 100%;
}

/* Image */
.article-image {
  width: 150px;
  height: 200px;
  border-radius: 10px;
  overflow: hidden;
  background: #34495e;
  grid-column: 1;
  grid-row: 1 / span 3;
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease;
}

.article-image:hover {
  transform: scale(1.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.card-image {
  width: 100%;
  height: 100%;
  border-radius: 8px;
  border: 2px solid transparent;
  transition: border-color 0.3s ease;
}

.article-panier:hover .card-image {
  border-color: #a2d2ff;
}

.image-error {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #34495e;
}

/* Informations */
.article-info {
  color: white;
  grid-column: 2;
  grid-row: 1;
  padding-right: 1rem;
  min-width: 0;
  width: 100%;
}

.article-name {
  font-size: 1.5rem;
  font-weight: bold;
  color: white;
  margin: 0 0 1rem 0;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  word-wrap: break-word;
  max-width: 100%;
}

.foil-line {
  margin-top: 0.5rem;
  margin-bottom: 0.3rem;
}

.foil-text {
  color: white;
  font-weight: 500;
  font-size: 0.9rem;
}

.article-details {
  margin-bottom: 1rem;
  width: 200%;
}

.detail-line {
  display: flex;
  margin-bottom: 0.3rem;
  font-size: 0.9rem;
  align-items: flex-start;
}

.detail-label {
  flex: 0 0 40px;
  color: #000000;
  font-weight: 600;
  white-space: nowrap;
  text-align: left;
  padding-right: 8px;
}

.detail-value {
  flex: 1;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  text-overflow: ellipsis;
  word-wrap: break-word;
  color: white;
  font-weight: 600;
  min-width: 0;
}

.discount-line {
  color: #e74c3c;
}

.discount-value {
  color: #e74c3c;
  font-weight: bold;
}

.price-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
  grid-column: 2;
  grid-row: 2;
  width: 100%;
}

.article-price {
  font-size: 1.8rem;
  font-weight: bold;
  color: white;
}

.article-price-discounted {
  font-size: 1.8rem;
  font-weight: bold;
  color: red;
}

.article-price-normal {
  font-size: 1.8rem;
  font-weight: bold;
  color: white;
}

.article-price-old {
  font-size: 1.2rem;
  font-weight: bold;
  color: #c0392b;
  text-decoration: line-through;
}

.price-unit {
  font-size: 0.9rem;
  color: #bdc3c7;
  font-weight: 400;
}

/* Bouton supprimer */
.delete-section {
  display: flex;
  align-items: center;
  justify-content: center;
  grid-column: 3;
  grid-row: 1;
  justify-self: end;
  align-self: start;
  width: 42px;
  height: 42px;
}

.delete-btn {
  opacity: 0.9;
  transition: opacity 0.2s ease;
  width: 42px;
  height: 42px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}
.delete-btn .q-btn__content {
  padding: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}
.delete-btn :deep(.q-icon) {
  font-size: 24px;
  line-height: 1;
  color: #b91c1c !important;
}

.delete-btn:hover {
  opacity: 1;
}

/* Contrôles de quantité */
.quantity-controls {
  display: grid;
  grid-template-columns: 44px 1fr 44px;
  align-items: center;
  justify-items: center;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 999px;
  padding: 4px 8px;
  min-width: 170px;
  max-width: 190px;
  height: 42px;
  grid-column: 3;
  grid-row: 2;
  justify-self: end;
}

.qty-btn {
  width: 36px;
  height: 36px;
  box-shadow: none;
  padding: 0;
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.qty-btn .q-btn__content {
  padding: 0;
}

.qty-btn .q-icon {
  color: #111827;
  font-size: 20px;
}

.plus-btn[disabled] .q-icon,
.plus-btn .q-icon:disabled {
  color: #9ca3af !important;
}

.minus-btn[disabled] .q-icon,
.minus-btn .q-icon:disabled {
  color: #9ca3af !important;
}

.quantity-display {
  font-size: 1.1rem;
  font-weight: 800;
  color: #111827;
  min-width: 38px;
  text-align: center;
  line-height: 36px;
}

/* Espace réservé pour le message de limite */
.limit-slot {
  min-height: 1.2rem;
  display: flex;
  align-items: center;
  grid-column: 2 / span 2;
  grid-row: 3;
}
.limit-message {
  color: #f44336;
  font-size: 0.85rem;
  font-style: italic;
  transition: opacity 0.2s ease;
}
.limit-message.is-hidden {
  visibility: hidden;
  opacity: 0;
}

/* Limite client */
.limite-client {
  background: #cfd4d7;
  color: #1f2937;
  text-align: center;
  padding: 0.6rem;
  font-weight: 700;
  font-size: 0.9rem;
}

@media (max-width: 1024px) {
  .article-panier {
    background: #a89c8d;
    border-radius: 12px;
    margin-bottom: 0.75rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    padding: 1rem;
  }

  .article-panier:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.3);
  }

  .stock-issue-message {
    border-radius: 12px 12px 0 0;
    margin: -1rem -1rem 1rem -1rem;
  }

  .article-main {
    display: grid;
    grid-template-columns: 100px 1fr;
    grid-template-rows: auto auto auto;
    gap: 0.75rem;
    padding: 0;
    align-items: start;
  }

  .article-image {
    width: 100px;
    height: 140px;
    grid-column: 1;
    grid-row: 1 / span 3;
    border-radius: 8px;
    background: #34495e;
  }

  .article-info {
    grid-column: 2;
    grid-row: 1;
    padding-right: 0;
    color: white;
  }

  .article-name {
    font-size: 1rem;
    font-weight: 600;
    color: white;
    margin: 0 0 0.5rem 0;
    line-height: 1.4;
    -webkit-line-clamp: 2;
  }

  .foil-line {
    margin-top: 0.3rem;
    margin-bottom: 0.25rem;
  }

  .foil-text {
    font-size: 0.85rem;
  }

  .article-details {
    width: 100%;
    margin-bottom: 0.5rem;
  }

  .detail-line {
    font-size: 0.85rem;
    margin-bottom: 0.25rem;
  }

  .detail-label {
    flex: 0 0 80px;
    color: #000000;
    font-weight: 600;
  }

  .detail-value {
    color: white;
    font-weight: 400;
  }

  .price-row {
    grid-column: 2;
    grid-row: 2;
    gap: 0.5rem;
    margin-top: 0.5rem;
  }

  .article-price-discounted {
    font-size: 1.25rem;
    font-weight: 700;
    color: #e74c3c;
  }

  .article-price-normal {
    font-size: 1.25rem;
    font-weight: 700;
    color: white;
  }

  .article-price-old {
    font-size: 0.9rem;
    color: #bdc3c7;
  }

  .price-unit {
    font-size: 0.75rem;
    color: #bdc3c7;
    font-weight: 400;
  }

  .delete-section {
    grid-column: 2;
    grid-row: 3;
    justify-self: start;
    align-self: center;
    width: auto;
    height: auto;
    margin-top: 0.5rem;
  }

  .delete-btn {
    opacity: 0.9;
    width: 38px;
    height: 38px;
  }

  .delete-btn :deep(.q-icon) {
    font-size: 22px;
    color: #b91c1c !important;
  }

  .quantity-controls {
    grid-column: 2;
    grid-row: 3;
    justify-self: end;
    align-self: center;
    min-width: 120px;
    max-width: 140px;
    height: 36px;
    background: #ffffff;
    border: 1px solid rgba(255, 255, 255, 0.3);
    margin-top: 0.5rem;
  }

  .qty-btn {
    width: 32px;
    height: 32px;
  }

  .qty-btn .q-icon {
    color: #111827;
    font-size: 18px;
  }

  .quantity-display {
    font-size: 1rem;
    font-weight: 600;
    color: #111827;
  }

  .limit-slot {
    grid-column: 2;
    grid-row: 4;
    margin-top: 0.25rem;
  }

  .limit-message {
    font-size: 0.8rem;
    color: #f44336;
  }

  .limite-client {
    background: #34495e;
    color: #bdc3c7;
    padding: 0.5rem 1rem;
    font-size: 0.85rem;
    font-weight: 400;
    text-align: left;
    margin: 0 -1rem -1rem -1rem;
    border-radius: 0 0 12px 12px;
  }

  .stock-epuise {
    color: #e74c3c;
    font-weight: 600;
    background-color: transparent;
    padding: 0;
    border: none;
    animation: none;
  }
}

/*  iPad*/
@media (min-width: 641px) and (max-width: 1024px) {
  .article-panier {
    padding: 1.5rem;
  }

  .article-main {
    grid-template-columns: 150px 1fr;
    gap: 1rem;
  }

  .article-image {
    width: 150px;
    height: 210px;
  }

  .article-name {
    font-size: 1.1rem;
  }

  .article-price-discounted,
  .article-price-normal {
    font-size: 1.4rem;
  }
}

@media (max-width: 640px) {
  .article-panier {
    background: #243142;
    border-radius: 12px;
    margin-bottom: 0.75rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    padding: 1rem;
  }

  .article-panier:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.3);
  }

  .stock-issue-message {
    border-radius: 12px 12px 0 0;
    margin: -1rem -1rem 1rem -1rem;
  }

  .article-main {
    display: grid;
    grid-template-columns: 100px 1fr;
    grid-template-rows: auto auto auto;
    gap: 0.75rem;
    padding: 0;
    align-items: start;
  }

  .article-image {
    width: 100px;
    height: 140px;
    grid-column: 1;
    grid-row: 1 / span 3;
    border-radius: 8px;
    background: #34495e;
  }

  .article-info {
    grid-column: 2;
    grid-row: 1;
    padding-right: 0;
    color: white;
  }

  .article-name {
    font-size: 1rem;
    font-weight: 600;
    color: white;
    margin: 0 0 0.5rem 0;
    line-height: 1.4;
    -webkit-line-clamp: 2;
  }

  .foil-line {
    margin-top: 0.3rem;
    margin-bottom: 0.25rem;
  }

  .foil-text {
    font-size: 0.85rem;
  }

  .article-details {
    width: 100%;
    margin-bottom: 0.5rem;
  }

  .detail-line {
    font-size: 0.85rem;
    margin-bottom: 0.25rem;
  }

  .detail-label {
    flex: 0 0 80px;
    color: #bdc3c7;
    font-weight: 400;
  }

  .detail-value {
    color: white;
    font-weight: 400;
  }

  .price-row {
    grid-column: 2;
    grid-row: 2;
    gap: 0.5rem;
    margin-top: 0.5rem;
  }

  .article-price-discounted {
    font-size: 1.25rem;
    font-weight: 700;
    color: #e74c3c;
  }

  .article-price-normal {
    font-size: 1.25rem;
    font-weight: 700;
    color: white;
  }

  .article-price-old {
    font-size: 0.9rem;
    color: #bdc3c7;
  }

  .price-unit {
    font-size: 0.75rem;
    color: #bdc3c7;
    font-weight: 400;
  }

  .delete-section {
    grid-column: 2;
    grid-row: 3;
    justify-self: start;
    align-self: center;
    width: auto;
    height: auto;
    margin-top: 0.5rem;
  }

  .delete-btn {
    opacity: 0.9;
    width: 32px;
    height: 32px;
  }

  .delete-btn .q-icon {
    font-size: 18px;
    color: #f44336;
  }

  .quantity-controls {
    grid-column: 2;
    grid-row: 3;
    justify-self: end;
    align-self: center;
    min-width: 140px;
    max-width: 160px;
    height: 42px;
    background: #ffffff;
    border: 1px solid rgba(255, 255, 255, 0.3);
    margin-top: 0.5rem;
  }

  .qty-btn {
    width: 38px;
    height: 38px;
  }

  .qty-btn .q-icon {
    color: #111827;
    font-size: 18px;
  }

  .quantity-display {
    font-size: 1rem;
    font-weight: 600;
    color: #111827;
  }

  .limit-slot {
    grid-column: 2;
    grid-row: 4;
    margin-top: 0.25rem;
  }

  .limit-message {
    font-size: 0.8rem;
    color: #f44336;
  }

  .limite-client {
    background: #34495e;
    color: #bdc3c7;
    padding: 0.5rem 1rem;
    font-size: 0.85rem;
    font-weight: 400;
    text-align: left;
    margin: 0 -1rem -1rem -1rem;
    border-radius: 0 0 12px 12px;
  }

  .stock-epuise {
    color: #e74c3c;
    font-weight: 600;
    background-color: transparent;
    padding: 0;
    border: none;
    animation: none;
  }
}
</style>

<style>
body.body--dark .article-panier {
  background-color: #1f2735 !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .quantity-controls {
  background-color: #2d3c49 !important;
  border-color: #4b5563 !important;
}

body.body--dark .qty-btn .q-icon {
  color: white !important;
}

body.body--dark .quantity-display {
  color: white !important;
}

@media (min-width: 1025px) {
  body.body--dark .limite-client,
  body.body--dark .limite-client div {
    color: #000000 !important;
  }
}

body.body--dark .delete-btn,
body.body--dark .delete-btn .q-icon {
  color: #b91c1c !important;
}

body.body--dark .limit-message {
  color: #f44336 !important;
}

body.body--dark .limite-client .stock-epuise,
body.body--dark .limite-client .stock-epuise span {
  color: #f44336 !important;
}

body.body--dark .detail-label {
  color: #bdc3c7 !important;
}
</style>

<style>
/* Light Mode  */
body:not(.body--dark) .article-panier {
  background-color: #ddd7c7 !important;
  border: 1px solid #000;
}

body:not(.body--dark) .article-panier .detail-label,
body:not(.body--dark) .article-panier .detail-value,
body:not(.body--dark) .article-panier .article-name,
body:not(.body--dark) .article-panier .foil-text {
  color: black !important;
}

body:not(.body--dark) .article-panier .article-price,
body:not(.body--dark) .article-panier .article-price-normal,
body:not(.body--dark) .article-panier .limit-message,
body:not(.body--dark) .article-panier .stock-issue-message {
  color: #000000 !important;
}

body:not(.body--dark) .article-panier .article-price-discounted {
  color: #ff0000 !important;
}

body.body--dark .article-panier .article-price-discounted {
  color: #ff0000 !important;
}

body:not(.body--dark) .article-panier .article-price-old {
  color: #555555 !important;
}

@media (max-width: 640px) {
  body:not(.body--dark) .article-panier .detail-label {
    color: black !important;
  }
}
</style>
