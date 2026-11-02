<script setup>
import '@/assets/gestionCompte.css'
import {
  validateName,
  validateFirstName,
  validateEmail,
  validatePhoneNumber,
} from '@/validators/registerValidator'
import { ref, onMounted, computed, watch } from 'vue'
import { useClientStore } from '@/stores/clientStore'
import { useAuthStore } from '@/stores/authStore'
import { storeToRefs } from 'pinia'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { useRoute, useRouter } from 'vue-router'
import AddressModal from '@/components/Shared/AddressModal.vue'

const route = useRoute()

const isDisabled = ref(true)
const isEditMode = ref(true)

const firstname = ref('')
const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const role = ref('Client')

const originalFirstname = ref('')
const originalName = ref('')
const originalEmail = ref('')
const originalPhoneNumber = ref('')
const isAccountLocked = ref(false)
const clientId = ref('')

const firstnameError = ref(false)
const nameError = ref(false)
const phoneNumberError = ref(false)
const emailError = ref(false)

const firstnameErrorMessage = ref('')
const nameErrorMessage = ref('')
const emailErrorMessage = ref('')
const phoneNumberErrorMessage = ref('')

const router = useRouter()
const store = useClientStore()

const loadClientData = async (userId) => {
  await store.getSelectedClientAsync(userId)
  await store.getClientAddressesAsync(userId)
}

const loadData = async (id) => {
  if (!id) {
    router.push('/')
    return
  }
  clientId.value = id
  await loadClientData(id)

  if (store.clientSelected) {
    firstname.value = store.clientSelected.firstName
    name.value = store.clientSelected.name
    email.value = store.clientSelected.email
    phoneNumber.value = store.clientSelected.phoneNumber
    isAccountLocked.value = store.clientSelected.isAccountLocked
  } else {
    Notify.create({
      type: 'negative',
      message: 'Client introuvable',
      position: 'top',
    })
    router.back()
  }
}

onMounted(async () => {
  await loadData(route.query.id)
})

watch(
  () => route.query.id,
  async (newId) => {
    if (newId) {
      await loadData(newId)
    }
  },
)

const clientAddresses = computed(() => store.clientAddresses || [])
const isAddressModalOpen = ref(false)
const editingAddressData = ref(null)
const editingAddressId = ref(null)

const currentPage = ref(1)
const itemsPerPage = ref(5)

const totalPages = computed(() => Math.ceil(clientAddresses.value.length / itemsPerPage.value))
const hasNextPage = computed(() => currentPage.value < totalPages.value)
const hasPreviousPage = computed(() => currentPage.value > 1)

const paginatedAddresses = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  return clientAddresses.value.slice(start, end)
})

const goToPreviousPage = () => {
  if (hasPreviousPage.value) currentPage.value--
}

const goToNextPage = () => {
  if (hasNextPage.value) currentPage.value++
}

const goToPage = (page) => {
  currentPage.value = page
}

const getVisiblePages = () => {
  const total = totalPages.value
  const current = currentPage.value
  const pages = []

  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    if (current <= 3) {
      for (let i = 1; i <= 5; i++) pages.push(i)
    } else if (current >= total - 2) {
      for (let i = total - 4; i <= total; i++) pages.push(i)
    } else {
      for (let i = current - 2; i <= current + 2; i++) pages.push(i)
    }
  }
  return pages
}

const formatAddress = (address) => {
  if (!address) return ''
  const parts = []
  if (address.doorNb) parts.push(address.doorNb.toString())
  if (address.street) parts.push(address.street)
  if (address.unit) parts.push(`Apt ${address.unit}`)
  if (address.city) parts.push(address.city)
  if (address.province) parts.push(address.province)
  if (address.postalCode) parts.push(address.postalCode)
  if (address.country) parts.push(address.country)
  return parts.join(', ')
}

const openEditAddressModal = (address) => {
  editingAddressId.value = address.id
  editingAddressData.value = {
    doorNb: address.doorNb,
    street: address.street,
    city: address.city,
    postalCode: address.postalCode,
    province: address.province,
    provinceId: address.provinceId,
    country: address.country,
    countryId: address.countryId,
    unit: address.unit,
  }
  isAddressModalOpen.value = true
}

const closeEditModal = () => {
  isAddressModalOpen.value = false
  editingAddressId.value = null
  editingAddressData.value = null
}

const openCreateAddressModal = () => {
  editingAddressId.value = null
  editingAddressData.value = null
  isAddressModalOpen.value = true
}

const handleAddressSave = async (data) => {
  const addressPayload = {
    addressNumber: parseInt(data.doorNb),
    street: data.street || '',
    city: data.city || '',
    postalCode: data.postalCode || '',
    state: data.state?.id || data.provinceId,
    country: data.country?.id || data.countryId,
    unit: data.unit,
  }

  let success = false
  if (editingAddressId.value) {
    success = await store.updateClientAddressAsync(editingAddressId.value, addressPayload)
  } else {
    success = await store.addClientAddressAsync(clientId.value, addressPayload)
  }

  if (success) {
    Notify.create({
      type: 'positive',
      message: editingAddressId.value
        ? 'Adresse mise à jour avec succès'
        : 'Adresse ajoutée avec succès',
      position: 'top',
    })
    closeEditModal()
    if (route.query.id) {
      await loadClientData(route.query.id)
    }
  } else {
    Notify.create({
      type: 'negative',
      message: 'Erreur lors de la sauvegarde',
      position: 'top',
    })
  }
}

const nameVerified = () => {
  return validateName(name, nameError, nameErrorMessage)
}

const firstnameVerified = () => {
  return validateFirstName(firstname, firstnameError, firstnameErrorMessage)
}

const emailVerified = () => {
  return validateEmail(email, emailError, emailErrorMessage)
}

const phoneNumberVerified = () => {
  return validatePhoneNumber(phoneNumber, phoneNumberError, phoneNumberErrorMessage)
}

const validateInputs = () => {
  let isValid = true
  if (!nameVerified()) isValid = false
  if (!firstnameVerified()) isValid = false
  if (!emailVerified()) isValid = false
  if (!phoneNumberVerified()) isValid = false
  return isValid
}

const submitForm = async () => {
  const clientData = {
    id: clientId.value,
    firstName: firstname.value,
    name: name.value,
    email: email.value,
    phoneNumber: phoneNumber.value,
    isAccountLocked: isAccountLocked.value,
  }

  const result = await store.updateClientAsync(clientData)

  if (result) {
    Notify.create({
      type: 'positive',
      message: 'Client mis à jour avec succès',
      position: 'top',
    })

    await loadClientData(clientId.value)
  } else {
    Notify.create({
      type: 'negative',
      message: 'Erreur lors de la mise à jour du client',
      position: 'top',
    })
  }
}

async function handleClick() {
  if (isEditMode.value == true) {
    originalFirstname.value = firstname.value
    originalName.value = name.value
    originalEmail.value = email.value
    originalPhoneNumber.value = phoneNumber.value

    isEditMode.value = false
    isDisabled.value = false
  } else {
    if (validateInputs()) {
      await submitForm()
      isEditMode.value = true
      isDisabled.value = true
    }
  }
}

function cancelEdit() {
  firstname.value = originalFirstname.value
  name.value = originalName.value
  email.value = originalEmail.value
  phoneNumber.value = originalPhoneNumber.value

  firstnameError.value = false
  nameError.value = false
  emailError.value = false
  phoneNumberError.value = false

  firstnameErrorMessage.value = ''
  nameErrorMessage.value = ''
  emailErrorMessage.value = ''
  phoneNumberErrorMessage.value = ''

  isEditMode.value = true
  isDisabled.value = true
}

const backClick = () => {
  router.push('/gestionMagasin/clients')
}
</script>

<template>
  <q-page-container>
    <q-page class="fit q-pa-lg page">
      <div class="title-section">
        <div class="text-h5 text-bold">Gérer le client</div>
      </div>
      <div class="row justify-content-end">
        <q-btn label="Retourner à la liste de clients" class="btn-back m-4" @click="backClick" />
      </div>
      <q-card class="row q-ma-md no-wrap my-card-border">
        <q-item class="col-2 row justify-content-center">
          <q-item-section avatar>
            <q-avatar class="row justify-content-center">
              <q-icon name="person" size="lg"></q-icon>
            </q-avatar>
          </q-item-section>
        </q-item>
        <q-card-section class="col-md-8 employee-details-section">
          <h6>{{ firstname }} {{ name }}</h6>

          <dl class="details-list">
            <dt>Rôle :</dt>
            <dd>{{ role.toUpperCase() }}</dd>

            <dt>État du compte :</dt>
            <dd v-if="!isAccountLocked">ACTIF</dd>
            <dd v-if="isAccountLocked">Inactif</dd>
          </dl>
        </q-card-section>
        <q-item class="col-2 row justify-content-center">
          <q-item-section>
            <q-icon class="lock_employe" color="red" size="xl"
              ><i
                :class="{
                  'bi bi-lock': isAccountLocked,
                }"
                ><q-tooltip class="text-body2">{{
                  isAccountLocked ? 'Compte bloqué.' : ''
                }}</q-tooltip></i
              ></q-icon
            >
          </q-item-section>
        </q-item>
      </q-card>
      <q-card class="q-ma-md employee-form-card">
        <q-form class="q-pa-md q-mt-xl q-mb-lg">
          <h6>Détails du client</h6>
          <div class="row align-items-center">
            <label class="col-3" for="name">Nom</label>
            <q-input
              class="col-7"
              id="name"
              v-model="name"
              filled
              :disable="isDisabled"
              @blur="nameVerified"
            ></q-input>
            <span class="nameErrorMessage col-2">{{ nameErrorMessage }} </span>
          </div>
          <div class="row align-items-center">
            <label class="col-3" for="firstname">Prénom</label>
            <q-input
              class="col-7"
              id="firstname"
              v-model="firstname"
              filled
              :disable="isDisabled"
              @blur="firstnameVerified"
            ></q-input>
            <span class="firstnameErrorMessage col-2">{{ firstnameErrorMessage }} </span>
          </div>
          <div class="row align-items-center">
            <label class="col-3" for="email">Adresse courriel</label>
            <q-input
              class="col-7"
              id="email"
              v-model="email"
              filled
              :disable="isDisabled"
              @blur="emailVerified"
            ></q-input>
            <span class="emailErrorMessage col-2">{{ emailErrorMessage }} </span>
          </div>
          <div class="row align-items-center">
            <label class="col-3" for="phoneNumber">Numéro de téléphone</label>
            <q-input
              class="col-7"
              id="phoneNumber"
              v-model="phoneNumber"
              filled
              mask="(###) ### - ####"
              :disable="isDisabled"
              @blur="phoneNumberVerified"
            ></q-input>
            <span class="phoneNumberErrorMessage col-2">{{ phoneNumberErrorMessage }} </span>
          </div>
          <div class="row justify-end q-pa-md no-wrap items-center">
            <q-btn
              :label="isEditMode ? 'Modifier' : 'Sauvegarder'"
              :class="isEditMode ? 'btn-brown' : isDisabled ? '' : 'bg-primary text-white'"
              :color="isEditMode ? '' : 'primary'"
              no-caps
              :disable="!isEditMode && isDisabled"
              @click="handleClick"
            />
            <q-btn
              v-if="!isEditMode"
              label="Annuler"
              color="negative"
              class="q-ml-sm"
              no-caps
              @click="cancelEdit"
            />
          </div>
        </q-form>
      </q-card>

      <!-- Address Section -->
      <q-card class="row q-ma-md info-card">
        <q-card-section
          class="title-2-card col-12"
          style="display: flex; justify-content: space-between; align-items: center; width: 100%"
        >
          <h6>Adresses enregistrées ({{ clientAddresses.length }})</h6>
          <q-btn label="Créer adresse" class="btn-brown" no-caps @click="openCreateAddressModal" />
        </q-card-section>

        <q-card-section class="col-12 q-pa-md address-list-section">
          <div v-if="!clientAddresses || clientAddresses.length === 0" class="text-center q-pa-lg">
            <p style="font-size: 1.1rem; color: inherit; margin-bottom: 0">
              Aucune adresse enregistrée
            </p>
          </div>
          <q-list v-else separator>
            <q-item
              v-for="address in paginatedAddresses"
              :key="address.id"
              class="q-pa-md address-item"
            >
              <q-item-section>
                <q-item-label class="text-body1 text-weight-medium">
                  {{ formatAddress(address) }}
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-btn
                  label="Modifier"
                  class="btn-brown"
                  size="sm"
                  no-caps
                  @click="openEditAddressModal(address)"
                />
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>

      <!-- Pagination -->
      <div
        class="pagination-container d-flex justify-content-center mt-4 mb-4"
        v-if="totalPages > 1"
      >
        <nav class="custom-pagination">
          <ul class="pagination-list">
            <li class="pagination-item">
              <button
                class="pagination-btn prev-btn"
                :disabled="!hasPreviousPage"
                @click="goToPreviousPage"
              >
                <q-icon name="chevron_left" size="sm" />
              </button>
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

            <li class="pagination-item">
              <button
                class="pagination-btn next-btn"
                :disabled="!hasNextPage"
                @click="goToNextPage"
              >
                <q-icon name="chevron_right" size="sm" />
              </button>
            </li>
          </ul>
        </nav>
      </div>

      <AddressModal
        v-model="isAddressModalOpen"
        :initial-address="editingAddressData"
        :is-edit-mode="!!editingAddressId"
        @save="handleAddressSave"
        @cancel="closeEditModal"
      />
    </q-page>
  </q-page-container>
</template>

<style scoped>
.firstnameErrorMessage,
.nameErrorMessage,
.emailErrorMessage,
.phoneNumberErrorMessage {
  color: red !important;
}

.edit-btn {
  color: white !important;
  background: #424242 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.save-btn {
  color: white !important;
  background: #4cae50 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.lock_employe {
  font-size: 110px !important;
}

.btn-back {
  color: white !important;
  background: #5d4037 !important;
  font-weight: bold !important;
  font-size: large;
  width: fit-content !important;
  display: inline-block !important;
  white-space: nowrap !important;
}

.page {
  padding-bottom: 100px;
  padding-top: 0;
  padding-left: 0;
  margin-left: 0;
  margin-right: 0;
  min-height: 600px;
}

.title-section {
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
}

.title-section .text-h5 {
  margin: 0;
}

.q-page-container {
  padding-bottom: 10px !important;
  padding-top: 0 !important;
  padding-left: 0px !important;
  padding-right: 0px;
  color: black;
}

.my-card-border {
  border: 1px solid black !important;
  overflow: hidden;
  border-radius: 8px;
}

.details-list {
  display: grid;
  grid-template-columns: max-content auto;
  align-items: center;
  gap: 0.5rem 1rem;
}

.details-list dt {
  font-weight: bold;
}

.details-list dd {
  margin: 0;
  display: flex;
  align-items: center;
}

.employee-details-section {
  padding-left: 20px;
}

body.body--dark .page input,
body.body--dark .page .q-field__control {
  background-color: #232a38;
  color: white;
  border: 1px solid #4a4a4a;
}

/* Pagination Styles  */
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

.address-item {
  background-color: #f0eada !important;
  border-radius: 8px;
  margin-bottom: 8px;
  border: 1px solid transparent;
  transition: all 0.3s ease;
}

.address-item:hover {
  background-color: #f0eada !important;
  border: 1px solid rgba(0, 0, 0, 0.2) !important;
}

body.body--dark .address-item {
  background-color: rgba(255, 255, 255, 0.1) !important;
  color: white !important;
}

body.body--dark .address-item:hover {
  background-color: transparent !important;
  border: 1px solid rgba(255, 255, 255, 0.1) !important;
}

body:not(.body--dark) .address-item,
body:not(.body--dark) .address-item .q-item__label {
  color: black !important;
}

body.body--dark .page .q-field__native {
  color: white !important;
}

.employee-form-card {
  display: flex;
  flex-direction: column;
}

.address-list-section {
  background-color: #a19b93 !important;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
}

body.body--dark .address-list-section {
  background-color: rgba(255, 255, 255, 0.05) !important;
}

.btn-brown {
  background-color: #5d4037 !important;
  color: white !important;
}
</style>
