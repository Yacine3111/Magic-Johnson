<script setup>
import { ref, computed } from 'vue'
import { useQuasar } from 'quasar'
import card_tendances_plusVendues from './card_components/TrendyMostSoldCard.vue'
import { cardService } from '@/services/CardService'

const $q = useQuasar()
let displayedCards = ref(null)
let loading = ref(true)

const hasCards = computed(() => {
  return displayedCards.value && displayedCards.value.length > 0
})

cardService
  .getMostSoldCards(4, 30)
  .then((response) => {
    if (response && response.data) {
      displayedCards.value = Array.isArray(response.data) ? response.data : []
    } else {
      displayedCards.value = []
    }
  })
  .catch((error) => {
    console.error('Erreur lors du chargement des plus vendues: ', error)
    displayedCards.value = []
  })
  .finally(() => {
    loading.value = false
  })
</script>

<template>
  <div v-if="hasCards || loading" class="tendanceSection_container">
    <h5 class="titre_promotion">Les plus vendues</h5>

    <!-- VERSION PC avec grid -->
    <div :class="[$q.screen.gt.sm ? 'desktop-plus-vendues' : 'hidden']">
      <div v-if="loading" class="loading-spinner-container">
        <q-spinner color="primary" size="3em" />
      </div>
      <div v-else-if="displayedCards && displayedCards.length > 0" class="cards-grid-container">
        <card_tendances_plusVendues
          v-for="card in displayedCards"
          :key="card.id"
          :card="card"
          class="grid-card-item"
        />
      </div>
      <div v-else class="no-cards-message">
        <p class="text-grey-6">Aucune carte disponible</p>
      </div>
    </div>

    <!-- VERSION MOBILE avec grid -->
    <div :class="[$q.screen.lt.md ? 'mobile-plus-vendues' : 'hidden']">
      <div v-if="loading" class="loading-spinner-container-mobile">
        <q-spinner color="primary" size="3em" />
      </div>
      <div
        v-else-if="displayedCards && displayedCards.length > 0"
        class="cards-grid-container mobile-cards-grid"
      >
        <card_tendances_plusVendues
          v-for="card in displayedCards"
          :key="card.id"
          :card="card"
          class="grid-card-item mobile-card"
        />
      </div>
      <div v-else class="no-cards-message-mobile">
        <p class="text-grey-6">Aucune carte disponible</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.hidden {
  display: none !important;
}

.tendanceSection_container {
  width: 99%;
  padding: 5px;
  margin: 7px;
}

.titre_promotion {
  text-decoration: underline;
  margin-left: 20px;
  position: relative;
  left: 10px;
  top: 0px;
}

.cards-grid-container {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  justify-content: center;
  gap: 20px;
  padding: 20px 15px;
  min-height: 320px;
  align-items: center;
}

.grid-card-item {
  flex-shrink: 0;
}

.mobile-plus-vendues {
  padding: 0 10px;
}

.mobile-cards-grid {
  padding: 10px 0;
  display: flex;
  flex-wrap: nowrap;
  overflow-x: auto;
  justify-content: flex-start;
  width: 100%;
  scrollbar-width: none;
}

.mobile-cards-grid::-webkit-scrollbar {
  display: none;
}

.mobile-card {
  flex-shrink: 0;
  width: 280px;
  max-width: 80vw;
}

.loading-spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  min-height: 280px;
}

.loading-spinner-container-mobile {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  min-height: 280px;
}

.no-cards-message {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  min-height: 280px;
}

.no-cards-message-mobile {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  min-height: 280px;
}
</style>

<style>
body.body--dark .titre_promotion {
  color: white !important;
}

body.body--dark .no-cards-message p {
  color: #ccc !important;
}
</style>
