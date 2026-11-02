<template>
  <div class="dropdown">
    <button
      class="btn btn-sm dropdown-toggle d-flex justify-content-between align-items-center"
      :class="selected.length === 1 ? 'btn-info' : 'btn-secondary'"
      type="button"
      data-bs-toggle="dropdown"
      data-bs-auto-close="outside"
    >
      <span>
        {{ selected.length === 1 ? selected[0] : label }}
      </span>
      <span v-if="selected.length > 1" class="badge bg-primary ms-2">{{ selected.length }}</span>
    </button>
    <div class="dropdown-menu p-2">
      <template v-if="options.length >= 8 || forceSearch">
        <div class="dropdown-search">
          <input
            type="text"
            v-model="localSearch"
            :placeholder="`Rechercher ${label.toLowerCase()}...`"
            class="form-control form-control-sm"
          />
        </div>
        <div class="dropdown-list-container">
          <div
            v-for="opt in sortedAndFilteredOptions"
            :key="opt"
            class="dropdown-checkbox-item"
            @click="toggleOption(opt)"
          >
            <input type="checkbox" :checked="selected.includes(opt)" />
            {{ opt }}
          </div>
        </div>
      </template>

      <template v-else>
        <div
          v-for="opt in options"
          :key="opt"
          class="dropdown-checkbox-item"
          @click="toggleOption(opt)"
        >
          <input type="checkbox" :checked="selected.includes(opt)" />
          {{ opt }}
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  options: {
    type: Array,
    default: () => [],
  },
  selected: {
    type: Array,
    default: () => [],
  },
  forceSearch: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:selected', 'toggle'])

const localSearch = ref('')

const sortedAndFilteredOptions = computed(() => {
  let opts = props.options

  if (localSearch.value) {
    const search = localSearch.value.toLowerCase()
    opts = opts.filter((opt) => opt.toLowerCase().includes(search))
  }

  return [...opts].sort((a, b) => {
    const aSelected = props.selected.includes(a)
    const bSelected = props.selected.includes(b)

    if (aSelected && !bSelected) return -1
    if (!aSelected && bSelected) return 1
    return a.localeCompare(b)
  })
})

const toggleOption = (value) => {
  const newSelected = props.selected.includes(value)
    ? props.selected.filter((v) => v !== value)
    : [...props.selected, value]

  emit('update:selected', newSelected)
}
</script>

<style scoped>
.dropdown-menu {
  background-color: #2d3748;
  color: #fff;
  border: 1px solid #fff;
}

.dropdown-search {
  padding: 0.5rem;
  background-color: #2d3748;
}

.dropdown-search input {
  background-color: #1f2735;
  color: #fff;
  border: 1px solid #fff;
}

.dropdown-search input:focus {
  border-color: #1e40af;
  box-shadow: 0 0 0 0.2rem rgba(30, 64, 175, 0.25);
}

.dropdown-list-container {
  max-height: 200px;
  overflow-y: auto;
}

.dropdown-checkbox-item {
  display: flex;
  align-items: flex-start;
  cursor: pointer;
  transition: background-color 0.2s ease;
  border-radius: 4px;
  padding: 0.4rem 0.2rem;
  word-break: break-word;
  color: #fff;
}

.dropdown-checkbox-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.dropdown-checkbox-item input[type='checkbox'] {
  appearance: none;
  -webkit-appearance: none;
  width: 18px;
  height: 18px;
  background-color: #fff;
  border: 2px solid #2d3748;
  border-radius: 4px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  margin-right: 0.8rem;
  margin-top: 3px;
  flex-shrink: 0;
}

.dropdown-checkbox-item input[type='checkbox']:hover {
  background-color: #e2e8f0;
}

.dropdown-checkbox-item input[type='checkbox']:checked {
  background-color: #1e40af;
  border-color: #1e40af;
}

.dropdown-checkbox-item input[type='checkbox']:checked::after {
  content: '✓';
  color: #fff;
  font-size: 12px;
  position: absolute;
  top: 0;
  left: 2px;
  line-height: 16px;
}

:global(body.body--dark) .dropdown-menu {
  background-color: #1f2735;
  border-color: #2d3748;
}

:global(body.body--dark) .dropdown-search {
  background-color: #1f2735;
}

:global(body.body--dark) .dropdown-checkbox-item {
  color: #fff;
}

:global(body.body--dark) .dropdown-checkbox-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

:global(body.body--dark) .dropdown-checkbox-item input[type='checkbox'] {
  background-color: #2d3748;
  border-color: #4a5568;
}

:global(body.body--dark) .dropdown-checkbox-item input[type='checkbox']:checked {
  background-color: #63b3ed;
  border-color: #63b3ed;
}
</style>
