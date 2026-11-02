<script setup>
import { useQuasar } from 'quasar'
import { useAuthStore } from '@/stores/authStore.js'
import { onMounted, ref, watch, onBeforeUnmount } from 'vue'

const $q = useQuasar()
const authStore = useAuthStore()

const menuList = [
  {
    icon: 'bi bi-person-circle',
    label: 'Profil',
    name: 'Profil',
  },
  {
    icon: 'bi bi-bi bi-receipt',
    label: 'Historique de commande',
    name: 'HistoriqueCommande',
  },
  {
    icon: 'bi bi-geo-alt',
    label: 'Adresse',
    name: 'Adresse',
  },
  {
    icon: 'bi bi-shield-lock',
    label: 'Sécurité',
    name: 'Securite',
  },
]

const adminMenuList = [
  {
    icon: 'bi bi-person-circle',
    label: 'Profil',
    name: 'Profil',
  },
  {
    icon: 'bi bi-geo-alt',
    label: 'Adresse',
    name: 'Adresse',
  },
  {
    icon: 'bi bi-shield-lock',
    label: 'Sécurité',
    name: 'Securite',
  },
]

const drawerOpen = ref(false)

const toggleDrawer = () => {
  drawerOpen.value = !drawerOpen.value

  setTimeout(() => {
    const drawer = document.querySelector('.q-drawer')
    if (drawer && drawerOpen.value) {
      drawer.classList.add('q-drawer--open')
    } else if (drawer && !drawerOpen.value) {
      drawer.classList.remove('q-drawer--open')
    }
  }, 50)
}

const handleMenuClick = () => {
  if ($q.screen.lt.md) {
    drawerOpen.value = false
  }
}

const handleOutsideClick = (event) => {
  if ($q.screen.lt.md && drawerOpen.value) {
    const drawer = document.querySelector('.q-drawer')
    const drawerContent = document.querySelector('.q-drawer__content')
    const menuButtonContainer = event.target.closest('.mobile-menu-btn')
    const backdrop = event.target.classList.contains('q-drawer__backdrop')

    if (backdrop) {
      drawerOpen.value = false
      return
    }

    const clickedOnDrawer = drawer && (drawer.contains(event.target) || drawer === event.target)
    const clickedOnDrawerContent = drawerContent && drawerContent.contains(event.target)
    const clickedOnMenuButton = menuButtonContainer

    if (!clickedOnDrawer && !clickedOnDrawerContent && !clickedOnMenuButton) {
      drawerOpen.value = false
    }
  }
}

onMounted(async () => {
  await authStore.fetchRole()

  if ($q.screen.gt.sm) {
    drawerOpen.value = true
  } else {
    drawerOpen.value = false
  }

  document.addEventListener('click', handleOutsideClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleOutsideClick)
})

watch(
  () => $q.screen.gt.sm,
  (isDesktop) => {
    if (isDesktop) {
      drawerOpen.value = true
    } else {
      drawerOpen.value = false
    }
  },
)

watch(
  () => drawerOpen.value,
  (newVal) => {
    setTimeout(() => {
      const drawer = document.querySelector('.q-drawer')
      if (drawer && newVal) {
        drawer.classList.add('q-drawer--open')
        drawer.setAttribute('data-open', 'true')
      } else if (drawer && !newVal) {
        drawer.classList.remove('q-drawer--open')
        drawer.removeAttribute('data-open')
      }
    }, 50)
  },
)
</script>
<template>
  <div class="gestion-compte-layout-wrapper">
    <q-layout view="hHh lpR fFf" class="gestion-compte-layout">
      <!-- Sidebar -->
      <q-drawer
        v-model="drawerOpen"
        :show-if-above="$q.screen.gt.sm"
        :width="300"
        :breakpoint="960"
        bordered
        style="overflow: hidden"
        :overlay="$q.screen.lt.md"
      >
        <div class="q-pa-md text-subtitle1 text-bold">Gestion de compte</div>
        <q-separator class="separator"></q-separator>
        <q-list>
          <template
            v-if="authStore.role == 'client' || authStore.role == null"
            v-for="menuItem in menuList"
            :key="`client-${menuItem.name}`"
          >
            <q-item
              clickable
              v-ripple
              :to="{ name: menuItem.name }"
              active-class="q-item--active"
              @click="handleMenuClick"
              class="menu-item"
            >
              <q-item-section avatar>
                <i :class="menuItem.icon"></i>
              </q-item-section>
              <q-item-section>
                {{ menuItem.label }}
              </q-item-section>
            </q-item>
          </template>
          <template
            v-else
            v-for="(menuItem, index) in adminMenuList"
            :key="`admin-${menuItem.name}`"
          >
            <q-item
              clickable
              v-ripple
              :to="{ name: menuItem.name }"
              active-class="q-item--active"
              @click="handleMenuClick"
              class="menu-item"
            >
              <q-item-section avatar>
                <i :class="menuItem.icon"></i>
              </q-item-section>
              <q-item-section>
                {{ menuItem.label }}
              </q-item-section>
            </q-item>
          </template>
        </q-list>
      </q-drawer>

      <!-- Contenu principal -->
      <q-page-container class="gestion-compte-page-container">
        <!-- Bouton hamburger mobile/tablette -->
        <div v-if="$q.screen.lt.md" class="mobile-menu-btn" @click="toggleDrawer">
          <q-btn
            flat
            dense
            round
            size="lg"
            icon="menu"
            color="white"
            @click.stop="toggleDrawer"
            aria-label="Menu"
          />
          <span class="menu-label">Menu</span>
        </div>

        <router-view></router-view>
      </q-page-container>
    </q-layout>
  </div>
</template>

<style scoped>
.gestion-compte-layout-wrapper {
  background: #d1cbbb;
}

.separator {
  height: 2px;
  background-color: white;
}

:deep(.q-drawer) .text-subtitle1 {
  color: white;
}

:deep(.q-item) {
  background-color: #f0eada !important;
  color: black !important;
}

:deep(.q-item-section) {
  color: black !important;
  background-color: transparent !important;
}

:deep(.q-item__section) {
  color: black !important;
  background-color: transparent !important;
}

:deep(.q-item__section--main) {
  color: black !important;
  background-color: transparent !important;
}

:deep(.q-item--active) {
  color: black !important;
  background-color: #d1d1d1 !important;
}

:deep(.q-item--active) .q-item-section,
:deep(.q-item--active) .q-item__section {
  color: black !important;
}

:deep(.q-list) {
  color: white;
}

:deep(.gestion-compte-page-container) {
  padding-top: 20px !important;
  background: #d1cbbb;
}

@media (min-width: 960px) {
  :deep(.gestion-compte-page-container) {
    background: #d1cbbb;
    padding-top: 20px !important;
    padding-left: 320px !important;
    padding-right: 30px !important;
    padding-bottom: 300px !important;
  }

  :deep(.q-drawer) {
    position: fixed !important;
    top: 80px !important;
    bottom: 0 !important;
    height: calc(100vh - 80px) !important;
    left: 0 !important;
  }
}

@media (max-width: 959px) {
  :deep(.q-drawer:not(.q-drawer--on-top)) {
    max-width: 300px !important;
    width: 300px !important;
    flex-shrink: 0 !important;
    background-color: #f0eada !important;
    color: black !important;
  }

  :deep(.q-drawer--mobile) {
    background-color: #313632 !important;
    z-index: 3000 !important;
  }

  :deep(.q-drawer--mobile.q-drawer--open),
  :deep(.q-drawer--mobile[data-model='true']) {
    background-color: #f0eada !important;
    z-index: 3000 !important;
    transform: translateX(0) !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer__inner) {
    max-width: 300px !important;
    width: 300px !important;
    background-color: #313632 !important;
    color: white !important;
  }

  :deep(.q-drawer__content) {
    background-color: #f0eada !important;
    color: black !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer *) {
    color: white !important;
  }

  :deep(.q-item) {
    background-color: #f0eada !important;
    color: black !important;
  }

  :deep(.q-drawer .q-item) {
    color: white !important;
    background-color: #313632 !important;
    display: flex !important;
    visibility: visible !important;
    opacity: 1 !important;
    min-height: 48px !important;
    padding: 8px 16px !important;
  }

  :deep(.q-drawer .q-item *),
  :deep(.q-drawer .q-item-section),
  :deep(.q-drawer .q-item-section *),
  :deep(.q-drawer .q-item__section),
  :deep(.q-drawer .q-item__section *),
  :deep(.q-drawer .q-item__label),
  :deep(.q-drawer .q-item__label *),
  :deep(.q-drawer .q-list),
  :deep(.q-drawer .q-list *),
  :deep(.q-drawer .text-subtitle1),
  :deep(.q-drawer .text-subtitle1 *) {
    color: white !important;
    visibility: visible !important;
    opacity: 1 !important;
    display: block !important;
  }

  :deep(.q-drawer .q-item__label) {
    color: white !important;
    font-size: 16px !important;
    font-weight: normal !important;
    line-height: 1.5 !important;
    visibility: visible !important;
    opacity: 1 !important;
    display: block !important;
    white-space: normal !important;
  }

  :deep(.q-drawer .q-item-section) {
    color: white !important;
    background-color: transparent !important;
    display: flex !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer .q-item__section) {
    color: white !important;
    background-color: transparent !important;
    display: flex !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer .q-item__section--main) {
    color: white !important;
    background-color: transparent !important;
    display: flex !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer .q-item__label) {
    color: white !important;
    display: block !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer__content) {
    visibility: visible !important;
    opacity: 1 !important;
    display: block !important;
  }

  :deep(.q-drawer__content *) {
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.gestion-compte-page-container) {
    background: #d1cbbb !important;
    padding: 20px 15px 100px 15px !important;
  }
}

.mobile-menu-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px;
  background-color: rgba(49, 54, 50, 0.95);
  border-radius: 8px;
  margin-bottom: 15px;
  margin-left: 15px;
  margin-top: 15px;
  width: fit-content;
  cursor: pointer;
  user-select: none;
  transition: background-color 0.2s ease;
  position: relative;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.mobile-menu-btn:active {
  background-color: rgba(49, 54, 50, 0.8);
}

.menu-label {
  font-size: 16px;
  font-weight: bold;
  color: white;
}
</style>

<style>
/* Light Mode */
.gestion-compte-layout-wrapper {
  background: #d1cbbb !important;
}

.gestion-compte-page-container {
  background: #d1cbbb !important;
}

.q-drawer {
  background-color: #a89c8d !important;
  color: black !important;
}

.q-drawer .text-subtitle1 {
  color: black !important;
}

.q-drawer .q-item,
.q-drawer .mobile-menu-btn {
  background-color: #a89c8d !important;
  color: black;
}

.q-drawer .q-item.q-item--active,
.q-drawer .q-item.q-item--active .q-item__section,
.q-drawer .q-item.q-item--active .q-icon {
  color: #8a422c !important;
  font-weight: bold !important;
}

.q-drawer .text-white {
  color: black !important;
}

.q-drawer .separator {
  background-color: black !important;
}

/* Dark Mode */
body.body--dark .gestion-compte-layout-wrapper,
body.body--dark .gestion-compte-page-container {
  background: rgb(35, 42, 56) !important;
}

body.body--dark .q-drawer,
body.body--dark .q-drawer__content,
body.body--dark .mobile-menu-btn {
  background-color: #1f2735 !important;
  color: white !important;
}

body.body--dark .q-drawer .q-item {
  background-color: #1f2735 !important;
  color: white !important;
}

body.body--dark .q-drawer .q-item:not(.q-item--active) * {
  color: white !important;
}

body.body--dark .q-drawer .q-item.q-item--active,
body.body--dark .q-drawer .q-item.q-item--active * {
  color: #ff5252 !important;
  font-weight: bold !important;
}

body.body--dark .q-drawer .text-subtitle1 {
  color: white !important;
}

body.body--dark .q-drawer .separator {
  background-color: white !important;
}

@media (max-width: 959px) {
  .gestion-compte-page-container {
    background: #d1cbbb !important;
  }
  body.body--dark .gestion-compte-page-container {
    background: rgb(35, 42, 56) !important;
  }

  .q-drawer--mobile .q-drawer__content,
  .q-drawer--mobile .q-item {
    background-color: #a89c8d !important;
  }

  body.body--dark .q-drawer--mobile .q-drawer__content,
  body.body--dark .q-drawer--mobile .q-item {
    background-color: #1f2735 !important;
  }
}
</style>
<style>
/* Light Mode */
.gestion-compte-layout-wrapper {
  background: #d1cbbb !important;
}

.gestion-compte-page-container {
  background: #d1cbbb !important;
}

.q-drawer {
  background-color: #a89c8d !important;
  color: black !important;
}

.q-drawer .text-subtitle1 {
  color: black !important;
}

/* Sidebar Items - Light Mode */
.q-drawer .q-item,
.q-drawer .mobile-menu-btn {
  background-color: #a89c8d !important;
  color: black;
  transition: all 0.3s ease;
}

/* Hover State - Light Mode */
.q-drawer .q-item:hover,
.q-drawer .mobile-menu-btn:hover {
  background-color: #d16a4a !important;
  color: white !important;
}

.q-drawer .q-item:hover .q-icon,
.q-drawer .q-item:hover .q-item__section {
  color: white !important;
}

.q-drawer .q-item.q-item--active,
.q-drawer .q-item.q-item--active .q-item__section,
.q-drawer .q-item.q-item--active .q-icon {
  color: #8a422c !important;
  font-weight: bold !important;
}

.q-drawer .text-white {
  color: black !important;
}

.q-drawer .separator {
  background-color: black !important;
}

body.body--dark .gestion-compte-layout-wrapper,
body.body--dark .gestion-compte-page-container {
  background: rgb(35, 42, 56) !important;
}

body.body--dark .q-drawer,
body.body--dark .q-drawer__content,
body.body--dark .mobile-menu-btn {
  background-color: #1f2735 !important;
  color: white !important;
}

body.body--dark .q-drawer .q-item {
  background-color: #1f2735 !important;
  color: white !important;
}

body.body--dark .q-drawer .q-item:not(.q-item--active) * {
  color: white !important;
}

body.body--dark .q-drawer .q-item.q-item--active,
body.body--dark .q-drawer .q-item.q-item--active * {
  color: #a89c8d !important;
  font-weight: bold !important;
}

body.body--dark .q-drawer .text-subtitle1 {
  color: white !important;
}

body.body--dark .q-drawer .separator {
  background-color: white !important;
}

@media (max-width: 959px) {
  .gestion-compte-page-container {
    background: #d1cbbb !important;
  }
  body.body--dark .gestion-compte-page-container {
    background: rgb(35, 42, 56) !important;
  }

  .q-drawer--mobile .q-drawer__content,
  .q-drawer--mobile .q-item {
    background-color: #a89c8d !important;
  }

  body.body--dark .q-drawer--mobile .q-drawer__content,
  body.body--dark .q-drawer--mobile .q-item {
    background-color: #1f2735 !important;
  }
}
</style>
