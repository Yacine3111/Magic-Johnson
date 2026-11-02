import { apiClient } from '@/api/Axios'

export const LockCustomer = async (employeeId) => {
  const response = await apiClient.post(`/client/lockAccount/${employeeId}`)
  return response
}

export const UnLockCustomer = async (employeeId) => {
  const response = await apiClient.post(`/client/unLockAccount/${employeeId}`)
  return response
}

export const ListCustomer = async () => {
  const response = await apiClient.get(`/client/list`)
  return response.data
}

export const SearchCustomerByEmail = async (emailQuery, limit = 10) => {
  const response = await apiClient.get(
    `/client/search?email=${encodeURIComponent(emailQuery)}&limit=${limit}`,
  )
  return response.data
}

export const GetCustomerById = async (id) => {
  const response = await apiClient.get(`/client/get/${id}`)
  return response.data
}

export const UpdateCustomer = async (data) => {
  const response = await apiClient.post('/client/update', data)
  return response.data
}

export const GetClientAddresses = async (userId) => {
  const response = await apiClient.get(`/address/client/${userId}`)
  return response.data
}

export const UpdateClientAddress = async (addressId, addressData) => {
  const response = await apiClient.put(`/address/client/update/${addressId}`, addressData)
  return response.data
}

export const AddClientAddress = async (userId, addressData) => {
  const response = await apiClient.post(`/address/client/${userId}`, addressData)
  return response.data
}
