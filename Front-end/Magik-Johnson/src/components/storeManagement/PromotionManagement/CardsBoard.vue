<template>
  <div class="cards-section">
    <div class="text-subtitle1 text-bold q-mb-md promotion-cards-title">
      Cartes associées à la promotion "{{ selectedPromotionForCards?.nom || 'Sélectionnée' }}" :
    </div>

    <div class="row q-mb-md items-stretch no-wrap q-gutter-sm">
      <q-input
        v-model="searchTerm"
        placeholder="Rechercher une carte"
        outlined
        dense
        class="col search-input"
        @input="handleSearch"
      />
      <div class="col-auto buttons-container">
        <q-btn
          v-if="editingPromotion"
          color="primary"
          label="Ajouter carte"
          class="add-card-btn action-btn"
          unelevated
          @click="$emit('open-catalog')"
        />
        <q-btn
          v-if="editingPromotion && localSelectedCards.length > 0"
          color="red-8"
          :label="`Supprimer ${localSelectedCards.length} carte(s)`"
          class="delete-btn action-btn"
          unelevated
          @click="handleRemoveSelected"
        />
      </div>
    </div>

    <div v-if="loading" class="text-center q-pa-lg">
      <q-spinner-dots size="40px" color="primary" />
      <div class="q-mt-sm">Chargement des cartes...</div>
    </div>

    <div
      v-else-if="paginatedCards.length === 0"
      class="text-center q-pa-lg bg-grey-2 rounded-borders"
    >
      <div v-if="editingPromotion" class="q-mt-md">
        <q-banner class="bg-red-1 text-red-9 rounded-borders custom-warning-banner">
          <template #avatar>
            <q-icon name="warning" color="negative" />
          </template>
          <div class="text-weight-medium">
            Au moins une carte doit être ajoutée à la promotion pour pouvoir la sauvegarder.
          </div>
        </q-banner>
      </div>
    </div>

    <div v-else>
      <div class="cards-wrapper">
        <!-- Desktop - Tableau -->
        <div v-if="$q.screen.gt.md" class="table-container">
          <div class="cards-table">
            <table class="q-table table table-borderless table-hover w-100">
              <thead>
                <tr>
                  <th>Image</th>
                  <th>Nom</th>
                  <th>Série / #</th>
                  <th>Couleur • Rareté</th>
                  <th>Prix</th>
                  <th>Prix Foil</th>
                  <th style="width: 96px" class="text-center">Stock</th>
                  <th style="width: 80px" class="text-center">État</th>
                  <th style="width: 160px" class="text-end">
                    <span
                      v-if="editingPromotion"
                      class="material-symbols-outlined select-all-icon"
                      :class="{ 'all-selected': areAllSelected }"
                      @click="toggleSelectAll"
                      :title="areAllSelected ? 'Désélectionner tout' : 'Sélectionner tout'"
                    >
                      {{ areAllSelected ? 'done_all' : 'checklist' }}
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="card in paginatedCards"
                  :key="card.id"
                  :class="[
                    {
                      'highlight-added': isNewlyAddedCard(card),
                    },
                    card.stock === 0 ? 'stock-zero' : card.stock <= 5 ? 'stock-low' : '',
                  ]"
                >
                  <td class="wrap-text">
                    <div class="image-wrapper" :class="{ 'foil-card': isFoilOnly(card) }">
                      <img
                        :src="card.image"
                        :alt="card.nom"
                        class="catalog-card-image"
                        @error="handleImageError"
                      />
                      <div v-if="isFoilOnly(card)" class="foil-overlay"></div>
                    </div>
                  </td>
                  <td class="wrap-text">{{ card.nom }}</td>
                  <td class="wrap-text">{{ card.serie }} • #{{ card.numero }}</td>
                  <td class="wrap-text">{{ card.couleur }} • {{ card.rarete }}</td>
                  <td>{{ formatPrice(card.prix) }}</td>
                  <td class="foil-price">
                    <span v-if="card.hasFoil">{{ formatPrice(card.prixFoil) }}</span>
                    <span v-else class="no-foil">Non disponible</span>
                  </td>
                  <td class="text-center">{{ card.stock }}</td>
                  <td class="text-center">
                    <span class="material-symbols-outlined" :class="getStockClass(card)">
                      capture
                      <q-tooltip>{{ getStockTooltip(card) }}</q-tooltip>
                    </span>
                  </td>
                  <td>
                    <div class="d-flex gap-2 align-items-center justify-content-end">
                      <div class="checkbox-container">
                        <input
                          type="checkbox"
                          :checked="localSelectedCards.includes(card.id)"
                          :disabled="!editingPromotion"
                          @change="toggleCardSelection(card.id, $event)"
                          class="custom-checkbox"
                          aria-label="Sélectionner la carte"
                        />
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Mobile - Cartes -->
        <div v-else class="mobile-cards-container">
          <div
            v-for="card in paginatedCards"
            :key="card.id"
            class="mobile-card"
            :class="{
              'highlight-added': isNewlyAddedCard(card),
              'stock-zero': card.stock === 0,
              'stock-low': card.stock <= 5 && card.stock > 0,
            }"
          >
            <div class="mobile-card-header">
              <div class="mobile-card-image-container">
                <div class="image-wrapper-mobile" :class="{ 'foil-card': isFoilOnly(card) }">
                  <img
                    :src="card.image"
                    :alt="card.nom"
                    class="mobile-card-image"
                    @error="handleImageError"
                  />
                  <div v-if="isFoilOnly(card)" class="foil-overlay-mobile"></div>
                </div>
              </div>
              <div class="mobile-card-info">
                <div class="card-name">{{ card.nom }}</div>
                <div class="card-set">
                  {{ card.serie
                  }}<span
                    v-if="card.numero"
                    style="color: #ccc; font-size: 0.85em; margin-left: 4px"
                    >• #{{ card.numero }}</span
                  >
                </div>
                <div class="card-badges">
                  <span class="rarity-badge" :class="getRarityClass(card.rarete)">{{
                    card.rarete
                  }}</span>
                  <span class="stock-indicator" :class="getStockClass(card)"
                    >● {{ getStockLevel(card) }}</span
                  >
                </div>
                <div class="card-details">
                  <div class="card-detail-row">
                    <span class="detail-label">Couleur:</span>
                    <span class="detail-value">{{ card.couleur }}</span>
                  </div>
                  <div class="card-detail-row">
                    <span class="detail-label">Prix:</span>
                    <span class="detail-value">{{ formatPrice(card.prix) }}</span>
                  </div>
                  <div class="card-detail-row">
                    <span class="detail-label">Foil:</span>
                    <span v-if="card.hasFoil" class="detail-value foil-price-mobile">{{
                      formatPrice(card.prixFoil)
                    }}</span>
                    <span v-else class="detail-value no-foil">Non disponible</span>
                  </div>
                  <div class="card-detail-row">
                    <span class="detail-label">Stock:</span>
                    <span class="detail-value stock-value">{{ card.stock }}</span>
                  </div>
                </div>
              </div>
            </div>
            <div v-if="editingPromotion" class="mobile-card-selection">
              <div class="selection-checkbox-container">
                <input
                  type="checkbox"
                  :id="`checkbox-${card.id}`"
                  :checked="localSelectedCards.includes(card.id)"
                  :disabled="!editingPromotion"
                  @change="toggleCardSelection(card.id, $event)"
                  class="custom-checkbox-mobile"
                  aria-label="Sélectionner la carte"
                />
                <label :for="`checkbox-${card.id}`" class="checkbox-label-mobile">
                  {{ localSelectedCards.includes(card.id) ? 'Sélectionnée' : 'Sélectionner' }}
                </label>
              </div>
            </div>
          </div>
        </div>

        <!-- Desktop Pagination -->
        <div v-if="totalCardsPages > 1 && $q.screen.gt.md" class="pagination-container">
          <nav class="custom-pagination">
            <ul class="pagination-list">
              <li class="pagination-item">
                <button
                  class="pagination-btn prev-btn"
                  :disabled="!hasPreviousCardsPage"
                  @click="$emit('change-page', currentCardsPage - 1)"
                >
                  <q-icon name="chevron_left" size="sm" />
                </button>
              </li>
              <li v-if="currentCardsPage > 3" class="pagination-item">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentCardsPage === 1 }"
                  @click="$emit('change-page', 1)"
                >
                  1
                </button>
              </li>
              <li v-if="currentCardsPage > 3" class="pagination-item">
                <span class="pagination-ellipsis">...</span>
              </li>
              <li v-for="page in getVisibleCardsPages()" :key="page" class="pagination-item">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentCardsPage === page }"
                  @click="$emit('change-page', page)"
                >
                  {{ page }}
                </button>
              </li>
              <li v-if="currentCardsPage < totalCardsPages - 2" class="pagination-item">
                <span class="pagination-ellipsis">...</span>
              </li>
              <li v-if="currentCardsPage < totalCardsPages - 2" class="pagination-item">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentCardsPage === totalCardsPages }"
                  @click="$emit('change-page', totalCardsPages)"
                >
                  {{ totalCardsPages }}
                </button>
              </li>
              <li class="pagination-item">
                <button
                  class="pagination-btn next-btn"
                  :disabled="!hasNextCardsPage"
                  @click="$emit('change-page', currentCardsPage + 1)"
                >
                  <q-icon name="chevron_right" size="sm" />
                </button>
              </li>
            </ul>
          </nav>
        </div>

        <!-- Mobile Pagination -->
        <div v-if="totalCardsPages > 1 && $q.screen.lt.lg" class="mobile-pagination-container">
          <button
            class="mobile-pagination-btn"
            :disabled="!hasPreviousCardsPage"
            @click="$emit('change-page', currentCardsPage - 1)"
          >
            <q-icon name="chevron_left" size="sm" />
          </button>
          <span class="mobile-pagination-info">
            Page {{ currentCardsPage }} / {{ totalCardsPages }}
          </span>
          <button
            class="mobile-pagination-btn"
            :disabled="!hasNextCardsPage"
            @click="$emit('change-page', currentCardsPage + 1)"
          >
            <q-icon name="chevron_right" size="sm" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, ref, watch, computed, nextTick } from 'vue'
import { useQuasar } from 'quasar'
import { getStatusClass, getStatusText, getStockInfo } from '@/services/stockService'
import TranslationService from '@/services/translationService'

const $q = useQuasar()

const props = defineProps({
  selectedPromotionForCards: Object,
  associatedCards: Array,
  paginatedCards: Array,
  selectedCards: Array,
  editingPromotion: Object,
  currentCardsPage: Number,
  totalCardsPages: Number,
  hasNextCardsPage: Boolean,
  hasPreviousCardsPage: Boolean,
  loading: Boolean,
  validationErrors: Object,
  originalPromotionCards: Array,
})

const emit = defineEmits([
  'open-catalog',
  'remove-selected',
  'add-card',
  'toggle-selection',
  'change-page',
  'update-selected-cards',
])

// État de recherche
const searchTerm = ref('')

// Fonction de recherche
const handleSearch = () => {
  emit('search-cards', searchTerm.value)
}

watch(searchTerm, (newValue) => {
  emit('search-cards', newValue)
})

const localSelectedCards = ref([])

watch(
  () => props.selectedCards,
  (newVal) => {
    if (!props.editingPromotion) {
      const ids = Array.isArray(newVal)
        ? newVal.map((item) => (typeof item === 'object' && item?.id ? item.id : item))
        : []
      localSelectedCards.value = ids
    }
  },
  { deep: true },
)

watch(
  () => props.editingPromotion,
  (newVal) => {
    if (newVal) {
      if (localSelectedCards.value.length === 0) {
        localSelectedCards.value = []
      }
    }
  },
  { deep: true, immediate: false },
)

// Watcher pour paginatedCards
watch(
  () => props.paginatedCards,
  (newVal) => {},
  { deep: true },
)

const isNewlyAddedCard = (card) => {
  if (!props.editingPromotion) return false

  if (props.editingPromotion.id === null) {
    return props.editingPromotion.carteIds && props.editingPromotion.carteIds.includes(card.id)
  }

  const originalCardIds = props.originalPromotionCards || []
  const isInEditingPromotion =
    props.editingPromotion.carteIds && props.editingPromotion.carteIds.includes(card.id)
  const isNotInOriginalCards = !originalCardIds.includes(card.id)

  return isInEditingPromotion && isNotInOriginalCards
}

const handleImageError = (event) => {
  event.target.style.display = 'none'
}

const getVisibleCardsPages = () => {
  const pages = []
  const start = Math.max(1, props.currentCardsPage - 2)
  const end = Math.min(props.totalCardsPages, props.currentCardsPage + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
}

const toggleCardSelection = (cardId, event) => {
  if (!props.editingPromotion) {
    return
  }

  event.stopPropagation()
  const index = localSelectedCards.value.indexOf(cardId)

  if (index === -1) {
    localSelectedCards.value.push(cardId)
  } else {
    localSelectedCards.value.splice(index, 1)
  }

  emit('toggle-selection', cardId, event)
  emit('update-selected-cards', [...localSelectedCards.value])
}

const handleRemoveSelected = () => {
  if (localSelectedCards.value.length === 0) {
    return
  }

  emit('remove-selected', [...localSelectedCards.value])

  localSelectedCards.value = []
  emit('update-selected-cards', [])
}

const formatPrice = (price) => {
  if (price === null || price === undefined || isNaN(price)) {
    return 'N/A'
  }
  return `${Number(price).toFixed(2)} $`
}

const isFoilOnly = (card) => {
  return (
    card.hasFoil &&
    (card.prix === null || card.prix === undefined || card.prix === 0 || Number(card.prix) === 0)
  )
}

const getRarityClass = (rarity) => {
  if (!rarity) return ''
  const rarityLower = rarity.toLowerCase()
  if (rarityLower.includes('commune')) return 'rarity-commune'
  if (rarityLower.includes('peu-commune') || rarityLower.includes('peu commune'))
    return 'rarity-peu-commune'
  if (rarityLower.includes('rare')) return 'rarity-rare'
  if (rarityLower.includes('mythique')) return 'rarity-mythique'
  return ''
}

const getStockInfoForCard = (card) => {
  const rarity = TranslationService.translateRarity(card.rarete || card.rarity || 'Rare')
  return getStockInfo(card.stock || 0, rarity, card.rarete || card.rarity)
}

const getStockClass = (card) => {
  const stockInfo = getStockInfoForCard(card)
  return stockInfo.class
}

const getStockLevel = (card) => {
  const stockInfo = getStockInfoForCard(card)
  return stockInfo.level
}

const getStockTooltip = (card) => {
  const stockInfo = getStockInfoForCard(card)
  return stockInfo.tooltip
}

const getAllPromotionCards = computed(() => {
  if (!props.editingPromotion) {
    return props.associatedCards || []
  }

  const allCardIds = props.editingPromotion.carteIds || []

  if (allCardIds.length === 0) {
    return []
  }

  const cardMap = new Map()

  if (props.associatedCards && props.associatedCards.length > 0) {
    props.associatedCards.forEach((card) => {
      cardMap.set(card.id, card)
    })
  }

  const result = []
  allCardIds.forEach((cardId) => {
    if (cardMap.has(cardId)) {
      result.push(cardMap.get(cardId))
    } else {
      const paginatedCard = (props.paginatedCards || []).find((card) => card.id === cardId)
      if (paginatedCard) {
        result.push(paginatedCard)
      } else {
        result.push({ id: cardId })
      }
    }
  })

  return result
})

const areAllSelected = computed(() => {
  if (!props.editingPromotion) {
    return false
  }

  const allCardIds = props.editingPromotion.carteIds || []

  if (allCardIds.length === 0) {
    return false
  }

  return allCardIds.every((cardId) => localSelectedCards.value.includes(cardId))
})

const toggleSelectAll = async (event) => {
  if (event) {
    event.preventDefault()
    event.stopPropagation()
  }

  if (!props.editingPromotion) {
    return
  }

  const allCardIds = props.editingPromotion.carteIds || []

  if (allCardIds.length === 0) {
    return
  }

  const allCurrentlySelected = allCardIds.every((cardId) =>
    localSelectedCards.value.includes(cardId),
  )

  if (allCurrentlySelected) {
    localSelectedCards.value = localSelectedCards.value.filter(
      (cardId) => !allCardIds.includes(cardId),
    )
  } else {
    const existingIds = new Set(localSelectedCards.value)
    allCardIds.forEach((cardId) => existingIds.add(cardId))
    localSelectedCards.value = Array.from(existingIds)
  }

  await nextTick()

  emit('update-selected-cards', [...localSelectedCards.value])
}
</script>

<style scoped>
.cards-section {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  margin-bottom: 16px;
}

.search-input {
  background: transparent !important;
}

.search-input :deep(.q-field__control),
.search-input :deep(.q-field__control::before),
.search-input :deep(.q-field__control::after) {
  background-color: transparent !important;
}

/* Light mode  */
body:not(.body--dark) .search-input :deep(.q-field__control) {
  background-color: white !important;
}

/* Dark mode */
body.body--dark .search-input :deep(.q-field__control) {
  background-color: #232a38 !important;
  color: white !important;
}

.search-input :deep(.q-field__native) {
  padding-left: 12px !important;
}

body.body--dark .search-input :deep(.q-field__label),
body.body--dark .search-input :deep(.q-field__native),
body.body--dark .search-input :deep(.q-icon) {
  color: #fff !important;
}

body.body--dark .search-input :deep(.q-field__control::before) {
  border-color: #4a5568 !important;
}

.promotion-cards-title {
  background-color: #2d3748;
  padding: 12px 16px;
  border-radius: 6px;
  color: white;
}

.cards-wrapper {
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.table-container {
  flex-grow: 1;
  overflow-y: auto;
  margin-bottom: 16px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.cards-table {
  width: 100%;
  border-radius: 10px;
  overflow: hidden;
  border: 2px solid #d1c0a8;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.cards-table :deep(.q-table tbody tr) {
  height: 90px;
}

/* Styles commun */
.cards-table :deep(.q-table thead tr) {
  position: sticky;
  top: 0;
  z-index: 10;
}

/* Light Mode Styles */
body:not(.body--dark) .cards-table :deep(.q-table thead tr) {
  background-color: #676767;
}

body:not(.body--dark) .cards-table :deep(.q-table th) {
  color: #ffffff;
  border-bottom: 2px solid #d1c0a8;
}

body:not(.body--dark) .cards-table :deep(.q-table tbody tr) {
  background-color: #f5f0e8;
  color: #000;
}

body:not(.body--dark) .cards-table :deep(.q-table tbody tr:hover) {
  background-color: #f5f0e8c7;
}

body:not(.body--dark) .cards-table :deep(.q-table td) {
  border-bottom: 1px solid #d1c0a8;
}

/* Dark Mode Styles */
body.body--dark .cards-table :deep(.q-table thead tr) {
  background-color: #2d3748;
}

body.body--dark .cards-table :deep(.q-table th) {
  color: #e2e8f0;
  border-bottom: 2px solid #4a5568;
}

body.body--dark .cards-table :deep(.q-table tbody tr),
body.body--dark .cards-table :deep(.q-table tbody td) {
  background-color: #1a202c;
  color: white !important;
}

body.body--dark
  .cards-table
  :deep(
    .q-table tbody td *:not(.stock_status_low):not(.stock_status_medium):not(.stock_status_high)
  ) {
  color: white !important;
}

body.body--dark .cards-table :deep(.q-table tbody tr:hover) {
  background-color: #2d3748;
}

body.body--dark .cards-table :deep(.q-table td) {
  border-bottom: 1px solid #4a5568;
}

/* Styles  Commun */
.cards-table :deep(.q-table thead tr) {
  position: sticky;
  top: 0;
  z-index: 10;
}

.cards-table :deep(.q-table th) {
  font-weight: bold;
}

.cards-table :deep(.q-table tbody tr) {
  height: 100px;
}

.cards-table :deep(.q-table td) {
  vertical-align: middle !important;
  height: 100px;
}

.cards-table :deep(.q-table td.wrap-text) {
  max-width: 180px;
  word-wrap: break-word;
  overflow-wrap: break-word;
  white-space: normal;
  line-height: 1.2;
  padding-top: 8px;
  padding-bottom: 8px;
}

.image-wrapper {
  position: relative;
  width: 63px;
  height: 88px;
  flex-shrink: 0;
  border-radius: 4px;
  overflow: hidden;
  margin: 0;
  padding: 0;

  transition: transform 0.2s ease;
  z-index: 1;
}

.image-wrapper:hover {
  transform: scale(1.1);
}
.catalog-card-image {
  width: 100%;
  height: 100%;
  border-radius: 4px;
  box-shadow: none;
  position: relative;
  z-index: 1;
  display: block;
  object-fit: contain;
}

/* Effet foil */
.foil-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 4px;
  background: linear-gradient(
    45deg,
    rgba(255, 0, 255, 0.15),
    rgba(0, 255, 255, 0.15),
    rgba(255, 255, 0, 0.15),
    rgba(255, 0, 255, 0.15)
  );
  background-size: 400% 400%;
  animation: foilShine 3s ease infinite;
  pointer-events: none;
  z-index: 2;
  margin: 0;
  padding: 0;
}

.foil-overlay::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 20%;
  height: 100%;
  background: linear-gradient(to right, transparent, rgba(255, 255, 255, 0.2) 50%, transparent);
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

.buttons-container {
  display: flex;
  gap: 8px;
  align-items: center;
}

.action-btn {
  height: 40px;
  min-height: 40px;
  line-height: 1;
  border-radius: 4px;
  font-weight: 500;
  padding: 0 16px;
  transition: all 0.2s ease;
  white-space: nowrap;
  font-size: 14px;
  min-width: 140px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.search-btn {
  background-color: #8d6e63;
  color: white;
}

.search-btn:hover {
  background-color: #6d4c41;
}

.delete-btn {
  background-color: #d32f2f;
  color: white;
}

.delete-btn:hover {
  background-color: #b71c1c;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.add-card-btn {
  background-color: #1976d2;
  color: white;
}

.checkbox-container {
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.custom-checkbox {
  appearance: none;
  -webkit-appearance: none;
  width: 22px;
  height: 22px;
  background-color: #232a38;
  border: 2px solid #000;
  border-radius: 5px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.custom-checkbox:hover {
  background-color: #3b4456;
}

.custom-checkbox:checked {
  background-color: #1e40af;
  border-color: #1e40af;
  animation: pop 0.2s ease forwards;
}

.custom-checkbox:checked::after {
  content: '✓';
  color: #fff;
  font-size: 14px;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: pop-check 0.2s ease forwards;
}

.custom-checkbox:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.custom-checkbox:disabled:hover {
  background-color: #232a38;
}

@keyframes pop {
  0% {
    transform: scale(0.9);
  }
  50% {
    transform: scale(1.2);
  }
  100% {
    transform: scale(1);
  }
}

@keyframes pop-check {
  0% {
    transform: translate(-50%, -50%) scale(0);
    opacity: 0;
  }
  50% {
    transform: translate(-50%, -50%) scale(1.3);
    opacity: 1;
  }
  100% {
    transform: translate(-50%, -50%) scale(1);
  }
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 16px;
  flex-shrink: 0;
}

.custom-pagination {
  display: flex;
  justify-content: center;
}

.pagination-list {
  display: flex;
  list-style: none;
  padding: 0;
  margin: 0;
  gap: 5px;
}

.pagination-item {
  display: flex;
}

.pagination-ellipsis {
  padding: 8px 12px;
  color: #6c757d;
}

.table-container::-webkit-scrollbar {
  width: 8px;
}

.table-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

.table-container::-webkit-scrollbar-thumb {
  background: #8d6e63;
  border-radius: 4px;
}

.table-container::-webkit-scrollbar-thumb:hover {
  background: #5d4037;
}

.cards-table tr.stock-low {
  background: rgba(255, 193, 7, 0.06);
}

.cards-table tr.stock-zero {
  background: rgba(220, 53, 69, 0.08);
}

.cards-table tr.stock-low td,
.cards-table tr.stock-zero td {
  transition: background-color 0.2s ease;
}

.highlight-added {
  box-shadow: 0 4px 12px rgba(13, 110, 253, 0.12);
  border-color: rgba(13, 110, 253, 0.22);
}

.stock_status_low {
  color: #ce0707 !important;
  font-weight: bold;
  font-size: large;
}

body.body--dark .stock_status_low {
  color: #ce0707 !important;
}

.stock_status_medium {
  color: #ffc107 !important;
  font-weight: bold;
  font-size: large;
}

body.body--dark .stock_status_medium {
  color: #ffc107 !important;
}

.stock_status_high {
  color: #07ce0e !important;
  font-weight: bold;
  font-size: large;
}

body.body--dark .stock_status_high {
  color: #07ce0e !important;
}

.select-all-icon {
  font-size: 28px;
  cursor: pointer;
  vertical-align: middle;
  color: #333;
  transition: all 0.2s ease;
  user-select: none;
  display: inline-block;
  padding: 6px;
  border-radius: 4px;
  font-weight: 500;
  background-color: rgba(255, 255, 255, 0.8);
  border: 1px solid #ddd;
}

.select-all-icon:hover {
  color: #1976d2;
  background-color: rgba(25, 118, 210, 0.15);
  border-color: #1976d2;
  transform: scale(1.05);
}

.select-all-icon.all-selected {
  color: #28a745;
  font-weight: 600;
  background-color: rgba(40, 167, 69, 0.1);
  border-color: #28a745;
}

.select-all-icon.all-selected:hover {
  color: #218838;
  background-color: rgba(40, 167, 69, 0.15);
  border-color: #218838;
  transform: scale(1.05);
}

/* colonne Prix Foil */
.foil-price {
  font-weight: 600;
  color: #5c6bc0;
}

.no-foil {
  color: #9e9e9e;
  font-style: italic;
}

/* Style pour les cartes nouvellement ajoutées */
.highlight-added {
  background-color: #d4edda !important;
  border-left: 4px solid #28a745 !important;
  transition: all 0.3s ease;
}

.highlight-added:hover {
  background-color: #c3e6cb !important;
}

body.body--dark .cards-table :deep(.q-table tbody tr.highlight-added),
body.body--dark .cards-table :deep(.q-table tbody tr.highlight-added td) {
  background-color: #d4edda !important;
  color: #000 !important;
}

body.body--dark .cards-table :deep(.q-table tbody tr.highlight-added td *) {
  color: #000 !important;
}

body.body--dark .custom-warning-banner {
  background-color: #ffcdd2 !important;
  color: #b71c1c !important;
}

body.body--dark .custom-warning-banner .q-icon {
  color: #c62828 !important;
}

@media (max-width: 1023px) {
  .cards-section {
    padding: 0 8px;
    margin-top: 24px;
  }

  .cards-section .text-subtitle1 {
    font-size: 16px;
    margin-bottom: 12px;
  }

  .promotion-cards-title {
    margin-top: 16px;
  }

  .row.q-mb-md.items-stretch.no-wrap.q-gutter-sm {
    flex-direction: column;
    gap: 8px !important;
  }

  .buttons-container {
    width: 100%;
    flex-direction: column;
    gap: 8px;
  }

  .action-btn {
    width: 100%;
    min-width: 100%;
  }

  .table-container {
    overflow-x: auto;
  }

  .cards-table {
    min-width: 800px;
  }

  .cards-table :deep(.q-table th),
  .cards-table :deep(.q-table td) {
    padding: 8px 4px;
    font-size: 12px;
  }

  .pagination-container {
    margin-top: 12px;
  }

  .pagination-list {
    flex-wrap: wrap;
    justify-content: center;
  }
}

/* Mobile Cards Styles */
.mobile-cards-container {
  padding: 8px;
}

.mobile-card {
  margin-bottom: 12px;
  border: 2px solid #ffffff;
  border-radius: 8px;
  overflow: hidden;
  background-color: #4c4b4b;
  transition: all 0.2s ease;
  color: white;
}

.mobile-card.highlight-added {
  border-color: #28a745;
  box-shadow: 0 0 0 2px rgba(40, 167, 69, 0.3);
}

.mobile-card.stock-low {
  border-left: 4px solid #ffc107;
}

.mobile-card.stock-zero {
  border-left: 4px solid #dc3545;
}

.mobile-card-header {
  display: flex;
  gap: 12px;
  padding: 12px;
  background-color: #4c4b4b;
  border-bottom: 2px solid #ffffff;
}

.mobile-card-image-container {
  flex-shrink: 0;
  width: 100px;
}

.image-wrapper-mobile {
  position: relative;
  width: 100%;
  height: auto;
  border-radius: 6px;
  overflow: hidden;
  border: 2px solid #ffffff;
  transition: transform 0.2s;
}

.image-wrapper-mobile.foil-card {
  box-shadow: 0 0 15px rgba(156, 39, 176, 0.5);
}

.mobile-card-image {
  width: 100%;
  height: auto;
  border-radius: 6px;
  display: block;
}

.image-wrapper-mobile:hover {
  transform: scale(1.05);
}

.foil-overlay-mobile {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 6px;
  background: linear-gradient(
    45deg,
    rgba(255, 0, 255, 0.3),
    rgba(0, 255, 255, 0.3),
    rgba(255, 255, 0, 0.3),
    rgba(255, 0, 255, 0.3)
  );
  background-size: 400% 400%;
  animation: foilShine 4s ease infinite;
  pointer-events: none;
  z-index: 2;
  mix-blend-mode: overlay;
  opacity: 0.7;
}

.mobile-card-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: #fff;
}

.card-name {
  font-size: 16px;
  font-weight: bold;
  line-height: 1.2;
  color: #ffffff;
}

.card-set {
  font-size: 13px;
  color: #e0e0e0;
  line-height: 1.2;
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.card-badges {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

.rarity-badge {
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: bold;
  text-transform: uppercase;
}

.rarity-commune {
  background-color: #666;
  color: #fff;
}

.rarity-peu-commune {
  background-color: #07ce0e;
  color: #000;
}

.rarity-rare {
  background-color: #ce7107;
  color: #fff;
}

.rarity-mythique {
  background-color: #ce0707;
  color: #fff;
}

.stock-indicator {
  font-size: 12px;
  font-weight: bold;
  padding: 3px 8px;
  border-radius: 4px;
}

.stock-indicator {
  font-size: 12px;
  font-weight: bold;
}

.stock-indicator.stock_status_low {
  color: #ce0707;
}

.stock-indicator.stock_status_medium {
  color: #ffc107;
}

.stock-indicator.stock_status_high {
  color: #07ce0e;
}

.card-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 4px;
}

.card-detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
}

.detail-label {
  font-weight: 600;
  color: #ccc;
}

.detail-value {
  color: #ffffff;
  text-align: right;
}

.foil-price-mobile {
  color: #5c6bc0;
  font-weight: 600;
}

.stock-value {
  font-weight: bold;
}

.mobile-card-selection {
  padding: 12px;
  background-color: #4c4b4b;
  border-top: 2px solid #ffffff;
}

.selection-checkbox-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.custom-checkbox-mobile {
  appearance: none;
  -webkit-appearance: none;
  width: 24px;
  height: 24px;
  background-color: #232a38;
  border: 2px solid #ffffff;
  border-radius: 5px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.custom-checkbox-mobile:hover:not(:disabled) {
  background-color: #3b4456;
}

.custom-checkbox-mobile:checked {
  background-color: #1e40af;
  border-color: #1e40af;
}

.custom-checkbox-mobile:checked::after {
  content: '✓';
  color: #fff;
  font-size: 16px;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.custom-checkbox-mobile:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.checkbox-label-mobile {
  color: #ffffff;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  user-select: none;
}

.custom-checkbox-mobile:disabled + .checkbox-label-mobile {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Mobile Pagination */
.mobile-pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background-color: #4c4b4b;
  border: 2px solid #ffffff;
  border-radius: 8px;
  margin-top: 8px;
}

.mobile-pagination-btn {
  padding: 8px 16px;
  border: 2px solid #ffffff;
  background-color: transparent;
  color: #ffffff;
  font-size: 18px;
  font-weight: bold;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 44px;
}

.mobile-pagination-btn:hover:not(:disabled) {
  background-color: #676767;
  transform: translateY(-1px);
}

.mobile-pagination-btn:active:not(:disabled) {
  transform: translateY(0);
}

.mobile-pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.mobile-pagination-info {
  color: #ffffff;
  font-weight: 500;
  font-size: 14px;
}
</style>
