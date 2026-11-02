<template>
  <div class="page-behind">
    <!-- En-tête de la promotion -->
    <div class="promotion-header container mb-4">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h2 class="text-light mb-2">{{ promotion?.title || promotion?.nom || 'Promotion' }}</h2>
          <p class="text-light mb-2" v-if="promotion?.description">{{ promotion.description }}</p>
          <div class="d-flex align-items-center gap-3 text-light">
            <span v-if="promotion?.rate">
              <strong>Rabais : {{ promotion.rate }}%</strong>
            </span>
            <span v-if="countdown.days >= 0 || countdown.hours >= 0 || countdown.minutes >= 0">
              <strong>
                {{ countdown.isStartOfPromotion ? 'Début dans :' : 'Fin dans :' }}
                {{ countdown.days }}j {{ countdown.hours }}h {{ countdown.minutes }}m
                {{ countdown.secondes }}s
              </strong>
            </span>
          </div>
        </div>
        <button class="btn btn-secondary" @click="$router.go(-1)">
          <span class="material-symbols-outlined">arrow_back</span>
          Retour
        </button>
      </div>
    </div>

    <!-- Barre de recherche -->
    <div class="d-flex justify-content-center search-container">
      <div class="input-group search-wrapper" :class="{ 'mobile-search': isMobile }">
        <button class="btn search-btn" type="button">
          <img src="https://cdn-icons-png.flaticon.com/512/109/109859.png" class="search-icon" />
        </button>
        <input
          type="search"
          v-model="filters.searchQuery"
          @input="handleSearchInput"
          @focus="handleSearchFocus"
          @blur="handleSearchBlur"
          placeholder="Rechercher une carte..."
          class="form-control search-input"
        />
        <!-- Dropdown des suggestions -->
        <div
          v-if="showSuggestions && searchSuggestions.length > 0"
          class="search-suggestions"
          @mousedown.prevent
          @mouseenter="isMouseInSuggestions = true"
          @mouseleave="isMouseInSuggestions = false"
        >
          <div
            v-for="(suggestion, index) in searchSuggestions"
            :key="index"
            class="suggestion-item"
            @mousedown="selectSuggestion(suggestion)"
          >
            {{ suggestion }}
          </div>
        </div>
      </div>
    </div>

    <!-- Barre de filtres -->
    <div
      class="filters-bar d-flex align-items-center gap-2 p-3 text-light"
      style="background-color: rgb(35, 42, 56)"
    >
      <!-- Bouton Tous les filtres -->
      <button
        id="toggle-filters-btn"
        class="btn btn-secondary btn-sm"
        @click="showFilters = !showFilters"
      >
        <span v-if="!isMobile">Tous les filtres (actifs: {{ filters.activeFiltersCount }})</span>
        <span v-else>Filtres ({{ filters.activeFiltersCount }})</span>
      </button>

      <!-- Version ordi des filtres -->
      <template v-if="!isMobile">
        <!-- Série -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.serie.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.serie.length === 1 ? filters.serie[0] : 'Série' }}
            </span>
            <span v-if="filters.serie.length > 1" class="badge bg-primary ms-2">{{
              filters.serie.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Série'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchSerie"
                  placeholder="Rechercher une série..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredSerie"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('serie', opt)"
                >
                  <input type="checkbox" :checked="filters.serie.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Série']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('serie', opt)"
              >
                <input type="checkbox" :checked="filters.serie.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Rareté -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.rarete.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.rarete.length === 1 ? filters.rarete[0] : 'Rareté' }}
            </span>
            <span v-if="filters.rarete.length > 1" class="badge bg-primary ms-2">{{
              filters.rarete.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Rareté'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchRarete"
                  placeholder="Rechercher une rareté..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredRarete"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('rarete', opt)"
                >
                  <input type="checkbox" :checked="filters.rarete.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Rareté']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('rarete', opt)"
              >
                <input type="checkbox" :checked="filters.rarete.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Type -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.type.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.type.length === 1 ? filters.type[0] : 'Type' }}
            </span>
            <span v-if="filters.type.length > 1" class="badge bg-primary ms-2">{{
              filters.type.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Type'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchType"
                  placeholder="Rechercher un type..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredType"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('type', opt)"
                >
                  <input type="checkbox" :checked="filters.type.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Type']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('type', opt)"
              >
                <input type="checkbox" :checked="filters.type.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Sous-Type  -->
        <div v-if="filters.type.length > 0 && availableSubtypes.length > 0" class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.sousType.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.sousType.length === 1 ? filters.sousType[0] : 'Sous-Type' }}
            </span>
            <span v-if="filters.sousType.length > 1" class="badge bg-primary ms-2">{{
              filters.sousType.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="availableSubtypes.length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchSousType"
                  placeholder="Rechercher un sous-type..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredSousType"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('sousType', opt)"
                >
                  <input type="checkbox" :checked="filters.sousType.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in availableSubtypes"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('sousType', opt)"
              >
                <input type="checkbox" :checked="filters.sousType.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Couleur -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.couleur.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.couleur.length === 1 ? filters.couleur[0] : 'Couleur' }}
            </span>
            <span v-if="filters.couleur.length > 1" class="badge bg-primary ms-2">{{
              filters.couleur.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Couleur'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchCouleur"
                  placeholder="Rechercher une couleur..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredCouleur"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('couleur', opt)"
                >
                  <input type="checkbox" :checked="filters.couleur.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Couleur']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('couleur', opt)"
              >
                <input type="checkbox" :checked="filters.couleur.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Langue -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.langue.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.langue.length === 1 ? filters.langue[0] : 'Langue' }}
            </span>
            <span v-if="filters.langue.length > 1" class="badge bg-primary ms-2">{{
              filters.langue.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Langue'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchLangue"
                  placeholder="Rechercher une langue..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredLangue"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('langue', opt)"
                >
                  <input type="checkbox" :checked="filters.langue.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Langue']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('langue', opt)"
              >
                <input type="checkbox" :checked="filters.langue.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>
        <!-- Disponibilité -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.disponibilite.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.disponibilite.length === 1 ? filters.disponibilite[0] : 'Disponibilité' }}
            </span>
            <span v-if="filters.disponibilite.length > 1" class="badge bg-primary ms-2">{{
              filters.disponibilite.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <template v-if="filterOptions['Disponibilité'].length >= 8">
              <div class="dropdown-search">
                <input
                  type="text"
                  v-model="searchDisponibilite"
                  placeholder="Rechercher une disponibilité..."
                  class="form-control form-control-sm"
                />
              </div>
              <div class="dropdown-list-container">
                <div
                  v-for="opt in filteredDisponibilite"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('disponibilite', opt)"
                >
                  <input type="checkbox" :checked="filters.disponibilite.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </template>
            <template v-else>
              <div
                v-for="opt in filterOptions['Disponibilité']"
                :key="opt"
                class="dropdown-checkbox-item"
                @click="toggleFilter('disponibilite', opt)"
              >
                <input type="checkbox" :checked="filters.disponibilite.includes(opt)" />
                {{ opt }}
              </div>
            </template>
          </div>
        </div>

        <!-- Finition -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="filters.finition.length === 1 ? 'btn-info' : 'btn-secondary'"
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>
              {{ filters.finition.length === 1 ? filters.finition[0] : 'Finition' }}
            </span>
            <span v-if="filters.finition.length > 1" class="badge bg-primary ms-2">{{
              filters.finition.length
            }}</span>
          </button>
          <div class="dropdown-menu p-2">
            <div
              v-for="opt in ['Foil', 'Non-Foil']"
              :key="opt"
              class="dropdown-checkbox-item"
              @click="toggleFilter('finition', opt)"
            >
              <input type="checkbox" :checked="filters.finition.includes(opt)" />
              {{ opt }}
            </div>
          </div>
        </div>
        <!-- Prix -->
        <div class="dropdown">
          <button
            class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
            :class="
              filters.priceMin !== null || filters.priceMax !== null ? 'btn-info' : 'btn-secondary'
            "
            type="button"
            data-bs-toggle="dropdown"
            data-bs-auto-close="outside"
          >
            <span>Prix</span>
            <span
              v-if="filters.priceMin !== null || filters.priceMax !== null"
              class="badge bg-primary ms-2"
            >
              {{ filters.priceMin !== null ? filters.priceMin + ' $' : '0 $' }} -
              {{ filters.priceMax !== null ? filters.priceMax + ' $' : '∞' }}
            </span>
          </button>
          <div class="dropdown-menu p-3" style="min-width: 200px">
            <div class="mb-2">
              <label class="form-label text-white small mb-1">Min ($)</label>
              <input
                type="number"
                v-model.number="filters.priceMin"
                class="form-control form-control-sm"
                placeholder="0"
                min="0"
              />
            </div>
            <div class="mb-2">
              <label class="form-label text-white small mb-1">Max</label>
              <input
                type="number"
                v-model.number="filters.priceMax"
                class="form-control form-control-sm"
                placeholder="∞"
                min="0"
              />
            </div>
            <div class="d-flex justify-content-end mt-2">
              <button
                class="btn btn-xs btn-danger"
                @click="((filters.priceMin = null), (filters.priceMax = null))"
                v-if="filters.priceMin !== null || filters.priceMax !== null"
              >
                Effacer
              </button>
            </div>
          </div>
        </div>
      </template>

      <!-- Bouton de tri pour mobile -->
      <div v-if="isMobile" class="mobile-sort">
        <select v-model="filters.sortBy" class="form-select form-select-sm">
          <option value="">Trier</option>
          <option value="price-asc">Prix croissant</option>
          <option value="price-desc">Prix décroissant</option>
          <option value="name-asc">Nom (A-Z)</option>
          <option value="name-desc">Nom (Z-A)</option>
        </select>
      </div>

      <div class="d-flex gap-2 ms-auto">
        <button class="btn btn-danger btn-sm" @click="filters.clearFilters()">
          <span v-if="!isMobile">Effacer les filtres</span>
          <span v-else>Effacer</span>
        </button>
      </div>
    </div>

    <!-- Utiliser SidebarCatalogue -->
    <SidebarCatalogue
      v-if="isMobile || showFilters"
      :showFilters="showFilters"
      @update:showFilters="showFilters = $event"
      :isMobile="isMobile"
      :filterOptions="filterOptions"
      :activeFilters="filters.activeFilters"
      :filteredSerie="filteredSerie"
      :filteredRarete="filteredRarete"
      :filteredType="filteredType"
      :filteredCouleur="filteredCouleur"
      :filteredLangue="filteredLangue"
      :filteredDisponibilite="filteredDisponibilite"
      :searchSerie="searchSerie"
      @update:searchSerie="searchSerie = $event"
      :searchRarete="searchRarete"
      @update:searchRarete="searchRarete = $event"
      :searchType="searchType"
      @update:searchType="searchType = $event"
      :searchCouleur="searchCouleur"
      @update:searchCouleur="searchCouleur = $event"
      :searchLangue="searchLangue"
      @update:searchLangue="searchLangue = $event"
      :searchDisponibilite="searchDisponibilite"
      @update:searchDisponibilite="searchDisponibilite = $event"
      :searchSousType="searchSousType"
      @update:searchSousType="searchSousType = $event"
      :availableSubtypes="availableSubtypes"
      :filteredFinition="filteredFinition"
      :toggleFilter="toggleFilter"
    />

    <!-- Nécessaire pour version mobile -->
    <div v-if="isMobile && showFilters" class="sidebar-overlay" @click="showFilters = false"></div>

    <hr class="separator" />

    <!-- Liste des cartes -->
    <div class="container">
      <div class="d-flex justify-content-between align-items-center mb-3 text-light">
        <div class="d-flex align-items-center gap-3">
          <h5 v-if="filteredAndSortedCards.length === 1">
            1 carte {{ hasActiveFilters ? 'trouvée' : 'disponible' }} dans cette promotion
          </h5>
          <h5 v-else class="mb-0">
            {{ filteredAndSortedCards.length }} cartes
            {{ hasActiveFilters ? 'trouvées' : 'disponibles' }} dans cette promotion
          </h5>
        </div>

        <!-- Tri pour ordi -->
        <div v-if="!isMobile" class="d-flex align-items-center gap-2">
          <span>Trier par :</span>
          <select v-model="filters.sortBy" class="form-select form-select-sm" style="width: auto">
            <option value="">Rien</option>
            <option value="price-asc">Prix croissant</option>
            <option value="price-desc">Prix décroissant</option>
            <option value="name-asc">Nom (A-Z)</option>
            <option value="name-desc">Nom (Z-A)</option>
          </select>
        </div>
      </div>
      <hr class="separator" />

      <!-- Indicateur de chargement -->
      <div v-if="loading" class="text-center text-light py-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Chargement...</span>
        </div>
        <p class="mt-3">Chargement des cartes...</p>
      </div>

      <!-- Message d'erreur -->
      <div v-else-if="error" class="alert alert-warning text-center" role="alert">
        {{ error }}
      </div>

      <!-- Utiliser CartesCatalogue.vue -->
      <CatalogCards
        v-else
        :cards="paginatedCards"
        :isMobile="isMobile"
        :selectedFinishes="filters.finition"
      />

      <!-- Pagination -->
      <div class="pagination-container d-flex justify-content-center mt-4 mb-4">
        <Pagination
          :currentPage="filters.currentPage"
          :totalPages="totalPages"
          @page-change="changePage"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useFiltersStore } from '@/stores/filters'
import { useCartStore } from '@/stores/cartStore'
import { promotionService } from '@/services/PromotionService'
import { cardService } from '@/services/CardService'
import { searchSuggestionsService } from '@/services/searchSuggestionsService'
import CatalogCards from '@/components/CatalogCards.vue'
import SidebarCatalogue from '@/components/SidebarCatalogue.vue'
import TranslationService from '@/services/translationService'
import Pagination from '@/components/common/Pagination.vue'
import '../assets/catalogue.css'

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const filters = useFiltersStore()
const cartStore = useCartStore()

onUnmounted(() => {
  filters.clearFilters()
})

const promotionId = computed(() => {
  const id = route.params.id
  return String(id)
})

const loading = ref(false)
const error = ref(null)
const allCards = ref([])
const promotion = ref(null)
const isMobile = ref(false)
const currentTime = ref(Date.now())

const showSuggestions = ref(false)
const searchSuggestions = ref([])
let isMouseInSuggestions = false

const handleSearchInput = () => {
  const query = filters.searchQuery?.trim() || ''
  searchSuggestionsService.loadSuggestionsWithDebounce(
    query,
    (suggestions) => {
      searchSuggestions.value = suggestions
      showSuggestions.value = suggestions.length > 0
    },
    300,
  )
}

const handleSearchFocus = () => {
  if (searchSuggestions.value.length > 0 && filters.searchQuery.length >= 3) {
    showSuggestions.value = true
  }
}

let blurTimeout = null

const handleSearchBlur = (event) => {
  if (blurTimeout) {
    clearTimeout(blurTimeout)
  }

  blurTimeout = setTimeout(() => {
    if (isMouseInSuggestions) {
      return
    }

    const activeElement = document.activeElement
    const clickedElement = event.relatedTarget

    if (clickedElement && clickedElement.closest('.search-suggestions')) {
      return
    }

    if (activeElement && activeElement.closest('.search-wrapper')) {
      return
    }

    showSuggestions.value = false
  }, 200)
}

const selectSuggestion = (suggestion) => {
  filters.searchQuery = suggestion
  searchSuggestions.value = []
  showSuggestions.value = false
}

const showFilters = ref(false)
let countdownInterval = null

const searchSerie = ref('')
const searchRarete = ref('')
const searchType = ref('')
const searchSousType = ref('')
const searchCouleur = ref('')
const searchLangue = ref('')
const searchDisponibilite = ref('')

// Variables pour les sous-types
const availableSubtypes = ref([])

// Fonction pour basculer les filtres
const toggleFilter = (filterType, value) => {
  const filterArray = filters[filterType]
  if (filterArray.includes(value)) {
    filters[filterType] = filterArray.filter((item) => item !== value)
  } else {
    filters[filterType] = [...filterArray, value]
  }
}

// Countdown
const countdown = computed(() => {
  if (!promotion.value) {
    return {
      days: 0,
      hours: 0,
      minutes: 0,
      secondes: 0,
      isStartOfPromotion: false,
    }
  }

  const nowMs = currentTime.value
  const startMs = new Date(promotion.value.startDate).getTime()
  const endMs = new Date(promotion.value.endDate).getTime()

  let targetMs
  let isStartOfPromotion = false

  if (nowMs < startMs) {
    targetMs = startMs
    isStartOfPromotion = true
  } else if (nowMs >= startMs && nowMs <= endMs) {
    targetMs = endMs
    isStartOfPromotion = false
  } else {
    return {
      days: 0,
      hours: 0,
      minutes: 0,
      secondes: 0,
      isStartOfPromotion: false,
    }
  }

  const timeLeft = Math.max(0, Math.floor((targetMs - nowMs) / 1000))

  return {
    secondes: timeLeft % 60,
    minutes: Math.floor((timeLeft / 60) % 60),
    hours: Math.floor((timeLeft / 3600) % 24),
    days: Math.floor(timeLeft / 86400),
    isStartOfPromotion,
  }
})

// Fonction pour démarrer le timer de countdown
const startCountdownTimer = () => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }

  countdownInterval = setInterval(() => {
    currentTime.value = Date.now()
  }, 1000)
}

// Options de filtres depuis les cartes chargées
const filterOptions = computed(() => {
  const series = new Set()
  const rarities = new Set()
  const types = new Set()
  const colors = new Set()
  const languages = new Set()
  const availabilities = new Set()

  allCards.value.forEach((card) => {
    if (card.setName) series.add(card.setName)
    if (card.rarity) rarities.add(card.rarity)
    if (card.type) types.add(card.type)
    if (card.colors && Array.isArray(card.colors)) {
      card.colors.forEach((c) => colors.add(c))
    } else if (card.colors) {
      colors.add(card.colors)
    }
    if (card.languages && Array.isArray(card.languages)) {
      card.languages.forEach((l) => languages.add(l))
    } else if (card.language) {
      languages.add(card.language)
    }
    if (card.totalQuantity > 0) {
      availabilities.add('En stock')
    } else {
      availabilities.add('Rupture de stock')
    }
  })

  return {
    Série: Array.from(series).sort(),
    Rareté: TranslationService.translateRarities(Array.from(rarities)),
    Type: TranslationService.translateTypes(Array.from(types)),
    Couleur: TranslationService.translateColors(Array.from(colors)),
    Langue: TranslationService.translateLanguages(Array.from(languages)),
    Langue: TranslationService.translateLanguages(Array.from(languages)),
    Disponibilité: Array.from(availabilities).map((a) =>
      TranslationService.translateAvailability(a),
    ),
    Finition: ['Foil', 'Non-Foil'],
    Description: [],
  }
})

// Options de filtres filtrées pour la recherche
const filteredSerie = computed(() => {
  if (!searchSerie.value) return filterOptions.value['Série']
  return filterOptions.value['Série'].filter((option) =>
    option.toLowerCase().includes(searchSerie.value.toLowerCase()),
  )
})

const filteredRarete = computed(() => {
  if (!searchRarete.value) return filterOptions.value['Rareté']
  return filterOptions.value['Rareté'].filter((option) =>
    option.toLowerCase().includes(searchRarete.value.toLowerCase()),
  )
})

const filteredType = computed(() => {
  if (!searchType.value) return filterOptions.value['Type']
  return filterOptions.value['Type'].filter((option) =>
    option.toLowerCase().includes(searchType.value.toLowerCase()),
  )
})

const filteredCouleur = computed(() => {
  if (!searchCouleur.value) return filterOptions.value['Couleur']
  return filterOptions.value['Couleur'].filter((option) =>
    option.toLowerCase().includes(searchCouleur.value.toLowerCase()),
  )
})

const filteredLangue = computed(() => {
  if (!searchLangue.value) return filterOptions.value['Langue']
  return filterOptions.value['Langue'].filter((option) =>
    option.toLowerCase().includes(searchLangue.value.toLowerCase()),
  )
})

const filteredDisponibilite = computed(() => {
  if (!searchDisponibilite.value) return filterOptions.value['Disponibilité']
  return filterOptions.value['Disponibilité'].filter((option) =>
    option.toLowerCase().includes(searchDisponibilite.value.toLowerCase()),
  )
})

const filteredFinition = computed(() => {
  return filterOptions.value['Finition']
})

const filteredSousType = computed(() => {
  const translatedSubtypes = TranslationService.translateSubtypes(availableSubtypes.value)
  if (!searchSousType.value) return translatedSubtypes
  return translatedSubtypes.filter((option) =>
    option.toLowerCase().includes(searchSousType.value.toLowerCase()),
  )
})

// Détecter si des filtres sont actifs
const hasActiveFilters = computed(() => {
  return (
    filters.searchQuery.trim() !== '' ||
    filters.disponibilite.length > 0 ||
    filters.langue.length > 0 ||
    filters.type.length > 0 ||
    filters.couleur.length > 0 ||
    filters.rarete.length > 0 ||
    filters.serie.length > 0 ||
    filters.sousType.length > 0 ||
    filters.finition.length > 0 ||
    filters.priceMin !== null ||
    filters.priceMax !== null ||
    (filters.description && filters.description.trim() !== '') ||
    filters.sortBy !== ''
  )
})

// Filtrer et trier les cartes localement
const filteredAndSortedCards = computed(() => {
  let filtered = [...allCards.value]

  // Filtre par recherche
  if (filters.searchQuery.trim()) {
    const query = filters.searchQuery.toLowerCase().trim()
    filtered = filtered.filter(
      (card) =>
        card.name?.toLowerCase().includes(query) || card.nom_carte?.toLowerCase().includes(query),
    )
  }

  // Filtre par série
  if (filters.serie.length > 0) {
    filtered = filtered.filter((card) => filters.serie.includes(card.setName))
  }

  // Filtre par rareté
  if (filters.rarete.length > 0) {
    filtered = filtered.filter((card) => {
      const translatedRarity = TranslationService.translateRarity(card.rarity)
      return filters.rarete.includes(translatedRarity)
    })
  }

  // Filtre par type
  if (filters.type.length > 0) {
    filtered = filtered.filter((card) => {
      const translatedType = TranslationService.translateType(card.type)
      return filters.type.includes(translatedType)
    })
  }

  // Filtre par couleur
  if (filters.couleur.length > 0) {
    filtered = filtered.filter((card) => {
      const cardColors = card.colors || []
      const translatedColors = Array.isArray(cardColors)
        ? cardColors.map((c) => TranslationService.translateColor(c))
        : [TranslationService.translateColor(cardColors)]
      return filters.couleur.some((c) => translatedColors.includes(c))
    })
  }

  // Filtre par langue
  if (filters.langue.length > 0) {
    filtered = filtered.filter((card) => {
      const cardLanguages = card.languages || []
      const translatedLanguages = Array.isArray(cardLanguages)
        ? cardLanguages.map((l) => TranslationService.translateLanguage(l))
        : card.language
          ? [TranslationService.translateLanguage(card.language)]
          : []
      return filters.langue.some((l) => translatedLanguages.includes(l))
    })
  }

  // Filtre par prix
  if (filters.priceMin !== null) {
    filtered = filtered.filter((card) => {
      const price =
        card.discount && card.discount > 0
          ? (parseFloat(card.price) || 0) * (1 - card.discount / 100)
          : parseFloat(card.price) || 0
      return price >= filters.priceMin
    })
  }

  if (filters.priceMax !== null) {
    filtered = filtered.filter((card) => {
      const price =
        card.discount && card.discount > 0
          ? (parseFloat(card.price) || 0) * (1 - card.discount / 100)
          : parseFloat(card.price) || 0
      return price <= filters.priceMax
    })
  }

  // Filtre par finition
  if (filters.finition.length > 0) {
    filtered = filtered.filter((card) => {
      const finishes = []

      if ((card.price && card.price > 0) || (!card.hasFoil && !card.foilPrice)) {
        finishes.push('Non-Foil')
      }

      if ((card.foilPrice && card.foilPrice > 0) || card.hasFoil === true) {
        finishes.push('Foil')
      }

      return filters.finition.some((f) => finishes.includes(f))
    })
  }

  // Filtre par description
  if (filters.description && filters.description.trim() !== '') {
    const descQuery = filters.description.toLowerCase().trim()
    filtered = filtered.filter(
      (card) =>
        (card.text || '').toLowerCase().includes(descQuery) ||
        (card.oracle_text || '').toLowerCase().includes(descQuery) ||
        (card.description || '').toLowerCase().includes(descQuery),
    )
  }

  // Filtre par sous-type
  if (filters.sousType.length > 0) {
    filtered = filtered.filter((card) => {
      const rawTypes = card.types || []
      const knownTypes = [
        'Artifact',
        'Battle',
        'Creature',
        'Enchantment',
        'Instant',
        'Land',
        'Planeswalker',
        'Sorcery',
        'Conspiracy',
        'Phenomenon',
        'Plane',
        'Scheme',
        'Tribal',
        'Vanguard',
        'Emblem',
        'Token',
        'Card',
        'Stickers',
        'Attraction',
        'Dungeon',
        'Case',
        'Role',
      ]
      const subtypes = rawTypes.filter((t) => !knownTypes.includes(t))
      const translatedCardSubtypes = TranslationService.translateSubtypes(subtypes)
      return filters.sousType.some((s) => translatedCardSubtypes.includes(s))
    })
  }

  // Filtre par disponibilité
  if (filters.disponibilite.length > 0) {
    filtered = filtered.filter((card) => {
      const availability = card.totalQuantity > 0 ? 'En stock' : 'Rupture de stock'
      const translatedAvailability = TranslationService.translateAvailability(availability)
      return filters.disponibilite.includes(translatedAvailability)
    })
  }

  // Trier
  if (filters.sortBy) {
    filtered.sort((a, b) => {
      switch (filters.sortBy) {
        case 'price-asc': {
          // Utiliser le prix discounté pour le tri
          const priceA =
            a.discount && a.discount > 0
              ? (parseFloat(a.price) || 0) * (1 - a.discount / 100)
              : parseFloat(a.price) || 0
          const priceB =
            b.discount && b.discount > 0
              ? (parseFloat(b.price) || 0) * (1 - b.discount / 100)
              : parseFloat(b.price) || 0
          return priceA - priceB
        }
        case 'price-desc': {
          // Utiliser le prix discounté pour le tri
          const priceA =
            a.discount && a.discount > 0
              ? (parseFloat(a.price) || 0) * (1 - a.discount / 100)
              : parseFloat(a.price) || 0
          const priceB =
            b.discount && b.discount > 0
              ? (parseFloat(b.price) || 0) * (1 - b.discount / 100)
              : parseFloat(b.price) || 0
          return priceB - priceA
        }
        case 'name-asc':
          return (a.name || a.nom_carte || '').localeCompare(b.name || b.nom_carte || '')
        case 'name-desc':
          return (b.name || b.nom_carte || '').localeCompare(a.name || a.nom_carte || '')
        default:
          return 0
      }
    })
  }

  return filtered
})

// Charger les sous-types
const loadSubtypes = () => {
  availableSubtypes.value = []
  if (filters.type.length === 0) return

  const subtypesSet = new Set()
  const knownTypes = [
    'Artifact',
    'Battle',
    'Creature',
    'Enchantment',
    'Instant',
    'Land',
    'Planeswalker',
    'Sorcery',
    'Conspiracy',
    'Phenomenon',
    'Plane',
    'Scheme',
    'Tribal',
    'Vanguard',
    'Emblem',
    'Token',
    'Card',
    'Stickers',
    'Attraction',
    'Dungeon',
    'Case',
    'Role',
  ]

  allCards.value.forEach((card) => {
    const translatedType = TranslationService.translateType(card.type)
    if (filters.type.includes(translatedType)) {
      const rawTypes = card.types || []
      rawTypes.forEach((t) => {
        if (!knownTypes.includes(t)) {
          subtypesSet.add(t)
        }
      })
    }
  })

  availableSubtypes.value = Array.from(subtypesSet).sort()
}

watch(
  () => filters.type,
  () => {
    filters.sousType = []
    loadSubtypes()
  },
  { deep: true },
)

// Pagination
const totalPages = computed(() => {
  return Math.ceil(filteredAndSortedCards.value.length / filters.itemsPerPage)
})

const paginatedCards = computed(() => {
  const start = (filters.currentPage - 1) * filters.itemsPerPage
  const end = start + filters.itemsPerPage
  return filteredAndSortedCards.value.slice(start, end)
})

// Montrer les pages autour de la page actuelle
const windowSize = computed(() => (isMobile.value ? 3 : 5))

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = filters.currentPage
  const half = Math.floor(windowSize.value / 2)

  let start = Math.max(2, current - half)
  let end = Math.min(total - 1, current + half)

  if (current <= half + 1) {
    start = 2
    end = Math.min(total - 1, 1 + windowSize.value)
  } else if (current >= total - half) {
    end = total - 1
    start = Math.max(2, total - windowSize.value)
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return pages
})

const showFirstPage = computed(() => totalPages.value >= 1)
const showLastPage = computed(() => totalPages.value > 1)
const showLeftEllipsis = computed(() => visiblePages.value.length && visiblePages.value[0] > 2)
const showRightEllipsis = computed(
  () =>
    visiblePages.value.length &&
    visiblePages.value[visiblePages.value.length - 1] < totalPages.value - 1,
)

const changePage = (page) => {
  const p = Math.min(Math.max(1, page), totalPages.value)
  if (p !== filters.currentPage) {
    filters.currentPage = p
  }
}

// Watcher pour réinitialiser la page quand les filtres changent
watch(
  () => [
    filters.searchQuery,
    filters.serie,
    filters.rarete,
    filters.type,
    filters.couleur,
    filters.langue,
    filters.disponibilite,
    filters.sortBy,
  ],
  () => {
    filters.currentPage = 1
  },
)

// Charger la promotion et ses cartes
const loadPromotionCards = async () => {
  loading.value = true
  error.value = null

  try {
    // Charger la promotion
    const allPromotions = await promotionService.getAllPromotions()
    const foundPromotion = allPromotions.find((p) => String(p.id) === String(promotionId.value))

    if (!foundPromotion) {
      error.value = 'Promotion introuvable'
      loading.value = false
      return
    }

    promotion.value = foundPromotion

    // Charger les cartes de la promotion
    const promotionCards = await promotionService.getPromotionCards(promotionId.value)

    if (!promotionCards || promotionCards.length === 0) {
      allCards.value = []
      loading.value = false
      return
    }

    const allCardIds = promotionCards.map((card) => card.id || card)

    const allFullCards = await Promise.all(
      allCardIds.map(async (cardId) => {
        try {
          return await cardService.getCard(cardId)
        } catch (error) {
          return promotionCards.find((c) => (c.id || c) === cardId) || null
        }
      }),
    )

    const discountPercentage = promotion.value.rate || 0

    const mappedCards = allFullCards
      .filter((card) => card !== null)
      .map((fullCard) => {
        const originalPrice = fullCard.price || 0
        const foilPrice = fullCard.foilPrice || 0

        let setName = 'Set inconnu'
        if (fullCard.setName) {
          setName = fullCard.setName
        } else if (fullCard.setId) {
          setName = `Set ${fullCard.setId.substring(0, 8)}`
        }

        const rawTypes = Array.isArray(fullCard.types)
          ? fullCard.types
          : fullCard.typesToDisplay || (fullCard.type ? [fullCard.type] : [])

        let primaryType = ''
        if (Array.isArray(rawTypes) && rawTypes.length > 0) {
          const knownTypes = [
            'Artifact',
            'Battle',
            'Creature',
            'Enchantment',
            'Instant',
            'Land',
            'Planeswalker',
            'Sorcery',
            'Conspiracy',
            'Phenomenon',
            'Plane',
            'Scheme',
            'Tribal',
            'Vanguard',
            'Emblem',
            'Token',
            'Card',
            'Stickers',
            'Attraction',
            'Dungeon',
            'Case',
            'Role',
          ]
          primaryType = rawTypes.find((t) => knownTypes.includes(t)) || rawTypes[0] || ''
        }

        if (typeof primaryType === 'string') {
          primaryType = primaryType.split('—')[0]?.split('-')[0]?.trim() || primaryType.trim()
        }

        let normalizedColors = []
        if (Array.isArray(fullCard.colors)) {
          normalizedColors = fullCard.colors
        } else if (typeof fullCard.colors === 'string' && fullCard.colors.trim() !== '') {
          normalizedColors = fullCard.colors
            .split(',')
            .map((color) => color.trim())
            .filter(Boolean)
        }

        return {
          ...fullCard,
          type: primaryType,
          colors: normalizedColors,
          types: rawTypes || [],
          nom_carte: fullCard.name || 'Carte sans nom',
          nom_set: setName,
          serie: setName,
          numero: fullCard.number || 'N/A',
          image_src: `https://api.scryfall.com/cards/${fullCard.scryfallId}?format=image`,
          image: `https://api.scryfall.com/cards/${fullCard.scryfallId}?format=image`,
          alt_text: fullCard.name || 'Image de carte',
          nom: fullCard.name || 'Carte sans nom',
          rarete: fullCard.rarity || 'Inconnue',
          stock: fullCard.totalQuantity || 0,
          hasFoil: fullCard.hasFoil || false,
          price: originalPrice,
          foilPrice: foilPrice,
          discount: discountPercentage,
        }
      })

    allCards.value = mappedCards
  } catch (err) {
    console.error('Erreur lors du chargement des cartes de la promotion:', err)
    error.value = 'Erreur lors du chargement des cartes de la promotion'
    allCards.value = []
  } finally {
    loading.value = false
  }
}

const checkMobile = () => {
  isMobile.value = window.innerWidth < 768 || $q.screen.xs || $q.screen.sm
}

const handleClickOutside = (event) => {
  const sidebar = document.getElementById('sidebar-filters')
  const toggleBtn = document.getElementById('toggle-filters-btn')
  const clickedInSidebar = sidebar && sidebar.contains(event.target)
  const clickedToggle = toggleBtn && toggleBtn.contains(event.target)

  const isFilterElement =
    event.target.closest('.sidebar-checkbox-item') ||
    event.target.closest('.dropdown-checkbox-item') ||
    event.target.closest('.dropdown-menu') ||
    event.target.closest('.btn') ||
    event.target.closest('.filter-btn') ||
    event.target.closest('.reset-btn') ||
    event.target.closest('input[type="checkbox"]') ||
    event.target.closest('input[type="number"]')

  if (showFilters.value) {
    if (clickedInSidebar || isFilterElement) {
      return
    }

    if (!clickedToggle) {
      showFilters.value = false
    }
  }

  if (!isMobile.value) {
    const clickedInDropdown = event.target.closest('.dropdown')
    const clickedDropdownButton = event.target.closest(
      '.dropdown button[data-bs-toggle="dropdown"]',
    )

    if (clickedDropdownButton) {
      const allDropdowns = document.querySelectorAll('.dropdown-menu.show')
      allDropdowns.forEach((menu) => {
        const parentDropdown = menu.closest('.dropdown')
        if (parentDropdown !== clickedInDropdown) {
          menu.classList.remove('show')
        }
      })
    } else if (!clickedInDropdown && !clickedInSidebar && !clickedToggle) {
      const openDropdowns = document.querySelectorAll('.dropdown-menu.show')
      openDropdowns.forEach((menu) => {
        menu.classList.remove('show')
      })
    }
  }
}

const handleDropdownShow = (event) => {
  const allDropdowns = document.querySelectorAll('.dropdown-menu.show')
  allDropdowns.forEach((menu) => {
    const parentDropdown = menu.closest('.dropdown')
    const currentDropdown = event.target.closest('.dropdown')
    if (parentDropdown !== currentDropdown) {
      menu.classList.remove('show')
    }
  })
}

onMounted(async () => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('show.bs.dropdown', handleDropdownShow)
  await loadPromotionCards()
  startCountdownTimer()
  filters.clearFilters()
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('show.bs.dropdown', handleDropdownShow)
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})
</script>

<style scoped>
.promotion-header {
  padding: 20px;
  background-color: rgba(35, 42, 56, 0.8);
  border-radius: 8px;
  margin-top: 20px;
}

.promotion-header h2 {
  color: #fff;
}

.promotion-header .btn {
  display: flex;
  align-items: center;
  gap: 8px;
}

.separator {
  border-color: rgba(255, 255, 255, 0.2);
  margin: 20px 0;
}
</style>
