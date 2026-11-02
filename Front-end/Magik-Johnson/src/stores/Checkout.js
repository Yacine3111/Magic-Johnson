import { defineStore } from 'pinia'

export const useCheckoutStore = defineStore('Checkout', {
  state: () => ({
    // Info client
    email: '',
    firstName: '',
    lastName: '',

    // Panier local (items, rabais, livraison)
    panier: {
      items: [],
      shippingCost: 0,
      discountRate: 0,
    },

    // Mode de livraison
    deliveryMode: '',

    shippingAddress: '',

    // Méthode de paiement
    paymentMethod: 'card',

    pendingSessionId: null,
    pendingPaymentIntentId: null,

    totalAmount: 0,
  }),

  getters: {
    getShippingAddress: (state) => state.shippingAddress,
  },

  actions: {
    setCustomerInfo(email, firstName, lastName) {
      this.email = email
      this.firstName = firstName
      this.lastName = lastName
    },

    setPaymentMethod(method) {
      this.paymentMethod = method
    },

    setPanier(items, shippingCost, discountRate) {
      this.panier = { items, shippingCost, discountRate }
    },

    setDeliveryMode(deliveryMode) {
      this.deliveryMode = deliveryMode
    },

    setShippingAddress(address) {
      this.shippingAddress = address
    },

    setPendingPayment(sessionId, paymentIntentId, totalAmount) {
      this.pendingSessionId = sessionId
      this.pendingPaymentIntentId = paymentIntentId
      this.totalAmount = totalAmount
    },

    reset() {
      this.email = ''
      this.firstName = ''
      this.lastName = ''
      this.panier = { items: [], shippingCost: 0, discountRate: 0 }
      this.deliveryMode = ''
      this.shippingAddress = ''
      this.paymentMethod = 'card'
      this.pendingSessionId = null
      this.pendingPaymentIntentId = null
      this.totalAmount = 0
    },
  },

  persist: {
    key: 'checkout-data',
    storage: localStorage,
    serializer: {
      serialize: (state) => JSON.stringify(state),
      deserialize: (str) => {
        const data = JSON.parse(str)
        data.deliveryMode = data.deliveryMode || ''
        data.paymentMethod = data.paymentMethod || 'card'
        return data
      },
    },
  },
})
