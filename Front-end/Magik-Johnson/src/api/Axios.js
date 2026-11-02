import axios from 'axios'
import VueCookies from 'vue-cookies'
import { Loading } from 'quasar'
import { useAuthStore } from '@/stores/authStore'

const apiUrl = import.meta.env.VITE_API_URL

export const apiClient = axios.create({
  baseURL: `${apiUrl}/api`,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
})

apiClient.interceptors.request.use(
  (request) => {
    const accessToken = localStorage.getItem('accessToken')

    if (accessToken) {
      request.headers['Authorization'] = `Bearer ${accessToken}`
    }

    return request
  },
  (error) => {
    return Promise.reject(error)
  },
)

apiClient.interceptors.response.use(
  (response) => {
    return response
  },

  async (error) => {
    const originalRequest = error.config
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        Loading.show({ spinnerColor: 'primary' })
        const currentRefreshToken = localStorage.getItem('refreshToken')
        const response = await axios.post(`${apiUrl}/refresh`, {
          refreshToken: currentRefreshToken,
        })

        const { accessToken, refreshToken: newRefreshToken } = response.data

        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)

        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`

        await useAuthStore().fetchUser()
        return apiClient(originalRequest)
      } catch (refreshError) {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')

        var rememberMe_cookie = VueCookies.get('rememberMe')
        if (rememberMe_cookie) {
          const email = VueCookies.get('email')
          const password = VueCookies.get('password')

          try {
            const response = await axios.post(`${apiUrl}/login`, {
              email: email,
              password: password,
            })

            localStorage.setItem('accessToken', response.data.accessToken)
            localStorage.setItem('refreshToken', response.data.refreshToken)

            await useAuthStore().fetchUser()
          } catch (err) {
            throw new Error(err)
          }
        }
        return Promise.reject(refreshError)
      } finally {
        Loading.hide()
      }
    }
    return Promise.reject(error)
  },
)
