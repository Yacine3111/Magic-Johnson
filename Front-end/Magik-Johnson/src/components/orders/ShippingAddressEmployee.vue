<template>
  <div class="panel-section">
    <h3 style="font-size: x-large">Adresse de livraison :</h3>

    <!-- Sélection d'adresse enregistrée -->
    <div v-if="selectedClient && clientAddresses.length > 0" class="q-mb-md">
      <label class="label" style="margin-bottom: 8px; display: block">
        Sélectionner une adresse enregistrée :
      </label>
      <select
        v-model="selectedShippingAddressId"
        class="address-select"
        @change="onAddressSelected"
      >
        <option value="" disabled>Sélectionnez une adresse</option>
        <option v-for="address in clientAddresses" :key="address.id" :value="address.id">
          {{ formatAddressText(address) }}
        </option>
      </select>
    </div>
    <p
      v-else-if="selectedClient && clientAddresses.length === 0"
      class="text-center q-mt-xl text-h6"
    >
      <span class="text-weight-medium">L'adresse : </span>
      <span class="text-grey-6 text-italic text-h6"> Aucune adresse enregistrée </span>
    </p>
    <p
      v-else-if="selectedClient && clientAddresses.length > 0 && !useCheckout.shippingAddress"
      class="text-center q-mt-xl text-h6"
    >
      <span class="text-grey-6 text-italic text-h6">
        Veuillez sélectionner une adresse ci-dessus
      </span>
    </p>

    <div class="q-mt-md text-center">
      <q-btn
        label="Insérer adresse"
        color="primary"
        icon="add_circle"
        unelevated
        class="full-width"
        @click="ajouterAdresse"
        style="border-radius: 8px"
      />
    </div>

    <div
      v-if="useCheckout.shippingAddress && !selectedShippingAddressId"
      class="q-mt-md text-center animated fadeIn"
    >
      <p class="text-subtitle1 q-mb-xs">Adresse à utiliser :</p>
      <div class="row justify-center items-center no-wrap">
        <div class="bg-primary text-white q-pa-sm rounded-borders ellipsis" style="max-width: 80%">
          {{ useCheckout.shippingAddress }}
          <q-tooltip>{{ useCheckout.shippingAddress }}</q-tooltip>
        </div>

        <q-btn
          v-if="selectedClient && !isAddressAlreadySaved"
          round
          dense
          flat
          color="secondary"
          icon="save"
          class="q-ml-sm"
          style="display: flex; align-items: center; justify-content: center"
          @click="showSaveConfirmation = true"
        >
          <q-tooltip>Enregistrer cette adresse</q-tooltip>
        </q-btn>
      </div>
    </div>

    <AddressModal
      v-model="isAddressDialogOpen"
      :is-edit-mode="false"
      @save="handleSaveAddress"
      @cancel="isAddressDialogOpen = false"
    />

    <AddressConfirmationModal
      v-model="showSaveConfirmation"
      :client-name="selectedClient ? selectedClient.firstName + ' ' + selectedClient.name : ''"
      @confirm="saveAddressForClient"
      @cancel="showSaveConfirmation = false"
    />
  </div>
</template>

<script setup>
import AddressConfirmationModal from '../shared/AddressConfirmationModal.vue'
import AddressModal from '../shared/AddressModal.vue'
import { useCheckoutStore } from '@/stores/Checkout'
import { ref, onMounted, toRaw, watch, computed } from 'vue'
import {
  getAddress,
  getCustomerAddresses,
  AddAddress,
  AddAddressForClient,
} from '@/api/address/AddressApi'
import { useAuthStore } from '@/stores/authStore'
import { Notify, Loading } from 'quasar'

const props = defineProps({
  selectedClient: {
    type: Object,
    default: null,
  },
})

const useCheckout = useCheckoutStore()
const isAddressDialogOpen = ref(false)
const showSaveConfirmation = ref(false)
const savedAddressData = ref(null)

const useStoredAddress = ref(true)
const allAddress = ref([])
const clientAddresses = ref([])
const selectedShippingAddressId = ref()

const isAddressAlreadySaved = computed(() => {
  if (!useCheckout.shippingAddress) return false
  if (!clientAddresses.value || clientAddresses.value.length === 0) return false

  const current = useCheckout.shippingAddress.toLowerCase().trim()
  return clientAddresses.value.some(
    (addr) => formatAddressText(addr).toLowerCase().trim() === current,
  )
})

onMounted(async () => {
  allAddress.value = await getAddress()
})

watch(
  () => props.selectedClient,
  async (newClient) => {
    if (newClient && newClient.id) {
      try {
        let addresses = []
        const auth = useAuthStore()
        if (auth.user && auth.user.id === newClient.id) {
          addresses = await getAddress()
        } else {
          addresses = await getCustomerAddresses(newClient.id)
        }

        clientAddresses.value = addresses || []

        if (clientAddresses.value.length === 1) {
          selectedShippingAddressId.value = clientAddresses.value[0].id
          onAddressSelected()
        } else if (clientAddresses.value.length > 0) {
          selectedShippingAddressId.value = null
          useCheckout.setShippingAddress('')
        } else {
          clientAddresses.value = []
          selectedShippingAddressId.value = null
          useCheckout.setShippingAddress('')
        }
      } catch (error) {
        console.error('Error fetching addresses:', error)
        clientAddresses.value = []
        selectedShippingAddressId.value = null
        useCheckout.setShippingAddress('')
      }
    } else {
      clientAddresses.value = []
      selectedShippingAddressId.value = null
      useCheckout.setShippingAddress('')
    }
  },
  { immediate: true },
)

function formatAddressText(address) {
  if (!address) return ''
  const parts = []

  if (address.doorNb) parts.push(address.doorNb.toString())
  if (address.street) parts.push(address.street)
  if (address.city) parts.push(address.city)
  if (address.province) parts.push(address.province)
  if (address.postalCode) parts.push(address.postalCode)
  if (address.country) parts.push(address.country)

  return parts.join(', ')
}

function onAddressSelected() {
  const selectedAddress = clientAddresses.value.find(
    (a) => a.id === selectedShippingAddressId.value,
  )
  if (selectedAddress) {
    const formattedAddress = formatAddressText(selectedAddress)
    useCheckout.setShippingAddress(formattedAddress)
  }
}

const ajouterAdresse = () => {
  isAddressDialogOpen.value = true
}

watch(useStoredAddress, (newVal) => {
  if (!newVal) {
    isAddressDialogOpen.value = true
  }
})

function handleSaveAddress(data) {
  const addressText = formAddressString(data)
  useCheckout.setShippingAddress(addressText)
  selectedShippingAddressId.value = null

  savedAddressData.value = {
    doorNb: data.doorNb,
    street: data.street,
    city: data.city,
    postalCode: data.postalCode,
    state: data.state,
    country: data.country,
    unit: data.unit || '',
  }

  isAddressDialogOpen.value = false

  if (props.selectedClient && props.selectedClient.id) {
    showSaveConfirmation.value = true
  }
}

function formAddressString(data) {
  const parts = []
  if (data.doorNb) parts.push(data.doorNb.toString())
  if (data.street) parts.push(data.street)
  if (data.city) parts.push(data.city)
  if (data.state && data.state.name) parts.push(data.state.name)
  if (data.postalCode) parts.push(data.postalCode)
  if (data.country && data.country.name) parts.push(data.country.name)
  return parts.join(', ')
}

async function saveAddressForClient() {
  if (!savedAddressData.value) return

  try {
    Loading.show()

    const addressPayload = {
      addressNumber: savedAddressData.value.doorNb,
      street: savedAddressData.value.street,
      city: savedAddressData.value.city,
      postalCode: savedAddressData.value.postalCode,
      state: savedAddressData.value.state ? savedAddressData.value.state.id : null,
      country: savedAddressData.value.country ? savedAddressData.value.country.id : null,
      unit: savedAddressData.value.unit,
    }

    if (props.selectedClient && props.selectedClient.id) {
      await AddAddressForClient(props.selectedClient.id, addressPayload)
    } else {
      await AddAddress(addressPayload)
    }

    Notify.create({
      type: 'positive',
      message: 'Adresse enregistrée avec succès!',
    })

    const newAddresses = await getCustomerAddresses(props.selectedClient.id)
    clientAddresses.value = newAddresses || []

    if (newAddresses && newAddresses.length > 0) {
      const found = newAddresses.find(
        (a) =>
          a.postalCode === savedAddressData.value.postalCode &&
          a.doorNb == savedAddressData.value.doorNb,
      )
      if (found) {
        selectedShippingAddressId.value = found.id
        onAddressSelected()
      }
    }
  } catch (error) {
    console.error('Error saving address:', error)
    Notify.create({
      type: 'negative',
      message: "Erreur lors de l'enregistrement de l'adresse.",
    })
  } finally {
    Loading.hide()
    showSaveConfirmation.value = false
  }
}
</script>

<style scoped>
.countryErrorMessage,
.unitErrorMessage {
  color: red !important;
}

.address-dialog-card {
  width: 100%;
  max-width: 50%;
  background: #1f2837;
  overflow-x: hidden !important;
}

.address-dialog-card :deep(.q-card__section) {
  overflow-x: hidden !important;
  max-width: 100%;
}

.address-dialog-card :deep(.grid-2) {
  overflow-x: hidden;
  max-width: 100%;
}

@media (max-width: 1400px) {
  .address-dialog-card {
    max-width: 70%;
    margin: 0 auto;
  }
}

@media (max-width: 1024px) {
  .address-dialog-card {
    max-width: 85%;
    margin: 0 auto;
  }
}

@media (max-width: 768px) {
  .address-dialog-card {
    max-width: 90%;
    margin: 0 10px;
  }
}

@media (max-width: 600px) {
  .address-dialog-card {
    max-width: 100%;
    margin: 0 8px;
  }
}

.error {
  border: 2px solid red !important;
}

/* Layout */
.checkout-page {
  display: grid;
  grid-template-columns: 1fr 330px;
  gap: 18px;
  padding: 16px;
}

.card {
  background: #1f2837;
  border-radius: 16px;
  color: #e6ecff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  border: 1px solid #2b3550;
}

.panel {
  overflow: hidden;
}
.panel-header {
  background: #2e3a5a;
  padding: 14px 18px;
  font-weight: 700;
  letter-spacing: 0.5px;
  border-bottom: 1px solid #3a466d;
}
.panel-section {
  padding: 16px 18px;
  border-top: 1px solid #2b3550;
}
.panel-section h3 {
  margin: 0 0 10px;
  font-size: 18px;
  font-weight: 700;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 10px;
}
.field > span {
  font-size: 13px;
  color: #a9b6d1;
}
.field input {
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
}

.field select {
  background: white;
  border: 1px solid #506096;
  color: #3b4a71;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
  cursor: pointer;
}

.address-select {
  width: 100%;
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
  font-size: 14px;
  cursor: pointer;
}

.address-select:focus {
  border-color: #586aa0;
  box-shadow: 0 0 0 2px rgba(88, 106, 160, 0.2);
}

.address-select option {
  background: #2a344f;
  color: #e6ecff;
}

.field input::placeholder {
  color: #c6d1ef88;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  width: 100%;
  box-sizing: border-box;
}

.grid-2 .field {
  min-width: 0;
  box-sizing: border-box;
}

.grid-2 .field input,
.grid-2 .field select {
  width: 100%;
  box-sizing: border-box;
}
.grid-3 {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 10px;
}
.compact {
  grid-template-columns: 1fr 1fr;
}

.chip-group {
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
}
.chip {
  background: #3b4a71;
  color: #e6ecff;
  border: 1px solid #506096;
  padding: 8px 12px;
  border-radius: 999px;
  cursor: pointer;
  font-weight: 600;
}
.chip.active {
  background: #586aa0;
}

.cart-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.cart-item {
  display: grid;
  grid-template-columns: 60px 1fr auto auto 28px;
  gap: 10px;
  align-items: center;
  background: #2a344f;
  border-radius: 10px;
  padding: 8px;
  border: 1px solid #3e4c79;
}
.cart-item img {
  width: 60px;
  height: 60px;
  border-radius: 8px;
  object-fit: cover;
}
.cart-item .info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.muted {
  color: #a9b6d1;
}
.qty,
.price {
  font-size: medium;
  display: flex;
  align-items: center;
  gap: 8px;
}
.qty input {
  width: 80px;
  text-align: right;
  padding: 8px 10px;
  border-radius: 8px;
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
}
.icon.danger {
  background: transparent;
  border: none;
  color: #ffd2d2;
  font-size: 20px;
  cursor: pointer;
}

.summary {
  padding-bottom: 12px;
}
.totals {
  list-style: none;
  margin: 0;
  padding: 10px 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.totals li {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #cfe1ff;
}
.grand {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 10px 16px;
  padding: 12px;
  background: #1b2231;
  border: 1px solid #3a466d;
  border-radius: 10px;
  font-weight: 800;
  letter-spacing: 0.3px;
}
.pay {
  display: block;
  margin: 8px auto 12px;
  padding: 10px 18px;
  border-radius: 999px;
  background: #e9eefc;
  border: 0;
  color: #0e1a33;
  font-weight: 800;
  cursor: pointer;
}

@media (max-width: 1024px) {
  .checkout-page {
    grid-template-columns: 1fr;
  }
  .summary {
    order: -1;
  }
}

body:not(.body--dark) .address-dialog-card {
  background-color: #f8f5f0 !important;
  color: black !important;
}

body:not(.body--dark) .address-select {
  background: #ffffff !important;
  color: #000000 !important;
  border: 1px solid #ccc !important;
}

body:not(.body--dark) .address-select option {
  background: #ffffff !important;
  color: #000000 !important;
}

body:not(.body--dark) .address-dialog-card .text-h6 {
  color: black !important;
}

body:not(.body--dark) .address-dialog-card .field input,
body:not(.body--dark) .address-dialog-card .field select,
body:not(.body--dark) .address-dialog-card .address-select {
  background: #fff !important;
  color: black !important;
  border: 1px solid #cccccc !important;
}

body:not(.body--dark) .address-dialog-card .field span {
  color: black !important;
}
</style>
