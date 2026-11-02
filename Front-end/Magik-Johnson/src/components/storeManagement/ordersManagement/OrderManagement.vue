<script setup>
import { onMounted, ref, onBeforeUnmount, watch, computed } from 'vue'
import { useOrderStore } from '@/stores/orderStore'
import { useRouter } from 'vue-router'
import { Notify } from 'quasar'
import { UpdateOrderStatus } from '@/api/order/OrderApi'
import { useAuthStore } from '@/stores/authStore'
import { ORDER_STATUS } from '@/models/OrderStatus'
import '@/assets/catalogue.css'

//Stores & router
const order = useOrderStore()
const router = useRouter()
const authStore = useAuthStore()

const DTO = ref([])
const isLoading = ref(false)
const selectedOrder = ref('')

// Nouvelles variables pour le filtre par heure
const startTime = ref(null)
const endTime = ref(null)

// Variables pour le filtrage
const timeFilterType = ref('')
const sortBy = ref({ field: 'date', order: 'desc' }) // Tri par défaut => date descendantes
const sortByValue = ref('date-desc')
const showMyOrdersOnly = ref(false)
const startDate = ref(null)
const endDate = ref(null)
const dateFilterType = ref('')
const selectedStatus = ref('')

// Gestion des dropdowns
const activeDropdown = ref('')

const toggleDropdown = (name) => {
  if (activeDropdown.value === name) {
    activeDropdown.value = ''
  } else {
    activeDropdown.value = name
  }
}

const closeDropdowns = (event) => {
  if (!event.target.closest('.dropdown')) {
    activeDropdown.value = ''
  }
}

//Filtres
const statusFilter = computed(() => {
  if (order.selectedOrderStatus.length === 0) return ''
  if (order.selectedOrderStatus.length === 1) return order.selectedOrderStatus[0]
  return `${order.selectedOrderStatus.length} statuts`
})

const hasActiveFilters = computed(() => {
  return (
    order.selectedOrderStatus.length > 0 ||
    showMyOrdersOnly.value ||
    startDate.value ||
    endDate.value ||
    startTime.value ||
    endTime.value
  )
})

const canUseEmployeeFilter = computed(() => authStore.isUserEmployee)

let spinnerTimeout

const reloadOrders = async () => {
  clearTimeout(spinnerTimeout)
  isLoading.value = true
  filterOrders()
  currentPage.value = 1
  spinnerTimeout = setTimeout(() => (isLoading.value = false), 500)
}

watch(() => order.selectedOrderStatus, reloadOrders)

//Chargement des commandes (filtrées)
const filterOrders = () => {
  if (!order.orders?.items) {
    DTO.value = []
    return
  }

  let filtered = [...order.orders.items]

  if (order.selectedOrderStatus.length > 0) {
    filtered = filtered.filter((o) => order.selectedOrderStatus.includes(o.orderStatus.name))
  }

  if (showMyOrdersOnly.value && authStore.user) {
    filtered = filtered.filter((o) => o.employeeId === authStore.user.id)
  }

  if (startDate.value) {
    const start = new Date(startDate.value)
    start.setHours(0, 0, 0, 0)
    filtered = filtered.filter((o) => {
      const orderDate = toLocalDate(o.checkoutDate)
      orderDate.setHours(0, 0, 0, 0)
      return orderDate >= start
    })
  }

  if (endDate.value) {
    const end = new Date(endDate.value)
    end.setHours(23, 59, 59, 999)
    filtered = filtered.filter((o) => {
      const orderDate = toLocalDate(o.checkoutDate)
      return orderDate <= end
    })
  }

  if (startTime.value && endTime.value) {
    const [startH, startM] = startTime.value.split(':').map(Number)
    const [endH, endM] = endTime.value.split(':').map(Number)

    const startMinutes = startH * 60 + startM
    const endMinutes = endH * 60 + endM

    filtered = filtered.filter((o) => {
      const orderDate = toLocalDate(o.checkoutDate)
      const orderMinutes = orderDate.getHours() * 60 + orderDate.getMinutes()

      if (startMinutes <= endMinutes) {
        return orderMinutes >= startMinutes && orderMinutes <= endMinutes
      } else {
        return orderMinutes >= startMinutes || orderMinutes <= endMinutes
      }
    })
  }

  if (sortBy.value.field) {
    filtered.sort((a, b) => {
      let comparison = 0

      switch (sortBy.value.field) {
        case 'date':
          const dateA = new Date(a.checkoutDate).getTime()
          const dateB = new Date(b.checkoutDate).getTime()
          comparison = dateA - dateB
          break
        case 'quantity':
          comparison = (a.totalQuantity || 0) - (b.totalQuantity || 0)
          break
        default:
          comparison = 0
      }

      return sortBy.value.order === 'asc' ? comparison : -comparison
    })
  }

  DTO.value = filtered
}

const toggleFilter = (filterType, value) => {
  const filterArray = order[filterType]
  if (filterArray.includes(value)) {
    order[filterType] = filterArray.filter((item) => item !== value)
  } else {
    order[filterType] = [...filterArray, value]
  }
}

function editStatus(orderItem) {
  selectedOrder.value = orderItem
  selectedStatus.value = orderItem.orderStatus.id
}

function goToOrder(orderId) {
  router.push({ name: 'DetailsCommande', params: { orderId } })
}

//Supprimer les filtres actifs
const clearFilters = () => {
  order.clearStoreFilters()
  if (showMyOrdersOnly.value) {
    showMyOrdersOnly.value = false
  }
  startDate.value = null
  endDate.value = null
  dateFilterType.value = ''
  startTime.value = null
  endTime.value = null
  timeFilterType.value = ''
  reloadOrders()
}

const applyDateFilter = (type) => {
  if (type === 'all') {
    dateFilterType.value = 'all'
    startDate.value = null
    endDate.value = null
    reloadOrders()
    return
  }

  if (dateFilterType.value === type) {
    dateFilterType.value = 'all'
    startDate.value = null
    endDate.value = null
    reloadOrders()
    return
  }

  dateFilterType.value = type

  const today = new Date()
  today.setHours(0, 0, 0, 0)

  switch (type) {
    case 'today':
      startDate.value = today.toISOString().split('T')[0]
      endDate.value = new Date(today.getTime() + 24 * 60 * 60 * 1000 - 1)
        .toISOString()
        .split('T')[0]
      break
    case 'thisWeek':
      const startOfWeek = new Date(today)
      startOfWeek.setDate(today.getDate() - today.getDay())
      startDate.value = startOfWeek.toISOString().split('T')[0]
      endDate.value = new Date(today.getTime() + 24 * 60 * 60 * 1000 - 1)
        .toISOString()
        .split('T')[0]
      break
    case 'thisMonth':
      startDate.value = new Date(today.getFullYear(), today.getMonth(), 1)
        .toISOString()
        .split('T')[0]
      endDate.value = new Date(today.getTime() + 24 * 60 * 60 * 1000 - 1)
        .toISOString()
        .split('T')[0]
      break

    case 'thisYear':
      startDate.value = new Date(today.getFullYear(), 0, 1).toISOString().split('T')[0]
      endDate.value = new Date(today.getTime() + 24 * 60 * 60 * 1000 - 1)
        .toISOString()
        .split('T')[0]
      break
    case 'custom':
      break
    case 'all':
      startDate.value = null
      endDate.value = null
      break
  }
  reloadOrders()
}

const applyTimeFilter = (type) => {
  if (type === 'all') {
    timeFilterType.value = 'all'
    startTime.value = null
    endTime.value = null
    reloadOrders()
    return
  }

  if (timeFilterType.value === type) {
    timeFilterType.value = 'all'
    startTime.value = null
    endTime.value = null
    reloadOrders()
    return
  }

  timeFilterType.value = type

  switch (type) {
    case 'morning':
      startTime.value = '06:00'
      endTime.value = '12:00'
      break
    case 'afternoon':
      startTime.value = '12:00'
      endTime.value = '18:00'
      break
    case 'evening':
      startTime.value = '18:00'
      endTime.value = '23:59'
      break
    case 'night':
      startTime.value = '00:00'
      endTime.value = '06:00'
      break
    case 'custom':
      break
  }
  reloadOrders()
}

const getTimeFromType = (type) => {
  switch (type) {
    case 'morning':
      return { start: '06:00', end: '12:00' }
    case 'afternoon':
      return { start: '12:00', end: '18:00' }
    case 'evening':
      return { start: '18:00', end: '23:59' }
    case 'night':
      return { start: '00:00', end: '06:00' }
    default:
      return { start: null, end: null }
  }
}

const onCustomDateChange = () => {
  if (startDate.value && endDate.value) {
    dateFilterType.value = 'custom'
    reloadOrders()
  }
}

// gérer le changement d'heure
const onCustomTimeChange = () => {
  if (startTime.value && endTime.value) {
    const morning = getTimeFromType('morning')
    const afternoon = getTimeFromType('afternoon')
    const evening = getTimeFromType('evening')
    const night = getTimeFromType('night')

    if (startTime.value === morning.start && endTime.value === morning.end) {
      timeFilterType.value = 'morning'
    } else if (startTime.value === afternoon.start && endTime.value === afternoon.end) {
      timeFilterType.value = 'afternoon'
    } else if (startTime.value === evening.start && endTime.value === evening.end) {
      timeFilterType.value = 'evening'
    } else if (startTime.value === night.start && endTime.value === night.end) {
      timeFilterType.value = 'night'
    } else {
      timeFilterType.value = 'custom'
    }
    reloadOrders()
  }
}

const handleSortChange = () => {
  if (!sortByValue.value) {
    sortBy.value = { field: null, order: 'asc' }
  } else {
    const [field, order] = sortByValue.value.split('-')
    sortBy.value = { field, order }
  }
  reloadOrders()
}

const toggleMyOrdersFilter = () => {
  showMyOrdersOnly.value = !showMyOrdersOnly.value
  reloadOrders()
}

const toLocalDate = (dateStr) => {
  if (!dateStr) return null
  return new Date(dateStr.replace(/(Z|[+-]\d{2}:?\d{2})$/, ''))
}

const formatDate = (iso) => iso?.split('T')[0] ?? '—'
const formatTime = (iso) => iso?.substring(11, 19) ?? ''
const truncateId = (id) => id?.toString().substring(0, 8) ?? '—'

//Submit du nouveau statut
const notify = (type, message) => {
  Notify.create({
    type,
    message,
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

async function submitOrderStatus() {
  if (!selectedOrder.value) return console.warn('Aucune commande sélectionnée')

  const updates = { orderId: selectedOrder.value.id, newStatus: selectedStatus.value }
  try {
    const response = await UpdateOrderStatus(updates)
    if (response == 200) {
      notify('positive', 'Statut de la commande actualisé avec succès')
      await order.fetchAllOrders()
      DTO.value = order.orders?.items ?? []
    } else {
      notify('negative', 'Erreur : impossible d' + 'actualiser le statut de la commande')
    }
  } catch (error) {
    console.error('Erreur de la mise à jour : ', error)
  }
}

//Pagination
const currentPage = ref(1)
const itemsPerPage = 8

const totalPages = computed(() => Math.ceil(DTO.value.length / itemsPerPage))

const paginatedOrders = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return DTO.value.slice(start, end)
})

const maxVisiblePages = 5 // nombre maximal de pages visibles
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

onMounted(async () => {
  if (!authStore.role) {
    await authStore.fetchRole()
  }
  await order.fetchAllOrders()
  await order.fetchOrderStatus()
  DTO.value = order.orders?.items ?? []
  document.addEventListener('click', closeDropdowns)
})

onBeforeUnmount(() => {
  clearTimeout(spinnerTimeout)
  document.removeEventListener('click', closeDropdowns)
})
</script>

<template>
  <div class="order-container">
    <div class="title-section">
      <div class="text-h5 text-bold">Gestion des commandes</div>
    </div>
    <div
      v-if="$q.screen.gt.sm"
      class="filters-bar d-flex align-items-center gap-2 p-3 text-light"
      style="background-color: rgb(35, 42, 56)"
    >
      <div class="dropdown status-dropdown-position">
        <button
          class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
          :class="statusFilter ? 'btn-info' : 'btn-secondary'"
          type="button"
          @click="toggleDropdown('status')"
        >
          <span>{{ statusFilter || 'Statut' }}</span>
        </button>
        <div class="dropdown-menu p-2" :class="{ show: activeDropdown === 'status' }">
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
      <!--Filtrer par Date-->
      <div class="dropdown">
        <button
          class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
          :class="dateFilterType ? 'btn-info' : 'btn-secondary'"
          type="button"
          @click="toggleDropdown('date')"
        >
          <span>Filtrer par date</span>
        </button>
        <div class="dropdown-menu p-2" :class="{ show: activeDropdown === 'date' }">
          <div class="dropdown-list-container">
            <div class="dropdown-checkbox-item" @click="applyDateFilter('today')">
              <input type="checkbox" :checked="dateFilterType === 'today'" />
              Aujourd'hui
            </div>
            <div class="dropdown-checkbox-item" @click="applyDateFilter('thisWeek')">
              <input type="checkbox" :checked="dateFilterType === 'thisWeek'" />
              Cette semaine
            </div>
            <div class="dropdown-checkbox-item" @click="applyDateFilter('thisMonth')">
              <input type="checkbox" :checked="dateFilterType === 'thisMonth'" />
              Ce mois
            </div>

            <div class="dropdown-checkbox-item" @click="applyDateFilter('thisYear')">
              <input type="checkbox" :checked="dateFilterType === 'thisYear'" />
              Cette année
            </div>
            <div class="dropdown-checkbox-item" @click="applyDateFilter('all')">
              <input type="checkbox" :checked="dateFilterType === 'all'" />
              Toutes les dates
            </div>
          </div>
          <hr style="margin: 0.5rem 0" />
          <div class="p-2">
            <label class="form-label text-light" style="font-size: 0.9rem">Personnalisé</label>
            <div class="d-flex gap-2 mb-2">
              <input
                type="date"
                v-model="startDate"
                class="form-control form-control-sm"
                placeholder="Date début"
                @change="onCustomDateChange"
              />
              <input
                type="date"
                v-model="endDate"
                class="form-control form-control-sm"
                placeholder="Date fin"
                @change="onCustomDateChange"
              />
            </div>
          </div>
        </div>
      </div>

      <!--Filtrer par Heure-->
      <div class="dropdown">
        <button
          class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
          :class="timeFilterType ? 'btn-info' : 'btn-secondary'"
          type="button"
          @click="toggleDropdown('time')"
        >
          <span>Filtrer par heure</span>
        </button>
        <div class="dropdown-menu p-2" :class="{ show: activeDropdown === 'time' }">
          <div class="dropdown-list-container">
            <div class="dropdown-checkbox-item" @click="applyTimeFilter('morning')">
              <input type="checkbox" :checked="timeFilterType === 'morning'" />
              Matin (6h-12h)
            </div>
            <div class="dropdown-checkbox-item" @click="applyTimeFilter('afternoon')">
              <input type="checkbox" :checked="timeFilterType === 'afternoon'" />
              Après-midi (12h-18h)
            </div>
            <div class="dropdown-checkbox-item" @click="applyTimeFilter('evening')">
              <input type="checkbox" :checked="timeFilterType === 'evening'" />
              Soir (18h-00h)
            </div>
            <div class="dropdown-checkbox-item" @click="applyTimeFilter('night')">
              <input type="checkbox" :checked="timeFilterType === 'night'" />
              Nuit (00h-6h)
            </div>

            <div class="dropdown-checkbox-item" @click="applyTimeFilter('all')">
              <input type="checkbox" :checked="timeFilterType === 'all'" />
              Toutes les heures
            </div>
          </div>
          <hr style="margin: 0.5rem 0" />
          <div class="p-2">
            <label class="form-label text-light" style="font-size: 0.9rem">Personnalisé</label>
            <div class="d-flex gap-2 mb-2">
              <input
                type="time"
                v-model="startTime"
                class="form-control form-control-sm"
                placeholder="Heure début"
                @change="onCustomTimeChange"
              />
              <input
                type="time"
                v-model="endTime"
                class="form-control form-control-sm"
                placeholder="Heure fin"
                @change="onCustomTimeChange"
              />
            </div>
          </div>
        </div>
      </div>

      <!--Trier par-->
      <span class="text-white ms-3">Trier par :</span>
      <select
        v-model="sortByValue"
        class="form-select form-select-sm"
        style="width: auto"
        @change="handleSortChange"
      >
        <option value="date-desc">Date (Récent - Ancien)</option>
        <option value="date-asc">Date (Ancien - Récent)</option>
        <option value="quantity-desc">Quantité (Haut - Bas)</option>
        <option value="quantity-asc">Quantité (Bas - Haut)</option>
      </select>

      <div class="d-flex gap-2 ms-auto align-items-center">
        <button
          v-if="canUseEmployeeFilter"
          class="btn btn-sm"
          :class="showMyOrdersOnly ? 'btn-primary' : 'btn-outline-light'"
          style="height: 35px"
          @click="toggleMyOrdersFilter"
        >
          Voir mes commandes
        </button>
        <button class="btn btn-danger btn-sm" style="height: 35px" @click="clearFilters">
          <span>Effacer les filtres</span>
        </button>
      </div>
    </div>

    <div style="position: relative; min-height: 200px">
      <div v-if="DTO.length > 0">
        <table>
          <thead>
            <tr>
              <th></th>
              <th>Identifiant de la commande</th>
              <th>Nombre de cartes commandés</th>
              <th>Date de la commande</th>
              <th>Statut</th>
              <th>Préposé(e) à la commandes</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in paginatedOrders" :key="item.id" class="employe_section">
              <td>
                <q-icon
                  :name="item.deliveryMode === 'HomeDelivery' ? 'local_shipping' : 'store'"
                  class="profil"
                  size="lg"
                />
              </td>
              <td>{{ truncateId(item.id) }}</td>
              <td>{{ item.totalQuantity ?? 0 }}</td>

              <td>
                {{ formatDate(item.checkoutDate) }}
                {{ formatTime(item.checkoutDate) }}
              </td>

              <td>
                <div class="row flex-row justify-content-end no-wrap">
                  <button
                    v-if="
                      item.orderStatusId == ORDER_STATUS.refunded ||
                      item.orderStatusId == ORDER_STATUS.refused
                    "
                    class="btn btn-secondary"
                    type="button"
                    disabled
                  >
                    {{ item.orderStatus.name }}
                  </button>
                  <button
                    v-else-if="item.orderStatusId != ORDER_STATUS.pending"
                    @click="editStatus(item)"
                    class="btn btn-secondary"
                    type="button"
                    data-bs-toggle="offcanvas"
                    data-bs-target="#offcanvas"
                    aria-controls="offcanvas"
                  >
                    {{ item.orderStatus.name }}
                  </button>
                </div>
              </td>
              <td>
                {{
                  item.employeeFirstName && item.employeeLastName
                    ? `${item.employeeFirstName} ${item.employeeLastName}`
                    : item.employeeId
                      ? item.employeeId
                      : 'N/A'
                }}
              </td>
              <td>
                <div class="row flex-row justify-content-end no-wrap">
                  <q-btn label="Détails" class="btn-go-order mr-3" @click="goToOrder(item.id)" />
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div
          class="pagination-container d-flex justify-content-center mt-4 mb-4"
          v-if="totalPages > 1"
        >
          <nav class="custom-pagination">
            <ul class="pagination-list">
              <!-- Bouton Précédent -->
              <li class="pagination-item">
                <button
                  class="pagination-btn prev-btn"
                  :disabled="currentPage === 1"
                  @click="changePage(currentPage - 1)"
                >
                  ‹
                </button>
              </li>

              <!-- Page 1 -->
              <li class="pagination-item" v-if="showFirstPage">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentPage === 1 }"
                  @click="changePage(1)"
                >
                  1
                </button>
              </li>

              <!-- Ellipsis gauche -->
              <li class="pagination-item" v-if="showLeftEllipsis">
                <span class="pagination-ellipsis">...</span>
              </li>

              <!-- Pages visibles -->
              <li class="pagination-item" v-for="page in visiblePages" :key="page">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentPage === page }"
                  @click="changePage(page)"
                >
                  {{ page }}
                </button>
              </li>

              <!-- Ellipsis droite -->
              <li class="pagination-item" v-if="showRightEllipsis">
                <span class="pagination-ellipsis">...</span>
              </li>

              <!-- Dernière page -->
              <li class="pagination-item" v-if="showLastPage">
                <button
                  class="pagination-btn page-btn"
                  :class="{ active: currentPage === totalPages }"
                  @click="changePage(totalPages)"
                >
                  {{ totalPages }}
                </button>
              </li>

              <!-- Bouton Suivant -->
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
      <div v-else-if="!isLoading" class="text-center q-pa-xl text-white">
        <q-icon name="assignment_late" size="4em" class="q-mb-md" />
        <div class="text-h6">Aucune commande trouvée</div>
        <div v-if="hasActiveFilters" class="text-subtitle2">Essayez de modifier vos filtres</div>
      </div>
      <q-inner-loading :showing="isLoading">
        <q-spinner size="50px" color="primary" />
      </q-inner-loading>
    </div>
  </div>
  <div
    class="offcanvas offcanvas-bottom"
    style="height: 50%"
    tabindex="-1"
    id="offcanvas"
    aria-labelledby="offcanvasLabel"
  >
    <div class="offcanvas-header">
      <button
        type="button"
        class="btn-close text-reset"
        data-bs-dismiss="offcanvas"
        aria-label="Close"
      ></button>
    </div>
    <div class="offcanvas-body">
      <div>
        <div id="form-wrapper">
          <form @submit.prevent="submitOrderStatus">
            <div id="slider">
              <input
                type="radio"
                name="order-status"
                id="1"
                :value="ORDER_STATUS.preparing"
                v-model="selectedStatus"
                required
              />
              <label for="1" data-order-status="Préparation"></label>

              <input
                type="radio"
                name="order-status"
                class="order-status"
                id="2"
                :value="ORDER_STATUS.shipped"
                v-model="selectedStatus"
                :disabled="selectedOrder?.deliveryMode !== 'HomeDelivery'"
                required
              />
              <label for="2" data-order-status="Expédié"></label>
              <input
                type="radio"
                name="order-status"
                id="3"
                :value="ORDER_STATUS.delivered"
                v-model="selectedStatus"
                required
              />
              <label for="3" data-order-status="Livré"></label>
              <input
                type="radio"
                name="order-status"
                id="4"
                :value="ORDER_STATUS.cancelled"
                v-model="selectedStatus"
                required
              />
              <label for="4" data-order-status="Annulé"></label>
              <div id="debt-amount-pos"></div>
            </div>
            <button class="btn confirm-btn" type="submit">Confirmer</button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.order-container {
  background-color: #676767;
  min-height: 100vh;
  padding-bottom: 20px;
}

.title-section {
  background-color: #959595;
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
}

.status-dropdown-position {
  position: relative;
}

hr {
  border: none;
  height: 4px;
  background-color: rgb(255, 255, 255);
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead th {
  padding: 10px;
  color: white;
}

tbody td {
  padding: 10px;
  font-size: small;
}

body:not(.body--dark) tbody tr {
  background-color: #efebe9 !important;
  color: black !important;
}

tbody tr {
  background-color: #9c9a9a;
  color: #fff;
  border-top: 2px solid #ffffff;
  border-bottom: 2px solid #ffffff;
  transition:
    opacity 0.3s ease,
    background-color 0.3s ease;
}

.profil {
  color: #89412b !important;
}

.btn-go-order {
  color: white !important;
  background: #5d4037 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

/*Style du gestionnaire de status*/
#form-wrapper {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

form {
  width: 90%;
  max-width: 500px;
}

form #form-title {
  margin-top: 0;
  font-weight: 400;
  text-align: center;
}

form #slider {
  display: flex;
  flex-direction: row;
  align-content: stretch;
  position: relative;
  width: 100%;
  height: 50px;
  user-select: none;
}

form #slider::before {
  content: ' ';
  position: absolute;
  height: 2px;
  width: 100%;
  width: calc(100% * (3 / 4));
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: #000;
}

form #slider input,
form #slider label {
  box-sizing: border-box;
  flex: 1;
  user-select: none;
  cursor: pointer;
}

form #slider label {
  display: inline-block;
  position: relative;
  width: 25%;
  height: 100%;
  user-select: none;
}

form #slider label::before {
  content: attr(data-order-status);
  position: absolute;
  left: 50%;
  padding-top: 10px;
  transform: translate(-50%, 45px);
  font-size: 14px;
  letter-spacing: 0.4px;
  font-weight: 400;
  white-space: nowrap;
  opacity: 0.85;
  transition: all 0.15s ease-in-out;
}

form #slider label::after {
  content: ' ';
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  width: 30px;
  height: 30px;
  border: 2px solid #000;
  background: #fff;
  border-radius: 50%;
  pointer-events: none;
  user-select: none;
  z-index: 1;
  cursor: pointer;
  transition: all 0.15s ease-in-out;
}

form #slider label:hover::after {
  transform: translate(-50%, -50%) scale(1.25);
}

form #slider input {
  display: none;
}

form #slider input:checked + label::before {
  font-weight: 800;
  opacity: 1;
}

form #slider input:checked + label::after {
  border-width: 4px;
  transform: translate(-50%, -50%) scale(0.75);
}

form #slider input:checked ~ #debt-amount-pos {
  opacity: 1;
}

form #slider input:checked:nth-child(1) ~ #debt-amount-pos {
  left: 12.5%;
}

form #slider input:checked:nth-child(3) ~ #debt-amount-pos {
  left: 37.5%;
}

form #slider input:checked:nth-child(5) ~ #debt-amount-pos {
  left: 62.5%;
}

form #slider input:checked:nth-child(7) ~ #debt-amount-pos {
  left: 87.5%;
}

form #slider #debt-amount-pos {
  display: block;
  position: absolute;
  top: 50%;
  width: 12px;
  height: 12px;
  background: #000;
  border-radius: 50%;
  transition: all 0.15s ease-in-out;
  transform: translate(-50%, -50%);
  border: 2px solid #fff;
  opacity: 0;
  z-index: 2;
}

.confirm-btn {
  display: block;
  position: relative;
  z-index: 1;
  margin: 70px auto 0;
  padding: 5px 10px;
  appearance: none;
  transition: all 0.15s ease-in-out;
  font-family: inherit;
  font-size: 18px;
  font-weight: 500;
  background: #fff;
  border: 1px solid #000;
  border-radius: 8px;
  outline: 0;
  user-select: none;
  cursor: pointer;
}

.order-status:disabled {
  opacity: 0.2;
  cursor: not-allowed;
  transform: scale(0.95);
  filter: grayscale(100%);
}

.order-status:disabled + label {
  opacity: 0.2;
  cursor: not-allowed;
}

body:not(.body--dark) .order-container {
  background-color: #d1cbbb !important;
  color: black !important;
}

body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .filters-bar {
  background-color: #a89c8d !important;
  color: black !important;
}

body:not(.body--dark) .filters-bar .text-white {
  color: black !important;
}

body:not(.body--dark) .filters-bar .text-light {
  color: black !important;
}

body:not(.body--dark) .filters-bar .btn {
  color: white !important;
}
</style>
