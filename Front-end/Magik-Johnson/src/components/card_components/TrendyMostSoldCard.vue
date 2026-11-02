<script setup>
import { useRouter } from 'vue-router'
import { computed } from 'vue'
import { useCartStore } from '@/stores/cartStore'
import { cardService } from '@/services/CardService'
import { formatPrice } from '@/services/priceFormatter'
import { Notify } from 'quasar'
import '../../assets/catalogue.css'

const router = useRouter()
const cartStore = useCartStore()

const props = defineProps({
  card: {
    type: Object,
    required: true,
  },
})
const card = props.card

// Détermine quel type de prix est affiché pour l'effet foil
const getPriceType = computed(() => {
  const hasNormalStock = (card.stockNonFoil || 0) > 0
  const hasFoilStock = (card.stockFoil || 0) > 0
  const hasNormalPrice = (card.price || 0) > 0
  const hasFoilPrice = (card.foilPrice || 0) > 0

  if (hasNormalStock && hasNormalPrice) return 'normal'
  if (!hasNormalStock && hasFoilStock && hasFoilPrice) return 'foil'

  if (hasNormalPrice) return 'normal'
  if (hasFoilPrice) return 'foil'

  return 'normal'
})

const displayPrice = computed(() => {
  if (getPriceType.value === 'foil') {
    return props.card.foilPrice || 0
  }
  return props.card.price || 0
})

function goToRoute() {
  cardService.updateCardClickCount(props.card.id)

  router.push({
    name: 'DetailsCarte',
    params: {
      id: props.card.id,
    },
  })
}

const normalizeLanguage = (langName) => {
  if (!langName) return null
  const langMap = {
    Anglais: 'English',
    Français: 'French',
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
  }
  return langMap[langName] || langName
}

const addToCart = async (card) => {
  try {
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
        return availableLanguages[0]
      }

      return 'Anglais'
    }

    let selectedLanguage = null
    let stockFoil = 0
    let stockNonFoil = 0
    let isFoil = false

    const inventoryResponse = await cardService.getInventoryCards(1, 1, {
      searchQuery: card.name,
    })

    if (inventoryResponse && inventoryResponse.cards && inventoryResponse.cards.length > 0) {
      const inventoryCard = inventoryResponse.cards.find((c) => c.id === card.id)

      if (inventoryCard) {
        if (inventoryCard.languageQuantities && inventoryCard.languageQuantities.length > 0) {
          selectedLanguage = determineLanguageFromInventory(inventoryCard.languageQuantities)

          const selectedLanguageEnglish = normalizeLanguage(selectedLanguage) || 'English'

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

        const hasNormalStock = stockNonFoil > 0
        const hasFoilStock = stockFoil > 0
        const hasNormalPrice = (inventoryCard.price || 0) > 0
        const hasFoilPrice = (inventoryCard.foilPrice || 0) > 0

        if (hasNormalStock && hasNormalPrice) isFoil = false
        else if (!hasNormalStock && hasFoilStock && hasFoilPrice) isFoil = true
        else if (hasNormalPrice) isFoil = false
        else if (hasFoilPrice) isFoil = true
      }
    }

    if (!selectedLanguage) {
      selectedLanguage = 'Anglais'
    }

    let originalPrice = card.price
    if (card.price === 0 && card.foilPrice > 0) {
      originalPrice = card.foilPrice
      isFoil = true
    }

    const stock = isFoil ? stockFoil : stockNonFoil

    const product = {
      id: card.id,
      name: card.name,
      description: `${card.serie} - ${card.rarete}`,
      price: originalPrice,
      discount: card.discount || 0,
      limit: 5,
      image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image`,
      setName: card.serie,
      number: card.numero,
      rarity: card.rarete,
      scryfallId: card.scryfallId || 'fake-scryfall-id',
      language: selectedLanguage,
      isFoil: isFoil,
      stock: stock,
      stockFoil: stockFoil,
      stockNonFoil: stockNonFoil,
    }

    cartStore.addItem(product)
  } catch (error) {
    Notify.create({
      type: 'negative',
      message: "Erreur lors de l'ajout au panier",
      position: 'top',
    })
  }
}

// Retirer une carte du panier
const removeFromCart = async (cardId, languageName) => {
  let lang = languageName
  if (!lang) {
    const item = cartStore.items.find((i) => i.id === cardId)
    if (item) {
      lang = item.language
    }
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
      return availableLanguages[0]
    }

    return 'Anglais'
  }

  let selectedLanguage = null
  let stockFoil = 0
  let stockNonFoil = 0
  let isFoil = false

  const inventoryResponse = await cardService.getInventoryCards(1, 1, {
    searchQuery: card.name,
  })

  if (inventoryResponse && inventoryResponse.cards && inventoryResponse.cards.length > 0) {
    const inventoryCard = inventoryResponse.cards.find((c) => c.id === card.id)

    if (inventoryCard) {
      if (inventoryCard.languageQuantities && inventoryCard.languageQuantities.length > 0) {
        selectedLanguage = determineLanguageFromInventory(inventoryCard.languageQuantities)

        const selectedLanguageEnglish = normalizeLanguage(selectedLanguage) || 'English'

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

      const hasNormalStock = stockNonFoil > 0
      const hasFoilStock = stockFoil > 0
      const hasNormalPrice = (inventoryCard.price || 0) > 0
      const hasFoilPrice = (inventoryCard.foilPrice || 0) > 0

      if (hasNormalStock && hasNormalPrice) isFoil = false
      else if (!hasNormalStock && hasFoilStock && hasFoilPrice) isFoil = true
      else if (hasNormalPrice) isFoil = false
      else if (hasFoilPrice) isFoil = true
    }
  }
  if (card.price === 0 && card.foilPrice > 0) {
    isFoil = true
  }

  cartStore.removeItem(cardId, lang, isFoil)
}

// Vérifier si une carte est dans le panier
const isCardInCart = (cardId) => {
  return cartStore.isInCart(cardId)
}

// Prix à afficher Logique
const getDisplayPrice = (card) => {
  // Calculer le prix original (avant rabais)
  let originalPrice = card.price
  if (card.price === 0 && card.foilPrice > 0) {
    originalPrice = card.foilPrice
  }

  // Appliquer le rabais s'il existe
  const discount = card.discount || 0
  if (discount > 0 && originalPrice > 0) {
    return originalPrice * (1 - discount / 100)
  }

  return originalPrice || 0
}

// Prix original (avant rabais)
const getOriginalPrice = (card) => {
  if (getPriceType.value === 'foil') {
    return card.foilPrice || 0
  }
  return card.price || 0
}

// Vérifier si la carte est disponible
const isCardAvailable = (card) => {
  const price = parseFloat(card.price || 0)
  const foilPrice = parseFloat(card.foilPrice || 0)
  const hasPrice = price > 0 || foilPrice > 0

  const stock = getCardStock(card)
  return hasPrice && stock > 0
}

const getCardStock = (card) => {
  const type = getPriceType.value
  const isFoil = type === 'foil'

  if (isFoil) {
    return card.stockFoil !== undefined ? card.stockFoil : card.totalQuantity || 0
  } else {
    return card.stockNonFoil !== undefined ? card.stockNonFoil : card.totalQuantity || 0
  }
}
</script>

<template>
  <div class="card">
    <div class="horizontal_container">
      <div class="card_item">
        <div class="image-wrapper" @click="goToRoute">
          <img
            class="mtg-card-img"
            :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image`"
            :alt="`Image de ${card?.name}`"
          />
          <div v-if="getPriceType === 'foil'" class="foil-overlay"></div>
          <div
            v-if="card.onlineOnly || (!isCardAvailable(card) && getCardStock(card) > 0)"
            class="out-of-stock-overlay"
          >
            <span>Non disponible</span>
          </div>
          <div v-else-if="getCardStock(card) === 0" class="out-of-stock-overlay">
            <span>Stock épuisé</span>
          </div>
        </div>
      </div>
      <div class="card_item">
        <p class="nom_carte">{{ card.name }}</p>
        <p class="nom_set">{{ card.setName }}</p>
        <div class="price-container">
          <span
            v-if="card.discount && card.discount > 0 && getOriginalPrice(card) > 0"
            class="label_prix discounted"
          >
            {{ formatPrice(getDisplayPrice(card)) }} $
          </span>
          <span
            v-else
            class="label_prix"
            :style="{ visibility: displayPrice > 0 ? 'visible' : 'hidden' }"
          >
            {{ displayPrice > 0 ? formatPrice(displayPrice) + ' $' : '0,00 $' }}
          </span>
          <span
            v-if="card.discount && card.discount > 0 && getOriginalPrice(card) > 0"
            class="label_ancien_prix"
          >
            {{ formatPrice(getOriginalPrice(card)) }} $
          </span>
        </div>
      </div>
    </div>
    <div
      class="mtg-card-actions"
      style="justify-content: center; margin-bottom: auto; align-items: center"
    >
      <button class="btn btn-primary btn-sm" type="button" @click="goToRoute">Voir carte</button>
      <button
        type="button"
        :class="[
          'btn btn-sm',
          isCardInCart(card.id)
            ? 'btn-danger'
            : !isCardAvailable(card) || card.onlineOnly
              ? 'btn-secondary'
              : 'btn-primary',
        ]"
        :style="
          isCardInCart(card.id)
            ? { backgroundColor: '#dc3545', borderColor: '#dc3545', color: 'white' }
            : !isCardAvailable(card) || card.onlineOnly
              ? {
                  backgroundColor: '#6c757d',
                  borderColor: '#6c757d',
                  color: 'white',
                  cursor: 'not-allowed',
                }
              : {}
        "
        @click="isCardInCart(card.id) ? removeFromCart(card.id, card.language) : addToCart(card)"
        :disabled="!isCardAvailable(card) && !isCardInCart(card.id)"
      >
        {{
          isCardInCart(card.id)
            ? 'Retirer du panier'
            : card.onlineOnly
              ? 'Non disponible'
              : getCardStock(card) === 0
                ? 'Stock épuisé'
                : isCardAvailable(card)
                  ? 'Ajouter au panier'
                  : 'Non disponible'
        }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.card {
  border: solid 0px black;
  border-right: solid 2px rgb(45, 44, 44);
  width: 340px;
  min-width: 340px;
  height: 300px;
  margin: 0;
  background-color: #f0eada;
  transition-duration: 0.5s;
  display: flex;
  flex-direction: column;
}

.card:hover {
  box-shadow: 0 0 10px 5px;
}

.horizontal_container {
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  justify-content: flex-start;
  align-items: flex-start;
  padding: 5px;
}

.card_item {
  width: 140px;
  height: 200px;
  margin: 6px;
}

.card_item > p {
  margin: 2px;
  max-width: 100%;
}

.card_item:last-child {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  overflow: hidden;
  width: 160px;
}

.image-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  transition: transform 0.3s ease;
  cursor: pointer;
}

.image-wrapper:hover {
  transform: scale(1.05);
}

.mtg-card-img {
  width: 100%;
  height: 100%;
  object-fit: fill;
  position: relative;
  z-index: 1;
}

.foil-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(
    45deg,
    rgba(255, 0, 255, 0.3),
    rgba(0, 255, 255, 0.3),
    rgba(255, 255, 0, 0.3),
    rgba(255, 0, 255, 0.3)
  );
  background-size: 400% 400%;
  animation: foilShine 3s ease infinite;
  pointer-events: none;
  z-index: 2;
}

.foil-overlay::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 20%;
  height: 100%;
  background: linear-gradient(to right, transparent, rgba(255, 255, 255, 0.4) 50%, transparent);
  animation: lightSweep 4s ease-in-out infinite;
  pointer-events: none;
}

@keyframes foilShine {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

@keyframes lightSweep {
  0% {
    left: 0%;
    opacity: 1;
  }
  25% {
    left: 80%;
    opacity: 1;
  }
  100% {
    left: 80%;
    opacity: 0;
  }
}

.nom_carte {
  font-size: 110%;
  font-weight: bold;
  word-wrap: break-word;
  overflow-wrap: break-word;
  hyphens: auto;
  max-width: 100%;
  line-height: 1.25;
  margin-bottom: 5px;
}

.nom_set {
  font-size: 90%;
  opacity: 0.8;
}

.label_prix {
  font-size: 120%;
  font-weight: bold;
  color: black;
}

.label_prix.discounted {
  color: red;
}

.label_ancien_prix {
  font-size: 75%;
  text-decoration: line-through;
  color: black;
  font-weight: bold;
  margin-left: 8px;
}

.price-container {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.price-container {
  position: relative;
  top: 12px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.4rem;
  margin-top: 8px;
}

.material-symbols-outlined {
  font-size: small;
}

.out-of-stock-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 3;
  color: white;
  font-weight: bold;
  text-transform: uppercase;
  font-size: 14px;
  text-align: center;
  padding: 5px;
}
</style>

<style>
body.body--dark .card {
  background-color: rgb(35, 42, 56) !important;
  color: white !important;
  border-right: solid 2px rgba(255, 255, 255, 0.1);
}

body.body--dark .label_prix {
  color: white !important;
}

body.body--dark .label_prix.discounted {
  color: red !important;
}

body.body--dark .label_ancien_prix {
  color: white !important;
}

body.body--dark .nom_carte {
  color: white !important;
}

body.body--dark .nom_set {
  color: #ccc !important;
}
</style>
