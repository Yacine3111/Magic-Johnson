<template>
  <div class="d-flex justify-content-center search-container custom-search-bg">
    <div class="input-group search-wrapper" :class="{ 'mobile-search': isMobile }">
      <button class="btn search-btn" type="button">
        <img src="https://cdn-icons-png.flaticon.com/512/109/109859.png" class="search-icon" />
      </button>
      <input
        type="search"
        :value="modelValue"
        @input="handleInput"
        @focus="handleFocus"
        @blur="handleBlur"
        :placeholder="placeholder"
        class="form-control search-input custom-search-input"
      />
      <!-- Dropdown des suggestions -->
      <div
        v-if="showSuggestions && suggestions.length > 0"
        class="search-suggestions"
        @mousedown.prevent
        @mouseenter="isMouseInSuggestions = true"
        @mouseleave="isMouseInSuggestions = false"
      >
        <div
          v-for="(suggestion, index) in suggestions"
          :key="index"
          class="suggestion-item"
          @mousedown="selectSuggestion(suggestion)"
        >
          {{ suggestion }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { searchSuggestionsService } from '@/services/searchSuggestionsService'

const props = defineProps({
  modelValue: {
    type: String,
    default: '',
  },
  placeholder: {
    type: String,
    default: 'Rechercher...',
  },
  isMobile: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:modelValue', 'search'])

const suggestions = ref([])
const showSuggestions = ref(false)
const isMouseInSuggestions = ref(false)
let blurTimeout = null

const handleInput = (event) => {
  const value = event.target.value
  emit('update:modelValue', value)

  if (value.length < 3) {
    suggestions.value = []
    showSuggestions.value = false
    return
  }

  searchSuggestionsService.loadSuggestionsWithDebounce(
    value,
    (results) => {
      suggestions.value = results
      showSuggestions.value = results.length > 0
    },
    300,
  )
}

const handleFocus = () => {
  if (suggestions.value.length > 0 && props.modelValue.length >= 3) {
    showSuggestions.value = true
  }
}

const handleBlur = (event) => {
  if (blurTimeout) clearTimeout(blurTimeout)

  blurTimeout = setTimeout(() => {
    if (isMouseInSuggestions.value) return

    showSuggestions.value = false
  }, 200)
}

const selectSuggestion = (suggestion) => {
  emit('update:modelValue', suggestion)
  suggestions.value = []
  showSuggestions.value = false
}
</script>

<style scoped>
.custom-search-bg {
  background: transparent !important;
  padding: 1rem 0;
}

.search-wrapper {
  position: relative;
  max-width: 600px;
  width: 100%;
}
.search-input {
  border-radius: 25px;
  padding-left: 45px;
}
.search-icon {
  width: 20px;
  height: 20px;
}
.search-btn {
  position: absolute;
  left: 5px;
  top: 50%;
  transform: translateY(-50%);
  z-index: 10;
  border: none;
  background: transparent;
}
.search-suggestions {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  z-index: 1000;
  max-height: 300px;
  overflow-y: auto;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
.suggestion-item {
  padding: 8px 12px;
  cursor: pointer;
  color: black;
}
.suggestion-item:hover {
  background-color: #f0f0f0;
}
</style>
