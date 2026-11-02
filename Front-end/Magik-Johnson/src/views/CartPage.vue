<template>
  <div class="panier-page">
    <!-- Header avec titre -->
    <div class="panier-header" v-if="cartItems.length != 0">
      <h1 class="panier-title">
        Panier ({{ totalQuantity }})<span v-if="cartItems.length > 0"> </span>
      </h1>
    </div>

    <!-- Contenu principal -->
    <div class="panier-container">
      <!-- Panier vide -->
      <div v-if="cartItems.length === 0" class="empty-state">
        <div class="empty-card">
          <q-icon name="shopping_cart" size="80px" class="empty-icon" />
          <h3>Votre panier est vide</h3>
          <p>Ajoutez des articles à votre panier pour continuer.</p>
          <RouterLink to="/catalogue">
            <q-btn color="primary" size="lg" label="Parcourir le catalogue" />
          </RouterLink>
        </div>
      </div>

      <!-- Panier avec articles -->
      <div v-else class="panier-content">
        <!-- Bandeau compteur -->
        <div class="votre-panier">
          <span class="panier-label">
            Votre panier : <span class="count-badge">{{ totalQuantity }}</span>
          </span>
          <button
            v-if="cartItems.length > 0"
            class="clear-cart-btn"
            type="button"
            @click="clearCart"
          >
            Enlever toutes les cartes
          </button>
        </div>

        <!-- Résumé commande  mobile/iPad -->
        <div class="right-section mobile-summary">
          <CartSummary
            :items="cartItems"
            :discount-rate="discountRate"
            :shipping-cost="shippingCost"
          />
        </div>

        <!-- Liste des articles -->
        <div class="articles-list">
          <ItemCart
            v-for="item in cartItems"
            :key="item.id"
            :item="item"
            @update-quantity="updateQuantity"
            @remove-item="removeItem"
          />
        </div>

        <!-- Total partiel en bas -->
        <div class="total-partiel">
          Total partiel ({{ totalQuantity }} articles) : {{ subtotalFormatted }}
        </div>

        <!-- Résumé commande  desktop -->
        <div class="right-section desktop-summary">
          <CartSummary
            :items="cartItems"
            :discount-rate="discountRate"
            :shipping-cost="shippingCost"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { Notify } from 'quasar'
import ItemCart from '@/components/cart/ItemCart.vue'
import CartSummary from '@/components/cart/CartSummary.vue'
import { useCartStore } from '@/stores/cartStore'
import { formatPrice } from '@/services/priceFormatter'

// Store du panier
const cartStore = useCartStore()

// Getters réactifs depuis le store PINIA
const cartItems = computed(() => cartStore.items)
const discountRate = computed(() => cartStore.discountRate)
const shippingCost = computed(() => cartStore.shippingCost)
const totalQuantity = computed(() => cartStore.totalQuantity)

// Actions du panier
const showDeleteConfirmation = (message) => {
  return new Promise((resolve) => {
    const notification = Notify.create({
      color: 'bleu-1',
      icon: 'warning',
      message,
      position: 'top',
      timeout: 0,
      actions: [
        {
          label: 'Oui',
          color: 'negative',
          handler: () => {
            notification()
            resolve(true)
          },
        },
        {
          label: 'Non',
          color: 'white',
          handler: () => {
            notification()
            resolve(false)
          },
        },
      ],
    })
  })
}

const updateQuantity = async (itemId, languageName, isFoil, newQuantity) => {
  const item = cartItems.value.find(
    (cartItem) => cartItem.id === itemId && cartItem.language === languageName,
  )
  if (!item) {
    return
  }

  if (newQuantity <= 0) {
    const confirmed = await showDeleteConfirmation(
      `Voulez-vous vraiment supprimer la carte : ${item.name || 'cette carte'} ?`,
    )
    if (!confirmed) {
      return
    }
  }

  cartStore.updateQuantity(itemId, languageName, isFoil, newQuantity)
}

const removeItem = async (itemId, languageName, isFoil) => {
  const item = cartItems.value.find((cartItem) => cartItem.id === itemId)
  const itemLabel = item?.name || 'cette carte'

  const confirmed = await showDeleteConfirmation(
    `Voulez-vous vraiment supprimer la carte : ${itemLabel} ?`,
  )
  if (!confirmed) {
    return
  }

  cartStore.removeItem(itemId, languageName, isFoil)
}

onMounted(async () => {
  await cartStore.updateStocksInRealTime()
})

const clearCart = async () => {
  if (cartItems.value.length === 0) {
    return
  }

  const confirmed = await showDeleteConfirmation(
    'Voulez-vous vraiment supprimer toutes les cartes ?',
  )
  if (!confirmed) {
    return
  }

  cartStore.clearCart()
}

// Formatage du sous-total
const subtotalFormatted = computed(() => {
  const subtotal = cartStore.subtotal
  return `${formatPrice(subtotal)} $`
})
</script>

<style src="../css/CartPage.css" scoped></style>

<style>
body.body--dark .panier-page,
body.body--dark .panier-header {
  background: rgb(35, 42, 56) !important;
}

body.body--dark .votre-panier,
body.body--dark .empty-card,
body.body--dark .total-partiel {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .panier-title,
body.body--dark .panier-label,
body.body--dark .empty-card h3,
body.body--dark .empty-card p {
  color: white !important;
}

body.body--dark .count-badge {
  background-color: #2d3c49 !important;
  color: white !important;
  border-color: #4b5563 !important;
}

body.body--dark .clear-cart-btn {
  background-color: #b91c1c !important;
  color: white !important;
  border-color: #b91c1c !important;
}

/* Light Mode Backgrounds */
body:not(.body--dark) .votre-panier,
body:not(.body--dark) .total-partiel,
body:not(.body--dark) .empty-card {
  background-color: #ddd7c7 !important;
  color: black !important;
  border: 1px solid #000;
}

body:not(.body--dark) .panier-label,
body:not(.body--dark) .panier-label .count-badge {
  color: black !important;
}
</style>
