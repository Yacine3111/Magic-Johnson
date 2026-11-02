<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Loading } from 'quasar'

const router = useRouter()

const resendResetCode = () => {
  Loading.show({
    message: 'Chargement ...',
    spinnerColor: 'primary',
  })
  router.push('/forgotPassword')
  Loading.hide()
}
</script>

<template>
  <div class="row justify-content-start page-pwdResetCode">
    <div class="col-6 col-sm-6 col-md- col-lg-6 pwdResetCode">
      <h3 class="pwdResetCode_title">Réinitialisation de mot de passe</h3>

      <div class="container d-flex justify-content-center align-items-center mt-5">
        <div class="card shadow-lg border-0 rounded-4 p-4 text-center" style="max-width: 500px">
          <div class="mb-3">
            <i class="bi bi-envelope-check text-success" style="font-size: 3rem"></i>
          </div>
          <h4 class="fw-bold text-success">Vérifiez votre courriel</h4>
          <p class="text-muted">
            Si votre adresse est enregistrée chez nous, un
            <strong>lien de réinitialisation du mot de passe</strong>
            vous a été envoyé. Veuillez vérifier votre boîte de réception (et le dossier de courrier
            indésirable).
          </p>
        </div>
      </div>

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
  height: 100vh;
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

.page-pwdResetCode {
  display: flex;
  flex-wrap: wrap;
  height: 100vh;
  padding: 15px;
  width: 100%;
  margin: 0;
}

/* Light Mode Background */
body:not(.body--dark) .page-pwdResetCode {
  background-color: #d1cbbb;
  color: black;
}

/* Dark Mode Background */
body.body--dark .page-pwdResetCode {
  background-color: #2d3c49;
  color: white;
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
  .pwdResetCode {
    width: 80% !important;
    margin-left: 10%;
    margin-right: 10%;
  }
}

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
  height: 100vh;
}

body:not(.body--dark) .vertical-line {
  border-left: 6px solid #a89c8d;
}

body.body--dark .vertical-line {
  border-left: 6px solid #f0eada;
}

.resetCode-error {
  border: red 2.5px solid !important;
}
</style>
