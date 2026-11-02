<script setup>
import '@/assets/gestionCompte.css'
import { onMounted, onBeforeUnmount, ref, watch, computed } from 'vue'
import cardOrder from '../orders/cardOrder.vue'
import { useOrderStore } from '@/stores/orderStore'

import { GetFilteredOrders } from '@/api/order/OrderApi'
import { Notify } from 'quasar'

const order = useOrderStore()

//Variables pour le filtre sur la date
const startDate = ref('')
const endDate = ref('')

//Variables pour la liste des  commandes
const DTO = ref([])
const isLoading = ref(false)

let delayTimeoutTimer
let spinnerTimeout

const delayTimeoutReload = () => {
  clearTimeout(delayTimeoutTimer)
  delayTimeoutTimer = setTimeout(reloadOrders, 800)
}

watch(() => [order.selectedOrderStatus, startDate.value, endDate.value], delayTimeoutReload)

const reloadOrders = async () => {
  try {
    clearTimeout(spinnerTimeout)

    // Vérification de la validité des dates avant de comparer
    if (startDate.value && endDate.value) {
      const start = new Date(startDate.value)
      const end = new Date(endDate.value)

      if (end < start) {
        Notify.create({
          type: 'negative',
          message: 'La date de fin ne peut pas être antérieure à la date de début',
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
    }

    isLoading.value = true
    await loadOrders()
  } catch (error) {
  } finally {
    spinnerTimeout = setTimeout(() => {
      isLoading.value = false
    }, 1000)
  }
}

const sortOrdersByDate = (orders) => {
  return [...orders].sort((a, b) => {
    const dateA = new Date(a.checkoutDate)
    const dateB = new Date(b.checkoutDate)
    return dateB - dateA
  })
}
const formatEndDate = (dateString) => {
  if (!dateString) return null
  if (dateString.length === 10) {
    return `${dateString}T23:59:59.999Z`
  }
  return dateString
}

const loadOrders = async () => {
  try {
    const formattedEndDate = formatEndDate(endDate.value)
    const response = await GetFilteredOrders(
      order.selectedOrderStatus,
      startDate.value,
      formattedEndDate,
    )

    if (response && response.length > 0) {
      DTO.value = sortOrdersByDate(response)
    } else if (
      order.selectedOrderStatus.length > 0 ||
      startDate.value != '' ||
      endDate.value != ''
    ) {
      DTO.value = []
    } else {
      DTO.value = sortOrdersByDate(order.orders.items || [])
    }
    currentPage.value = 1
  } catch (err) {
    DTO.value = []
  }
}

const currentPage = ref(1)
const itemsPerPage = 5

const totalPages = computed(() => Math.ceil(DTO.value.length / itemsPerPage))

const paginatedOrders = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return DTO.value.slice(start, end)
})

const maxVisiblePages = 5
const visiblePages = computed(() => {
  const pages = []
  const half = Math.floor(maxVisiblePages / 2)
  let start = currentPage.value - half
  let end = currentPage.value + half

  if (start < 1) {
    start = 1
    end = Math.min(totalPages.value, maxVisiblePages)
  }

  if (end > totalPages.value) {
    end = totalPages.value
    start = Math.max(1, totalPages.value - maxVisiblePages + 1)
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})

const showFirstPage = computed(() => {
  return visiblePages.value[0] !== 1
})
const showLastPage = computed(() => {
  return totalPages.value > 1 && visiblePages.value.at(-1) !== totalPages.value
})
const showLeftEllipsis = computed(() => visiblePages.value[0] > 2)
const showRightEllipsis = computed(() => visiblePages.value.at(-1) < totalPages.value - 1)

function changePage(page) {
  if (page < 1) page = 1
  if (page > totalPages.value) page = totalPages.value
  currentPage.value = page
}

const toggleFilter = (filterType, value) => {
  const filterArray = order[filterType]
  if (filterArray.includes(value)) {
    order[filterType] = filterArray.filter((item) => item !== value)
  } else {
    order[filterType] = [...filterArray, value]
  }
}

const clearFilters = () => {
  order.clearStoreFilters()
  startDate.value = ''
  endDate.value = ''
}

onMounted(async () => {
  await order.fetchOrder()
  await order.fetchOrderStatus()
  DTO.value = sortOrdersByDate(order.orders.items || [])
})

onBeforeUnmount(() => {
  clearTimeout(delayTimeoutTimer)
  clearTimeout(spinnerTimeout)
})
</script>
<template>
  <q-page class="fit q-pa-xs-sm q-pa-md-lg">
    <q-card class="row q-ma-md q-pa-sm text-align-center title-card">
      <h5>Historique des commandes</h5>
    </q-card>
    <q-card class="row q-ma-xs-none q-ma-md-md info-card">
      <q-card-section class="title-2-card">
        <h6>Commandes et états</h6>
      </q-card-section>
      <q-card-section class="col q-pa-xs-sm q-pa-md-xl relative-position">
        <div class="d-flex flex-wrap align-items-center" style="gap: 20px">
          <div class="dropdown flex align-items-center full-width-mobile-container">
            <p class="q-mr-sm no-margin">Filtrer par :</p>
            <button
              class="btn btn-secondary btn-sm dropdown-toggle d-flex justify-content-between align-items-center full-width-mobile"
              type="button"
              data-bs-toggle="dropdown"
              data-bs-auto-close="outside"
            >
              <span>
                {{ order.orderStatus.length === 1 ? order.orderStatus[0] : 'Statut' }}
              </span>
              <span v-if="order.orderStatus.length > 1" class="badge bg-primary ms-2">{{
                order.orderStatus.length
              }}</span>
            </button>
            <div class="dropdown-menu p-2">
              <div class="dropdown-list-container">
                <div
                  v-for="opt in order.orderStatus.status"
                  :key="opt"
                  class="dropdown-checkbox-item"
                  @click="toggleFilter('selectedOrderStatus', opt)"
                >
                  <input type="checkbox" :checked="order.selectedOrderStatus.includes(opt)" />
                  {{ opt }}
                </div>
              </div>
            </div>
          </div>
          <div class="dropdown flex align-items-center full-width-mobile-container">
            <p class="q-mr-sm no-margin">Filtrer par :</p>
            <button
              class="btn btn-secondary btn-sm dropdown-toggle d-flex justify-content-between align-items-center full-width-mobile"
              type="button"
              data-bs-toggle="dropdown"
              data-bs-auto-close="outside"
            >
              <span> Date de paiement</span>
            </button>
            <div class="dropdown-menu p-2">
              <div class="dropdown-list-container d-flex align-items-center">
                <input
                  id="startDate"
                  :value="startDate"
                  @change="(e) => (startDate = e.target.value)"
                  type="date"
                  class="form-control q-mr-sm"
                />
                <label class="q-mr-sm" for="endDate"> - </label>
                <input
                  id="endDate"
                  :value="endDate"
                  @change="(e) => (endDate = e.target.value)"
                  type="date"
                  class="form-control"
                />
              </div>
            </div>
          </div>
          <div class="d-flex gap-2 ms-auto">
            <button class="btn btn-danger btn-sm" style="height: 35px" @click="clearFilters">
              <span>Effacer les filtres</span>
            </button>
          </div>
        </div>
        <div v-if="DTO.length > 0" class="q-pa-sm">
          <card-order
            class="q-mb-lg"
            v-for="commande in paginatedOrders"
            :key="commande.id"
            :item="commande"
          >
          </card-order>

          <div
            class="pagination-container d-flex justify-content-center mt-4 mb-4"
            v-if="totalPages > 1"
          >
            <nav class="custom-pagination">
              <ul class="pagination-list">
                <li class="pagination-item">
                  <button
                    class="pagination-btn prev-btn"
                    :disabled="currentPage === 1"
                    @click="changePage(currentPage - 1)"
                  >
                    ‹
                  </button>
                </li>

                <li class="pagination-item" v-if="showFirstPage">
                  <button
                    class="pagination-btn page-btn"
                    :class="{ active: currentPage === 1 }"
                    @click="changePage(1)"
                  >
                    1
                  </button>
                </li>

                <li class="pagination-item" v-if="showLeftEllipsis">
                  <span class="pagination-ellipsis">...</span>
                </li>

                <li class="pagination-item" v-for="page in visiblePages" :key="page">
                  <button
                    class="pagination-btn page-btn"
                    :class="{ active: currentPage === page }"
                    @click="changePage(page)"
                  >
                    {{ page }}
                  </button>
                </li>

                <li class="pagination-item" v-if="showRightEllipsis">
                  <span class="pagination-ellipsis">...</span>
                </li>

                <li class="pagination-item" v-if="showLastPage">
                  <button
                    class="pagination-btn page-btn"
                    :class="{ active: currentPage === totalPages }"
                    @click="changePage(totalPages)"
                  >
                    {{ totalPages }}
                  </button>
                </li>

                <li class="pagination-item">
                  <button
                    class="pagination-btn next-btn"
                    :disabled="currentPage === totalPages"
                    @click="changePage(currentPage + 1)"
                  >
                    ›
                  </button>
                </li>
              </ul>
            </nav>
          </div>
        </div>
        <p v-else>Aucune commande</p>
        <q-inner-loading :showing="isLoading">
          <q-spinner-gears size="50px" color="primary" />
        </q-inner-loading>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<style scoped>
@media (max-width: 599px) {
  .full-width-mobile {
    width: 100%;
  }
  .full-width-mobile-container {
    width: 100%;
  }
}
</style>

<style>
/* Light Mode */
.q-card {
  background-color: #a89c8d !important;
  color: black;
}

/* Dark Mode  */
body.body--dark .q-card {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark h5,
body.body--dark h6,
body.body--dark p,
body.body--dark span,
body.body--dark .field-label {
  color: white !important;
}

body.body--dark .btn-secondary {
  color: white !important;
}
</style>
