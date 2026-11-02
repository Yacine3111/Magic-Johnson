<script setup>
import { ref, onMounted } from 'vue'
import VueCookies from 'vue-cookies'
import { UserResetPassword } from '@/api/authentication/AuthenticationApi'
import { useRouter } from 'vue-router'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { validatePassword } from '@/validators/registerValidator'
import { resetPasswordRequest } from '@/models/resetPasswordRequest'

const router = useRouter()

const password = ref('')
const pwdConfirmation = ref('')

const pwdConfirmationErrorMessage = ref('')
const pwdConfirmationError = ref(false)
const pwdConfirmationInputType = ref('password')
const ispwdConfirmationHide = ref(true)

const passwordErrorMessage = ref('')
const passwordError = ref(false)
const errorMessage = ref('')
const passwordInputType = ref('password')
const isPwdHide = ref(true)

var email
var resetCode

onMounted(() => {
  var token = VueCookies.get('resetCode')
  if (token == null) {
    router.push('/')
  } else {
    resetCode = token
    email = VueCookies.get('email')
  }
})

function showPassword() {
  if (passwordInputType.value == 'password') {
    isPwdHide.value = false
    passwordInputType.value = 'Text'
  } else if (passwordInputType.value == 'Text') {
    isPwdHide.value = true
    passwordInputType.value = 'password'
  }
}

function showPwdConfirmation() {
  if (pwdConfirmationInputType.value == 'password') {
    ispwdConfirmationHide.value = false
    pwdConfirmationInputType.value = 'Text'
  } else if (pwdConfirmationInputType.value == 'Text') {
    ispwdConfirmationHide.value = true
    pwdConfirmationInputType.value = 'password'
  }
}

const passwordVerified = () => {
  return validatePassword(password, passwordError, passwordErrorMessage)
}

const pwdConfirmationVerified = () => {
  if (pwdConfirmation.value.trim() === '') {
    pwdConfirmationErrorMessage.value = 'Mot de passe de confirmation requis.'
    pwdConfirmationError.value = true
    return false
  } else {
    pwdConfirmationErrorMessage.value = ''
    pwdConfirmationError.value = false
    checkPwdEqualPwdConfirmation()
    return true
  }
}

const checkPwdEqualPwdConfirmation = () => {
  if (password.value != pwdConfirmation.value) {
    errorMessage.value = 'les deux mots de passe ne correspondent pas.'
    return false
  } else {
    errorMessage.value = ''
    return true
  }
}

function dataLoginValided() {
  //Vérifier que tout est valide
  return passwordVerified() && pwdConfirmationVerified() && checkPwdEqualPwdConfirmation()
}

const handleForgotPwd = async () => {
  if (dataLoginValided()) {
    try {
      //Affichier un loading en attendant la réponse
      Loading.show({
        message: 'Réinitialisation en cours ...',
        spinnerColor: 'primary',
      })

      //Appel à l'api

      const response = await UserResetPassword(
        new resetPasswordRequest(email, resetCode, password.value),
      )

      if (!(response.status >= 200 && response.status < 300)) {
        //SI pas réussi, envoyer un notification pour informer l'utilisateur
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

        return
      }

      //Si tout est réussi, pousser l'utilisateur à la page de connexion
      //Retirer le resetcode et l'émail
      VueCookies.remove('email')
      VueCookies.remove('resetCode')
      router.push('/connexion')

      Notify.create({
        type: 'positive',
        message: 'Mot de passe réinitialisé avec succès.',
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
    } catch (e) {
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
    } finally {
      Loading.hide()
    }
  }
}
</script>

<template>
  <div class="row justify-content-start page-forgotPwd">
    <div class="col-6 col-sm-6 col-md- col-lg-6 forgotPwd">
      <h3 class="forgotPwd_title">Nouveau mot de passe</h3>

      <form @submit.prevent="handleForgotPwd" class="form-lg form">
        <!-- Password -->
        <div class="mb-3">
          <label class="form-label text-adaptive" for="password">Mot de passe</label>
          <div class="input-group shadow-sm w-100">
            <input
              id="password"
              class="form-control"
              :class="{ 'password-error': passwordError }"
              v-model="password"
              @blur="passwordVerified"
              :type="passwordInputType"
            />
            <button
              class="btn btn-outline-secondary"
              @click="showPassword"
              type="button"
              id="eye"
              aria-label="Afficher ou masquer le mot de passe"
            >
              <i :class="{ 'bi bi-eye': !isPwdHide, 'bi bi-eye-slash': isPwdHide }"></i>
            </button>
          </div>
          <div v-if="passwordErrorMessage" class="text-danger small mt-1 text-center">
            {{ passwordErrorMessage }}
          </div>
        </div>

        <!-- Password Confirmation-->
        <div class="mb-3">
          <label class="form-label text-adaptive label-pwdConfirmation" for="pwdConfirmation"
            >Confirmer le mot de passe</label
          >
          <div class="input-group shadow-sm w-100">
            <input
              id="pwdConfirmation"
              class="form-control"
              :class="{ 'password-error': passwordError }"
              v-model="pwdConfirmation"
              @blur="pwdConfirmationVerified"
              :type="pwdConfirmationInputType"
            />
            <button
              class="btn btn-outline-secondary"
              @click="showPwdConfirmation"
              type="button"
              id="eye"
              aria-label="Afficher ou masquer le mot de passe"
            >
              <i
                :class="{
                  'bi bi-eye': !ispwdConfirmationHide,
                  'bi bi-eye-slash': ispwdConfirmationHide,
                }"
              ></i>
            </button>
          </div>
          <div
            v-if="pwdConfirmationErrorMessage || errorMessage"
            class="text-danger small mt-1 text-center"
          >
            {{ pwdConfirmationErrorMessage || errorMessage }}
          </div>
        </div>

        <button type="submit" class="b btn-lg w-100 rounded-3 shadow-sm mt-4 mb-3 btn-forgotPwd">
          Renouveler le mot de passe
        </button>

        <!-- Links -->
        <div class="mt-3 text-center small">
          <p class="text-adaptive link-container">
            <router-link to="/Connexion" class="fw-bold text-decoration-none back-to-login"
              >Revenir à la page de connexion</router-link
            >
          </p>

          <p class="text-adaptive link-container">
            <router-link to="/Inscription" class="fw-bold text-decoration-none no-account"
              >S'inscrire</router-link
            >
          </p>
        </div>
      </form>
    </div>

    <div class="col-1 col-sm-1 col-md-1 col-lg-1 vertical-line"></div>

    <div
      class="col-4 col-sm-4 col-md-4 col-lg-4 row justify-content-center align-content-center img-presentation"
    >
      <img class="img-logo" src="@/assets/image_logo.png" />
    </div>
  </div>
</template>

<style>
body,
html {
  overflow-x: hidden;
  height: 100vh;
}

.form-label {
  font-size: large;
}

.forgotPwd {
  margin-top: 50px !important;
  margin-left: 2%;
  margin-bottom: 50px;
}

.forgotPwd_title {
  margin-bottom: 6%;
  color: #ba9461;
  font-weight: 500;
}

#pwdConfirmation,
#password {
  text-align: center;
  font-weight: 500;
  border-radius: 4px;
}

/* Light Mode  */
body:not(.body--dark) #pwdConfirmation,
body:not(.body--dark) #password {
  background-color: white;
  color: black;
  border: 1px solid #ccc;
}

/* Dark Mode Input */
body.body--dark #pwdConfirmation,
body.body--dark #password {
  background-color: #a3b8ca;
  background-color: #455a64;
  color: white;
  border: 1px solid #455a64;
}

.page-forgotPwd {
  display: flex;
  flex-wrap: wrap;
  height: 100vh;
  padding: 15px;
  width: 100%;
  margin: 0;
}

/* Light Mode Page Background */
body:not(.body--dark) .page-forgotPwd {
  background-color: #d1cbbb;
  color: black;
}

/* Dark Mode Page Background */
body.body--dark .page-forgotPwd {
  background-color: #2d3c49;
  color: white;
}

body:not(.body--dark) .text-adaptive {
  color: black !important;
}
body.body--dark .text-adaptive {
  color: white !important;
}

.btn-forgotPwd {
  background-color: #ba9461;
  border: #cde8fd;
  color: black;
  font-weight: bold;
}
.btn-forgotPwd:hover {
  background-color: rgb(186, 148, 97, 0.8);
}

.back-to-login,
.no-account {
  color: lightgray;
}

body:not(.body--dark) .back-to-login,
body:not(.body--dark) .no-account {
  color: #000;
}

.back-to-login:hover,
.no-account:hover {
  color: #ba9461;
}

@media only screen and (max-width: 1024px) {
  .vertical-line,
  .img-presentation {
    display: none !important;
  }
  .forgotPwd {
    width: 80% !important;
    margin-left: 10%;
    margin-right: 10%;
  }
}

@media only screen and (max-width: 726px) {
  .forgotPwd_title {
    font-size: 35px;
  }
}

@media only screen and (max-width: 540px) {
  .forgotPwd_title {
    font-size: 30px;
    font-weight: bold;
  }
}
.vertical-line {
  margin-left: 3%;
  margin-top: 50px !important;
  height: 100vh;
}

body:not(.body--dark) .vertical-line {
  border-left: 6px solid #a89c8d;
}

body.body--dark .vertical-line {
  border-left: 6px solid #f0eada;
}

.password-error {
  border-left: 3px solid #ff0000 !important;
  border-radius: 5px 5px 0 0 !important;
}

body.body--dark .page-forgotPwd .text-danger {
  color: #ff6b6b !important;
}
</style>
