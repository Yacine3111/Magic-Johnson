<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { Notify, useQuasar } from 'quasar'
import { cardService } from '@/services/CardService'
import TranslationService from '@/services/translationService'
import '../assets/catalogue.css'

const route = useRoute()
const router = useRouter()
const $q = useQuasar()

let displayedCard = ref(null)
let scryfallVariants = ref([])
let selectedScryfallId = ref(null)
let selectedLanguage = ref('')
let showFoil = ref(false)
let otherCards = ref([])
let manaCostSymbols = ref(null)
let inventoryCard = ref(null)
let isLoading = ref(true)

const cardRotateX = ref(0)
const cardRotateY = ref(0)
const isHovering = ref(false)

const searchSeries = ref('')

const isDragging = ref(false)
const startX = ref(0)
const scrollLeft = ref(0)
const hasMovedDuringDrag = ref(false)

const canToggleFoil = computed(() => {
  return displayedCard.value?.hasFoil && displayedCard.value?.hasNonFoil
})

const canNotToggleFoil = computed(() => {
  return !displayedCard.value?.hasFoil && displayedCard.value?.hasNonFoil
})

const onlyShowFoil = computed(() => {
  return displayedCard.value?.hasFoil && !canToggleFoil.value
})

const isCreatureCard = computed(() => {
  if (!displayedCard.value?.types) return false
  return displayedCard.value.types.some((type) => type.toLowerCase() === 'creature')
})

const hasMoreThanTenEditions = computed(() => {
  return otherCards.value.length > 10
})

const filteredOtherCards = computed(() => {
  let filtered = otherCards.value
  if (searchSeries.value) {
    filtered = otherCards.value.filter((card) =>
      card.setName.toLowerCase().includes(searchSeries.value.toLowerCase()),
    )
  }
  return filtered.sort((a, b) => a.setName.localeCompare(b.setName))
})

// Prix original sans rabais
const originalPrice = computed(() => {
  if (!displayedCard.value) return 0

  if (showFoil.value) {
    if (displayedCard.value.foilPrice > 0) {
      return displayedCard.value.foilPrice
    } else if (displayedCard.value.price > 0) {
      return displayedCard.value.price
    }
  } else {
    if (displayedCard.value.price > 0) {
      return displayedCard.value.price
    } else if (displayedCard.value.foilPrice > 0) {
      return displayedCard.value.foilPrice
    }
  }

  return 0
})

const displayPrice = computed(() => {
  const original = originalPrice.value
  const discount = displayedCard.value?.discount || displayedCard.value?.Discount || 0

  if (discount > 0) {
    return original * (1 - discount / 100)
  }

  return original
})

const shouldShowFoilEffect = computed(() => {
  if (!displayedCard.value) return false
  if (showFoil.value) return false

  return displayedCard.value.price <= 0 && displayedCard.value.foilPrice > 0
})

const cardStyle = computed(() => {
  if (!isHovering.value) {
    return {
      transform: 'perspective(1000px) rotateX(0deg) rotateY(0deg) scale(1)',
      transition: 'transform 0.5s ease-out',
    }
  }

  return {
    transform: `perspective(1000px) rotateX(${cardRotateX.value}deg) rotateY(${cardRotateY.value}deg) scale(1.05)`,
    transition: 'transform 0.1s ease-out',
  }
})

const initializeFoilState = () => {
  if (displayedCard.value?.hasFoil && displayedCard.value?.hasNonFoil) {
    showFoil.value = false
  } else if (displayedCard.value?.hasFoil && !displayedCard.value?.hasNonFoil) {
    showFoil.value = true
  } else {
    showFoil.value = false
  }
}

const loadCard = async (cardId) => {
  isLoading.value = true

  try {
    const cardResponse = await cardService.getCardById(cardId)
    displayedCard.value = cardResponse.data

    if (displayedCard.value.manaCost != null) {
      manaCostSymbols.value = displayedCard.value.manaCost
      manaCostSymbols.value = manaCostSymbols.value.replace(/\W/gi, '')
    }

    const initialScryfallId = cardResponse.data.scryfallId
    selectedScryfallId.value = initialScryfallId
    selectedLanguage.value = ''
    searchSeries.value = ''
    initializeFoilState()

    const [variantsResponse, cardsResponse] = await Promise.all([
      cardService.getCardScryfallVariants(cardId),
      cardService.getCardsByName(cardResponse.data.name),
      loadInventoryCard(cardId, cardResponse.data.name),
    ])

    scryfallVariants.value = variantsResponse.data
    otherCards.value = cardsResponse.data
      .filter((c) => c.id !== cardId)
      .sort((a, b) => a.setName.localeCompare(b.setName))

    if (scryfallVariants.value.length > 0 && selectedScryfallId.value === initialScryfallId) {
      const frenchVariant = scryfallVariants.value.find((v) => v.language === 'Français')
      const englishVariant = scryfallVariants.value.find((v) => v.language === 'Anglais')
      const defaultVariant = frenchVariant || englishVariant || scryfallVariants.value[0]

      if (defaultVariant) {
        selectedScryfallId.value = defaultVariant.scryfallId
        selectedLanguage.value = TranslationService.translateLanguage(defaultVariant.language)
      }
    } else if (inventoryCard.value && inventoryCard.value.languageQuantities) {
      const availableLangs = inventoryCard.value.languageQuantities.filter(
        (l) => (l.foilQuantity || 0) + (l.nonFoilQuantity || 0) > 0,
      )
      if (availableLangs.length > 0) {
        selectedLanguage.value = TranslationService.translateLanguage(
          availableLangs[0].languageName || 'English',
        )
      }
    }
  } catch (error) {
  } finally {
    isLoading.value = false
  }
}

const loadInventoryCard = async (cardId, cardName = null) => {
  try {
    const filters = cardName ? { searchQuery: cardName } : {}

    const response = await cardService.getInventoryCards(1, 100, filters)
    if (response && response.cards) {
      inventoryCard.value = response.cards.find((c) => c.id === cardId)

      if (!inventoryCard.value && response.pagination && response.pagination.totalPages > 1) {
        const maxPagesToSearch = Math.min(10, response.pagination.totalPages)
        for (let page = 2; page <= maxPagesToSearch; page++) {
          const nextPageResponse = await cardService.getInventoryCards(page, 100, filters)
          if (nextPageResponse && nextPageResponse.cards) {
            const foundCard = nextPageResponse.cards.find((c) => c.id === cardId)
            if (foundCard) {
              inventoryCard.value = foundCard
              break
            }
          }
        }
      }
    }

    if (!inventoryCard.value && cardName) {
      const responseWithoutFilter = await cardService.getInventoryCards(1, 100, {})
      if (responseWithoutFilter && responseWithoutFilter.cards) {
        inventoryCard.value = responseWithoutFilter.cards.find((c) => c.id === cardId)
      }
    }
  } catch (error) {
    inventoryCard.value = null
  }
}

const handleMouseMove = (event) => {
  const card = event.currentTarget
  const rect = card.getBoundingClientRect()

  const x = event.clientX - rect.left
  const y = event.clientY - rect.top

  const centerX = rect.width / 2
  const centerY = rect.height / 2

  const rotateY = ((x - centerX) / centerX) * 15
  const rotateX = ((centerY - y) / centerY) * 15

  cardRotateX.value = rotateX
  cardRotateY.value = rotateY
}

const handleMouseEnter = () => {
  isHovering.value = true
}

const handleMouseLeave = () => {
  isHovering.value = false
  cardRotateX.value = 0
  cardRotateY.value = 0
}

const handleTouchMove = (event) => {
  event.preventDefault()
  const card = event.currentTarget
  const rect = card.getBoundingClientRect()
  const touch = event.touches[0]

  const x = touch.clientX - rect.left
  const y = touch.clientY - rect.top

  const centerX = rect.width / 2
  const centerY = rect.height / 2

  const rotateY = ((x - centerX) / centerX) * 15
  const rotateX = ((centerY - y) / centerY) * 15

  cardRotateX.value = rotateX
  cardRotateY.value = rotateY
}

const handleTouchStart = () => {
  isHovering.value = true
}

const handleTouchEnd = () => {
  isHovering.value = false
  cardRotateX.value = 0
  cardRotateY.value = 0
}

const imageError = ref(false)
const isImageLoading = ref(true)

const onImageLoad = () => {
  isImageLoading.value = false
}

const handleImageError = () => {
  imageError.value = true
  isImageLoading.value = false
}

watch(selectedScryfallId, () => {
  imageError.value = false
  isImageLoading.value = true
})

loadCard(route.params.id)

watch(
  () => route.params.id,
  (newId) => {
    loadCard(newId)
  },
)

const allLanguageOptions = computed(() => {
  const options = []
  const seenLanguages = new Set()

  if (scryfallVariants.value.length > 0) {
    scryfallVariants.value.forEach((variant) => {
      const translatedLang = TranslationService.translateLanguage(variant.language)

      if (!seenLanguages.has(translatedLang)) {
        options.push({
          id: variant.scryfallId,
          language: translatedLang,
          label: translatedLang,
          isScryfall: true,
        })
        seenLanguages.add(translatedLang)
      }
    })
  }

  if (inventoryCard.value && inventoryCard.value.languageQuantities) {
    inventoryCard.value.languageQuantities.forEach((langQty) => {
      const langName = langQty.languageName || 'English'
      const translatedLang = TranslationService.translateLanguage(langName)

      if (!seenLanguages.has(translatedLang)) {
        options.push({
          id: `inv-${langName}-${Date.now()}`,
          language: translatedLang,
          label: translatedLang,
          isScryfall: false,
        })
        seenLanguages.add(translatedLang)
      }
    })
  }

  return options.sort((a, b) => {
    if (a.label === 'Français') return -1
    if (b.label === 'Français') return 1
    if (a.label === 'Anglais') return -1
    if (b.label === 'Anglais') return 1
    return a.label.localeCompare(b.label)
  })
})

const currentSelectValue = computed(() => {
  const option = allLanguageOptions.value.find((o) => o.language === selectedLanguage.value)

  if (option) return option.id

  return selectedScryfallId.value
})

const onLanguageChange = (event) => {
  const selectedId = event.target.value

  const selectedOption = allLanguageOptions.value.find((o) => o.id === selectedId)

  if (selectedOption) {
    selectedLanguage.value = selectedOption.language

    if (selectedOption.isScryfall) {
      selectedScryfallId.value = selectedOption.id
    } else {
      selectedScryfallId.value = null
    }
  }
}

// Computed pour calculer le stock disponible
const availableStock = computed(() => {
  if (!inventoryCard.value || !inventoryCard.value.languageQuantities) {
    return 0
  }

  let stockFoil = 0
  let stockNonFoil = 0

  const languageQuantities = inventoryCard.value.languageQuantities

  const currentLanguage = selectedLanguage.value

  if (currentLanguage && languageQuantities.length > 0) {
    const languageQuantity = languageQuantities.find((langQty) => {
      const backendLangName = (langQty.languageName || '').trim()

      const backendLangFr = TranslationService.translateLanguage(backendLangName).toLowerCase()
      const currentLangFr = TranslationService.translateLanguage(currentLanguage).toLowerCase()

      return backendLangFr === currentLangFr
    })

    if (languageQuantity) {
      stockFoil = Number(languageQuantity.foilQuantity) || 0
      stockNonFoil = Number(languageQuantity.nonFoilQuantity) || 0
    } else {
      stockFoil = 0
      stockNonFoil = 0
    }
  } else {
    stockFoil = 0
    stockNonFoil = 0
  }

  return showFoil.value ? stockFoil : stockNonFoil
})

const selectCard = (cardId) => {
  // Scroll vers le haut de la page
  window.scrollTo({ top: 0, behavior: 'smooth' })
  router.push(`/cards/${cardId}`)
}

const handleSeriesSelect = (selectedSeriesName) => {
  if (selectedSeriesName) {
    // Trouver la première carte de la série sélectionnée et naviguer vers elle
    const cardInSeries = otherCards.value.find((card) => card.setName === selectedSeriesName)
    if (cardInSeries) {
      // Scroll vers le haut avant de naviguer
      window.scrollTo({ top: 0, behavior: 'smooth' })
      selectCard(cardInSeries.id)
    }
  }
}

const cartStore = useCartStore()

const addToCart = (cardId) => {
  if (availableStock.value === 0) {
    Notify.create({
      type: 'warning',
      message: 'Aucun stock disponible',
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

  cardService.getCardById(cardId).then((response) => {
    let originalPrice = 0
    if (showFoil.value) {
      originalPrice = response.data.foilPrice > 0 ? response.data.foilPrice : 0
    } else {
      originalPrice = response.data.price > 0 ? response.data.price : 0
    }

    const discount = response.data.discount || response.data.Discount || 0

    if (originalPrice <= 0) {
      return
    }

    const selectedScryfallIdForCart =
      selectedScryfallId.value || response.data.scryfallId || 'fake-scryfall-id'
    const imageUrl = `https://api.scryfall.com/cards/${selectedScryfallIdForCart}?format=image&version=small`

    const selectedVariant = scryfallVariants.value.find(
      (v) => v.scryfallId === selectedScryfallIdForCart,
    )
    const language =
      selectedLanguage.value ||
      (selectedVariant ? TranslationService.translateLanguage(selectedVariant.language) : null)

    if (!language) return

    let stockFoil = 0
    let stockNonFoil = 0

    if (inventoryCard.value && inventoryCard.value.languageQuantities) {
      const languageQuantities = inventoryCard.value.languageQuantities
      if (language) {
        const languageQuantity = languageQuantities.find((langQty) => {
          const backendLangName = (langQty.languageName || '').trim()
          const backendLangFr = TranslationService.translateLanguage(backendLangName)
          const targetLangFr = TranslationService.translateLanguage(language)
          return backendLangFr === targetLangFr
        })

        if (languageQuantity) {
          stockFoil = languageQuantity.foilQuantity || 0
          stockNonFoil = languageQuantity.nonFoilQuantity || 0
        } else {
          stockFoil = 0
          stockNonFoil = 0
        }
      } else {
        stockFoil = 0
        stockNonFoil = 0
      }
    }

    const stock = showFoil.value ? stockFoil : stockNonFoil

    const limitPerClient = 4

    const product = {
      id: response.data.id,
      name: response.data.name,
      description: `${response.data.serie} - ${response.data.rarete}`,
      price: originalPrice,
      discount: discount,
      limit: limitPerClient,
      image: imageUrl,
      setName: response.data.serie,
      number: response.data.numero,
      rarity: response.data.rarete,
      scryfallId: selectedScryfallIdForCart,
      language: language,
      isFoil: showFoil.value,
      stock: stock,
      stockFoil: stockFoil,
      stockNonFoil: stockNonFoil,
    }

    cartStore.addItem(product)
  })
}

const removeFromCart = (cardId, languageName) => {
  let lang = languageName
  if (!lang) {
    const item = cartStore.items.find((i) => i.id === cardId)
    if (item) {
      lang = item.language
    }
  }
  cartStore.removeItem(cardId, lang, showFoil.value)
}

const isCardInCart = computed(() => {
  if (!displayedCard.value) return false

  const language = selectedLanguage.value
  if (!language) return false

  return cartStore.isInCart(displayedCard.value.id, language, showFoil.value)
})

const getPointerX = (event) => {
  if (event.touches && event.touches.length > 0) {
    return event.touches[0].pageX
  }

  if (event.changedTouches && event.changedTouches.length > 0) {
    return event.changedTouches[0].pageX
  }

  if (typeof event.pageX === 'number') {
    return event.pageX
  }

  if (typeof event.clientX === 'number') {
    return event.clientX + window.scrollX
  }

  return 0
}

const startDrag = (event) => {
  isDragging.value = true
  hasMovedDuringDrag.value = false
  const container = event.currentTarget
  container.style.scrollBehavior = 'auto'
  startX.value = getPointerX(event)
  scrollLeft.value = container.scrollLeft
}

const drag = (event) => {
  if (!isDragging.value) return

  if (!event.type.startsWith('touch')) {
    event.preventDefault()
  }

  hasMovedDuringDrag.value = true
  const container = event.currentTarget
  const x = getPointerX(event)
  const walk = (x - startX.value) * 1.5

  const maxScroll = container.scrollWidth - container.clientWidth
  const newScrollLeft = scrollLeft.value - walk

  if (maxScroll > 0) {
    if (newScrollLeft < 0) {
      container.scrollLeft = newScrollLeft * 0.1
    } else if (newScrollLeft > maxScroll) {
      const overscroll = newScrollLeft - maxScroll
      container.scrollLeft = maxScroll + overscroll * 0.1
    } else {
      container.scrollLeft = newScrollLeft
    }
  } else {
    container.scrollLeft = newScrollLeft
  }
}

const stopDrag = (event) => {
  const container = event?.currentTarget || event?.target?.closest('.other-cards-list')
  if (!container) {
    isDragging.value = false
    hasMovedDuringDrag.value = false
    return
  }

  const maxScroll = container.scrollWidth - container.clientWidth
  const isOverflowLeft = container.scrollLeft < 0
  const isOverflowRight = container.scrollLeft > maxScroll

  if (isOverflowLeft || isOverflowRight) {
    container.scrollTo({
      left: isOverflowLeft ? 0 : maxScroll,
      behavior: 'smooth',
    })
  }

  container.style.scrollBehavior = 'smooth'
  isDragging.value = false
  hasMovedDuringDrag.value = false
}

const handleCardClick = (event, cardId) => {
  if (hasMovedDuringDrag.value) {
    event.preventDefault()
    event.stopPropagation()
    return
  }
  selectCard(cardId)
}

function replaceManaSymbols(text) {
  return text.replace(/\{([^}]+)\}/g, (symbol) => {
    const cleaned = symbol.replace(/\W/gi, '')
    return `
      <svg style="width:20px;height:20px;">
        <image xlink:href="https://svgs.scryfall.io/card-symbols/${cleaned}.svg" 
        width="20" 
        height="20"/>
      </svg>
      `
  })
}
const displayedText = computed(() => {
  if (!displayedCard.value?.text) return ''

  return replaceManaSymbols(displayedCard.value.text)
})
</script>

<template>
  <div class="main_container">
    <!-- Version Desktop -->
    <template v-if="$q.screen.gt.sm">
      <div
        v-if="isLoading"
        class="content_container"
        style="display: flex; justify-content: center; align-items: center; min-height: 400px"
      >
        <div style="text-align: center">
          <div
            class="spinner"
            style="
              border: 4px solid #f3f3f3;
              border-top: 4px solid #3498db;
              border-radius: 50%;
              width: 40px;
              height: 40px;
              animation: spin 1s linear infinite;
              margin: 0 auto 20px;
            "
          ></div>
          <p>Chargement des informations de la carte...</p>
        </div>
      </div>

      <div v-else class="content_container">
        <div
          class="image_flex_container flex_items"
          @mousemove="handleMouseMove"
          @mouseenter="handleMouseEnter"
          @mouseleave="handleMouseLeave"
        >
          <div class="image-wrapper" :style="cardStyle">
            <q-skeleton
              v-if="isImageLoading"
              class="full-width"
              style="aspect-ratio: 63/88; border-radius: 4.75% / 3.5%"
            />
            <img
              v-if="selectedScryfallId != null"
              v-show="!isImageLoading"
              :src="`https://api.scryfall.com/cards/${selectedScryfallId}?format=image`"
              :alt="`Image de ${displayedCard?.name}`"
              @load="onImageLoad"
              @error="handleImageError"
            />
            <img
              v-else
              v-show="!isImageLoading"
              :src="`https://api.scryfall.com/cards/${displayedCard.scryfallId}?format=image`"
              :alt="`Image de ${displayedCard?.name}`"
              @load="onImageLoad"
              @error="handleImageError"
            />
            <div
              v-if="!isImageLoading && (showFoil || shouldShowFoilEffect)"
              class="foil-overlay"
            ></div>
          </div>
        </div>

        <div class="data_flex_container flex_items">
          <div class="data_flex_container_item_info">
            <h2 class="card_title">{{ displayedCard?.name }}</h2>

            <div v-if="displayedCard?.text" class="description_section">
              <h3>Description:</h3>
              <div class="description_text" v-html="displayedText"></div>
            </div>

            <div class="details_grid">
              <div class="detail_row">
                <span class="label">Série :</span>
                <span class="value">{{ displayedCard?.setName }}</span>
              </div>
              <div class="detail_row">
                <span class="label">Rareté :</span>
                <span class="value">{{ displayedCard?.rarity }}</span>
              </div>
              <div class="detail_row">
                <span class="label">Numéro de la carte dans la série # :</span>
                <span class="value">{{ displayedCard?.number }}</span>
              </div>
              <div class="detail_row">
                <span class="label">Type et sous-type de la carte :</span>
                <span class="value">{{ displayedCard?.types?.join('- ') || 'Type Inconnu' }} </span>

                <span class="value" v-if="displayedCard?.subTypes?.length > 0">
                  - {{ displayedCard?.subTypes?.join(' ') }}
                </span>
              </div>
              <div class="detail_row">
                <span class="label">Coût en mana :</span>
                <span v-if="manaCostSymbols == null">Non specifié</span>
                <span class="value">
                  <svg v-for="symbol in manaCostSymbols" :key="symbol">
                    <image :xlink:href="`https://svgs.scryfall.io/card-symbols/${symbol}.svg`" />
                  </svg>
                </span>
              </div>
              <div class="detail_row">
                <span class="label">Couleur(s) de la carte :</span>
                <span class="value">{{ displayedCard?.colors }}</span>
              </div>
              <div class="detail_row" v-if="isCreatureCard">
                <span class="label">Puissance :</span>
                <span class="value" v-if="!displayedCard?.power">N/A</span>
                <span class="value" v-else>{{ displayedCard?.power }}</span>
              </div>
              <div class="detail_row" v-if="isCreatureCard">
                <span class="label">Résistance :</span>
                <span class="value" v-if="!displayedCard?.toughness">N/A</span>
                <span class="value" v-else>{{ displayedCard?.toughness }}</span>
              </div>
              <div class="detail_row">
                <span class="label">Langue :</span>
                <select
                  v-if="allLanguageOptions.length > 1"
                  @change="onLanguageChange"
                  :value="currentSelectValue"
                >
                  <option value="" disabled>Sélectionnez une langue</option>
                  <option v-for="option in allLanguageOptions" :key="option.id" :value="option.id">
                    {{ option.label }}
                  </option>
                </select>
                <span v-else class="value">Anglais</span>
              </div>
              <div class="detail_row">
                <span class="label">Finition(s) disponible :</span>
                <span class="value">
                  <div v-if="canToggleFoil" class="foil-toggle">
                    <label class="toggle-label">
                      <input type="checkbox" v-model="showFoil" class="toggle-checkbox" />
                      <span class="toggle-text">Version Foil</span>
                    </label>
                  </div>
                  <div v-if="onlyShowFoil">
                    <span class="value">foil</span>
                  </div>
                  <div v-if="canNotToggleFoil">
                    <span class="value">finition de base</span>
                  </div>
                </span>
              </div>
              <div class="detail_row">
                <span class="label">Artiste(s) :</span>
                <span class="value">{{
                  displayedCard?.artistsNames?.join(', ') || 'Inconnu'
                }}</span>
              </div>
            </div>
          </div>

          <div class="data_flex_container_item_values">
            <div class="price_details">
              <p v-if="!displayedCard?.onlineOnly && originalPrice > 0">
                <strong>Condition : {{ displayedCard?.condition }}</strong>
              </p>
              <p v-if="displayPrice > 0">
                <strong
                  :class="
                    (displayedCard?.discount && displayedCard.discount > 0) ||
                    (displayedCard?.Discount && displayedCard.Discount > 0)
                      ? 'price-original'
                      : ''
                  "
                  >Valeur réelle :
                  {{ originalPrice > 0 ? originalPrice.toFixed(2) + ' $' : '0 $' }}</strong
                >
              </p>
              <p v-if="displayPrice > 0">
                <strong
                  :class="
                    (displayedCard?.discount && displayedCard.discount > 0) ||
                    (displayedCard?.Discount && displayedCard.Discount > 0)
                      ? 'price-current-discounted'
                      : ''
                  "
                  >Prix actuel :
                  {{ displayPrice > 0 ? displayPrice.toFixed(2) + ' $' : '0 $' }}</strong
                >
              </p>
              <p v-if="!displayedCard?.onlineOnly && originalPrice > 0">
                <strong>Stock disponible : {{ availableStock }}</strong>
              </p>
              <p v-if="displayedCard?.onlineOnly" class="text-center q-mb-md">
                <strong>Seulement disponible en ligne</strong>
              </p>
            </div>
            <button
              @click="
                isCardInCart
                  ? removeFromCart(route.params.id, displayedCard.language, displayedCard.isFoil)
                  : addToCart(route.params.id)
              "
              :class="['add_to_cart_btn', { remove_from_cart_btn: isCardInCart }]"
              :disabled="
                (!isCardInCart && availableStock === 0) ||
                displayedCard?.onlineOnly ||
                originalPrice <= 0
              "
              :style="
                displayedCard?.onlineOnly || originalPrice <= 0
                  ? { backgroundColor: '#6c757d', cursor: 'not-allowed' }
                  : {}
              "
            >
              {{
                displayedCard?.onlineOnly || originalPrice <= 0
                  ? 'Non disponible'
                  : isCardInCart
                    ? 'Retirer du panier'
                    : availableStock === 0
                      ? 'Aucun stock disponible'
                      : 'Ajouter au panier'
              }}
            </button>
          </div>
        </div>
      </div>

      <!-- Autres éditions de la carte -->
      <div v-if="otherCards.length > 0" class="other-cards-section">
        <div class="other-cards-header">
          <h3 class="other-cards-title">Autres éditions de {{ displayedCard?.name }} :</h3>
          <div v-if="hasMoreThanTenEditions" class="other-cards-search">
            <div class="dropdown">
              <button
                class="btn btn-sm dropdown-toggle btn-secondary"
                type="button"
                data-bs-toggle="dropdown"
                data-bs-auto-close="outside"
              >
                Rechercher série
              </button>
              <div class="dropdown-menu p-2">
                <div class="dropdown-search">
                  <input
                    type="text"
                    v-model="searchSeries"
                    placeholder="Rechercher une série..."
                    class="form-control form-control-sm"
                  />
                </div>
                <div class="dropdown-list-container">
                  <div
                    v-for="card in filteredOtherCards"
                    :key="card.id"
                    class="dropdown-checkbox-item"
                    @click="selectCard(card.id)"
                  >
                    {{ card.setName }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          class="other-cards-list"
          @mousedown="startDrag"
          @mousemove="drag"
          @mouseup="stopDrag"
          @mouseleave="stopDrag"
        >
          <div
            v-for="card in otherCards"
            :key="card.id"
            @click="(e) => handleCardClick(e, card.id)"
            class="other-card-item"
          >
            <img
              :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image&version=small`"
              class="other-card-thumbnail"
            />
            <span class="other-card-set">{{ card.setName }}</span>
          </div>
        </div>
      </div>
    </template>

    <!-- Version Mobile -->
    <template v-else>
      <div class="mobile-container">
        <div
          v-if="isLoading"
          class="q-pa-xl text-center"
          style="min-height: 400px; display: flex; align-items: center; justify-content: center"
        >
          <div>
            <q-spinner color="primary" size="3em" />
            <p class="q-mt-md text-white">Chargement des informations de la carte...</p>
          </div>
        </div>

        <template v-else>
          <div class="mobile-content-wrapper">
            <div class="mobile-card-section">
              <div class="mobile-image-container">
                <div
                  class="mobile-image-wrapper"
                  :style="cardStyle"
                  @touchstart="handleTouchStart"
                  @touchmove="handleTouchMove"
                  @touchend="handleTouchEnd"
                  @touchcancel="handleTouchEnd"
                >
                  <div v-if="!imageError">
                    <img
                      v-if="selectedScryfallId != null"
                      :src="`https://api.scryfall.com/cards/${selectedScryfallId}?format=image`"
                      :alt="`Image de ${displayedCard?.name}`"
                      class="mobile-card-image"
                      @error="handleImageError"
                    />

                    <img
                      v-else
                      :src="`https://api.scryfall.com/cards/${displayedCard.scryfallId}?format=image`"
                      :alt="`Image de ${displayedCard?.name}`"
                      class="mobile-card-image"
                      @error="handleImageError"
                    />
                  </div>
                  <div
                    v-else
                    class="mobile-card-image flex justify-center items-center bg-grey-9 text-grey-5 column"
                    style="border: 2px solid #555"
                  >
                    <q-icon name="image_not_supported" size="4rem" />
                    <span class="q-mt-sm text-caption">Image non disponible</span>
                  </div>
                  <div
                    v-if="(showFoil || shouldShowFoilEffect) && !imageError"
                    class="foil-overlay"
                  ></div>
                </div>
              </div>
            </div>

            <div class="mobile-info-wrapper">
              <q-card class="mobile-info-card q-mb-md">
                <q-card-section>
                  <div class="text-h6 text-white q-mb-sm">{{ displayedCard?.name }}</div>
                  <div class="text-caption q-mb-md">
                    <span class="mobile-series-label">Série :</span>
                    <span class="mobile-series-value text-white">{{ displayedCard?.setName }}</span>
                  </div>

                  <q-separator class="q-mb-md" />

                  <div v-if="displayedCard?.text" class="q-mb-md">
                    <div class="text-subtitle2 mobile-description-label q-mb-xs">Description</div>
                    <div
                      class="text-body2 text-white description-text-mobile"
                      v-html="displayedText"
                    ></div>
                  </div>
                </q-card-section>
              </q-card>

              <q-card class="mobile-details-card q-mb-md">
                <q-card-section>
                  <q-list separator class="mobile-details-list">
                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Rareté</q-item-label>
                        <q-item-label caption class="detail-value">{{
                          displayedCard?.rarity
                        }}</q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Numéro de série</q-item-label>
                        <q-item-label caption class="detail-value"
                          >#{{ displayedCard?.number }}</q-item-label
                        >
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Type</q-item-label>
                        <q-item-label caption class="detail-value">
                          {{ displayedCard?.types?.join(', ') || 'Type Inconnu' }}
                          <span v-if="displayedCard?.subTypes?.length > 0">
                            - {{ displayedCard?.subTypes?.join(', ') }}
                          </span>
                        </q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item v-if="manaCostSymbols">
                      <q-item-section>
                        <q-item-label class="detail-label">Coût en mana</q-item-label>
                        <q-item-label caption class="detail-value">
                          <svg v-for="symbol in manaCostSymbols" :key="symbol" class="mana-symbol">
                            <image
                              :xlink:href="`https://svgs.scryfall.io/card-symbols/${symbol}.svg`"
                            />
                          </svg>
                        </q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Couleur(s)</q-item-label>
                        <q-item-label caption class="detail-value">{{
                          displayedCard?.colors
                        }}</q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item
                      v-if="isCreatureCard && (displayedCard?.power || displayedCard?.toughness)"
                    >
                      <q-item-section>
                        <q-item-label class="detail-label">Puissance / Résistance</q-item-label>
                        <q-item-label caption class="detail-value">
                          {{ displayedCard?.power || 'N/A' }} /
                          {{ displayedCard?.toughness || 'N/A' }}
                        </q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Langue</q-item-label>
                        <q-item-label caption>
                          <div v-if="scryfallVariants.length > 1" class="native-select-wrapper">
                            <select
                              @change="onLanguageChange"
                              :value="currentSelectValue"
                              class="native-select"
                            >
                              <option
                                v-for="variant in allLanguageOptions"
                                :key="variant.id"
                                :value="variant.id"
                              >
                                {{ TranslationService.translateLanguage(variant.language) }}
                              </option>
                            </select>
                            <q-icon name="arrow_drop_down" class="select-arrow" />
                          </div>
                          <span v-else class="detail-value">Anglais</span>
                        </q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Finition</q-item-label>
                        <q-item-label caption>
                          <q-toggle
                            v-if="canToggleFoil"
                            v-model="showFoil"
                            label="Version Foil"
                            color="amber"
                            class="mobile-toggle"
                          />
                          <span v-else-if="onlyShowFoil" class="detail-value">Foil</span>
                          <span v-else-if="canNotToggleFoil" class="detail-value"
                            >Finition de base</span
                          >
                        </q-item-label>
                      </q-item-section>
                    </q-item>

                    <q-item>
                      <q-item-section>
                        <q-item-label class="detail-label">Artiste(s)</q-item-label>
                        <q-item-label caption class="detail-value">
                          {{ displayedCard?.artistsNames?.join(', ') || 'Inconnu' }}
                        </q-item-label>
                      </q-item-section>
                    </q-item>
                  </q-list>
                </q-card-section>
              </q-card>

              <q-card class="mobile-price-card q-mb-md">
                <q-card-section class="q-pa-md">
                  <div
                    v-if="!displayedCard?.onlineOnly && originalPrice > 0"
                    class="mobile-price-header q-mb-md"
                  >
                    <div class="mobile-price-header-title">
                      <strong>Condition : {{ displayedCard?.condition }}</strong>
                    </div>
                  </div>

                  <div v-if="displayPrice > 0" class="mobile-price-content q-mb-md">
                    <div class="mobile-price-item q-mb-sm">
                      <strong
                        :class="
                          (displayedCard?.discount && displayedCard.discount > 0) ||
                          (displayedCard?.Discount && displayedCard.Discount > 0)
                            ? 'price-original-mobile'
                            : ''
                        "
                      >
                        Valeur réelle :
                        {{ originalPrice > 0 ? originalPrice.toFixed(2) + ' $' : '0 $' }}
                      </strong>
                    </div>

                    <div class="mobile-price-item q-mb-sm">
                      <strong
                        :class="
                          (displayedCard?.discount && displayedCard.discount > 0) ||
                          (displayedCard?.Discount && displayedCard.Discount > 0)
                            ? 'price-current-discounted'
                            : ''
                        "
                        >Prix actuel :
                        {{ displayPrice > 0 ? displayPrice.toFixed(2) + ' $' : '0 $' }}</strong
                      >
                    </div>
                  </div>

                  <div
                    v-if="!displayedCard?.onlineOnly && originalPrice > 0"
                    class="mobile-price-item q-mb-md"
                  >
                    <strong>Stock disponible : {{ availableStock }}</strong>
                  </div>
                  <div
                    v-if="displayedCard?.onlineOnly"
                    class="mobile-price-item q-mb-md text-center"
                  >
                    <strong>Seulement disponible en ligne</strong>
                  </div>

                  <q-btn
                    :class="[
                      'full-width mobile-cart-button',
                      { 'mobile-cart-button-remove': isCardInCart },
                    ]"
                    :icon="isCardInCart ? 'remove_shopping_cart' : 'add_shopping_cart'"
                    :label="
                      displayedCard?.onlineOnly || originalPrice <= 0
                        ? 'Non disponible'
                        : isCardInCart
                          ? 'Retirer du panier'
                          : availableStock === 0
                            ? 'Aucun stock disponible'
                            : 'Ajouter au panier'
                    "
                    :disabled="
                      (!isCardInCart && availableStock === 0) ||
                      displayedCard?.onlineOnly ||
                      originalPrice <= 0
                    "
                    size="lg"
                    unelevated
                    @click="
                      isCardInCart
                        ? removeFromCart(
                            route.params.id,
                            displayedCard.language,
                            displayedCard.isFoil,
                          )
                        : addToCart(route.params.id)
                    "
                  />
                </q-card-section>
              </q-card>
            </div>
          </div>

          <!-- Autres éditions de la carte - Version Mobile avec scroller horizontal -->
          <q-card v-if="otherCards.length > 0" class="mobile-other-cards-card">
            <q-card-section>
              <div class="mobile-other-cards-header q-mb-md">
                <div class="text-subtitle1 text-weight-bold text-white q-mb-sm">
                  Autres éditions de {{ displayedCard?.name }}
                </div>
                <div v-if="hasMoreThanTenEditions" class="mobile-dropdown-container">
                  <div class="mobile-dropdown-wrapper">
                    <div class="native-select-wrapper">
                      <select
                        :value="searchSeries"
                        @change="
                          (e) => {
                            searchSeries = e.target.value
                            handleSeriesSelect(e.target.value)
                          }
                        "
                        class="native-select"
                      >
                        <option value="" disabled selected>Rechercher série</option>
                        <option
                          v-for="seriesName in Array.from(
                            new Set(otherCards.map((c) => c.setName)),
                          ).sort()"
                          :key="seriesName"
                          :value="seriesName"
                        >
                          {{ seriesName }}
                        </option>
                      </select>
                      <q-icon name="arrow_drop_down" class="select-arrow" />
                    </div>
                  </div>
                </div>
              </div>
              <div class="mobile-other-cards-list">
                <div
                  v-for="card in otherCards"
                  :key="card.id"
                  @click="(e) => handleCardClick(e, card.id)"
                  class="mobile-other-card-item"
                >
                  <img
                    :src="`https://api.scryfall.com/cards/${card.scryfallId}?format=image&version=small`"
                    :alt="card.setName"
                    class="mobile-other-card-thumbnail"
                  />
                  <span class="mobile-other-card-set">{{ card.setName }}</span>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </template>
      </div>
    </template>
  </div>
</template>

<style src="../css/CardDetailsPage.css" scoped></style>
<style>
@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
