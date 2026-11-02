<script setup>
import { ref, onMounted } from 'vue'
import { UserCheckResetCode } from '@/api/authentication/AuthenticationApi'
import { useRouter, useRoute } from 'vue-router'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import VueCookies from 'vue-cookies'
import { resetCodeCheckRequest } from '@/models/resetCodeCheckRequest'

const email = ref('')
const resetCode = ref('')

const router = useRouter()
const route = useRoute()

onMounted(() => {
  email.value = route.query.email || ''
  resetCode.value = route.query.resetCode || ''

  if (email.value && resetCode.value) {
    VueCookies.set('resetCode', resetCode.value, '30min')
    VueCookies.set('email', email.value, '30min')

    handleResetCodeLoading()
  } else {
    router.push('/')
    Notify.create({
      type: 'negative',
      message: 'Nous sommes désolés, une erreur est survenue.',
      position: 'top',
      actions: [
        { icon: 'close', color: 'white', round: true, dense: true, style: { display: 'flex', alignItems: 'center', justifyContent: 'center' }, handler: () => { } }
      ]
    })
  }
})

const handleResetCodeLoading = async () => {
  try {
    Loading.show({
      message: 'Vérification ...',
      spinnerColor: 'primary',
    })

    await UserCheckResetCode(new resetCodeCheckRequest(email.value, resetCode.value))

    router.push('/NewPassword')

    Notify.create({
      type: 'positive',
      message: 'Entrer un nouveau mot de passe.',
      position: 'top',
      actions: [
        { icon: 'close', color: 'white', round: true, dense: true, style: { display: 'flex', alignItems: 'center', justifyContent: 'center' }, handler: () => { } }
      ]
    })
  } catch (e) {
    router.push('/')
    Notify.create({
      type: 'negative',
      message: 'Nous sommes désolés, une erreur est survenue.',
      position: 'top',
      actions: [
        { icon: 'close', color: 'white', round: true, dense: true, style: { display: 'flex', alignItems: 'center', justifyContent: 'center' }, handler: () => { } }
      ]
    })
  } finally {
    Loading.hide()
  }
}
</script>

<template>
  <div class="row justify-content-start page-pwdResetCode">
    <div class="col-6 col-sm-6 col-md- col-lg-6 pwdResetCode">
      <h3 class="pwdResetCode_title">Réinitialisation de mot de passe</h3>
      <p class="errorMessage">{{ errorMessage }}</p>

      <!-- Links -->
      <div class="mt-3 text-center small">
        <p class="text-light">
          <a @click="resendResetCode" class="fw-bold text-decoration-none no-account"
            >Renvoyer le code de confirmation</a
          >
        </p>
        <p class="text-light">
          <router-link to="/Connexion" class="fw-bold text-decoration-none back-to-login"
            >Revenir à la page de connexion</router-link
          >
        </p>
      </div>
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
  /* background-color: #2d3c49; */
}

.label-resetCode {
  font-size: large;
}

.pwdResetCode {
  margin-top: 100px !important;
  margin-left: 2%;
  margin-bottom: 100px;
}

.pwdResetCode_title {
  margin-bottom: 6%;
  color: #ba9461;
  font-weight: 500;
}
.resetCode {
  background-color: #a3b8ca;
  text-align: center;
  border: #a3b8ca;
  resize: none;
  background-color: #c7d6e3;
  text-align: center;
  border: #a3b8ca;
  font-size: large !important;
  font-weight: 500;
  font-family: Arial, Helvetica, sans-serif;
}

.resetCodeErrorMessage,
.errorMessage {
  color: red;
  height: 10px;
}

.page-pwdResetCode {
  background-color: #2d3c49;
  display: flex;
  flex-wrap: wrap;
  height: 100vh;
  padding: 15px;
}
.btn-continue {
  background-color: #ba9461;
  border: #cde8fd;
  color: black;
  font-weight: bold;
}
.btn-continue:hover {
  background-color: rgb(186, 148, 97, 0.8);
}

.back-to-login,
.no-account {
  color: lightgray;
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
  .pwdResetCode {
    width: 80% !important;
    margin-left: 10%;
    margin-right: 10%;
  }
}

/* On diminue la taille du titre à partir d'une certaine taille de l'écran pour l'esthétique */
@media only screen and (max-width: 726px) {
  .pwdResetCode_title {
    font-size: 35px;
  }
}

@media only screen and (max-width: 540px) {
  .pwdResetCode_title {
    font-size: 30px;
    font-weight: bold;
  }
}
.vertical-line {
  margin-left: 3%;
  margin-top: 50px !important;
  border-left: 6px solid #f0eada;
  height: 100vh;
}
.resetCode-error {
  border: red 2.5px solid !important;
}
</style>
