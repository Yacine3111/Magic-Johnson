<template>
  <div class="checkout-page">
    <!-- Colonne gauche -->
    <section class="card panel">
      <header class="panel-header">Détails de la commande</header>

      <!-- Sélection du client-->
      <div v-if="!isClientMode" class="panel-section">
        <label class="label">Email du client</label>
        <div class="client-search-container">
          <input
            v-model="clientEmailSearch"
            @input="onClientEmailInput"
            @focus="showClientSuggestions = true"
            @blur="handleClientSearchBlur"
            type="email"
            class="client-email-input"
            placeholder="Rechercher un client par email..."
            autocomplete="off"
          />
          <div
            v-if="showClientSuggestions && clientSuggestions.length > 0"
            class="client-suggestions"
          >
            <div
              v-for="client in clientSuggestions"
              :key="client.id"
              class="client-suggestion-item"
              @mousedown="selectClient(client)"
            >
              <div class="client-email">{{ client.email }}</div>
              <div class="client-name">{{ client.firstName }} {{ client.name }}</div>
            </div>
          </div>
        </div>
        <div v-if="selectedClient" class="selected-client-info">
          <strong>Client sélectionné :</strong> {{ selectedClient.firstName }}
          {{ selectedClient.name }} ({{ selectedClient.email }})
        </div>
      </div>

      <!-- Champs manuels aucun client sélectionné Employé seulement -->
      <div
        v-if="!selectedClient && !isClientMode"
        class="panel-section"
        style="border-top: none; padding-top: 0"
      >
        <div class="grid-2">
          <div class="field">
            <span>Nom</span>
            <input v-model="clientLastNameInput" type="text" placeholder="Nom du client" />
          </div>
          <div class="field">
            <span>Prénom</span>
            <input v-model="clientFirstNameInput" type="text" placeholder="Prénom du client" />
          </div>
        </div>
      </div>

      <div class="chip-group">
        <button
          class="chip"
          :class="{ active: useCheckout.deliveryMode === 'HomeDelivery' }"
          @click="useCheckout.setDeliveryMode('HomeDelivery')"
        >
          Livraison
        </button>
        <button
          class="chip"
          :class="{ active: useCheckout.deliveryMode === 'InStorePickup' }"
          @click="useCheckout.setDeliveryMode('InStorePickup')"
        >
          Cueillette en magasin
        </button>
      </div>

      <AddressLivraisonClient
        v-if="useCheckout.deliveryMode === 'HomeDelivery' && isClientMode"
        :selected-client="selectedClient"
      ></AddressLivraisonClient>
      <AddressLivraisonEmploye
        v-else-if="useCheckout.deliveryMode === 'HomeDelivery'"
        :selected-client="selectedClient"
      ></AddressLivraisonEmploye>
      <div v-else class="panel-section">
        <div style="font-size: large">
          Récupérer votre commande en magasin à l'adresse :
          <span class="text-warning">430 Rue Wellington, Verdun, QC, H4G 1W5</span>
        </div>
      </div>
      <div class="panel-section">
        <h3>Articles</h3>
        <div class="cart-list">
          <article v-for="it in items" :key="it.id">
            <CartItem :item="it" />
          </article>
        </div>
      </div>
    </section>

    <!-- Colonne droite -->

    <OrderSummary
      :orderAmount="originalorderAmount"
      :discount="discount"
      :itemCount="itemCount"
      :shippingCost="getShippingCost"
      :totalAmount="totalAmount"
    >
      <template #actions>
        <q-separator spaced color="grey-8" />

        <div class="payment-actions q-mt-md">
          <button class="btn-stripe" @click="payerEnLigne">
            <i class="bi bi-credit-card me-2"></i>
            {{ isClientMode ? 'Payer' : 'Payer avec carte (Stripe)' }}
          </button>

          <button v-if="!isClientMode" class="btn-cash" @click="payerComptant">
            <i class="bi bi-cash-coin me-2"></i>
            Payer en argent comptant
          </button>
        </div>
      </template>
    </OrderSummary>
  </div>
</template>

<script setup lang="ts">
import { useCheckoutStore } from '@/stores/Checkout'
import { computed, onMounted, ref, watch } from 'vue'
import { useCartStore } from '@/stores/cartStore'
import { useAuthStore } from '@/stores/authStore'
import { Loading, Notify } from 'quasar'
import { CreateCheckoutCard, CreateCheckoutCash } from '@/api/payment/PaymentApi'
import AddressLivraisonClient from './shippringAddressClient.vue'
import AddressLivraisonEmploye from './ShippingAddressEmployee.vue'
import CartItem from './CartItem.vue'
import OrderSummary from './OrderSummary.vue'
import router from '@/router'
import { SearchCustomerByEmail } from '@/api/customer/CustomerApi'

const cartStore = useCartStore()
const useCheckout = useCheckoutStore()
const auth = useAuthStore()

const paymentMode = ref('')
const user = ref(null)

const isClientMode = computed(() => auth.isUserClient)

// Recherche de client
const clientEmailSearch = ref('')
const clientSuggestions = ref([])
const showClientSuggestions = ref(false)
const selectedClient = ref(null)
const clientFirstNameInput = ref('')
const clientLastNameInput = ref('')
let clientSearchTimeout = null

// Getters réactifs depuis le store PINIA
const items = computed(() => cartStore.items.filter((item) => item.quantity > 0))
const shippingCost = computed(() => cartStore.shippingCost)

const totalAmount = computed(() => {
  return orderAmount.value + getShippingCost.value
})

const getShippingCost = computed(() => {
  if (useCheckout.deliveryMode === 'InStorePickup') return 0

  const totalBeforeShippingCost = orderAmount.value - discount.value
  if (totalBeforeShippingCost >= 100) {
    return 0
  } else {
    return 10
  }
})

const subTotal = computed(() => {
  return orderAmount.value + getShippingCost.value
})

// Prix original
const originalorderAmount = computed(() => {
  return items.value.reduce((total, item) => total + item.price * item.quantity, 0)
})

// Sous-total avec les prix après discount
const orderAmount = computed(() => {
  return items.value.reduce((total, item) => {
    const originalPrice = item.price || 0
    const discount = item.discount || 0
    const discountedPrice = discount > 0 ? originalPrice * (1 - discount / 100) : originalPrice
    return total + discountedPrice * item.quantity
  }, 0)
})

// Rabais total
const discount = computed(() => {
  return originalorderAmount.value - orderAmount.value
})

onMounted(async () => {
  try {
    await auth.fetchUser()
    user.value = auth.user

    if (isClientMode.value && user.value) {
      selectedClient.value = {
        id: user.value.id,
        email: user.value.email,
        firstName: user.value.firstName,
        name: user.value.lastName,
      }
    }

    paymentMode.value = ''
    useCheckout.setDeliveryMode('InStorePickup')
  } catch (error) {
    console.error("Erreur lors du chargement de l'utilisateur:", error)
  }
})

const itemCount = computed(() => {
  return items.value.reduce((total, item) => total + item.quantity, 0)
})

// Fonctions de recherche de client
const onClientEmailInput = async () => {
  if (clientSearchTimeout) {
    clearTimeout(clientSearchTimeout)
  }

  const searchEmail = clientEmailSearch.value.trim()

  if (searchEmail.length < 2) {
    clientSuggestions.value = []
    showClientSuggestions.value = false
    selectedClient.value = null
    return
  }

  // Vérifier si c'est un email valide
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const isValidEmail = emailRegex.test(searchEmail)

  clientSearchTimeout = setTimeout(async () => {
    try {
      const results = await SearchCustomerByEmail(searchEmail, 10)
      clientSuggestions.value = results || []

      if (results && results.length > 0) {
        const exactMatch = results.find((c) => {
          if (!c.email) return false
          const clientEmail = c.email.trim().toLowerCase()
          const searchEmailLower = searchEmail.trim().toLowerCase()
          return clientEmail === searchEmailLower
        })

        if (exactMatch) {
          selectedClient.value = exactMatch
          showClientSuggestions.value = false
        } else if (isValidEmail) {
          showClientSuggestions.value = false
          selectedClient.value = null
        } else {
          showClientSuggestions.value = true
        }
      } else {
        showClientSuggestions.value = false
        selectedClient.value = null
      }
    } catch (error) {
      console.error('Erreur lors de la recherche de clients:', error)
      clientSuggestions.value = []
      if (searchEmail.length === 0) {
        selectedClient.value = null
      }
      showClientSuggestions.value = false
    }
  }, 300)
}

const selectClient = (client) => {
  selectedClient.value = client
  clientEmailSearch.value = client.email
  showClientSuggestions.value = false
  clientSuggestions.value = []
  clientFirstNameInput.value = ''
  clientLastNameInput.value = ''
}

const handleClientSearchBlur = () => {
  setTimeout(() => {
    showClientSuggestions.value = false
  }, 200)
}

async function payerComptant() {
  paymentMode.value = 'cash'
  useCheckout.setPaymentMethod('cash')
  await payer()
}

async function payerEnLigne() {
  paymentMode.value = 'card'
  useCheckout.setPaymentMethod('card')
  if (subTotal.value < 0.5) {
    Notify.create({
      type: 'negative',
      message: "Achats minimum d'au moins 50 centimes est requis pour payer par carte.",
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  await payer()
}

async function payer() {
  if (!cartStore.items || cartStore.items.length === 0) {
    Notify.create({
      type: 'negative',
      message: 'Votre panier est vide.',
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  if (useCheckout.deliveryMode === 'HomeDelivery' && useCheckout.shippingAddress === '') {
    Notify.create({
      type: 'negative',
      message: 'Entrer une adresse de livraison.',
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  const items = cartStore.items.filter((item) => item.quantity > 0)
  const itemsRequest = []

  for (const it of items) {
    const originalPrice = it.price || 0
    const discount = it.discount || 0
    let discountedPrice = discount > 0 ? originalPrice * (1 - discount / 100) : originalPrice
    discountedPrice = Math.round(discountedPrice * 100) / 100

    if (!it.id || !it.name || !it.image || !it.quantity || it.quantity <= 0) {
      console.error('Item invalide:', it)
      Notify.create({
        type: 'negative',
        message: `L'article "${it.name || 'inconnu'}" contient des informations invalides.`,
        position: 'top',
        actions: [
          {
            icon: 'close',
            color: 'white',
            round: true,
            dense: true,
            style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
            handler: () => {},
          },
        ],
      })
      return
    }

    itemsRequest.push({
      cardId: it.id,
      price: discountedPrice,
      cardName: it.name,
      imageURL: it.image || '',
      quantity: it.quantity,
      isFoil: it.isFoil || false,
      language: it.language || 'Anglais',
    })
  }

  // Vérifier qu'il y a au moins un item
  if (itemsRequest.length === 0) {
    Notify.create({
      type: 'negative',
      message: 'Votre panier est vide.',
      position: 'top',
    })
    return
  }

  if (!useCheckout.deliveryMode || useCheckout.deliveryMode === '') {
    Notify.create({
      type: 'negative',
      message: 'Veuillez sélectionner un mode de livraison.',
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  // Logic pour récupérer l'email client
  let finalClientEmail = ''
  let finalClientFirstName = ''
  let finalClientLastName = ''

  if (isClientMode.value) {
    finalClientEmail = user.value?.email
    finalClientFirstName = user.value?.firstName
    finalClientLastName = user.value?.lastName
  } else {
    // Employé Mode
    if (!ClientEmailOrManualInputValid()) return

    const clientEmail = clientEmailSearch.value.trim()
    finalClientEmail = clientEmail

    if (
      selectedClient.value &&
      selectedClient.value.email.toLowerCase() === clientEmail.toLowerCase()
    ) {
      finalClientFirstName = selectedClient.value.firstName || ''
      finalClientLastName = selectedClient.value.name || ''
    } else {
    }
  }

  if (isClientMode.value) {
    const request = {
      itemsRequest: itemsRequest,
      shippingAddress: useCheckout.shippingAddress || '',
      shippingCost: getShippingCost.value,
      deliveryMode: useCheckout.deliveryMode,
      email: user.value.email,
      lastName: user.value.lastName,
      firstName: user.value.firstName,
    }
    await processPayment(request)
    return
  }

  // Vérifier qu'un email client est fourni mode employee
  if (!clientEmailSearch.value || clientEmailSearch.value.trim() === '') {
    Notify.create({
      type: 'negative',
      message: "Veuillez entrer l'email du client.",
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  // Valider le format de l'email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const clientEmail = clientEmailSearch.value.trim()
  if (!emailRegex.test(clientEmail)) {
    Notify.create({
      type: 'negative',
      message: 'Veuillez entrer un email valide.',
      position: 'top',
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
    return
  }

  // Récupérer les informations du client depuis l'email
  let clientFirstName = ''
  let clientLastName = ''

  if (
    selectedClient.value &&
    selectedClient.value.email.toLowerCase() === clientEmail.toLowerCase()
  ) {
    clientFirstName = selectedClient.value.firstName || ''
    clientLastName = selectedClient.value.name || ''
  } else {
    try {
      Loading.show({ message: 'Recherche du client...' })
      const results = await SearchCustomerByEmail(clientEmail, 10)

      if (results && results.length > 0) {
        const foundClient = results.find((c) => {
          const clientEmailLower = (c.email || '').toLowerCase()
          const searchEmailLower = clientEmail.toLowerCase()
          return clientEmailLower === searchEmailLower
        })

        if (foundClient) {
          clientFirstName = foundClient.firstName || ''
          clientLastName = foundClient.name || ''
          selectedClient.value = foundClient
        } else {
          const firstResult = results[0]
          if (firstResult) {
            clientFirstName = firstResult.firstName || ''
            clientLastName = firstResult.name || ''
            selectedClient.value = firstResult
          }
        }
      }

      if (!clientFirstName || !clientLastName) {
        if (clientFirstNameInput.value && clientLastNameInput.value) {
          clientFirstName = clientFirstNameInput.value
          clientLastName = clientLastNameInput.value
        } else {
          Loading.hide()
          Notify.create({
            type: 'negative',
            message: `Client non trouvé. Veuillez remplir les champs Nom et Prénom pour continuer avec cet email.`,
            position: 'top',
            timeout: 5000,
            actions: [
              {
                icon: 'close',
                color: 'white',
                round: true,
                dense: true,
                style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
                handler: () => {},
              },
            ],
          })
          return
        }
      }
    } catch (error) {
      Loading.hide()
      Notify.create({
        type: 'negative',
        message: `Erreur lors de la recherche du client.`,
        position: 'top',
        timeout: 5000,
        actions: [
          {
            icon: 'close',
            color: 'white',
            round: true,
            dense: true,
            style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
            handler: () => {},
          },
        ],
      })
      return
    } finally {
      Loading.hide()
    }
  }

  const request = {
    itemsRequest: itemsRequest,
    shippingAddress: useCheckout.shippingAddress || '',
    shippingCost: getShippingCost.value,
    deliveryMode: useCheckout.deliveryMode,
    email: clientEmail,
    lastName: clientLastName,
    firstName: clientFirstName,
  }

  await processPayment(request)
}

async function processPayment(request) {
  try {
    Loading.show()
    if (paymentMode.value == 'cash') {
      await CreateCheckoutCash(request)

      await cartStore.clearCart()

      paymentMode.value = ''
      useCheckout.reset()

      router.push('/PaymentSuccess')
    } else if (paymentMode.value == 'card') {
      const checkoutUrl = await CreateCheckoutCard(request)

      if (user.value) {
        useCheckout.setCustomerInfo(
          user.value?.email || '',
          user.value?.firstName || '',
          user.value?.lastName || '',
        )
      }

      paymentMode.value = ''
      window.location.href = checkoutUrl
    }
  } catch (err) {
    console.error('Erreur lors du paiement:', err)

    paymentMode.value = ''

    let errorMessage = 'Une erreur est survenue lors du paiement. Veuillez réessayer.'
    if (err.response?.data) {
      if (err.response.data.errors) {
        const validationErrors = Object.values(err.response.data.errors).flat()
        errorMessage = validationErrors.join(', ') || errorMessage
      } else if (err.response.data.message) {
        errorMessage = err.response.data.message
      } else if (typeof err.response.data === 'string') {
        errorMessage = err.response.data
      }
    } else if (err.message) {
      errorMessage = err.message
    }

    Notify.create({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000,
      actions: [
        {
          icon: 'close',
          color: 'white',
          round: true,
          dense: true,
          style: { display: 'flex', alignItems: 'center', justifyContent: 'center' },
          handler: () => {},
        },
      ],
    })
  } finally {
    Loading.hide()
  }
}

function ClientEmailOrManualInputValid() {
  return true
}
</script>

<style scoped>
.label {
  font-size: medium !important  ;
}

/* Recherche de client */
.client-search-container {
  position: relative;
  margin-top: 8px;
}

.client-email-input {
  width: 100%;
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
  font-size: 14px;
}

.client-email-input::placeholder {
  color: #c6d1ef88;
}

.client-email-input:focus {
  border-color: #586aa0;
  box-shadow: 0 0 0 2px rgba(88, 106, 160, 0.2);
}

.client-suggestions {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #2a344f;
  border: 1px solid #3e4c79;
  border-radius: 8px;
  margin-top: 4px;
  max-height: 300px;
  overflow-y: auto;
  z-index: 1000;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.client-suggestion-item {
  padding: 12px 16px;
  cursor: pointer;
  border-bottom: 1px solid #3e4c79;
  transition: background 0.2s;
}

.client-suggestion-item:last-child {
  border-bottom: none;
}

.client-suggestion-item:hover {
  background: #3b4a71;
}

.client-email {
  font-weight: 600;
  color: #e6ecff;
  font-size: 14px;
  margin-bottom: 4px;
}

.client-name {
  font-size: 12px;
  color: #a9b6d1;
}

.selected-client-info {
  margin-top: 12px;
  padding: 10px;
  background: #2a344f;
  border: 1px solid #3e4c79;
  border-radius: 8px;
  font-size: 14px;
  color: #e6ecff;
}

.payment-actions {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 10px;
  padding: 0 16px 12px;
}

/* Bouton principal - Stripe */
.btn-stripe {
  background: linear-gradient(135deg, #4c6ef5, #364fc7);
  color: #fff;
  border: none;
  border-radius: 10px;
  padding: 14px;
  font-weight: 700;
  letter-spacing: 0.3px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.25s ease;
  box-shadow: 0 3px 8px rgba(76, 110, 245, 0.3);
}
.btn-stripe:hover {
  background: linear-gradient(135deg, #5c7cfa, #4263eb);
  box-shadow: 0 4px 12px rgba(76, 110, 245, 0.45);
}

/* Bouton secondaire */
.btn-cash {
  background: #ffd814;
  color: #000000;
  border: 1px solid #ffd814;
  border-radius: 10px;
  padding: 14px;
  font-weight: 600;
  letter-spacing: 0.2px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.25s ease;
}
.btn-cash:hover {
  background: #f7ca00;
  border-color: #f7ca00;
  color: #000000;
}

/* Icônes */
.btn-stripe i,
.btn-cash i {
  font-size: 1.1rem;
  vertical-align: middle;
}

/* Texte d'aide */
.text-caption {
  font-size: 0.85rem;
  color: #a9b6d1;
}

.is-invalid {
  border-color: #d93025;
}

.error {
  color: #d93025 !important;
  font-size: 0.85rem !important ;
  margin-top: 0.25rem !important;
}

.field {
  font-family: 'Segoe UI', Helvetica, Arial, sans-serif;
}

/* Layout */
.checkout-page {
  display: grid;
  grid-template-columns: 1fr minmax(420px, max-content);
  gap: 18px;
  padding: 96px 16px 16px 16px;
  background-color: #1b2231;
  min-height: 100vh;
  margin-top: -80px;
}

.card {
  background: #1f2837;
  border-radius: 16px;
  color: #e6ecff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  border: 1px solid #2b3550;
}

.panel {
  overflow: hidden;
}
.panel-header {
  background: #2e3a5a;
  padding: 14px 18px;
  font-weight: 700;
  letter-spacing: 0.5px;
  border-bottom: 1px solid #3a466d;
  font-size: large;
}
.panel-section {
  padding: 16px 18px;
  border-top: 1px solid #2b3550;
}
.panel-section h3 {
  margin: 0 0 10px;
  font-size: 18px;
  font-weight: 700;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 10px;
}
.field > span {
  font-size: 13px;
  color: #a9b6d1;
}
.field input {
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
}
.field input::placeholder {
  color: #c6d1ef88;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
.grid-3 {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 10px;
}
.compact {
  grid-template-columns: 1fr 1fr;
}

.chip-group {
  display: flex;
  gap: 10px;
  margin: 18px 18px 12px 18px;
  padding: 0;
}
.chip {
  background: #3b4a71;
  color: #e6ecff;
  border: 1px solid #506096;
  padding: 8px 12px;
  border-radius: 999px;
  cursor: pointer;
  font-weight: 600;
}
.chip.active {
  background: #586aa0;
}

.cart-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.cart-item {
  display: grid;
  grid-template-columns: 60px 1fr auto auto 28px;
  gap: 10px;
  align-items: center;
  background: #2a344f;
  border-radius: 10px;
  padding: 8px;
  border: 1px solid #3e4c79;
}
.cart-item img {
  width: 60px;
  height: 60px;
  border-radius: 8px;
  object-fit: cover;
}
.cart-item .info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.muted {
  color: #a9b6d1;
}
.qty,
.price {
  font-size: medium;
  display: flex;
  align-items: center;
  gap: 8px;
}
.qty input {
  width: 80px;
  text-align: right;
  padding: 8px 10px;
  border-radius: 8px;
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
}
.icon.danger {
  background: transparent;
  border: none;
  color: #ffd2d2;
  font-size: 20px;
  cursor: pointer;
}

.summary {
  padding-bottom: 12px;
}
.totals {
  list-style: none;
  margin: 0;
  padding: 10px 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.totals li {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #cfe1ff;
}
.grand {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 10px 16px;
  padding: 12px;
  background: #1b2231;
  border: 1px solid #3a466d;
  border-radius: 10px;
  font-weight: 800;
  letter-spacing: 0.3px;
}
.pay {
  display: block;
  margin: 8px auto 12px;
  padding: 10px 18px;
  border-radius: 999px;
  background: #e9eefc;
  border: 0;
  color: #0e1a33;
  font-weight: 800;
  cursor: pointer;
}

@media (max-width: 1024px) {
  .checkout-page {
    grid-template-columns: 1fr;
  }
  .summary {
    order: -1;
  }
}

/* Light Mode*/
body:not(.body--dark) .checkout-page {
  background-color: #d1cbbb;
}

body:not(.body--dark) .card {
  background: #f8f5f0;
  border: 1px solid #000;
  color: #000;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .panel-header {
  background: #a89c8d;
  color: #000;
  border-bottom: 1px solid #000;
}

body:not(.body--dark) .client-email-input {
  background: #fff;
  border: 1px solid #ccc;
  color: #000;
}

body:not(.body--dark) .client-email-input::placeholder {
  color: #666;
}

body:not(.body--dark) .client-suggestions {
  background: #fff;
  border: 1px solid #ccc;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .client-suggestion-item {
  border-bottom: 1px solid #eee;
}

body:not(.body--dark) .client-suggestion-item:hover {
  background: #f0f0f0;
}

body:not(.body--dark) .client-email {
  color: #000;
}

body:not(.body--dark) .client-name {
  color: #555;
}

body:not(.body--dark) .selected-client-info {
  background: #fff;
  border: 1px solid #ccc;
  color: #000;
}

body:not(.body--dark) .label {
  color: #000;
}

body:not(.body--dark) .field span {
  color: #000;
}

body:not(.body--dark) .field input {
  background: #fff;
  border: 1px solid #ccc;
  color: #000;
}

body:not(.body--dark) .field input::placeholder {
  color: #666;
}

body:not(.body--dark) .panel-section {
  color: #000;
  border-top: 1px solid #ccc;
}
body:not(.body--dark) .panel-section h3 {
  color: #000;
}

body:not(.body--dark) .text-warning {
  color: #d32f2f !important;
}

body:not(.body--dark) .btn-stripe {
  color: #fff;
}

body:not(.body--dark) .btn-cash {
  background: #ffd814;
  color: #000;
  border: 1px solid #ffd814;
}
body:not(.body--dark) .btn-cash:hover {
  background: #f7ca00;
}

body:not(.body--dark) .cart-item {
  background: #d1cbbb;
  border: 1px solid #ccc;
  color: #000;
}
body:not(.body--dark) .muted {
  color: #000;
}
body:not(.body--dark) .qty input {
  background: #fff;
  border: 1px solid #ccc;
  color: #000;
}
body:not(.body--dark) .chip {
  background: #e0e0e0;
  border: 1px solid #ccc;
  color: #333;
}
body:not(.body--dark) .chip.active {
  background: #a89c8d;
  color: white;
  border-color: #8b7d6b;
}
</style>
