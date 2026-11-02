import { apiClient } from '@/api/Axios'

export const GetUser = async () => {
  const response = await apiClient.get(`/user`)
  return response.data
}

export const GetUserRole = async () => {
  const response = await apiClient.get(`/user/role`)
  return response.data
}

export const UpdateUser = async (updates) => {
  await apiClient.put(`/user`, updates)
}

export const UpdatePassword = async (updates) => {
  await apiClient.put(`/user/changePassword`, updates)
}
