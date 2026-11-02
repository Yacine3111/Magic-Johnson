<script setup>
import '@/assets/gestionCompte.css'
import {
  validateName,
  validateFirstName,
  validateEmail,
  validatePhoneNumber,
} from '@/validators/registerValidator'
import { ref } from 'vue'
import { onMounted } from 'vue'
import { useEmployeeStore } from '@/stores/employeeStore'
import { GetUser } from '@/api/User/UserApi'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const route = useRoute()

const isDisabled = ref(true)
const isEditMode = ref(true)

const firstname = ref('')
const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const role = ref('')

const originalFirstname = ref('')
const originalName = ref('')
const originalEmail = ref('')
const originalPhoneNumber = ref('')
const isAccountLocked = ref(false)
const employeId = ref('')

const firstnameError = ref(false)
const nameError = ref(false)
const phoneNumberError = ref(false)
const emailError = ref(false)

const firstnameErrorMessage = ref('')
const nameErrorMessage = ref('')
const emailErrorMessage = ref('')
const phoneNumberErrorMessage = ref('')

const router = useRouter()
const store = useEmployeeStore()
const authStore = useAuthStore()

onMounted(async () => {
  employeId.value = route.query.id
  if (employeId === '') {
    router.push('/')
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
  }

  await store.getSelectedEmployeeAsync(employeId.value)

  if (store.employeeSelected != null) {
    firstname.value = store.employeeSelected.firstName
    name.value = store.employeeSelected.name
    email.value = store.employeeSelected.email
    phoneNumber.value = store.employeeSelected.phoneNumber
    isAccountLocked.value = store.employeeSelected.isAccountLocked
    role.value = store.employeeSelected.role
  } else {
    Notify.create({
      type: 'negative',
      message: 'Une erreur est survenue',
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
  }
})

async function handleClick() {
  if (isEditMode.value == true) {
    originalFirstname.value = firstname.value
    originalName.value = name.value
    originalEmail.value = email.value
    originalPhoneNumber.value = phoneNumber.value

    isEditMode.value = false
    isDisabled.value = false
  } else {
    if (validateInputs) {
      await submitForm()
      isEditMode.value = true
      isDisabled.value = true
    }
  }
}

const firstnameVerified = () => {
  return validateFirstName(firstname, firstnameError, firstnameErrorMessage)
}

const nameVerified = () => {
  return validateName(name, nameError, nameErrorMessage)
}

const emailVerified = () => {
  return validateEmail(email, emailError, emailErrorMessage)
}

const phoneNumberVerified = () => {
  return validatePhoneNumber(phoneNumber, phoneNumberError, phoneNumberErrorMessage)
}

function validateInputs() {
  return firstnameVerified && nameVerified && emailVerified && phoneNumberVerified
}

async function submitForm() {
  const updatedEmploye = {
    id: employeId.value,
    firstname: firstname.value,
    name: name.value,
    email: email.value,
    phoneNumber: phoneNumber.value,
  }

  try {
    Loading.show({
      message: 'Enregistrement...',
      spinnerColor: 'primary',
    })
    await store.updateEmployeeAsync(updatedEmploye)

    Notify.create({
      type: 'positive',
      message: "Les infos de l'employé ont été mises à jour ",
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
    await store.getSelectedEmployeeAsync(employeId.value)
  } catch (e) {
    Notify.create({
      type: 'negative',
      message: 'Impossible de modifier les informations ',
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
    Loading.hide()
  }
}

function cancel() {
  firstname.value = originalFirstname.value
  name.value = originalName.value
  email.value = originalEmail.value
  phoneNumber.value = originalPhoneNumber.value

  isEditMode.value = true
  isDisabled.value = true
}

//Événement
const backClick = () => {
  router.push('/gestionMagasin/employes')
}
</script>

<template>
  <q-page-container>
    <q-page class="fit q-pa-lg page">
      <div class="title-section">
        <div class="text-h5 text-bold">Gérer l'employé</div>
      </div>
      <div class="row justify-content-end">
        <q-btn label="Retourner à la liste d'employés" class="btn-back m-4" @click="backClick" />
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
            <dd v-if="!isLocked">ACTIF</dd>
            <dd v-if="isLocked">
              Inactif <q-icon name="mdi-lock" class="lock_employe" size="sm"></q-icon>
            </dd>
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
          <h6>Détails de l'employé</h6>
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
            <label class="col-3" for="name">Prénom</label>
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
            <label class="col-3" for="name">Adresse courriel</label>
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
            <label class="col-3" for="name">Numéro de téléphone</label>
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
        </q-form>
      </q-card>
      <div class="row q-ma-md">
        <q-btn
          @click="handleClick"
          class="q-ma-md q-mt-xl"
          :label="isEditMode ? 'Modifier' : 'Sauvegarder'"
          :class="isEditMode ? 'edit-btn' : 'save-btn'"
          no-caps
          :disable="
            !nameVerified() || !firstnameVerified() || !emailVerified() || !phoneNumberVerified()
          "
        >
        </q-btn>
        <q-btn
          v-if="!isEditMode"
          @click="cancel"
          class="q-ma-md q-mt-xl cancel-btn"
          label="Annuler"
        >
        </q-btn>
      </div>
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
  background-color: white !important;
  color: black !important;
}

body.body--dark .page .q-field__native {
  color: black !important;
}

.employee-form-card {
  display: flex;
  flex-direction: column;
}
</style>
