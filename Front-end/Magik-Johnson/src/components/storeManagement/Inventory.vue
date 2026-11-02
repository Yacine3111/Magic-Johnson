<script setup>
import { ref, computed, onMounted, watch, onBeforeUnmount } from 'vue'
import { cardService } from '@/services/CardService'
import TranslationService from '@/services/translationService'
import { getStockInfo } from '@/services/stockService'
import '@/assets/catalogue.css'
import { useInventoryFiltersStore } from '@/stores/inventoryFilters'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useQuasar } from 'quasar'
import { searchSuggestionsService } from '@/services/searchSuggestionsService'
import { useInventoryCalculations } from '@/composables/useInventoryCalculations'
import { useInventoryEditing } from '@/composables/useInventoryEditing'
import Pagination from '@/components/common/Pagination.vue'
import FilterDropdown from '@/components/common/FilterDropdown.vue'

const $q = useQuasar()
const router = useRouter()

const cards = ref([])
const loading = ref(false)
const error = ref(null)
const currentRequest = ref(null)
const currentPage = ref(1)
const itemsPerPage = ref(18)
const totalCardsFromAPI = ref(0)
const totalPagesFromAPI = ref(0)
const showMobileFilters = ref(false)

// Composables de calcul
const { calculatedTotalQuantity, transformCardForInventory, updateCardStockInfo } =
  useInventoryCalculations(cards)

const activeFiltersCount = computed(() => {
  let count = 0
  if (filters.serie.length > 0) count += filters.serie.length
  if (filters.rarete.length > 0) count += filters.rarete.length
  if (filters.type.length > 0) count += filters.type.length
  if (filters.sousType.length > 0) count += filters.sousType.length
  if (filters.couleur.length > 0) count += filters.couleur.length
  if (filters.langue.length > 0) count += filters.langue.length
  if (filters.disponibilite.length > 0) count += filters.disponibilite.length
  if (filters.finition.length > 0) count += filters.finition.length
  return count
})

const activeFilters = computed(() => {
  return {
    serie: filters.serie,
    rarete: filters.rarete,
    type: filters.type,
    sousType: filters.sousType,
    couleur: filters.couleur,
    langue: filters.langue,
    disponibilite: filters.disponibilite,
    finition: filters.finition,
  }
})

const searchQuery = ref('')
const filters = useInventoryFiltersStore()
const authStore = useAuthStore()
const showFilters = ref(false)
const isMobile = ref(false)
const userRole = ref('')
const isEmployee = ref(false)

// Composable pour l'édition des quantités
const { toggleLanguageEdit, adjustQuantity, cancelLanguageEdit, saveQuantity } =
  useInventoryEditing(isEmployee, cardService, updateCardStockInfo)

// Variables pour les suggestions de recherche
const searchSuggestions = ref([])
const showSuggestions = ref(false)
let isMouseInSuggestions = false
let blurTimeout = null

const totalPages = computed(() => totalPagesFromAPI.value)
const hasNextPage = computed(() => currentPage.value < totalPages.value)
const hasPreviousPage = computed(() => currentPage.value > 1)
const showFirstPage = computed(() => currentPage.value > 3)
const showLastPage = computed(() => currentPage.value < totalPages.value - 2)

const loadCards = async (page = null) => {
  if (currentRequest.value) {
    currentRequest.value.cancelled = true
  }

  const requestId = { cancelled: false }
  currentRequest.value = requestId
  loading.value = true
  error.value = null

  // Vider les cartes au début du chargement pour éviter de voir les anciennes cartes
  cards.value = []

  try {
    const currentPageNum = page || currentPage.value
    const apiFilters = {
      searchQuery: filters.searchQuery,
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
    }

    const response = await cardService.getInventoryCards(
      currentPageNum,
      itemsPerPage.value,
      apiFilters,
    )

    if (requestId.cancelled) return

    if (response?.cards) {
      cards.value = response.cards.map((card) => transformCardForInventory(card))

      totalCardsFromAPI.value = response.pagination.totalCards
      totalPagesFromAPI.value = response.pagination.totalPages
      currentPage.value = response.pagination.currentPage
      error.value = null
    } else {
      throw new Error("Pas de cartes retournées par l'API")
    }
  } catch (err) {
    console.error('Erreur API:', err)
    error.value = 'Service temporairement indisponible. Veuillez réessayer dans quelques instants.'
    throw err
  } finally {
    loading.value = false
  }
}

const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    // Scroller vers le haut de la page instantanément lors du changement de page
    window.scrollTo({ top: 0, behavior: 'auto' })
    loadCards(page)
  }
}

const goToPreviousPage = () => {
  if (hasPreviousPage.value) changePage(currentPage.value - 1)
}

const goToNextPage = () => {
  if (hasNextPage.value) changePage(currentPage.value + 1)
}

const getVisiblePages = () => {
  const pages = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
}

const getRarityClass = (rarity) => {
  const normalized = rarity.toLowerCase().replace(/[^a-z]/g, '')
  if (normalized === 'commune') return 'rarity-commune'
  if (normalized === 'peucommune') return 'rarity-peu-commune'
  if (normalized === 'rare') return 'rarity-rare'
  if (normalized === 'mythique') return 'rarity-mythique'
  return 'rarity-commune'
}

const apiFilterOptions = ref(null)
const availableSubtypes = ref([])
const loadingSubtypes = ref(false)

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
    }
  }
  return {
    Série: [],
    Rareté: [],
    Type: [],
    Couleur: [],
    Langue: [],
    Disponibilité: [],
  }
})

const loadFilterOptions = async () => {
  try {
    const response = await cardService.getFilterOptions()
    apiFilterOptions.value = response
  } catch (err) {
    console.error('Erreur lors du chargement des filtres:', err)
  }
}

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

watch(
  () => filters.type,
  async () => {
    filters.sousType = []
    await loadSubtypes()
  },
  { deep: true },
)

const translatedSubtypes = computed(() => {
  return TranslationService.translateSubtypes(availableSubtypes.value)
})

const handleSearchInput = () => {
  const query = filters.searchQuery?.trim() || ''
  if (query.length < 3) {
    searchSuggestions.value = []
    showSuggestions.value = false
    return
  }
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

let filterTimeout = null
watch(
  () => [
    filters.searchQuery,
    filters.disponibilite,
    filters.langue,
    filters.type,
    filters.sousType,
    filters.couleur,
    filters.rarete,
    filters.serie,
    filters.sortBy,
    filters.priceMin,
    filters.priceMax,
  ],
  async () => {
    if (filterTimeout) clearTimeout(filterTimeout)
    filterTimeout = setTimeout(async () => {
      filters.currentPage = 1
      try {
        loading.value = true
        await loadCards(1)
      } catch (error) {
        console.warn('Erreur lors du rechargement avec filtres:', error)
      } finally {
        loading.value = false
      }
    }, 150)
  },
)

watch(
  () => filters.currentPage,
  async (newPage) => {
    try {
      await loadCards(newPage)
    } catch (error) {
      console.warn('Erreur lors du changement de page:', error)
    }
  },
)

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

onMounted(async () => {
  filters.searchQuery = ''

  loadCards()

  try {
    await authStore.fetchRole()
    userRole.value = authStore.role?.[0] || ''
    isEmployee.value = userRole.value === 'employee'
  } catch (err) {
    console.error('Erreur lors du chargement du rôle:', err)
  }

  document.addEventListener('click', handleClickOutside)
  try {
    await loadFilterOptions()
  } catch (apiError) {
    console.error("Erreur lors du chargement de l'API:", apiError)
    error.value = 'Service temporairement indisponible. Veuillez réessayer dans quelques instants.'
  }
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
  searchSuggestionsService.cancelCurrentRequest()
  if (blurTimeout) {
    clearTimeout(blurTimeout)
  }
})

function goToDetail(id) {
  router.push(`/Cards/${id}`)
}

const parseQuantity = (value) => {
  if (value === 'N/A' || value === null || value === undefined) return 0
  if (typeof value === 'string') {
    const parsed = parseInt(value, 10)
    return isNaN(parsed) ? 0 : parsed
  }
  return typeof value === 'number' ? value : 0
}

const getRowStockInfo = (card, langQuantity) => {
  const foilQty = card.hasFoil ? parseQuantity(langQuantity.foilQuantity) : 0
  const nonFoilQty = card.hasNonFoil ? parseQuantity(langQuantity.nonFoilQuantity) : 0
  const total = foilQty + nonFoilQty

  return getStockInfo(total, card.rarete, card.rarity)
}
</script>

<template>
  <div class="inventaire-container">
    <div class="title-section">
      <div class="text-h5 text-bold">Inventaire</div>
    </div>

    <!-- Barre de recherche Desktop -->
    <div v-if="$q.screen.gt.sm" class="search-section">
      <div class="row items-stretch no-wrap q-gutter-sm search-bar-container">
        <div class="search-wrapper col">
          <q-input
            v-model="filters.searchQuery"
            @update:model-value="handleSearchInput"
            @focus="handleSearchFocus"
            @blur="handleSearchBlur"
            placeholder="Rechercher une carte..."
            outlined
            dense
            class="search-input"
          />

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
    </div>

    <!-- Barre de filtres -->
    <div
      v-if="$q.screen.gt.sm"
      class="filters-bar d-flex align-items-center gap-2 p-3 text-light"
      style="background-color: rgb(35, 42, 56)"
    >
      <!-- Série -->
      <FilterDropdown
        label="Série"
        :options="filterOptions['Série']"
        :selected="filters.serie"
        @update:selected="(val) => (filters.serie = val)"
        force-search
      />

      <!-- Rareté -->
      <FilterDropdown
        label="Rareté"
        :options="filterOptions['Rareté']"
        :selected="filters.rarete"
        @update:selected="(val) => (filters.rarete = val)"
      />

      <!-- Type -->
      <FilterDropdown
        label="Type"
        :options="filterOptions['Type']"
        :selected="filters.type"
        @update:selected="(val) => (filters.type = val)"
      />

      <!-- Sous-Type -->
      <FilterDropdown
        v-if="filters.type.length > 0 && availableSubtypes.length > 0"
        label="Sous-Type"
        :options="translatedSubtypes"
        :selected="filters.sousType"
        @update:selected="(val) => (filters.sousType = val)"
        force-search
      />

      <!-- Couleur -->
      <FilterDropdown
        label="Couleur"
        :options="filterOptions['Couleur']"
        :selected="filters.couleur"
        @update:selected="(val) => (filters.couleur = val)"
      />

      <!-- Langue -->
      <FilterDropdown
        label="Langue"
        :options="filterOptions['Langue']"
        :selected="filters.langue"
        @update:selected="(val) => (filters.langue = val)"
      />

      <!-- Disponibilité -->
      <FilterDropdown
        label="Disponibilité"
        :options="filterOptions['Disponibilité']"
        :selected="filters.disponibilite"
        @update:selected="(val) => (filters.disponibilite = val)"
      />

      <!-- Finition -->
      <FilterDropdown
        label="Finition"
        :options="['Foil', 'Non-Foil']"
        :selected="filters.finition"
        @update:selected="(val) => (filters.finition = val)"
      />

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

      <div class="d-flex gap-2 ms-auto">
        <button class="btn btn-danger btn-sm" @click="filters.clearFilters()">
          Effacer les filtres
        </button>
      </div>
    </div>

    <!-- MOBILE - Barre recherche + bouton filtres -->
    <div v-else class="mobile-filters-bar">
      <div class="mobile-search-container">
        <div class="search-wrapper mobile-search-wrapper">
          <input
            type="search"
            v-model="filters.searchQuery"
            @input="handleSearchInput"
            @focus="handleSearchFocus"
            @blur="handleSearchBlur"
            placeholder="Rechercher une carte..."
            class="mobile-search-input"
          />

          <div
            v-if="showSuggestions && searchSuggestions.length > 0"
            class="search-suggestions mobile-search-suggestions"
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
        <button class="mobile-filters-btn" @click="showMobileFilters = true">
          <span class="filter-icon">☰</span>
          <span class="filter-text">Filtres</span>
          <span v-if="activeFiltersCount > 0" class="filter-badge">{{ activeFiltersCount }}</span>
        </button>
      </div>

      <div v-if="activeFiltersCount > 0" class="mobile-clear-filters">
        <button class="clear-filters-btn" @click="filters.clearFilters()">
          Effacer les filtres
        </button>
      </div>
    </div>

    <hr />

    <!-- DESKTOP - Tableau -->
    <table v-if="$q.screen.gt.sm">
      <thead>
        <tr>
          <th></th>
          <th>Nom de la carte</th>
          <th>Nom de la série</th>
          <th>Rareté</th>
          <th>Prix</th>
          <th>Prix foil</th>
          <th>Langue</th>
          <th>Stock Foil</th>
          <th>Stock Non-Foil</th>
          <th>Total</th>
          <th>État</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <template v-for="Card in cards" :key="Card.id">
          <template v-if="Card.languageQuantities && Card.languageQuantities.length > 0">
            <tr
              class="info_section"
              v-for="(langQuantity, index) in Card.languageQuantities"
              :key="`${Card.id}-${langQuantity.languageId}`"
            >
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">
                <div class="image-wrapper" @click="goToDetail(Card.id)">
                  <img
                    class="card_image"
                    :src="`https://api.scryfall.com/cards/${Card.scryfallId}?format=image`"
                  />
                </div>
              </td>
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">{{ Card.name }}</td>
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">
                {{ Card.setName }}
              </td>
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">
                {{ Card.rarity }}
              </td>
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">
                {{ Card.price }}$
              </td>
              <td v-if="index === 0" :rowspan="Card.languageQuantities.length">
                {{ Card.foilPrice }}$
              </td>
              <td>{{ TranslationService.translateLanguage(langQuantity.languageName) }}</td>
              <td :class="{ 'stock-changed-warning': langQuantity.stockChangedDuringEdit }">
                <div v-if="Card.hasFoil && langQuantity.isEditing" class="stock-input-wrapper">
                  <q-input
                    v-model.number="langQuantity.editedFoilQuantity"
                    :min="isEmployee ? parseQuantity(langQuantity.foilQuantity) : 0"
                    type="number"
                    class="stock-input"
                    :disabled="langQuantity.foilQuantity === 'N/A'"
                  />
                  <div class="stock-spinner-buttons-bottom">
                    <button
                      type="button"
                      class="spinner-btn spinner-btn-up"
                      @click="adjustQuantity(langQuantity, 'foil', 'up')"
                      :disabled="langQuantity.foilQuantity === 'N/A'"
                    >
                      <q-icon name="keyboard_arrow_up" size="16px" />
                    </button>
                    <button
                      type="button"
                      class="spinner-btn spinner-btn-down"
                      @click="adjustQuantity(langQuantity, 'foil', 'down')"
                      :disabled="
                        langQuantity.foilQuantity === 'N/A' ||
                        (isEmployee &&
                          langQuantity.editedFoilQuantity <=
                            parseQuantity(langQuantity.foilQuantity))
                      "
                    >
                      <q-icon name="keyboard_arrow_down" size="16px" />
                    </button>
                  </div>
                </div>
                <span v-else-if="Card.hasFoil" class="stock-value">{{
                  langQuantity.foilQuantity
                }}</span>
                <span v-else class="stock-value">N/A</span>
              </td>
              <td :class="{ 'stock-changed-warning': langQuantity.stockChangedDuringEdit }">
                <div v-if="Card.hasNonFoil && langQuantity.isEditing" class="stock-input-wrapper">
                  <q-input
                    v-model.number="langQuantity.editedNonFoilQuantity"
                    :min="isEmployee ? parseQuantity(langQuantity.nonFoilQuantity) : 0"
                    type="number"
                    class="stock-input"
                    :disabled="langQuantity.nonFoilQuantity === 'N/A'"
                  />
                  <div class="stock-spinner-buttons-bottom">
                    <button
                      type="button"
                      class="spinner-btn spinner-btn-up"
                      @click="adjustQuantity(langQuantity, 'nonFoil', 'up')"
                      :disabled="langQuantity.nonFoilQuantity === 'N/A'"
                    >
                      <q-icon name="keyboard_arrow_up" size="16px" />
                    </button>
                    <button
                      type="button"
                      class="spinner-btn spinner-btn-down"
                      @click="adjustQuantity(langQuantity, 'nonFoil', 'down')"
                      :disabled="
                        langQuantity.nonFoilQuantity === 'N/A' ||
                        (isEmployee &&
                          langQuantity.editedNonFoilQuantity <=
                            parseQuantity(langQuantity.nonFoilQuantity))
                      "
                    >
                      <q-icon name="keyboard_arrow_down" size="16px" />
                    </button>
                  </div>
                </div>
                <span v-else-if="Card.hasNonFoil" class="stock-value">{{
                  langQuantity.nonFoilQuantity
                }}</span>
                <span v-else class="stock-value">N/A</span>
              </td>
              <td>
                {{
                  (Card.hasFoil ? parseQuantity(langQuantity.foilQuantity) : 0) +
                  (Card.hasNonFoil ? parseQuantity(langQuantity.nonFoilQuantity) : 0)
                }}
              </td>
              <td>
                <span
                  class="material-symbols-outlined"
                  :class="getRowStockInfo(Card, langQuantity).class"
                >
                  capture
                  <q-tooltip>{{ getRowStockInfo(Card, langQuantity).tooltip }}</q-tooltip>
                </span>
              </td>
              <td>
                <q-btn
                  v-if="!langQuantity.isEditing"
                  push
                  color="grey-9"
                  text-color="white"
                  label="Éditer"
                  no-caps
                  size="sm"
                  @click="toggleLanguageEdit(Card, langQuantity)"
                >
                  <q-tooltip>Modifier les quantités pour cette langue</q-tooltip>
                </q-btn>
                <div v-else style="display: flex; flex-direction: column; gap: 10px">
                  <q-btn
                    push
                    color="success"
                    text-color="white"
                    label="Sauvegarder"
                    no-caps
                    size="sm"
                    style="width: 100%"
                    @click="saveQuantity(Card, langQuantity)"
                  >
                    <q-tooltip>Enregistrer les modifications</q-tooltip>
                  </q-btn>
                  <q-btn
                    push
                    color="grey-9"
                    text-color="white"
                    label="Annuler"
                    no-caps
                    size="sm"
                    style="width: 100%"
                    @click="cancelLanguageEdit(langQuantity)"
                  >
                    <q-tooltip>Annuler les modifications</q-tooltip>
                  </q-btn>
                </div>
              </td>
            </tr>
          </template>
          <tr v-else class="info_section">
            <td>
              <div class="image-wrapper">
                <img
                  class="card_image"
                  :src="`https://api.scryfall.com/cards/${Card.scryfallId}?format=image`"
                />
              </div>
            </td>
            <td>{{ Card.name }}</td>
            <td>{{ Card.setName }}</td>
            <td>{{ Card.rarity }}</td>
            <td>{{ Card.price }}$</td>
            <td>{{ Card.foilPrice }}$</td>
            <td>Aucune langue</td>
            <td>0</td>
            <td>0</td>
            <td>0</td>
            <td>
              <span class="material-symbols-outlined" :class="Card.stockClass">
                capture
                <q-tooltip>{{ Card.stockTooltip }}</q-tooltip>
              </span>
            </td>
            <td>
              <span class="text-muted">Aucune quantité</span>
            </td>
          </tr>
        </template>
      </tbody>
    </table>

    <!-- MOBILE - Cards -->
    <div v-else class="mobile-cards-container">
      <div v-for="Card in cards" :key="Card.id" class="mobile-card">
        <div class="mobile-card-header">
          <div class="mobile-card-image-container" @click="goToDetail(Card.id)">
            <img
              :src="`https://api.scryfall.com/cards/${Card.scryfallId}?format=image`"
              class="mobile-card-image"
            />
          </div>
          <div class="mobile-card-info">
            <div class="card-name">{{ Card.name }}</div>
            <div class="card-set">{{ Card.setName }}</div>
            <div class="card-badges">
              <span class="rarity-badge" :class="getRarityClass(Card.rarity)">{{
                Card.rarity
              }}</span>
              <span class="stock-indicator" :class="Card.stockClass">● {{ Card.stockLevel }}</span>
            </div>
            <div class="card-prices">
              <span>Prix: {{ Card.price }}$</span>
              <span class="separator">|</span>
              <span>Foil: {{ Card.foilPrice }}$</span>
            </div>
          </div>
        </div>

        <div class="mobile-card-languages">
          <div v-if="Card.languageQuantities && Card.languageQuantities.length > 0">
            <div
              v-for="(langQuantity, index) in Card.languageQuantities"
              :key="langQuantity.languageId"
              class="language-row"
              :class="{ 'language-row-odd': index % 2 === 0, 'language-row-even': index % 2 !== 0 }"
            >
              <div v-if="!langQuantity.isEditing" class="language-display">
                <div class="language-name">
                  {{ TranslationService.translateLanguage(langQuantity.languageName) }}
                </div>
                <div class="language-quantities">
                  <span v-if="Card.hasFoil"
                    >Foil: <strong>{{ langQuantity.foilQuantity }}</strong></span
                  >
                  <span v-else>Foil: <strong>N/A</strong></span>
                  <span class="separator">|</span>
                  <span v-if="Card.hasNonFoil"
                    >Non-Foil: <strong>{{ langQuantity.nonFoilQuantity }}</strong></span
                  >
                  <span v-else>Non-Foil: <strong>N/A</strong></span>
                  <span class="separator">|</span>
                  <span
                    >Total:
                    <strong>{{
                      (Card.hasFoil ? parseQuantity(langQuantity.foilQuantity) : 0) +
                      (Card.hasNonFoil ? parseQuantity(langQuantity.nonFoilQuantity) : 0)
                    }}</strong></span
                  >
                </div>
                <button class="edit-btn" @click="toggleLanguageEdit(Card, langQuantity)">
                  ÉDITER
                </button>
              </div>
              <div v-else class="language-edit">
                <div class="language-name">
                  {{ TranslationService.translateLanguage(langQuantity.languageName) }}
                </div>
                <div class="edit-inputs">
                  <input
                    v-if="Card.hasFoil"
                    type="number"
                    v-model.number="langQuantity.editedFoilQuantity"
                    placeholder="Foil"
                    class="qty-input"
                  />
                  <input
                    v-else
                    type="text"
                    value="N/A"
                    disabled
                    class="qty-input"
                    style="opacity: 0.6"
                  />
                  <input
                    v-if="Card.hasNonFoil"
                    type="number"
                    v-model.number="langQuantity.editedNonFoilQuantity"
                    placeholder="Non-Foil"
                    class="qty-input"
                  />
                  <input
                    v-else
                    type="text"
                    value="N/A"
                    disabled
                    class="qty-input"
                    style="opacity: 0.6"
                  />
                </div>
                <div class="edit-actions">
                  <button class="save-btn" @click="saveQuantity(Card, langQuantity)">
                    SAUVEGARDER
                  </button>
                  <button class="cancel-btn" @click="cancelLanguageEdit(langQuantity)">
                    ANNULER
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="no-quantity">Aucune quantité disponible</div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="text-center py-5" style="color: white">
      <div
        class="spinner-border"
        role="status"
        style="border-color: rgba(255, 255, 255, 0.3); border-left-color: white"
      >
        <span class="visually-hidden">Chargement...</span>
      </div>
      <p class="mt-3">Chargement des cartes...</p>
    </div>

    <Pagination :currentPage="currentPage" :totalPages="totalPages" @page-change="changePage" />

    <div class="total-quantity-display mt-4 p-3 bg-light rounded">
      <h5 class="mb-0">
        <strong>Quantité totale de l'inventaire : {{ calculatedTotalQuantity }} cartes</strong>
      </h5>
    </div>

    <!-- SIDEBAR FILTRES MOBILE -->
    <div
      v-if="showMobileFilters"
      class="mobile-filters-overlay sidebar-overlay"
      @click="showMobileFilters = false"
    ></div>
    <div
      class="sidebar text-light p-3"
      :class="{ active: showMobileFilters, mobile: $q.screen.lt.md }"
      id="sidebar-filters"
    >
      <!-- Header -->
      <div class="sidebar-header">
        <h5>Filtres</h5>
        <button v-if="$q.screen.lt.md" class="close-btn" @click="showMobileFilters = false">
          ✖
        </button>
      </div>

      <div class="sidebar-content">
        <hr />
        <!-- Filtres actifs -->
        <div class="active-filters">
          <div class="filters-list">
            <template v-for="(vals, key) in activeFilters" :key="key">
              <button
                v-for="val in vals"
                :key="key + val"
                class="filter-btn"
                @click="
                  (event) => {
                    event.preventDefault()
                    event.stopPropagation()
                    filters.removeFilter(key, val)
                  }
                "
              >
                {{ val }}
                <span class="remove">✖</span>
              </button>
            </template>
            <span v-if="Object.values(activeFilters).flat().length === 0" class="no-filter">
              Aucun filtre sélectionné
            </span>
          </div>
          <!-- Bouton réinitialiser  -->
          <button
            v-if="Object.values(activeFilters).flat().length > 0"
            class="reset-btn"
            @click="
              (event) => {
                event.preventDefault()
                event.stopPropagation()
                filters.clearFilters()
              }
            "
          >
            Réinitialiser les filtres
          </button>
        </div>
        <hr />
        <!-- Série -->
        <p class="fw-bold mt-3">Série</p>
        <template v-if="filterOptions['Série'].length >= 8">
          <input
            type="text"
            :value="searchSerie"
            @input="searchSerie = $event.target.value"
            placeholder="Rechercher une série..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in searchSerie ? filteredSerie : filterOptions['Série']"
              :key="opt"
              class="sidebar-checkbox-item"
              :class="{ selected: filters.serie.includes(opt) }"
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
            class="sidebar-checkbox-item"
            :class="{ selected: filters.serie.includes(opt) }"
            @click="toggleFilter('serie', opt)"
          >
            <input type="checkbox" :checked="filters.serie.includes(opt)" />
            {{ opt }}
          </div>
        </template>
        <hr />
        <!-- Rareté -->
        <p class="fw-bold mt-3">Rareté</p>
        <template v-if="filterOptions['Rareté'].length >= 4">
          <input
            type="text"
            :value="searchRarete"
            @input="searchRarete = $event.target.value"
            placeholder="Rechercher une rareté..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in searchRarete ? filteredRarete : filterOptions['Rareté']"
              :key="opt"
              class="sidebar-checkbox-item"
              :class="{ selected: filters.rarete.includes(opt) }"
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
            class="sidebar-checkbox-item"
            :class="{ selected: filters.rarete.includes(opt) }"
            @click="toggleFilter('rarete', opt)"
          >
            <input type="checkbox" :checked="filters.rarete.includes(opt)" />
            {{ opt }}
          </div>
        </template>
        <hr />
        <!-- Type -->
        <p class="fw-bold mt-3">Type de carte</p>
        <template v-if="filterOptions['Type'].length >= 5">
          <input
            type="text"
            :value="searchType"
            @input="searchType = $event.target.value"
            placeholder="Rechercher un type..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in searchType ? filteredType : filterOptions['Type']"
              :key="opt"
              class="sidebar-checkbox-item"
              :class="{ selected: filters.type.includes(opt) }"
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
            class="sidebar-checkbox-item"
            :class="{ selected: filters.type.includes(opt) }"
            @click="toggleFilter('type', opt)"
          >
            <input type="checkbox" :checked="filters.type.includes(opt)" />
            {{ opt }}
          </div>
        </template>
        <hr />
        <!-- Sous-Type  -->
        <template v-if="filters.type.length > 0 && availableSubtypes.length > 0">
          <p class="fw-bold mt-3">Sous-Type</p>
          <template v-if="availableSubtypes.length >= 8">
            <input
              type="text"
              :value="searchSousType"
              @input="searchSousType = $event.target.value"
              placeholder="Rechercher un sous-type..."
              class="form-control form-control-sm mb-2"
            />
            <div class="sidebar-list-container with-scroll">
              <div
                v-for="opt in searchSousType ? filteredSousType : availableSubtypes"
                :key="opt"
                class="sidebar-checkbox-item"
                :class="{ selected: filters.sousType.includes(opt) }"
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
              class="sidebar-checkbox-item"
              :class="{ selected: filters.sousType.includes(opt) }"
              @click="toggleFilter('sousType', opt)"
            >
              <input type="checkbox" :checked="filters.sousType.includes(opt)" />
              {{ opt }}
            </div>
          </template>
          <hr />
        </template>

        <!-- Couleur -->
        <p class="fw-bold mt-3">Couleur</p>
        <template v-if="filterOptions['Couleur'].length >= 4">
          <input
            type="text"
            :value="searchCouleur"
            @input="searchCouleur = $event.target.value"
            placeholder="Rechercher une couleur..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in searchCouleur ? filteredCouleur : filterOptions['Couleur']"
              :key="opt"
              class="sidebar-checkbox-item"
              :class="{ selected: filters.couleur.includes(opt) }"
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
            class="sidebar-checkbox-item"
            :class="{ selected: filters.couleur.includes(opt) }"
            @click="toggleFilter('couleur', opt)"
          >
            <input type="checkbox" :checked="filters.couleur.includes(opt)" />
            {{ opt }}
          </div>
        </template>
        <hr />
        <!-- Langue -->
        <p class="fw-bold mt-3">Langue</p>
        <template v-if="filterOptions['Langue'].length >= 5">
          <input
            type="text"
            :value="searchLangue"
            @input="searchLangue = $event.target.value"
            placeholder="Rechercher une langue..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in searchLangue ? filteredLangue : filterOptions['Langue']"
              :key="opt"
              class="sidebar-checkbox-item"
              :class="{ selected: filters.langue.includes(opt) }"
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
            class="sidebar-checkbox-item"
            :class="{ selected: filters.langue.includes(opt) }"
            @click="toggleFilter('langue', opt)"
          >
            <input type="checkbox" :checked="filters.langue.includes(opt)" />
            {{ opt }}
          </div>
        </template>
      </div>

      <!-- Footer avec boutons -->
      <div class="sidebar-footer">
        <button class="reset-btn" @click="filters.clearFilters()">Effacer les filtres</button>
        <button v-if="$q.screen.lt.md" class="close-btn" @click="showMobileFilters = false">
          Fermer
        </button>
      </div>
    </div>
  </div>
</template>

<style src="../../css/Inventory.css" scoped></style>

<style>
body:not(.body--dark) .inventaire-container {
  background-color: #d1cbbb !important;
  color: black !important;
  min-height: 100vh;
}

body:not(.body--dark) .q-field__control {
  background-color: #efebe9 !important;
  border-radius: 4px !important;
}

body:not(.body--dark) .q-field__control:hover:before {
  border-color: transparent !important;
}

body:not(.body--dark) .q-field__control:before {
  border-color: transparent !important;
}

body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .filters-bar {
  background-color: #a89c8d !important;
  color: black !important;
}

body:not(.body--dark) .filters-bar .dropdown-toggle {
  color: black !important;
}

body:not(.body--dark) .mobile-card {
  border: 2px solid #d1cbbb !important;
}

body:not(.body--dark) .mobile-card-header {
  background-color: #d1cbbb !important;
  color: black !important;
  border-bottom: 2px solid white !important;
}

body:not(.body--dark) .mobile-card-info {
  color: black !important;
}

body:not(.body--dark) .card-set {
  color: #333 !important;
}

body:not(.body--dark) .mobile-card-languages {
  background-color: white !important;
}

body:not(.body--dark) .language-row {
  color: black !important;
  border-bottom: 1px solid #e0e0e0 !important;
}

body:not(.body--dark) .language-row-odd {
  background-color: #f9f9f9 !important;
}

body:not(.body--dark) .language-row-even {
  background-color: white !important;
}

body:not(.body--dark) .qty-input {
  border: 1px solid #ccc !important;
  color: black !important;
  background-color: white !important;
}

body:not(.body--dark) .filters-bar .btn-info {
  color: black !important;
}

body:not(.body--dark) .inventaire-container table tbody tr {
  background-color: #efebe9 !important;
  color: black !important;
}
</style>
