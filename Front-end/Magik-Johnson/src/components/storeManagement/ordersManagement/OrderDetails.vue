<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { GetOrderById } from '@/api/order/OrderApi'

const route = useRoute()
const router = useRouter()

const goToCardDetails = (cardId) => {
  router.push({ name: 'DetailsCarte', params: { id: cardId } })
}
const loading = ref(true)
const order = ref(null)

const orderId = computed(() => route.params.orderId || order.value?.id)
const checkoutDate = computed(() => {
  if (!order.value?.checkoutDate) return ''

  const localDate = new Date(order.value.checkoutDate.replace(/(Z|[+-]\d{2}:?\d{2})$/, ''))
  return localDate.toLocaleString('fr-FR')
})
const clientId = computed(() => order.value?.userId || '')
const deliveryDate = computed(() => {
  if (!order.value?.deliveryDate) return ''

  const localDate = new Date(order.value.deliveryDate.replace(/(Z|[+-]\d{2}:?\d{2})$/, ''))
  return localDate.toLocaleString('fr-FR')
})
const orderStatus = computed(() => order.value?.orderStatusName || '')
const deliveryType = ref('Livraison')

const orderedCards = computed(() => {
  if (!order.value?.items) return []
  return order.value.items.map((item) => ({
    id: item.card.id,
    scryfallId: item.card.scryfallId,
    name: item.card.name,
    setName: item.card.setName,
    isFoil: item.isFoil,
    languageName: item.languageName,
    quantity: item.quantity,
    price: item.unitPrice,
  }))
})

const totalOrderedQuantity = computed(() => {
  return order.value?.totalQuantity || 0
})

const totalOrderPrice = computed(() => {
  return order.value?.totalPrice || 0
})

const subTotal = computed(() => {
  return order.value?.subTotal || 0
})

const taxeAmount = computed(() => {
  return order.value?.taxeAmount || 0
})

const grandTotal = computed(() => {
  return subTotal.value + taxeAmount.value
})

function goBack() {
  history.back()
}

const truncateId = (id) => {
  if (!id) return '—'
  return id.toString().substring(0, 8)
}

onMounted(async () => {
  try {
    const orderIdParam = route.params.orderId
    if (!orderIdParam) {
      console.error('ID de commande manquant')
      loading.value = false
      return
    }
    order.value = await GetOrderById(orderIdParam)
  } catch (error) {
    console.error('Erreur lors du chargement de la commande:', error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="q-pa-lg page-promotion">
    <div v-if="loading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
      <p class="q-mt-md">Chargement de la commande...</p>
    </div>
    <div v-else-if="!order" class="text-center q-pa-xl">
      <p class="text-h6">Commande introuvable</p>
    </div>
    <div v-else>
      <div class="title-section q-mt-md">
        <div class="text-h5 text-bold">Détails de la Commande</div>
      </div>
      <q-btn @click="goBack()" class="q-ml-sm q-mt-sm edit-btn" style="min-width: 120px">
        <q-icon name="arrow_back" size="sm" />
        retour
      </q-btn>

      <q-card class="q-ma-md">
        <div class="q-pa-md q-mt-md q-mb-lg">
          <h6 class="order-number">
            Numéro de la commande : #<span>{{ truncateId(orderId) }}</span>
          </h6>
          <div v-if="order?.clientFirstName || order?.clientLastName" class="info-row">
            <label class="info-label">Client :</label>
            <span class="info-value">{{ order.clientFirstName }} {{ order.clientLastName }}</span>
          </div>
          <div v-if="order?.clientEmail" class="info-row">
            <label class="info-label">Email :</label>
            <span class="info-value">{{ order.clientEmail }}</span>
          </div>

          <div class="info-row">
            <label class="info-label">ID du client :</label>
            <span class="info-value">{{ clientId }}</span>
          </div>

          <div class="info-row">
            <label class="info-label">Date de paiement :</label>
            <span class="info-value">{{ checkoutDate }}</span>
          </div>

          <div class="info-row">
            <label class="info-label">Date de livraison :</label>
            <span class="info-value">{{ deliveryDate }}</span>
          </div>

          <div class="info-row">
            <label class="info-label">État de la commande :</label>
            <span class="info-value">{{ orderStatus }}</span>
          </div>

          <div class="info-row">
            <label class="info-label">Type d'expédition :</label>
            <span class="info-value">{{ deliveryType }}</span>
          </div>
        </div>
      </q-card>

      <div class="title-section">
        <div class="text-h6 text-bold">Liste des cartes et quantités</div>
      </div>

      <!-- Desktop Table -->
      <div class="gt-xs">
        <table class="order-cards-table">
          <thead>
            <tr>
              <th></th>
              <th>Nom de la carte</th>
              <th>Série</th>
              <th>Finition</th>
              <th>Langue</th>
              <th>Quantité</th>
              <th>Prix</th>
            </tr>
          </thead>
          <tbody>
            <template v-if="orderedCards.length > 0">
              <tr v-for="card in orderedCards" :key="card.id" class="order-card-row">
                <td>
                  <div class="image-wrapper">
                    <img
                      class="card_image"
                      :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image`"
                      alt="Card image"
                      @click="goToCardDetails(card.id)"
                    />
                  </div>
                </td>
                <td>{{ card.name }}</td>
                <td>{{ card.setName }}</td>
                <td>{{ card.isFoil ? 'Foil' : 'Non-Foil' }}</td>
                <td>{{ card.languageName }}</td>
                <td>{{ card.quantity }}</td>
                <td>{{ card.price }}$</td>
              </tr>
            </template>
            <template v-else>
              <tr class="order-card-row">
                <td colspan="7" class="empty-message">Aucune carte dans cette commande</td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- Mobile List -->
      <div class="lt-sm q-mt-md">
        <template v-if="orderedCards.length > 0">
          <q-card
            v-for="card in orderedCards"
            :key="card.id"
            class="q-mb-md q-pa-sm bg-grey-9 text-white"
          >
            <div class="row q-col-gutter-sm">
              <div class="col-4">
                <img
                  class="full-width rounded-borders"
                  :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image`"
                  alt="Card image"
                  @click="goToCardDetails(card.id)"
                  style="cursor: pointer"
                />
              </div>
              <div class="col-8">
                <div class="text-subtitle2 text-weight-bold ellipsis-2-lines">{{ card.name }}</div>
                <div class="text-caption text-grey-4">{{ card.setName }}</div>

                <div class="row q-mt-sm text-caption">
                  <div class="col-6">
                    <div class="text-grey-5">Finition</div>
                    <div>{{ card.isFoil ? 'Foil' : 'Non-Foil' }}</div>
                  </div>
                  <div class="col-6">
                    <div class="text-grey-5">Langue</div>
                    <div>{{ card.languageName }}</div>
                  </div>
                </div>

                <div class="row q-mt-xs items-center justify-between">
                  <div class="text-body2">Qté: {{ card.quantity }}</div>
                  <div class="text-subtitle2 text-weight-bold">{{ card.price }}$</div>
                </div>
              </div>
            </div>
          </q-card>
        </template>
        <div v-else class="text-center text-white q-pa-md font-italic">
          Aucune carte dans cette commande
        </div>
      </div>

      <div class="order-totals">
        <div class="total-quantity-display">
          <h5 class="mb-0">
            <strong
              >Quantité totale de cartes commandées : {{ totalOrderedQuantity }} cartes</strong
            >
          </h5>
        </div>
        <div class="total-price-display">
          <div class="price-breakdown">
            <div class="price-row">
              <span class="price-label">Sous-total :</span>
              <span class="price-value">{{ subTotal.toFixed(2) }}$ CAD</span>
            </div>
            <div class="price-row">
              <span class="price-label">Taxes :</span>
              <span class="price-value">{{ taxeAmount.toFixed(2) }}$ CAD</span>
            </div>
            <div class="price-row total-row">
              <span class="price-label"><strong>Total :</strong></span>
              <span class="price-value"
                ><strong>{{ grandTotal.toFixed(2) }}$ CAD</strong></span
              >
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
.title-section {
  background-color: #959595;
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
  color: white;
}

.page-promotion {
  background-color: #676767;
  height: auto;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* Détails commande */
.order-number {
  color: #000000;
  margin-bottom: 24px;
  font-size: 18px;
}

.info-row {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  color: #000000;
  font-weight: 600;
  min-width: 200px;
  font-size: 15px;
}

.info-value {
  color: #333333;
  font-size: 15px;
  flex: 1;
}

.order-cards-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 16px;
}

.order-cards-table thead th {
  padding: 10px;
  color: white;
  background-color: rgb(35, 42, 56);
  text-align: left;
  border: 2px solid #ffffff;
}

.order-cards-table tbody td {
  padding: 10px;
  font-size: small;
  color: #ffffff !important;
}

.order-cards-table tbody td * {
  color: #ffffff !important;
}

.order-card-row {
  border-top: 2px solid #ffffff;
  border-bottom: 2px solid #ffffff;
}

.order-card-row:nth-child(odd) {
  background-color: #9c9a9a;
}

.order-card-row:nth-child(even) {
  background-color: #4c4b4b;
}

.image-wrapper {
  width: 80px;
  height: auto;
}

.card_image {
  width: 80%;
  height: auto;
  margin-top: 15px;
  border-radius: 4px;
  cursor: pointer;
  transition: transform 0.2s ease;
}

.card_image:hover {
  transform: scale(1.1);
}

.empty-message {
  text-align: center;
  padding: 20px;
  color: #ffffff !important;
  font-style: italic;
}

.order-totals {
  margin-top: 24px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.total-quantity-display,
.total-price-display {
  background-color: #4c4b4b;
  color: white;
  border: 2px solid #ffffff;
  padding: 16px;
  border-radius: 8px;
}

.total-quantity-display h5,
.total-price-display h5 {
  color: white;
  margin: 0;
}

.price-breakdown {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.price-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.price-row:last-child {
  border-bottom: none;
}

.total-row {
  padding-top: 12px;
  margin-top: 4px;
}

.price-label {
  color: white;
  font-size: 15px;
}

.price-value {
  color: white;
  font-size: 15px;
}

.edit-btn {
  background: #8a422c !important;
  color: white !important;
}
</style>

<style>
/* Light Mode */
.page-promotion {
  background-color: #d1cbbb !important;
}
.title-section,
.total-quantity-display,
.total-price-display {
  background-color: #a89c8d !important;
  color: black !important;
}
.title-section .text-h6,
.title-section .text-h5,
.total-quantity-display h5,
.total-price-display h5,
.price-label,
.price-value {
  color: black !important;
}

body:not(.body--dark) .order-cards-table tbody tr {
  background-color: #EFEBE9 !important;
  color: black !important;
}

body:not(.body--dark) .order-cards-table tbody td,
body:not(.body--dark) .order-cards-table tbody td * {
  color: black !important;
}

/* Dark Mode  */
body.body--dark .page-promotion {
  background-color: rgb(35, 42, 56) !important;
}

body.body--dark .title-section,
body.body--dark .total-quantity-display,
body.body--dark .total-price-display,
body.body--dark .q-card {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .title-section .text-h6,
body.body--dark .title-section .text-h5,
body.body--dark .total-quantity-display h5,
body.body--dark .total-price-display h5,
body.body--dark .price-label,
body.body--dark .price-value,
body.body--dark .info-label,
body.body--dark .info-value,
body.body--dark .text-h5,
body.body--dark .text-h6,
body.body--dark .order-number,
body.body--dark span,
body.body--dark label {
  color: white !important;
}

body.body--dark .order-cards-table thead th {
  background-color: #2d3c49 !important;
  color: white !important;
  border-color: #4b5563 !important;
}

body.body--dark .order-cards-table tbody td {
  color: white !important;
  border-color: #4b5563 !important;
}

body.body--dark .order-card-row,
body.body--dark .order-card-row:nth-child(odd),
body.body--dark .order-card-row:nth-child(even) {
  background-color: #1f2735 !important;
  border-color: #4b5563 !important;
}

body.body--dark .info-row {
  border-bottom: 1px solid rgba(255, 255, 255, 0.1) !important;
}
</style>
