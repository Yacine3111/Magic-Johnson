<template>
  <div class="panel-section">
    <h3 style="font-size: x-large">Adresse de livraison :</h3>

    <!-- Sélection d'adresse enregistrée -->
    <div v-if="allAddress && allAddress.length > 0" class="q-mb-md">
      <label class="label" style="margin-bottom: 8px; display: block">
        Sélectionner une adresse enregistrée :
      </label>
      <select
        v-model="selectedShippingAddressId"
        class="address-select"
        @change="onAddressSelected"
      >
        <option value="" disabled>Sélectionnez une adresse</option>
        <option v-for="address in allAddress" :key="address.id" :value="address.id">
          {{ formatAddressText(address) }}
        </option>
      </select>
    </div>
    <p v-else-if="allAddress && allAddress.length === 0" class="text-center q-mt-xl text-h6">
      <span class="text-weight-medium">L'adresse : </span>
      <span class="text-grey-6 text-italic text-h6"> Aucune adresse enregistrée </span>
    </p>

    <div class="q-mt-md text-center">
      <q-btn
        :label="buttonLabel"
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
          v-if="!isAddressAlreadySaved"
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

    <q-dialog
      v-model="isAddressDialogOpen"
      persistent
      transition-show="scale"
      transition-hide="scale"
    >
      <q-card class="address-dialog-card">
        <q-card-section class="text-h6">Nouvelle adresse de livraison</q-card-section>

        <q-card-section>
          <div class="grid-2">
            <label class="field">
              <span>Numéro civique :</span>
              <q-input
                v-model="doorNb"
                mask="########"
                placeholder="1234"
                outlined
                dense
                hide-bottom-space
                :error="!!doorNbErrorMessage"
                :error-message="doorNbErrorMessage"
              />
            </label>

            <label class="field">
              <span>Rue :</span>
              <q-input
                v-model.number="street"
                placeholder="Rue Principale"
                outlined
                dense
                hide-bottom-space
                :error="streetError"
                :error-message="streetErrorMessage"
                @blur="streetVerified"
              />
            </label>

            <label class="field">
              <span>Ville :</span>
              <q-input
                v-model="city"
                placeholder="Granby"
                outlined
                dense
                hide-bottom-space
                :error="cityError"
                :error-message="cityErrorMessage"
                @blur="cityVerified"
              />
            </label>

            <label class="field">
              <span>Unité :</span>
              <q-input
                v-model="unit"
                mask="########"
                placeholder="Apt 2"
                outlined
                dense
                hide-bottom-space
              />
            </label>

            <label class="field">
              <span>Pays :</span>
              <q-input
                v-model="countrySelected.name"
                outlined
                dense
                readonly
                disable
                bg-color="grey-2"
                hide-bottom-space
              />
            </label>

            <label class="field">
              <span>Code postal :</span>
              <q-input
                v-model="postalCode"
                placeholder="J2G 5C7"
                mask="A#A #A#"
                outlined
                dense
                hide-bottom-space
                :error="postalCodeError"
                :error-message="postalCodeErrorMessage"
                @blur="postalCodeVerified"
              />
            </label>

            <label class="field">
              <span> Province :</span>
              <q-select
                v-model="state"
                :options="states"
                option-value="name"
                option-label="name"
                emit-value
                map-options
                outlined
                dense
                class="bg-blue-grey-1"
                :error="stateError"
                :error-message="stateErrorMessage"
                hide-bottom-space
              />
            </label>
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn label="Annuler" color="negative" @click="AnnulerTapingAddress" />
          <q-btn label="Confirmer" color="primary" @click="confirmShippingAddress" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <AddressConfirmationModal
      v-model="showSaveConfirmation"
      @confirm="handleConfirmSave"
      @cancel="showSaveConfirmation = false"
    >
      Souhaitez-vous enregistrer cette adresse pour vos prochaines commandes ?
    </AddressConfirmationModal>
  </div>
</template>

<script setup lang="ts">
import { useCheckoutStore } from '@/stores/Checkout'
import { ref, onMounted, watch, computed, toRaw } from 'vue'
import { AddAddress, getAddress, getCanada } from '@/api/address/AddressApi'
import { useAuthStore } from '@/stores/authStore'
import { Loading, useQuasar } from 'quasar'
import AddressConfirmationModal from '../shared/AddressConfirmationModal.vue'
import { checkEmptyFieldAddress } from '@/validators/emptyFieldsValidator'
import {
  ValidateCity,
  ValidatePostalCode,
  ValidateState,
  ValidateStreet,
} from '@/validators/addressValidator'

const $q = useQuasar()

const useCheckout = useCheckoutStore()
const auth = useAuthStore()

const isAddressDialogOpen = ref(false)
const showSaveConfirmation = ref(false)
const savedAddressData = ref(null)
const useStoredAddress = ref(true)
const allAddress = ref<any[]>([])
const selectedShippingAddressId = ref()

const doorNb = ref('')
const unit = ref('')
const street = ref('')
const city = ref('')
const postalCode = ref('')
const state = ref('')
const country = ref()

const streetError = ref(false)
const cityError = ref(false)
const postalCodeError = ref(false)
const stateError = ref(false)

const doorNbErrorMessage = ref('')
const streetErrorMessage = ref('')
const cityErrorMessage = ref('')
const postalCodeErrorMessage = ref('')
const stateErrorMessage = ref('')
const countryErrorMessage = ref('')

const countrySelected = ref<any>({})
const states = ref([])

onMounted(async () => {
  allAddress.value = await getAddress()
  countrySelected.value = await getCanada()
  states.value = toRaw(countrySelected.value.provinces)
})

watch(
  () => [selectedShippingAddressId.value, isAddressDialogOpen.value],
  () => {
    if (isAddressDialogOpen.value === true) {
      return
    }

    if (selectedShippingAddressId.value === undefined || selectedShippingAddressId.value === null) {
      return
    }
    const shippingAddressText = useStoredAddress.value
      ? getSelectedAddressText()
      : useCheckout.shippingAddress

    if (useStoredAddress.value) {
      useCheckout.setShippingAddress(shippingAddressText)
    }
  },
  { immediate: true },
)

const isAddressAlreadySaved = computed(() => {
  if (!useCheckout.shippingAddress) return false
  if (!allAddress.value || allAddress.value.length === 0) return false

  const current = useCheckout.shippingAddress.toLowerCase().trim()
  return allAddress.value.some((addr) => formatAddressText(addr).toLowerCase().trim() === current)
})

function formatAddressText(address) {
  if (!address) return ''
  const parts = []

  if (address.doorNb) parts.push(address.doorNb.toString())
  if (address.street) parts.push(address.street)
  if (address.unit) parts.push(address.unit)
  if (address.city) parts.push(address.city)
  if (address.province) parts.push(address.province)
  if (address.postalCode) parts.push(address.postalCode)
  if (address.country) parts.push(address.country)

  return parts.join(', ')
}

function getSelectedAddressText() {
  const selected = allAddress.value.find((a) => a.id === selectedShippingAddressId.value)
  return selected
    ? `${selected.doorNb} ${selected.street}, ${selected.city}, ${selected.province}, ${selected.postalCode}, ${selected.country}`
    : ''
}

function onAddressSelected() {
  const selectedAddress = allAddress.value.find((a) => a.id === selectedShippingAddressId.value)
  if (selectedAddress) {
    const formattedAddress = formatAddressText(selectedAddress)
    useCheckout.setShippingAddress(formattedAddress)
  }
}

const buttonLabel = computed(() => {
  if (useCheckout.shippingAddress && !selectedShippingAddressId.value && savedAddressData.value) {
    return "Modifier l'adresse"
  }
  return 'Insérer adresse'
})

const ajouterAdresse = () => {
  if (useCheckout.shippingAddress && !selectedShippingAddressId.value && savedAddressData.value) {
    const data = savedAddressData.value
    doorNb.value = data.doorNb
    street.value = data.street
    city.value = data.city
    postalCode.value = data.postalCode
    unit.value = data.unit || ''
    state.value = data.state.name
  } else {
    // Reset form
    doorNb.value = ''
    unit.value = ''
    street.value = ''
    city.value = ''
    postalCode.value = ''
    state.value = ''
    country.value = null
  }

  // Réinitialiser les erreurs
  doorNbErrorMessage.value = ''
  streetErrorMessage.value = ''
  cityErrorMessage.value = ''
  postalCodeErrorMessage.value = ''
  stateErrorMessage.value = ''
  countryErrorMessage.value = ''

  streetError.value = false
  cityError.value = false
  postalCodeError.value = false
  stateError.value = false

  isAddressDialogOpen.value = true
}

function AnnulerTapingAddress() {
  doorNb.value = ''
  unit.value = ''
  street.value = ''
  city.value = ''
  postalCode.value = ''
  state.value = ''
  country.value = null
  isAddressDialogOpen.value = false
}

async function confirmShippingAddress() {
  if (!validateInputs()) return

  selectedShippingAddressId.value = null

  const formatted = getManualAddressText()
  useCheckout.setShippingAddress(formatted)

  isAddressDialogOpen.value = false

  const selectedStateObj = states.value.find((s) => s.name === state.value)
  const stateId = selectedStateObj ? selectedStateObj.id : null
  const countryId =
    countrySelected.value && countrySelected.value.id
      ? countrySelected.value.id
      : countrySelected.value
        ? countrySelected.value.id
        : null

  const cName = countrySelected.value ? countrySelected.value.name : 'Canada'

  savedAddressData.value = {
    doorNb: doorNb.value,
    street: street.value,
    city: city.value,
    postalCode: postalCode.value,
    state: { id: stateId, name: state.value },
    country: { id: countryId, name: cName },
    unit: unit.value,
  }

  if (!isAddressAlreadySaved.value) {
    showSaveConfirmation.value = true
  }
}

function getManualAddressText() {
  const parts = []
  const streetPart = []
  if (doorNb.value) streetPart.push(doorNb.value.toString().trim())
  if (street.value) streetPart.push(street.value.trim())
  if (streetPart.length > 0) parts.push(streetPart.join(' '))

  if (city.value && city.value.trim()) parts.push(city.value.trim())
  if (state.value && state.value.trim()) parts.push(state.value.trim())
  if (postalCode.value && postalCode.value.trim()) parts.push(postalCode.value.trim())
  if (countrySelected.value && countrySelected.value.name) parts.push(countrySelected.value.name)

  return parts.join(', ')
}

function validateInputs() {
  return (
    checkEmptyFieldAddress(
      doorNb,
      doorNbErrorMessage,
      street,
      streetErrorMessage,
      city,
      cityErrorMessage,
      postalCode,
      postalCodeErrorMessage,
      country,
      countryErrorMessage,
      state,
      stateErrorMessage,
    ) &&
    streetVerified() &&
    cityVerified() &&
    postalCodeVerified()
  )
}

const streetVerified = () => {
  return ValidateStreet(street, streetError, streetErrorMessage)
}

const cityVerified = () => {
  return ValidateCity(city, cityError, cityErrorMessage)
}

const postalCodeVerified = () => {
  return ValidatePostalCode(postalCode, postalCodeError, postalCodeErrorMessage)
}

async function handleConfirmSave() {
  if (!savedAddressData.value) return
  await SaveAddress(savedAddressData.value)
  allAddress.value = await getAddress()

  if (allAddress.value && allAddress.value.length > 0) {
    const found = allAddress.value.find(
      (a) =>
        a.postalCode === savedAddressData.value.postalCode &&
        a.doorNb == savedAddressData.value.doorNb,
    )
    if (found) {
      selectedShippingAddressId.value = found.id
      onAddressSelected()
    }
  }

  $q.notify({
    type: 'positive',
    message: 'Adresse enregistrée avec succès',
    position: 'top',
    timeout: 2500,
    classes: 'text-body1 bg-primary text-white shadow-2',
  })

  showSaveConfirmation.value = false
}

async function SaveAddress(data) {
  const addressPayload = {
    addressNumber: data.doorNb,
    street: data.street,
    city: data.city,
    postalCode: data.postalCode,
    state: data.state.id,
    country: data.country.id,
    unit: data.unit,
  }

  try {
    Loading.show({ message: 'Enregistrement...', spinnerColor: 'primary' })
    await AddAddress(addressPayload)
  } catch (e) {
    throw e
  } finally {
    Loading.hide()
  }
}
</script>

<style>
.center-buttons-dialog .q-card__actions {
  justify-content: center !important;
}
</style>

<style scoped>
/* Layout */
.checkout-page {
  display: grid;
  grid-template-columns: 1fr 330px;
  gap: 18px;
  padding: 16px;
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

.address-selector {
  margin-top: 1rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  color: #a9b6d1;
}

.form-select {
  background: #3b4a71;
  border: 1px solid #506096;
  color: #fff;
  padding: 10px 12px;
  border-radius: 8px;
  outline: none;
  width: 100%;
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
.field input::placeholder {
  color: #c6d1ef88;
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

.addressNumberErrorMessage,
.streetErrorMessage,
.cityErrorMessage,
.postalCodeErrorMessage,
.stateErrorMessage,
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

/* Light Mode Support */
body:not(.body--dark) .form-select,
body:not(.body--dark) .address-select {
  background: #ffffff !important;
  color: #000000 !important;
  border: 1px solid #ccc !important;
}

body:not(.body--dark) .address-select option {
  background: #ffffff !important;
  color: #000000 !important;
}

body:not(.body--dark) .form-label {
  color: #333 !important;
}

body:not(.body--dark) h3 {
  color: #000 !important;
}

body:not(.body--dark) .panel-section {
  color: #000 !important;
}

body:not(.body--dark) .address-dialog-card {
  background-color: #f8f5f0 !important;
  color: black !important;
  border: 1px solid #ccc;
}

body:not(.body--dark) .address-dialog-card .field input,
body:not(.body--dark) .address-dialog-card .field select,
body:not(.body--dark) .address-dialog-card .address-select {
  background: #fff !important;
  color: black !important;
  border: 1px solid #cccccc !important;
}

body:not(.body--dark) .address-dialog-card .field > span {
  color: #333 !important;
}
</style>
