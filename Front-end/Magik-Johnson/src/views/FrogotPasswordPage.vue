<script setup>
import { ref } from 'vue'
import { UserForgotPassword } from '@/api/authentication/AuthenticationApi'
import { useRouter } from 'vue-router'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { validateEmail } from '@/validators/registerValidator'

const router = useRouter()

const email = ref('')

const emailErrorMessage = ref('')
const emailError = ref(false)
const errorMessage = ref('')

const emailVerified = () => {
  return validateEmail(email, emailError, emailErrorMessage)
}

function dataLoginValided() {
  return emailVerified()
}

const handleForgotPwd = async () => {
  if (dataLoginValided()) {
    try {
      Loading.show({
        message: 'Envoi du code réinitialisation ...',
        spinnerColor: 'primary',
      })

      await UserForgotPassword({ email: email.value })

      router.push('/ResetCodeSent')
      Notify.create({
        type: 'positive',
        message: 'Le code de rénitialisation a été envoyé. Vérifier dans vos courriels.',
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
      <h3 class="forgotPwd_title">Mot de passe oublié</h3>
      <p class="errorMessage">{{ errorMessage }}</p>

      <form @submit.prevent="handleForgotPwd" class="form-lg form">
        <div class="mb-4">
          <label class="form-label text-adaptive" for="email">Adresse courriel</label>
          <input
            type="email"
            id="email"
            autocomplete="username"
            class="form-control"
            v-model="email"
            @blur="emailVerified"
            :class="{ 'email-error': emailError }"
          />
          <span class="emailErrorMessage">{{ emailErrorMessage }}</span>
        </div>
        <button type="submit" class="b btn-lg w-100 rounded-3 shadow-sm mt-4 mb-3 btn-forgotPwd">
          Envoyer le code
        </button>

        <div class="mt-3 text-center small">
          <p class="text-adaptive">
            <router-link to="/Connexion" class="fw-bold text-decoration-none back-to-login"
              >Revenir à la page de connexion</router-link
            >
          </p>

          <p class="text-adaptive">
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

<style scoped>
body,
html {
  overflow-x: hidden;
}

.forgotPwd {
  margin-top: 100px !important;
  margin-left: 2%;
  margin-bottom: 100px;
}

.forgotPwd_title {
  margin-bottom: 6%;
  color: #ba9461;
  font-weight: 500;
}

.form-control {
  border-radius: 4px;
  padding: 8px 12px;
  font-weight: 500;
  width: 100%;
}

/* Light Mode  */
body:not(.body--dark) .form-control {
  background-color: white;
  color: black;
  border: 1px solid #ccc;
}

/* Dark Mode  */
body.body--dark .form-control {
  background-color: #455a64;
  color: white;
  border: 1px solid #455a64;
}

/* Autofill Hack for Dark Mode */
body.body--dark input:-webkit-autofill,
body.body--dark input:-webkit-autofill:hover,
body.body--dark input:-webkit-autofill:focus,
body.body--dark input:-webkit-autofill:active {
  -webkit-box-shadow: 0 0 0 30px #455a64 inset !important;
  -webkit-text-fill-color: white !important;
}

.emailErrorMessage,
.errorMessage {
  color: red !important;
}

.page-forgotPwd {
  display: flex;
  flex-wrap: wrap;
  min-height: 100vh;
  padding: 15px;
  width: 100%;
  margin: 0;
}

/* Light Mode Background */
body:not(.body--dark) .page-forgotPwd {
  background-color: #d1cbbb;
  color: black;
}

/* Dark Mode Background */
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
  background-color: #cde8fd;
  border: #cde8fd;
  color: black;
  font-weight: bold;
}
.btn-forgotPwd:hover {
  background-color: #ba9461;
}

.back-to-login,
.no-account {
  color: lightgray;
}

body:not(.body--dark) .back-to-login,
body:not(.body--dark) .no-account {
  color: #000000;
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

#email {
  display: block;
  width: 100%;
  padding: 10px 12px;
  font-size: 1rem;
  line-height: 1.5;
  color: #000000;
  background-color: #ffffff;
  background-clip: padding-box;
  border: 1px solid #ced4da;
  border-radius: 0.25rem;
  transition:
    border-color 0.15s ease-in-out,
    box-shadow 0.15s ease-in-out;
  margin-top: 5px;
}

#email:focus {
  color: #000000;
  background-color: #ffffff;
  border-color: #86b7fe;
  outline: 0;
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}

body.body--dark #email {
  background-color: #ffffff;
  color: #000000;
  border: 1px solid #ced4da;
}

.email-error {
  border: 2px solid red !important;
}
</style>
