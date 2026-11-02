import { apiClient } from '@/api/Axios'

export const AddAddress = async (address) => {
  await apiClient.post(`/address`, address)
}

export const AddAddressForClient = async (userId, address) => {
  await apiClient.post(`/address/client/${userId}`, address)
}

export const getAddress = async () => {
  const response = await apiClient.get(`/address`)
  return response.data
}

export const getCanada = async () => {
  const response = await apiClient.get(`/address/canada`)
  return response.data
}

export const getCustomerAddresses = async (userId) => {
  const response = await apiClient.get(`/address/client/${userId}`)
  return response.data
}

export const UpdateAddress = async (addressId, address) => {
  await apiClient.put(`/address/update/${addressId}`, address)
}

export const DeleteAddress = async (addressId) => {
  await apiClient.delete(`/address/delete/${addressId}`)
}
