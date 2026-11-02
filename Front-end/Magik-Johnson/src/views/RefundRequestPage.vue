<script setup>
import { onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { GetOrderById } from '@/api/order/OrderApi'
import { getAddress } from '@/api/address/AddressApi'
import router from '@/router'
import { Notify } from 'quasar'
import { CreateRefundRequest } from '@/api/payment/RefundRequestApi'
import { GetOrderTaxPercentage } from '@/api/payment/RefundRequestApi'
import { TranslationService } from '@/services/translationService'
import { ORDER_STATUS } from '@/models/OrderStatus'

const DTO = ref('')
const subTotalOrderPrice = ref(0)
const totalOrderPrice = ref(0)
const originalOrderPrice = ref(0)
const taxResponse = ref(0)
const addressDTO = ref('')
const deliveryFees = ref(0)
const refundType = ref('')
const selectedCardsToRefund = ref([])
const isActive = ref(false)
const reasonOfRefund = ref('')
const comments = ref('')
const taxPercentage = ref(0)

const route = useRoute()
const orderId = route.params.id

onMounted(async () => {
  DTO.value = await GetOrderById(orderId, true)
  addressDTO.value = await getAddress()
  try {
    taxPercentage.value = await GetOrderTaxPercentage(orderId)
  } catch (e) {}

  originalOrderPrice.value = DTO.value.subTotal + DTO.value.taxeAmount

  watch([refundType, selectedCardsToRefund], () => {
    calculatePrices()
  })
})

function calculatePrices() {
  if (!DTO.value || !DTO.value.items) return

  if (refundType.value === 'partiel') {
    subTotalOrderPrice.value = selectedCardsToRefund.value.reduce((sum, item) => {
      const price = item.unitPrice * item.quantity
      return sum + price
    }, 0)

    const historicalRate =
      DTO.value.subTotal && DTO.value.subTotal > 0
        ? DTO.value.taxeAmount / DTO.value.subTotal
        : taxPercentage.value

    taxResponse.value = subTotalOrderPrice.value * historicalRate

    // Delivery fees
    deliveryFees.value = 0
  } else {
    subTotalOrderPrice.value = DTO.value.subTotal - 10
    deliveryFees.value =
      DTO.value.orderStatusId != ORDER_STATUS.preparing ||
      subTotalOrderPrice.value > 100 ||
      DTO.value.deliveryMode == 'InStorePickup'
        ? 0
        : 10
    taxResponse.value = DTO.value.taxeAmount
  }

  let calculatedTotal = +(
    subTotalOrderPrice.value +
    deliveryFees.value +
    taxResponse.value
  ).toFixed(2)

  const originalDeliveryFee =
    DTO.value.subTotal > 100 || DTO.value.deliveryMode == 'InStorePickup' ? 0 : 10
  const realTotalPaid = +(
    DTO.value.subTotal +
    (DTO.value.taxeAmount || 0) +
    originalDeliveryFee
  ).toFixed(2)

  if (calculatedTotal > realTotalPaid) {
    calculatedTotal = realTotalPaid
  }

  totalOrderPrice.value = calculatedTotal.toFixed(2)
}

async function submit() {
  if (!validateSubmit()) return

  const request = {
    orderId: DTO.value.id,
    totalAmount: originalOrderPrice.value,
    isPartial: refundType.value === 'partiel',
    previousOrderStatusId: DTO.value.orderStatusId,
    refundJustification: reasonOfRefund.value,
    comments: comments.value,
    items: selectedCardsToRefund.value,
    isStillPreparing: DTO.value.orderStatusId == ORDER_STATUS.preparing ? true : false,
  }

  try {
    await CreateRefundRequest(request)
    Notify.create({
      type: 'positive',
      message: 'Demande de remboursement soumise avec succès',
      position: 'top',
    })
    router.push({ name: 'DemandeRemboursementSuccess' })
  } catch (error) {
    Notify.create({
      type: 'negative',
      message: 'Erreur lors de la soumission: ' + (error.response?.data?.message || error.message),
      position: 'top',
    })
  }
}

function validateSubmit() {
  if (!reasonOfRefund.value) {
    isActive.value = true
    Notify.create({
      type: 'negative',
      message: 'Veuillez justifier la raison du remboursement',
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
    return false
  }

  if (refundType.value === 'partiel' && selectedCardsToRefund.value.length === 0) {
    isActive.value = true
    Notify.create({
      type: 'negative',
      message: 'Veuillez sélectionner des cartes à rembourser',
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
    return false
  }
  return true
}

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('fr-CA', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}
</script>

<template>
  <div class="page-container">
    <div class="content-wrapper">
      <div class="header-section">
        <h1 class="page-title">Demande de remboursement</h1>
        <p class="page-subtitle">Commande #{{ DTO.id ? DTO.id.slice(0, 8) : '...' }}</p>
      </div>
      <div class="main-card">
        <div class="section order-info">
          <h2 class="section-title">Informations sur la commande</h2>
          <div class="info-grid">
            <div class="info-item">
              <span class="label">Date</span>
              <span class="value">{{ formatDate(DTO.checkoutDate) }}</span>
            </div>
            <div class="info-item">
              <span class="label">Statut</span>
              <span class="value status-badge">{{ DTO.orderStatusName }}</span>
            </div>
            <div class="info-item">
              <span class="label">Total payé</span>
              <span class="value">{{ (DTO.subTotal + (DTO.taxeAmount || 0)).toFixed(2) }} $</span>
            </div>
          </div>
        </div>

        <div class="divider"></div>

        <div class="section products-list">
          <h2 class="section-title">Produits commandés</h2>
          <div class="products-grid">
            <div v-for="item in DTO.items" :key="item.id" class="product-card">
              <div class="product-image-wrapper">
                <img
                  :src="`https://api.scryfall.com/cards/${item.card.scryfallId}?format=image`"
                  class="product-image"
                  alt="Card image"
                />
              </div>
              <div class="product-details">
                <div class="product-name">{{ item.card.name }}</div>
                <div class="product-meta">
                  <span class="product-qty">Qté: {{ item.quantity }}</span>
                  <span class="product-lang">{{
                    TranslationService.translateLanguage(item.languageName)
                  }}</span>
                </div>
                <div class="product-price">{{ (item.unitPrice * item.quantity).toFixed(2) }} $</div>
              </div>
            </div>
          </div>

          <div class="price-summary">
            <div class="summary-row">
              <span>Sous-total</span>
              <span>{{ subTotalOrderPrice.toFixed(2) }} $</span>
            </div>
            <div class="summary-row">
              <span>Livraison</span>
              <span>{{ deliveryFees.toFixed(2) }} $</span>
            </div>
            <div class="summary-row">
              <span>Taxes</span>
              <span>{{ taxResponse.toFixed(2) }} $</span>
            </div>
            <div class="summary-row total">
              <span>Total remboursable</span>
              <span>{{ totalOrderPrice }} $</span>
            </div>
          </div>
        </div>

        <div class="divider"></div>

        <div class="section refund-type">
          <h2 class="section-title">Type de remboursement</h2>
          <div class="refund-options">
            <label
              class="refund-option"
              :class="{
                active: refundType === 'complet',
                disabled: DTO.orderStatusName === 'Partiellement remboursée',
              }"
            >
              <input
                type="radio"
                v-model="refundType"
                value="complet"
                name="refundType"
                :disabled="DTO.orderStatusName === 'Partiellement remboursée'"
              />
              <div class="option-content">
                <span class="option-title">Remboursement complet</span>
                <span class="option-desc">Rembourser la totalité de la commande</span>
              </div>
            </label>

            <label class="refund-option" :class="{ active: refundType === 'partiel' }">
              <input type="radio" v-model="refundType" value="partiel" name="refundType" />
              <div class="option-content">
                <span class="option-title">Remboursement partiel</span>
                <span class="option-desc">Sélectionner des articles spécifiques</span>
              </div>
            </label>
          </div>

          <transition name="slide-fade">
            <div v-if="refundType === 'partiel'" class="partial-selection">
              <h3
                class="subsection-title"
                :class="{ 'text-error': isActive && selectedCardsToRefund.length === 0 }"
              >
                Sélectionnez les articles à rembourser
              </h3>
              <div class="selection-list">
                <label v-for="item in DTO.items" :key="item.id" class="selection-item">
                  <input
                    type="checkbox"
                    v-model="selectedCardsToRefund"
                    :value="{
                      cardId: item.card.id,
                      quantity: item.quantity,
                      language: item.languageName,
                      unitPrice: item.unitPrice,
                    }"
                  />
                  <span class="item-name">{{ item.card.name }}</span>
                </label>
              </div>
            </div>
          </transition>
        </div>

        <div class="divider"></div>

        <div class="section request-details">
          <h2 class="section-title">Détails de la demande</h2>

          <div class="form-group">
            <label class="form-label" :class="{ 'text-error': isActive && !reasonOfRefund }">
              Raison du remboursement <span class="required">*</span>
            </label>
            <div class="select-wrapper">
              <select v-model="reasonOfRefund" class="form-select" required>
                <option value="" disabled selected>Sélectionnez une raison</option>
                <optgroup label="Problèmes liés au produit">
                  <option>Produit endommagé à la réception</option>
                  <option>Mauvais produit reçu</option>
                  <option>Couleur / modèle incorrect</option>
                  <option>Qualité du produit insatisfaisante</option>
                </optgroup>
                <optgroup label="Problèmes de livraison">
                  <option>Commande non reçue</option>
                  <option>Livraison incomplète</option>
                  <option>Délai de livraison trop long</option>
                  <option>Erreur d’adresse ou de transporteur</option>
                </optgroup>
                <optgroup label="Problèmes de facturation">
                  <option>Erreur de montant facturé</option>
                  <option>Erreur dans le mode de paiement</option>
                </optgroup>
                <optgroup label="Autres">
                  <option>Changement d’avis / erreur de commande</option>
                  <option>Autre</option>
                </optgroup>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Commentaires additionnels</label>
            <textarea
              v-model="comments"
              class="form-textarea"
              rows="4"
              placeholder="Détails supplémentaires..."
            ></textarea>
          </div>
        </div>

        <div class="actions">
          <button class="submit-btn" @click="submit">Soumettre la demande</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="../css/RefundRequestPage.css" scoped></style>
