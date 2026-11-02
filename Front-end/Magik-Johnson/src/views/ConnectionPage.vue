<script setup>
import { ref } from 'vue'
import { UserLogin } from '@/api/authentication/AuthenticationApi'
import { useAuthStore } from '@/stores/authStore'
import { useCartStore } from '@/stores/cartStore'
import { useRouter } from 'vue-router'
import { Loading, Notify, useQuasar } from 'quasar'
import VueCookies from 'vue-cookies'
import { validateEmail, validatePasswordForLogin } from '@/validators/registerValidator'
const $q = useQuasar()

const email = ref('')
const password = ref('')
const passwordInputType = ref('password')
const emailErrorMessage = ref('')
const emailError = ref(false)
const passwordError = ref(false)
const passwordErrorMessage = ref('')
const rememberMe = ref(false)
const isPwdHide = ref(true)

const auth = useAuthStore()
const cartStore = useCartStore()
const router = useRouter()

function showPassword() {
  if (passwordInputType.value == 'password') {
    passwordInputType.value = 'text'
    isPwdHide.value = false
  } else if (passwordInputType.value == 'text') {
    passwordInputType.value = 'password'
    isPwdHide.value = true
  }
}

const passwordVerified = () => {
  return validatePasswordForLogin(password, passwordError, passwordErrorMessage)
}

const emailVerified = () => {
  return validateEmail(email, emailError, emailErrorMessage)
}

function dataLoginValided() {
  return emailVerified() && passwordVerified()
}

const handleLogin = async () => {
  if (dataLoginValided()) {
    try {
      Loading.show({
        message: 'Connexion en cours...',
        spinnerColor: 'primary',
      })

      const response = await UserLogin({
        email: email.value,
        password: password.value,
      })

      //Si se rappeler est coché, stockées l'email et le mot de passe 7 jours, sinon les retirer si déjà existantes
      if (rememberMe.value) {
        VueCookies.set('rememberMe', rememberMe.value, '7d')
        VueCookies.set('email', email.value, '7d')
        VueCookies.set('password', password.value, '7d')
      } else {
        VueCookies.remove('rememberMe')
        VueCookies.remove('email')
        VueCookies.remove('password')
      }

      localStorage.setItem('accessToken', response.accessToken)
      localStorage.setItem('refreshToken', response.refreshToken)

      await auth.fetchUser()

      // Synchroniser le panier avec le backend après la connexion
      try {
        await cartStore.syncWithBackend()
      } catch (error) {
        console.warn('Erreur lors de la synchronisation du panier:', error)
      }

      router.push('/')
    } catch (e) {
      Notify.create({
        type: 'negative',
        message:
          "Nous sommes désolés, l'adresse de courriel et le mot de passe ne correspondent pas. Veuillez essayer une autre combinaison. ",
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
      passwordError.value = true
    } finally {
      Loading.hide()
    }
  }
}
</script>

<template>
  <div class="row justify-content-start page-connexion">
    <div class="col-6 col-sm-6 col-md- col-lg-6 connexion">
      <h2 class="connexion_title">Connexion</h2>
      <form @submit.prevent="handleLogin" class="form-lg form">
        <!-- Email -->
        <div class="mb-4">
          <label class="form-label text-adaptive" for="email">Adresse courriel</label>
          <input
            type="email"
            id="email"
            autocomplete="username"
            class="form-control shadow-sm"
            v-model="email"
            @blur="emailVerified"
            :class="{ 'email-error': emailError }"
          />
          <span class="emailErrorMessage">{{ emailErrorMessage }}</span>
        </div>

        <!-- Password -->
        <div class="mb-3">
          <label class="form-label text-adaptive" for="password">Mot de passe</label>
          <div class="input-group text-adaptive shadow-sm w-100">
            <input
              id="password"
              autocomplete="current-password"
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
          <span class="passwordErrorMessage">{{ passwordErrorMessage }}</span>
        </div>

        <!-- Se rappeller de moi -->
        <div class="mb-3 form-check">
          <input type="checkbox" class="form-check-input" id="rememberMe" v-model="rememberMe" />
          <label class="form-check-label text-adaptive rememberMe" for="rememberMe">
            Se souvenir de moi
          </label>
        </div>

        <button type="submit" class="b btn-lg w-100 rounded-3 shadow-sm mb-3 btn-login">
          Se connecter
        </button>

        <!-- Links -->
        <div class="mt-3 text-center small">
          <p class="text-adaptive">
            Pas encore de compte ?
            <router-link to="/Inscription" class="fw-bold text-decoration-none no-account"
              >S'inscrire</router-link
            >
          </p>
          <p class="text-adaptive">
            Mot de passe oublié ?
            <router-link to="/forgotpassword" class="fw-bold text-decoration-none forgot-password"
              >Réinitialiser</router-link
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
}

.no-outline {
  width: min-content !important;
}

.rememberMe {
  width: max-content !important;
}

.connexion {
  margin-top: 100px !important;
  margin-left: 2%;
  margin-bottom: 100px;
}

.connexion_title {
  margin-bottom: 7%;
  color: #ba9461;
  font-weight: bold;
}
#email,
#password,
#eye {
  text-align: center;
  font-weight: bold;
  font-size: large !important;
}

/* Light Mode inputs */
body:not(.body--dark) #email,
body:not(.body--dark) #password,
body:not(.body--dark) #eye {
  background-color: white;
  color: black;
  border: 1px solid #ccc;
}

/* Dark Mode Inputs */
body.body--dark #email,
body.body--dark #password,
body.body--dark #eye {
  background-color: #a3b8ca;
  color: black;
  border: #a3b8ca;
}
.passwordErrorMessage,
.emailErrorMessage,
.errorMessage,
body.body--dark .passwordErrorMessage,
body.body--dark .emailErrorMessage,
body.body--dark .errorMessage {
  color: red !important;
}
#eye {
  border: none;
  -webkit-text-stroke: 1px;
}
.page-connexion {
  display: flex;
  flex-wrap: wrap;
  min-height: 100vh;
  padding: 0;
  width: 100%;
  margin: 0;
}

/* Light Mode Background */
body:not(.body--dark) .page-connexion {
  background-color: #d1cbbb;
  color: black;
}

/* Dark Mode Background */
body.body--dark .page-connexion {
  background-color: #2d3c49;
  color: white;
}

body.body--dark {
  background-color: #2d3c49 !important;
}

body:not(.body--dark) {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .text-adaptive {
  color: black !important;
}

body.body--dark .text-adaptive {
  color: white !important;
}
.btn-login {
  background-color: #cde8fd;
  border: #cde8fd;
  color: black;
  font-weight: bold;
}
.btn-login:hover {
  background-color: #ba9461;
}

.forgot-password,
.no-account {
  color: lightgray;
}

body:not(.body--dark) .forgot-password,
body:not(.body--dark) .no-account {
  color: #000000;
}

.forgot-password:hover,
.no-account:hover {
  color: #ba9461;
}
@media only screen and (max-width: 1024px) {
  .vertical-line,
  .img-presentation {
    display: none !important;
  }
  .connexion {
    width: 80% !important;
    margin-left: 10%;
    margin-right: 10%;
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
.email-error,
.password-error {
  border: 2px solid red !important;
  border-radius: 5px !important;
}

#rememberMe {
  transform: scale(1.3);
  margin-right: 10px;
  cursor: pointer;
}

.form-check-label {
  cursor: pointer;
  font-size: 1.1em;
  padding-left: 5px;
}

.form-check-input:checked {
  background-color: #1976d2 !important;
  border-color: #1976d2 !important;
}

.form-check-input:focus {
  border-color: #1976d2;
  box-shadow: 0 0 0 0.25rem rgba(25, 118, 210, 0.25);
}
</style>
