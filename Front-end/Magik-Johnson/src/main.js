import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

import VueTheMask from 'vue-the-mask'
import App from './App.vue'
import router from './router'
import VueCookies from 'vue-cookies'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

import { Quasar, Loading, Notify, Dialog } from 'quasar'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'
import '@quasar/extras/mdi-v7/mdi-v7.css'

import 'bootstrap-icons/font/bootstrap-icons.css'

const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

app.use(VueTheMask)
app.use(pinia)
app.use(router)
app.use(VueCookies)
app.use(Quasar, {
  plugins: {
    Loading,
    Notify,
    Dialog,
  },
})

app.mount('#app')
