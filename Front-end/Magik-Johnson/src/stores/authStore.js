// stores/auth.js
import { defineStore } from 'pinia'
import { GetUser } from '@/api/User/UserApi'
import { UpdateUser } from '@/api/User/UserApi'
import { UpdatePassword } from '@/api/User/UserApi'
import { GetUserRole } from '@/api/User/UserApi'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    role: null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    isUserClient: (state) => {
      if (state.role?.includes('client')) {
        return true
      }
    },
    isUserEmployee: (state) => {
      if (state.role?.includes('employee')) {
        return true
      }
    },
  },
  actions: {
    async fetchUser() {
      try {
        const user = await GetUser()
        this.user = user
      } catch (err) {
        this.user = null
      }
    },
    async fetchRole() {
      try {
        const role = await GetUserRole()
        this.role = role
      } catch (err) {
        this.role = null
      }
    },
    async updateUserInfo(updatedUser) {
      try {
        return await UpdateUser(updatedUser)
      } catch (err) {
        throw err
      }
    },
    async updateUserPassword(updatedPassword) {
      try {
        return await UpdatePassword(updatedPassword)
      } catch (err) {
        throw err
      }
    },
    setUser(user) {
      this.user = user
    },
    clearUser() {
      this.user = null
      this.role = null
      localStorage.removeItem('auth')
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
    },
  },

  persist: {
    storage: localStorage,
  },
})
