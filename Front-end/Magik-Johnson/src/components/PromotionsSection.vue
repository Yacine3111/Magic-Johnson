<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import cardPromotion from './card_components/CardPromotion.vue'
import cardPromotionMobile from './card_components/CardPromotionMobile.vue'
import { promotionService } from '@/services/PromotionService'
import { cardService } from '@/services/CardService'
import { useCartStore } from '@/stores/cartStore'
import TranslationService from '@/services/translationService'

const router = useRouter()

const emit = defineEmits(['promotions-updated'])

const activePromoIndex = ref(0)
const activeCardIndices = ref({})
const nextPromotion = () => {
  if (activePromoIndex.value < listeDePromotions.value.length - 1) {
    activePromoIndex.value++
  } else {
    activePromoIndex.value = 0
  }
}

const prevPromotion = () => {
  if (activePromoIndex.value > 0) {
    activePromoIndex.value--
  } else {
    activePromoIndex.value = listeDePromotions.value.length - 1
  }
}

// Helper to ensure index exists
const getCardIndex = (promoId) => {
  if (activeCardIndices.value[promoId] === undefined) {
    activeCardIndices.value[promoId] = 0
  }
  return activeCardIndices.value[promoId]
}

const prevCard = (promoId) => {
  const promo = listeDePromotions.value.find((p) => p.id === promoId)
  if (!promo || !promo.cartes) return

  const currentIndex = getCardIndex(promoId)
  if (currentIndex > 0) {
    activeCardIndices.value[promoId]--
  } else {
    // Loop to end
    activeCardIndices.value[promoId] = promo.cartes.length - 1
  }
}

const nextCard = (promoId) => {
  const promo = listeDePromotions.value.find((p) => p.id === promoId)
  if (!promo || !promo.cartes) return

  const currentIndex = getCardIndex(promoId)
  if (currentIndex < promo.cartes.length - 1) {
    activeCardIndices.value[promoId]++
  } else {
    // Loop to start
    activeCardIndices.value[promoId] = 0
  }
}

// Swipe handlers
const handlePromoSwipe = ({ direction }) => {
  if (direction === 'left') nextPromotion()
  if (direction === 'right') {
    if (activePromoIndex.value > 0) activePromoIndex.value--
    else activePromoIndex.value = listeDePromotions.value.length - 1
  }
}

const handleCardSwipe = ({ evt, direction }, promoId) => {
  if (evt) evt.stopPropagation()
  if (direction === 'left') nextCard(promoId)
  if (direction === 'right') prevCard(promoId)
}

const controlTypeCarouselExterieur = ref('outline')
const controlTypeCarouselInterieur = ref('flat')
const button_voirOffre_text = ref('Voir les autres offres de cette promotion !')

const autoplay = ref(true)

// Variables pour le countdown dynamique
const currentPromotionCountdown = ref(null)
let countdownInterval = null
const countdownData = ref({})
const currentTime = ref(Date.now())

// Fuseau horaire Québec
const TARGET_TIME_ZONE = 'America/Toronto'

const getZonedEpochMs = (dateInput, timeZone = TARGET_TIME_ZONE) => {
  const date = dateInput instanceof Date ? dateInput : new Date(dateInput)
  const parts = new Intl.DateTimeFormat('en-CA', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  })
    .formatToParts(date)
    .reduce((acc, p) => {
      if (p.type !== 'literal') acc[p.type] = p.value
      return acc
    }, {})

  const year = Number(parts.year) || 0
  const month = (Number(parts.month) || 1) - 1
  const day = Number(parts.day) || 1
  const hour = Number(parts.hour) || 0
  const minute = Number(parts.minute) || 0
  const second = Number(parts.second) || 0

  return Date.UTC(year, month, day, hour, minute, second)
}

const getZonedNowMs = (timeZone = TARGET_TIME_ZONE) => {
  return getZonedEpochMs(new Date(currentTime.value), timeZone)
}

// Map pour stocker les countdowns réactifs de chaque promotion
const promotionCountdowns = ref(new Map())

// État pour les promotions
const loading = ref(false)
const error = ref(null)
const listeDePromotions = ref([])

// obtenir le countdown réactif d'une promotion
const getPromotionCountdown = (promo) => {
  if (!promo || !promotionCountdowns.value.has(promo.id)) {
    return {
      secondes: 0,
      minutes: 0,
      hours: 0,
      days: 0,
      isStartOfPromotion: false,
    }
  }

  return promotionCountdowns.value.get(promo.id).value
}

// Countdown timer
const timeCountDown = computed(() => {
  if (!currentPromotionCountdown.value) {
    return {
      secondes: 0,
      minutes: 0,
      hours: 0,
      days: 0,
      isStartOfPromotion: false,
    }
  }

  return getPromotionCountdown(currentPromotionCountdown.value)
})

const mainPromotionCarouselFirstSlide = ref('loading')
const secondaryPromotionCarouselSlides = ref({})

// Détection mobile
const $q = useQuasar()
const cartStore = useCartStore()

const isMobile = computed(() => {
  if (typeof window === 'undefined') return false
  return window.innerWidth < 768 || $q.screen.xs || $q.screen.sm
})

const initializeSecondarySlides = () => {
  listeDePromotions.value.forEach((promo) => {
    if (promo.cartes && promo.cartes.length > 0) {
      const firstCard = promo.cartes[0]
      secondaryPromotionCarouselSlides.value[promo.title || promo.nom] = firstCard.id
    } else {
      // Initialiser avec 'empty' si pas de cartes
      secondaryPromotionCarouselSlides.value[promo.title || promo.nom] = 'empty'
    }
  })
}

// Fcréer un countdown réactif pour une promotion
const createReactiveCountdown = (promo) => {
  return computed(() => {
    if (!promo) {
      return {
        secondes: 0,
        minutes: 0,
        hours: 0,
        days: 0,
        isStartOfPromotion: false,
      }
    }

    const nowMs = currentTime.value
    const startMs = new Date(promo.startDate).getTime()
    const endMs = new Date(promo.endDate).getTime()

    let targetMs
    let isStartOfPromotion = false

    if (nowMs < startMs) {
      targetMs = startMs
      isStartOfPromotion = true
    } else if (nowMs >= startMs && nowMs <= endMs) {
      // Promotion en cours
      targetMs = endMs
      isStartOfPromotion = false
    } else {
      // Promotion terminée
      return {
        secondes: 0,
        minutes: 0,
        hours: 0,
        days: 0,
        isStartOfPromotion: false,
      }
    }

    const timeLeft = Math.max(0, Math.floor((targetMs - nowMs) / 1000))

    return {
      secondes: timeLeft % 60,
      minutes: Math.floor((timeLeft / 60) % 60),
      hours: Math.floor((timeLeft / 3600) % 24),
      days: Math.floor(timeLeft / 86400),
      isStartOfPromotion,
    }
  })
}

// Fonction pour mettre à jour le countdown avec la promotion actuelle
const updateCountdownForCurrentPromotion = () => {
  if (listeDePromotions.value.length > 0) {
    currentPromotionCountdown.value = listeDePromotions.value[0]

    promotionCountdowns.value.clear()
    listeDePromotions.value.forEach((promo) => {
      promotionCountdowns.value.set(promo.id, createReactiveCountdown(promo))
    })
  }
}

// Fonction pour démarrer le timer de mise à jour
const startCountdownTimer = () => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }

  countdownInterval = setInterval(() => {
    currentTime.value = Date.now()
  }, 1000)
}

async function handleAddToCart(card) {
  try {
    const determineLanguageFromInventory = (languageQuantities) => {
      return TranslationService.determineBestLanguageFromInventory(languageQuantities)
    }

    let selectedLanguage = null
    let stockFoil = 0
    let stockNonFoil = 0
    let isFoil = false

    try {
      const inventoryResponse = await cardService.getInventoryCards(1, 1000, {
        searchQuery: card.nom_carte || card.name,
      })

      if (inventoryResponse && inventoryResponse.cards && inventoryResponse.cards.length > 0) {
        const inventoryCard = inventoryResponse.cards.find((c) => c.id === card.id)

        if (inventoryCard) {
          if (inventoryCard.languageQuantities && inventoryCard.languageQuantities.length > 0) {
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

            isFoil = inventoryCard.price <= 0 && inventoryCard.foilPrice > 0
          } else {
            try {
              const cardResponse = await cardService.getCardById(card.id)
              if (cardResponse?.data?.languages && cardResponse.data.languages.length > 0) {
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
                const availableLanguages = cardResponse.data.languages.map(
                  (lang) => languageMap[lang] || lang,
                )

                if (availableLanguages.includes('Français')) {
                  selectedLanguage = 'Français'
                } else if (availableLanguages.length > 0) {
                  selectedLanguage = availableLanguages[0]
                }
              }
            } catch (cardError) {}
          }
        } else {
          try {
            const cardResponse = await cardService.getCardById(card.id)
            if (cardResponse?.data?.languages && cardResponse.data.languages.length > 0) {
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
              const availableLanguages = cardResponse.data.languages.map(
                (lang) => languageMap[lang] || lang,
              )

              if (availableLanguages.includes('Français')) {
                selectedLanguage = 'Français'
              } else if (availableLanguages.length > 0) {
                selectedLanguage = availableLanguages[0]
              }
            }
          } catch (cardError) {}
        }
      } else {
      }
    } catch (error) {
      if (card.isFoil !== undefined) {
        isFoil = card.isFoil
      } else {
        const prixNormal = parseFloat(card.prix) || parseFloat(card.price) || 0
        const prixFoil = parseFloat(card.prixFoil) || parseFloat(card.foilPrice) || 0
        isFoil = prixNormal <= 0 && prixFoil > 0
      }
      if (!selectedLanguage) {
        selectedLanguage = 'Anglais'
      }
    }

    const originalPrice = isFoil
      ? parseFloat(card.prixFoil) ||
        parseFloat(card.foilPrice) ||
        parseFloat(card.prix) ||
        parseFloat(card.price) ||
        0
      : parseFloat(card.prix) ||
        parseFloat(card.price) ||
        parseFloat(card.prixFoil) ||
        parseFloat(card.foilPrice) ||
        0

    const stock = isFoil ? stockFoil : stockNonFoil

    if (!selectedLanguage) {
      selectedLanguage = card.language || 'Anglais'
    }

    const finalLanguage = selectedLanguage || 'Anglais'

    const product = {
      id: card.id,
      name: card.nom_carte || card.name,
      price: originalPrice,
      discount: card.discount || 0,
      image: card.image_src || card.image,
      setName: card.nom_set || card.setName,
      number: card.numero || card.number,
      rarity: card.rarity,
      scryfallId: card.scryfallId || card.id,
      description: `${card.nom_set || card.setName} - ${card.rarity}`,
      quantity: 1,
      limit: 5,
      stock: stock || 0,
      stockFoil: stockFoil || 0,
      stockNonFoil: stockNonFoil || 0,
      isFoil: isFoil || false,
      language: finalLanguage,
    }

    if (!product.language || product.language === null || product.language === undefined) {
      product.language = 'Anglais'
    }

    Object.keys(product).forEach((key) => {
      if (product[key] === undefined) {
      }
    })

    cartStore.addItem(product)
  } catch (error) {
    throw error
  }
}

// Fonction pour charger les promotions depuis le backend
const loadPromotions = async () => {
  loading.value = true
  error.value = null

  try {
    const promotions = await promotionService.getAllPromotions()

    if (promotions && promotions.length > 0) {
      const now = new Date()
      const relevantPromotions = promotions.filter((promo) => {
        const startDate = new Date(promo.startDate)
        const endDate = new Date(promo.endDate)
        return (
          (promo.status === 'Actif' || promo.status === 'En Attente') &&
          startDate <= endDate &&
          ((startDate <= now && endDate >= now) || startDate > now)
        )
      })

      //fix le chargement de promotion quand ya trop de carte LOCK IN CHRISTIAN KETSUVA
      const promotionsWithCards = await Promise.all(
        relevantPromotions.slice(0, 5).map(async (promo) => {
          //FIX POUR LIMITER A 5 PROMOTIONS LOCK IN YACINE
          try {
            const promotionCards = await promotionService.getPromotionCards(promo.id)

            if (!promotionCards || promotionCards.length === 0) {
              return {
                ...promo,
                cartes: [],
              }
            }

            const allCardIds = promotionCards.map((card) => card.id || card)

            const maxCardsToLoad = Math.min(allCardIds.length, 50)
            const cardIdsToLoad = allCardIds.slice(0, maxCardsToLoad)

            const allFullCards = await Promise.all(
              cardIdsToLoad.map(async (cardId) => {
                try {
                  return await cardService.getCard(cardId)
                } catch (error) {
                  return promotionCards.find((c) => (c.id || c) === cardId) || null
                }
              }),
            )

            const sortedCards = allFullCards
              .filter((card) => {
                if (!card) return false
                return true
              })
              .sort((a, b) => {
                const stockA = a.totalQuantity || 0
                const stockB = b.totalQuantity || 0
                const priceA = a.price || 0
                const foilPriceA = a.foilPrice || 0
                const priceB = b.price || 0
                const foilPriceB = b.foilPrice || 0
                const hasPriceA = priceA > 0 || foilPriceA > 0
                const hasPriceB = priceB > 0 || foilPriceB > 0
                const hasStockA = stockA > 0
                const hasStockB = stockB > 0

                if (hasStockA && hasPriceA && !(hasStockB && hasPriceB)) return -1
                if (hasStockB && hasPriceB && !(hasStockA && hasPriceA)) return 1

                if (hasPriceA && !hasPriceB) return -1
                if (hasPriceB && !hasPriceA) return 1

                if (hasStockA && !hasStockB) return -1
                if (hasStockB && !hasStockA) return 1

                if (hasStockA && hasStockB) {
                  return stockB - stockA
                }

                if (hasPriceA && hasPriceB) {
                  const effectivePriceA = priceA > 0 ? priceA : foilPriceA
                  const effectivePriceB = priceB > 0 ? priceB : foilPriceB
                  return effectivePriceA - effectivePriceB
                }

                return 0
              })

            const selectedCards = sortedCards.slice(0, 5)

            const mappedCards = selectedCards
              .map((fullCard) => {
                const card = promotionCards.find((c) => (c.id || c) === fullCard.id)

                let setName = 'Set inconnu'
                if (fullCard.setName) {
                  setName = fullCard.setName
                } else if (fullCard.setId) {
                  setName = `Set ${fullCard.setId.substring(0, 8)}`
                }

                let cardColor = 'Inconnue'
                if (fullCard.colors) {
                  cardColor = fullCard.colors
                } else if (fullCard.manaCost) {
                  const colors = fullCard.manaCost.match(/[WUBRG]/g) || []
                  if (colors.length > 0) {
                    const colorMap = { W: 'Blanc', U: 'Bleu', B: 'Noir', R: 'Rouge', G: 'Vert' }
                    cardColor = [...new Set(colors)].map((c) => colorMap[c]).join(', ')
                  }
                }

                // Calculer les prix avec discount
                const discountPercentage = promo.rate || 0
                const originalPrice = fullCard.price || 0
                const foilPrice = fullCard.foilPrice || 0

                // Déterminer quel prix utiliser (normal ou foil)
                let basePrice = originalPrice
                let isFoil = false

                if (originalPrice === 0 && foilPrice > 0) {
                  // Si pas de prix normal mais prix foil disponible
                  basePrice = foilPrice
                  isFoil = true
                }

                // Calculer le prix après discount
                const discountedPrice = basePrice * (1 - discountPercentage / 100)

                return {
                  ...fullCard,
                  nom_carte: fullCard.name || 'Carte sans nom',
                  nom_set: setName,
                  serie: setName,
                  numero: fullCard.number || 'N/A',
                  image_src: `https://api.scryfall.com/cards/${fullCard.scryfallId}?format=image`,
                  image: `https://api.scryfall.com/cards/${fullCard.scryfallId}?format=image`,
                  alt_text: fullCard.name || 'Image de carte',
                  prix: discountedPrice.toFixed(2),
                  ancien_prix: basePrice.toFixed(2),

                  nom: fullCard.name || 'Carte sans nom',
                  couleur: cardColor,
                  rarete: fullCard.rarity || 'Inconnue',
                  stock: fullCard.totalQuantity || 0,
                  hasFoil: fullCard.hasFoil || false,
                  prixFoil: fullCard.foilPrice || 0,
                  isFoil: isFoil,
                }
              })
              .filter((card) => card !== null)

            if (mappedCards.length === 0 && promotionCards.length > 0) {
              const fallbackCards = promotionCards.slice(0, 5).map((card) => {
                const fallbackBasePrice = parseFloat(card?.price || 0)
                const fallbackDiscount = promo.rate || 0
                const fallbackDiscountedPrice = fallbackBasePrice * (1 - fallbackDiscount / 100)

                return {
                  ...card,
                  nom_carte: card?.name || 'Carte sans nom',
                  nom_set: `Set ${card?.setId?.substring(0, 8) || 'Inconnu'}`,
                  serie: `Set ${card?.setId?.substring(0, 8) || 'Inconnu'}`,
                  numero: card?.number || 'N/A',
                  image_src: card?.image || card?.imageUrl || '/default-card.png',
                  image: card?.image || card?.imageUrl || '/default-card.png',
                  alt_text: card?.name || 'Image de carte',
                  prix: fallbackDiscountedPrice.toFixed(2),
                  ancien_prix: fallbackBasePrice.toFixed(2),
                  nom: card?.name || 'Carte sans nom',
                  couleur: 'Inconnue',
                  rarete: card?.rarity || 'Inconnue',
                  stock: card?.totalQuantity || 0,
                  hasFoil: card?.hasFoil || false,
                  prixFoil: card?.foilPrice || 0,
                }
              })
              mappedCards.push(...fallbackCards)
            }

            return {
              ...promo,
              cartes: mappedCards,
            }
          } catch (error) {
            return {
              ...promo,
              cartes: [],
            }
          }
        }),
      )

      listeDePromotions.value = promotionsWithCards

      // Initialiser les slides si on a des promotions
      if (listeDePromotions.value.length > 0) {
        mainPromotionCarouselFirstSlide.value =
          listeDePromotions.value[0].title || listeDePromotions.value[0].nom
        initializeSecondarySlides()

        // Initialiser le countdown avec la première promotion
        updateCountdownForCurrentPromotion()
        startCountdownTimer()
      } else {
        mainPromotionCarouselFirstSlide.value = 'no-promotions'
      }
    } else {
      listeDePromotions.value = []
      mainPromotionCarouselFirstSlide.value = 'no-promotions'
    }

    emit('promotions-updated', listeDePromotions.value.length > 0)
  } catch (err) {
    error.value = 'Erreur lors du chargement des promotions'
    listeDePromotions.value = []

    emit('promotions-updated', false)
  } finally {
    loading.value = false
  }
}

// Fonction pour recharger les promotions
const reloadPromotions = async () => {
  await loadPromotions()
}

// Initialisation au montage du composant
onMounted(async () => {
  mainPromotionCarouselFirstSlide.value = 'loading'
  secondaryPromotionCarouselSlides.value['loading'] = 'loading-spinner'
  await loadPromotions()

  // Démarrer le countdown timer
  startCountdownTimer()
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})

const hasPromotions = computed(() => {
  return listeDePromotions.value.length > 0
})

const isLongText = (text) => {
  return text && text.length > 180
}

const goToPromotionCards = (promo) => {
  if (promo && promo.id) {
    router.push({
      name: 'PromotionCartes',
      params: {
        id: promo.id,
      },
    })
  }
}

defineExpose({
  reloadPromotions,
  hasPromotions,
})
</script>

<template>
  <div v-if="hasPromotions || loading" class="promotionSection_container">
    <hr />
    <h5 class="titre_promotion">Promotions</h5>

    <div v-if="error" class="text-center q-pa-lg text-negative">
      {{ error }}
    </div>

    <div v-if="$q.screen.gt.sm">
      <div class="q-pa-sm">
        <q-carousel
          v-model="mainPromotionCarouselFirstSlide"
          swipeable
          animated
          :control-type="controlTypeCarouselExterieur"
          control-color="grey"
          navigation
          infinite
          :autoplay="autoplay && !loading"
          transition-prev="slide-right"
          transition-next="slide-left"
          @mouseenter="autoplay = false"
          @mouseleave="autoplay = true"
          padding
          arrows
          height="400px"
          class="text-black rounded-borders caroussel_exterieur"
        >
          <q-carousel-slide v-if="loading" name="loading" class="row no-wrap">
            <div class="col-4 promo-left-column">
              <div class="promo-title-section">
                <q-skeleton type="text" width="150px" />
              </div>
              <div class="promo-description-section">
                <q-skeleton type="text" width="90%" />
                <q-skeleton type="text" width="70%" />
                <q-skeleton type="text" width="80%" />
              </div>
              <div class="container_button_offre">
                <q-skeleton type="QBtn" class="button_voir_plus_offre shadow-12" />
              </div>
            </div>
            <div class="col-7 carousel-center-column">
              <div class="q-pa-md full-width">
                <q-carousel
                  v-model="secondaryPromotionCarouselSlides['loading']"
                  vertical
                  transition-prev="slide-down"
                  transition-next="slide-up"
                  swipeable
                  :control-type="controlTypeCarouselInterieur"
                  animated
                  control-color="black"
                  navigation-icon="radio_button_unchecked"
                  navigation
                  arrows
                  height="300px"
                  class="caroussel_interieur text-white shadow-1 rounded-borders shadow-12 full-width"
                >
                  <q-carousel-slide name="loading-spinner" class="column no-wrap flex-center">
                    <div class="promotion-card-content">
                      <div class="image-wrapper" style="width: 130px; height: 180px">
                        <q-skeleton width="130px" height="180px" />
                      </div>
                      <div class="promotion-card-info q-ml-md" style="flex: 1">
                        <div class="flex column flex-center">
                          <q-skeleton type="text" width="150px" class="q-mb-xs" />
                          <q-skeleton type="text" width="100px" class="q-mb-sm" />
                          <div class="row q-gutter-sm justify-center q-mb-sm">
                            <q-skeleton type="text" width="60px" />
                            <q-skeleton type="text" width="60px" />
                          </div>
                          <q-skeleton type="text" width="180px" class="q-mb-sm" />
                          <div class="flex column items-center q-gutter-xs full-width">
                            <q-skeleton type="QBtn" width="60%" height="28px" />
                            <q-skeleton type="QBtn" width="60%" height="28px" />
                          </div>
                        </div>
                      </div>
                    </div>
                  </q-carousel-slide>
                </q-carousel>
              </div>
            </div>
          </q-carousel-slide>
          <q-carousel-slide
            v-else-if="listeDePromotions.length === 0"
            name="no-promotions"
            class="row no-wrap items-center"
          >
            <div class="col-12 text-center">
              <p class="text-grey-6">Aucune promotion disponible</p>
            </div>
          </q-carousel-slide>
          <q-carousel-slide
            v-for="promo in listeDePromotions"
            :key="promo.id"
            :name="promo.title || promo.nom"
            class="row no-wrap"
          >
            <div :class="['promo-left-column', isLongText(promo.description) ? 'col-5' : 'col-3']">
              <div class="promo-title-section">
                <p class="text_offres_special">{{ promo.title || promo.nom }} &#8658;</p>
              </div>
              <div class="promo-description-section">
                <p>{{ promo.description }}</p>
              </div>
              <div class="container_button_offre">
                <q-btn
                  class="button_voir_plus_offre shadow-12"
                  size="xs"
                  text-color="black"
                  :label="button_voirOffre_text"
                  @click="goToPromotionCards(promo)"
                />
              </div>
            </div>

            <div class="col-7 carousel-center-column">
              <div class="q-pa-md full-width">
                <q-carousel
                  v-model="secondaryPromotionCarouselSlides[promo.title || promo.nom]"
                  vertical
                  transition-prev="slide-down"
                  transition-next="slide-up"
                  swipeable
                  :control-type="controlTypeCarouselInterieur"
                  animated
                  control-color="black"
                  navigation-icon="radio_button_unchecked"
                  navigation
                  arrows
                  height="300px"
                  class="caroussel_interieur text-white shadow-1 rounded-borders shadow-12 full-width"
                >
                  <q-carousel-slide
                    v-if="!promo.cartes || promo.cartes.length === 0"
                    name="empty"
                    class="column no-wrap flex-center"
                  >
                    <div class="text-center">
                      <q-spinner color="primary" size="2.5em" />
                    </div>
                  </q-carousel-slide>
                  <q-carousel-slide
                    v-for="promotionCard in promo.cartes"
                    :key="promotionCard.id"
                    :name="promotionCard.id"
                    class="column no-wrap flex-center"
                  >
                    <cardPromotion
                      :card="promotionCard"
                      :time-count-down="getPromotionCountdown(promo)"
                      :is-start-of-promotion="getPromotionCountdown(promo).isStartOfPromotion"
                      @add-to-cart="handleAddToCart"
                    />
                  </q-carousel-slide>
                </q-carousel>
              </div>
            </div>
          </q-carousel-slide>
        </q-carousel>
      </div>
    </div>

    <div v-else-if="$q.screen.lt.md">
      <div
        class="q-pa-sm mobile-custom-container"
        v-touch-swipe.mouse.left.right="handlePromoSwipe"
      >
        <div v-if="loading" class="mobile-slide-content">
          <div class="mobile-promo-header">
            <div class="mobile-promo-title">
              <q-skeleton type="text" width="150px" class="q-mx-auto" />
            </div>
            <div class="mobile-promo-description">
              <q-skeleton type="text" width="90%" class="q-mx-auto" />
              <q-skeleton type="text" width="70%" class="q-mx-auto" />
            </div>
          </div>
          <div class="mobile-inner-carousel">
            <div class="mobile-card-slide column no-wrap flex-center">
              <div class="promotion-card-content">
                <div class="image-wrapper" style="width: 130px; height: 180px">
                  <q-skeleton width="130px" height="180px" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="listeDePromotions.length === 0" class="text-center q-pa-lg">
          <p class="text-grey-6">Aucune promotion disponible</p>
        </div>

        <div v-else class="mobile-slide-content">
          <div class="mobile-promo-header">
            <div class="row items-center justify-between no-wrap full-width q-px-sm">
              <q-btn flat dense round color="grey-8" icon="chevron_left" @click="prevPromotion" />
              <div class="col text-center">
                <p class="mobile-promo-title q-mb-none">
                  {{
                    listeDePromotions[activePromoIndex].title ||
                    listeDePromotions[activePromoIndex].nom
                  }}
                </p>
              </div>
              <q-btn flat dense round color="grey-8" icon="chevron_right" @click="nextPromotion" />
            </div>
            <p class="text-caption text-grey-6 q-mt-xs q-mb-none text-center">
              Promotion {{ activePromoIndex + 1 }} / {{ listeDePromotions.length }}
            </p>
            <p class="mobile-promo-description">
              {{ listeDePromotions[activePromoIndex].description }}
            </p>
          </div>

          <div class="mobile-inner-carousel-wrapper">
            <q-btn
              flat
              round
              dense
              color="primary"
              icon="chevron_left"
              class="mobile-nav-btn prev-card-btn"
              @click="prevCard(listeDePromotions[activePromoIndex].id)"
            />

            <div
              class="mobile-inner-carousel"
              v-touch-swipe.mouse.left.right="
                (evt) => handleCardSwipe(evt, listeDePromotions[activePromoIndex].id)
              "
            >
              <div class="mobile-card-display">
                <transition name="fade" mode="out-in">
                  <cardPromotionMobile
                    v-if="
                      listeDePromotions[activePromoIndex].cartes &&
                      listeDePromotions[activePromoIndex].cartes.length > 0
                    "
                    :key="
                      listeDePromotions[activePromoIndex].cartes[
                        getCardIndex(listeDePromotions[activePromoIndex].id)
                      ].id
                    "
                    :card="
                      listeDePromotions[activePromoIndex].cartes[
                        getCardIndex(listeDePromotions[activePromoIndex].id)
                      ]
                    "
                    :time-count-down="getPromotionCountdown(listeDePromotions[activePromoIndex])"
                    :is-start-of-promotion="
                      getPromotionCountdown(listeDePromotions[activePromoIndex]).isStartOfPromotion
                    "
                    @add-to-cart="handleAddToCart"
                  />
                </transition>
                <div
                  v-if="
                    !listeDePromotions[activePromoIndex].cartes ||
                    listeDePromotions[activePromoIndex].cartes.length === 0
                  "
                  class="text-center"
                >
                  <q-spinner color="primary" size="2.5em" />
                </div>
              </div>
            </div>

            <q-btn
              flat
              round
              dense
              color="primary"
              icon="chevron_right"
              class="mobile-nav-btn next-card-btn"
              @click="nextCard(listeDePromotions[activePromoIndex].id)"
            />
          </div>

          <div class="mobile-voir-plus">
            <q-btn
              class="button_voir_plus_offre shadow-12"
              size="xs"
              text-color="black"
              color="white"
              :label="button_voirOffre_text"
              icon-right="add"
              @click="goToPromotionCards(listeDePromotions[activePromoIndex])"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="../css/PromotionSection.css" scoped></style>

<style>
body.body--dark .caroussel_exterieur,
body.body--dark .mobile-carousel {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
}

body.body--dark .caroussel_interieur,
body.body--dark .mobile-inner {
  background-color: rgb(35, 42, 56) !important;
  color: white !important;
}

body.body--dark .caroussel_interieur .q-carousel__slide,
body.body--dark .mobile-inner .q-carousel__slide {
  background-color: rgb(35, 42, 56) !important;
}

body.body--dark .container_button_offre .button_voir_plus_offre,
body.body--dark .mobile-voir-plus .button_voir_plus_offre {
  background-color: #2d3c49 !important;
  color: white !important;
}

body.body--dark .promo-title-section p,
body.body--dark .promo-description-section p,
body.body--dark .mobile-promo-title,
body.body--dark .mobile-promo-description {
  color: white !important;
}

body.body--dark .countDown_label_text {
  color: white !important;
}
</style>
