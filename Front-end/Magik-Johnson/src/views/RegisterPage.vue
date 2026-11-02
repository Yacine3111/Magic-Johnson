<script setup>
import { UserRegistration } from '@/api/authentication/AuthenticationApi'
import { ClientRegistrationRequest } from '@/models/connection/AuthenticationRequests'
import {
  validateFirstName,
  validateName,
  validateEmail,
  validatePhoneNumber,
  validatePassword,
  validateConfirmationPassword,
} from '@/validators/registerValidator'
import router from '@/router'
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { Loading, Notify, useQuasar } from 'quasar'

const $q = useQuasar()
const firstName = ref('')
const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const password = ref('')
const passwordConfirmation = ref('')
const isPwd = ref(true)
const isPwdConfirmation = ref(true)

const firstnameError = ref(false)
const nameError = ref(false)
const phoneNumberError = ref(false)
const emailError = ref(false)
const passwordError = ref(false)
const passwordConfirmationError = ref(false)

const firstnameErrorMessage = ref('')
const nameErrorMessage = ref('')
const emailErrorMessage = ref('')
const phoneNumberErrorMessage = ref('')
const passwordErrorMessage = ref('')
const passwordConfirmationErrorMessage = ref('')

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

function dataSigninValided() {
  const isFirstnameValid = firstnameVerified()
  const isNameValid = nameVerified()
  const isPhoneNumberValid = phoneNumberVerified()
  const isEmailValid = emailVerified()
  const isPasswordValid = passwordVerified()
  const isPasswordConfirmationValid = passwordConfirmationVerified()

  return (
    isFirstnameValid &&
    isNameValid &&
    isPhoneNumberValid &&
    isEmailValid &&
    isPasswordValid &&
    isPasswordConfirmationValid
  )
}

const signin = async () => {
  if (dataSigninValided()) {
    try {
      Loading.show({ spinnerColor: 'primary' })
      const clientRegistrationRequest = new ClientRegistrationRequest(
        firstName.value,
        name.value,
        email.value,
        phoneNumber.value,
        password.value,
        passwordConfirmation.value,
      )
      const registrationSuccess = await UserRegistration(clientRegistrationRequest)

      if (registrationSuccess) {
        router.push('/Connexion')
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
      Notify.create({
        type: 'negative',
        message: 'Une erreur est survenue!',
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
}
</script>

<template>
  <div class="row page-inscription">
    <div class="col-12 col-sm-10 col-md-6 col-lg-6 inscription">
      <h2 class="inscription_title">Inscription</h2>
      <form @submit.prevent="signin" class="form-lg form">
        <div class="row mb-3">
          <div class="col-6 col-sm-6 col-md-6 col-lg-6">
            <label class="form-label text-adaptive" for="firstname">Prénom</label>
            <input
              id="firstname"
              type="text"
              class="form-control"
              v-model="firstName"
              @blur="firstnameVerified"
              :class="{ 'firstname-error': firstnameError }"
            />
            <span class="firstnameErrorMessage">{{ firstnameErrorMessage }} </span>
          </div>
          <div class="col-6 col-sm-6 col-md-6 col-lg-6">
            <label class="form-label text-adaptive" for="name">Nom</label>
            <input
              id="name"
              type="text"
              class="form-control"
              v-model="name"
              @blur="nameVerified"
              :class="{ 'name-error': nameError }"
            />
            <span class="nameErrorMessage">{{ nameErrorMessage }} </span>
          </div>
        </div>
        <div class="mb-3">
          <label class="form-label text-adaptive" for="email">Adresse courriel</label>
          <input
            id="email"
            type="email"
            autocomplete="username"
            class="form-control"
            v-model="email"
            @blur="emailVerified"
            :class="{ 'email-error': emailError }"
          />
          <span class="emailErrorMessage">{{ emailErrorMessage }} </span>
        </div>
        <div class="mb-3">
          <label class="form-label text-adaptive" for="phoneNumber">Numéro de téléphone</label>
          <input
            id="phoneNumber"
            type="tel"
            class="form-control"
            v-model="phoneNumber"
            @blur="phoneNumberVerified"
            :class="{ 'phoneNumber-error': phoneNumberError }"
            v-mask="'(###) ### - ####'"
          />
          <span class="phoneNumberErrorMessage">{{ phoneNumberErrorMessage }} </span>
        </div>
        <div class="mb-3">
          <label class="form-label text-adaptive" for="password">Mot de passe</label>
          <div class="input-group">
            <input
              id="password"
              :type="isPwd ? 'password' : 'text'"
              class="form-control"
              v-model="password"
              placeholder="********"
              @blur="passwordVerified"
              :class="{ 'password-error': passwordError }"
            />
            <button class="btn btn-outline-secondary btn-eye" type="button" @click="isPwd = !isPwd">
              <i :class="isPwd ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
            </button>
          </div>
          <span class="passwordErrorMessage"> {{ passwordErrorMessage }}</span>
        </div>
        <div class="mb-4">
          <label class="form-label text-adaptive" for="passwordConfirmation"
            >Confirmer mot de passe</label
          >
          <div class="input-group">
            <input
              id="passwordConfirmation"
              :type="isPwdConfirmation ? 'password' : 'text'"
              class="form-control"
              v-model="passwordConfirmation"
              placeholder="********"
              @blur="passwordConfirmationVerified"
              :class="{ 'passwordConfirmation-error': passwordConfirmationError }"
            />
            <button
              class="btn btn-outline-secondary btn-eye"
              type="button"
              @click="isPwdConfirmation = !isPwdConfirmation"
            >
              <i :class="isPwdConfirmation ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
            </button>
          </div>
          <span class="passwordConfirmationErrorMessage">
            {{ passwordConfirmationErrorMessage }}</span
          >
        </div>
        <button type="submit" class="btn btn-lg w-100 rounded-3 shadow-sm mb-3 btn-register">
          S'inscrire
        </button>
        <div class="mt-5 text-center small">
          <p class="text-adaptive mb-0">
            Vous avez déjà un compte ?
            <RouterLink to="/Connexion" class="text-primary text-decoration-none no-account"
              >Se connecter</RouterLink
            >
          </p>
        </div>
      </form>
    </div>
    <div class="col-1 col-sm-1 col-md-1 col-lg-1 vertical-line"></div>
    <div
      class="col-6 col-sm-4 col-md-4 col-lg-4 d-none d-md-block row justify-content-center align-content-center img-presentation"
    >
      <img class="img-logo" src="@/assets/Logo/logo_arene.png" />
    </div>
  </div>
</template>

<style src="../css/RegisterPage.css" scoped></style>
