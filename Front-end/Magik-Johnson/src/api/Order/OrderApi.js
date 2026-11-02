import { apiClient } from '@/api/Axios'

export const GetOrders = async () => {
  const response = await apiClient.get('/order')
  return response.data
}

export const GetAllOrders = async () => {
  const response = await apiClient.get('/order/all')
  return response.data
}

export const GetOrderById = async (orderId, onlyNonRefunded = false) => {
  const response = await apiClient.get(`/Order/${orderId}`, { params: { onlyNonRefunded } })
  return response.data
}

export const GetOrderItemsById = async (orderId) => {
  const response = await apiClient.get(`/Order/${orderId}/items`)
  return response.data
}

export const GetFilteredOrders = async (
  statusList = [],
  startDate,
  endDate,
  employeeOnly = false,
  startTime,
  endTime,
) => {
  const params = new URLSearchParams()

  statusList.forEach((s) => params.append('status', s))

  if (startDate) params.append('startDate', startDate)
  if (endDate) params.append('endDate', endDate)
  if (startTime) params.append('startTime', startTime)
  if (endTime) params.append('endTime', endTime)
  if (employeeOnly) params.append('employeeOnly', 'true')

  const queryString = params.toString()
  const response = await apiClient.get(`/order/filtered${queryString ? `?${queryString}` : ''}`)
  return response.data
}

export const GetOrderStatus = async () => {
  const response = await apiClient.get('/orderStatus')
  return response.data
}

export const UpdateOrderStatus = async (updates) => {
  const response = await apiClient.put(`/order/changeStatus`, updates)
  return response.status
}
