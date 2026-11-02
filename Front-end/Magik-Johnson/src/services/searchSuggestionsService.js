import { cardService } from '@/services/CardService'

/**
 *
 */
export class SearchSuggestionsService {
  constructor() {
    this.currentRequest = null
    this.suggestionsTimeout = null
  }

  /**
   *
   * @param {string} query
   * @param {number} debounceMs
   * @param {number} maxResults
   * @returns {Promise<string[]>}
   */
  async loadSuggestions(query, debounceMs = 300, maxResults = 5) {
    if (this.currentRequest) {
      this.currentRequest.cancelled = true
    }

    const trimmedQuery = query?.trim() || ''

    if (trimmedQuery.length < 3) {
      return []
    }

    const requestId = { cancelled: false }
    this.currentRequest = requestId

    try {
      const response = await cardService.getCards(1, 20, { searchQuery: trimmedQuery })

      if (requestId.cancelled) {
        return []
      }

      if (response && response.cards) {
        const uniqueNames = [...new Set(response.cards.map((card) => card.name))]

        const queryLower = trimmedQuery.toLowerCase()
        const sortedSuggestions = uniqueNames.sort((a, b) => {
          const aLower = a.toLowerCase()
          const bLower = b.toLowerCase()

          const aStarts = aLower.startsWith(queryLower)
          const bStarts = bLower.startsWith(queryLower)
          if (aStarts && !bStarts) return -1
          if (!aStarts && bStarts) return 1

          const aWordStarts = aLower.includes(' ' + queryLower) || aLower.includes('-' + queryLower)
          const bWordStarts = bLower.includes(' ' + queryLower) || bLower.includes('-' + queryLower)
          if (aWordStarts && !bWordStarts) return -1
          if (!aWordStarts && bWordStarts) return 1

          const aIndex = aLower.indexOf(queryLower)
          const bIndex = bLower.indexOf(queryLower)
          if (aIndex !== bIndex) {
            return aIndex - bIndex
          }

          return aLower.localeCompare(bLower)
        })

        return sortedSuggestions.slice(0, maxResults)
      }

      return []
    } catch (error) {
      if (!requestId.cancelled) {
        console.warn('Erreur lors du chargement des suggestions:', error)
      }
      return []
    }
  }

  /**
   *
   * @param {string} query
   * @param {Function} callback
   * @param {number} debounceMs
   */
  loadSuggestionsWithDebounce(query, callback, debounceMs = 300) {
    if (this.suggestionsTimeout) {
      clearTimeout(this.suggestionsTimeout)
    }

    const trimmedQuery = query?.trim() || ''

    if (trimmedQuery.length < 3) {
      callback([])
      return
    }

    this.suggestionsTimeout = setTimeout(async () => {
      const suggestions = await this.loadSuggestions(trimmedQuery, 0)
      callback(suggestions)
    }, debounceMs)
  }

  /**
   *
   */
  cancelCurrentRequest() {
    if (this.currentRequest) {
      this.currentRequest.cancelled = true
      this.currentRequest = null
    }
    if (this.suggestionsTimeout) {
      clearTimeout(this.suggestionsTimeout)
      this.suggestionsTimeout = null
    }
  }
}

export const searchSuggestionsService = new SearchSuggestionsService()
