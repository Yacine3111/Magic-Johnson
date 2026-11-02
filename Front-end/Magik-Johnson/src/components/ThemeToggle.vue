<template>
  <label class="theme-switch">
    <input
      type="checkbox"
      class="theme-switch__checkbox"
      :checked="modelValue"
      @change="$emit('update:modelValue', $event.target.checked)"
    />
    <div class="theme-switch__container">
      <div class="theme-switch__clouds"></div>
      <div class="theme-switch__stars-container">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="currentColor"
          width="100%"
          height="100%"
        >
          <path
            d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"
          />
        </svg>
      </div>
      <div class="theme-switch__circle-container">
        <div class="theme-switch__sun-moon-container">
          <div class="theme-switch__moon">
            <div class="theme-switch__spot"></div>
            <div class="theme-switch__spot"></div>
            <div class="theme-switch__spot"></div>
          </div>
        </div>
      </div>
    </div>
  </label>
</template>

<script setup>
defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
})
defineEmits(['update:modelValue'])
</script>

<style src="../css/ThemeToggle.css" scoped></style>
