<template>
  <div class="promotions-section">
    <div class="promotions-wrapper">
      <div class="table-container" :class="{ 'fixed-height': totalPages > 1 }">
        <div v-if="loading" class="text-center q-pa-lg">
          <q-spinner-dots size="40px" color="primary" />
          <div class="q-mt-sm">Chargement des promotions...</div>
        </div>

        <q-table
          v-else
          :rows="paginatedPromotions"
          :columns="[
            {
              name: 'selection',
              label: '',
              field: 'selection',
              align: 'center',
              style: 'width: 50px',
            },
            {
              name: 'nom',
              label: 'Nom promotion',
              field: 'nom',
              align: 'left',
              style: 'width: 22%',
              classes: 'wrap-text-promo',
            },
            {
              name: 'debut',
              label: 'Date début',
              field: 'debut',
              align: 'center',
              style: 'width: 15%',
            },
            {
              name: 'fin',
              label: 'Date expiration',
              field: 'fin',
              align: 'center',
              style: 'width: 15%',
            },
            {
              name: 'statut',
              label: 'Statut',
              field: 'statut',
              align: 'center',
              style: 'width: 10%',
            },
            {
              name: 'rabais',
              label: 'Rabais',
              field: 'rabais',
              align: 'center',
              style: 'width: 10%',
            },
            {
              name: 'actions',
              label: 'Actions',
              field: 'actions',
              align: 'center',
              style: 'width: 10%',
            },
          ]"
          row-key="id"
          flat
          bordered
          dense
          class="rounded-borders promotions-table"
          hide-bottom
        >
          <template #body-cell-selection="props">
            <q-td
              :props="props"
              class="selection-cell"
              @click="$emit('toggle-selection', props.row)"
            >
              <q-icon
                :name="
                  selectedPromotionForCards?.id === props.row.id
                    ? 'check_circle'
                    : 'radio_button_unchecked'
                "
                :color="selectedPromotionForCards?.id === props.row.id ? 'green' : 'grey-5'"
                size="md"
                class="selection-icon"
              />
            </q-td>
          </template>
          <template #body-cell-statut="props">
            <q-td :props="props">
              <q-badge
                :color="
                  getDisplayStatus(props.row) === 'Active'
                    ? 'green'
                    : getDisplayStatus(props.row) === 'En attente'
                      ? 'orange'
                      : getDisplayStatus(props.row) === 'Expirée'
                        ? 'red'
                        : 'grey'
                "
              >
                {{ getDisplayStatus(props.row) }}
              </q-badge>
            </q-td>
          </template>
          <template #body-cell-rabais="props">
            <q-td :props="props"> - {{ props.row.rabais }}% </q-td>
          </template>
          <template #body-cell-actions="props">
            <q-td :props="props" class="actions-cell">
              <q-btn
                color="brown-8"
                label="Modifier"
                size="sm"
                class="text-white action-btn"
                style="background-color: #424242 !important"
                @click="$emit('edit', props.row)"
              />
              <q-btn
                color="red-8"
                label="Supprimer"
                size="sm"
                class="text-white action-btn"
                @click="$emit('delete', props.row)"
              />
            </q-td>
          </template>

          <!-- Template pour les lignes vides -->
          <template v-if="totalPages > 1 && paginatedPromotions.length < 5" #bottom-row>
            <q-tr v-for="i in 5 - paginatedPromotions.length" :key="`empty-${i}`">
              <q-td v-for="col in 8" :key="`empty-${col}`" class="empty-cell" />
            </q-tr>
          </template>
        </q-table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination-container">
        <nav class="custom-pagination">
          <ul class="pagination-list">
            <li class="pagination-item">
              <button
                class="pagination-btn prev-btn"
                :disabled="!hasPreviousPage"
                @click="$emit('change-page', currentPage - 1)"
              >
                <q-icon name="chevron_left" size="sm" />
              </button>
            </li>
            <li v-for="page in getVisiblePages()" :key="page" class="pagination-item">
              <button
                class="pagination-btn page-btn"
                :class="{ active: currentPage === page }"
                @click="$emit('change-page', page)"
              >
                {{ page }}
              </button>
            </li>
            <li class="pagination-item">
              <button
                class="pagination-btn next-btn"
                :disabled="!hasNextPage"
                @click="$emit('change-page', currentPage + 1)"
              >
                <q-icon name="chevron_right" size="sm" />
              </button>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  paginatedPromotions: Array,
  currentPage: Number,
  totalPages: Number,
  hasNextPage: Boolean,
  hasPreviousPage: Boolean,
  selectedPromotionForCards: Object,
  editingPromotion: Object,
  loading: Boolean,
})

defineEmits(['edit', 'delete', 'toggle-selection', 'change-page'])

const getVisiblePages = () => {
  const pages = []
  const start = Math.max(1, props.currentPage - 2)
  const end = Math.min(props.totalPages, props.currentPage + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
}

const getDisplayStatus = (promo) => {
  if (promo.statut === 'Actif') return 'Active'
  if (promo.statut === 'En Attente') return 'En attente'
  if (promo.statut === 'Inactif') return 'Inactive'

  const now = new Date()
  if (promo.fin && new Date(promo.fin) < now) {
    return 'Expirée'
  }

  return 'Inactive'
}
</script>

<style scoped>
.promotions-section {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  margin-bottom: 16px;
}

.promotions-wrapper {
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.table-container {
  flex-grow: 1;
  overflow-y: auto;
  margin-bottom: 16px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* Hauteur fixe uniquement lorsqu'il y a pagination */
.table-container.fixed-height {
  height: 406px;
}

.promotions-table {
  width: 100%;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* Light Mode Styles */
body:not(.body--dark) .promotions-table {
  border: 2px solid #d1c0a8;
  background-color: white;
}

body:not(.body--dark) .promotions-table :deep(.q-table thead tr) {
  background-color: #676767;
}

body:not(.body--dark) .promotions-table :deep(.q-table th) {
  color: #ffffff;
  border-bottom: 2px solid #d1c0a8;
}

body:not(.body--dark) .promotions-table :deep(.q-table tbody tr) {
  background-color: #efebe9 !important;
  color: #000 !important;
}

body:not(.body--dark) .promotions-table :deep(.q-table tbody tr:hover) {
  background-color: #f5f0e8;
}

body:not(.body--dark) .promotions-table :deep(.q-table tbody tr:hover td) {
  background-color: #f5f0e8 !important;
}

body:not(.body--dark) .promotions-table :deep(.q-table td) {
  border-bottom: 1px solid #d1c0a8;
}

/* Dark Mode Styles */
body.body--dark .promotions-table {
  border: 2px solid #4a5568;
  background-color: #1a202c;
}

body.body--dark .promotions-table :deep(.q-table thead tr) {
  background-color: #2d3748;
}

body.body--dark .promotions-table :deep(.q-table th) {
  color: #e2e8f0;
  border-bottom: 2px solid #4a5568;
}

body.body--dark .promotions-table :deep(.q-table tbody tr) {
  background-color: #1a202c;
  color: #e2e8f0;
}

body.body--dark .promotions-table :deep(.q-table tbody tr:hover) {
  background-color: #2d3748;
}

body.body--dark .promotions-table :deep(.q-table tbody tr:hover td) {
  background-color: #2d3748 !important;
}

body.body--dark .promotions-table :deep(.q-table td) {
  border-bottom: 1px solid #4a5568;
}

/*Styles commun */
.promotions-table :deep(.q-table thead tr) {
  position: sticky;
  top: 0;
  z-index: 10;
}

.promotions-table :deep(.q-table th) {
  font-weight: bold;
}

.promotions-table :deep(.q-table tbody tr) {
  min-height: 60px;
}

.promotions-table :deep(.q-table td) {
  vertical-align: middle !important;
  height: 70px;
}

.promotions-table :deep(.q-table td.wrap-text-promo) {
  max-width: 180px;
  word-wrap: break-word;
  overflow-wrap: break-word;
  white-space: normal;
  line-height: 1.2;
  padding-top: 8px;
  padding-bottom: 8px;
}

/* Style pour les cellules vides */
.empty-cell {
  height: 70px;
  border-bottom: 1px solid #d1c0a8;
}

.selection-cell {
  display: flex !important;
  justify-content: center !important;
  align-items: center !important;
  text-align: center !important;
  border: none !important;

  width: 36px !important;
  min-width: 36px !important;
  cursor: pointer;
  min-height: 60px !important;
}

.promotions-table :deep(.q-table tbody td.selection-cell) {
  width: 100% !important;
  min-width: 36px !important;
}

.selection-icon {
  display: block !important;
  margin: 0 !important;
  text-align: center;
  position: relative;
  left: 0;
  font-size: 24px;
}

.action-btn {
  margin: 0 2px;
  min-width: 80px;
  font-weight: 500;
  white-space: nowrap;
}

.page-info {
  color: #333;
  font-weight: 500;
  font-size: 14px;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 16px;
  flex-shrink: 0;
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
</style>
