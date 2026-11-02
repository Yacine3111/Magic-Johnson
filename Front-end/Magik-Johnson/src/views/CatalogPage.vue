<template>
  <div class="page-catalogue-light">
    <!-- Barre de recherche -->
    <div class="d-flex justify-content-center search-container custom-search-bg">
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
          class="form-control search-input custom-search-input"
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
      class="filters-bar d-flex align-items-center gap-2 p-3 text-dark"
      style="background-color: #a89c8d"
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
        <!-- Sous-Type (affiché seulement si des types sont sélectionnés) -->
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

    <!-- Necessaire pour version mobile -->
    <div v-if="isMobile && showFilters" class="sidebar-overlay" @click="showFilters = false"></div>

    <hr class="separator" />

    <!-- Liste des cartes -->
    <div class="container">
      <div class="d-flex justify-content-between align-items-center mb-3 text-dark">
        <div class="d-flex align-items-center gap-3">
          <h5 v-if="totalCardsFromAPI == 1">
            1 carte {{ hasActiveFilters ? 'trouvée' : 'disponible' }}
          </h5>
          <h5 v-else class="mb-0">
            {{ totalCardsFromAPI }} cartes {{ hasActiveFilters ? 'trouvées' : 'disponibles' }}
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
      <div v-if="loading" class="text-center text-dark py-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Chargement...</span>
        </div>
        <p class="mt-3">Chargement des cartes...</p>
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

<style scoped>
.page-catalogue-light {
  background-color: #d1cbbb;
  min-height: 100vh;
  color: black;
}

.custom-search-bg {
  background: #a89c8d !important;
  padding: 2rem 0;
}

.custom-search-input {
  color: black !important;
  background-color: white !important;
}

.custom-search-input::placeholder {
  color: #666 !important;
}

.separator {
  background-color: #2d3748 !important;
}
</style>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useFiltersStore } from '@/stores/filters'
import '../assets/catalogue.css'
import CatalogCards from '@/components/CatalogCards.vue'
import SidebarCatalogue from '@/components/SidebarCatalogue.vue'
import { cardService } from '@/services/CardService'
import TranslationService from '@/services/translationService'
import { searchSuggestionsService } from '@/services/searchSuggestionsService'
import Pagination from '@/components/common/Pagination.vue'

const filters = useFiltersStore()
const showFilters = ref(false)
const isMobile = ref(false)

// Variables pour la recherche dans les filtres
const searchSerie = ref('')
const searchRarete = ref('')
const searchType = ref('')
const searchSousType = ref('')
const searchCouleur = ref('')
const searchLangue = ref('')

const searchDisponibilite = ref('')
const searchFinition = ref('')

// Variables pour les sous-types
const availableSubtypes = ref([])
const loadingSubtypes = ref(false)

// Variables pour les suggestions de recherche
const searchSuggestions = ref([])
const showSuggestions = ref(false)
let isMouseInSuggestions = false

// Fonction pour basculer les filtres
const toggleFilter = (filterType, value) => {
  const filterArray = filters[filterType]
  if (filterArray.includes(value)) {
    filters[filterType] = filterArray.filter((item) => item !== value)
  } else {
    filters[filterType] = [...filterArray, value]
  }
}

// État pour les cartes et le chargement
const cards = ref([])
const loading = ref(false)
const apiFilterOptions = ref(null)
const totalCardsFromAPI = ref(0)
const totalPagesFromAPI = ref(0)
let currentRequest = null
const error = ref('')

// Filtrer et trier les cartes depuis l'API
const filteredAndSortedCards = computed(() => {
  return cards.value
})

// Options de filtres pour sidebar depuis l'API
const filterOptions = computed(() => {
  if (apiFilterOptions.value) {
    return {
      Série: apiFilterOptions.value.series || [],
      Rareté: TranslationService.translateRarities(apiFilterOptions.value.rarities || []),
      Type: TranslationService.translateTypes(apiFilterOptions.value.types || []),
      Couleur: TranslationService.translateColors(apiFilterOptions.value.colors || []),
      Langue: TranslationService.translateLanguages(apiFilterOptions.value.languages || []),
      Disponibilité: (apiFilterOptions.value.availabilities || []).map((a) =>
        TranslationService.translateAvailability(a),
      ),

      Finition: ['Foil', 'Non-Foil'],
      Description: [],
    }
  }

  // options vides si pas de données
  return {
    Série: [],
    Rareté: [],
    Type: [],
    Couleur: [],
    Langue: [],
    Disponibilité: [],

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

// Pagination depuis l'API
const totalPages = computed(() => {
  return totalPagesFromAPI.value
})

const paginatedCards = computed(() => {
  return filteredAndSortedCards.value
})

//montrer les pages autour de la page actuelle
const windowSize = computed(() => (isMobile.value ? 3 : 5))

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = filters.currentPage
  const half = Math.floor(windowSize.value / 2)

  let start = Math.max(2, current - half)
  let end = Math.min(total - 1, current + half)

  // Ajuster fenêtre si proche des bords
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

// Détecter si des filtres sont actifs
const hasActiveFilters = computed(() => {
  return (
    filters.searchQuery.trim() !== '' ||
    filters.disponibilite.length > 0 ||
    filters.langue.length > 0 ||
    filters.finition.length > 0 ||
    filters.type.length > 0 ||
    filters.sousType.length > 0 ||
    filters.description.trim() !== '' ||
    filters.couleur.length > 0 ||
    filters.rarete.length > 0 ||
    filters.serie.length > 0 ||
    filters.sortBy !== ''
  )
})

// Watcher pour charger les sous-types quand les types changent
watch(
  () => filters.type,
  async () => {
    filters.sousType = []
    await loadSubtypes()
  },
  { deep: true },
)

// Fonction pour charger les suggestions de recherche
const loadSearchSuggestions = async (query) => {
  const suggestions = await searchSuggestionsService.loadSuggestions(query, 0, 5)
  searchSuggestions.value = suggestions
  showSuggestions.value = suggestions.length > 0
}

// Gestionnaire pour l'input de recherche
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

// Gestionnaire pour le focus
const handleSearchFocus = () => {
  if (searchSuggestions.value.length > 0 && filters.searchQuery.length >= 3) {
    showSuggestions.value = true
  }
}

// Gestionnaire pour le blur (fermer les suggestions après un délai)
let blurTimeout = null

const handleSearchBlur = (event) => {
  // Annuler le timeout précédent s'il existe
  if (blurTimeout) {
    clearTimeout(blurTimeout)
  }

  // Attendre un peu pour voir si on clique sur les suggestions
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

// Sélectionner une suggestion
const selectSuggestion = (suggestion) => {
  filters.searchQuery = suggestion
  searchSuggestions.value = []
  showSuggestions.value = false
}

// Rechargement automatique quand les filtres changent
let filterTimeout = null
watch(
  () => [
    filters.searchQuery,
    filters.disponibilite,

    filters.langue,
    filters.finition,
    filters.type,

    filters.sousType,
    filters.couleur,
    filters.rarete,
    filters.serie,
    filters.sortBy,
    filters.priceMin,
    filters.priceMax,
    filters.description,
  ],
  async () => {
    // éviter trop de requêtes
    if (filterTimeout) clearTimeout(filterTimeout)

    filterTimeout = setTimeout(async () => {
      filters.currentPage = 1
      try {
        await loadCards(1)
      } catch (error) {
        if (error && error.message && !error.message.includes('cancelled')) {
          console.warn('Erreur lors du rechargement avec filtres:', error)
        }
      }
    }, 300)
  },
)

// Recharge quand la page change
watch(
  () => filters.currentPage,
  async (newPage) => {
    window.scrollTo({ top: 0, behavior: 'auto' })
    try {
      await loadCards(newPage)
    } catch (error) {
      console.warn('Erreur lors du changement de page:', error)
    }
  },
)

// Gestionnaire pour fermer la sidebar mobile et les dropdowns
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

// Voir si l'appareil est mobile
const checkMobile = () => {
  isMobile.value = window.innerWidth <= 768
}

// Fonction pour charger les cartes depuis l'API avec filtres
const loadCards = async (page = null) => {
  if (currentRequest) {
    currentRequest.cancelled = true
  }

  const requestId = { cancelled: false }
  currentRequest = requestId

  const currentSearchQuery = filters.searchQuery

  loading.value = true

  try {
    const currentPage = page || filters.currentPage

    // conversion français vers anglais
    const apiFilters = {
      searchQuery: currentSearchQuery,
      serie: filters.serie,
      rarete: filters.rarete.map((r) => TranslationService.reverseTranslateRarity(r)),
      type: filters.type.map((t) => TranslationService.reverseTranslateType(t)),
      sousType: filters.sousType.map((st) => TranslationService.reverseTranslateSubtype(st)),
      couleur: filters.couleur.map((c) => TranslationService.reverseTranslateColor(c)),
      langue: filters.langue.map((l) => TranslationService.reverseTranslateLanguage(l)),
      disponibilite: filters.disponibilite,
      finition: filters.finition,
      sortBy: filters.sortBy,
      priceMin: filters.priceMin,
      priceMax: filters.priceMax,
      description: filters.description,
    }

    const response = await cardService.getCards(currentPage, filters.itemsPerPage, apiFilters)

    if (requestId.cancelled || filters.searchQuery !== currentSearchQuery) {
      return
    }

    if (response && response.cards) {
      const mappedCards = response.cards.map((card) => ({
        ...card,
        rarete: TranslationService.translateRarity(card.rarete),
        type: TranslationService.translateType(card.type),
        couleur: TranslationService.translateColor(card.couleur),
        langue: TranslationService.translateLanguage(card.langue),
        disponibilite: TranslationService.translateAvailability(card.disponibilite),
      }))

      const searchQuery = currentSearchQuery?.trim().toLowerCase()

      if (searchQuery && !filters.sortBy) {
        const rankedCards = mappedCards
          .map((card, index) => {
            const name = (card.name || '').toLowerCase()
            const exactMatch = name === searchQuery
            const prefixMatch = !exactMatch && name.startsWith(searchQuery)
            const containsIndex = !exactMatch ? name.indexOf(searchQuery) : -1

            let rank = 3
            if (exactMatch) {
              rank = 0
            } else if (prefixMatch) {
              rank = 1
            } else if (containsIndex !== -1) {
              rank = 2 + containsIndex / 100
            }

            return { card, index, rank }
          })
          .sort((a, b) => {
            if (a.rank !== b.rank) {
              return a.rank - b.rank
            }
            return a.index - b.index
          })
          .map((entry) => entry.card)

        cards.value = rankedCards
      } else {
        cards.value = mappedCards
      }
      totalCardsFromAPI.value = response.pagination.totalCards
      totalPagesFromAPI.value = response.pagination.totalPages
    } else {
      if (!currentSearchQuery || currentSearchQuery.trim() === '') {
        cards.value = []
        totalCardsFromAPI.value = 0
        totalPagesFromAPI.value = 0
      } else {
        throw new Error("Pas de cartes retournées par l'API")
      }
    }
  } catch (err) {
    throw err
  } finally {
    if (currentRequest === requestId) {
      loading.value = false
      currentRequest = null
    }
  }
}

// Fonction pour charger les options de filtres
const loadFilterOptions = async () => {
  try {
    const response = await cardService.getFilterOptions()
    apiFilterOptions.value = response
  } catch (err) {
    console.error('Erreur lors du chargement des filtres:', err)
  }
}

// Fonction pour charger les sous-types basés sur les types sélectionnés
const loadSubtypes = async () => {
  if (filters.type.length === 0) {
    availableSubtypes.value = []
    return
  }

  try {
    loadingSubtypes.value = true
    const englishTypes = filters.type.map((t) => TranslationService.reverseTranslateType(t))
    const response = await cardService.getSubtypes(englishTypes)
    availableSubtypes.value = response.subTypes || []
  } catch (err) {
    console.error('Erreur lors du chargement des sous-types:', err)
    availableSubtypes.value = []
  } finally {
    loadingSubtypes.value = false
  }
}

onMounted(async () => {
  filters.searchQuery = ''

  try {
    checkMobile()
    window.addEventListener('resize', checkMobile)

    document.addEventListener('click', handleClickOutside)

    document.addEventListener('show.bs.dropdown', (event) => {
      const allDropdowns = document.querySelectorAll('.dropdown-menu.show')
      allDropdowns.forEach((menu) => {
        const parentDropdown = menu.closest('.dropdown')
        const currentDropdown = event.target.closest('.dropdown')
        if (parentDropdown !== currentDropdown) {
          menu.classList.remove('show')
        }
      })
    })

    try {
      await loadCards()
      await loadFilterOptions()
    } catch (apiError) {}
  } catch (error) {
    console.error("Erreur lors de l'initialisation:", error)
    error.value = "Une erreur inattendue s'est produite. Veuillez actualiser la page."
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', checkMobile)
  document.removeEventListener('click', handleClickOutside)
  searchSuggestionsService.cancelCurrentRequest()
  if (blurTimeout) {
    clearTimeout(blurTimeout)
  }
})
</script>

<style>
body.body--dark .custom-search-bg {
  background: #1f2735 !important;
}

body.body--dark .filters-bar {
  background-color: #1f2735 !important;
  color: white !important;
}
</style>
