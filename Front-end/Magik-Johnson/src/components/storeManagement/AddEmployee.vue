<script setup>
import {
  validateFirstName,
  validateName,
  validateEmail,
  validatePhoneNumber,
  validatePassword,
  validateConfirmationPassword,
} from '@/validators/registerValidator'

import { ref } from 'vue'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { useQuasar } from 'quasar'
import router from '@/router'
import { EmployeeRegistrationRequest } from '@/models/EmployeeRegistrationRequest'
import { useEmployeeStore } from '@/stores/employeeStore'

const $q = useQuasar()
const store = useEmployeeStore()

const backClick = () => {
  router.push('employes')
}

//variables
const firstName = ref('')
const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const password = ref('')
const passwordConfirmation = ref('')
const isPwd = ref(true)
const isPwdConfirmation = ref(true)
const hiringDate = ref('')
const isAccountEnabled = ref(false)

//Error status
const firstnameError = ref(false)
const nameError = ref(false)
const phoneNumberError = ref(false)
const emailError = ref(false)
const passwordError = ref(false)
const passwordConfirmationError = ref(false)
const hiringDateError = ref(false)

//Error messages
const firstnameErrorMessage = ref('')
const nameErrorMessage = ref('')
const emailErrorMessage = ref('')
const phoneNumberErrorMessage = ref('')
const passwordErrorMessage = ref('')
const passwordConfirmationErrorMessage = ref('')
const hiringDateErrorMessage = ref('')

const firstnameVerified = () => {
  return validateFirstName(firstName, firstnameError, firstnameErrorMessage)
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

const passwordVerified = () => {
  return validatePassword(password, passwordError, passwordErrorMessage)
}

const passwordConfirmationVerified = () => {
  return validateConfirmationPassword(
    password,
    passwordConfirmation,
    passwordConfirmationError,
    passwordConfirmationErrorMessage,
  )
}

const hiringDateVerified = () => {
  if (hiringDate.value.trim == '') {
    hiringDateErrorMessage.value = "Date d'embauche requise"
    hiringDateError.value = true

    return false
  }

  hiringDateErrorMessage.value = ''
  hiringDateError.value = false
  return true
}

//function validating that all inputs are valid
function dataSigninValided() {
  return (
    firstnameVerified &&
    nameVerified &&
    phoneNumberVerified &&
    emailVerified &&
    passwordVerified &&
    passwordConfirmationVerified
  )
}

//function validating that fields are not empty
//triggered on buton clicked
function fieldNotEmpty() {
  let triggered = false
  if (firstName.value.trim() == '') {
    firstnameErrorMessage.value = 'Prénom requis.'
    firstnameError.value = true
    triggered = true
  }
  if (name.value.trim() == '') {
    nameErrorMessage.value = 'Nom requis'
    nameError.value = true
    triggered = true
  }
  if (email.value.trim() == '') {
    emailErrorMessage.value = 'Courriel requis'
    emailError.value = true
    triggered = true
  }
  if (phoneNumber.value.trim() == '') {
    phoneNumberErrorMessage.value = 'Téléphone requis'
    phoneNumberError.value = true
    triggered = true
  }
  if (password.value.trim() == '') {
    passwordErrorMessage.value = 'Mot de passe requis'
    passwordError.value = true
    triggered = true
  }
  if (passwordConfirmation.value.trim() == '') {
    passwordConfirmationErrorMessage.value = ' Mot de passe de confirmation requis'
    passwordConfirmationError.value = true
    triggered = true
  }

  if (hiringDate.value.trim() == '') {
    hiringDateErrorMessage.value = "Date d'embauche requise"
    hiringDateError.value = true
    triggered = true
  }

  if (triggered == true) {
    return false
  }
  return true
}

const signin = async () => {
  if (dataSigninValided && fieldNotEmpty()) {
    try {
      Loading.show({ spinnerColor: 'primary' })
      const employeeRegistrationRequest = new EmployeeRegistrationRequest(
        firstName.value,
        name.value,
        email.value,
        phoneNumber.value,
        password.value,
        passwordConfirmation.value,
        isAccountEnabled.value,
        hiringDate.value,
      )
      const registrationSuccess = await store.addEmployeeAsync(employeeRegistrationRequest)

      if (registrationSuccess) {
        router.push('employes')
        Notify.create({
          type: 'positive',
          message: 'Le compte a été créé avec succès',
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
      } else {
        Notify.create({
          type: 'negative',
          message: "L'adresse courriel a déjà été utilisée !",
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
    } catch (e) {
    } finally {
      Loading.hide()
    }
  }
}
</script>

<template>
  <div class="page-container">
    <div class="title-section">
      <div class="text-h5 text-bold text-white">Nouvel employé</div>
      <q-btn
        color="brown-8"
        label="Retour"
        class="btn-back-mobile"
        v-if="!$q.screen.gt.sm"
        unelevated
        no-caps
        @click="backClick"
      />
    </div>

    <div v-if="$q.screen.gt.sm" class="row justify-end q-mb-md container-width">
      <q-btn
        label="Retourner à la liste d'employés"
        class="btn-back-desktop"
        @click="backClick"
        no-caps
        unelevated
      />
    </div>

    <div class="employee-details-card container-width">
      <h2 class="form-title">Détails de l'employé</h2>

      <q-form @submit.prevent="signin" class="employee-form">
        <!-- Nom -->
        <div class="form-row">
          <div class="label-col">
            <label for="name">Nom</label>
          </div>
          <div class="input-col">
            <q-input
              id="name"
              filled
              dense
              v-model="name"
              class="dark-input"
              @blur="nameVerified"
              :class="{ 'input-error': nameError }"
              hide-bottom-space
            />
            <div v-if="nameError" class="error-msg">{{ nameErrorMessage }}</div>
          </div>
        </div>

        <!-- Prénom -->
        <div class="form-row">
          <div class="label-col">
            <label for="firstname">Prénom</label>
          </div>
          <div class="input-col">
            <q-input
              id="firstname"
              filled
              dense
              v-model="firstName"
              class="dark-input"
              @blur="firstnameVerified"
              :class="{ 'input-error': firstnameError }"
              hide-bottom-space
            />
            <div v-if="firstnameError" class="error-msg">{{ firstnameErrorMessage }}</div>
          </div>
        </div>

        <!-- Courriel -->
        <div class="form-row">
          <div class="label-col">
            <label for="email">Adresse courriel</label>
          </div>
          <div class="input-col">
            <q-input
              id="email"
              filled
              dense
              v-model="email"
              class="dark-input"
              @blur="emailVerified"
              :class="{ 'input-error': emailError }"
              autocomplete="username"
              hide-bottom-space
            />
            <div v-if="emailError" class="error-msg">{{ emailErrorMessage }}</div>
          </div>
        </div>

        <!-- Téléphone -->
        <div class="form-row">
          <div class="label-col">
            <label for="phoneNumber">Numéro de téléphone</label>
          </div>
          <div class="input-col">
            <q-input
              id="phoneNumber"
              filled
              dense
              v-model="phoneNumber"
              class="dark-input"
              mask="(###) ### - ####"
              @blur="phoneNumberVerified"
              :class="{ 'input-error': phoneNumberError }"
              hide-bottom-space
            />
            <div v-if="phoneNumberError" class="error-msg">{{ phoneNumberErrorMessage }}</div>
          </div>
        </div>

        <!-- Date d'embauche -->
        <div class="form-row">
          <div class="label-col">
            <label for="hiringDate">Date d'embauche</label>
          </div>
          <div class="input-col">
            <q-input
              id="hiringDate"
              filled
              dense
              v-model="hiringDate"
              class="dark-input"
              type="date"
              @blur="hiringDateVerified"
              :class="{ 'input-error': hiringDateError }"
              hide-bottom-space
            />
            <div v-if="hiringDateError" class="error-msg">{{ hiringDateErrorMessage }}</div>
          </div>
        </div>

        <!-- Mot de passe -->
        <div class="form-row">
          <div class="label-col">
            <label for="password">Mot de passe</label>
          </div>
          <div class="input-col">
            <q-input
              id="password"
              filled
              v-model="password"
              class="dark-input"
              :type="isPwd ? 'password' : 'text'"
              @blur="passwordVerified"
              :class="{ 'input-error': passwordError }"
              hide-bottom-space
            >
              <q-icon
                :name="isPwd ? 'visibility_off' : 'visibility'"
                class="cursor-pointer icon"
                size="xs"
                @click="isPwd = !isPwd"
              />
            </q-input>
            <div v-if="passwordError" class="error-msg">{{ passwordErrorMessage }}</div>
          </div>
        </div>

        <!-- Confirmation Mot de passe -->
        <div class="form-row">
          <div class="label-col">
            <label for="passwordConfirmation">Confirmer mot de passe</label>
          </div>
          <div class="input-col">
            <q-input
              id="passwordConfirmation"
              filled
              v-model="passwordConfirmation"
              class="dark-input"
              :type="isPwdConfirmation ? 'password' : 'text'"
              @blur="passwordConfirmationVerified"
              :class="{ 'input-error': passwordConfirmationError }"
              hide-bottom-space
            >
              <q-icon
                :name="isPwdConfirmation ? 'visibility_off' : 'visibility'"
                class="cursor-pointer icon"
                size="xs"
                @click="isPwdConfirmation = !isPwdConfirmation"
              />
            </q-input>
            <div v-if="passwordConfirmationError" class="error-msg">
              {{ passwordConfirmationErrorMessage }}
            </div>
          </div>
        </div>

        <!-- Checkbox et Submit -->
        <div class="form-actions">
          <div class="checkbox-wrapper q-mb-md">
            <input
              type="checkbox"
              id="accountEnabled"
              v-model="isAccountEnabled"
              class="custom-checkbox"
            />
            <label for="accountEnabled" class="text-white no-wrap-text checkbox-label">
              Cocher pour activer le compte dès sa création
            </label>
          </div>

          <q-btn
            type="submit"
            color="cyan-8"
            text-color="white"
            label="Ajouter l'employé"
            no-caps
            class="btn-submit full-width"
            unelevated
            size="lg"
          />
        </div>
      </q-form>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  min-height: 100vh;
  background-color: #676767;
  display: flex;
  flex-direction: column;
  padding: 20px;
}

.container-width {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
}

.header-section {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  margin-bottom: 20px;
}

.btn-back-desktop {
  background-color: #5d4037;
  color: white;
  font-weight: 500;
  white-space: nowrap;
}

.btn-back-mobile {
  border-radius: 8px;
}

/* Card Style */
.employee-details-card {
  background-color: #1f2228;
  border-radius: 8px;
  padding: 30px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
}

.form-title {
  color: white;
  font-size: 1.25rem;
  font-weight: 500;
  margin: 0 0 24px 0;
}

.employee-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-row {
  display: flex;
  align-items: center;
  gap: 20px;
}

.label-col {
  width: 250px;
  flex-shrink: 0;
}

.label-col label {
  color: #e5e7eb;
  font-weight: 500;
  font-size: 0.95rem;
}

.input-col {
  flex-grow: 1;
}

/* Input Styling */
.dark-input :deep(.q-field__control) {
  background-color: #2d323b !important;
  border-radius: 4px;
}

.dark-input :deep(.q-field__native),
.dark-input :deep(.q-field__input) {
  color: #e5e7eb !important;
}

.dark-input :deep(.q-placeholder) {
  color: #9ca3af !important;
}

:deep(input:-webkit-autofill),
:deep(input:-webkit-autofill:hover),
:deep(input:-webkit-autofill:focus),
:deep(input:-webkit-autofill:active) {
  -webkit-box-shadow: 0 0 0 30px #2d323b inset !important;
  -webkit-text-fill-color: #e5e7eb !important;
  transition: background-color 5000s ease-in-out 0s;
}

.input-error :deep(.q-field__control) {
  border: 1px solid #ef4444;
}

.error-msg {
  color: #ef4444 !important;
  font-size: 0.8rem;
  margin-top: 4px;
  font-weight: bold;
}

.form-actions {
  margin-top: 10px;
  display: flex;
  flex-direction: column;
}

.btn-submit {
  font-weight: 600;
  border-radius: 4px;
}

.icon {
  height: 100%;
  color: gray;
}

.no-wrap-text {
  white-space: nowrap;
}

.custom-checkbox {
  appearance: none;
  -webkit-appearance: none;
  width: 26px;
  height: 26px;
  background-color: #fff;
  border: 2px solid #2d3748;
  border-radius: 5px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  margin-right: 1rem;
  vertical-align: middle;
  transform: scale(1);
}

.custom-checkbox:hover {
  background-color: #e2e8f0;
}

.custom-checkbox:checked {
  background-color: #1e40af;
  border-color: #1e40af;
  animation: pop 0.2s ease forwards;
}

.custom-checkbox:checked::after {
  content: '✓';
  color: #fff;
  font-size: 16px;
  position: absolute;
  top: 2px;
  left: 5px;
  transform: scale(0);
  animation: pop-check 0.2s ease forwards;
}

@keyframes pop {
  0% {
    transform: scale(0.9);
  }
  50% {
    transform: scale(1.2);
  }
  100% {
    transform: scale(1);
  }
}

@keyframes pop-check {
  0% {
    transform: scale(0);
    opacity: 0;
  }
  50% {
    transform: scale(1.3);
    opacity: 1;
  }
  100% {
    transform: scale(1);
  }
}

.checkbox-wrapper {
  display: flex;
  align-items: center;
}

.checkbox-label {
  cursor: pointer;
  font-weight: 500;
}

@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }

  .label-col {
    width: 100%;
  }

  .employee-details-card {
    padding: 20px;
  }
}

.title-section {
  background-color: #959595;
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
}
.title-section .text-h5 {
  color: white;
  margin: 0;
}
</style>

<style>
/* Light Mode Overrides (Global to ensure autofill specificity works) */
body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .page-container {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .employee-details-card {
  background-color: #f0eada !important;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .form-title {
  color: black !important;
}

body:not(.body--dark) .label-col label {
  color: black !important;
}

/* Force white background on inputs in light mode */
body:not(.body--dark) .dark-input .q-field__control {
  background-color: white !important;
  border: 1px solid #c2c2c2;
}

body:not(.body--dark) .dark-input .q-field__native,
body:not(.body--dark) .dark-input .q-field__input {
  color: black !important;
}

body:not(.body--dark) .dark-input .q-placeholder {
  color: #555 !important;
}

/* Fix Autofill Background */
body:not(.body--dark) input:-webkit-autofill,
body:not(.body--dark) input:-webkit-autofill:hover,
body:not(.body--dark) input:-webkit-autofill:focus,
body:not(.body--dark) input:-webkit-autofill:active {
  -webkit-box-shadow: 0 0 0 1000px white inset !important;
  -webkit-text-fill-color: black !important;
  caret-color: black !important;
  background-color: white !important;
  color: black !important;
}

body:not(.body--dark) .checkbox-label {
  color: black !important;
}

body:not(.body--dark) .custom-checkbox {
  border-color: #a89c8d;
  background-color: white;
}

body:not(.body--dark) .error-msg {
  color: #c10015 !important;
}
</style>
