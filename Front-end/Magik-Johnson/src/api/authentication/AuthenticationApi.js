import { apiClient } from '@/api/Axios'
import axios from 'axios'

const apiUrl = import.meta.env.VITE_API_URL

export const UserRegistration = async (request) => {
  const response = await apiClient.post(`/authentication/registration`, request)

  return response.data
}

export const UserLogin = async (request) => {
  const response = await axios.post(`${apiUrl}/login`, request)

  return response.data
}

export const UserLogout = async () => {
  const response = await apiClient.post('/authentication/logout')

  return response
}

export const UserCheckResetCode = async (request) => {
  await apiClient.post('/authentication/checkResetCode', request)
}

export const UserResetPassword = async (request) => {
  const response = await apiClient.post(`/PasswordReset/reset`, request)

  return response
}

export const UserForgotPassword = async (request) => {
  await apiClient.post(`/PasswordReset/forgot`, request)
}
