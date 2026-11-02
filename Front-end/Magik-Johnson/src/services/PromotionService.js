import { apiClient } from '../api/Axios.js'

class PromotionService {
  // Récupérer les promotions actives
  async getActivePromotions() {
    try {
      const response = await apiClient.get('/discount/active')
      return response.data
    } catch (error) {
      console.error('Erreur lors de la récupération des promotions actives:', error)
      throw error
    }
  }

  // Récupérer toutes les promotions
  async getAllPromotions() {
    try {
      const response = await apiClient.get('/discount/all')
      return response.data
    } catch (error) {
      console.error('Erreur lors de la récupération de toutes les promotions:', error)
      throw error
    }
  }

  // Créer une nouvelle promotion
  async createPromotion(promotionData) {
    try {
      const response = await apiClient.post('/discount', promotionData)
      return response.data
    } catch (error) {
      console.error('Erreur lors de la création de la promotion:', error)
      throw error
    }
  }

  // Mettre à jour une promotion
  async updatePromotion(promotionData) {
    try {
      const response = await apiClient.put('/discount', promotionData)
      return response.data
    } catch (error) {
      console.error('Erreur lors de la mise à jour de la promotion:', error)
      throw error
    }
  }

  // Supprimer une promotion
  async deletePromotion(promotionId) {
    try {
      const response = await apiClient.delete(`/discount/${promotionId}`)
      return response.data
    } catch (error) {
      console.error('Erreur lors de la suppression de la promotion:', error)
      throw error
    }
  }

  // Récupérer les cartes d'une promotion
  async getPromotionCards(promotionId) {
    try {
      const response = await apiClient.get(`/discount/${promotionId}/cards`)
      return response.data
    } catch (error) {
      console.error('Erreur lors de la récupération des cartes de la promotion:', error)
      throw error
    }
  }

  // Ajouter des cartes à une promotion existante
  async addCardsToPromotion(promotionId, cardIds) {
    try {
      const response = await apiClient.post(`/discount/${promotionId}/cards`, cardIds)
      return response.data
    } catch (error) {
      console.error("Erreur lors de l'ajout des cartes à la promotion:", error)
      throw error
    }
  }
}

export const promotionService = new PromotionService()
