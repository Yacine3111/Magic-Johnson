<script setup lang="ts">
import Order from '@/components/orders/Order.vue'
import { useAuthStore } from '@/stores/authStore'
import { useCheckoutStore } from '@/stores/Checkout'
import { onMounted, ref } from 'vue'
import { onBeforeMount } from 'vue'
import router from '@/router'

const auth = useAuthStore()
const useCheckout = useCheckoutStore()

const isClient = ref(false)
const isEmploye = ref(false)
const isLoading = ref(true)

onBeforeMount(async () => {
  try {
    if (!auth.role) {
      await auth.fetchRole()
    }

    isClient.value = auth.isUserClient || false
    isEmploye.value = auth.isUserEmployee || false

    if (!isClient.value && !isEmploye.value) {
      router.push('/connexion')
      return
    }
  } catch (error) {
    router.push('/connexion')
  } finally {
    isLoading.value = false
  }
})

onMounted(() => {
  useCheckout.setShippingAddress('')
})
</script>

<template>
  <div class="page-commande">
    <div v-if="isLoading" class="loading-container">
      <q-spinner color="primary" size="3em" />
      <p>Chargement...</p>
    </div>
    <Order v-else></Order>
  </div>
</template>

<style scoped>
.page-commande {
  margin-top: 10px;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  gap: 20px;
}

.loading-container p {
  color: #666;
  font-size: 1.1rem;
}
</style>
