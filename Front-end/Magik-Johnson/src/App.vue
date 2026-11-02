<script setup>
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { onBeforeMount, onMounted } from 'vue'
import { onUpdated } from 'vue'
import { useAuthStore } from './stores/authStore'
import { useCartStore } from './stores/cartStore'
import { UserLogout } from './api/authentication/AuthenticationApi'
import { Loading } from 'quasar'
import { Notify } from 'quasar'
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { ref } from 'vue'
import ThemeToggle from './components/ThemeToggle.vue'

const $q = useQuasar()
const auth = useAuthStore()
const role = ref('')
const cart = useCartStore()

const router = useRouter()

onMounted(async () => {
  if (auth.user != null) {
    await auth.fetchUser()
    await hasPermission()

    try {
      const backendItems = await cart.loadCartFromBackend()
      if (backendItems && backendItems.length > 0) {
        cart.items = backendItems
        cart.saveToLocalStorage()
      }
      cart.isSyncedWithBackend = true
    } catch (error) {
      console.warn('Impossible de charger le panier depuis le backend:', error)
      cart.isSyncedWithBackend = false
    }
  } else {
    cart.isSyncedWithBackend = false
  }
})

async function hasPermission() {
  if (auth.user != null) {
    await auth.fetchRole()
    try {
      role.value = auth.role[0]
    } catch (e) {}
  }
}

const Logout = async () => {
  try {
    Loading.show({
      spinnerColor: 'primary',
    })
    await UserLogout()
    role.value = ''
    auth.clearUser()
    cart.clearCart(false)
    cart.isSyncedWithBackend = false
    router.push('/')
    Notify.create({
      type: 'positive',
      message: 'Vous avez été déconnecté avec succès',
      position: 'top',
      actions: [{ icon: 'close', color: 'white' }],
    })
  } catch (e) {
    Notify.create({
      type: 'negative',
      message: 'Nous sommes désolés, une erreur est survenue.',
      position: 'top',
    })
  } finally {
    Loading.hide()
  }
}

const activeRoute = useRoute()
const isGestionMagasin = computed(() => {
  return (
    activeRoute.name == 'Promotions' ||
    activeRoute.name == 'Inventaire' ||
    activeRoute.name == 'Employes' ||
    activeRoute.name == 'Clients' ||
    activeRoute.name == 'Add-employee' ||
    activeRoute.name == 'Manage-employee' ||
    activeRoute.name == 'gestionCommandes' ||
    activeRoute.name == 'gestionDemandesRemboursement'
  )
})

const headerClasses = computed(() => {
  return [
    'header',
    'd-flex',
    'flex-row',
    'items-center',
    'justify-between',
    'col-12',
    'col-sm-12',
    'col-md-12',
    'col-lg-12',
    'nav-page',
    $q.screen.lt.md ? '' : 'q-px-md',
  ]
})

const isDark = computed({
  get: () => $q.dark.isActive,
  set: (val) => $q.dark.set(val),
})
</script>

<template>
  <q-layout view="hHh lpR fff">
    <q-header :class="headerClasses">
      <RouterLink to="/" class="header_logo_link">
        <q-avatar class="header_logo_container">
          <img src="@/assets/Logo/header_logo.png" />
        </q-avatar>
      </RouterLink>

      <!-- Desktop -->
      <template v-if="$q.screen.gt.md">
        <div class="div_link text-center q-mt-lg">
          <RouterLink class="link q-mr-xl" to="/">Accueil</RouterLink>
          <RouterLink class="link q-mr-xl" to="/Catalogue">Catalogue</RouterLink>
          <RouterLink
            v-if="hasPermission() && (role == 'manager' || role == 'employee')"
            class="link"
            to="/GestionMagasin"
            >Magasin</RouterLink
          >
        </div>

        <div class="div_link text-center q-mt-lg">
          <div class="q-mr-lg flex items-center">
            <ThemeToggle v-model="isDark" />
          </div>
          <RouterLink class="link q-mr-lg" to="/panier">
            <q-btn flat round class="cart-button" aria-label="Panier">
              <q-icon size="lg" name="shopping_bag" />
              <q-badge
                v-if="cart.totalQuantity > 0"
                floating
                color="red"
                :label="cart.totalQuantity"
              />
            </q-btn>
          </RouterLink>
          <RouterLink
            v-if="auth.user"
            class="account-link"
            to=""
            @click="(e) => e.preventDefault()"
          >
            <q-btn flat class="account-button" aria-label="Menu compte">
              <div class="account-trigger">
                <q-icon name="person" size="lg" />
                <span class="account-trigger__text">Bonjour {{ auth.user.firstName }} !</span>
              </div>
              <q-menu class="account-menu" anchor="bottom right" self="top right">
                <q-list class="account-menu__list">
                  <q-item
                    clickable
                    v-close-popup
                    class="account-menu__item"
                    @click="router.push('/GestionCompte')"
                  >
                    <q-item-section class="account-menu__icon" side>
                      <q-icon name="person" />
                    </q-item-section>
                    <q-item-section class="account-menu__label">Mon compte</q-item-section>
                  </q-item>
                  <q-item
                    clickable
                    v-close-popup
                    class="account-menu__item account-menu__item--logout"
                    @click="Logout"
                  >
                    <q-item-section class="account-menu__icon" side>
                      <q-icon name="logout" />
                    </q-item-section>
                    <q-item-section class="account-menu__label">Déconnexion</q-item-section>
                  </q-item>
                </q-list>
              </q-menu>
            </q-btn>
          </RouterLink>
          <RouterLink v-else class="link" to="/Connexion"> Se connecter </RouterLink>
        </div>
      </template>

      <!-- Mobile et Tablette -->
      <div v-if="$q.screen.lt.lg" class="mobile-header">
        <div class="mobile-actions">
          <RouterLink class="link" to="/panier">
            <q-btn flat round dense class="cart-button" aria-label="Panier">
              <q-icon size="lg" name="shopping_bag" />
              <q-badge v-if="cart.totalQuantity > 0" color="red" :label="cart.totalQuantity" />
            </q-btn>
          </RouterLink>
          <q-btn flat round dense class="mobile-menu-button">
            <q-icon size="lg" name="menu" />
            <q-menu class="mobile-menu">
              <div class="mobile-menu-content">
                <div class="menu-item" @click="$router.push('/')">
                  <q-icon name="home" /> Accueil
                </div>
                <div class="menu-item" @click="$router.push('/Catalogue')">
                  <q-icon name="view_module" /> Catalogue
                </div>

                <div
                  v-if="hasPermission() && (role == 'manager' || role == 'employee')"
                  class="menu-item"
                  @click="$router.push('/GestionMagasin')"
                >
                  <q-icon name="store" /> Magasin
                </div>
                <div class="menu-item" @click="$router.push('/panier')">
                  <q-icon name="shopping_cart" />
                  Panier
                  <q-badge
                    v-if="cart.totalQuantity > 0"
                    color="red"
                    :label="cart.totalQuantity"
                    class="ml-2"
                  />
                </div>
                <div class="menu-item" @click="isDark = !isDark">
                  <q-icon :name="isDark ? 'light_mode' : 'dark_mode'" />
                  {{ isDark ? 'Mode Clair' : 'Mode Sombre' }}
                </div>
                <div
                  class="menu-item"
                  @click="auth.user ? $router.push('/GestionCompte') : $router.push('/Connexion')"
                >
                  <q-icon name="person" /> {{ auth.user ? 'Mon Compte' : 'Se connecter' }}
                </div>
                <div v-if="auth.user" class="menu-item logout-item" @click="Logout">
                  <q-icon name="logout" /> Déconnexion
                </div>
              </div>
            </q-menu>
          </q-btn>
        </div>
      </div>
    </q-header>
    <RouterView class="router-view" :key="$route.fullPath" />
    <q-footer class="text-white q-pa-lg footer">
      <div class="row items-start q-col-gutter-lg">
        <div class="col-12 col-md-10">
          <div class="row items-start q-col-gutter-lg">
            <div class="col-md-2 flex flex-center">
              <q-avatar size="1000%">
                <img src="@/assets/Logo/header_logo.png" />
              </q-avatar>
            </div>

            <div class="col-md-4">
              <p class="q-mb-md">+ 1 (514) 555 7676</p>
              <p class="q-mb-md">info@arenedesstrateges.com</p>
              <p class="q-mb-md">4430 Rue Wellington, Verdun, QC, H4G 1W5</p>
            </div>

            <div class="col-md-2 column q-gutter-sm">
              <q-btn
                flat
                class="facebook-btn"
                icon="mdi-facebook"
                href="https://www.facebook.com/arenedesstrateges"
                target="_blank"
              />
            </div>

            <div class="col-md-3 row">
              <RouterLink to="/APropos" class="footer_link q-mb-md">À propos de nous</RouterLink>
              <RouterLink to="/PolitiqueConfidentialite" class="footer_link q-mb-md"
                >Politique de confidentialité</RouterLink
              >
              <RouterLink to="/GuideUtilisation" class="footer_link"
                >Guide d'utilisation</RouterLink
              >
            </div>
          </div>

          <q-separator dark spaced inset />

          <div class="q-mt-md text-center copyright-text">
            <p>© 2025 L'Arène des Stratèges. Tous droits réservés.</p>
          </div>
        </div>

        <div class="col-md-2 flex justify-end">
          <iframe
            src="https://maps.google.com/maps?q=4430%20Rue%20Wellington%2C%20Verdun%2C%20QC%20H4G%201W5&z=14&output=embed"
            width="100%"
            height="200"
            allowfullscreen=""
            loading="lazy"
            class="q-mr-sm rounded-borders"
          ></iframe>
        </div>
      </div>
    </q-footer>
  </q-layout>
</template>

<style scoped>
.header {
  position: fixed !important;
  top: 0;
  left: 0;
  width: 100%;
  z-index: 1100;
  background-color: #f0eada;
  backdrop-filter: blur(4px);
  user-select: none;
}

.header-magasin {
  background-color: #2d3c49;
  color: white;
}

.header-magasin a,
.header-magasin p,
.header-magasin span,
.header-magasin i,
.header-magasin .q-btn,
.header-magasin .q-item {
  color: white !important;
}

.router-view {
  position: relative !important;
  min-height: calc(100vh - 70px - 200px);
  padding-top: 80px;
}

.footer {
  position: relative;
  background-color: #1c1919;
  margin-top: auto;
}

.div_link {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.25rem;
  flex-wrap: nowrap;
}

.link {
  text-decoration: none;
  color: #2e332f;
  font-size: large;
  font-weight: bolder;
}
.header_logo_link {
  text-decoration: none;
  display: inline-block;
  cursor: pointer;
}

.header_logo_container {
  width: 150px;
  height: 80px;
}
.q-avatar__content,
.q-avatar img:not(.q-icon):not(.q-img__image) {
  border-radius: inherit;
  height: 100%;
  width: auto;
  object-fit: contain;
}

.text {
  color: #2e332f;
  margin-bottom: 0px;
}
.footer_link {
  color: white !important;
  text-decoration: underline;
  opacity: 0.9;
  transition: opacity 0.3s;
}
.footer_link:hover {
  opacity: 1;
}

.copyright-text p {
  color: white !important;
  font-size: 0.9rem;
  opacity: 0.7;
}

.mobile-header {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-end;
  padding: 0.5rem;
}

.mobile-actions {
  display: flex;
  align-items: center;
  gap: 2rem;
}

.cart-button {
  position: relative;
  color: #2e332f;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  padding: 0.25rem;
  border-radius: 999px;
}

.cart-button :deep(.q-btn__content) {
  padding: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.cart-button :deep(.q-icon) {
  color: #2e332f;
  vertical-align: middle;
  line-height: 1;
}

.account-link {
  text-decoration: none;
}

.account-button {
  position: relative;
  color: #2e332f;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 0.75rem;
  min-height: 44px;
  border-radius: 999px;
  gap: 0.5rem;
}

.account-button :deep(.q-btn__content) {
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.account-trigger {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.account-trigger__text {
  color: #2e332f;
  font-weight: 600;
  white-space: nowrap;
}

.account-button :deep(.q-icon) {
  color: #2e332f;
  line-height: 1;
}

.account-menu {
  border-radius: 8px;
  padding: 0;
  min-width: 180px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.16);
  overflow: hidden;
}

.account-menu__list {
  padding: 0.25rem 0;
  display: flex;
  flex-direction: column;
}

.account-menu__item {
  padding: 0.75rem 1rem;
  width: 100%;
  box-sizing: border-box;
  display: flex;
  align-items: center;
}

.account-menu__item:hover {
  background-color: #f5f5f5;
}

.account-menu__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 0.25rem;
  min-width: auto;
}

.account-menu__icon :deep(.q-icon) {
  color: #2e332f;
}

.account-menu__label {
  font-weight: 600;
  color: #2e332f;
  white-space: nowrap;
}

.account-menu__item--logout .account-menu__label,
.account-menu__item--logout .account-menu__icon :deep(.q-icon) {
  color: #e53e3e;
}

.cart-button .q-badge {
  position: absolute;
  top: -2px;
  right: -10px;
  min-width: 20px;
  width: auto;
  height: 20px;
  padding: 0 6px;
  border-radius: 9999px;
  font-size: 0.85rem;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}

.mobile-greeting {
  color: #2e332f;
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
}

.mobile-menu-button {
  position: relative;
  color: #2e332f;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  padding: 0.25rem;
  border-radius: 999px;
}

.mobile-menu-button :deep(.q-btn__content) {
  padding: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mobile-menu-button :deep(.q-icon) {
  color: #2e332f;
  vertical-align: middle;
  line-height: 1;
}

.mobile-menu {
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.mobile-menu-content {
  min-width: 180px;
  padding: 0.5rem 0;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  cursor: pointer;
  font-size: 1rem;
  color: #2e332f;
  transition: background-color 0.2s;
}

.menu-item:hover {
  background-color: #f5f5f5;
}

.menu-item.logout-item {
  color: #e53e3e;
  border-top: 1px solid #eee;
  margin-top: 0.5rem;
}

.menu-item .q-icon {
  font-size: 1.2rem;
}

.cart-button {
  color: #2e332f;
}

.cart-button :deep(.q-icon) {
  color: #2e332f;
}

.theme-toggle-item {
  border-bottom: 1px solid #eee;
  margin-bottom: 0.5rem;
  padding: 0.25rem 1rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.facebook-btn {
  color: white !important;
}

.facebook-btn :deep(.q-icon) {
  color: white !important;
}

@media (min-width: 768px) and (max-width: 1024px) {
  .mobile-header {
    padding: 0.75rem 1rem;
  }

  .mobile-menu-content {
    min-width: 220px;
  }

  .menu-item {
    padding: 0.875rem 1.25rem;
    font-size: 1.05rem;
  }
}
</style>

<style>
body.body--dark .header {
  background-color: rgb(35, 42, 56) !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

body.body--dark .link {
  color: white !important;
}

body.body--dark .account-trigger__text {
  color: white !important;
}

body.body--dark .cart-button {
  color: white !important;
}

body.body--dark .cart-button .q-icon {
  color: white !important;
}

body.body--dark .mobile-menu-button {
  color: white !important;
}

body.body--dark .mobile-menu-button .q-icon {
  color: white !important;
}

body.body--dark .account-button .q-icon {
  color: white !important;
}

body.body--dark .account-menu {
  background-color: rgb(35, 42, 56) !important;
}

body.body--dark .account-menu .account-menu__label {
  color: white !important;
}

body.body--dark .account-menu .account-menu__icon .q-icon {
  color: white !important;
}

body.body--dark .account-menu__item:hover {
  background-color: rgba(255, 255, 255, 0.1) !important;
}

body.body--dark .account-menu__item--logout .account-menu__label,
body.body--dark .account-menu__item--logout .account-menu__icon .q-icon {
  color: #ff5252 !important;
}
</style>
