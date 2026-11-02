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
import { useAuthStore } from '@/stores/authStore'
import { GetUser } from '@/api/User/UserApi'
import { Loading } from 'quasar'
import { Notify } from 'quasar'

//Button state
const isDisabled = ref(true)
const isEditMode = ref(true)

const firstname = ref('')
const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const role = ref('')

//Variables to store initial value while on edit mode
const originalFirstname = ref('')
const originalName = ref('')
const originalEmail = ref('')
const originalPhoneNumber = ref('')

//Error status
const firstnameError = ref(false)
const nameError = ref(false)
const phoneNumberError = ref(false)
const emailError = ref(false)

//Error messages
const firstnameErrorMessage = ref('')
const nameErrorMessage = ref('')
const emailErrorMessage = ref('')
const phoneNumberErrorMessage = ref('')

const auth = useAuthStore()

onMounted(async () => {
  firstname.value = auth.user.firstName
  name.value = auth.user.lastName
  email.value = auth.user.email
  phoneNumber.value = auth.user.phoneNumber
  await auth.fetchRole()

  role.value = auth.role[0]
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
  const updatedUser = {
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
    await auth.updateUserInfo(updatedUser)

    Notify.create({
      type: 'positive',
      message: 'Vos informations ont été mises à jour',
      position: 'top',
    })
    await auth.fetchUser()
  } catch (e) {
    Notify.create({
      type: 'negative',
      message: 'Impossible de modifier les informations ',
      position: 'top',
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
</script>

<template>
  <q-page class="fit q-pa-lg">
    <q-card class="row q-ma-md" style="overflow: hidden">
      <q-item class="col-2 row justify-content-center icon_container_bg">
        <q-item-section avatar>
          <q-avatar class="row justify-content-center">
            <q-icon name="person" size="lg"></q-icon>
          </q-avatar>
        </q-item-section>
      </q-item>
      <q-card-section class="col-md-8">
        <h6>{{ firstname }} {{ name }}</h6>
        <p>{{ role }}</p>
      </q-card-section>
    </q-card>
    <q-card class="q-ma-md">
      <q-form class="q-pa-md q-mt-xl q-mb-lg">
        <h6>Détails personnels</h6>
        <div class="row align-items-center">
          <label class="col-3" for="name">Nom</label>
          <q-input
            class="col-7"
            id="name"
            v-model="name"
            filled
            text-color="black"
            bg-color="white"
            :dense="true"
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
            text-color="black"
            bg-color="white"
            :dense="true"
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
            text-color="black"
            bg-color="white"
            :dense="true"
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
            text-color="black"
            bg-color="white"
            mask="(###) ### - ####"
            :dense="true"
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
        :color="isEditMode ? undefined : 'primary'"
        :style="isEditMode ? 'background-color: #8A422C !important; color: white' : ''"
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
        color="negative"
      >
      </q-btn>
    </div>
  </q-page>
</template>

<style scoped>
.firstnameErrorMessage,
.nameErrorMessage,
.emailErrorMessage,
.phoneNumberErrorMessage {
  color: red;
}
.icon_container_bg {
  background-color: rgb(92, 99, 90) !important;
}
</style>

<style>
/* Light Mode d */
.q-card {
  background-color: #a89c8d !important;
  color: black;
}

/* Dark Mode */
body.body--dark .q-card {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .q-field__label,
body.body--dark .q-field__native,
body.body--dark .q-input input {
  color: white !important;
}

body.body--dark .q-field--filled .q-field__control {
  background: #2d3c49 !important;
}

body.body--dark h6,
body.body--dark p,
body.body--dark label {
  color: white !important;
}
</style>
