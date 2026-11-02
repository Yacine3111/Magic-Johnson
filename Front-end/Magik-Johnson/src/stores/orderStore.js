import { defineStore } from 'pinia'
import { GetOrders } from '@/api/order/OrderApi'
import { GetOrderStatus } from '@/api/order/OrderApi'
import { GetAllOrders } from '@/api/order/OrderApi'

export const useOrderStore = defineStore('order', {
  state: () => ({
    orders: null,
    orderStatus: [],
    selectedOrderStatus: [],
  }),
  actions: {
    async fetchOrder() {
      try {
        const orders = await GetOrders()
        this.orders = orders
      } catch (err) {
        this.orders = null
      }
    },

    async fetchAllOrders() {
      try {
        const orders = await GetAllOrders()
        this.orders = orders
      } catch (err) {
        this.orders = null
      }
    },
    async fetchOrderStatus() {
      try {
        const orderStatus = await GetOrderStatus()
        this.orderStatus = orderStatus
      } catch (err) {
        this.orderStatus = null
      }
    },

    clearStoreFilters() {
      this.selectedOrderStatus = []
    },
  },
})
