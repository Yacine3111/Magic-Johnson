<script setup>
import '@/assets/gestionCompte.css'
import { ref } from 'vue'
import { validatePassword, validateConfirmationPassword } from '@/validators/registerValidator'
import { useAuthStore } from '@/stores/authStore'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { checkEmptyFieldSecurity } from '@/validators/emptyFieldsValidator'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const isDisabled = ref(false)
const isCurrentPwd = ref(true)
const isNewPwd = ref(true)

const currentPassword = ref('')
const newPassword = ref('')
const passwordConfirmation = ref('')

const currentPasswordError = ref('')
const newPasswordError = ref('')
const passwordConfirmationError = ref('')

const currentPasswordErrorMessage = ref('')
const newPasswordErrorMessage = ref('')
const passwordConfirmationErrorMessage = ref('')

const auth = useAuthStore()

async function handleClick() {
  if (
    checkEmptyFieldSecurity(
      currentPassword,
      currentPasswordErrorMessage,
      newPassword,
      newPasswordErrorMessage,
      passwordConfirmation,
      passwordConfirmationErrorMessage,
    ) &&
    validateInputs
  ) {
    await submitForm()
  } else {
  }
}

const currentPasswordVerified = () => {
  return validatePassword(currentPassword, currentPasswordError, currentPasswordErrorMessage)
}

const newPasswordVerified = () => {
  return validatePassword(newPassword, newPasswordError, newPasswordErrorMessage)
}

const passwordConfirmationVerified = () => {
  return validateConfirmationPassword(
    newPassword,
    passwordConfirmation,
    passwordConfirmationError,
    passwordConfirmationErrorMessage,
  )
}

function validateInputs() {
  return currentPasswordVerified && newPasswordVerified && passwordConfirmationVerified
}

async function submitForm() {
  const updatedPassword = {
    currentPassword: currentPassword.value,
    newPassword: newPassword.value,
    passwordConfirmation: passwordConfirmation.value,
  }

  try {
    Loading.show({
      message: 'Enregistrement...',
      spinnerColor: 'primary',
    })
    await auth.updateUserPassword(updatedPassword)

    Notify.create({
      type: 'positive',
      message: 'Votre mot de passe a été mit à jour avec succès ',
      position: 'top',
    })
    resetFields()
  } catch (err) {
    Notify.create({
      type: 'negative',
      message: 'Impossible de modifier le mot de passe ',
      position: 'top',
    })
  } finally {
    Loading.hide()
  }
}

function resetFields() {
  currentPassword.value = ''
  newPassword.value = ''
  passwordConfirmation.value = ''
}
</script>
<template>
  <q-page class="fit q-pa-lg">
    <!-- Version Desktop -->
    <div v-if="$q.screen.gt.sm">
      <q-card class="row q-ma-md q-pa-sm text-align-center title-card">
        <h5>Sécurité</h5>
      </q-card>
      <q-card class="row q-ma-md info-card">
        <q-card-section class="title-2-card">
          <h6>Changer le mot de passe</h6>
        </q-card-section>
        <q-form class="q-pa-md">
          <div class="column q-mt-lg">
            <label for="currentPassword">Mot de passe actuel</label>
            <q-input
              id="currentPassword"
              filled
              v-model="currentPassword"
              text-color="black"
              bg-color="white"
              class="col-8"
              placeholder="********"
              @blur="currentPasswordVerified"
              :dense="true"
              :disable="isDisabled"
              :type="isCurrentPwd ? 'password' : 'text'"
              :class="{ 'currentPassword-error': currentPasswordError }"
            >
              <q-icon
                class="cursor-pointer icon"
                size="xs"
                :name="isCurrentPwd ? 'visibility_off' : 'visibility'"
                @click="isCurrentPwd = !isCurrentPwd"
              ></q-icon>
            </q-input>
            <span class="currentPasswordErrorMessage col-7"
              >{{ currentPasswordErrorMessage }}
            </span>
          </div>
          <div class="column q-mt-lg">
            <label for="newPassword">Nouveau mot de passe</label>
            <q-input
              id="newPassword"
              filled
              v-model="newPassword"
              text-color="black"
              bg-color="white"
              class="col-8"
              placeholder="********"
              @blur="newPasswordVerified"
              :dense="true"
              :disable="isDisabled"
              :type="isNewPwd ? 'password' : 'text'"
              :class="{ 'newPassword-error': newPasswordError }"
            >
              <q-icon
                class="cursor-pointer icon"
                size="xs"
                :name="isNewPwd ? 'visibility_off' : 'visibility'"
                @click="isNewPwd = !isNewPwd"
              ></q-icon>
            </q-input>
            <span class="newPasswordErrorMessage col-7">{{ newPasswordErrorMessage }} </span>
          </div>
          <div class="column q-mt-lg q-mb-lg">
            <label for="confirmationPassword">Confirmer le mot de passe</label>
            <q-input
              id="confirmationPassword"
              class="col-8"
              v-model="passwordConfirmation"
              filled
              type="password"
              bg-color="white"
              text-color="black"
              :dense="true"
              :disable="isDisabled"
              @blur="passwordConfirmationVerified"
              :class="{ 'passwordConfirmation-error': passwordConfirmationError }"
            >
            </q-input>
            <span class="passwordConfirmationErrorMessage col-7"
              >{{ passwordConfirmationErrorMessage }}
            </span>
          </div>
        </q-form>
      </q-card>
      <div class="row q-ma-md">
        <q-btn
          @click="handleClick"
          class="q-ma-md q-mt-xl edit-btn"
          label="Sauvegarder"
          no-caps
          color="primary"
        >
        </q-btn>
      </div>
    </div>

    <!-- Version Mobile -->
    <div v-else class="mobile-container">
      <q-card class="q-ma-sm q-pa-md mobile-card">
        <h5 class="mobile-title text-center">Sécurité</h5>
      </q-card>

      <q-card class="q-ma-sm mobile-card">
        <q-card-section>
          <h6 class="mobile-section-title">Changer le mot de passe</h6>
        </q-card-section>
        <q-form class="q-pa-md">
          <div class="mobile-form-field">
            <label for="currentPassword-mobile" class="mobile-label">Mot de passe actuel</label>
            <q-input
              id="currentPassword-mobile"
              filled
              v-model="currentPassword"
              text-color="black"
              bg-color="white"
              placeholder="********"
              @blur="currentPasswordVerified"
              :dense="true"
              :disable="isDisabled"
              :type="isCurrentPwd ? 'password' : 'text'"
              :class="{ 'currentPassword-error': currentPasswordError }"
              class="mobile-input"
            >
              <q-icon
                class="cursor-pointer icon"
                size="xs"
                :name="isCurrentPwd ? 'visibility_off' : 'visibility'"
                @click="isCurrentPwd = !isCurrentPwd"
              ></q-icon>
            </q-input>
            <span class="mobile-error">{{ currentPasswordErrorMessage }}</span>
          </div>

          <div class="mobile-form-field">
            <label for="newPassword-mobile" class="mobile-label">Nouveau mot de passe</label>
            <q-input
              id="newPassword-mobile"
              filled
              v-model="newPassword"
              text-color="black"
              bg-color="white"
              placeholder="********"
              @blur="newPasswordVerified"
              :dense="true"
              :disable="isDisabled"
              :type="isNewPwd ? 'password' : 'text'"
              :class="{ 'newPassword-error': newPasswordError }"
              class="mobile-input"
            >
              <q-icon
                class="cursor-pointer icon"
                size="xs"
                :name="isNewPwd ? 'visibility_off' : 'visibility'"
                @click="isNewPwd = !isNewPwd"
              ></q-icon>
            </q-input>
            <span class="mobile-error">{{ newPasswordErrorMessage }}</span>
          </div>

          <div class="mobile-form-field">
            <label for="confirmationPassword-mobile" class="mobile-label"
              >Confirmer le mot de passe</label
            >
            <q-input
              id="confirmationPassword-mobile"
              v-model="passwordConfirmation"
              filled
              type="password"
              bg-color="white"
              text-color="black"
              :dense="true"
              :disable="isDisabled"
              @blur="passwordConfirmationVerified"
              :class="{ 'passwordConfirmation-error': passwordConfirmationError }"
              class="mobile-input"
            >
            </q-input>
            <span class="mobile-error">{{ passwordConfirmationErrorMessage }}</span>
          </div>
        </q-form>
      </q-card>

      <div class="mobile-buttons">
        <q-btn
          @click="handleClick"
          class="mobile-btn edit-btn"
          label="Sauvegarder"
          no-caps
          color="primary"
        >
        </q-btn>
      </div>
    </div>
  </q-page>
</template>
<style scoped>
.currentPasswordErrorMessage,
.newPasswordErrorMessage,
.passwordConfirmationErrorMessage {
  color: red !important;
}

.icon {
  height: 100%;
  color: gray;
}

.currentPassword-error,
.newPassword-error,
.passwordConfirmation-error {
  border-left: 3px solid #ff0000;
  border-radius: 5px 5px 0 0;
}

/* Styles Mobile */
.mobile-container {
  width: 100%;
  max-width: 100%;
}

.mobile-card {
  background-color: #f5f5f5;
  border-radius: 8px;
}

.mobile-title {
  font-size: 1.3rem;
  font-weight: bold;
  margin: 0;
  color: #333;
}

.mobile-section-title {
  font-size: 1.1rem;
  font-weight: bold;
  margin: 0;
  color: #333;
}

.mobile-form-field {
  margin-bottom: 1.5rem;
}

.mobile-label {
  display: block;
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: #333;
  font-size: 0.9rem;
}

.mobile-input {
  width: 100%;
}

.mobile-error {
  display: block;
  color: red !important;
  font-size: 0.8rem;
  margin-top: 0.25rem;
  min-height: 1.2rem;
}

.mobile-buttons {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 15px;
}

.mobile-btn {
  width: 100%;
}
</style>

<style>
/* Light Mode */
.q-card,
.mobile-card {
  background-color: #a89c8d !important;
  color: black;
}

/* Dark Mode  */
body.body--dark .q-card,
body.body--dark .mobile-card {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark h5,
body.body--dark h6,
body.body--dark .mobile-title,
body.body--dark .mobile-section-title,
body.body--dark label,
body.body--dark .mobile-label,
body.body--dark .icon {
  color: white !important;
}

body.body--dark .q-input .q-field__control,
body.body--dark input {
  background-color: #2d3c49 !important;
  color: white !important;
}

body.body--dark .q-field__native,
body.body--dark .q-icon {
  color: white !important;
}
</style>
