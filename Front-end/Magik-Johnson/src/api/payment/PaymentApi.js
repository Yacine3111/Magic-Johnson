import { apiClient } from '@/api/Axios'

export const CreateCheckoutCard = async (request) => {
  const response = await apiClient.post('/payment/card', request)

  return response.data
}

export const CreateCheckoutCash = async (request) => {
  const response = await apiClient.post('/payment/cash', request)

  return response.data
}

export const CreateOrder = async (request) => {
  return await apiClient.post('/payment/order', request)
}
