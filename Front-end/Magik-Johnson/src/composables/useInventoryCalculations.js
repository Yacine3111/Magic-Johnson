import { computed } from 'vue'
import { getStockInfo } from '@/services/stockService'
import TranslationService from '@/services/translationService'

const parseQuantity = (value) => {
  if (value === 'N/A' || value === null || value === undefined) return 0
  if (typeof value === 'string') {
    const parsed = parseInt(value, 10)
    return isNaN(parsed) ? 0 : parsed
  }
  return typeof value === 'number' ? value : 0
}

export function useInventoryCalculations(cardsRef) {
  const calculateLanguageTotalQuantity = (card, langQuantity) => {
    const foilQty = card.hasFoil ? parseQuantity(langQuantity.foilQuantity) : 0
    const nonFoilQty = card.hasNonFoil ? parseQuantity(langQuantity.nonFoilQuantity) : 0
    return foilQty + nonFoilQty
  }

  const calculateCardTotalQuantity = (card) => {
    return (
      card.languageQuantities?.reduce(
        (total, lang) => total + calculateLanguageTotalQuantity(card, lang),
        0,
      ) || 0
    )
  }

  const calculatedTotalQuantity = computed(() => {
    let total = 0
    cardsRef.value.forEach((card) => {
      if (card.languageQuantities && card.languageQuantities.length > 0) {
        card.languageQuantities.forEach((langQuantity) => {
          const foilQty = langQuantity.isEditing
            ? parseQuantity(langQuantity.editedFoilQuantity)
            : parseQuantity(langQuantity.foilQuantity)
          const nonFoilQty = langQuantity.isEditing
            ? parseQuantity(langQuantity.editedNonFoilQuantity)
            : parseQuantity(langQuantity.nonFoilQuantity)

          const finalFoilQty = card.hasFoil ? foilQty : 0
          const finalNonFoilQty = card.hasNonFoil ? nonFoilQty : 0

          total += finalFoilQty + finalNonFoilQty
        })
      }
    })
    return total
  })

  const updateCardStockInfo = (card) => {
    const totalQuantity = calculateCardTotalQuantity(card)
    const translatedRarity = TranslationService.translateRarity(card.rarity)
    const stockInfo = getStockInfo(totalQuantity, translatedRarity, card.rarity)
    card.totalQuantity = totalQuantity
    card.stockClass = stockInfo.class
    card.stockLevel = stockInfo.level
    card.stockTooltip = stockInfo.tooltip
  }

  const transformCardForInventory = (card) => {
    const totalQuantity = calculateCardTotalQuantity(card)
    const translatedRarity = TranslationService.translateRarity(card.rarity)
    const stockInfo = getStockInfo(totalQuantity, translatedRarity, card.rarity)

    return {
      ...card,
      rarete: translatedRarity,
      type: TranslationService.translateType(card.type),
      couleur: TranslationService.translateColor(card.couleur),
      disponibilite: totalQuantity > 0 ? 'En stock' : 'En rupture de stock',
      isEditing: false,
      editedQuantity: totalQuantity,
      stockClass: stockInfo.class,
      stockLevel: stockInfo.level,
      stockTooltip: stockInfo.tooltip,
      totalQuantity,
    }
  }

  return {
    calculatedTotalQuantity,
    calculateCardTotalQuantity,
    calculateLanguageTotalQuantity,
    transformCardForInventory,
    updateCardStockInfo,
  }
}
