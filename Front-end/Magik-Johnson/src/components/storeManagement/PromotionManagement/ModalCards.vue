<template>
  <q-dialog :model-value="show" @update:model-value="$emit('update:show', $event)" persistent>
    <q-card class="catalog-modal" :style="modalStyle" @click="handleOutsideClick">
      <q-card-section class="catalog-header-section">
        <!-- Version Desktop -->
        <div v-if="$q.screen.gt.md" class="catalog-header row items-center q-gutter-sm">
          <div class="col">
            <q-input
              :model-value="catalogSearch"
              @update:model-value="$emit('update:catalogSearch', $event)"
              placeholder="Rechercher dans le catalogue"
              outlined
              dense
            />
          </div>
          <div class="col-auto">
            <select
              :value="catalogSort"
              @change="$emit('update:catalogSort', $event.target.value)"
              class="form-select form-select-sm"
            >
              <option value="">Trier</option>
              <option value="name-asc">Nom (A-Z)</option>
              <option value="name-desc">Nom (Z-A)</option>
              <option value="price-asc">Prix (asc)</option>
              <option value="price-desc">Prix (desc)</option>
              <option value="rarete-asc">Rareté (asc)</option>
              <option value="rarete-desc">Rareté (desc)</option>
              <option value="stock-asc">Stock (asc)</option>
              <option value="stock-desc">Stock (desc)</option>
            </select>
          </div>
          <div class="col-auto q-mr-sm">
            <button
              class="mobile-filters-btn-promo"
              @click="showFiltersSidebar = !showFiltersSidebar"
            >
              <span class="filter-icon">☰</span>
              <span class="filter-text">Filtres</span>
              <span v-if="activeFiltersCount > 0" class="filter-badge">{{
                activeFiltersCount
              }}</span>
            </button>
          </div>
          <div class="col-auto" v-if="catalogFilters.serie">
            <q-btn
              label="Ajouter toute la série"
              color="green-8"
              unelevated
              @click="$emit('add-series', catalogFilters.serie)"
              class="text-white"
            />
          </div>
          <div class="col-auto">
            <q-btn label="Fermer" color="red-8" unelevated @click="$emit('close')" />
          </div>
        </div>

        <!-- Version Mobile -->
        <div v-else class="catalog-header-mobile">
          <div class="col">
            <q-input
              :model-value="catalogSearch"
              @update:model-value="$emit('update:catalogSearch', $event)"
              placeholder="Rechercher dans le catalogue"
              outlined
              dense
            />
          </div>
          <div class="mobile-header-buttons">
            <div class="mobile-button-left">
              <button
                class="mobile-filters-btn-promo"
                @click="showFiltersSidebar = !showFiltersSidebar"
              >
                <span class="filter-icon">☰</span>
                <span class="filter-text">Filtres</span>
                <span v-if="activeFiltersCount > 0" class="filter-badge">{{
                  activeFiltersCount
                }}</span>
              </button>
            </div>
            <div class="mobile-button-center">
              <select
                :value="catalogSort"
                @change="$emit('update:catalogSort', $event.target.value)"
                class="form-select form-select-sm"
              >
                <option value="">Trier</option>
                <option value="name-asc">Nom (A-Z)</option>
                <option value="name-desc">Nom (Z-A)</option>
                <option value="price-asc">Prix (asc)</option>
                <option value="price-desc">Prix (desc)</option>
                <option value="rarete-asc">Rareté (asc)</option>
                <option value="rarete-desc">Rareté (desc)</option>
                <option value="stock-asc">Stock (asc)</option>
                <option value="stock-desc">Stock (desc)</option>
              </select>
            </div>
            <div class="mobile-button-right">
              <q-btn
                v-if="catalogFilters.serie"
                label="Ajouter série"
                color="green-8"
                size="sm"
                unelevated
                @click="$emit('add-series', catalogFilters.serie)"
                class="text-white q-mr-sm"
              />
              <q-btn label="Fermer" color="red-8" size="sm" unelevated @click="$emit('close')" />
            </div>
          </div>
        </div>

        <!-- fermer le sidebar -->
        <div
          class="filters-overlay"
          :class="{ open: showFiltersSidebar }"
          @click="showFiltersSidebar = false"
        ></div>

        <!-- Sidebar Filtres -->
        <div class="filters-sidebar text-light" :class="{ open: showFiltersSidebar }">
          <div class="sidebar-header">
            <h5>Filtres</h5>
            <button class="sidebar-close-btn" @click="showFiltersSidebar = false">✖</button>
          </div>
          <div class="sidebar-content">
            <hr />
            <!-- Filtres actifs -->
            <div class="active-filters">
              <!-- Liste des filtres en haut du sidebar -->
              <div class="filters-list">
                <template v-for="(vals, key) in activeFilters" :key="key">
                  <button
                    v-for="val in vals"
                    :key="key + val"
                    class="filter-btn"
                    @click.stop="removeFilter(key, val)"
                  >
                    {{ val }}
                    <span class="remove">✖</span>
                  </button>
                </template>
                <span v-if="Object.values(activeFilters).flat().length === 0" class="no-filter">
                  Aucun filtre sélectionné
                </span>
              </div>
              <!-- Bouton réinitialiser tous les filtres -->
              <button
                v-if="Object.values(activeFilters).flat().length > 0"
                class="reset-btn"
                @click.stop="$emit('reset-filters')"
              >
                Réinitialiser les filtres
              </button>
            </div>
            <hr />
            <!-- Couleurs -->
            <div class="filter-section">
              <p class="fw-bold">Couleurs</p>
              <div class="filter-options">
                <div
                  class="sidebar-checkbox-item"
                  :class="{ selected: !catalogFilters.couleur }"
                  @click="handleFilterClick('couleur', '')"
                >
                  <input type="checkbox" :checked="!catalogFilters.couleur" />
                  (toutes)
                </div>
                <div
                  v-for="couleur in filterOptions.couleurs"
                  :key="couleur"
                  class="sidebar-checkbox-item"
                  :class="{ selected: catalogFilters.couleur === couleur }"
                  @click="handleFilterClick('couleur', couleur)"
                >
                  <input type="checkbox" :checked="catalogFilters.couleur === couleur" />
                  {{ couleur }}
                </div>
              </div>
            </div>

            <hr />

            <!-- Raretés -->
            <div class="filter-section">
              <p class="fw-bold">Raretés</p>
              <div class="filter-options">
                <div
                  class="sidebar-checkbox-item"
                  :class="{ selected: !catalogFilters.rarete }"
                  @click="handleFilterClick('rarete', '')"
                >
                  <input type="checkbox" :checked="!catalogFilters.rarete" />
                  (toutes)
                </div>
                <div
                  v-for="rarete in filterOptions.raretes"
                  :key="rarete"
                  class="sidebar-checkbox-item"
                  :class="{ selected: catalogFilters.rarete === rarete }"
                  @click="handleFilterClick('rarete', rarete)"
                >
                  <input type="checkbox" :checked="catalogFilters.rarete === rarete" />
                  {{ rarete }}
                </div>
              </div>
            </div>

            <hr />

            <!-- Prix -->
            <div class="filter-section">
              <p class="fw-bold">Prix</p>
              <div class="filter-inputs-row">
                <input
                  type="number"
                  min="0"
                  :value="catalogFilters.prixMin"
                  @input="updatePrixMin"
                  class="filter-number-input"
                  placeholder="Prix min"
                />
                <input
                  type="number"
                  min="0"
                  :value="catalogFilters.prixMax"
                  @input="updatePrixMax"
                  class="filter-number-input"
                  placeholder="Prix max"
                />
              </div>
            </div>

            <hr />

            <!-- Disponibilités -->
            <div class="filter-section">
              <p class="fw-bold">Disponibilités</p>
              <div class="filter-options">
                <div
                  class="sidebar-checkbox-item"
                  :class="{ selected: !catalogFilters.disponibilite }"
                  @click="handleFilterClick('disponibilite', '')"
                >
                  <input type="checkbox" :checked="!catalogFilters.disponibilite" />
                  (toutes)
                </div>
                <div
                  v-for="disponibilite in filterOptions.disponibilites"
                  :key="disponibilite"
                  class="sidebar-checkbox-item"
                  :class="{ selected: catalogFilters.disponibilite === disponibilite }"
                  @click="handleFilterClick('disponibilite', disponibilite)"
                >
                  <input
                    type="checkbox"
                    :checked="catalogFilters.disponibilite === disponibilite"
                  />
                  {{ disponibilite }}
                </div>
              </div>
            </div>

            <hr />

            <!-- Langues -->
            <div class="filter-section">
              <p class="fw-bold">Langues</p>
              <template v-if="filterOptions.langues.length >= 8">
                <div class="filter-search">
                  <input
                    type="text"
                    :value="catalogSearchLangue"
                    @input="$emit('update:searchLangue', $event.target.value)"
                    placeholder="Rechercher une langue..."
                    class="filter-search-input"
                    @click.stop
                  />
                </div>
                <div class="filter-list-container">
                  <div
                    class="sidebar-checkbox-item"
                    :class="{ selected: !catalogFilters.langue }"
                    @click="handleFilterClick('langue', '')"
                  >
                    <input type="checkbox" :checked="!catalogFilters.langue" />
                    (toutes)
                  </div>
                  <div
                    v-for="langue in filteredCatalogLangues"
                    :key="langue"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.langue === langue }"
                    @click="handleFilterClick('langue', langue)"
                  >
                    <input type="checkbox" :checked="catalogFilters.langue === langue" />
                    {{ langue }}
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="filter-options">
                  <div
                    class="sidebar-checkbox-item"
                    :class="{ selected: !catalogFilters.langue }"
                    @click="handleFilterClick('langue', '')"
                  >
                    <input type="checkbox" :checked="!catalogFilters.langue" />
                    (toutes)
                  </div>
                  <div
                    v-for="langue in filterOptions.langues"
                    :key="langue"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.langue === langue }"
                    @click="handleFilterClick('langue', langue)"
                  >
                    <input type="checkbox" :checked="catalogFilters.langue === langue" />
                    {{ langue }}
                  </div>
                </div>
              </template>
            </div>

            <hr />

            <!-- Types -->
            <div class="filter-section">
              <p class="fw-bold">Types</p>
              <template v-if="filterOptions.types.length >= 8">
                <div class="filter-search">
                  <input
                    type="text"
                    :value="catalogSearchType"
                    @input="$emit('update:searchType', $event.target.value)"
                    placeholder="Rechercher un type..."
                    class="filter-search-input"
                    @click.stop
                  />
                </div>
                <div class="filter-list-container">
                  <div
                    class="sidebar-checkbox-item"
                    :class="{ selected: !catalogFilters.type }"
                    @click="handleFilterClick('type', '')"
                  >
                    <input type="checkbox" :checked="!catalogFilters.type" />
                    (tous)
                  </div>
                  <div
                    v-for="type in filteredCatalogTypes"
                    :key="type"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.type === type }"
                    @click="handleFilterClick('type', type)"
                  >
                    <input type="checkbox" :checked="catalogFilters.type === type" />
                    {{ type }}
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="filter-options">
                  <div
                    class="sidebar-checkbox-item"
                    :class="{ selected: !catalogFilters.type }"
                    @click="handleFilterClick('type', '')"
                  >
                    <input type="checkbox" :checked="!catalogFilters.type" />
                    (tous)
                  </div>
                  <div
                    v-for="type in filterOptions.types"
                    :key="type"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.type === type }"
                    @click="handleFilterClick('type', type)"
                  >
                    <input type="checkbox" :checked="catalogFilters.type === type" />
                    {{ type }}
                  </div>
                </div>
              </template>
            </div>

            <hr v-if="catalogFilters.type && availableSubtypes.length > 0" />

            <!-- Sous-types -->
            <div v-if="catalogFilters.type && availableSubtypes.length > 0" class="filter-section">
              <p class="fw-bold">Sous-types</p>
              <div class="filter-options">
                <div
                  class="sidebar-checkbox-item"
                  :class="{ selected: !catalogFilters.sousType }"
                  @click="handleFilterClick('sousType', '')"
                >
                  <input type="checkbox" :checked="!catalogFilters.sousType" />
                  (tous)
                </div>
                <div
                  v-for="sousType in availableSubtypes"
                  :key="sousType"
                  class="sidebar-checkbox-item"
                  :class="{ selected: catalogFilters.sousType === sousType }"
                  @click="handleFilterClick('sousType', sousType)"
                >
                  <input type="checkbox" :checked="catalogFilters.sousType === sousType" />
                  {{ sousType }}
                </div>
              </div>
            </div>

            <hr />

            <!-- Séries -->
            <div class="filter-section">
              <p class="fw-bold">Série</p>
              <template v-if="catalogSeriesOptions.length >= 8">
                <div class="filter-search">
                  <input
                    type="text"
                    :value="catalogSearchSerie"
                    @input="$emit('update:searchSerie', $event.target.value)"
                    placeholder="Rechercher une série..."
                    class="filter-search-input"
                    @click.stop
                  />
                </div>
                <div class="filter-list-container">
                  <div
                    v-for="opt in filteredCatalogSeries"
                    :key="opt"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.serie === opt }"
                    @click="handleFilterClick('serie', opt)"
                  >
                    <input type="checkbox" :checked="catalogFilters.serie === opt" />
                    {{ opt }}
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="filter-options">
                  <div
                    v-for="opt in catalogSeriesOptions"
                    :key="opt"
                    class="sidebar-checkbox-item"
                    :class="{ selected: catalogFilters.serie === opt }"
                    @click="handleFilterClick('serie', opt)"
                  >
                    <input type="checkbox" :checked="catalogFilters.serie === opt" />
                    {{ opt }}
                  </div>
                </div>
              </template>
            </div>
          </div>
          <div class="sidebar-footer">
            <q-btn
              label="Fermer"
              color="grey-7"
              unelevated
              @click="showFiltersSidebar = false"
              class="close-sidebar-btn"
            />
          </div>
        </div>
      </q-card-section>

      <q-separator />

      <q-card-section class="catalog-body">
        <div v-if="loading" class="text-center q-pa-lg">
          <q-spinner-dots size="40px" color="primary" />
          <div class="q-mt-sm">Chargement du catalogue...</div>
        </div>

        <div
          v-else-if="filteredCatalog.length === 0"
          class="text-center q-pa-lg bg-grey-2 rounded-borders"
        >
          <p class="text-grey-7">Aucune carte trouvée ou toutes les cartes sont déjà associées.</p>
        </div>

        <div v-else class="catalog-grid">
          <div v-for="card in catalogPaginated" :key="card.id" class="catalog-card mtg-card">
            <div class="image-wrapper" :class="{ 'foil-card': isFoilOnly(card) }">
              <img
                v-if="card.image"
                :src="card.image"
                :alt="card.nom"
                class="mtg-card-img"
                @error="handleImageError"
              />
              <div v-if="isFoilOnly(card)" class="foil-overlay"></div>
            </div>
            <div class="mtg-card-body">
              <h6 class="mtg-card-title">{{ card.nom }}</h6>
              <p class="mtg-card-serie">{{ card.serie }}, {{ card.rarete }}, #{{ card.numero }}</p>
              <div class="mtg-card-price">
                <span class="mtg-card-new">{{ formatPrice(card.prix) }}</span>
                <span v-if="card.hasFoil" class="mtg-card-foil">
                  (Foil: {{ formatPrice(card.prixFoil) }})
                </span>
              </div>
              <div class="mtg-card-actions">
                <q-btn
                  size="sm"
                  :color="getButtonColor(card.id)"
                  :label="getButtonLabel(card.id)"
                  :disable="isButtonDisabled(card.id)"
                  :class="getButtonClass(card.id)"
                  unelevated
                  @click="$emit('add-card', card)"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>

      <q-card-actions align="center" class="q-pt-none">
        <div v-if="totalCatalogPages > 1" class="pagination-container q-mt-sm">
          <nav class="custom-pagination">
            <ul class="pagination-list">
              <li class="pagination-item">
                <button
                  class="pagination-btn prev-btn"
                  :disabled="catalogPage === 1"
                  @click="$emit('change-page', catalogPage - 1)"
                >
                  ‹
                </button>
              </li>
              <li
                v-for="page in visiblePages"
                :key="page === '...' ? `ellipsis-${page}` : page"
                class="pagination-item"
              >
                <button
                  v-if="page !== '...'"
                  class="pagination-btn page-btn"
                  :class="{ active: catalogPage === page }"
                  @click="$emit('change-page', page)"
                >
                  {{ page }}
                </button>
                <span v-else class="pagination-ellipsis">...</span>
              </li>
              <li class="pagination-item">
                <button
                  class="pagination-btn next-btn"
                  :disabled="catalogPage === totalCatalogPages"
                  @click="$emit('change-page', catalogPage + 1)"
                >
                  ›
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const showFiltersSidebar = ref(false)
const openDropdown = ref(null)
const dropdownRefs = ref({})
const dropdownPositions = ref({})

const activeFiltersCount = computed(() => {
  let count = 0
  if (props.catalogFilters.couleur) count++
  if (props.catalogFilters.rarete) count++
  if (props.catalogFilters.disponibilite) count++
  if (props.catalogFilters.langue) count++
  if (props.catalogFilters.type) count++
  if (props.catalogFilters.sousType) count++
  if (props.catalogFilters.serie) count++
  if (props.catalogFilters.prixMin || props.catalogFilters.prixMax) count++
  return count
})

const activeFilters = computed(() => {
  const filters = {}
  if (props.catalogFilters.couleur) {
    if (!filters['Couleur']) filters['Couleur'] = []
    filters['Couleur'].push(props.catalogFilters.couleur)
  }
  if (props.catalogFilters.rarete) {
    if (!filters['Rareté']) filters['Rareté'] = []
    filters['Rareté'].push(props.catalogFilters.rarete)
  }
  if (props.catalogFilters.disponibilite) {
    if (!filters['Disponibilité']) filters['Disponibilité'] = []
    filters['Disponibilité'].push(props.catalogFilters.disponibilite)
  }
  if (props.catalogFilters.langue) {
    if (!filters['Langue']) filters['Langue'] = []
    filters['Langue'].push(props.catalogFilters.langue)
  }
  if (props.catalogFilters.type) {
    if (!filters['Type']) filters['Type'] = []
    filters['Type'].push(props.catalogFilters.type)
  }
  if (props.catalogFilters.sousType) {
    if (!filters['Sous-Type']) filters['Sous-Type'] = []
    filters['Sous-Type'].push(props.catalogFilters.sousType)
  }
  if (props.catalogFilters.serie) {
    if (!filters['Série']) filters['Série'] = []
    filters['Série'].push(props.catalogFilters.serie)
  }
  if (props.catalogFilters.prixMin || props.catalogFilters.prixMax) {
    const priceLabel = []
    if (props.catalogFilters.prixMin) priceLabel.push(`Min: $${props.catalogFilters.prixMin}`)
    if (props.catalogFilters.prixMax) priceLabel.push(`Max: $${props.catalogFilters.prixMax}`)
    filters['Prix'] = [priceLabel.join(' - ')]
  }
  return filters
})

const removeFilter = (key, val) => {
  const newFilters = { ...props.catalogFilters }
  switch (key) {
    case 'Couleur':
      newFilters.couleur = ''
      break
    case 'Rareté':
      newFilters.rarete = ''
      break
    case 'Disponibilité':
      newFilters.disponibilite = ''
      break
    case 'Langue':
      newFilters.langue = ''
      break
    case 'Type':
      newFilters.type = ''
      newFilters.sousType = ''
      break
    case 'Sous-Type':
      newFilters.sousType = ''
      break
    case 'Série':
      newFilters.serie = ''
      break
    case 'Prix':
      newFilters.prixMin = null
      newFilters.prixMax = null
      break
  }
  emit('update:filters', newFilters)
}

const modalStyle = computed(() => {
  if ($q.screen.lt.lg) {
    return {
      width: '100vw',
      maxWidth: '100vw',
      minWidth: '100vw',
      height: '100vh',
      maxHeight: '100vh',
    }
  }
  return {
    width: '95vw',
    maxWidth: '1400px',
    minWidth: '1200px',
    height: '88vh',
  }
})

const updateDropdownPosition = (dropdownName) => {
  const dropdownEl = dropdownRefs.value[dropdownName]
  if (dropdownEl) {
    const buttonEl = dropdownEl.querySelector('.dropdown-toggle')
    if (buttonEl) {
      const rect = buttonEl.getBoundingClientRect()
      dropdownPositions.value[dropdownName] = {
        top: rect.bottom + window.scrollY,
        left: rect.left + window.scrollX,
        width: rect.width,
      }
    }
  }
}

const toggleDropdown = (dropdownName, event) => {
  if (openDropdown.value === dropdownName) {
    openDropdown.value = null
  } else {
    openDropdown.value = dropdownName

    setTimeout(() => {
      updateDropdownPosition(dropdownName)
    }, 0)
  }
}

const getDropdownStyle = (dropdownName) => {
  if (openDropdown.value === dropdownName && dropdownPositions.value[dropdownName]) {
    const pos = dropdownPositions.value[dropdownName]
    return {
      position: 'fixed',
      top: `${pos.top}px`,
      left: `${pos.left}px`,
      width: `${pos.width}px`,
      minWidth: '10rem',
    }
  }
  return {}
}

const handleScroll = () => {
  if (openDropdown.value) {
    updateDropdownPosition(openDropdown.value)
  }
}

const handleResize = () => {
  if (openDropdown.value) {
    updateDropdownPosition(openDropdown.value)
  }
}

onMounted(() => {
  const filtersContainer = document.querySelector('.catalog-filters-container')
  if (filtersContainer) {
    filtersContainer.addEventListener('scroll', handleScroll)
  }
  window.addEventListener('scroll', handleScroll, true)
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  const filtersContainer = document.querySelector('.catalog-filters-container')
  if (filtersContainer) {
    filtersContainer.removeEventListener('scroll', handleScroll)
  }
  window.removeEventListener('scroll', handleScroll, true)
  window.removeEventListener('resize', handleResize)
})

watch(openDropdown, (newVal) => {
  if (newVal) {
    setTimeout(() => {
      updateDropdownPosition(newVal)
    }, 0)
  }
})

const closeDropdown = () => {
  openDropdown.value = null
}

const handleOutsideClick = () => {
  if (openDropdown.value) {
    openDropdown.value = null
  }
}

const props = defineProps({
  show: Boolean,
  catalogSearch: String,
  catalogSort: String,
  catalogFilters: Object,
  catalogSearchSerie: String,
  catalogSeriesOptions: Array,
  filteredCatalogSeries: Array,
  catalogSearchLangue: String,
  filteredCatalogLangues: Array,
  catalogSearchType: String,
  filteredCatalogTypes: Array,
  filteredCatalog: Array,
  catalogPaginated: Array,
  catalogPage: Number,
  totalCatalogPages: Number,
  loading: Boolean,
  filterOptions: Object,
  availableSubtypes: Array,
  loadingSubtypes: Boolean,
  addedCards: {
    type: Array,
    default: () => [],
  },
  isCardInAnotherPromotion: {
    type: Function,
    default: () => false,
  },
})

const emit = defineEmits([
  'update:show',
  'update:catalogSearch',
  'update:catalogSort',
  'update:filters',
  'update:searchSerie',
  'update:searchLangue',
  'update:searchType',
  'close',
  'add-card',
  'add-series',
  'change-page',
  'reset-filters',
])

const handleFilterClick = (filterName, value) => {
  const currentValue = props.catalogFilters[filterName]
  if (currentValue === value) {
    emit('update:filters', { ...props.catalogFilters, [filterName]: '' })
  } else {
    emit('update:filters', { ...props.catalogFilters, [filterName]: value })
  }
  closeDropdown()
}

const updatePrixMin = (event) => {
  emit('update:filters', { ...props.catalogFilters, prixMin: event.target.value })
}

const updatePrixMax = (event) => {
  emit('update:filters', { ...props.catalogFilters, prixMax: event.target.value })
}

const handleImageError = (event) => {
  event.target.style.display = 'none'
}

const isFoilOnly = (card) => {
  return (
    card.hasFoil &&
    (card.prix === null || card.prix === undefined || card.prix === 0 || Number(card.prix) === 0)
  )
}

const formatPrice = (price) => {
  if (price === null || price === undefined || isNaN(price)) {
    return 'N/A'
  }
  return `${Number(price).toFixed(2)} $`
}

const getButtonColor = (cardId) => {
  if (props.addedCards.includes(cardId)) {
    return 'grey-6'
  }
  if (props.isCardInAnotherPromotion(cardId)) {
    return 'orange'
  }
  return 'green-8'
}

const getButtonLabel = (cardId) => {
  if (props.addedCards.includes(cardId)) {
    return 'Déjà ajouté'
  }
  if (props.isCardInAnotherPromotion(cardId)) {
    return 'Déjà dans une autre promotion'
  }
  return 'Ajouter'
}

const isButtonDisabled = (cardId) => {
  return props.addedCards.includes(cardId)
}

const getButtonClass = (cardId) => {
  if (props.isCardInAnotherPromotion(cardId)) {
    return 'btn-long-orange'
  }
  return ''
}

// Pagination
const visiblePages = computed(() => {
  const total = props.totalCatalogPages
  const current = props.catalogPage
  const delta = 2
  const pages = []

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    pages.push(1)

    let start = Math.max(2, current - delta)
    let end = Math.min(total - 1, current + delta)

    if (current <= delta + 2) {
      end = Math.min(total - 1, 5)
    }

    if (current >= total - delta - 1) {
      start = Math.max(2, total - 4)
    }

    if (start > 2) {
      pages.push('...')
    }

    for (let i = start; i <= end; i++) {
      pages.push(i)
    }

    if (end < total - 1) {
      pages.push('...')
    }

    pages.push(total)
  }

  return pages
})
</script>

<style scoped>
.catalog-modal {
  display: flex;
  flex-direction: column;
  background-color: #f5eded !important;
  border-radius: 8px;
  color: #000000 !important;
  overflow: hidden;
}

body.body--dark .catalog-modal {
  background-color: rgb(35, 42, 56) !important;
  color: #ffffff !important;
}

:deep(.q-dialog__backdrop) {
  background-color: rgba(0, 0, 0, 0.6) !important;
}

.catalog-header-section {
  flex: 0 0 auto;
  position: relative;
  z-index: 10;
  overflow: hidden;
  padding: 8px 12px !important;
}

.catalog-header-section :deep(.q-card__section) {
  padding: 0 !important;
}

.catalog-body {
  flex: 1 1 auto;
  overflow-y: auto;
  overflow-x: hidden;
  padding-top: 12px;
  padding-bottom: 12px;

  scrollbar-width: thin;
  scrollbar-color: #4a5568 #2d3748;
  position: relative;
  z-index: 1;
}

.catalog-body::-webkit-scrollbar {
  width: 6px;
}

.catalog-body::-webkit-scrollbar-track {
  background: #2d3748;
  border-radius: 3px;
}

.catalog-body::-webkit-scrollbar-thumb {
  background: #4a5568;
  border-radius: 3px;
}

.catalog-body::-webkit-scrollbar-thumb:hover {
  background: #718096;
}

.catalog-modal .q-card-actions {
  flex: 0 0 auto;
  overflow: hidden;
}

.catalog-header {
  background: #a89c8d !important;
  padding: 8px 10px;
  border-radius: 6px;
  color: #000000 !important;
  overflow: hidden;
  margin-bottom: 6px;
}

body.body--dark .catalog-header {
  background: linear-gradient(to bottom, rgb(35, 42, 56), #707f8b) !important;
  color: #ffffff !important;
}

.catalog-header :deep(.q-field__control) {
  background: #f0eada;
  border-radius: 4px;
}

.catalog-filters {
  display: flex;
  gap: 8px;
  align-items: center;
}

.catalog-filters .form-select {
  min-width: 140px;
}

.catalog-filters .form-control {
  width: 100px;
}

.catalog-card {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  width: 100%;
  box-sizing: border-box;
  position: relative;
  z-index: 1;
}

.catalog-card-title {
  color: #0d3f91;
}

.catalog-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 18px;
  padding: 0 8px;
}

.catalog-card {
  background: #ffffff;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  padding: 12px;
  display: flex;
  gap: 12px;
  align-items: flex-start;
  min-height: 120px;
}

.catalog-card-image {
  width: 84px;
  height: 118px;
  object-fit: cover;
  border-radius: 6px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

/* Styles pour l'effet foil */
.image-wrapper {
  position: relative;
  width: 110px;
  height: 154px;
  flex-shrink: 0;
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #d1c0a8;
  transition: transform 0.2s ease;
  z-index: 1;
}

.image-wrapper:hover {
  transform: scale(1.05);
}

.mtg-card-img {
  width: 100%;
  height: 100%;
  border-radius: 4px;
  object-fit: cover;
  position: relative;
  z-index: 1;
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
  animation: foilShine 4s ease infinite;
  pointer-events: none;
  z-index: 2;
  mix-blend-mode: overlay;
  opacity: 0.7;
}

.foil-card {
  box-shadow: 0 0 15px rgba(156, 39, 176, 0.5);
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

.catalog-card-body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
}

.mtg-card-title {
  font-weight: 600;
  color: #333;
  margin: 0;
  padding: 0;
  line-height: 1.2;
}

.mtg-card-serie {
  font-size: 12px;
  color: #666;
  margin: 0;
  padding: 0;
}

.mtg-card-price {
  margin-top: auto;
}

.mtg-card-new {
  font-weight: 700;
  color: #0d3f91;
}

.mtg-card-foil {
  font-weight: 600;
  color: #5c6bc0;
  margin-left: 8px;
}

.mtg-card-actions {
  margin-top: auto;
  display: flex;
  justify-content: flex-end;
}

/* Tous les boutons ont la même taille */
.mtg-card-actions .q-btn {
  min-width: 200px !important;
  width: 100% !important;
  font-weight: 600 !important;
  border-radius: 6px !important;
  padding: 8px 16px !important;
  font-size: 12px !important;
  text-align: center !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
}

/* Bouton pour les cartes dans une autre promotion */
.btn-long-orange {
  background-color: #ff9800 !important;
  color: white !important;
}

.btn-long-orange:hover {
  background-color: #f57c00 !important;
}

.btn-long-orange:disabled {
  background-color: #ffb74d !important;
  opacity: 0.7 !important;
  cursor: not-allowed !important;
}

.btn-long-orange:not(:disabled) {
  cursor: pointer !important;
  opacity: 1 !important;
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

.pagination-btn {
  padding: 8px 12px;
  border: 1px solid #dee2e6;
  background: white;
  color: #007bff;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;
  min-width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pagination-btn:hover:not(:disabled) {
  background: #007bff;
  color: white;
}

.pagination-btn.active {
  background: #8d6e63;
  color: white;
  border-color: #8d6e63;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-ellipsis {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 8px;
  color: #dee2e6;
  font-weight: bold;
}

.form-select {
  background: white;
  cursor: pointer;
}

.catalog-filters-container {
  width: 100%;
  overflow-x: auto;
  padding-bottom: 4px;
  margin-bottom: 0;
  margin-top: 4px;
  scrollbar-width: thin;
  scrollbar-color: #4a5568 #2d3748;
  position: relative;
  z-index: 5;
}

.catalog-filters-container::-webkit-scrollbar {
  height: 6px;
}

.catalog-filters-container::-webkit-scrollbar-track {
  background: #2d3748;
  border-radius: 3px;
}

.catalog-filters-container::-webkit-scrollbar-thumb {
  background: #4a5568;
  border-radius: 3px;
}

.catalog-filters-container::-webkit-scrollbar-thumb:hover {
  background: #718096;
}

.catalog-filters {
  display: flex;
  flex-wrap: nowrap;
  gap: 8px;
  min-width: max-content;
  position: relative;
  z-index: 1;
}

.filter-item {
  flex-shrink: 0;
  position: relative;
  z-index: 1;
}

.filter-item.ml-auto {
  margin-left: auto;
  display: flex;
  align-items: center;
}

.filter-item.ml-auto .q-btn {
  height: 32px;
  min-height: 32px;
  padding: 0 12px;
}

.filter-item .form-select,
.filter-item .form-control {
  min-width: 90px;
  max-width: 110px;
  padding: 0.2rem 0.4rem;
  font-size: 0.75rem;
}

.filter-item .dropdown {
  min-width: 90px;
}

.filter-item .btn {
  padding: 0.2rem 0.4rem;
  font-size: 0.75rem;
}

.catalog-dropdown {
  position: relative;
  z-index: 10;
}

.catalog-dropdown.show {
  z-index: 100;
}

.catalog-dropdown .dropdown-toggle {
  position: relative;
  z-index: 5;
  cursor: pointer;
}

.catalog-dropdown .dropdown-menu {
  position: absolute !important;
  top: 100% !important;
  left: 0 !important;
  z-index: 1000 !important;
  display: none;
  float: none;
  min-width: 10rem;
  padding: 0.5rem 0;
  margin: 0.125rem 0 0;
  font-size: 1rem;
  color: #212529;
  text-align: left;
  list-style: none;
  background-color: #fff;
  background-clip: padding-box;
  border: 1px solid rgba(0, 0, 0, 0.15);
  border-radius: 0.25rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.175);
  will-change: transform;
}

.catalog-dropdown.show .dropdown-menu {
  display: block;
  z-index: 9999 !important;
}

.catalog-dropdown .dropdown-item,
.catalog-dropdown .dropdown-checkbox-item {
  display: flex;
  align-items: flex-start;
  cursor: pointer;
  transition: background-color 0.2s ease;
  border-radius: 4px;
  padding: 0.4rem 0.2rem;
  word-break: break-word;
  color: #fff;
}

.catalog-dropdown .dropdown-item:hover,
.catalog-dropdown .dropdown-checkbox-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox'] {
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
  margin-right: 1rem;
  margin-top: 2px;
  flex-shrink: 0;
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox']:hover {
  background-color: #3b4456;
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox']:checked {
  background-color: #1e40af;
  border-color: #1e40af;
  animation: pop 0.2s ease forwards;
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox']:checked::after {
  content: '✓';
  color: #fff;
  font-size: 14px;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: pop-check 0.2s ease forwards;
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox']:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.catalog-dropdown .dropdown-checkbox-item input[type='checkbox']:disabled:hover {
  background-color: #232a38;
}

.catalog-dropdown .dropdown-search {
  padding: 0.5rem;
  position: sticky;
  top: 0;
  width: 100%;
  min-width: 300px;
  box-sizing: border-box;
  background-color: #2d3748;
  z-index: 1;
}

.catalog-dropdown .dropdown-search input {
  background-color: #1f2735;
  color: #fff;
  border: 1px solid #fff;
  border-radius: 4px;
  padding: 0.25rem 0.5rem;
  width: 100%;
  min-width: 280px;
  box-sizing: border-box;
}

.catalog-dropdown .dropdown-search input:focus {
  outline: none;
  border-color: #1e40af;
  box-shadow: 0 0 0 0.2rem rgba(30, 64, 175, 0.25);
}

.catalog-dropdown .dropdown-list-container {
  max-height: 260px;
  overflow-y: auto;
  padding-right: 6px;

  scrollbar-width: thin;
  scrollbar-color: #4a5568 #2d3748;
}

.catalog-dropdown .dropdown-list-container::-webkit-scrollbar {
  width: 6px;
}

.catalog-dropdown .dropdown-list-container::-webkit-scrollbar-track {
  background: #2d3748;
  border-radius: 3px;
}

.catalog-dropdown .dropdown-list-container::-webkit-scrollbar-thumb {
  background: #4a5568;
  border-radius: 3px;
}

.catalog-dropdown .dropdown-list-container::-webkit-scrollbar-thumb:hover {
  background: #718096;
}

.catalog-dropdown.show .dropdown-menu {
  position: fixed !important;
  top: auto !important;
  left: auto !important;
  transform: none !important;
}

.catalog-header .col-auto {
  display: flex;
  align-items: center;
}

.catalog-header .q-btn {
  margin-left: 8px;
}

.catalog-modal .dropdown-menu {
  z-index: 1050 !important;
}

/* Styles pour le sidebar de filtres */
.filters-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 9998;
  opacity: 0;
  transition: opacity 0.15s ease;
  pointer-events: none;
}

.filters-overlay.open {
  opacity: 1;
  pointer-events: all;
}

.filters-sidebar {
  background-color: #1f2735;
  position: fixed;
  top: 0;
  left: 0;
  width: 320px;
  height: 100vh;
  transform: translate3d(-100%, 0, 0);
  transition: transform 0.15s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 9999;
  font-family: 'Lucida Console', monospace;
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-width: thin;
  scrollbar-color: #888 transparent;
  padding: 1.5rem;
  box-shadow: 2px 0 10px rgba(0, 0, 0, 0.5);
  will-change: transform;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  display: flex;
  flex-direction: column;
}

.filters-sidebar::-webkit-scrollbar {
  width: 10px;
}

.filters-sidebar::-webkit-scrollbar-thumb {
  background-color: #888;
  border-radius: 6px;
  border: 2px solid transparent;
}

.filters-sidebar::-webkit-scrollbar-thumb:hover {
  background-color: #555;
}

.filters-sidebar.open {
  transform: translate3d(0, 0, 0);
}

.filters-sidebar .sidebar-header {
  position: sticky;
  top: 0;
  background-color: #1f2735;
  z-index: 100;
  padding: 1rem 1.5rem;
  padding-bottom: 0.5rem;
  margin: -1.5rem -1.5rem 0 -1.5rem;
  margin-bottom: 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.filters-sidebar .sidebar-header h5 {
  margin: 0;
  padding: 0.5rem 0;
  color: #f8f9fa;
}

.filters-sidebar .sidebar-title {
  margin: 0;
  padding: 0.5rem 0;
  color: #f8f9fa;
  font-weight: bold;
  font-size: 1.1rem;
}

.filters-sidebar .sidebar-close-btn {
  background: none;
  border: none;
  color: white;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
}

.filters-sidebar .sidebar-content {
  padding: 0;
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Filtres actifs */
.filters-sidebar .active-filters {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  background-color: #1f2735;
  padding: 1.2rem;
  border-radius: 8px;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.filters-sidebar .active-filters .filters-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.filters-sidebar .active-filters .filter-btn {
  background-color: #2d3748;
  color: #fff;
  border: 1px solid #fff;
  border-radius: 6px;
  padding: 0.4rem 0.8rem;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  transition: background 0.2s;
  cursor: pointer;
}

.filters-sidebar .active-filters .filter-btn:hover {
  background-color: #4a5568;
}

.filters-sidebar .active-filters .filter-btn .remove {
  margin-left: 0.4rem;
  color: #e53e3e;
  cursor: pointer;
}

.filters-sidebar .active-filters .reset-btn {
  background-color: #e53e3e;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 0.4rem 0.8rem;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.2s;
}

.filters-sidebar .active-filters .reset-btn:hover {
  background-color: #c53030;
}

.filters-sidebar .active-filters .no-filter {
  color: #a0aec0;
  font-style: italic;
  font-size: 0.9rem;
}

.filters-sidebar .sidebar-content .filter-section {
  margin-bottom: 0.8rem;
}

.filters-sidebar .sidebar-content .filter-section:last-child {
  margin-bottom: 0;
}

.filters-sidebar .fw-bold {
  font-weight: bold;
  font-size: 1.1rem;
  margin-bottom: 1rem;
  color: #f8f9fa;
}

.filters-sidebar .filter-options {
  display: flex;
  flex-direction: column;
  gap: 0;
}

/* Styles pour sidebar-checkbox-item\*/
.filters-sidebar .sidebar-checkbox-item {
  display: flex;
  align-items: center;
  padding: 0.5rem;
  margin-bottom: 0;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
  position: relative;
  color: #f8f9fa;
}

.filters-sidebar .filter-section select,
.filters-sidebar .filter-options .sidebar-checkbox-item {
  cursor: pointer;
}

.filters-sidebar .sidebar-checkbox-item:hover {
  background-color: #3b4456;
  transform: translateX(4px);
}

.filters-sidebar .sidebar-checkbox-item:active {
  background-color: #4a5568;
  transform: translateX(2px);
}

/* Barre orange à gauche */
.filters-sidebar .sidebar-checkbox-item::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: linear-gradient(45deg, #ff6b35, #f7971e);
  border-radius: 0 2px 2px 0;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.filters-sidebar .sidebar-checkbox-item:hover::before {
  opacity: 1;
}

/* Style éléments sélectionnés */
.filters-sidebar .sidebar-checkbox-item.selected {
  background-color: rgba(255, 107, 53, 0.1);
  border-left: 3px solid #ff6b35;
}

.filters-sidebar .sidebar-checkbox-item.selected::before {
  opacity: 1;
}

/*  */
.filters-sidebar .sidebar-checkbox-item input[type='checkbox'] {
  appearance: none;
  -webkit-appearance: none;
  width: 26px;
  height: 26px;
  background-color: #232a38;
  border: 2px solid #000;
  border-radius: 5px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  margin-right: 1rem;
  vertical-align: middle;
  transform: scale(1);
  pointer-events: none;
}

.filters-sidebar .sidebar-checkbox-item input[type='checkbox']:hover {
  background-color: #3b4456;
}

.filters-sidebar .sidebar-checkbox-item input[type='checkbox']:checked {
  background-color: #1e40af;
  border-color: #1e40af;
  animation: pop 0.2s ease forwards;
}

.filters-sidebar .sidebar-checkbox-item input[type='checkbox']:checked::after {
  content: '✓';
  color: #fff;
  font-size: 16px;
  position: absolute;
  top: 2px;
  left: 5px;
  transform: scale(0);
  animation: pop-check 0.2s ease forwards;
}

.filters-sidebar .checkbox-text {
  flex: 1;
  font-size: 0.9rem;
  line-height: 1.4;
  word-wrap: break-word;
  color: #f8f9fa;
}

.filters-sidebar .filter-inputs-row {
  display: flex;
  gap: 8px;
  width: 100%;
  box-sizing: border-box;
}

.filters-sidebar .filter-number-input {
  flex: 1;
  min-width: 0;
  max-width: 50%;
  padding: 0.25rem 0.5rem;
  background-color: #2d3748;
  border: 1px solid #4a5568;
  color: #fff;
  font-size: 0.9rem;
  border-radius: 4px;
  margin-bottom: 0.5rem;
  width: 100%;
  box-sizing: border-box;
}

.filters-sidebar .filter-number-input:focus {
  border-color: #63b3ed;
  box-shadow: 0 0 0 0.2rem rgba(99, 179, 237, 0.25);
  outline: none;
}

.filters-sidebar .filter-number-input::placeholder {
  color: #a0aec0;
}

.filters-sidebar .filter-search {
  margin-bottom: 0.5rem;
}

.filters-sidebar .filter-search-input {
  width: 100%;
  padding: 0.25rem 0.5rem;
  background-color: #232a38;
  color: #fff;
  border: 1px solid #fff;
  border-radius: 4px;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.filters-sidebar .filter-search-input:focus {
  outline: none;
  border-color: #1e40af;
  box-shadow: 0 0 0 0.2rem rgba(30, 64, 175, 0.25);
}

.filters-sidebar .filter-search-input::placeholder {
  color: #a0aec0;
}

.filters-sidebar .filter-list-container {
  max-height: 250px;
  overflow-y: auto;
  padding-right: 5px;
}

.filters-sidebar .filter-list-container::-webkit-scrollbar {
  width: 8px;
}

.filters-sidebar .filter-list-container::-webkit-scrollbar-thumb {
  background-color: #888;
  border-radius: 4px;
}

.filters-sidebar .filter-list-container::-webkit-scrollbar-thumb:hover {
  background-color: #555;
}

.filters-sidebar .sidebar-footer {
  display: flex;
  gap: 12px;
  padding: 1rem 0;
  margin-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.filters-sidebar .sidebar-footer .reset-filters-btn,
.filters-sidebar .sidebar-footer .close-sidebar-btn {
  flex: 1;
  font-size: 0.9rem;
  padding: 0.5rem 1rem;
}

.mobile-filters-btn-promo {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  background-color: transparent;
  border: 1px solid rgba(255, 255, 255, 0.5);
  color: white;
  font-size: 14px;
  font-weight: 500;
  border-radius: 4px;
  cursor: pointer;
  white-space: nowrap;
  position: relative;
  transition: all 0.2s;
}

.mobile-filters-btn-promo:active {
  background-color: rgba(255, 255, 255, 0.1);
}

.filter-icon {
  font-size: 18px;
}

.filter-text {
  color: white;
}

.filter-badge {
  position: absolute;
  top: -8px;
  right: -8px;
  background-color: #ce0707;
  color: white;
  font-size: 11px;
  font-weight: bold;
  padding: 2px 6px;
  border-radius: 10px;
  min-width: 20px;
  text-align: center;
}

/* Styles pour les dropdowns du modal */
.catalog-dropdown .dropdown-menu {
  padding: 0.8rem;
  background-color: #2d3748;
  color: #fff;
  border: 1px solid #fff;
  border-radius: 6px;
  width: max-content;
  max-width: 400px;
}

/* Animation checkbox */
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
    transform: scale(0);
    opacity: 0;
  }
  50% {
    transform: scale(1.3);
    opacity: 1;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

body.body--dark .catalog-modal {
  background-color: #2d3748 !important;
  color: white !important;
}

body.body--dark .catalog-header-section,
body.body--dark .catalog-body {
  background-color: #2d3748 !important;
  color: white !important;
}

body.body--dark .form-select {
  background-color: #1a202c;
  color: white;
  border-color: #4a5568;
}

body.body--dark .form-select option {
  background-color: #1a202c;
  color: white;
}

@media (max-width: 1023px) {
  .catalog-modal {
    width: 100vw !important;
    min-width: 100vw !important;
    height: 100vh !important;
    max-height: 100vh !important;
    margin: 0 !important;
    border-radius: 0 !important;
  }

  .catalog-grid {
    grid-template-columns: 1fr !important;
    gap: 12px;
    padding: 0 8px;
  }

  .catalog-modal :deep(.q-card__section) {
    padding: 12px !important;
  }

  .catalog-header-section {
    padding: 8px !important;
  }

  /* Version Mobile */
  .catalog-header-mobile {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 8px;
    margin-bottom: 6px;
  }

  .catalog-header-mobile .col {
    width: 100%;
    margin-bottom: 8px;
  }

  .mobile-header-buttons {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    gap: 8px;
    position: relative;
  }

  /* Container pour chaque bouton */
  .mobile-button-left,
  .mobile-button-center,
  .mobile-button-right {
    display: flex;
    align-items: center;
    flex: 0 0 auto;
  }

  /* Bouton Filtres */
  .mobile-button-left {
    order: 1;
    flex: 0 0 auto;
  }

  /* Select Trier */
  .mobile-button-center {
    order: 2;
    display: flex;
    justify-content: center;
    align-items: center;
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    pointer-events: all;
  }

  .mobile-button-center .form-select {
    min-width: 120px;
    max-width: 150px;
  }

  /* Bouton Fermer*/
  .mobile-button-right {
    order: 3;
    flex: 0 0 auto;
    margin-left: auto;
  }

  .catalog-header .col-auto .q-btn {
    width: auto;
    min-width: auto;
    padding: 6px 12px;
  }

  .catalog-header .mobile-filters-btn-promo {
    width: auto;
    min-width: auto;
  }

  .catalog-header .form-select {
    width: auto;
    min-width: 120px;
  }

  .catalog-header .q-mb-sm {
    margin-bottom: 0 !important;
  }

  .catalog-filters-container {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    padding-bottom: 8px;
    margin-top: 12px;
  }

  .catalog-filters {
    flex-wrap: nowrap;
    min-width: max-content;
    gap: 8px;
  }

  .catalog-body {
    padding: 12px !important;
    max-height: calc(100vh - 200px);
    overflow-y: auto;
    overflow-x: hidden;
  }

  .catalog-grid {
    grid-template-columns: 1fr !important;
    gap: 12px;
    padding: 0 8px;
  }

  .catalog-card {
    flex-direction: row;
    align-items: flex-start;
    text-align: left;
    padding: 8px;
    min-height: 120px;
    max-height: 140px;
    overflow: hidden;
  }

  .catalog-card .image-wrapper {
    width: 60px;
    height: auto;
    aspect-ratio: 63 / 88;
    flex-shrink: 0;
    margin-bottom: 0;
    margin-right: 8px;
  }

  .catalog-card .mtg-card-body {
    width: 100%;
    text-align: left;
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    min-height: 100%;
  }

  .mtg-card-title {
    font-size: 12px;
    margin-bottom: 2px;
    line-height: 1.2;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
  }

  .mtg-card-serie {
    font-size: 10px;
    margin-bottom: 4px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .mtg-card-price {
    margin: 4px 0;
    font-size: 11px;
  }

  .mtg-card-actions {
    width: 100%;
    margin-top: 4px;
  }

  .mtg-card-actions .q-btn {
    min-width: 100% !important;
    width: 100% !important;
    font-size: 10px !important;
    padding: 4px 8px !important;
    min-height: 28px !important;
  }

  .pagination-container {
    margin-top: 12px;
  }

  .pagination-list {
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px;
  }

  .pagination-btn {
    min-width: 36px;
    height: 36px;
    font-size: 14px;
    padding: 4px 8px;
  }
}
</style>
