import { apiClient } from '@/api/Axios'

class CartService {
  async getCart() {
    try {
      const response = await apiClient.get('/Cart')
      return response.data
    } catch (error) {
      throw error
    }
  }

  async updateCart(cartData) {
    const payload = cartData.items
      ? { items: cartData.items }
      : {
          items: Object.entries(cartData.cardQuantityDictionary || {}).map(
            ([cardId, quantity]) => ({
              cardId,
              quantity,
              language,
              isFoil,
            }),
          ),
        }

    const response = await apiClient.post('/Cart', payload)

    return response.data
  }

  async deleteCardFromCart(cardId, languageName, isFoil) {
    const response = await apiClient.delete(`/Cart/${cardId}`, {
      data: { languageName, isFoil },
      headers: { 'Content-Type': 'application/json' },
    })
    return response.data
  }
}

export const cartService = new CartService()
