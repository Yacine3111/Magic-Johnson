<script setup>
import { useQuasar } from 'quasar'
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'

import { Notify } from 'quasar'
import { useClientStore } from '@/stores/clientStore'
import router from '@/router'

const $q = useQuasar()
const store = useClientStore()

const clients = ref([])
const allClients = ref([])
const loading = ref(false)
const statusFilter = ref(null)
const sortBy = ref({ field: 'nom', order: 'asc' })
const sortByValue = ref('nom-asc')
const currentPage = ref(1)
const itemsPerPage = ref(10)
const searchTerm = ref('')
const showMobileFilters = ref(false)

const handleClickOutside = (event) => {
  const clickedInDropdown = event.target.closest('.dropdown')
  const clickedDropdownButton = event.target.closest('.dropdown button[data-bs-toggle="dropdown"]')

  if (clickedDropdownButton) {
    return
  }

  if (!clickedInDropdown) {
    const openDropdowns = document.querySelectorAll('.dropdown-menu.show')
    openDropdowns.forEach((menu) => {
      menu.classList.remove('show')
    })
  }
}

onMounted(async () => {
  try {
    loading.value = true
    await store.listClientAsync()
    filterClients()

    document.addEventListener('click', handleClickOutside)
  } catch (Error) {
    Notify.create({
      type: 'negative',
      message: 'Nous sommes désolés, une erreur est survenue.',
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
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

const gererClientClick = (clientId) => {
  router.push({ path: '/gestionMagasin/manage-client', query: { id: clientId } })
}

const filterClients = () => {
  if (!(store.listCustomers == null)) {
    let filtered = [...store.listCustomers]

    if (searchTerm.value.trim()) {
      const searchLower = searchTerm.value.toLowerCase().trim()
      filtered = filtered.filter((client) => {
        const name = (client.name || '').toLowerCase()
        const firstName = (client.firstName || '').toLowerCase()
        return name.includes(searchLower) || firstName.includes(searchLower)
      })
    }

    if (statusFilter.value) {
      filtered = filtered.filter((client) => {
        if (statusFilter.value === 'Actif' && !client.isAccountLocked) return true
        if (statusFilter.value === 'Inactif' && client.isAccountLocked) return true
        return false
      })
    }

    if (sortBy.value.field === 'nom') {
      filtered = filtered.sort((a, b) => {
        const nameComparison = (a.name || '').localeCompare(b.name || '', 'fr', {
          sensitivity: 'base',
        })
        if (nameComparison !== 0) {
          return sortBy.value.order === 'asc' ? nameComparison : -nameComparison
        }

        const firstNameComparison = (a.firstName || '').localeCompare(b.firstName || '', 'fr', {
          sensitivity: 'base',
        })
        return sortBy.value.order === 'asc' ? firstNameComparison : -firstNameComparison
      })
    } else if (sortBy.value.field === 'prenom') {
      filtered = filtered.sort((a, b) => {
        const firstNameComparison = (a.firstName || '').localeCompare(b.firstName || '', 'fr', {
          sensitivity: 'base',
        })
        if (firstNameComparison !== 0) {
          return sortBy.value.order === 'asc' ? firstNameComparison : -firstNameComparison
        }

        const nameComparison = (a.name || '').localeCompare(b.name || '', 'fr', {
          sensitivity: 'base',
        })
        return sortBy.value.order === 'asc' ? nameComparison : -nameComparison
      })
    } else if (sortBy.value.field === 'statut') {
      filtered = filtered.sort((a, b) => {
        if (a.isAccountLocked !== b.isAccountLocked) {
          const statutComparison = a.isAccountLocked ? 1 : -1 // Actifs (false) avant inactifs (true)
          return sortBy.value.order === 'asc' ? statutComparison : -statutComparison
        }

        const nameComparison = (a.name || '').localeCompare(b.name || '', 'fr', {
          sensitivity: 'base',
        })
        if (nameComparison !== 0) {
          return nameComparison
        }

        return (a.firstName || '').localeCompare(b.firstName || '', 'fr', { sensitivity: 'base' })
      })
    } else {
      filtered = filtered.sort((a, b) => {
        const nameComparison = (a.name || '').localeCompare(b.name || '', 'fr', {
          sensitivity: 'base',
        })
        if (nameComparison !== 0) {
          return nameComparison
        }
        return (a.firstName || '').localeCompare(b.firstName || '', 'fr', { sensitivity: 'base' })
      })
    }

    allClients.value = filtered
    applyPagination()
  }
}

const applyPagination = () => {
  const startIndex = (currentPage.value - 1) * itemsPerPage.value
  const endIndex = startIndex + itemsPerPage.value
  clients.value = allClients.value.slice(startIndex, endIndex)
}

const totalPages = computed(() => {
  return Math.ceil(allClients.value.length / itemsPerPage.value)
})

const hasNextPage = computed(() => currentPage.value < totalPages.value)
const hasPreviousPage = computed(() => currentPage.value > 1)

const goToNextPage = () => {
  if (hasNextPage.value) {
    currentPage.value++
    applyPagination()
  }
}

const goToPreviousPage = () => {
  if (hasPreviousPage.value) {
    currentPage.value--
    applyPagination()
  }
}

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    applyPagination()
  }
}

const getVisiblePages = () => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 3) {
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
    } else if (current >= total - 2) {
      for (let i = total - 4; i <= total; i++) {
        pages.push(i)
      }
    } else {
      for (let i = current - 2; i <= current + 2; i++) {
        pages.push(i)
      }
    }
  }

  return pages
}

const toggleStatusFilter = (status) => {
  if (statusFilter.value === status) {
    statusFilter.value = null
  } else {
    statusFilter.value = status
    if (sortBy.value.field === 'statut') {
      sortBy.value = { field: 'nom', order: 'asc' }
      sortByValue.value = 'nom-asc'
    }
  }
  currentPage.value = 1
  filterClients()
}

const handleSortChange = () => {
  if (!sortByValue.value) {
    sortBy.value = { field: null, order: 'asc' }
  } else {
    const [field, order] = sortByValue.value.split('-')
    if (statusFilter.value && field === 'statut') {
      sortBy.value = { field: 'nom', order: 'asc' }
      sortByValue.value = 'nom-asc'
    } else {
      sortBy.value = { field, order }
    }
  }
  filterClients()
}

watch(
  () => sortBy.value,
  (newSortBy) => {
    if (newSortBy.field) {
      sortByValue.value = `${newSortBy.field}-${newSortBy.order}`
    } else {
      sortByValue.value = ''
    }
  },
  { immediate: true },
)

watch(
  () => statusFilter.value,
  (newStatusFilter) => {
    if (newStatusFilter && sortBy.value.field === 'statut') {
      sortBy.value = { field: 'nom', order: 'asc' }
      sortByValue.value = 'nom-asc'
      filterClients()
    }
  },
)

watch(
  () => searchTerm.value,
  () => {
    currentPage.value = 1
    filterClients()
  },
)

watch(
  () => $q.screen.width,
  () => {
    if ($q.screen.gt.md) {
      showMobileFilters.value = false
    }
  },
)

const unlockOrLockAccount = async (clientId, isAccountLocked) => {
  var succeded
  if (isAccountLocked) {
    succeded = await store.unlockClientAsync(clientId)
  } else {
    succeded = await store.lockClientAsync(clientId)
  }

  //Si réussi , changer le type de verrou pour indiquer l'activité ou l'inactivité du compte
  if (succeded) {
    const clientUpdated = clients.value.find((c) => c.id === clientId)

    clientUpdated.isAccountLocked = !clientUpdated.isAccountLocked

    filterClients()
  }
}
</script>

<template>
  <div class="page">
    <div class="title-section">
      <div class="text-h5 text-bold">Clients</div>
    </div>

    <!-- Desktop - Barre de recherche -->
    <div v-if="$q.screen.gt.sm" class="search-section">
      <div class="row items-stretch no-wrap q-gutter-sm search-bar-container">
        <q-input
          v-model="searchTerm"
          placeholder="Rechercher un client..."
          outlined
          dense
          class="col search-input"
        />
      </div>
    </div>

    <!-- Desktop - Barre de filtres -->
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
          data-bs-toggle="dropdown"
          data-bs-auto-close="outside"
        >
          <span>{{ statusFilter || 'Statut' }}</span>
        </button>
        <div class="dropdown-menu p-2">
          <div class="dropdown-checkbox-item" @click="toggleStatusFilter('Actif')">
            <input type="checkbox" :checked="statusFilter === 'Actif'" />
            Actif
          </div>
          <div class="dropdown-checkbox-item" @click="toggleStatusFilter('Inactif')">
            <input type="checkbox" :checked="statusFilter === 'Inactif'" />
            Inactif
          </div>
        </div>
      </div>

      <span style="margin-left: 20px">Trier par :</span>
      <select
        v-model="sortByValue"
        class="form-select form-select-sm"
        style="width: auto"
        @change="handleSortChange"
      >
        <option value="">Rien</option>
        <option value="nom-asc">Nom (A-Z)</option>
        <option value="nom-desc">Nom (Z-A)</option>
        <option value="prenom-asc">Prénom (A-Z)</option>
        <option value="prenom-desc">Prénom (Z-A)</option>
        <option v-if="!statusFilter" value="statut-asc">Statut (Actif-Inactif)</option>
        <option v-if="!statusFilter" value="statut-desc">Statut (Inactif-Actif)</option>
      </select>
    </div>

    <!-- Mobile-->
    <div v-else class="mobile-filters-bar">
      <div class="mobile-search-container">
        <input
          type="search"
          v-model="searchTerm"
          placeholder="Rechercher un client..."
          class="mobile-search-input"
        />
        <button class="mobile-filters-btn" @click="showMobileFilters = !showMobileFilters">
          <span class="filter-icon">☰</span>
          <span class="filter-text">Filtres</span>
          <span v-if="statusFilter" class="filter-badge">1</span>
        </button>
      </div>

      <div v-if="statusFilter" class="mobile-clear-filters">
        <button class="clear-filters-btn" @click="toggleStatusFilter(null)">
          Effacer les filtres
        </button>
      </div>
    </div>

    <!-- Mobile - Sidebar filtres -->
    <div
      v-if="!$q.screen.gt.md"
      class="mobile-filters-sidebar"
      :class="{ open: showMobileFilters }"
    >
      <div class="sidebar-header">
        <h5>Filtres</h5>
        <button class="sidebar-close-btn" @click="showMobileFilters = false">✖</button>
      </div>
      <div class="sidebar-content">
        <hr />
        <!-- Statut -->
        <div class="filter-section">
          <p class="fw-bold">Statut</p>
          <div class="filter-options">
            <div
              class="sidebar-checkbox-item"
              :class="{ selected: !statusFilter }"
              @click="toggleStatusFilter(null)"
            >
              <input type="checkbox" :checked="!statusFilter" />
              (tous)
            </div>
            <div
              class="sidebar-checkbox-item"
              :class="{ selected: statusFilter === 'Actif' }"
              @click="toggleStatusFilter('Actif')"
            >
              <input type="checkbox" :checked="statusFilter === 'Actif'" />
              Actif
            </div>
            <div
              class="sidebar-checkbox-item"
              :class="{ selected: statusFilter === 'Inactif' }"
              @click="toggleStatusFilter('Inactif')"
            >
              <input type="checkbox" :checked="statusFilter === 'Inactif'" />
              Inactif
            </div>
          </div>
        </div>
        <hr />
        <!-- Trier par -->
        <div class="filter-section">
          <p class="fw-bold">Trier par</p>
          <select
            v-model="sortByValue"
            class="form-select form-select-sm filter-select"
            @change="handleSortChange"
          >
            <option value="">Rien</option>
            <option value="nom-asc">Nom (A-Z)</option>
            <option value="nom-desc">Nom (Z-A)</option>
            <option value="prenom-asc">Prénom (A-Z)</option>
            <option value="prenom-desc">Prénom (Z-A)</option>
            <option v-if="!statusFilter" value="statut-asc">Statut (Actif-Inactif)</option>
            <option v-if="!statusFilter" value="statut-desc">Statut (Inactif-Actif)</option>
          </select>
        </div>
        <div class="sidebar-footer">
          <q-btn
            label="Fermer"
            color="grey-7"
            unelevated
            @click="showMobileFilters = false"
            class="close-sidebar-btn"
          />
        </div>
      </div>
    </div>

    <!-- Mobile - pour fermer le sidebar -->
    <div
      v-if="!$q.screen.gt.md && showMobileFilters"
      class="mobile-filters-overlay"
      @click="showMobileFilters = false"
    ></div>

    <hr />

    <!-- Indicateur de chargement -->
    <div v-if="loading" class="text-center text-light py-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Chargement...</span>
      </div>
      <p class="mt-3">Chargement des clients...</p>
    </div>

    <!-- Desktop-->
    <div v-if="!loading && $q.screen.gt.md">
      <table>
        <thead>
          <tr>
            <th></th>
            <th>Nom</th>
            <th>Prénom</th>
            <th></th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="client in clients"
            :key="client.id"
            class="employe_section"
            :class="{ 'account-lock': client.isAccountLocked }"
          >
            <td>
              <q-icon name="person" class="profil" size="lg" />
            </td>
            <td>{{ client.name }}</td>
            <td>{{ client.firstName }}</td>
            <td>
              <q-icon
                @click="unlockOrLockAccount(client.id, client.isAccountLocked)"
                class="lock_employe"
                size="md"
                ><i
                  :class="{
                    'bi bi-unlock': !client.isAccountLocked,
                    'bi bi-lock': client.isAccountLocked,
                  }"
                  ><q-tooltip class="text-body2">{{
                    client.isAccountLocked
                      ? 'Compte bloqué. Cliquer pour le débloquer.'
                      : 'Compte actif. Cliquer pour le bloquer.'
                  }}</q-tooltip></i
                ></q-icon
              >
            </td>
            <td>
              <div class="row flex-row justify-content-end no-wrap">
                <q-btn
                  v-if="client.isAccountLocked"
                  label="Activer"
                  color="black"
                  class="btn-activer mr-3"
                  @click="unlockOrLockAccount(client.id, client.isAccountLocked)"
                />
                <q-btn
                  v-if="!client.isAccountLocked"
                  label="Désactiver"
                  color="black"
                  class="btn-desactiver mr-3"
                  @click="unlockOrLockAccount(client.id, client.isAccountLocked)"
                />
                <q-btn label="Gérer" class="btn-gerer mr-3" @click="gererClientClick(client.id)" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Mobile -->
    <div v-else-if="!loading && !$q.screen.gt.md">
      <div v-if="allClients.length === 0" class="mobile-empty-state">
        <div class="empty-message">Aucun client trouvé</div>
      </div>
      <div v-else class="mobile-clients-container">
        <div
          v-for="client in clients"
          :key="client.id"
          class="mobile-client-card"
          :class="{ 'account-lock': client.isAccountLocked }"
        >
          <div class="mobile-client-header">
            <q-icon name="person" class="mobile-client-icon" size="32px" />
            <div class="mobile-client-info">
              <div class="mobile-client-name">{{ client.name }} {{ client.firstName }}</div>
            </div>
            <q-icon
              @click.stop="unlockOrLockAccount(client.id, client.isAccountLocked)"
              class="mobile-lock-icon"
              size="24px"
            >
              <i
                :class="{
                  'bi bi-unlock': !client.isAccountLocked,
                  'bi bi-lock': client.isAccountLocked,
                }"
              ></i>
            </q-icon>
          </div>
          <div class="mobile-client-actions">
            <q-btn
              v-if="client.isAccountLocked"
              label="Activer"
              color="black"
              class="mobile-action-btn btn-activer"
              @click="unlockOrLockAccount(client.id, client.isAccountLocked)"
            />
            <q-btn
              v-else
              label="Désactiver"
              color="black"
              class="mobile-action-btn btn-desactiver"
              @click="unlockOrLockAccount(client.id, client.isAccountLocked)"
            />
            <q-btn
              label="Gérer"
              class="mobile-action-btn"
              color="brown-8"
              @click="gererClientClick(client.id)"
            />
          </div>
        </div>
      </div>
    </div>

    <p v-if="!loading && allClients.length === 0 && $q.screen.gt.md" class="manque-employe">
      Aucun client trouvé
    </p>

    <!-- Desktop - Pagination -->
    <div
      v-if="!loading && totalPages > 1 && $q.screen.gt.md"
      class="pagination-container d-flex justify-content-center mt-4 mb-4"
    >
      <nav class="custom-pagination">
        <ul class="pagination-list">
          <li class="pagination-item">
            <button
              class="pagination-btn prev-btn"
              :disabled="!hasPreviousPage"
              @click="goToPreviousPage"
            >
              <span class="btn-text">‹</span>
            </button>
          </li>

          <li v-if="currentPage > 3 && totalPages > 7" class="pagination-item">
            <button
              class="pagination-btn page-btn"
              :class="{ active: currentPage === 1 }"
              @click="goToPage(1)"
            >
              1
            </button>
          </li>
          <li v-if="currentPage > 3 && totalPages > 7" class="pagination-item">
            <span class="pagination-ellipsis">...</span>
          </li>

          <li v-for="page in getVisiblePages()" :key="page" class="pagination-item">
            <button
              class="pagination-btn page-btn"
              :class="{ active: currentPage === page }"
              @click="goToPage(page)"
            >
              {{ page }}
            </button>
          </li>

          <li v-if="currentPage < totalPages - 2 && totalPages > 7" class="pagination-item">
            <span class="pagination-ellipsis">...</span>
          </li>
          <li v-if="currentPage < totalPages - 2 && totalPages > 7" class="pagination-item">
            <button
              class="pagination-btn page-btn"
              :class="{ active: currentPage === totalPages }"
              @click="goToPage(totalPages)"
            >
              {{ totalPages }}
            </button>
          </li>

          <li class="pagination-item">
            <button class="pagination-btn next-btn" :disabled="!hasNextPage" @click="goToNextPage">
              <span class="btn-text">›</span>
            </button>
          </li>
        </ul>
      </nav>
    </div>

    <!-- Mobile  -->
    <div v-if="!loading && totalPages > 1 && !$q.screen.gt.md" class="mobile-pagination-container">
      <div class="mobile-pagination-info">Page {{ currentPage }} sur {{ totalPages }}</div>
      <div class="mobile-pagination-buttons">
        <button
          class="mobile-pagination-btn prev"
          :disabled="!hasPreviousPage"
          @click="goToPreviousPage"
        >
          ‹ Précédent
        </button>
        <button class="mobile-pagination-btn next" :disabled="!hasNextPage" @click="goToNextPage">
          Suivant ›
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page {
  background-color: #676767;
  padding-bottom: 100px;
  padding-top: 0;
  padding-left: 0;
  margin-left: 0;
  margin-right: 0;
  min-height: 600px;
}

.title-section {
  background-color: #959595;
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.title-section .text-h5 {
  color: white;
  margin: 0;
}

.filters-container {
  position: absolute;
  left: 0;
  top: 0;
}

.status-dropdown-position {
  position: relative;
}

.row.position-relative {
  min-height: 48px;
}

hr {
  border: none;
  height: 4px;
  background-color: rgb(255, 255, 255);
  margin-top: 0;
  margin-bottom: 16px;
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

tbody tr {
  background-color: #9c9a9a;
  color: #fff;
  border-top: 2px solid #ffffff;
  border-bottom: 2px solid #ffffff;
  transition:
    opacity 0.3s ease,
    background-color 0.3s ease;
}

body:not(.body--dark) tbody tr {
  background-color: #efebe9 !important;
  color: black !important;
}

.employe_section:hover {
  border-bottom: 2px solid #ffffff;
}

.profil {
  color: #89412b !important;
}

.lock_employe {
  color: #676767 !important;
  cursor: pointer !important;
  transition: all 0.3s ease !important;
}

.lock_employe:hover {
  color: #89412b !important;
  transform: scale(1.2) rotate(5deg);
  filter: drop-shadow(0 2px 4px rgba(137, 65, 43, 0.4));
}

.lock_employe:active {
  transform: scale(1.1) rotate(-5deg);
  transition: all 0.1s ease !important;
}

.btn-gerer {
  color: white !important;
  background: #5d4037 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.btn-activer {
  color: white !important;
  background: #4cae50 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.btn-desactiver {
  color: white !important;
  background: #d32f2f !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.manque-employe {
  font-size: 40px;
  text-align: center;
  color: white;
}

.account-lock {
  background-color: rgba(0, 0, 0, 0.3) !important;
  transition: background-color 0.4s ease;
}

.account-lock td {
  color: rgba(255, 255, 255, 0.9) !important;
}

.account-lock .lock_employe {
  color: #89412b !important;
  opacity: 1 !important;
}

.account-lock .btn-gerer {
  opacity: 1 !important;
  background: #6d4c41 !important;
}

.account-lock .profil {
  opacity: 1 !important;
}

.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 0.25em solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
  display: inline-block;
}

@keyframes spinner-border {
  to {
    transform: rotate(360deg);
  }
}

.text-light {
  color: #f8f9fa !important;
}

.dropdown-checkbox-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  cursor: pointer;
  user-select: none;
  transition: background-color 0.2s ease;
  border-radius: 4px;
}

.dropdown-checkbox-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.dropdown-checkbox-item input[type='checkbox'] {
  appearance: none;
  -webkit-appearance: none;
  width: 22px;
  height: 22px;
  background-color: #232a38;
  border: 2px solid #000;
  border-radius: 5px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  margin-right: 0;
  margin-top: 2px;
  flex-shrink: 0;
}

.dropdown-checkbox-item input[type='checkbox']:hover {
  background-color: #3b4456;
}

.dropdown-checkbox-item input[type='checkbox']:checked {
  background-color: #1e40af;
  border-color: #1e40af;
  animation: pop 0.2s ease forwards;
}

.dropdown-checkbox-item input[type='checkbox']:checked::after {
  content: '✓';
  color: #fff;
  font-size: 14px;
  position: absolute;
  top: 1px;
  left: 3px;
  transform: scale(0);
  animation: pop-check 0.2s ease forwards;
}

@keyframes pop {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.1);
  }
  100% {
    transform: scale(1);
  }
}

@keyframes pop-check {
  0% {
    transform: scale(0);
  }
  100% {
    transform: scale(1);
  }
}

.dropdown-menu {
  background-color: #2d3748 !important;
  color: #fff !important;
  border: 1px solid #fff !important;
  border-radius: 6px !important;
  min-width: 150px !important;
  padding: 0.5rem !important;
}

.dropdown-toggle.btn-info {
  background-color: #1e40af !important;
  border-color: #1e40af !important;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 2rem;
  margin-bottom: 2rem;
}

.custom-pagination {
  display: flex;
  justify-content: center;
}

.pagination-list {
  display: flex;
  list-style: none;
  padding: 0;
  margin: 0;
  gap: 5px;
}

.pagination-item {
  display: flex;
}

.pagination-btn {
  padding: 8px 12px;
  border: 1px solid #dee2e6;
  background: white;
  color: #4c4b4b;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: #6e7276;
  color: white;
}

.pagination-btn.active {
  background: #4c4b4b;
  color: white;
  border-color: #ffffff;
  box-shadow: 0 4px 12px rgba(227, 213, 208, 0.4);
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-ellipsis {
  padding: 8px 12px;
  color: #eaedef;
}

.search-section {
  margin-bottom: 16px;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: transparent !important;
}

.search-bar-container {
  width: 100%;
  max-width: 80%;
  background-color: transparent !important;
}

.search-input {
  background-color: transparent !important;
}

.search-input :deep(.q-field__control) {
  height: 40px;
  border-radius: 4px;
}

.search-input :deep(.q-field__control::before) {
  border: 1px solid #ced4da;
}

.search-input :deep(.q-field__control:hover::before) {
  border-color: #8d6e63;
}

.search-input :deep(.q-field__control::after) {
  border: 1px solid #8d6e63;
}

.filters-bar {
  padding: 12px 16px;
  background-color: rgb(35, 42, 56);
  margin-bottom: 16px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

/* ================== VERSION MOBILE ================== */
@media (max-width: 1023px) {
  .page {
    padding: 0 8px;
  }

  .title-section {
    width: fit-content;
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .title-section .text-h5 {
    margin: 0;
  }

  /* Mobile  */
  .mobile-filters-bar {
    padding: 12px 8px;
    background-color: rgb(35, 42, 56);
    margin-bottom: 12px;
  }

  .mobile-search-container {
    display: flex;
    gap: 8px;
    align-items: stretch;
  }

  .mobile-search-input {
    flex: 1;
    padding: 10px 16px;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-size: 14px;
    background-color: white;
    color: #333;
  }

  .mobile-search-input:focus {
    outline: none;
    border-color: #5d4037;
    box-shadow: 0 0 0 2px rgba(93, 64, 55, 0.2);
  }

  .mobile-filters-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 16px;
    background-color: transparent;
    border: 1px solid rgba(255, 255, 255, 0.5);
    color: white;
    font-size: 14px;
    font-weight: 500;
    border-radius: 4px;
    cursor: pointer;
    white-space: nowrap;
    position: relative;
    transition: all 0.2s;
  }

  .mobile-filters-btn:active {
    background-color: rgba(255, 255, 255, 0.1);
  }

  .mobile-filters-btn .filter-icon {
    font-size: 18px;
  }

  .mobile-filters-btn .filter-text {
    color: white;
  }

  .mobile-filters-btn .filter-badge {
    position: absolute;
    top: -8px;
    right: -8px;
    background-color: #ce0707;
    color: white;
    font-size: 11px;
    font-weight: bold;
    padding: 2px 6px;
    border-radius: 10px;
    min-width: 20px;
    text-align: center;
  }

  .mobile-clear-filters {
    margin-top: 12px;
  }

  .clear-filters-btn {
    padding: 8px 16px;
    background-color: transparent;
    border: 1px solid #ce0707;
    color: #ce0707;
    font-size: 13px;
    font-weight: 500;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
    width: 100%;
  }

  .clear-filters-btn:hover {
    background-color: rgba(206, 7, 7, 0.1);
  }

  .clear-filters-btn:active {
    background-color: rgba(206, 7, 7, 0.2);
  }

  /* Mobile - Sidebar filtres */
  .mobile-filters-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    z-index: 9998;
    transition: opacity 0.15s ease;
  }

  .mobile-filters-sidebar {
    background-color: #1f2735;
    position: fixed;
    top: 0;
    left: 0;
    width: 320px;
    height: 100vh;
    transform: translate3d(-100%, 0, 0);
    transition: transform 0.15s cubic-bezier(0.4, 0, 0.2, 1);
    z-index: 9999;
    font-family: 'Lucida Console', monospace;
    overflow-y: auto;
    overflow-x: hidden;
    scrollbar-width: thin;
    scrollbar-color: #888 transparent;
    padding: 1.5rem;
    box-shadow: 2px 0 10px rgba(0, 0, 0, 0.5);
    will-change: transform;
    backface-visibility: hidden;
    display: flex;
    flex-direction: column;
  }

  .mobile-filters-sidebar::-webkit-scrollbar {
    width: 10px;
  }

  .mobile-filters-sidebar::-webkit-scrollbar-thumb {
    background-color: #888;
    border-radius: 6px;
    border: 2px solid transparent;
  }

  .mobile-filters-sidebar::-webkit-scrollbar-thumb:hover {
    background-color: #555;
  }

  .mobile-filters-sidebar.open {
    transform: translate3d(0, 0, 0);
  }

  .mobile-filters-sidebar .sidebar-header {
    position: sticky;
    top: 0;
    background-color: #1f2735;
    z-index: 100;
    padding: 1rem 1.5rem;
    padding-bottom: 0.5rem;
    margin: -1.5rem -1.5rem 0 -1.5rem;
    margin-bottom: 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-shrink: 0;
  }

  .mobile-filters-sidebar .sidebar-header h5 {
    margin: 0;
    padding: 0.5rem 0;
    color: #f8f9fa;
  }

  .mobile-filters-sidebar .sidebar-close-btn {
    background: none;
    border: none;
    color: white;
    font-size: 1.5rem;
    cursor: pointer;
    padding: 0;
    line-height: 1;
  }

  .mobile-filters-sidebar .sidebar-content {
    padding: 0;
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
  }

  .mobile-filters-sidebar .filter-section {
    margin-bottom: 0.8rem;
  }

  .mobile-filters-sidebar .filter-section p.fw-bold {
    color: white;
    margin-bottom: 0.5rem;
  }

  .mobile-filters-sidebar .filter-options {
    display: flex;
    flex-direction: column;
    gap: 0;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item {
    display: flex;
    align-items: center;
    padding: 0.5rem;
    margin-bottom: 0;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s ease;
    user-select: none;
    position: relative;
    color: #f8f9fa;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item:hover {
    background-color: #3b4456;
    transform: translateX(4px);
  }

  .mobile-filters-sidebar .sidebar-checkbox-item:active {
    background-color: #4a5568;
    transform: translateX(2px);
  }

  .mobile-filters-sidebar .sidebar-checkbox-item::before {
    content: '';
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    width: 3px;
    background: linear-gradient(45deg, #ff6b35, #f7971e);
    border-radius: 0 2px 2px 0;
    opacity: 0;
    transition: opacity 0.2s ease;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item:hover::before {
    opacity: 1;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item.selected {
    background-color: rgba(255, 107, 53, 0.1);
    border-left: 3px solid #ff6b35;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item.selected::before {
    opacity: 1;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item input[type='checkbox'] {
    appearance: none;
    -webkit-appearance: none;
    width: 26px;
    height: 26px;
    background-color: #232a38;
    border: 2px solid #000;
    border-radius: 5px;
    cursor: pointer;
    position: relative;
    transition: all 0.2s ease;
    margin-right: 1rem;
    vertical-align: middle;
    transform: scale(1);
    pointer-events: none;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item input[type='checkbox']:hover {
    background-color: #3b4456;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item input[type='checkbox']:checked {
    background-color: #1e40af;
    border-color: #1e40af;
    animation: pop 0.2s ease forwards;
  }

  .mobile-filters-sidebar .sidebar-checkbox-item input[type='checkbox']:checked::after {
    content: '✓';
    color: #fff;
    font-size: 16px;
    position: absolute;
    top: 2px;
    left: 5px;
    transform: scale(0);
    animation: pop-check 0.2s ease forwards;
  }

  .mobile-filters-sidebar .filter-select {
    width: 100%;
    padding: 0.5rem;
    background-color: #2d3748;
    border: 1px solid #4a5568;
    color: #fff;
    font-size: 0.9rem;
    border-radius: 4px;
    margin-bottom: 0.5rem;
  }

  .mobile-filters-sidebar .filter-select:focus {
    border-color: #63b3ed;
    box-shadow: 0 0 0 0.2rem rgba(99, 179, 237, 0.25);
    outline: none;
  }

  .mobile-filters-sidebar .sidebar-footer {
    position: sticky;
    bottom: 0;
    background-color: #1f2735;
    z-index: 10;
    padding-top: 0.5rem;
    margin-top: 0;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .mobile-filters-sidebar .close-sidebar-btn {
    background-color: #e74c3c;
    color: white;
    border: none;
    border-radius: 6px;
    padding: 8px 16px;
    font-size: 1rem;
    cursor: pointer;
    transition: background-color 0.2s ease;
    width: 100%;
  }

  .mobile-filters-sidebar .close-sidebar-btn:hover {
    background-color: #c0392b;
  }

  /* Mobile - Cartes de clients */
  .mobile-clients-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 8px 0;
  }

  .mobile-client-card {
    background-color: #9c9a9a;
    border-radius: 8px;
    padding: 12px;
    border: 2px solid #ffffff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    transition: all 0.2s ease;
  }

  .mobile-client-card:hover {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  }

  .mobile-client-card.account-lock {
    background-color: rgba(0, 0, 0, 0.3);
    opacity: 0.8;
  }

  .mobile-client-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
  }

  .mobile-client-icon {
    color: #89412b !important;
    flex-shrink: 0;
  }

  .mobile-client-info {
    flex: 1;
    min-width: 0;
  }

  .mobile-client-name {
    font-size: 16px;
    font-weight: bold;
    color: #fff;
    word-wrap: break-word;
  }

  .mobile-lock-icon {
    color: #676767 !important;
    cursor: pointer !important;
    transition: all 0.3s ease !important;
    flex-shrink: 0;
  }

  .mobile-lock-icon:hover {
    color: #89412b !important;
    transform: scale(1.2) rotate(5deg);
  }

  .mobile-lock-icon:active {
    transform: scale(1.1) rotate(-5deg);
  }

  .mobile-client-actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
  }

  .mobile-action-btn {
    flex: 1;
    min-width: 100px;
    font-weight: 500;
  }

  .mobile-empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 16px;
    text-align: center;
  }

  .empty-message {
    font-size: 18px;
    color: white;
    margin-bottom: 24px;
  }

  /* Mobile */
  .mobile-pagination-container {
    padding: 16px 0;
    margin-top: 24px;
  }

  .mobile-pagination-info {
    text-align: center;
    color: white;
    font-size: 14px;
    margin-bottom: 12px;
  }

  .mobile-pagination-buttons {
    display: flex;
    gap: 8px;
    justify-content: center;
  }

  .mobile-pagination-btn {
    padding: 10px 16px;
    background-color: white;
    border: 1px solid #dee2e6;
    color: #4c4b4b;
    border-radius: 4px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    min-width: 120px;
  }

  .mobile-pagination-btn:hover:not(:disabled) {
    background-color: #6e7276;
    color: white;
  }

  .mobile-pagination-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}
</style>

<style>
body:not(.body--dark) .page {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .q-field__control {
  background-color: #efebe9 !important;
  border-radius: 4px !important;
}

body:not(.body--dark) .q-field__control:hover:before {
  border-color: transparent !important;
}

body:not(.body--dark) .q-field__control:before {
  border-color: transparent !important;
}

body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .filters-bar {
  background-color: #d1cbbb !important;
  color: black !important;
}

body:not(.body--dark) thead th {
  background-color: #a89c8d !important;
  color: black !important;
}

body:not(.body--dark) tbody tr {
  background-color: white !important;
  color: black !important;
  border: 1px solid #000 !important;
}

body:not(.body--dark) .employe_section:hover {
  background-color: #f5f5f5 !important;
  border-bottom: 1px solid #ccc !important;
}

body:not(.body--dark) .manque-employe {
  color: black !important;
}

body:not(.body--dark) .mobile-client-card {
  background-color: white !important;
  color: black !important;
  border: 1px solid #000 !important;
}

body:not(.body--dark) .mobile-client-header {
  background-color: #a89c8d !important;
  color: black !important;
  border-bottom: 1px solid #e0e0e0 !important;
}

body:not(.body--dark) .mobile-client-name,
body:not(.body--dark) .mobile-client-info {
  color: black !important;
}

body:not(.body--dark) .mobile-action-btn,
body:not(.body--dark) .mobile-action-btn * {
  color: white !important;
}

body:not(.body--dark) .custom-pagination .pagination-btn {
  background-color: white !important;
  color: black !important;
  border: 1px solid #ccc !important;
}

body:not(.body--dark) .custom-pagination .pagination-btn.active {
  background-color: #8d6e63 !important;
  color: white !important;
  border-color: #8d6e63 !important;
}

body:not(.body--dark) .account-lock {
  background-color: #e0e0e0 !important;
}

body:not(.body--dark) .account-lock td {
  color: #555 !important;
}

body:not(.body--dark) .account-lock:hover {
  background-color: #d0d0d0 !important;
}
</style>
