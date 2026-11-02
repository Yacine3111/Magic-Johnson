import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useInventoryFiltersStore = defineStore('inventoryFilters', () => {
  const searchQuery = ref('')
  const disponibilite = ref([])
  const langue = ref([])
  const type = ref([])
  const sousType = ref([])
  const couleur = ref([])
  const rarete = ref([])

  const serie = ref([])
  const finition = ref([])

  // Pagination
  const currentPage = ref(1)
  const itemsPerPage = ref(18)

  // Tri
  const sortBy = ref('')

  // Prix
  const priceMin = ref(null)
  const priceMax = ref(null)

  const clearFilters = () => {
    searchQuery.value = ''
    disponibilite.value = []
    langue.value = []
    type.value = []
    sousType.value = []
    couleur.value = []
    rarete.value = []
    rarete.value = []
    serie.value = []
    finition.value = []
    currentPage.value = 1
    sortBy.value = ''
    priceMin.value = null
    priceMax.value = null
  }

  const activeFilters = computed(() => {
    const f = {}
    if (disponibilite.value.length) f['Disponibilité'] = [...disponibilite.value]
    if (langue.value.length) f['Langue'] = [...langue.value]
    if (type.value.length) f['Type'] = [...type.value]
    if (sousType.value.length) f['Sous-Type'] = [...sousType.value]
    if (couleur.value.length) f['Couleur'] = [...couleur.value]
    if (rarete.value.length) f['Rareté'] = [...rarete.value]
    if (rarete.value.length) f['Rareté'] = [...rarete.value]
    if (serie.value.length) f['Série'] = [...serie.value]
    if (finition.value.length) f['Finition'] = [...finition.value]
    if (priceMin.value !== null || priceMax.value !== null) {
      const priceLabel = []
      if (priceMin.value !== null) priceLabel.push(`Min: $${priceMin.value}`)
      if (priceMax.value !== null) priceLabel.push(`Max: $${priceMax.value}`)
      f['Prix'] = [priceLabel.join(' - ')]
    }
    return f
  })

  // Calcul du nombre total de filtres actifs
  const activeFiltersCount = computed(() => {
    return Object.values(activeFilters.value).reduce((total, filters) => total + filters.length, 0)
  })

  const removeFilter = (key, val) => {
    const map = {
      Disponibilité: disponibilite,
      Langue: langue,
      Type: type,
      'Sous-Type': sousType,
      Couleur: couleur,
      Rareté: rarete,
      Série: serie,
      Finition: finition,
    }

    // Gestion filtres de prix
    if (key === 'Prix') {
      priceMin.value = null
      priceMax.value = null
    } else if (map[key]) {
      map[key].value = map[key].value.filter((v) => v !== val)
    }

    currentPage.value = 1
  }

  const toggleFilter = (key, value) => {
    const map = {
      Disponibilité: disponibilite,
      Langue: langue,
      Type: type,
      'Sous-Type': sousType,
      Couleur: couleur,
      Rareté: rarete,
      Série: serie,
      Finition: finition,
    }
    const refArray = map[key]
    if (!refArray) return
    if (refArray.value.includes(value)) {
      refArray.value = refArray.value.filter((v) => v !== value)
    } else {
      refArray.value.push(value)
    }
    currentPage.value = 1
  }

  return {
    searchQuery,
    disponibilite,
    langue,
    type,
    sousType,
    couleur,
    rarete,
    serie,
    finition,
    currentPage,
    itemsPerPage,
    sortBy,
    priceMin,
    priceMax,
    clearFilters,
    activeFilters,
    activeFiltersCount,
    removeFilter,
    toggleFilter,
  }
})
