<template>
  <div class="pagination-container d-flex justify-content-center mt-4 mb-4">
    <nav class="custom-pagination" v-if="totalPages > 1">
      <ul class="pagination-list">
        <!-- Bouton Précédent -->
        <li class="pagination-item">
          <button
            class="pagination-btn prev-btn"
            :disabled="currentPage === 1"
            @click="changePage(currentPage - 1)"
          >
            <span class="btn-text">‹</span>
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

        <!-- Points de suspension gauche -->
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

        <!-- Points de suspension droite -->
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
            <span class="btn-text">›</span>
          </button>
        </li>
      </ul>
    </nav>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  currentPage: {
    type: Number,
    required: true,
  },
  totalPages: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['page-change'])

const changePage = (page) => {
  if (page >= 1 && page <= props.totalPages) {
    emit('page-change', page)
  }
}

const showFirstPage = computed(() => props.currentPage > 3)
const showLastPage = computed(() => props.currentPage < props.totalPages - 2)
const showLeftEllipsis = computed(() => props.currentPage > 4)
const showRightEllipsis = computed(() => props.currentPage < props.totalPages - 3)

const visiblePages = computed(() => {
  const pages = []
  const start = Math.max(1, props.currentPage - 2)
  const end = Math.min(props.totalPages, props.currentPage + 2)

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})
</script>

<style>
.pagination-container {
  margin: 2rem 0;
  display: flex !important;
  justify-content: center;
}

.custom-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
}

.pagination-list {
  display: flex;
  align-items: center;
  gap: 8px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.pagination-item {
  display: flex;
  align-items: center;
}

.pagination-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 44px;
  height: 44px;
  padding: 8px 12px;
  background: #2d3748;
  color: #ffffff;
  border: 2px solid #4a5568;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
}

.pagination-btn:hover:not(:disabled) {
  background: #4a5568;
  border-color: #718096;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.pagination-btn:active:not(:disabled) {
  transform: translateY(0);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
}

.pagination-btn.active {
  background: #ff6b35;
  border-color: #ff6b35;
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(255, 107, 53, 0.4);
}

.pagination-btn.active:hover {
  background: #e85a2b;
  border-color: #e85a2b;
  transform: translateY(-2px);
}

.pagination-btn:disabled {
  background: #1a202c;
  border-color: #2d3748;
  color: #718096;
  cursor: not-allowed;
  opacity: 0.5;
}

.pagination-btn:disabled:hover {
  transform: none;
  box-shadow: none;
}

.prev-btn,
.next-btn {
  font-size: 20px;
  font-weight: bold;
}

.pagination-ellipsis {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 44px;
  height: 44px;
  color: #a0aec0;
  font-size: 18px;
  font-weight: bold;
  user-select: none;
}

.page-btn {
  position: relative;
  overflow: hidden;
}

.page-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s ease;
}

.page-btn:hover::before:not(.active) {
  left: 100%;
}

@media (max-width: 768px) {
  .pagination-btn {
    min-width: 36px;
    height: 36px;
    padding: 6px 8px;
    font-size: 14px;
  }

  .pagination-ellipsis {
    min-width: 36px;
    height: 36px;
    font-size: 16px;
  }

  .pagination-list {
    gap: 4px;
  }
}
</style>
