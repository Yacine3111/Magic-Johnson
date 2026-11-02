<template>
  <div id="sidebar-filters" class="sidebar p-3" :class="{ active: showFilters, mobile: isMobile }">
    <div class="sidebar-header">
      <h5>Filtres</h5>
      <button class="close-btn" @click="$emit('update:showFilters', false)">✖</button>
    </div>

    <div class="sidebar-content">
      <hr />
      <!-- Filtres actifs -->
      <div class="active-filters">
        <!-- Liste des filtres qu'on peut enlever en haut du sidebar -->
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
              {{ key === 'Description' ? 'DESCRIPTION' : val }}
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
      <!-- Disponibilité -->
      <p class="fw-bold">Disponibilité</p>
      <template v-if="filterOptions['Disponibilité'].length >= 8">
        <input
          type="text"
          :value="searchDisponibilite"
          @input="$emit('update:searchDisponibilite', $event.target.value)"
          placeholder="Rechercher une disponibilité..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedDisponibilite"
            :key="opt"
            class="sidebar-checkbox-item"
            :class="{ selected: filters.disponibilite.includes(opt) }"
            @click="toggleFilter('disponibilite', opt)"
          >
            <input type="checkbox" :checked="filters.disponibilite.includes(opt)" />
            {{ opt }}
          </div>
        </div>
      </template>
      <template v-else>
        <div
          v-for="opt in sortedDisponibilite"
          :key="opt"
          class="sidebar-checkbox-item"
          :class="{ selected: filters.disponibilite.includes(opt) }"
          @click="toggleFilter('disponibilite', opt)"
        >
          <input type="checkbox" :checked="filters.disponibilite.includes(opt)" />
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
          @input="$emit('update:searchLangue', $event.target.value)"
          placeholder="Rechercher une langue..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedLangue"
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
          v-for="opt in sortedLangue"
          :key="opt"
          class="sidebar-checkbox-item"
          :class="{ selected: filters.langue.includes(opt) }"
          @click="toggleFilter('langue', opt)"
        >
          <input type="checkbox" :checked="filters.langue.includes(opt)" />
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
          @input="$emit('update:searchType', $event.target.value)"
          placeholder="Rechercher un type..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedType"
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
          v-for="opt in sortedType"
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
      <!-- Sous-Type (affiché seulement si des types sont sélectionnés) -->
      <template v-if="filters.type.length > 0">
        <p class="fw-bold mt-3">Sous-Type</p>
        <template v-if="availableSubtypes.length >= 8">
          <input
            type="text"
            :value="searchSousType"
            @input="$emit('update:searchSousType', $event.target.value)"
            placeholder="Rechercher un sous-type..."
            class="form-control form-control-sm mb-2"
          />
          <div class="sidebar-list-container with-scroll">
            <div
              v-for="opt in sortedSousType"
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
          @input="$emit('update:searchCouleur', $event.target.value)"
          placeholder="Rechercher une couleur..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedCouleur"
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
          v-for="opt in sortedCouleur"
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
      <!-- Rareté -->
      <p class="fw-bold mt-3">Rareté</p>
      <template v-if="filterOptions['Rareté'].length >= 4">
        <input
          type="text"
          :value="searchRarete"
          @input="$emit('update:searchRarete', $event.target.value)"
          placeholder="Rechercher une rareté..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedRarete"
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
          v-for="opt in sortedRarete"
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
      <!-- Série -->
      <p class="fw-bold mt-3">Série</p>
      <template v-if="filterOptions['Série'].length >= 8">
        <input
          type="text"
          :value="searchSerie"
          @input="$emit('update:searchSerie', $event.target.value)"
          placeholder="Rechercher une série..."
          class="form-control form-control-sm mb-2"
        />
        <div class="sidebar-list-container with-scroll">
          <div
            v-for="opt in sortedSerie"
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
          v-for="opt in sortedSerie"
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
      <!-- Finition -->
      <p class="fw-bold mt-3">Finition</p>
      <div
        v-for="opt in sortedFinition"
        :key="opt"
        class="sidebar-checkbox-item"
        :class="{ selected: filters.finition.includes(opt) }"
        @click="toggleFilter('finition', opt)"
      >
        <input type="checkbox" :checked="filters.finition.includes(opt)" />
        {{ opt }}
      </div>
      <hr />

      <!-- Description -->
      <p class="fw-bold mt-3 description">Description</p>
      <textarea
        ref="descriptionTextarea"
        v-model="filters.description"
        placeholder="Rechercher par description..."
        class="form-control form-control-sm mb-2"
        rows="1"
        style="resize: none; overflow-y: hidden; min-height: 38px"
        @input="adjustHeight"
      ></textarea>

      <hr />

      <!-- Fourchette de Prix -->
      <p class="fw-bold mt-3">Prix ($)</p>
      <div class="price-range-container">
        <div class="d-flex gap-2 align-items-center mb-2">
          <div class="input-group input-group-sm">
            <span class="input-group-text">$</span>
            <input
              type="number"
              v-model.number="filters.priceMin"
              placeholder="0"
              class="form-control"
              min="0"
              step="0.01"
            />
          </div>
          <span class="text-light">-</span>
          <div class="input-group input-group-sm">
            <span class="input-group-text">$</span>
            <input
              type="number"
              v-model.number="filters.priceMax"
              placeholder="999"
              class="form-control"
              min="0"
              step="0.01"
            />
          </div>
        </div>
        <div class="d-flex gap-2">
          <button
            class="btn btn-primary btn-sm flex-grow-1"
            @click="(event) => applyPriceFilter(event)"
            :disabled="!filters.priceMin && !filters.priceMax"
          >
            Appliquer
          </button>
          <button
            class="btn btn-outline-secondary btn-sm"
            @click="(event) => resetPriceFilter(event)"
            :disabled="!filters.priceMin && !filters.priceMax"
            title="Effacer les filtres prix"
          >
            ✕
          </button>
        </div>
      </div>
      <hr />

      <!-- Tri rapide -->
      <p class="fw-bold mt-3">Tri rapide</p>
      <div class="quick-sort-container">
        <button
          v-for="sortOption in quickSortOptions"
          :key="sortOption.value"
          class="btn btn-outline-light btn-sm mb-2 w-100"
          :class="{ active: filters.sortBy === sortOption.value }"
          @click="filters.sortBy = sortOption.value"
        >
          {{ sortOption.label }}
        </button>
        <button
          class="btn btn-outline-secondary btn-sm w-100"
          @click="filters.sortBy = ''"
          :disabled="!filters.sortBy"
        >
          Aucun tri
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch, onMounted } from 'vue'
import { useFiltersStore } from '@/stores/filters'

const props = defineProps({
  showFilters: {
    type: Boolean,
    required: true,
  },
  isMobile: {
    type: Boolean,
    required: true,
  },
  filterOptions: {
    type: Object,
    required: true,
  },
  activeFilters: {
    type: Object,
    required: true,
  },
  filteredSerie: {
    type: Array,
    required: true,
  },
  filteredRarete: {
    type: Array,
    required: true,
  },
  filteredType: {
    type: Array,
    required: true,
  },
  filteredCouleur: {
    type: Array,
    required: true,
  },
  filteredLangue: {
    type: Array,
    required: true,
  },
  filteredDisponibilite: {
    type: Array,
    required: true,
  },
  searchSerie: {
    type: String,
    required: true,
  },
  searchRarete: {
    type: String,
    required: true,
  },
  searchType: {
    type: String,
    required: true,
  },
  searchCouleur: {
    type: String,
    required: true,
  },
  searchLangue: {
    type: String,
    required: true,
  },
  searchDisponibilite: {
    type: String,
    required: true,
  },
  searchSousType: {
    type: String,
    required: true,
  },

  availableSubtypes: {
    type: Array,
    required: true,
  },
  filteredFinition: {
    type: Array,
    required: true,
  },
  toggleFilter: {
    type: Function,
    required: true,
  },
})

const emit = defineEmits([
  'update:showFilters',
  'update:searchSerie',
  'update:searchRarete',
  'update:searchType',
  'update:searchSousType',
  'update:searchCouleur',
  'update:searchLangue',
  'update:searchDisponibilite',
])

const filters = useFiltersStore()

// Variable pour forcer les sélectionnés en haut à l'ouverture
const forceSelectedOnTop = ref(false)

// Options de tri rapide
const quickSortOptions = [
  { value: 'name-asc', label: 'Nom (A-Z)' },
  { value: 'name-desc', label: 'Nom (Z-A)' },
  { value: 'price-asc', label: 'Prix croissant' },
  { value: 'price-desc', label: 'Prix décroissant' },
]

// Watcher pour l'ouverture du sidebar
watch(
  () => props.showFilters,
  (newVal) => {
    if (newVal) {
      forceSelectedOnTop.value = true
    }
  },
)

// Watcher pour détecter certaines interactions
watch(
  [
    () => props.searchDisponibilite,
    () => props.searchLangue,
    () => props.searchType,
    () => props.searchCouleur,
    () => props.searchRarete,
    () => props.searchSerie,
    () => filters.disponibilite,
    () => filters.langue,
    () => filters.type,
    () => filters.couleur,
    () => filters.rarete,
    () => filters.serie,
    () => filters.finition,
  ],
  () => {
    if (forceSelectedOnTop.value) {
      forceSelectedOnTop.value = false
    }
  },
  { deep: true },
)

// Fonction pour trier les options avec les éléments sélectionnés en premier
const sortOptionsWithSelectedFirst = (options, selectedItems, searchTerm = '') => {
  if (!options || !Array.isArray(options)) return []

  // Filtrer par terme de recherche si fourni
  let filteredOptions = options
  if (searchTerm) {
    filteredOptions = options.filter((option) =>
      option.toLowerCase().includes(searchTerm.toLowerCase()),
    )
  }

  // Séparer les éléments sélectionnés et non sélectionnés
  const selected = filteredOptions.filter((option) => selectedItems.includes(option))
  const notSelected = filteredOptions.filter((option) => !selectedItems.includes(option))

  // Trier chaque groupe par ordre alphabétique
  const sortedSelected = selected.sort((a, b) => a.localeCompare(b))
  const sortedNotSelected = notSelected.sort((a, b) => a.localeCompare(b))

  // Retourner les sélectionnés en premier
  return [...sortedSelected, ...sortedNotSelected]
}

// Fonction de tri
const sortOptions = (options, selectedOptions) => {
  return sortOptionsWithSelectedFirst(options, selectedOptions)
}

// Propriétés calculées pour chaque type de filtre, pour mettre les trucs en haut apres fermerture
const sortedDisponibilite = computed(() => {
  const list =
    props.filterOptions['Disponibilité'].length >= 8
      ? props.filteredDisponibilite
      : props.filterOptions['Disponibilité']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.disponibilite)
  } else {
    return list
  }
})

const sortedLangue = computed(() => {
  const list =
    props.filterOptions['Langue'].length >= 5 ? props.filteredLangue : props.filterOptions['Langue']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.langue)
  } else {
    return list
  }
})

const sortedType = computed(() => {
  const list =
    props.filterOptions['Type'].length >= 5 ? props.filteredType : props.filterOptions['Type']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.type)
  } else {
    return list
  }
})

const sortedCouleur = computed(() => {
  const list =
    props.filterOptions['Couleur'].length >= 4
      ? props.filteredCouleur
      : props.filterOptions['Couleur']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.couleur)
  } else {
    return list
  }
})

const sortedRarete = computed(() => {
  const list =
    props.filterOptions['Rareté'].length >= 4 ? props.filteredRarete : props.filterOptions['Rareté']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.rarete)
  } else {
    return list
  }
})

const sortedSerie = computed(() => {
  const list =
    props.filterOptions['Série'].length >= 8 ? props.filteredSerie : props.filterOptions['Série']

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.serie)
  } else {
    return list
  }
})

const sortedFinition = computed(() => {
  const list = props.filteredFinition

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.finition)
  } else {
    return list
  }
})

const sortedSousType = computed(() => {
  const list = props.availableSubtypes

  if (forceSelectedOnTop.value) {
    return sortOptions(list, filters.sousType)
  } else {
    return list
  }
})

const applyPriceFilter = (event = null) => {
  if (event) {
    event.preventDefault()
    event.stopPropagation()
  }

  if (filters.priceMin !== null || filters.priceMax !== null) {
    filters.currentPage = 1
  }
}

// Fonction pour réinitialiser le filtre de prix
const resetPriceFilter = (event = null) => {
  if (event) {
    event.preventDefault()
    event.stopPropagation()
  }

  filters.priceMin = null
  filters.priceMax = null
  filters.currentPage = 1
}

const descriptionTextarea = ref(null)

const adjustHeight = () => {
  const element = descriptionTextarea.value
  if (element) {
    element.style.height = 'auto'
    element.style.height = element.scrollHeight + 'px'
  }
}

watch(
  () => filters.description,
  () => {
    setTimeout(adjustHeight, 0)
  },
)

onMounted(() => {
  setTimeout(adjustHeight, 0)
})
</script>
