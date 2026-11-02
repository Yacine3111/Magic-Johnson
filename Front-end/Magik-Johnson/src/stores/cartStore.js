import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { Notify } from 'quasar'
import { cardService } from '@/services/CardService'
import TranslationService from '@/services/translationService'

export const useCartStore = defineStore('cart', () => {
  // État du panier
  const items = ref([])
  const discountRate = ref(0)
  const shippingCost = ref(0)
  const isSyncedWithBackend = ref(false)

  // Getters calculés
  const itemCount = computed(() => items.value.length)
  const totalQuantity = computed(() =>
    items.value.reduce((total, item) => total + item.quantity, 0),
  )

  const subtotal = computed(() => {
    return items.value.reduce((total, item) => total + item.price * item.quantity, 0)
  })

  const discount = computed(() => {
    return subtotal.value * discountRate.value
  })

  const total = computed(() => {
    return subtotal.value - discount.value + shippingCost.value
  })

  // Actions
  const addItem = async (product) => {
    if (product.id && (!product.language || !product.number || !product.setName)) {
      try {
        const cardResponse = await cardService.getCardById(product.id)
        if (cardResponse?.data) {
          const cardData = cardResponse.data

          if (!product.language && cardData.languages && cardData.languages.length > 0) {
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
            const availableLanguages = cardData.languages.map((lang) => languageMap[lang] || lang)

            if (availableLanguages.includes('Français')) {
              product.language = 'Français'
            } else if (availableLanguages.length > 0) {
              product.language = availableLanguages[0]
            } else {
              product.language = 'Anglais'
            }
          } else if (!product.language) {
            product.language = 'Anglais'
          }

          if (!product.number && cardData.number) {
            product.number = cardData.number
          }

          if (!product.setName && cardData.setName) {
            product.setName = cardData.setName
          }
        }
      } catch (error) {
        if (!product.language) {
          product.language = 'Anglais'
        }
      }
    }

    let stock = product.stock || product.totalQuantity || 0
    const clientLimit = 4

    if (!product.stock && !product.totalQuantity) {
      try {
        const response = await cardService.getCardById(product.id)

        if (response && response.data) {
          const cardData = response.data

          if (product.isFoil) {
            stock = cardData.stockFoil || 0
          } else {
            stock = cardData.stockNonFoil || 0
          }
        }
      } catch (error) {}
    }

    const effectiveLimit = Math.min(clientLimit, stock)

    const productLanguage = TranslationService.translateLanguage(product.language || 'Anglais')
    const productIsFoil = product.isFoil || false

    const existingItem = items.value.find(
      (item) =>
        item.id === product.id &&
        (item.language || 'Anglais') === productLanguage &&
        (item.isFoil || false) === productIsFoil,
    )

    if (existingItem) {
      if (existingItem.quantity < effectiveLimit) {
        existingItem.quantity += 1
        existingItem.limit = effectiveLimit
        existingItem.stock = stock
        existingItem.clientLimit = clientLimit
        existingItem.isFoil = product.isFoil
        existingItem.stockFoil = product.stockFoil
        existingItem.stockNonFoil = product.stockNonFoil

        if (product.price !== undefined) {
          existingItem.price = product.price
        }
        if (product.discount !== undefined) {
          existingItem.discount = product.discount
        }
        if (product.language !== undefined && product.language !== null) {
          existingItem.language = product.language
        }
      } else {
        const reason = stock < clientLimit ? 'Stock insuffisant' : 'Limite client atteinte'
        const maxValue = stock < clientLimit ? stock : clientLimit
        Notify.create({
          type: 'warning',
          message: `${reason}. Maximum ${maxValue} exemplaire(s) autorisé(s)`,
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
    } else {
      const itemLanguage = product.language || 'Anglais'

      items.value.push({
        id: product.id,
        name: product.name,
        description: product.description || 'Description non disponible',
        price: product.price || 0,
        discount: product.discount || 0,
        quantity: 1,
        limit: effectiveLimit,
        stock: stock,
        clientLimit: clientLimit,
        isFoil: product.isFoil || false,
        stockFoil: product.stockFoil || 0,
        stockNonFoil: product.stockNonFoil || 0,
        image:
          product.image ||
          `https://api.scryfall.com/cards/${product.scryfallId || 'random'}?format=image&version=small` ||
          'https://via.placeholder.com/80x80?text=No+Image',
        setName: product.setName || 'Série inconnue',
        number: product.number || '',
        rarity: product.rarity || 'Commune',
        scryfallId: product.scryfallId,
        language: itemLanguage,
      })
    }

    saveToLocalStorage()

    if (isSyncedWithBackend.value) {
      syncToBackend().catch(() => {})
    }
  }

  const removeItem = async (itemId, languageName, isFoil) => {
    const itemIndex = items.value.findIndex(
      (item) => item.id === itemId && item.language === languageName,
    )

    const indexToRemove =
      itemIndex > -1 ? itemIndex : items.value.findIndex((item) => item.id === itemId)

    if (indexToRemove > -1) {
      items.value.splice(indexToRemove, 1)
      saveToLocalStorage()

      if (isSyncedWithBackend.value) {
        try {
          const { cartService } = await import('@/services/CartService')
          await cartService.deleteCardFromCart(
            itemId,
            TranslationService.reverseTranslateLanguage(languageName),
            isFoil,
          )
        } catch (error) {
          syncToBackend().catch((syncError) => {})
        }
      }
    }
  }

  const updateQuantity = (itemId, languageName, isFoil, newQuantity) => {
    const item = items.value.find(
      (item) =>
        item.id === itemId &&
        (item.language || 'Anglais') === languageName &&
        item.isFoil == isFoil,
    )
    if (item) {
      if (newQuantity <= 0) {
        removeItem(itemId, languageName, isFoil)
      } else if (newQuantity <= item.limit) {
        item.quantity = newQuantity
        saveToLocalStorage()

        if (isSyncedWithBackend.value) {
          syncToBackend().catch((error) => {})
        }
      } else {
        const reason =
          item.stock < item.clientLimit ? 'Stock insuffisant' : 'Limite client atteinte'
        const maxValue = item.stock < item.clientLimit ? item.stock : item.clientLimit
        Notify.create({
          type: 'warning',
          message: `${reason}. Maximum ${maxValue} exemplaire(s) autorisé(s)`,
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
    }
  }

  const clearCart = async (syncWithBackend = true) => {
    const wasSynced = isSyncedWithBackend.value
    const itemsToDelete = [...items.value]

    items.value = []
    localStorage.removeItem('magik-johnson-cart')
    isSyncedWithBackend.value = false

    if (!syncWithBackend) return

    if (wasSynced && itemsToDelete.length > 0) {
      try {
        const { cartService } = await import('@/services/CartService')

        const deletePromises = itemsToDelete.map((item) =>
          cartService
            .deleteCardFromCart(
              item.id,
              TranslationService.reverseTranslateLanguage(item.language),
              item.isFoil,
            )
            .catch(() => {}),
        )

        await Promise.all(deletePromises)
      } catch (error) {}
    }
  }

  const updateStocksInRealTime = async (suppressNotifications = false) => {
    if (items.value.length === 0) return

    try {
      const { cardService } = await import('@/services/CardService')
      const { TranslationService } = await import('@/services/translationService')

      const cardsWithInsufficientStock = []

      for (const item of items.value) {
        try {
          const filters = item.name ? { searchQuery: item.name } : {}
          const inventoryResponse = await cardService.getInventoryCards(1, 100, filters)

          let inventoryCard = null
          if (inventoryResponse && inventoryResponse.cards) {
            inventoryCard = inventoryResponse.cards.find((c) => c.id === item.id)

            if (!inventoryCard) {
              inventoryCard = inventoryResponse.cards.find((c) => c.name === item.name)
            }

            if (
              !inventoryCard &&
              inventoryResponse.pagination &&
              inventoryResponse.pagination.totalPages > 1
            ) {
              const maxPagesToSearch = Math.min(10, inventoryResponse.pagination.totalPages)
              for (let page = 2; page <= maxPagesToSearch; page++) {
                const nextPageResponse = await cardService.getInventoryCards(page, 100, filters)
                if (nextPageResponse && nextPageResponse.cards) {
                  const foundCard = nextPageResponse.cards.find((c) => c.id === item.id)
                  if (foundCard) {
                    inventoryCard = foundCard
                    break
                  }
                  if (!inventoryCard) {
                    const foundByName = nextPageResponse.cards.find((c) => c.name === item.name)
                    if (foundByName) {
                      inventoryCard = foundByName
                      break
                    }
                  }
                }
              }
            }

            if (!inventoryCard && item.name) {
              const responseWithoutFilter = await cardService.getInventoryCards(1, 100, {})
              if (responseWithoutFilter && responseWithoutFilter.cards) {
                inventoryCard = responseWithoutFilter.cards.find((c) => c.id === item.id)
                if (!inventoryCard) {
                  inventoryCard = responseWithoutFilter.cards.find((c) => c.name === item.name)
                }
              }
            }
          }

          if (inventoryCard) {
            const previousStock = item.stock || 0
            const previousHasStockIssue = item.hasStockIssue || false
            const previousQuantity = item.quantity

            let originalLanguage = item.language

            if (
              !originalLanguage ||
              originalLanguage === null ||
              originalLanguage === undefined ||
              originalLanguage === ''
            ) {
              try {
                const cardResponse = await cardService.getCardById(item.id)
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

                  if (availableLanguages.length > 0) {
                    originalLanguage = availableLanguages[0]
                  } else {
                    originalLanguage = 'Anglais'
                  }
                  item.language = originalLanguage
                } else {
                  originalLanguage = 'Anglais'
                  item.language = 'Anglais'
                }
              } catch (error) {
                originalLanguage = 'Anglais'
                item.language = 'Anglais'
              }
            } else {
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

            let stockFoil = 0
            let stockNonFoil = 0

            if (inventoryCard.languageQuantities && inventoryCard.languageQuantities.length > 0) {
              if (
                originalLanguage &&
                originalLanguage !== null &&
                originalLanguage !== undefined &&
                originalLanguage !== ''
              ) {
                const normalizedItemLanguage = normalizeLanguage(originalLanguage)

                const languageQuantity = inventoryCard.languageQuantities.find((langQty) => {
                  const langName = (langQty.languageName || '').trim()
                  const normalizedSearch = (normalizedItemLanguage || '').trim().toLowerCase()
                  const originalSearch = (originalLanguage || '').trim().toLowerCase()
                  const backendLangName = langName.trim().toLowerCase()

                  const matches =
                    backendLangName === normalizedSearch ||
                    backendLangName === originalSearch ||
                    (langName === 'English' &&
                      (originalLanguage === 'Anglais' || !originalLanguage)) ||
                    (normalizedItemLanguage === 'Japanese' && backendLangName.includes('japan')) ||
                    (normalizedItemLanguage === 'French' && backendLangName.includes('french')) ||
                    (normalizedItemLanguage === 'Portuguese' && backendLangName.includes('portug'))

                  if (matches) {
                  }

                  return matches
                })

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

            const isFoil =
              item.isFoil !== undefined
                ? item.isFoil
                : inventoryCard.price <= 0 && inventoryCard.foilPrice > 0
            const currentStock = isFoil ? stockFoil : stockNonFoil

            const correctPrice = isFoil ? inventoryCard.foilPrice || 0 : inventoryCard.price || 0

            const correctClientLimit = 4
            const effectiveLimit = Math.min(correctClientLimit, currentStock)

            const originalQuantity = item.quantity
            let newQuantity = item.quantity
            let hasStockIssue = false
            let stockIssueMessage = ''

            const quantityNeedsReduction = originalQuantity > currentStock
            const quantityActuallyChanged =
              quantityNeedsReduction && previousQuantity !== currentStock

            if (currentStock === 0) {
              newQuantity = 0
              hasStockIssue = true
              stockIssueMessage = 'STOCK ÉPUISÉ'

              if (previousQuantity > 0 && !suppressNotifications) {
                cardsWithInsufficientStock.push(item.name)
              }
            } else if (quantityNeedsReduction) {
              newQuantity = currentStock

              if (quantityActuallyChanged && !suppressNotifications) {
                cardsWithInsufficientStock.push(item.name)
              }

              hasStockIssue = true
              stockIssueMessage = 'STOCK INSUFFISANT'
            } else if (item.hasStockIssue && previousHasStockIssue) {
              hasStockIssue = false
              stockIssueMessage = ''
            }

            let translatedRarity = item.rarity
            if (inventoryCard.rarity) {
              translatedRarity = TranslationService.translateRarity(inventoryCard.rarity)
            } else if (item.rarity) {
              const frenchRarities = [
                'Commune',
                'Peu commune',
                'Rare',
                'Mythique',
                'Rare mythique',
                'Spéciale',
                'Bonus',
                'Jeton',
                'Terrain de base',
              ]
              if (!frenchRarities.includes(item.rarity)) {
                translatedRarity = TranslationService.translateRarity(item.rarity)
              }
            }

            Object.assign(item, {
              stockFoil,
              stockNonFoil,
              isFoil,
              stock: currentStock,
              quantity: newQuantity,
              limit: effectiveLimit,
              price: correctPrice,
              foilPrice: inventoryCard.foilPrice,
              setName: inventoryCard.setName,
              rarity: translatedRarity,
              hasStockIssue,
              stockIssueMessage,
              clientLimit: correctClientLimit,
            })
            if (
              originalLanguage &&
              originalLanguage !== null &&
              originalLanguage !== undefined &&
              originalLanguage !== ''
            ) {
              item.language = originalLanguage
            } else {
              item.language = 'Anglais'
            }

            const stockJustBecameZero = currentStock === 0 && previousStock > 0
            if (stockJustBecameZero && !suppressNotifications) {
              const stockType = isFoil ? 'foil' : 'non-foil'
              Notify.create({
                type: 'warning',
                message: `Stock ${stockType} épuisé pour ${item.name}`,
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
          } else {
            const previousStock = item.stock || 0
            const previousHasStockIssue = item.hasStockIssue || false
            const stockJustBecameUnavailable = !(
              previousStock === 0 &&
              previousHasStockIssue &&
              item.stockIssueMessage === 'STOCK ÉPUISÉ'
            )

            item.stock = 0
            item.limit = 0
            item.hasStockIssue = true
            item.stockIssueMessage = 'STOCK ÉPUISÉ'

            if (stockJustBecameUnavailable && !suppressNotifications) {
              Notify.create({
                type: 'negative',
                message: `Stock épuisé pour ${item.name} (carte non trouvée dans l'inventaire).`,
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
          }
        } catch (error) {}
      }

      if (cardsWithInsufficientStock.length > 0 && !suppressNotifications) {
        const cardList = cardsWithInsufficientStock.map((name) => `• ${name}`).join('<br>')
        Notify.create({
          type: 'warning',
          message: `Quantité réduite pour ces cartes car stock insuffisant :<br>${cardList}`,
          position: 'top',
          badgeStyle: 'display: none',
          html: true,
          timeout: 7000,
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

      saveToLocalStorage()

      try {
        await syncToBackend()
      } catch (error) {}
    } catch (error) {
      items.value.forEach((item) => {
        item.stock = 0
        item.limit = 0
        item.hasStockIssue = true
        item.stockIssueMessage = 'ERREUR DE STOCK'
      })
      if (!suppressNotifications) {
        Notify.create({
          type: 'negative',
          message: 'Erreur lors de la récupération des stocks. Veuillez réessayer.',
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
    } finally {
      saveToLocalStorage()
    }
  }

  const isInCart = (productId, language = null, isFoil = null) => {
    if (language !== null && isFoil !== null) {
      return items.value.some(
        (item) =>
          item.id === productId &&
          (item.language || 'Anglais') === language &&
          (item.isFoil || false) === isFoil,
      )
    } else if (language !== null) {
      return items.value.some(
        (item) => item.id === productId && (item.language || 'Anglais') === language,
      )
    } else if (isFoil !== null) {
      return items.value.some((item) => item.id === productId && (item.isFoil || false) === isFoil)
    } else {
      return items.value.some((item) => item.id === productId)
    }
  }

  const getItemQuantity = (productId) => {
    const item = items.value.find((item) => item.id === productId)
    return item ? item.quantity : 0
  }

  const getCartItemsWithLatestData = async () => {
    try {
      const itemsWithLatestData = await Promise.all(
        items.value.map(async (item) => {
          try {
            const response = await cardService.getCardById(item.id)
            if (response && response.data) {
              const card = response.data

              const stock = item.isFoil ? card.stockFoil || 0 : card.stockNonFoil || 0
              const clientLimit = 4
              return {
                ...item,
                price: card.price || card.foilPrice || item.price,
                discount: card.discount || card.Discount || 0,
                limit: Math.min(item.clientLimit || item.limit, stock),
                stock: stock,
                stockFoil: card.stockFoil || 0,
                stockNonFoil: card.stockNonFoil || 0,
                clientLimit: clientLimit,
              }
            }

            return item
          } catch (error) {
            return item
          }
        }),
      )

      return itemsWithLatestData
    } catch (error) {
      return items.value
    }
  }

  const loadCartFromBackend = async () => {
    try {
      const { cartService } = await import('@/services/CartService')
      const { cardService } = await import('@/services/CardService')
      const cartResponse = await cartService.getCart()

      if (cartResponse && cartResponse.items) {
        const backendItems = []

        for (const item of cartResponse.items) {
          const { card, isFoil } = item

          let stockFoil = 0
          let stockNonFoil = 0

          try {
            const filters = card.name ? { searchQuery: card.name } : {}
            let inventoryResponse = await cardService.getInventoryCards(1, 100, filters)

            let inventoryCard = inventoryResponse?.cards?.find((c) => c.id === card.id)

            if (
              !inventoryCard &&
              inventoryResponse?.pagination &&
              inventoryResponse.pagination.totalPages > 1
            ) {
              const maxPagesToSearch = Math.min(10, inventoryResponse.pagination.totalPages)
              for (let page = 2; page <= maxPagesToSearch; page++) {
                const nextPageResponse = await cardService.getInventoryCards(page, 100, filters)
                if (nextPageResponse?.cards) {
                  const foundCard = nextPageResponse.cards.find((c) => c.id === card.id)
                  if (foundCard) {
                    inventoryCard = foundCard
                    break
                  }
                }
              }
            }

            if (!inventoryCard && card.name) {
              const responseWithoutFilter = await cardService.getInventoryCards(1, 100, {})
              if (responseWithoutFilter?.cards) {
                inventoryCard = responseWithoutFilter.cards.find((c) => c.id === card.id)
              }
            }

            if (inventoryCard && inventoryCard.languageQuantities) {
              const itemLanguage = item.language || 'Anglais'

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

              const normalizedLanguage = normalizeLanguage(itemLanguage)

              const languageQuantity = inventoryCard.languageQuantities.find((langQty) => {
                const langName = (langQty.languageName || '').trim().toLowerCase()
                return (
                  langName === normalizedLanguage?.toLowerCase() ||
                  langName === itemLanguage?.toLowerCase() ||
                  (langName === 'english' && itemLanguage === 'Anglais')
                )
              })

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
          } catch (error) {}

          const correctPrice = isFoil ? card.foilPrice || 0 : card.price || 0
          const currentStock = isFoil ? stockFoil : stockNonFoil
          const clientLimit = 4
          const effectiveLimit = Math.min(clientLimit, currentStock)

          let adjustedQuantity = item.quantity
          let hasStockIssue = false
          let stockIssueMessage = ''

          if (currentStock === 0) {
            adjustedQuantity = 0
            hasStockIssue = true
            stockIssueMessage = 'STOCK ÉPUISÉ'
          } else if (item.quantity > currentStock) {
            adjustedQuantity = currentStock
            hasStockIssue = true
            stockIssueMessage = 'STOCK INSUFFISANT'
          }

          const { TranslationService } = await import('@/services/translationService')

          backendItems.push({
            id: card.id,
            name: card.name,
            description: `${card.setName} - ${TranslationService.translateRarity(card.rarity)}`,
            price: correctPrice,
            discount: card.discount || 0,
            quantity: adjustedQuantity,
            limit: effectiveLimit,
            clientLimit: clientLimit,
            stock: currentStock,
            stockFoil: stockFoil,
            stockNonFoil: stockNonFoil,
            isFoil: isFoil,
            hasStockIssue: hasStockIssue,
            stockIssueMessage: stockIssueMessage,
            image: `https://api.scryfall.com/cards/${card.scryfallId}?format=image&version=small`,
            setName: card.setName,
            number: card.number,
            rarity: TranslationService.translateRarity(card.rarity),
            scryfallId: card.scryfallId,
            language: TranslationService.translateLanguage(item.language || 'Anglais'),
          })
        }

        return backendItems
      }
      return []
    } catch (error) {
      return []
    }
  }

  const mergeCarts = (localStorageItems, backendItems) => {
    const mergedItems = []

    for (const localItem of localStorageItems) {
      const existingBackendItem = backendItems.find((item) => item.id === localItem.id)

      if (existingBackendItem) {
        mergedItems.push(localItem)
      } else {
        mergedItems.push(localItem)
      }
    }

    for (const backendItem of backendItems) {
      const existingLocalItem = localStorageItems.find((item) => item.id === backendItem.id)
      if (!existingLocalItem) {
        mergedItems.push(backendItem)
      }
    }

    return mergedItems
  }

  const syncToBackend = async () => {
    try {
      const { cartService } = await import('@/services/CartService')

      const cartItems = items.value.map((item) => ({
        cardId: item.id,
        quantity: item.quantity,
        language: TranslationService.reverseTranslateLanguage(item.language || 'Anglais'),
        IsFoil: item.isFoil,
      }))

      await cartService.updateCart({ items: cartItems })
    } catch (error) {
      throw error
    }
  }

  const syncWithBackend = async () => {
    try {
      const backendItems = await loadCartFromBackend()

      const localStorageItems = items.value

      if (localStorageItems.length === 0) {
        items.value = backendItems
        isSyncedWithBackend.value = true
        saveToLocalStorage()
        return
      }

      const mergedItems = mergeCarts(localStorageItems, backendItems)

      items.value = mergedItems

      await syncToBackend()

      isSyncedWithBackend.value = true

      saveToLocalStorage()
    } catch (error) {}
  }

  const saveToLocalStorage = () => {
    try {
      items.value.forEach((item) => {
        if (!item.language || item.language === null || item.language === undefined) {
        }
      })

      const dataToSave = {
        items: items.value,
        discountRate: discountRate.value,
        shippingCost: shippingCost.value,
        isSyncedWithBackend: isSyncedWithBackend.value,
      }

      localStorage.setItem('magik-johnson-cart', JSON.stringify(dataToSave))
    } catch (error) {}
  }

  const loadFromLocalStorage = () => {
    try {
      const saved = localStorage.getItem('magik-johnson-cart')
      if (saved) {
        const data = JSON.parse(saved)
        items.value = data.items || []
        discountRate.value = data.discountRate || 0
        shippingCost.value = data.shippingCost || 0

        items.value.forEach((item) => {
          if (item.hasStockIssue === undefined) {
            item.hasStockIssue = false
          }
          if (item.stockIssueMessage === undefined) {
            item.stockIssueMessage = ''
          }

          if (item.isFoil === undefined) {
            item.isFoil = false
          }
          if (item.stockFoil === undefined) {
            item.stockFoil = 0
          }
          if (item.stockNonFoil === undefined) {
            item.stockNonFoil = 0
          }
          if (item.language === undefined || item.language === null || item.language === '') {
          } else {
          }
        })
      }
    } catch (error) {
      items.value = []
      discountRate.value = 0
      shippingCost.value = 0
    }
  }

  loadFromLocalStorage()

  return {
    items,
    discountRate,
    shippingCost,
    isSyncedWithBackend,

    itemCount,
    totalQuantity,
    subtotal,
    discount,
    total,

    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    updateStocksInRealTime,
    isInCart,
    getItemQuantity,
    loadFromLocalStorage,
    getCartItemsWithLatestData,
    syncWithBackend,
    loadCartFromBackend,
    saveToLocalStorage,

    resetStore: () => {
      items.value = []
      discountRate.value = 0
      shippingCost.value = 0
      localStorage.removeItem('magik-johnson-cart')
    },
  }
})
