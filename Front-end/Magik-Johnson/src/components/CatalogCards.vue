<template>
  <div class="row g-3">
    <div v-for="card in cards" :key="card.id" :class="isMobile ? 'col-12' : 'col-md-6 col-lg-4'">
      <div class="mtg-card shadow-sm">
        <div class="image-wrapper">
          <img
            :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image`"
            :alt="`Image de ${card?.name}`"
            class="mtg-card-img"
          />
          <div v-if="getPriceType(card) === 'foil'" class="foil-overlay"></div>
        </div>
        <div class="mtg-card-body">
          <h6 class="mtg-card-title">{{ card.name }}</h6>
          <p class="mtg-card-serie">{{ card.setName }}, {{ card.rarity }}, #{{ card.number }}</p>
          <div class="mtg-card-price">
            <span
              v-if="card.discount && card.discount > 0 && getOriginalPrice(card) > 0"
              class="mtg-card-new discounted"
            >
              {{ formatPrice(getDiscountedPrice(card, card.discount)) }} $
            </span>
            <span
              v-else
              class="mtg-card-new no-discount"
              :style="{ visibility: getOriginalPrice(card) > 0 ? 'visible' : 'hidden' }"
            >
              {{
                getOriginalPrice(card) > 0 ? formatPrice(getOriginalPrice(card)) + ' $' : '0,00 $'
              }}
            </span>
            <span
              v-if="card.discount && card.discount > 0 && getOriginalPrice(card) > 0"
              class="mtg-card-old"
            >
              {{ formatPrice(getOriginalPrice(card)) }} $
            </span>
          </div>
          <div class="mtg-card-actions">
            <button class="btn btn-primary btn-sm" @click="viewCard(card.id)">Voir carte</button>
            <button
              :class="[
                'btn btn-sm',
                isCardInCart(card.id, card.language, getPriceType(card) === 'foil')
                  ? 'btn-danger'
                  : getCardStock(card) === 0 || card.onlineOnly || getDisplayPrice(card) <= 0
                    ? 'btn-secondary'
                    : 'btn-primary',
              ]"
              :style="
                isCardInCart(card.id, card.language, getPriceType(card) === 'foil')
                  ? { backgroundColor: '#dc3545', borderColor: '#dc3545', color: 'white' }
                  : getCardStock(card) === 0 || card.onlineOnly || getDisplayPrice(card) <= 0
                    ? {
                        backgroundColor: '#6c757d',
                        borderColor: '#6c757d',
                        color: 'white',
                        cursor: 'not-allowed',
                      }
                    : {}
              "
              @click="
                isCardInCart(card.id, card.language, getPriceType(card) === 'foil')
                  ? removeFromCart(card.id, card.language, card)
                  : addToCart(card)
              "
              :disabled="getDisplayPrice(card) <= 0 || getCardStock(card) === 0 || card.onlineOnly"
            >
              {{
                card.onlineOnly
                  ? 'Non disponible'
                  : getCardStock(card) === 0
                    ? 'Stock épuisé'
                    : getDisplayPrice(card) <= 0
                      ? 'Non disponible'
                      : isCardInCart(card.id, card.language, getPriceType(card) === 'foil')
                        ? 'Retirer du panier'
                        : 'Ajouter au panier'
              }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { cardService } from '@/services/CardService'
import { formatPrice } from '@/services/priceFormatter'

const router = useRouter()
const cartStore = useCartStore()

const props = defineProps({
  cards: {
    type: Array,
    required: true,
  },
  isMobile: {
    type: Boolean,
    default: false,
  },
  selectedFinishes: {
    type: Array,
    default: () => [],
  },
})

// Navigation vers la page de détails de la carte
const viewCard = (cardId) => {
  cardService.updateCardClickCount(cardId)

  router.push({ name: 'DetailsCarte', params: { id: cardId } })
}

// Vérifier si une carte est dans le panier
const isCardInCart = (cardId, language = null, isFoil = null) => {
  return cartStore.isInCart(cardId, language, isFoil)
}

// Retirer une carte du panier
const removeFromCart = (cardId, languageName, card) => {
  const isFoil = getPriceType(card) === 'foil'
  let lang = languageName
  if (!lang) {
    const item = cartStore.items.find((i) => i.id === cardId)
    if (item) {
      lang = item.language
    }
  }
  cartStore.removeItem(cardId, lang, isFoil)
}

const addToCart = async (card) => {
  const originalPrice = getOriginalPrice(card)
  const isFoil = getPriceType(card) === 'foil'

  if (originalPrice <= 0) {
    return
  }

  const determineLanguageFromInventory = (languageQuantities) => {
    if (!languageQuantities || languageQuantities.length === 0) {
      return 'Anglais'
    }

    const languageMap = {
      English: 'Anglais',
      French: 'Français',
      German: 'Allemand',
      Spanish: 'Espagnol',
      Italian: 'Italien',
      Portuguese: 'Portugais',
      'Portuguese (Brazil)': 'Portugais (Brésil)',
      Russian: 'Russe',
      Japanese: 'Japonais',
      Korean: 'Coréen',
      'Chinese Simplified': 'Chinois simplifié',
      'Chinese Traditional': 'Chinois traditionnel',
      'Ancient Greek': 'Grec ancien',
      Arabic: 'Arabe',
      Hebrew: 'Hébreu',
      Latin: 'Latin',
      Sanskrit: 'Sanskrit',
      Phyrexian: 'Phyrexian',
    }

    const availableLanguages = languageQuantities
      .filter((lang) => {
        const totalStock = (lang.foilQuantity || 0) + (lang.nonFoilQuantity || 0)
        return totalStock > 0
      })
      .map((lang) => {
        const langName = lang.languageName || 'English'
        const frenchName = languageMap[langName] || langName
        return frenchName
      })

    if (availableLanguages.length === 0) {
      return 'Anglais'
    }

    if (availableLanguages.includes('Français')) {
      return 'Français'
    } else if (availableLanguages.length > 0) {
      const selected = availableLanguages[0]
      return selected
    }

    return 'Anglais'
  }

  let stockFoil = 0
  let stockNonFoil = 0
  let selectedLanguage = null
  let inventoryCard = null

  try {
    const { cardService } = await import('@/services/CardService')
    const inventoryResponse = await cardService.getInventoryCards(1, 1, { searchQuery: card.name })

    if (inventoryResponse && inventoryResponse.cards && inventoryResponse.cards.length > 0) {
      inventoryCard = inventoryResponse.cards.find((c) => c.id === card.id)
      if (inventoryCard && inventoryCard.languageQuantities) {
        selectedLanguage = determineLanguageFromInventory(inventoryCard.languageQuantities)

        const selectedLanguageEnglish =
          {
            Français: 'French',
            Anglais: 'English',
            Allemand: 'German',
            Espagnol: 'Spanish',
            Italien: 'Italian',
            Portugais: 'Portuguese',
            'Portugais (Brésil)': 'Portuguese (Brazil)',
            Russe: 'Russian',
            Japonais: 'Japanese',
            Coréen: 'Korean',
            'Chinois simplifié': 'Chinese Simplified',
            'Chinois traditionnel': 'Chinese Traditional',
            'Grec ancien': 'Ancient Greek',
            Arabe: 'Arabic',
            Hébreu: 'Hebrew',
            Latin: 'Latin',
            Sanskrit: 'Sanskrit',
            Phyrexian: 'Phyrexian',
          }[selectedLanguage] || 'English'

        const languageQuantity = inventoryCard.languageQuantities.find(
          (langQty) =>
            langQty.languageName?.toLowerCase() === selectedLanguageEnglish?.toLowerCase() ||
            langQty.languageName === selectedLanguage,
        )

        if (languageQuantity) {
          stockFoil = languageQuantity.foilQuantity || 0
          stockNonFoil = languageQuantity.nonFoilQuantity || 0
        } else {
          stockFoil = inventoryCard.languageQuantities.reduce(
            (total, lang) => total + (lang.foilQuantity || 0),
            0,
          )
          stockNonFoil = inventoryCard.languageQuantities.reduce(
            (total, lang) => total + (lang.nonFoilQuantity || 0),
            0,
          )
        }
      }
    }
  } catch (error) {
    stockFoil = card.stockFoil || 0
    stockNonFoil = card.stockNonFoil || 0
    if (!selectedLanguage) {
      selectedLanguage = card.language || 'Anglais'
    }
  }

  const stock = isFoil ? stockFoil : stockNonFoil

  if (!selectedLanguage) {
    selectedLanguage = 'Anglais'
  }

  const currentCartItem = cartStore.items.find(
    (item) =>
      item.id === card.id &&
      (item.language || 'Anglais') === selectedLanguage &&
      (item.isFoil || false) === isFoil,
  )
  const currentQuantity = currentCartItem ? currentCartItem.quantity : 0
  const clientLimit = card.limit || 4
  if (stock <= 0) {
    Notify.create({
      type: 'negative',
      message: `Stock épuisé pour cet article (${selectedLanguage} - ${isFoil ? 'Foil' : 'Regular'}).`,
      position: 'top',
    })
    return
  }

  if (currentQuantity >= clientLimit) {
    Notify.create({
      type: 'warning',
      message: `Limite d'achat atteinte (${clientLimit} exemplaires maximum).`,
      position: 'top',
    })
    return
  }

  if (currentQuantity + 1 > stock) {
    Notify.create({
      type: 'negative',
      message: `Stock insuffisant. Vous avez déjà ${currentQuantity} exemplaire(s) dans votre panier.`,
      position: 'top',
    })
    return
  }

  const product = {
    id: card.id,
    name: card.name,
    description: `${card.serie} - ${card.rarete}`,
    price: originalPrice,
    discount: card.discount || 0,
    limit: 5,
    stock: stock,
    stockFoil: stockFoil,
    stockNonFoil: stockNonFoil,
    isFoil: isFoil,
    image: card.img,
    setName: card.serie,
    number: card.numero,
    rarity: card.rarete,
    scryfallId: card.scryfallId,
    language: selectedLanguage,
  }

  cartStore.addItem(product)
}

// Prix original
const getOriginalPrice = (card) => {
  const isFoil = getPriceType(card) === 'foil'
  if (isFoil) {
    return parseFloat(card.foilPrice || 0)
  }
  return parseFloat(card.price || 0)
}

const getDiscountedPrice = (card, discount) => {
  const originalPrice = getOriginalPrice(card)
  const discountedPrice = originalPrice * (1 - discount / 100)
  return discountedPrice
}

// Prix final à utiliser pour le panier
const getDisplayPrice = (card) => {
  const originalPrice = getOriginalPrice(card)

  // Si il y a un rabais
  if (card.discount && card.discount > 0) {
    return parseFloat(getDiscountedPrice(card, card.discount))
  }

  // Sinon retourner le prix original
  return originalPrice
}

const getPriceType = (card) => {
  const wantsFoil = props.selectedFinishes?.includes('Foil')
  const wantsNonFoil = props.selectedFinishes?.includes('Non-Foil')

  if (wantsFoil && !wantsNonFoil) return 'foil'
  if (wantsNonFoil && !wantsFoil) return 'normal'

  const hasNormalStock = (card.stockNonFoil || 0) > 0
  const hasFoilStock = (card.stockFoil || 0) > 0
  const hasNormalPrice = (card.price || 0) > 0
  const hasFoilPrice = (card.foilPrice || 0) > 0

  if (hasNormalStock && hasNormalPrice) return 'normal'

  if (!hasNormalStock && hasFoilStock && hasFoilPrice) return 'foil'

  if (hasNormalPrice) return 'normal'
  if (hasFoilPrice) return 'foil'

  return 'normal'
}

const getCardStock = (card) => {
  const isFoil = getPriceType(card) === 'foil'
  if (isFoil) {
    return card.stockFoil || card.totalQuantity || 0
  } else {
    return card.stockNonFoil || card.totalQuantity || 0
  }
}
</script>
