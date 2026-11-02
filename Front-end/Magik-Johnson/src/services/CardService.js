import { apiClient } from '@/api/Axios'

const API_BASE_URL = 'https://localhost:7179'

class CardService {
  async makeRequest(endpoint, params) {
    try {
      const response = await apiClient.get(`${endpoint}?${params}`)
      return response.data
    } catch (axiosError) {
      const fetchResponse = await fetch(`${API_BASE_URL}${endpoint}?${params}`)
      if (!fetchResponse.ok) {
        throw new Error(`HTTP ${fetchResponse.status}: ${fetchResponse.statusText}`)
      }
      return await fetchResponse.json()
    }
  }

  buildFilterParams(page, pageSize, filters) {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    })

    if (filters.searchQuery) params.append('search', filters.searchQuery)
    if (filters.sortBy) params.append('sortBy', filters.sortBy)
    if (filters.description) params.append('searchByDescription', filters.description)

    if (filters.priceMin !== null && filters.priceMin !== undefined) {
      params.append('priceMin', filters.priceMin.toString())
    }
    if (filters.priceMax !== null && filters.priceMax !== undefined) {
      params.append('priceMax', filters.priceMax.toString())
    }

    const arrayFilters = {
      serie: 'series',
      rarete: 'rarities',
      type: 'types',
      sousType: 'subtypes',
      couleur: 'colors',
      langue: 'languages',
      disponibilite: 'availabilities',
      finition: 'finition',
    }

    Object.entries(arrayFilters).forEach(([filterKey, paramName]) => {
      if (filters[filterKey]?.length > 0) {
        filters[filterKey].forEach((value) => params.append(paramName, value))
      }
    })

    return params
  }

  handleError(error) {
    if (error.code === 'ERR_NETWORK' || error.code === 'ECONNREFUSED') {
      const networkError = new Error('Backend non disponible.')
      networkError.isNetworkError = true
      throw networkError
    }

    if (error.response?.status === 404) {
      const notFoundError = new Error('API endpoint non trouvé')
      notFoundError.status = 404
      throw notFoundError
    }

    if (error.response?.status >= 500) {
      const serverError = new Error('Erreur serveur interne')
      serverError.status = error.response.status
      throw serverError
    }

    const genericError = new Error(error.message || 'Erreur de connexion')
    genericError.originalError = error
    throw genericError
  }

  async getCards(page = 1, pageSize = 18, filters = {}) {
    try {
      const params = this.buildFilterParams(page, pageSize, filters)
      return await this.makeRequest('/Cards/catalogue', params)
    } catch (error) {
      this.handleError(error)
    }
  }

  async getCard(id) {
    try {
      const response = await apiClient.get(`/Cards/${id}`)
      return response.data
    } catch (error) {
      throw error
    }
  }

  async getCardById(id) {
    try {
      const response = await apiClient.get(`/Cards/${id}`)
      return response
    } catch (error) {
      this.handleError(error)
    }
  }

  // Récupérer plusieurs cartes par leurs IDs
  async getCardsByIds(cardIds) {
    try {
      if (!cardIds || cardIds.length === 0) {
        return []
      }

      const params = new URLSearchParams()
      cardIds.forEach((id) => params.append('ids', id))

      const response = await apiClient.get(`/Cards/batch?${params}`)
      return response.data
    } catch (error) {
      throw error
    }
  }

  async getFilterOptions() {
    try {
      return await this.makeRequest('/Cards/filters', '')
    } catch (error) {
      throw error
    }
  }

  async getSubtypes(types = []) {
    if (types.length === 0) return { subtypes: [] }

    const params = new URLSearchParams()
    types.forEach((type) => params.append('types', type))

    const response = await apiClient.get(`/Cards/subtypes?${params}`)
    return response.data
  }

  async getInventoryCards(page = 1, pageSize = 18, filters = {}) {
    try {
      const params = this.buildFilterParams(page, pageSize, filters)
      return await this.makeRequest('/Cards/inventory', params)
    } catch (error) {
      console.error("Erreur lors de la récupération des cartes d'inventaire:", error)
      this.handleError(error)
    }
  }

  async updateCardLanguageQuantity(cardId, languageId, foilQuantity, nonFoilQuantity) {
    try {
      const response = await apiClient.put(`/Cards/${cardId}/language/${languageId}/quantity`, {
        foilQuantity,
        nonFoilQuantity,
      })
      return response.data
    } catch (error) {
      console.error(`Erreur lors de la mise à jour des quantités de la carte ${cardId}:`, error)
      this.handleError(error)
    }
  }

  async getTrendyCards() {
    try {
      const response = await apiClient.get(`/Cards/trendyCards`)
      return response
    } catch (error) {
      this.handleError(error)
    }
  }

  async getMostSoldCards(count = 4, days = 30) {
    try {
      const response = await apiClient.get(`/Cards/mostSold`, {
        params: {
          count,
          days,
        },
      })
      return response
    } catch (error) {
      this.handleError(error)
    }
  }

  async updateCardClickCount(id) {
    try {
      const response = await apiClient.put(`/Cards/${id}/clickCount`)
      return response.data
    } catch (error) {
      console.error('Erreur lors de la mise à jour du nombre de click:', error)
      this.handleError(error)
    }
  }

  async getCardScryfallVariants(cardId) {
    try {
      const response = await apiClient.get(`/Cards/${cardId}/variants`)
      return response
    } catch (error) {
      this.handleError(error)
    }
  }

  async getCardsByName(cardName) {
    try {
      const response = await apiClient.get(`/Cards/by-name/${cardName}`)
      return response
    } catch (error) {
      this.handleError(error)
    }
  }
  async getScryfallVariants(scryfallId) {
    try {
      const cardResponse = await fetch(`https://api.scryfall.com/cards/${scryfallId}`)
      if (!cardResponse.ok) throw new Error('Failed to fetch card from Scryfall')
      const cardData = await cardResponse.json()
      const oracleId = cardData.oracle_id

      const searchResponse = await fetch(
        `https://api.scryfall.com/cards/search?q=oracle_id:${oracleId}&unique=prints&include_multilingual=true&order=released`,
      )
      if (!searchResponse.ok) throw new Error('Failed to fetch variants from Scryfall')
      const searchData = await searchResponse.json()

      const variants = searchData.data.map((card) => ({
        id: card.id,
        scryfallId: card.id,
        language: this.mapScryfallLanguage(card.lang),
        set: card.set_name,
        price: card.prices?.eur || 0,
        foilPrice: card.prices?.eur_foil || 0,
      }))

      return { data: variants }
    } catch (error) {
      console.error(error)
      return { data: [] }
    }
  }

  mapScryfallLanguage(code) {
    const map = {
      en: 'English',
      es: 'Spanish',
      fr: 'French',
      de: 'German',
      it: 'Italian',
      pt: 'Portuguese',
      ja: 'Japanese',
      ko: 'Korean',
      ru: 'Russian',
      zhs: 'Chinese Simplified',
      zht: 'Chinese Traditional',
      he: 'Hebrew',
      la: 'Latin',
      grc: 'Ancient Greek',
      ar: 'Arabic',
      sa: 'Sanskrit',
      ph: 'Phyrexian',
    }
    return map[code] || 'English'
  }
}

export const cardService = new CardService()
