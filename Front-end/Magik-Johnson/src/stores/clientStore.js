// stores/auth.js
import { defineStore } from 'pinia'
import {
  ListCustomer,
  LockCustomer,
  UnLockCustomer,
  GetCustomerById,
  UpdateCustomer,
  GetClientAddresses,
  UpdateClientAddress,
  AddClientAddress,
} from '@/api/customer/CustomerApi'

export const useClientStore = defineStore('Client', {
  state: () => ({
    listCustomers: null,
    clientSelected: null,
    clientAddresses: [],
  }),

  actions: {
    async getSelectedClientAsync(clientId) {
      try {
        const client = await GetCustomerById(clientId)
        this.clientSelected = client
      } catch (err) {
        this.clientSelected = null
        throw err
      }
    },
    async updateClientAsync(client) {
      try {
        return await UpdateCustomer(client)
      } catch (err) {
        throw err
      }
    },
    async lockClientAsync(employeeId) {
      try {
        return await LockCustomer(employeeId)
      } catch (err) {
        throw err
      }
    },
    async unlockClientAsync(employeeId) {
      try {
        return await UnLockCustomer(employeeId)
      } catch (err) {
        throw err
      }
    },
    async listClientAsync() {
      try {
        this.listCustomers = await ListCustomer()
      } catch (err) {
        throw err
      }
    },
    async getClientAddressesAsync(userId) {
      try {
        const addresses = await GetClientAddresses(userId)
        this.clientAddresses = addresses || []
      } catch (err) {
        console.error(err)
        this.clientAddresses = []
      }
    },
    async updateClientAddressAsync(addressId, addressData) {
      try {
        await UpdateClientAddress(addressId, addressData)
        return true
      } catch (err) {
        console.error(err)
        return false
      }
    },
    async addClientAddressAsync(userId, addressData) {
      try {
        await AddClientAddress(userId, addressData)
        return true
      } catch (err) {
        console.error(err)
        return false
      }
    },
  },
})
