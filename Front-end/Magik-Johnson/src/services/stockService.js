import TranslationService from './translationService'

export const STOCK_BY_RARITY = {
  Commune: 200,
  'Peu commune': 150,
  Rare: 100,
  Mythique: 100,
  'Rare mythique': 50,
  Spéciale: 30,
  Bonus: 20,
  Jeton: 100,
  'Terrain de base': 300,
}

/**
 *
 *
 * @param {string} rarity
 * @returns {number}
 */
export const getMaxStockByRarity = (rarity) => {
  if (!rarity) return 100

  const frenchRarity = TranslationService.translateRarity(rarity)

  if (STOCK_BY_RARITY[frenchRarity]) {
    return STOCK_BY_RARITY[frenchRarity]
  }

  const normalizedFrench = frenchRarity
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/\s+/g, ' ')
    .trim()

  for (const [key, value] of Object.entries(STOCK_BY_RARITY)) {
    const normalizedKey = key
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/\s+/g, ' ')
      .trim()

    if (
      normalizedFrench === normalizedKey ||
      normalizedFrench.includes(normalizedKey) ||
      normalizedKey.includes(normalizedFrench)
    ) {
      return value
    }
  }

  // Par défaut c'est 100
  return STOCK_BY_RARITY['Rare'] || 100
}

/**
 *
 * @param {number} quantity
 * @param {string} translatedRarity
 * @param {string} originalRarity
 * @returns {Object}
 */
export const getStockInfo = (quantity, translatedRarity = null, originalRarity = null) => {
  const rarity = translatedRarity || originalRarity || 'Rare'
  const maxStock = getMaxStockByRarity(rarity)

  const percentage = (quantity / maxStock) * 100

  if (percentage <= 39) {
    return {
      class: 'stock_status_low',
      level: 'faible',
      tooltip: `Quantité du stock disponible actuel : Faible (${quantity}/${maxStock})`,
    }
  } else if (percentage <= 69) {
    return {
      class: 'stock_status_medium',
      level: 'moyenne',
      tooltip: `Quantité du stock disponible actuel : Moyenne (${quantity}/${maxStock})`,
    }
  } else {
    return {
      class: 'stock_status_high',
      level: 'élevée',
      tooltip: `Quantité du stock disponible actuel : Élevée (${quantity}/${maxStock})`,
    }
  }
}

/**
 *
 * @param {number} stock
 * @param {string} rarity
 * @returns {string}
 */
export const getStatusClass = (stock, rarity = null) => {
  if (!stock && stock !== 0) return 'status-default'

  if (rarity) {
    const maxStock = getMaxStockByRarity(rarity)
    const percentage = (stock / maxStock) * 100

    if (percentage <= 39) return 'status-red'
    if (percentage <= 69) return 'status-orange'
    return 'status-green'
  }

  //fallback
  if (stock < 10) return 'status-red'
  if (stock <= 40) return 'status-orange'
  if (stock > 40) return 'status-green'
  return 'status-default'
}

/**
 *
 * @param {number} Stock
 * @param {string} rarity
 * @returns {string}
 */
export const getStatusText = (stock, rarity = null) => {
  if (!stock && stock !== 0) return 'Stock normal'

  if (rarity) {
    const maxStock = getMaxStockByRarity(rarity)
    const percentage = (stock / maxStock) * 100

    if (percentage <= 39) return 'Stock faible'
    if (percentage <= 69) return 'Stock moyen'
    return 'Stock élevé'
  }

  //fallback
  if (stock < 10) return 'Stock critique'
  if (stock <= 40) return 'Stock faible'
  if (stock > 40) return 'Stock suffisant'
  return 'Stock normal'
}

