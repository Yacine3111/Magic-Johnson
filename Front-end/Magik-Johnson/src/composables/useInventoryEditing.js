import { Notify } from 'quasar'

const parseQuantity = (value) => {
  if (value === 'N/A' || value === null || value === undefined) return 0
  if (typeof value === 'string') {
    const parsed = parseInt(value, 10)
    return isNaN(parsed) ? 0 : parsed
  }
  return typeof value === 'number' ? value : 0
}

export function useInventoryEditing(isEmployeeRef, cardService, updateCardStockInfo) {
  const toggleLanguageEdit = (card, languageQuantity) => {
    if (card.onlineOnly) {
      Notify.create({
        type: 'info',
        message: 'Les cartes en ligne uniquement ne peuvent pas être modifiées.',
        position: 'top',
      })
      return
    }

    languageQuantity.isEditing = !languageQuantity.isEditing
    if (languageQuantity.isEditing) {
      languageQuantity.originalFoilQuantity = card.hasFoil
        ? parseQuantity(languageQuantity.foilQuantity)
        : 0
      languageQuantity.originalNonFoilQuantity = parseQuantity(languageQuantity.nonFoilQuantity)
      languageQuantity.editedFoilQuantity = card.hasFoil
        ? parseQuantity(languageQuantity.foilQuantity)
        : 0
      languageQuantity.editedNonFoilQuantity = parseQuantity(languageQuantity.nonFoilQuantity)
      languageQuantity.stockChangedDuringEdit = false
    }
  }

  const adjustQuantity = (languageQuantity, field, direction) => {
    const currentQtyField = field === 'foil' ? 'foilQuantity' : 'nonFoilQuantity'
    const editedQtyField = field === 'foil' ? 'editedFoilQuantity' : 'editedNonFoilQuantity'

    const currentQty = parseQuantity(languageQuantity[currentQtyField])
    let editedQty = parseQuantity(languageQuantity[editedQtyField])
    const min = isEmployeeRef.value ? currentQty : 0

    if (direction === 'up') {
      languageQuantity[editedQtyField] = editedQty + 1
      if (languageQuantity[editedQtyField] < min) {
        languageQuantity[editedQtyField] = min
      }
    } else {
      if (isEmployeeRef.value && editedQty <= min) {
        Notify.create({
          type: 'negative',
          message: "Les employés ne peuvent qu'augmenter le stock, pas le diminuer.",
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
      if (editedQty > min) {
        languageQuantity[editedQtyField] = editedQty - 1
      } else {
        languageQuantity[editedQtyField] = min
      }
    }
  }

  const cancelLanguageEdit = (languageQuantity) => {
    languageQuantity.isEditing = false
    languageQuantity.editedFoilQuantity = languageQuantity.originalFoilQuantity
    languageQuantity.editedNonFoilQuantity = languageQuantity.originalNonFoilQuantity
    languageQuantity.stockChangedDuringEdit = false
  }

  const saveQuantity = async (card, languageQuantity) => {
    try {
      // Ne pas permettre la sauvegarde si la carte est OnlineOnly
      if (card.onlineOnly) {
        Notify.create({
          type: 'info',
          message: 'Les cartes en ligne uniquement ne peuvent pas être modifiées.',
          position: 'top',
        })
        return
      }

      const originalFoilQty =
        languageQuantity.originalFoilQuantity ?? parseQuantity(languageQuantity.foilQuantity)
      const originalNonFoilQty =
        languageQuantity.originalNonFoilQuantity ?? parseQuantity(languageQuantity.nonFoilQuantity)

      const currentApiFilters = {
        searchQuery: card.name,
        serie: [],
        rarete: [],
        type: [],
        sousType: [],
        couleur: [],
        langue: [],
        sortBy: null,
        priceMin: null,
        priceMax: null,
      }

      const response = await cardService.getInventoryCards(1, 100, currentApiFilters)
      const serverCard = response?.cards?.find((c) => c.id === card.id)
      const serverLanguageQty = serverCard?.languageQuantities?.find(
        (lang) => lang.languageId === languageQuantity.languageId,
      )

      const serverFoilQty = parseQuantity(serverLanguageQty?.foilQuantity)
      const serverNonFoilQty = parseQuantity(serverLanguageQty?.nonFoilQuantity)
      const stockChanged =
        originalFoilQty !== serverFoilQty || originalNonFoilQty !== serverNonFoilQty

      if (stockChanged) {
        Notify.create({
          type: 'info',
          message: 'Stock changé pendant la modification, le nouveau stock a été actualisé.',
          position: 'top',
          timeout: 4000,
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
        languageQuantity.foilQuantity = serverLanguageQty?.foilQuantity || '0'
        languageQuantity.nonFoilQuantity = serverLanguageQty?.nonFoilQuantity || '0'
        languageQuantity.editedFoilQuantity = serverFoilQty
        languageQuantity.editedNonFoilQuantity = serverNonFoilQty
        languageQuantity.originalFoilQuantity = serverFoilQty
        languageQuantity.originalNonFoilQuantity = serverNonFoilQty
        languageQuantity.stockChangedDuringEdit = true
        return
      }

      if (isEmployeeRef.value) {
        const newFoilQty = card.hasFoil ? languageQuantity.editedFoilQuantity || 0 : 0
        const newNonFoilQty = languageQuantity.editedNonFoilQuantity || 0

        if ((card.hasFoil && newFoilQty < serverFoilQty) || newNonFoilQty < serverNonFoilQty) {
          Notify.create({
            type: 'negative',
            message: "Les employés ne peuvent qu'augmenter le stock, pas le diminuer.",
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
          if (card.hasFoil) {
            languageQuantity.editedFoilQuantity = serverFoilQty
          }
          languageQuantity.editedNonFoilQuantity = serverNonFoilQty
          return
        }
      }

      const foilQtyToSave = card.hasFoil ? languageQuantity.editedFoilQuantity || 0 : 0
      const nonFoilQtyToSave = card.hasNonFoil ? languageQuantity.editedNonFoilQuantity || 0 : 0

      await cardService.updateCardLanguageQuantity(
        card.id,
        languageQuantity.languageId,
        foilQtyToSave,
        nonFoilQtyToSave,
      )

      // Convertir en string pour correspondre au nouveau format
      languageQuantity.foilQuantity = card.onlineOnly ? 'N/A' : foilQtyToSave.toString()
      languageQuantity.nonFoilQuantity = card.onlineOnly ? 'N/A' : nonFoilQtyToSave.toString()
      languageQuantity.isEditing = false
      languageQuantity.stockChangedDuringEdit = false

      updateCardStockInfo(card)
    } catch (error) {
      console.error('Erreur lors de la sauvegarde:', error)
      Notify.create({
        type: 'negative',
        message: 'Erreur lors de la mise à jour du stock.',
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
    }
  }

  return {
    toggleLanguageEdit,
    adjustQuantity,
    cancelLanguageEdit,
    saveQuantity,
  }
}
