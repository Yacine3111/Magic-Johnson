<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useQuasar } from 'quasar'
import FilterDropdown from '@/components/common/FilterDropdown.vue'
import { cardService } from '@/services/CardService'
import { promotionService } from '@/services/PromotionService'
import PromotionBoard from './PromotionManagement/PromotionBoard.vue'
import EditionBoard from './PromotionManagement/EditionBoard.vue'
import ModalCards from './PromotionManagement/ModalCards.vue'
import CardsBoard from './PromotionManagement/CardsBoard.vue'

const $q = useQuasar()

const searchTerm = ref('')
const filters = ref({ statut: [] })
const rabaisMin = ref(null)
const rabaisMax = ref(null)
const sortBy = ref('')
const editingPromotion = ref(null)
const selectedPromotionForCards = ref(null)
const selectedCards = ref([])
const validationErrors = ref({})
const showCatalog = ref(false)
const showMobileFiltersPromo = ref(false)

const showConflictModal = ref(false)
const conflictList = ref([])
const loadingSave = ref(false)

const activeFiltersCountPromo = computed(() => {
  let count = 0
  if (filters.value.statut.length > 0) count += filters.value.statut.length
  if (rabaisMin.value !== null && rabaisMin.value !== '') count += 1
  if (rabaisMax.value !== null && rabaisMax.value !== '') count += 1
  if (sortBy.value) count += 1
  return count
})
const catalogSearch = ref('')
const catalogPage = ref(1)
const catalogPerPage = ref(6)
const catalogFilters = ref({
  couleur: '',
  rarete: '',
  prixMin: null,
  prixMax: null,
  disponibilite: '',
  langue: '',
  type: '',
  sousType: '',
  serie: '',
})
const catalogSort = ref('')
const catalogSearchSerie = ref('')
const catalogSearchLangue = ref('')
const catalogSearchType = ref('')

// Pagination
const currentPage = ref(1)
const itemsPerPage = ref(5)
const currentCardsPage = ref(1)
const cardsPerPage = ref(5)

// État pour les cartes réelles du backend
const cards = ref([])
const loadingCards = ref(false)
const cardsError = ref(null)
const catalogCards = ref([])
const catalogLoading = ref(false)
const catalogError = ref(null)
const catalogSeriesOptions = ref([])
const totalCatalogPages = ref(0)
const catalogTotalCards = ref(0)

// États de chargement
const loadingPromotions = ref(false)
const loadingAssociatedCards = ref(false)
const loadingPromotionsService = ref(true)

// Utilisation du service de promotions
const promotions = ref([])

const associatedCards = ref([])
const cardsSearchTerm = ref('')
const newlyAddedCards = ref([])

// Fonctions pour charger les cartes du backend
const loadCards = async () => {
  loadingCards.value = true
  cardsError.value = null

  try {
    const response = await cardService.getCards(1, 50, {})
    if (response && response.cards) {
      cards.value = response.cards.map((card) => ({
        id: card.id,
        nom: card.name,
        serie: card.setName,
        numero: card.number,
        rarete: card.rarity,
        couleur: card.couleur,
        prix: card.price,
        prixFoil: card.foilPrice,
        hasFoil: card.hasFoil,
        stock: card.totalQuantity,
        image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image`,
        type: card.type,
        disponibilite: card.disponibilite,
        langue: card.langue,
        manaCost: card.manaCost,
        text: card.text,
        power: card.power,
        toughness: card.toughness,
      }))
    }
  } catch (error) {
    cardsError.value = 'Erreur lors du chargement des cartes'
  } finally {
    loadingCards.value = false
  }
}

// Fonction pour charger les promotions du backend
const loadPromotions = async () => {
  loadingPromotions.value = true

  try {
    const backendPromotions = await promotionService.getAllPromotions()

    if (backendPromotions && Array.isArray(backendPromotions) && backendPromotions.length > 0) {
      const formattedPromotionsWithCards = await Promise.all(
        backendPromotions.map(async (promo) => {
          // Vérifier que l'objet promo existe et a les propriétés nécessaires
          if (!promo || typeof promo !== 'object') {
            return null
          }

          // Charger les cartes de cette promotion
          let carteIds = []
          try {
            const cards = await promotionService.getPromotionCards(promo.id)
            if (cards && Array.isArray(cards)) {
              carteIds = cards.map((card) => card.id || card)
            }
          } catch (error) {}

          const now = new Date()
          const endDate = promo.endDate ? new Date(promo.endDate) : null
          return {
            id: promo.id || null,
            nom: promo.title || 'Sans titre',
            debut: promo.startDate
              ? new Date(promo.startDate).toISOString().split('T')[0]
              : new Date().toISOString().split('T')[0],
            fin: promo.endDate
              ? new Date(promo.endDate).toISOString().split('T')[0]
              : new Date().toISOString().split('T')[0],
            statut: promo.status || 'En Attente',
            rabais: promo.rate || 0,
            description: promo.description || '',
            carteIds: carteIds,
            cartes: [],
          }
        }),
      )

      const formattedPromotions = formattedPromotionsWithCards.filter((promo) => promo !== null)

      promotions.value = formattedPromotions
    } else {
      promotions.value = []
    }
  } catch (error) {
    let errorMessage = 'Erreur lors du chargement des promotions'
    if (error.response?.status === 404) {
      errorMessage = 'Endpoint non trouvé - vérifiez que le backend est démarré'
    } else if (error.response?.status === 500) {
      errorMessage = 'Erreur serveur - vérifiez les logs du backend'
    } else if (error.code === 'ERR_NETWORK') {
      errorMessage = 'Impossible de se connecter au backend'
    }

    $q.notify({
      type: 'negative',
      message: errorMessage,
      caption: error.response?.data?.message || error.message,
    })

    promotions.value = []
  } finally {
    loadingPromotions.value = false
  }
}

const isCardInAnotherPromotion = (cardId) => {
  if (!editingPromotion.value) return false

  return promotions.value.some((promo) => {
    if (editingPromotion.value.id && promo.id === editingPromotion.value.id) {
      return false
    }
    return promo.carteIds && promo.carteIds.includes(cardId)
  })
}

const loadCatalogCards = async (page = 1) => {
  catalogLoading.value = true
  catalogError.value = null

  try {
    const filters = {
      searchQuery: catalogSearch.value,
      serie: catalogFilters.value.serie || [],
      rarete: catalogFilters.value.rarete || [],
      type: catalogFilters.value.type || [],
      sousType: catalogFilters.value.sousType || [],
      couleur: catalogFilters.value.couleur || [],
      langue: catalogFilters.value.langue || [],
      disponibilite: catalogFilters.value.disponibilite || [],
      sortBy: catalogSort.value,
      priceMin: catalogFilters.value.prixMin,
      priceMax: catalogFilters.value.prixMax,
    }

    const response = await cardService.getCards(page, catalogPerPage.value, filters)
    if (response && response.cards) {
      catalogCards.value = response.cards.map((card) => ({
        id: card.id,
        nom: card.name,
        serie: card.setName,
        numero: card.number,
        rarete: card.rarity,
        couleur: card.couleur,
        prix: card.price,
        prixFoil: card.foilPrice,
        hasFoil: card.hasFoil,
        stock: card.totalQuantity,
        image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image`,
        type: card.type,
        disponibilite: card.disponibilite,
        langue: card.langue,
        manaCost: card.manaCost,
        text: card.text,
        power: card.power,
        toughness: card.toughness,
      }))
      totalCatalogPages.value = response.pagination?.totalPages || 0
      catalogTotalCards.value = response.pagination?.totalCards || 0
    }
  } catch (error) {
    catalogError.value = 'Erreur lors du chargement du catalogue'
  } finally {
    catalogLoading.value = false
  }
}

const loadFilterOptions = async () => {
  try {
    await loadFilterOptionsFromAPI()
    if (apiFilterOptions.value && apiFilterOptions.value.series) {
      catalogSeriesOptions.value = apiFilterOptions.value.series
    }
  } catch (error) {}
}

const apiFilterOptions = ref(null)

// Variables pour les sous-types
const availableSubtypes = ref([])
const loadingSubtypes = ref(false)

const loadFilterOptionsFromAPI = async () => {
  try {
    const response = await cardService.getFilterOptions()
    apiFilterOptions.value = response
  } catch (error) {}
}

// Fonction pour charger les sous-types
const loadSubtypes = async () => {
  try {
    if (!catalogFilters.value.type || catalogFilters.value.type === '') {
      availableSubtypes.value = []
      return
    }

    loadingSubtypes.value = true

    const response = await cardService.getSubtypes([catalogFilters.value.type])

    availableSubtypes.value = response?.subTypes || []
  } catch (error) {
    availableSubtypes.value = []
  } finally {
    loadingSubtypes.value = false
  }
}

const getDisplayStatus = (promo) => {
  const now = new Date()

  if (!promo.debut || !promo.fin) {
    if (!promo.statut) {
      return 'Inactive'
    }
    return 'Active'
  }

  const startDate = new Date(promo.debut)
  const endDate = new Date(promo.fin)

  if (now > endDate) {
    return 'Expirée'
  }

  if (now < startDate) {
    if (promo.statut) {
      return 'En attente'
    }
    return 'Inactive'
  }

  if (now >= startDate && now <= endDate) {
    if (promo.statut) {
      return 'Active'
    }
    return 'Inactive'
  }

  return 'Active'
}

const formatDate = (dateStr) => {
  if (!dateStr) return 'N/A'
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })
  } catch (e) {
    return dateStr
  }
}

const getCardStatus = (stock) => {
  if (stock === 0) {
    return { etat: 'Rupture de stock', etatColor: 'red' }
  } else if (stock <= 10) {
    return { etat: 'Stock faible', etatColor: 'orange' }
  } else {
    return { etat: 'En stock', etatColor: 'green' }
  }
}

const isNewlyAddedCard = (card) => {
  if (!editingPromotion.value) return false

  if (editingPromotion.value.id === null) {
    return editingPromotion.value.carteIds && editingPromotion.value.carteIds.includes(card.id)
  }

  const originalCardIds = originalPromotionCards.value || []
  const isInEditingPromotion =
    editingPromotion.value.carteIds && editingPromotion.value.carteIds.includes(card.id)
  const isNotInOriginalCards = !originalCardIds.includes(card.id)

  return isInEditingPromotion && isNotInOriginalCards
}

const clearFilters = () => {
  filters.value = { statut: [] }
  rabaisMin.value = null
  rabaisMax.value = null
  sortBy.value = ''
  currentPage.value = 1
}

const sanitizeRabais = (isMin) => (e) => {
  const v = e.target.value
  const n = v === '' || v === null ? null : Number(v)
  if (isMin) {
    rabaisMin.value = isNaN(n) || n < 0 ? 0 : n
  } else {
    rabaisMax.value = isNaN(n) || n < 0 ? 0 : n
  }
}

const filteredPromotions = computed(() => {
  if (!promotions.value || !Array.isArray(promotions.value)) {
    return []
  }

  let list = promotions.value.slice()

  if (searchTerm.value) {
    const q = searchTerm.value.toLowerCase()
    list = list.filter(
      (p) => p.nom.toLowerCase().includes(q) || (p.description || '').toLowerCase().includes(q),
    )
  }

  if (filters.value.statut.length) {
    list = list.filter((p) => {
      let statutText = 'Inactive'
      if (p.statut === 'Actif') statutText = 'Active'
      else if (p.statut === 'En Attente') statutText = 'En attente'

      if (p.statut === 'Actif') statutText = 'Active'
      else if (p.statut === 'Inactif') statutText = 'Inactive'
      else if (p.statut === 'En Attente') statutText = 'Inactive'

      return filters.value.statut.includes(statutText)
    })
  }

  if (rabaisMin.value !== null && rabaisMin.value !== '') {
    list = list.filter((p) => Number(p.rabais) >= Number(rabaisMin.value))
  }
  if (rabaisMax.value !== null && rabaisMax.value !== '') {
    list = list.filter((p) => Number(p.rabais) <= Number(rabaisMax.value))
  }

  if (sortBy.value) {
    const [key, dir] = sortBy.value.split('-')
    const multiplier = dir === 'desc' ? -1 : 1

    list.sort((a, b) => {
      const getVal = (item, k) => {
        if (k === 'name') return item.nom || ''
        if (k === 'debut' || k === 'fin') return new Date(item[k] || 0)
        if (k === 'rabais') return Number(item[k] || 0)
        if (k === 'statut') return item.statut || ''
        return item[k]
      }

      const va = getVal(a, key)
      const vb = getVal(b, key)

      if (va instanceof Date && vb instanceof Date) return multiplier * (va - vb)
      if (typeof va === 'number' && typeof vb === 'number') return multiplier * (va - vb)
      return multiplier * String(va).localeCompare(String(vb))
    })
  }

  return list
})

const filteredCatalog = computed(() => {
  return catalogCards.value
})

const catalogPaginated = computed(() => {
  return filteredCatalog.value
})

const filteredCatalogSeries = computed(() => {
  if (!catalogSearchSerie.value) return catalogSeriesOptions.value
  return catalogSeriesOptions.value.filter((option) =>
    option.toLowerCase().includes(catalogSearchSerie.value.toLowerCase()),
  )
})

const filteredCatalogLangues = computed(() => {
  if (!catalogSearchLangue.value) return filterOptions.value.langues || []
  return (filterOptions.value.langues || []).filter((option) =>
    option.toLowerCase().includes(catalogSearchLangue.value.toLowerCase()),
  )
})

const filteredCatalogTypes = computed(() => {
  if (!catalogSearchType.value) return filterOptions.value.types || []
  return (filterOptions.value.types || []).filter((option) =>
    option.toLowerCase().includes(catalogSearchType.value.toLowerCase()),
  )
})

// Options de filtres depuis l'API
const filterOptions = computed(() => {
  if (apiFilterOptions.value) {
    return {
      couleurs: apiFilterOptions.value.colors || [],
      raretes: apiFilterOptions.value.rarities || [],
      types: apiFilterOptions.value.types || [],
      langues: apiFilterOptions.value.languages || [],
      series: apiFilterOptions.value.series || [],
      disponibilites: apiFilterOptions.value.availabilities || [],
    }
  }

  // Options vides si pas de données
  return {
    couleurs: [],
    raretes: [],
    types: [],
    langues: [],
    series: [],
    disponibilites: [],
  }
})

// Computed pour la pagination du catalogue
const hasNextCatalogPage = computed(() => {
  return catalogPage.value < totalCatalogPages.value
})

const hasPreviousCatalogPage = computed(() => {
  return catalogPage.value > 1
})

const totalPages = computed(() => Math.ceil(filteredPromotions.value.length / itemsPerPage.value))
const hasNextPage = computed(() => currentPage.value < totalPages.value)
const hasPreviousPage = computed(() => currentPage.value > 1)
const paginatedPromotions = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  return filteredPromotions.value.slice(start, start + itemsPerPage.value)
})

const totalCardsPages = computed(() => {
  let sourceCards = []

  if (editingPromotion.value && editingPromotion.value.id === null) {
    sourceCards = filteredSelectedCards.value
  } else {
    const associatedCardsFiltered = filteredAssociatedCards.value.filter(
      (card) => !newlyAddedCards.value.some((newCard) => newCard.id === card.id),
    )
    sourceCards = [...associatedCardsFiltered, ...filteredNewlyAddedCards.value]
  }

  return Math.ceil(sourceCards.length / cardsPerPage.value)
})
const hasNextCardsPage = computed(() => currentCardsPage.value < totalCardsPages.value)
const hasPreviousCardsPage = computed(() => currentCardsPage.value > 1)

const filteredSelectedCards = computed(() => {
  if (!cardsSearchTerm.value) {
    return selectedCards.value
  }

  const searchTerm = cardsSearchTerm.value.toLowerCase()
  return selectedCards.value.filter(
    (card) =>
      (card.nom || '').toLowerCase().includes(searchTerm) ||
      (card.serie || '').toLowerCase().includes(searchTerm) ||
      (card.type || '').toLowerCase().includes(searchTerm) ||
      (card.rarete || '').toLowerCase().includes(searchTerm),
  )
})

// Computed pour filtrer les cartes associées par recherche (frontend)
const filteredAssociatedCards = computed(() => {
  if (!cardsSearchTerm.value) {
    return associatedCards.value
  }

  const searchTerm = cardsSearchTerm.value.toLowerCase()
  const filtered = associatedCards.value.filter(
    (card) =>
      (card.nom || '').toLowerCase().includes(searchTerm) ||
      (card.serie || '').toLowerCase().includes(searchTerm) ||
      (card.type || '').toLowerCase().includes(searchTerm) ||
      (card.rarete || '').toLowerCase().includes(searchTerm),
  )
  return filtered
})

const filteredNewlyAddedCards = computed(() => {
  if (!cardsSearchTerm.value) {
    return newlyAddedCards.value
  }

  const searchTerm = cardsSearchTerm.value.toLowerCase()
  const filtered = newlyAddedCards.value.filter(
    (card) =>
      (card.nom || '').toLowerCase().includes(searchTerm) ||
      (card.serie || '').toLowerCase().includes(searchTerm) ||
      (card.type || '').toLowerCase().includes(searchTerm) ||
      (card.rarete || '').toLowerCase().includes(searchTerm),
  )
  return filtered
})

const paginatedCards = computed(() => {
  if (editingPromotion.value && editingPromotion.value.id === null) {
    const start = (currentCardsPage.value - 1) * cardsPerPage.value
    return filteredSelectedCards.value.slice(start, start + cardsPerPage.value)
  } else {
    const associatedCardsFiltered = filteredAssociatedCards.value.filter(
      (card) => !newlyAddedCards.value.some((newCard) => newCard.id === card.id),
    )
    const allCards = [...associatedCardsFiltered, ...filteredNewlyAddedCards.value]
    const start = (currentCardsPage.value - 1) * cardsPerPage.value
    return allCards.slice(start, start + cardsPerPage.value)
  }
})

// Gestionnaires d'événements
const handleCatalogSearch = () => {
  catalogPage.value = 1
  loadCatalogCards()
}

const handleCatalogFilters = (newFilters) => {
  catalogFilters.value = { ...catalogFilters.value, ...newFilters }
  catalogPage.value = 1
  loadCatalogCards()
}

const handleCatalogSort = (sort) => {
  catalogSort.value = sort
  catalogPage.value = 1
  loadCatalogCards()
}

// Gestionnaire pour la recherche
const handleCatalogSearchInput = () => {
  catalogPage.value = 1
  loadCatalogCards()
}

const handleCatalogPageChange = (page) => {
  catalogPage.value = page
  loadCatalogCards(page)
}

const handleCatalogReset = () => {
  catalogFilters.value = {
    couleur: '',
    rarete: '',
    prixMin: null,
    prixMax: null,
    disponibilite: '',
    langue: '',
    type: '',
    sousType: '',
    serie: '',
  }
  catalogSearch.value = ''
  catalogSort.value = ''
  catalogPage.value = 1
  loadCatalogCards()
}

//  ajouter toutes les cartes d'une série depuis le modal
const handleAddSeriesFromModal = async (serieName) => {
  if (!editingPromotion.value || !serieName) {
    return
  }

  try {
    let allCards = []
    let page = 1
    let hasMore = true

    while (hasMore) {
      const filters = {
        searchQuery: '',
        serie: [serieName],
        rarete: [],
        type: [],
        sousType: [],
        couleur: [],
        langue: [],
        disponibilite: [],
        sortBy: '',
        priceMin: null,
        priceMax: null,
      }

      const response = await cardService.getCards(page, 100, filters)

      if (response && response.cards) {
        const cards = response.cards.map((card) => ({
          id: card.id,
          nom: card.name,
          serie: card.setName,
          numero: card.number,
          rarete: card.rarity,
          couleur: card.couleur,
          prix: card.price,
          prixFoil: card.foilPrice,
          hasFoil: card.hasFoil,
          stock: card.totalQuantity,
          image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image`,
          type: card.type,
          disponibilite: card.disponibilite,
          langue: card.langue,
          manaCost: card.manaCost,
          text: card.text,
          power: card.power,
          toughness: card.toughness,
        }))

        allCards = [...allCards, ...cards]

        const totalPages = response.pagination?.totalPages || 0
        if (page >= totalPages) {
          hasMore = false
        } else {
          page++
        }
      } else {
        hasMore = false
      }
    }

    let addedCount = 0
    for (const card of allCards) {
      if (!editingPromotion.value.carteIds || !editingPromotion.value.carteIds.includes(card.id)) {
        handleAddCard(card)
        addedCount++
      }
    }

    $q.notify({
      type: addedCount > 0 ? 'positive' : 'warning',
      message:
        addedCount > 0
          ? `${addedCount} carte(s) de la série "${serieName}" ajoutée(s)`
          : `Aucune carte de la série "${serieName}" n'a pu être ajoutée`,
      position: 'top',
    })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Erreur lors de la récupération des cartes de la série',
      caption: error.message,
      position: 'top',
    })
  }
}

// Fonction pour ajouter toutes les cartes d'une série depuis le panneau d'édition
const handleAddSeries = async () => {
  if (!editingPromotion.value || !editingPromotion.value.selectedSerie) {
    return
  }

  await handleAddSeriesFromModal(editingPromotion.value.selectedSerie)

  // Mettre à jour selectedPromotionForCards avec les nouveaux IDs
  if (selectedPromotionForCards.value && editingPromotion.value) {
    selectedPromotionForCards.value.carteIds = [...(editingPromotion.value.carteIds || [])]
  }

  if (selectedPromotionForCards.value) {
    if (editingPromotion.value.id === null) {
      await nextTick()
      if (selectedCards.value && selectedCards.value.length > 0) {
        associatedCards.value = [...selectedCards.value]
      }
    } else {
      await loadAssociatedCards()
    }
  }

  editingPromotion.value.selectedSerie = null
}

const handleAddCard = (card) => {
  if (!editingPromotion.value) {
    return
  }

  if (!editingPromotion.value.carteIds) {
    editingPromotion.value.carteIds = []
  }

  if (!editingPromotion.value.carteIds.includes(card.id)) {
    editingPromotion.value.carteIds.push(card.id)

    // Ajouter la carte à newlyAddedCards pour l'affichage immédiat avec rangée verte
    const cardWithStatus = {
      ...card,
      isNewlyAdded: true,
    }

    const existingCardIndex = newlyAddedCards.value.findIndex((c) => c.id === card.id)

    if (existingCardIndex === -1) {
      newlyAddedCards.value.push(cardWithStatus)
    }

    if (editingPromotion.value.id !== null) {
      if (!associatedCards.value.some((c) => c.id === card.id)) {
        associatedCards.value.push(cardWithStatus)
      }
    }

    if (selectedPromotionForCards.value && editingPromotion.value) {
      if (
        editingPromotion.value.id === null ||
        selectedPromotionForCards.value.id === editingPromotion.value.id
      ) {
        selectedPromotionForCards.value.carteIds = [...editingPromotion.value.carteIds]
      }
    }

    if (
      editingPromotion.value.id === null &&
      !selectedCards.value.some((selectedCard) => selectedCard.id === card.id)
    ) {
      selectedCards.value.push(cardWithStatus)
    }
  }
}

const openCatalog = () => {
  showCatalog.value = true
  catalogPage.value = 1
  catalogSearch.value = ''
  catalogSort.value = ''
  catalogFilters.value = {
    couleur: [],
    rarete: [],
    prixMin: null,
    prixMax: null,
    disponibilite: [],
    langue: [],
    type: [],
    sousType: [],
    serie: [],
  }
  loadCatalogCards()
}

// Initialisation des données
onMounted(async () => {
  try {
    const minLoadingTime = new Promise((resolve) => setTimeout(resolve, 500))

    const [cardsResult, filterOptionsResult, promotionsResult] = await Promise.all([
      loadCards(),
      loadFilterOptions(),
      loadPromotions(),
      minLoadingTime,
    ])
  } catch (error) {
  } finally {
    loadingPromotionsService.value = false
  }
})

// Actions
const originalPromotionCards = ref([])

const toLocalMidnightIso = (dateStr) => {
  if (!dateStr) return new Date().toISOString()
  try {
    const [year, month, day] = dateStr.split('-').map((v) => Number(v))

    const dt = new Date(year, (month || 1) - 1, day || 1, 0, 0, 0, 0)
    return dt.toISOString()
  } catch (e) {
    return new Date(dateStr).toISOString()
  }
}

const editPromotion = (promo) => {
  const promoToEdit = JSON.parse(JSON.stringify(promo))

  editingPromotion.value = promoToEdit
  selectedCards.value = []

  originalPromotionCards.value = [...(promo.carteIds || [])]
  newlyAddedCards.value = []

  showPromotionCards(promo)
}

const createNewPromotion = () => {
  editingPromotion.value = {
    id: null,
    nom: '',
    statut: 'En Attente',
    debut: '',
    fin: '',
    rabais: 0,
    description: '',
    cartes: [],
    carteIds: [],
    selectedSerie: null,
  }
  selectedPromotionForCards.value = editingPromotion.value
  selectedCards.value = []
  originalPromotionCards.value = []
  associatedCards.value = []
  newlyAddedCards.value = []
  validationErrors.value = {}
}

const showPromotionCards = async (promo) => {
  selectedPromotionForCards.value = {
    ...promo,
    carteIds: [...(promo.carteIds || [])],
  }

  editingPromotion.value = {
    id: promo.id,
    nom: promo.nom,
    statut: promo.statut,
    debut: promo.debut,
    fin: promo.fin,
    rabais: promo.rabais,
    description: promo.description,
    carteIds: [...(promo.carteIds || [])],
    cartes: promo.cartes || [],
    selectedSerie: null,
  }
  currentCardsPage.value = 1
  selectedCards.value = []
  await loadAssociatedCards()

  if (associatedCards.value && associatedCards.value.length > 0) {
    selectedCards.value = [...associatedCards.value]
  }
}

const validatePromotion = (promotion) => {
  const errors = {}

  if (!promotion.nom || promotion.nom.trim() === '') {
    errors.nom = 'Le nom de la promotion est obligatoire'
  } else if (promotion.nom.length > 100) {
    errors.nom = 'Le nom ne peut pas dépasser 100 caractères'
  }

  if (!promotion.description || promotion.description.trim() === '') {
    errors.description = 'La description est obligatoire'
  } else if (promotion.description.length > 300) {
    errors.description = 'La description ne peut pas dépasser 300 caractères'
  }

  if (!promotion.debut || promotion.debut.trim() === '') {
    errors.debut = 'La date de début est obligatoire'
  }

  if (!promotion.fin || promotion.fin.trim() === '') {
    errors.fin = 'La date de fin est obligatoire'
  }

  if (promotion.rabais === null || promotion.rabais === undefined || promotion.rabais < 0) {
    errors.rabais = 'Le rabais doit être un nombre positif'
  } else if (promotion.rabais > 100) {
    errors.rabais = 'Le rabais ne peut pas dépasser 100%'
  }

  if (promotion.rabais === 0) {
    errors.rabais = 'Le rabais ne peut pas être 0'
  }

  if (promotion.debut && promotion.fin) {
    const dateDebut = new Date(promotion.debut)
    const dateFin = new Date(promotion.fin)
    const today = new Date()
    today.setHours(0, 0, 0, 0)

    const createLocalDate = (dateStr) => {
      const [y, m, d] = dateStr.split('-').map(Number)
      return new Date(y, m - 1, d)
    }

    const startDateMidnight = createLocalDate(promotion.debut)
    startDateMidnight.setHours(0, 0, 0, 0)

    const endDateMidnight = createLocalDate(promotion.fin)
    endDateMidnight.setHours(0, 0, 0, 0)

    if (startDateMidnight < today) {
      errors.debut = "La date de début ne peut pas être antérieure à aujourd'hui"
    }

    if (dateDebut >= dateFin) {
      errors.fin = 'La date de fin doit être postérieure à la date de début'
    }

    if (!errors.fin && endDateMidnight < today) {
      errors.fin = "La date d'expiration ne peut pas être antérieure à aujourd'hui"
    }
  }

  const hasAssociatedCards = associatedCards.value && associatedCards.value.length > 0
  const hasSelectedCards = selectedCards.value && selectedCards.value.length > 0
  const hasPromotionCards = promotion.carteIds && promotion.carteIds.length > 0

  const hasCards = hasAssociatedCards || hasSelectedCards || hasPromotionCards

  if (!hasCards) {
    errors.cartes =
      'Au moins une carte doit être ajoutée à la promotion pour pouvoir la sauvegarder.'
  }

  return errors
}

const validateField = (fieldName, value) => {
  if (!editingPromotion.value) return ''

  const promotion = editingPromotion.value
  const errors = validatePromotion(promotion)
  return errors[fieldName] || ''
}

const findCardsInOtherPromotions = (cardIds) => {
  const affectedPromotions = {}

  cardIds.forEach((cardId) => {
    promotions.value.forEach((promo) => {
      if (
        editingPromotion.value &&
        editingPromotion.value.id &&
        promo.id === editingPromotion.value.id
      ) {
        return
      }

      if (promo.carteIds && promo.carteIds.includes(cardId)) {
        if (!affectedPromotions[promo.id]) {
          affectedPromotions[promo.id] = {
            nom: promo.nom,
            cardIds: [],
          }
        }
        affectedPromotions[promo.id].cardIds.push(cardId)
      }
    })
  })

  return Object.keys(affectedPromotions).length > 0 ? affectedPromotions : null
}

const savePromotion = async () => {
  if (editingPromotion.value) {
    const validationResult = validatePromotion(editingPromotion.value)

    if (Object.keys(validationResult).length > 0) {
      validationErrors.value = validationResult

      return
    }

    try {
      const accessToken = localStorage.getItem('accessToken')
      if (!accessToken) {
        $q.notify({
          type: 'negative',
          message: 'Non authentifié',
          caption: 'Veuillez vous reconnecter pour sauvegarder',
        })
        return
      }

      let allCardsIds = []
      let cardsToCheck = []

      if (editingPromotion.value.id === null) {
        allCardsIds = editingPromotion.value.carteIds || []
        cardsToCheck = allCardsIds
      } else {
        const currentCards = associatedCards.value.map((card) => card.id) || []
        const newlyAddedCardsIds = newlyAddedCards.value.map((card) => card.id)
        allCardsIds = [...new Set([...currentCards, ...newlyAddedCardsIds])]

        cardsToCheck = allCardsIds.filter(
          (cardId) => !originalPromotionCards.value.includes(cardId),
        )
      }

      const affectedPromotions = findCardsInOtherPromotions(cardsToCheck)

      if (affectedPromotions) {
        const getAllCards = () => {
          const allCardsSource = [
            ...cards.value,
            ...associatedCards.value,
            ...newlyAddedCards.value,
          ]
          return allCardsSource
        }

        const allAvailableCards = getAllCards()

        conflictList.value = Object.values(affectedPromotions).map((p) => {
          const cardNames = p.cardIds.map((cardId) => {
            const card = allAvailableCards.find((c) => c.id === cardId)

            return card
          })

          return {
            promoName: p.nom,
            cards: cardNames,
          }
        })

        showConflictModal.value = true
        return
      }

      await executeSavePromotion(allCardsIds)
    } catch (error) {
      if (error.response?.status === 401) {
        $q.notify({
          type: 'negative',
          message: 'Session expirée',
          caption: 'Veuillez vous reconnecter pour continuer',
          timeout: 5000,
        })

        return
      }

      if (error.response?.status === 400) {
        const errorMessage = error.response?.data?.message || 'Données invalides'
        $q.notify({
          type: 'negative',
          message: 'Erreur de validation',
          caption: errorMessage,
        })
        return
      }

      if (error.response?.status >= 500) {
        $q.notify({
          type: 'negative',
          message: 'Erreur serveur',
          caption: 'Le serveur rencontre un problème. Veuillez réessayer plus tard.',
        })
        return
      }

      $q.notify({
        type: 'negative',
        message: 'Erreur lors de la sauvegarde',
        caption:
          error.response?.data?.message || error.message || 'Une erreur inconnue est survenue.',
      })
    }
  }
}

const handleConfirmSave = async () => {
  loadingSave.value = true
  try {
    let allCardsIds = []
    if (editingPromotion.value.id === null) {
      allCardsIds = editingPromotion.value.carteIds || []
    } else {
      const currentCards = associatedCards.value.map((card) => card.id) || []
      const newlyAddedCardsIds = newlyAddedCards.value.map((card) => card.id)
      allCardsIds = [...new Set([...currentCards, ...newlyAddedCardsIds])]
    }

    await executeSavePromotion(allCardsIds)
    showConflictModal.value = false
  } catch (error) {
  } finally {
    loadingSave.value = false
  }
}

const executeSavePromotion = async (allCardsIds) => {
  try {
    const promotionData = {
      id: editingPromotion.value.id,
      title: editingPromotion.value.nom,
      description: editingPromotion.value.description,
      rate: editingPromotion.value.rabais,
      startDate: toLocalMidnightIso(editingPromotion.value.debut),
      endDate: toLocalMidnightIso(editingPromotion.value.fin),
      status: editingPromotion.value.statut,
      cardsIds: allCardsIds,
    }

    let savedPromotion
    if (editingPromotion.value.id === null) {
      savedPromotion = await promotionService.createPromotion(promotionData)
      $q.notify({
        type: 'positive',
        message: 'Promotion créée avec succès',
      })
    } else {
      if (allCardsIds.length === 0) {
        return
      }

      const promotionUpdateData = {
        id: editingPromotion.value.id,
        title: editingPromotion.value.nom,
        description: editingPromotion.value.description,
        rate: editingPromotion.value.rabais,
        startDate: toLocalMidnightIso(editingPromotion.value.debut),
        endDate: toLocalMidnightIso(editingPromotion.value.fin),
        status: editingPromotion.value.statut,
        cardsIds: allCardsIds,
      }

      const result = await promotionService.updatePromotion(promotionUpdateData)

      $q.notify({
        type: 'positive',
        message: 'Promotion mise à jour avec succès',
      })
    }

    await loadPromotions()

    editingPromotion.value = null
    newlyAddedCards.value = []

    if (selectedPromotionForCards.value) {
      loadAssociatedCards()
    }

    selectedCards.value = []
  } catch (error) {
    console.error('Erreur save:', error)
    if (error.response?.status === 401) {
      $q.notify({
        type: 'negative',
        message: 'Session expirée',
        caption: 'Veuillez vous reconnecter pour continuer',
        timeout: 5000,
      })
      throw error
    }

    $q.notify({
      type: 'negative',
      message: 'Erreur lors de la sauvegarde',
      caption: error.response?.data?.message || error.message || 'Erreur inconnue',
    })
    throw error
  }
}

const deletePromotion = async (promo) => {
  if (confirm('Êtes-vous sûr de vouloir supprimer cette promotion?')) {
    try {
      await promotionService.deletePromotion(promo.id)

      await loadPromotions()

      if (selectedPromotionForCards.value?.id === promo.id) {
        selectedPromotionForCards.value = null
        associatedCards.value = []
      }

      $q.notify({
        type: 'positive',
        message: 'Promotion supprimée avec succès !',
        position: 'top',
        timeout: 3000,
      })
    } catch (error) {
      let errorMessage = 'Erreur lors de la suppression'
      if (error.response?.status === 401) {
        errorMessage = 'Vous devez être connecté pour supprimer une promotion'
      } else if (error.response?.status === 403) {
        errorMessage = "Vous n'avez pas les permissions pour supprimer une promotion"
      } else if (error.response?.status === 404) {
        errorMessage = 'Promotion non trouvée'
      }

      $q.notify({
        type: 'negative',
        message: errorMessage,
        caption: error.response?.data?.message || error.message,
      })
    }
  }
}

const cancelEdit = async () => {
  const wasEditing = editingPromotion.value
  const promotionId = wasEditing?.id

  editingPromotion.value = null
  selectedCards.value = []
  validationErrors.value = {}

  if (wasEditing && promotionId !== null && originalPromotionCards.value.length > 0) {
    const originalPromo = promotions.value.find((p) => p.id === promotionId)
    if (originalPromo) {
      originalPromo.carteIds = [...originalPromotionCards.value]
    }

    if (selectedPromotionForCards.value && selectedPromotionForCards.value.id === promotionId) {
      if (originalPromotionCards.value.length === 0) {
        selectedPromotionForCards.value.carteIds = []
      } else {
        selectedPromotionForCards.value.carteIds = [...originalPromotionCards.value]
      }

      if (!loadingAssociatedCards.value) {
        loadAssociatedCards().catch(() => {})
      } else {
        const waitForLoading = async () => {
          while (loadingAssociatedCards.value) {
            await new Promise((resolve) => setTimeout(resolve, 100))
          }
          if (selectedPromotionForCards.value) {
            await loadAssociatedCards()
          }
        }
        waitForLoading().catch(() => {
          // Ignorer les erreurs
        })
      }
    }
  } else if (selectedPromotionForCards.value && !loadingAssociatedCards.value) {
    loadAssociatedCards().catch(() => {})
  }

  await nextTick()

  originalPromotionCards.value = []
  newlyAddedCards.value = []
}

// Pagination
const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) currentPage.value = page
}

const goToPreviousPage = () => {
  if (currentPage.value > 1) changePage(currentPage.value - 1)
}

const goToNextPage = () => {
  if (currentPage.value < totalPages.value) changePage(currentPage.value + 1)
}

const changeCardsPage = async (page) => {
  const maxPage = totalCardsPages.value || 1
  const validPage = Math.max(1, Math.min(page, maxPage))

  if (validPage >= 1) {
    currentCardsPage.value = validPage
  }
}

const goToPreviousCardsPage = () => {
  if (currentCardsPage.value > 1) changeCardsPage(currentCardsPage.value - 1)
}

const goToNextCardsPage = () => {
  if (currentCardsPage.value < totalCardsPages.value) changeCardsPage(currentCardsPage.value + 1)
}

// Cartes
const togglePromotionCards = async (promo) => {
  if (selectedPromotionForCards.value?.id === promo.id) {
    selectedPromotionForCards.value = null
    associatedCards.value = []
    selectedCards.value = []
  } else {
    selectedPromotionForCards.value = promo
    selectedCards.value = []

    await loadAssociatedCards()
  }
}

const handleUpdateSelectedCards = (cardIds) => {}

const toggleCardSelection = (cardId, event) => {
  if (!editingPromotion.value) {
    return
  }

  event?.stopPropagation()
}

const removeSelectedCards = (cardsToRemove = null) => {
  const cardIdsToDelete = cardsToRemove || []

  if (cardIdsToDelete.length === 0) {
    return
  }

  if (selectedPromotionForCards.value) {
    const originalPromo = promotions.value.find((p) => p.id === selectedPromotionForCards.value.id)
    if (originalPromo) {
      originalPromo.carteIds = originalPromo.carteIds.filter((id) => !cardIdsToDelete.includes(id))
    }

    selectedPromotionForCards.value.carteIds = selectedPromotionForCards.value.carteIds.filter(
      (id) => !cardIdsToDelete.includes(id),
    )

    associatedCards.value = associatedCards.value.filter(
      (card) => !cardIdsToDelete.includes(card.id),
    )

    newlyAddedCards.value = newlyAddedCards.value.filter(
      (card) => !cardIdsToDelete.includes(card.id),
    )

    if (editingPromotion.value && editingPromotion.value.id === null) {
      selectedCards.value = selectedCards.value.filter((card) => !cardIdsToDelete.includes(card.id))
    }

    if (
      editingPromotion.value &&
      editingPromotion.value.id === selectedPromotionForCards.value.id
    ) {
      const currentCardIds = associatedCards.value.map((card) => card.id)
      const newlyAddedCardIds = newlyAddedCards.value.map((card) => card.id)
      editingPromotion.value.carteIds = [...new Set([...currentCardIds, ...newlyAddedCardIds])]

      const totalCards = associatedCards.value.length + newlyAddedCards.value.length
      const maxPage = Math.ceil(totalCards / cardsPerPage.value) || 1
      if (currentCardsPage.value > maxPage) {
        currentCardsPage.value = maxPage
      }
    }
  }
}

const closeCatalog = () => {
  showCatalog.value = false
}

const getAddedCards = () => {
  if (!editingPromotion.value) return []

  if (editingPromotion.value.id === null) {
    return editingPromotion.value.carteIds || []
  }

  const existingCards = associatedCards.value.map((card) => card.id)
  const newCards = editingPromotion.value.carteIds || []

  return [...new Set([...existingCards, ...newCards])]
}

const addCardToEditingPromotion = (card) => {
  if (!editingPromotion.value) return
  if (!editingPromotion.value.carteIds) editingPromotion.value.carteIds = []
  if (editingPromotion.value.carteIds.includes(card.id)) return

  editingPromotion.value.carteIds.push(card.id)

  if (!selectedCards.value.some((selectedCard) => selectedCard.id === card.id)) {
    const cardWithStatus = { ...card, ...getCardStatus(card.stock) }
    selectedCards.value.push(cardWithStatus)

    selectedCards.value = [...selectedCards.value]
  }

  // Mettre à jour associatedCards
  if (!associatedCards.value.some((associatedCard) => associatedCard.id === card.id)) {
    const cardWithStatus = { ...card, ...getCardStatus(card.stock) }
    associatedCards.value.push(cardWithStatus)

    associatedCards.value = [...associatedCards.value]
  }

  // Mettre à jour si c'est la même promotion
  if (selectedPromotionForCards.value && editingPromotion.value) {
    if (
      editingPromotion.value.id === null ||
      selectedPromotionForCards.value.id === editingPromotion.value.id
    ) {
      selectedPromotionForCards.value.carteIds = [...editingPromotion.value.carteIds]
    }
  }
}

const resetCatalogFilters = () => {
  catalogFilters.value = {
    couleur: [],
    rarete: [],
    prixMin: null,
    prixMax: null,
    disponibilite: [],
    langue: [],
    type: [],
    sousType: [],
    serie: [],
  }
  catalogSearch.value = ''
  catalogSearchSerie.value = ''
  catalogSearchLangue.value = ''
  catalogSearchType.value = ''
  catalogSort.value = ''
}

const loadAssociatedCards = async () => {
  if (!selectedPromotionForCards.value) {
    associatedCards.value = []
    return
  }

  loadingAssociatedCards.value = true

  try {
    if (selectedPromotionForCards.value.id === null) {
      // Pour les nouvelles promotions, utiliser selectedCards qui contient déjà les objets complets
      // Ne pas filtrer depuis cards.value qui peut être incomplet
      if (selectedCards.value && selectedCards.value.length > 0) {
        // Utiliser directement selectedCards qui contient les objets complets de toutes les cartes ajoutées
        associatedCards.value = [...selectedCards.value]
      } else if (
        selectedPromotionForCards.value.carteIds &&
        selectedPromotionForCards.value.carteIds.length > 0
      ) {
        // Si selectedCards est vide mais qu'il y a des IDs, essayer de charger depuis cards.value
        if (cards.value.length > 0) {
          const associatedCardsData = cards.value.filter((card) =>
            selectedPromotionForCards.value.carteIds.includes(card.id),
          )
          associatedCards.value = associatedCardsData
        } else {
          await loadCards()
          const associatedCardsData = cards.value.filter((card) =>
            selectedPromotionForCards.value.carteIds.includes(card.id),
          )
          associatedCards.value = associatedCardsData
        }
      } else {
        associatedCards.value = []
      }
    } else {
      try {
        const backendCards = await promotionService.getPromotionCards(
          selectedPromotionForCards.value.id,
        )

        if (backendCards && Array.isArray(backendCards) && backendCards.length > 0) {
          const allCardIds = backendCards.map((card) => card.id || card)
          originalPromotionCards.value = allCardIds

          const { cardService } = await import('@/services/CardService.js')

          const formattedCards = await Promise.all(
            backendCards.map(async (card) => {
              try {
                const fullCard = await cardService.getCard(card.id)

                let setName = 'Set inconnu'
                if (fullCard.setName) {
                  setName = fullCard.setName
                } else if (fullCard.setId) {
                  setName = `Set ${fullCard.setId.substring(0, 8)}`
                }

                let cardColor = 'Inconnue'
                if (fullCard.colors) {
                  cardColor = fullCard.colors
                } else if (fullCard.manaCost) {
                  const colors = fullCard.manaCost.match(/[WUBRG]/g) || []
                  if (colors.length > 0) {
                    const colorMap = { W: 'Blanc', U: 'Bleu', B: 'Noir', R: 'Rouge', G: 'Vert' }
                    cardColor = [...new Set(colors)].map((c) => colorMap[c]).join(', ')
                  }
                }

                let totalQuantity = 0
                if (fullCard.languageQuantities && fullCard.languageQuantities.length > 0) {
                  totalQuantity = fullCard.languageQuantities.reduce(
                    (total, lang) => total + (lang.foilQuantity || 0) + (lang.nonFoilQuantity || 0),
                    0,
                  )
                } else {
                  totalQuantity = fullCard.totalQuantity || 0
                }

                return {
                  id: fullCard.id,
                  nom: fullCard.name,
                  serie: setName,
                  numero: fullCard.number,
                  rarete: fullCard.rarity,
                  couleur: cardColor,
                  prix: fullCard.price,
                  prixFoil: fullCard.foilPrice,
                  hasFoil: fullCard.hasFoil,
                  stock: totalQuantity,
                  image: `https://api.scryfall.com/cards/${fullCard.scryfallId}?format=image`,
                  type: fullCard.type || '',
                  disponibilite: fullCard.disponibilite || '',
                  langue: fullCard.langue || '',
                  manaCost: fullCard.manaCost,
                  text: fullCard.text,
                  power: fullCard.power,
                  toughness: fullCard.toughness,
                }
              } catch (error) {
                let totalQuantity = 0
                if (card.languageQuantities && card.languageQuantities.length > 0) {
                  totalQuantity = card.languageQuantities.reduce(
                    (total, lang) => total + (lang.foilQuantity || 0) + (lang.nonFoilQuantity || 0),
                    0,
                  )
                } else {
                  totalQuantity = card.totalQuantity || 0
                }

                return {
                  id: card.id,
                  nom: card.name,
                  serie: `Set ${card.setId?.substring(0, 8) || 'Inconnu'}`,
                  numero: card.number,
                  rarete: card.rarity,
                  couleur: 'Inconnue',
                  prix: card.price,
                  prixFoil: card.foilPrice,
                  hasFoil: card.hasFoil,
                  stock: totalQuantity,
                  image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image`,
                  type: card.type || '',
                  disponibilite: card.disponibilite || '',
                  langue: card.langue || '',
                  manaCost: card.manaCost,
                  text: card.text,
                  power: card.power,
                  toughness: card.toughness,
                }
              }
            }),
          )

          associatedCards.value = formattedCards
        } else {
          associatedCards.value = []
          originalPromotionCards.value = []
        }
      } catch (error) {
        associatedCards.value = []
        originalPromotionCards.value = []
      }
    }
  } finally {
    loadingAssociatedCards.value = false
  }
}

const handleCardsSearch = (searchTerm) => {
  cardsSearchTerm.value = searchTerm
  currentCardsPage.value = 1
}

// watcher pour sous-type
watch(
  () => catalogFilters.value.type,
  async (newType) => {
    try {
      catalogFilters.value.sousType = ''
      await loadSubtypes()
    } catch (error) {
      availableSubtypes.value = []
    }
  },
)

// Watcher pour edit
watch(
  () => editingPromotion.value,
  (newEditingPromotion, oldEditingPromotion) => {
    if (newEditingPromotion && newEditingPromotion.id) {
      const promotionIndex = promotions.value.findIndex((p) => p.id === newEditingPromotion.id)
      if (promotionIndex !== -1) {
        promotions.value[promotionIndex] = {
          ...promotions.value[promotionIndex],
          nom: newEditingPromotion.nom,
          statut: newEditingPromotion.statut,
          debut: newEditingPromotion.debut,
          fin: newEditingPromotion.fin,
          rabais: newEditingPromotion.rabais,
          description: newEditingPromotion.description,
        }
      } else {
      }
    }
  },
  { deep: true },
)

// Watcher pour le statut
watch(
  () => editingPromotion.value?.statut,
  (newStatut, oldStatut) => {
    if (editingPromotion.value && editingPromotion.value.id && newStatut !== oldStatut) {
      const promotionIndex = promotions.value.findIndex((p) => p.id === editingPromotion.value.id)
      if (promotionIndex !== -1) {
        promotions.value[promotionIndex].statut = newStatut
      }
    }
  },
)

// Watcher taille ecran
watch(
  () => $q.screen.width,
  (newWidth) => {
    if ($q.screen.gt.md && showMobileFiltersPromo.value) {
      showMobileFiltersPromo.value = false
    }
  },
)
</script>

<template>
  <q-page class="page-promotion">
    <div class="title-section">
      <div class="text-h5 text-bold">Promotions</div>
      <q-btn
        v-if="!$q.screen.gt.sm"
        color="brown-2"
        label="Créer"
        class="btn-create-mobile rounded-3 shadow"
        unelevated
        no-caps
        @click="createNewPromotion"
      />
    </div>

    <!-- Desktop - Barre de recherche -->
    <div v-if="$q.screen.gt.sm" class="search-section">
      <div class="row items-stretch no-wrap q-gutter-sm search-bar-container">
        <q-input
          v-model="searchTerm"
          placeholder="Rechercher une promotion..."
          outlined
          dense
          class="col search-input"
        />
        <q-btn
          color="brown-8"
          label="Créer une nouvelle promotion"
          class="col-auto text-white search-btn promotion-btn"
          unelevated
          no-caps
          @click="createNewPromotion"
        />
      </div>
    </div>

    <!-- Desktop - Barre de filtres -->
    <div v-if="$q.screen.gt.sm" class="filters-bar d-flex align-items-center gap-2 p-3 text-light">
      <FilterDropdown
        label="Statut"
        :options="['Active', 'Inactive']"
        :selected="filters.statut"
        @update:selected="
          (val) => {
            filters.statut = val
            currentPage = 1
          }
        "
      />

      <div class="d-flex align-items-center gap-2">
        <input
          type="number"
          min="0"
          v-model.number="rabaisMin"
          @input="sanitizeRabais(true)"
          class="form-control form-control-sm"
          placeholder="Rabais Min"
          aria-label="Rabais min"
        />
        <input
          type="number"
          min="0"
          v-model.number="rabaisMax"
          @input="sanitizeRabais(false)"
          class="form-control form-control-sm"
          placeholder="Rabais Max"
          aria-label="Rabais max"
        />
      </div>

      <div class="ms-2">
        <select v-model="sortBy" class="form-select form-select-sm">
          <option value="" hidden>Trier</option>
          <option value="name-asc">Nom (A-Z)</option>
          <option value="name-desc">Nom (Z-A)</option>
          <option value="debut-asc">Date début (asc)</option>
          <option value="debut-desc">Date début (desc)</option>
          <option value="fin-asc">Date fin (asc)</option>
          <option value="fin-desc">Date fin (desc)</option>
          <option value="rabais-asc">Rabais (asc)</option>
          <option value="rabais-desc">Rabais (desc)</option>
        </select>
      </div>

      <div class="d-flex gap-2 ms-auto">
        <button class="btn btn-danger btn-sm" @click="clearFilters()">Effacer les filtres</button>
      </div>
    </div>

    <!-- Mobile section Barre recherche -->
    <div v-else class="mobile-filters-bar">
      <div class="mobile-search-container">
        <input
          type="search"
          v-model="searchTerm"
          placeholder="Rechercher une promotion..."
          class="mobile-search-input"
        />
        <button
          class="mobile-filters-btn"
          @click="showMobileFiltersPromo = !showMobileFiltersPromo"
        >
          <span class="filter-icon">☰</span>
          <span class="filter-text">Filtres</span>
          <span v-if="activeFiltersCountPromo > 0" class="filter-badge">{{
            activeFiltersCountPromo
          }}</span>
        </button>
      </div>

      <div v-if="activeFiltersCountPromo > 0" class="mobile-clear-filters">
        <button class="clear-filters-btn" @click="clearFilters()">Effacer les filtres</button>
      </div>
    </div>

    <!-- Mobile - Sidebar filtres -->
    <div
      v-if="showMobileFiltersPromo && $q.screen.lt.lg"
      class="mobile-filters-overlay-promo"
      @click="showMobileFiltersPromo = false"
    ></div>
    <div
      class="mobile-filters-sidebar-promo"
      :class="{ open: showMobileFiltersPromo && $q.screen.lt.lg }"
    >
      <div class="sidebar-header">
        <h5 class="sidebar-title">Filtres</h5>
        <button class="sidebar-close-btn" @click="showMobileFiltersPromo = false">✕</button>
      </div>
      <hr class="sidebar-separator" />
      <div class="sidebar-content">
        <!-- Statut -->
        <div class="filter-section">
          <h6 class="filter-title">Statut</h6>
          <div class="filter-options">
            <div
              class="checkbox-label"
              :class="{ selected: filters.statut.includes('Active') }"
              @click="toggleFilter('statut', 'Active', $event)"
            >
              <input type="checkbox" :checked="filters.statut.includes('Active')" />
              <span class="checkbox-text">Active</span>
            </div>
            <div
              class="checkbox-label"
              :class="{ selected: filters.statut.includes('Inactive') }"
              @click="toggleFilter('statut', 'Inactive', $event)"
            >
              <input type="checkbox" :checked="filters.statut.includes('Inactive')" />
              <span class="checkbox-text">Inactive</span>
            </div>
          </div>
        </div>

        <hr class="sidebar-separator" />

        <!-- Rabais -->
        <div class="filter-section">
          <h6 class="filter-title">Rabais</h6>
          <div class="filter-inputs-row">
            <input
              type="number"
              min="0"
              v-model.number="rabaisMin"
              @input="sanitizeRabais(true)"
              class="filter-number-input"
              placeholder="Min"
            />
            <input
              type="number"
              min="0"
              v-model.number="rabaisMax"
              @input="sanitizeRabais(false)"
              class="filter-number-input"
              placeholder="Max"
            />
          </div>
        </div>

        <hr class="sidebar-separator" />

        <!-- Tri -->
        <div class="filter-section">
          <h6 class="filter-title">Trier par</h6>
          <select v-model="sortBy" class="filter-select">
            <option value="" hidden>Trier</option>
            <option value="name-asc">Nom (A-Z)</option>
            <option value="name-desc">Nom (Z-A)</option>
            <option value="debut-asc">Date début (asc)</option>
            <option value="debut-desc">Date début (desc)</option>
            <option value="fin-asc">Date fin (asc)</option>
            <option value="fin-desc">Date fin (desc)</option>
            <option value="rabais-asc">Rabais (asc)</option>
            <option value="rabais-desc">Rabais (desc)</option>
          </select>
        </div>
      </div>

      <div class="sidebar-footer">
        <button class="reset-btn" @click="clearFilters()">Effacer les filtres</button>
        <button class="close-btn" @click="showMobileFiltersPromo = false">Fermer</button>
      </div>
    </div>

    <div class="separator-container">
      <hr class="separator" />
    </div>

    <!-- Indicateur de chargement des promotions -->
    <div v-if="loadingPromotionsService" class="text-center q-pa-lg loading-indicator">
      <q-spinner-dots size="40px" color="primary" />
      <div class="q-mt-sm">Chargement des promotions...</div>
    </div>

    <!-- Contenu principal  -->
    <div v-if="!loadingPromotionsService">
      <!-- Formulaire d'édition -->
      <EditionBoard
        v-if="editingPromotion"
        :editingPromotion="editingPromotion"
        :validation-errors="validationErrors"
        :series-options="catalogSeriesOptions"
        @save="savePromotion"
        @cancel="cancelEdit"
        @update:editingPromotion="(value) => (editingPromotion = value)"
        @add-series="handleAddSeries"
      />

      <!-- Desktop - Tableau des promotions -->
      <PromotionBoard
        v-if="!editingPromotion && promotions.length > 0 && $q.screen.gt.md"
        :paginatedPromotions="paginatedPromotions"
        :currentPage="currentPage"
        :totalPages="totalPages"
        :hasNextPage="hasNextPage"
        :hasPreviousPage="hasPreviousPage"
        :selectedPromotionForCards="selectedPromotionForCards"
        :editingPromotion="editingPromotion"
        :loading="loadingPromotions"
        @edit="editPromotion"
        @delete="deletePromotion"
        @toggle-selection="togglePromotionCards"
        @change-page="changePage"
      />

      <!-- Mobile - Cartes des promotions -->
      <div
        v-else-if="!editingPromotion && promotions.length > 0 && $q.screen.lt.lg"
        class="mobile-promotions-container"
      >
        <div
          v-for="promo in paginatedPromotions"
          :key="promo.id"
          class="mobile-promotion-card"
          :class="{ selected: selectedPromotionForCards?.id === promo.id }"
        >
          <div class="mobile-promotion-header" @click="togglePromotionCards(promo)">
            <div class="promotion-selection-icon">
              <q-icon
                :name="
                  selectedPromotionForCards?.id === promo.id
                    ? 'check_circle'
                    : 'radio_button_unchecked'
                "
                :color="selectedPromotionForCards?.id === promo.id ? 'green' : 'grey-5'"
                size="24px"
              />
            </div>
            <div class="promotion-main-info">
              <div class="promotion-name">{{ promo.nom }}</div>
              <div class="promotion-rabais">-{{ promo.rabais }}%</div>
            </div>
            <q-badge
              :color="
                getDisplayStatus(promo) === 'Active'
                  ? 'green'
                  : getDisplayStatus(promo) === 'En attente'
                    ? 'orange'
                    : getDisplayStatus(promo) === 'Expirée'
                      ? 'red'
                      : 'grey'
              "
              class="promotion-status-badge"
            >
              {{ getDisplayStatus(promo) }}
            </q-badge>
          </div>

          <div class="mobile-promotion-details">
            <div class="detail-row">
              <span class="detail-label">Date début:</span>
              <span class="detail-value">{{ formatDate(promo.debut) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Date expiration:</span>
              <span class="detail-value">{{ formatDate(promo.fin) }}</span>
            </div>
            <div v-if="promo.description" class="detail-row description-row">
              <span class="detail-label">Description:</span>
              <span class="detail-value description-text">{{ promo.description }}</span>
            </div>
          </div>

          <div class="mobile-promotion-actions">
            <q-btn
              color="white-8"
              label="Modifier"
              size="sm"
              class="text-white mobile-action-btn"
              style="background-color: #424242 !important; color: #ffffff !important"
              @click.stop="editPromotion(promo)"
            />
            <q-btn
              color="red-8"
              label="Supprimer"
              size="sm"
              class="text-white mobile-action-btn"
              @click.stop="deletePromotion(promo)"
            />
          </div>
        </div>

        <!-- Pagination Mobile -->
        <div v-if="totalPages > 1" class="mobile-pagination-container">
          <button
            class="mobile-pagination-btn"
            :disabled="!hasPreviousPage"
            @click="changePage(currentPage - 1)"
          >
            ‹
          </button>
          <span class="mobile-pagination-info"> Page {{ currentPage }} / {{ totalPages }} </span>
          <button
            class="mobile-pagination-btn"
            :disabled="!hasNextPage"
            @click="changePage(currentPage + 1)"
          >
            ›
          </button>
        </div>
      </div>

      <!-- Desktop - Message pas de promotions -->
      <div
        v-if="!editingPromotion && promotions.length === 0 && !loadingPromotions && $q.screen.gt.md"
        class="text-center q-pa-lg"
      >
        <q-icon name="local_offer" size="64px" color="grey-5" class="q-mb-md" />
        <div class="text-h6 text-grey-7 q-mb-sm">Aucune promotion trouvée</div>
        <div class="text-body2 text-grey-6">
          Il n'y a actuellement aucune promotion dans le système.
          <br />
          Cliquez sur "Nouvelle promotion" pour en créer une.
        </div>
      </div>

      <!-- Mobile - Message pas de promotions -->
      <div
        v-if="!editingPromotion && promotions.length === 0 && !loadingPromotions && $q.screen.lt.lg"
        class="mobile-empty-state"
      >
        <q-icon name="local_offer" size="48px" color="grey-5" class="empty-icon" />
        <div class="empty-title">Aucune promotion trouvée</div>
        <div class="empty-message">Il n'y a actuellement aucune promotion dans le système.</div>
        <q-btn
          color="brown-8"
          label="Créer une promotion"
          class="empty-action-btn"
          unelevated
          no-caps
          @click="createNewPromotion"
        />
      </div>
    </div>

    <!-- Tableau des cartes associées -->
    <CardsBoard
      v-if="(selectedPromotionForCards || editingPromotion) && !loadingPromotionsService"
      :selectedPromotionForCards="selectedPromotionForCards"
      :associatedCards="filteredAssociatedCards"
      :paginatedCards="paginatedCards"
      :selectedCards="selectedCards"
      :editingPromotion="editingPromotion"
      :currentCardsPage="currentCardsPage"
      :totalCardsPages="totalCardsPages"
      :hasNextCardsPage="hasNextCardsPage"
      :hasPreviousCardsPage="hasPreviousCardsPage"
      :loading="loadingAssociatedCards"
      :validation-errors="validationErrors"
      :original-promotion-cards="originalPromotionCards"
      @open-catalog="openCatalog"
      @remove-selected="removeSelectedCards"
      @add-card="addCardToEditingPromotion"
      @toggle-selection="toggleCardSelection"
      @change-page="changeCardsPage"
      @search-cards="handleCardsSearch"
      @update-selected-cards="handleUpdateSelectedCards"
    />

    <!-- Modal catalogue -->
    <ModalCards
      :show="showCatalog"
      :catalogSearch="catalogSearch"
      :catalogSort="catalogSort"
      :catalogFilters="catalogFilters"
      :catalogSearchSerie="catalogSearchSerie"
      :catalogSeriesOptions="catalogSeriesOptions"
      :filteredCatalogSeries="filteredCatalogSeries"
      :catalogSearchLangue="catalogSearchLangue"
      :filteredCatalogLangues="filteredCatalogLangues"
      :catalogSearchType="catalogSearchType"
      :filteredCatalogTypes="filteredCatalogTypes"
      :filteredCatalog="filteredCatalog"
      :catalogPaginated="catalogPaginated"
      :catalogPage="catalogPage"
      :totalCatalogPages="totalCatalogPages"
      :loading="catalogLoading"
      :filterOptions="filterOptions"
      :availableSubtypes="availableSubtypes"
      :loadingSubtypes="loadingSubtypes"
      :addedCards="getAddedCards()"
      :is-card-in-another-promotion="isCardInAnotherPromotion"
      @close="closeCatalog"
      @add-card="handleAddCard"
      @add-series="handleAddSeriesFromModal"
      @change-page="handleCatalogPageChange"
      @update:filters="handleCatalogFilters"
      @update:searchSerie="(value) => (catalogSearchSerie = value)"
      @update:searchLangue="(value) => (catalogSearchLangue = value)"
      @update:searchType="(value) => (catalogSearchType = value)"
      @reset-filters="handleCatalogReset"
      @update:sort="handleCatalogSort"
      @update:catalogSearch="
        (value) => {
          catalogSearch = value
          handleCatalogSearchInput()
        }
      "
    />

    <!-- MODAL DE CONFLIT -->
    <q-dialog v-model="showConflictModal" persistent>
      <q-card
        style="
          min-width: 600px;
          max-width: 80vw;
          max-height: 80vh;
          display: flex;
          flex-direction: column;
          overflow: hidden;
        "
      >
        <q-card-section class="row items-center bg-primary text-white q-py-sm no-wrap">
          <div class="text-h6">Conflits de promotion détectés</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup class="text-white" />
        </q-card-section>

        <q-card-section
          class="q-pt-md modal-scroll-area"
          style="overflow-y: auto; overflow-x: hidden; flex: 1"
        >
          <div class="text-body1 q-mb-md">
            Les cartes suivantes sont déjà présentes dans d'autres promotions actives et en seront
            retirées si vous continuez :
          </div>

          <div v-for="(conflict, index) in conflictList" :key="index" class="q-mb-md">
            <div
              class="text-subtitle1 text-weight-bold q-mb-xs text-orange-9 promotion-conflict-title"
            >
              <q-icon name="warning" class="q-mr-xs" color="warning" />
              Promotion : {{ conflict.promoName }}
            </div>
            <div class="q-pl-md">
              <q-list dense bordered separator class="rounded-borders">
                <q-item v-for="card in conflict.cards" :key="card.id">
                  <q-item-section avatar>
                    <q-img :src="card.image" style="width: 40px; height: 56px" />
                  </q-item-section>
                  <q-item-section>
                    <q-item-label>{{ card.nom }}</q-item-label>
                    <q-item-label caption> {{ card.serie }} #{{ card.numero }} </q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </div>
          </div>
        </q-card-section>

        <q-separator />

        <q-card-actions align="right" class="q-pa-md actions-bar">
          <q-btn
            unelevated
            label="Annuler"
            color="negative"
            text-color="white"
            v-close-popup
            :disable="loadingSave"
          />
          <q-btn
            unelevated
            no-caps
            no-wrap
            label="Confirmer et Sauvegarder"
            color="primary"
            @click="handleConfirmSave"
            :loading="loadingSave"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<style src="../../css/Promotions.css" scoped></style>

<style>
body:not(.body--dark) .page-promotion {
  background-color: #d1cbbb !important;
  min-height: 100vh;
}

body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .filters-bar {
  background-color: #d1cbbb !important;
  color: black !important;
}

body:not(.body--dark) .filters-bar .dropdown-toggle {
  color: black !important;
}

body:not(.body--dark) .promotions-table {
  border: 2px solid black !important;
}

body:not(.body--dark) .promotions-table thead tr {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .promotions-table th {
  color: black !important;
  border-bottom: 2px solid white !important;
}

body:not(.body--dark) .promotions-table tbody tr {
  background-color: white !important;
  color: black;
}

body:not(.body--dark) .promotions-table tbody tr:hover {
  background-color: #f5f5f5 !important;
}

body:not(.body--dark) .mobile-promotion-card {
  background-color: white !important;
  border: 1px solid #a89c8d !important;
  color: black !important;
}

body:not(.body--dark) .mobile-promotion-header {
  background-color: #e8dfd4 !important;
  color: black !important;
  border-bottom: 1px solid #e0e0e0 !important;
}

body:not(.body--dark) .promotion-name {
  color: black !important;
}

body:not(.body--dark) .detail-label {
  color: #e8e2e2 !important;
}

body:not(.body--dark) .detail-value {
  color: rgb(255, 255, 255) !important;
}

body:not(.body--dark) .mobile-filters-bar {
  background-color: transparent !important;
}

body:not(.body--dark) input::placeholder {
  color: #666 !important;
  opacity: 1 !important;
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
</style>
