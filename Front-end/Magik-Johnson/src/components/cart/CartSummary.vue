<template>
  <div class="resume-commande">
    <div v-if="isChecking" class="loading-overlay">
      <div class="loading-content">
        <q-spinner-dots size="50px" />
        <div class="loading-text">Vérification de la commande en cours...</div>
      </div>
    </div>

    <div class="resume-content">
      <div class="resume-line">
        <span class="resume-label">Montant de la commande :</span>
        <span class="resume-value">{{ formatPrice(subtotal) }} $</span>
      </div>

      <div v-if="discount > 0" class="resume-line">
        <span class="resume-label">Rabais total :</span>
        <span class="resume-value discount-value">-{{ formatPrice(discount) }} $</span>
      </div>

      <div class="resume-line">
        <span class="resume-label">Nombre d'articles :</span>
        <span class="resume-value">{{ itemCount }}</span>
      </div>

      <div class="resume-line total-line">
        <span class="total-label">Prix total :</span>
        <span class="total-value">{{ formatPrice(total) }} $</span>
      </div>

      <div class="checkout-section">
        <button
          class="checkout-btn"
          :disabled="items.length === 0 || availableItemCount === 0 || isChecking"
          @click="proceedToCheckout"
        >
          <q-spinner-dots v-if="isChecking" size="20px" color="black" class="q-mr-sm" />
          {{ isChecking ? 'Vérification...' : 'Passer la commande' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, nextTick } from 'vue'
import { Notify } from 'quasar'
import { useAuthStore } from '@/stores/authStore'
import { useCartStore } from '@/stores/cartStore'
import { useRouter } from 'vue-router'
import { formatPrice } from '@/services/priceFormatter'
import { TranslationService } from '@/services/translationService'
import { cardService } from '@/services/CardService'

const router = useRouter()

const authStore = useAuthStore()
const cartStore = useCartStore()

const isChecking = ref(false)

onMounted(async () => {
  if (!authStore.role) {
    await authStore.fetchRole()
  }
})

const props = defineProps({
  items: {
    type: Array,
    required: true,
  },
  discountRate: {
    type: Number,
    default: 0.5,
  },
  shippingCost: {
    type: Number,
    default: 0,
  },
})

const originalSubtotal = computed(() => {
  return props.items.reduce((total, item) => total + item.price * item.quantity, 0)
})

const subtotal = computed(() => {
  return props.items.reduce((total, item) => {
    const originalPrice = item.price || 0
    const discount = item.discount || 0
    const discountedPrice = discount > 0 ? originalPrice * (1 - discount / 100) : originalPrice
    return total + discountedPrice * item.quantity
  }, 0)
})

const discount = computed(() => {
  return originalSubtotal.value - subtotal.value
})

const shipping = computed(() => {
  return props.shippingCost
})

const total = computed(() => {
  return subtotal.value + shipping.value
})

const itemCount = computed(() => {
  return props.items.reduce((total, item) => total + item.quantity, 0)
})

// Nombre d'articles avec quantité > 0 (articles disponibles)
const availableItemCount = computed(() => {
  return props.items.filter((item) => item.quantity > 0).length
})

const proceedToCheckout = async () => {
  const availableItems = props.items.filter((item) => item.quantity > 0)

  if (props.items.length === 0 || availableItems.length === 0) {
    Notify.create({
      type: 'negative',
      message: 'Votre panier est vide',
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

  if (total.value < 0.5) {
    Notify.create({
      type: 'warning',
      message: 'Le montant minimum de commande est de 0.50 $',
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

  if (!authStore.user) {
    Notify.create({
      type: 'warning',
      message: 'Vous devez être connecté pour passer une commande',
      position: 'top',
      timeout: 2000,
    })

    setTimeout(() => {
      router.push('/Connexion')
    }, 1500)
    return
  }

  const accessToken = localStorage.getItem('accessToken')
  if (!accessToken) {
    authStore.clearUser()

    Notify.create({
      type: 'warning',
      message: 'Session expirée',
      position: 'top',
      timeout: 2000,
    })

    setTimeout(() => {
      router.push('/Connexion')
    }, 1500)
    return
  }

  if (!authStore.role) {
    await authStore.fetchRole()
  }

  const userRole = Array.isArray(authStore.role) ? authStore.role[0] : authStore.role

  if (userRole === 'manager') {
    const roleName = userRole === 'manager' ? 'Manager' : 'Employee'
    Notify.create({
      type: 'negative',
      message: `Ne peut pas passer une commande en tant que ${roleName}`,
      position: 'top',
      badgeStyle: 'display: none',
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

  isChecking.value = true

  try {
    let hasErrors = false
    const errors = []

    const inventoryResponse = await cardService.getInventoryCards(1, 1000, {})

    const itemsToCheck = cartStore.items.filter((item) => item.quantity > 0)

    for (const item of itemsToCheck) {
      let backendCard = inventoryResponse.cards.find((c) => c.id === item.id)

      if (!backendCard) {
        try {
          backendCard = await cardService.getCard(item.id)
        } catch (fallbackError) {
          console.error('Impossible de récupérer la carte', item.id, fallbackError)
        }
      }

      if (!backendCard) {
        errors.push(`Carte ${item.name} non trouvée dans l'inventaire`)
        hasErrors = true

        continue
      }

      let stockFoil = 0
      let stockNonFoil = 0

      if (backendCard.languageQuantities) {
        if (
          item.language &&
          item.language !== null &&
          item.language !== undefined &&
          item.language !== ''
        ) {
          const normalizedItemLanguage = TranslationService.reverseTranslateLanguage(item.language)

          const languageQuantity = backendCard.languageQuantities.find((langQty) => {
            const langName = langQty.languageName || ''
            return (
              langName.toLowerCase() === normalizedItemLanguage?.toLowerCase() ||
              langName.toLowerCase() === item.language?.toLowerCase() ||
              (langName === 'English' && (item.language === 'Anglais' || !item.language))
            )
          })

          if (languageQuantity) {
            stockFoil = languageQuantity.foilQuantity || 0
            stockNonFoil = languageQuantity.nonFoilQuantity || 0
          } else {
            stockFoil = backendCard.languageQuantities.reduce(
              (total, lang) => total + (lang.foilQuantity || 0),
              0,
            )
            stockNonFoil = backendCard.languageQuantities.reduce(
              (total, lang) => total + (lang.nonFoilQuantity || 0),
              0,
            )
          }
        } else {
          stockNonFoil = backendCard.languageQuantities.reduce(
            (total, lang) => total + (lang.nonFoilQuantity || 0),
            0,
          )
        }
      }

      const isFoil =
        item.isFoil !== undefined
          ? item.isFoil
          : backendCard.price <= 0 && backendCard.foilPrice > 0
      const currentStock = isFoil ? stockFoil : stockNonFoil

      if (currentStock === 0) {
        const stockType = isFoil ? 'foil' : 'non-foil'
        errors.push(`Stock ${stockType} épuisé pour ${item.name}`)
        hasErrors = true
      } else if (currentStock < item.quantity) {
        const stockType = isFoil ? 'foil' : 'non-foil'
        errors.push(
          `Stock ${stockType} insuffisant pour ${item.name}: ${item.quantity} demandés, ${currentStock} disponibles`,
        )
        hasErrors = true
      }
    }

    if (hasErrors) {
      const inventoryResponse = await cardService.getInventoryCards(1, 1000, {})

      const itemStockInfo = new Map()
      for (const item of cartStore.items) {
        itemStockInfo.set(item.id, {
          originalQuantity: item.quantity,
          adjustedQuantity: item.quantity,
          hasStockProblem: false,
        })
      }

      for (const item of cartStore.items) {
        let backendCard = inventoryResponse.cards.find((c) => c.id === item.id)

        if (!backendCard) {
          try {
            backendCard = await cardService.getCard(item.id)
          } catch (fallbackError) {
            console.error('Impossible de récupérer la carte', item.id, fallbackError)
          }
        }

        if (backendCard) {
          let stockFoil = 0
          let stockNonFoil = 0

          if (backendCard.languageQuantities) {
            if (
              item.language &&
              item.language !== null &&
              item.language !== undefined &&
              item.language !== ''
            ) {
              const normalizedItemLanguage = TranslationService.reverseTranslateLanguage(
                item.language,
              )

              const languageQuantity = backendCard.languageQuantities.find((langQty) => {
                const langName = langQty.languageName || ''
                return (
                  langName.toLowerCase() === normalizedItemLanguage?.toLowerCase() ||
                  langName.toLowerCase() === item.language?.toLowerCase() ||
                  (langName === 'English' && (item.language === 'Anglais' || !item.language))
                )
              })

              if (languageQuantity) {
                stockFoil = languageQuantity.foilQuantity || 0
                stockNonFoil = languageQuantity.nonFoilQuantity || 0
              } else {
                stockFoil = backendCard.languageQuantities.reduce(
                  (total, lang) => total + (lang.foilQuantity || 0),
                  0,
                )
                stockNonFoil = backendCard.languageQuantities.reduce(
                  (total, lang) => total + (lang.nonFoilQuantity || 0),
                  0,
                )
              }
            } else {
              stockNonFoil = backendCard.languageQuantities.reduce(
                (total, lang) => total + (lang.nonFoilQuantity || 0),
                0,
              )
            }
          }

          const isFoil =
            item.isFoil !== undefined
              ? item.isFoil
              : backendCard.price <= 0 && backendCard.foilPrice > 0
          const currentStock = isFoil ? stockFoil : stockNonFoil

          const stockInfo = itemStockInfo.get(item.id)
          if (stockInfo) {
            stockInfo.availableStock = currentStock
          }

          if (currentStock === 0) {
            const adjustedQuantity = 0
            const quantityNeedsAdjustment = item.quantity !== 0
            stockInfo.adjustedQuantity = adjustedQuantity
            stockInfo.hasStockProblem = quantityNeedsAdjustment

            Object.assign(item, {
              quantity: adjustedQuantity,
              hasStockIssue: quantityNeedsAdjustment,
              stockIssueMessage: quantityNeedsAdjustment
                ? 'STOCK ÉPUISÉ'
                : item.stockIssueMessage || '',
              stockFoil: stockFoil,
              stockNonFoil: stockNonFoil,
              stock: currentStock,
              isFoil: isFoil,
              limit: 0,
            })
          } else if (currentStock < item.quantity) {
            const adjustedQuantity = currentStock
            const quantityNeedsAdjustment = item.quantity !== adjustedQuantity
            stockInfo.adjustedQuantity = adjustedQuantity
            stockInfo.hasStockProblem = quantityNeedsAdjustment

            Object.assign(item, {
              quantity: adjustedQuantity,
              hasStockIssue: quantityNeedsAdjustment,
              stockIssueMessage: quantityNeedsAdjustment
                ? 'STOCK INSUFFISANT'
                : item.stockIssueMessage || '',
              stockFoil: stockFoil,
              stockNonFoil: stockNonFoil,
              stock: currentStock,
              isFoil: isFoil,
              limit: Math.min(item.clientLimit || 4, currentStock),
            })
          }
        }
      }

      if (cartStore.saveToLocalStorage) {
        cartStore.saveToLocalStorage()
      }

      await cartStore.updateStocksInRealTime(true)

      let hasQuantityChanged = false
      for (const item of cartStore.items) {
        const stockInfo = itemStockInfo.get(item.id)
        if (stockInfo && stockInfo.hasStockProblem) {
          if (stockInfo.originalQuantity > stockInfo.availableStock) {
            item.hasStockIssue = true
            item.stockIssueMessage = 'STOCK INSUFFISANT'

            if (stockInfo.originalQuantity !== item.quantity) {
              hasQuantityChanged = true
            }
          }
        }
      }

      await nextTick()

      if (hasQuantityChanged) {
        Notify.create({
          type: 'warning',
          message: 'Quantités ajustées selon le stock disponible',
          position: 'top',
          badgeStyle: 'display: none',
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
      }

      isChecking.value = false
      return
    }

    await cartStore.updateStocksInRealTime(true)

    try {
      router.push({ name: 'Commander' })

      isChecking.value = false
      Notify.create({
        color: 'primary',
        textColor: 'white',
        icon: 'shopping_cart_checkout',
        position: 'top',
        timeout: 5000,
        message: 'Vous y êtes presque, plus que quelques étapes!',
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
    } catch (navigationError) {
      isChecking.value = false
      console.error('Impossible de naviguer vers la page Commander', navigationError)
      Notify.create({
        type: 'negative',
        message: "Une erreur est survenue en passant à l'étape suivante",
        position: 'top',
        badgeStyle: 'display: none',
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
    }
  } catch (error) {
    Notify.create({
      type: 'negative',
      message: 'Erreur lors de la validation de la commande',
      position: 'top',
      badgeStyle: 'display: none',
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
    isChecking.value = false
  }
}
</script>

<style scoped>
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.loading-content {
  text-align: center;
  color: white;
}

.loading-text {
  margin-top: 20px;
  font-size: 18px;
  font-weight: 500;
}

.resume-commande {
  background: #a89c8d;
  border-radius: 20px;
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  width: 100%;
  min-width: 380px;
}

.resume-commande:hover {
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.35);
}

.resume-content {
  padding: 1.5rem;
}

.resume-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.8rem;
  font-size: 1rem;
}

.resume-label {
  color: #000000;
  font-weight: 600;
  flex: 1;
}

.resume-value {
  color: rgb(59, 58, 58);
  font-weight: 600;
  font-size: 1.1rem;
  white-space: nowrap;
  flex-shrink: 0;
}

.discount-value {
  color: #e74c3c;
  font-weight: bold;
}

.total-line {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 2px solid rgba(255, 255, 255, 0.2);
  margin-bottom: 1.2rem;
}

.total-label {
  color: #000000;
  font-size: 1.2rem;
  font-weight: bold;
}

.total-value {
  color: rgb(59, 58, 58);
  font-size: 1.4rem;
  font-weight: bold;
}

.checkout-section {
  text-align: center;
  margin-left: -1.5rem;
  margin-right: -1.5rem;
  margin-bottom: -1.5rem;
  padding: 0 1.5rem 1.5rem 1.5rem;
}

.checkout-btn {
  width: 100%;
  padding: 0.9rem 2rem;
  border-radius: 28px;
  font-weight: 800;
  font-size: 1.05rem;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.35);
  transition: all 0.25s ease;
  background-color: #ffd814;
  color: black;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  text-transform: none;
}

.checkout-btn:not(:disabled):hover {
  transform: translateY(-1px);
  box-shadow: 0 5px 12px rgba(0, 0, 0, 0.4);
}

.checkout-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 1024px) {
  .resume-commande {
    background: #a89c8d;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
    min-width: unset;
    width: 100%;
    border-top: none;
    margin-top: 1rem;
  }

  .resume-content {
    padding: 1rem;
  }

  .resume-line {
    font-size: 0.9rem;
    margin-bottom: 0.75rem;
    padding-bottom: 0.75rem;
    border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  }

  .resume-line:last-of-type {
    border-bottom: none;
  }

  .resume-label {
    color: #000000;
    font-weight: 600;
  }

  .resume-value {
    color: rgb(59, 58, 58);
    font-weight: 600;
  }

  .discount-value {
    color: #e74c3c;
  }

  .total-line {
    border-top: 2px solid #121826;
    border-bottom: none;
    padding-top: 1rem;
    margin-top: 0.5rem;
    margin-bottom: 1rem;
  }

  .total-label {
    font-size: 1.1rem;
    color: #000000;
    font-weight: 600;
  }

  .total-value {
    font-size: 1.3rem;
    color: rgb(59, 58, 58);
    font-weight: bold;
  }

  .checkout-section {
    margin-left: -1rem;
    margin-right: -1rem;
    padding: 0 1rem 1rem 1rem;
    margin-top: 0.5rem;
  }

  .checkout-btn {
    width: 100%;
    padding: 0.875rem 1.5rem;
    font-size: 1rem;
    min-height: 48px;
    background-color: #ffd814;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
    color: black;
  }

  .checkout-btn:not(:disabled):hover {
    background-color: #f7ca00;
    transform: none;
  }

  .checkout-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}

@media (min-width: 641px) and (max-width: 1024px) {
  .resume-commande {
    padding: 0;
  }

  .resume-content {
    padding: 1.5rem;
  }

  .resume-line {
    font-size: 1rem;
  }

  .total-label {
    font-size: 1.2rem;
  }

  .total-value {
    font-size: 1.4rem;
  }

  .checkout-btn {
    padding: 1rem 1.5rem;
    font-size: 1.1rem;
    min-height: 52px;
  }
}

@media (max-width: 640px) {
  .resume-commande {
    background: #f0eada;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
    min-width: unset;
    width: 100%;
    border-top: none;
    margin-top: 1rem;
  }

  .resume-content {
    padding: 1rem;
  }

  .resume-line {
    font-size: 0.9rem;
    margin-bottom: 0.75rem;
    padding-bottom: 0.75rem;
    border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  }

  .resume-line:last-of-type {
    border-bottom: none;
  }

  .resume-label {
    color: #000000;
    font-weight: 500;
  }

  .resume-value {
    color: rgb(59, 58, 58);
    font-weight: 600;
  }

  .discount-value {
    color: #e74c3c;
  }

  .total-line {
    border-top: 2px solid #121826;
    border-bottom: none;
    padding-top: 1rem;
    margin-top: 0.5rem;
    margin-bottom: 1rem;
  }

  .total-label {
    font-size: 1.1rem;
    color: rgb(0, 0, 0);
    font-weight: bold;
  }

  .total-value {
    font-size: 1.3rem;
    color: rgb(59, 58, 58);
    font-weight: bold;
  }

  .checkout-section {
    margin-left: -1rem;
    margin-right: -1rem;
    padding: 0 1rem 1rem 1rem;
    margin-top: 0.5rem;
  }

  .checkout-btn {
    width: 100%;
    padding: 0.875rem 1.5rem;
    font-size: 1rem;
    min-height: 48px;
    background-color: #ffd814;
    border: none;
    border-radius: 8px;
    font-weight: bold;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
    color: black;
  }

  .checkout-btn:not(:disabled):hover {
    background-color: #f7ca00;
    transform: none;
  }

  .checkout-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}
</style>

<style>
body.body--dark .resume-commande {
  background-color: #1f2735 !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .total-value,
body.body--dark .resume-value {
  color: white !important;
}

body.body--dark .total-label,
body.body--dark .resume-label {
  color: #bdc3c7 !important;
}

/* Light Mode */
body:not(.body--dark) .resume-commande {
  background-color: #ddd7c7 !important;
  border: 1px solid #000;
  color: black !important;
}

body:not(.body--dark) .resume-label,
body:not(.body--dark) .total-label,
body:not(.body--dark) .resume-value,
body:not(.body--dark) .total-value {
  color: black !important;
}

body:not(.body--dark) .discount-value {
  color: #b72827 !important;
}
</style>
