/**
 * Formate un prix avec deux décimales et une virgule (format français)
 * @param {number} price - Le prix à formater
 * @returns {string} - Le prix formaté (ex: "0,10" ou "5202,49")
 */
export function formatPrice(price) {
  if (price === null || price === undefined || isNaN(price)) {
    return '0,00'
  }
  return parseFloat(price).toFixed(2).replace('.', ',')
}

/**
 *
 * @param {number} price
 * @returns {string}
 */
export function formatPriceWithCurrency(price) {
  return `${formatPrice(price)} $`
}
