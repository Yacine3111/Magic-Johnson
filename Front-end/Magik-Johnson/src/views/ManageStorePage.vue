<script setup>
import { useAuthStore } from '@/stores/authStore'
import { onMounted, ref, computed, watch, onBeforeUnmount } from 'vue'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const menuList = [
  {
    label: 'Promotions',
    name: 'Promotions',
  },
  {
    label: 'Inventaire',
    name: 'Inventaire',
  },
  {
    label: 'Employés',
    name: 'Employes',
  },
  {
    label: 'Gestion des Clients',
    name: 'Clients',
  },
  {
    label: 'Gestion des commandes',
    name: 'gestionCommandes',
  },
  {
    label: 'Gestion remboursements',
    name: 'gestionDemandesRemboursement',
  },
]

const auth = useAuthStore()
const role = ref('')

const drawerOpen = ref(false)

const displayedMenu = computed(() => {
  if (role.value === 'manager') {
    return menuList
  }
  return [
    {
      label: 'Inventaire',
      name: 'Inventaire',
    },
    {
      label: 'Gestion des Clients',
      name: 'Clients',
    },
    {
      label: 'Gestion des commandes',
      name: 'gestionCommandes',
    },
    {
      label: 'Gestion remboursements',
      name: 'gestionDemandesRemboursement',
    },
  ]
})

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
  await auth.fetchRole()
  role.value = auth.role[0]

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
  <div class="gestion-layout-wrapper">
    <q-layout view="hHh lpR fFf" class="gestion-layout">
      <!--  Sidebar -->
      <q-drawer
        v-model="drawerOpen"
        :show-if-above="$q.screen.gt.sm"
        :width="200"
        :breakpoint="960"
        bordered
        class="custom-drawer"
        :overlay="$q.screen.lt.md"
      >
        <div class="q-pa-md text-subtitle1 text-bold">Magasin</div>
        <q-separator style="background: #fff; height: 2px"></q-separator>

        <q-list>
          <template v-for="(menuItem, index) in displayedMenu" :key="index">
            <q-item
              clickable
              v-ripple
              :to="{ name: menuItem.name }"
              active-class="q-item--active"
              @click="handleMenuClick"
              class="text-white menu-item custom-menu-item"
            >
              <q-item-section>
                {{ menuItem.label }}
              </q-item-section>
            </q-item>
          </template>
        </q-list>
      </q-drawer>

      <!-- Contenu principal -->
      <q-page-container class="gestion-page-container">
        <!-- Bouton hamburger mobile/tablette  -->
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
.gestion-layout-wrapper {
  background: #676767;
}

:deep(.q-drawer) .text-subtitle1 {
  color: white;
}

:deep(.q-item) {
  background-color: #676767 !important;
  color: white !important;
}

:deep(.q-item-section) {
  color: white !important;
  background-color: transparent !important;
}

:deep(.q-item__section) {
  color: white !important;
  background-color: transparent !important;
}

:deep(.q-item__section--main) {
  color: white !important;
  background-color: transparent !important;
}

:deep(.q-item--active) {
  color: black !important;
  background-color: #938f85 !important;
}

:deep(.q-item--active) .q-item-section,
:deep(.q-item--active) .q-item__section {
  color: black !important;
}

:deep(.q-item:hover) {
  background-color: rgb(145, 140, 140);
}

:deep(.q-item:hover) .q-item-section,
:deep(.q-item:hover) .q-item__section {
  color: black !important;
}

/* Liste */
:deep(.q-list) {
  color: white;
}

:deep(.gestion-page-container) {
  padding-top: 20px !important;
}

@media (min-width: 960px) {
  :deep(.gestion-page-container) {
    background: #676767;
    padding-top: 20px !important;
    padding-left: 220px !important;
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
    max-width: 250px !important;
    width: 250px !important;
    flex-shrink: 0 !important;
    background-color: #676767 !important;
    color: white !important;
  }

  :deep(.q-drawer--mobile) {
    background-color: #676767 !important;
    z-index: 3000 !important;
  }

  :deep(.q-drawer--mobile.q-drawer--open),
  :deep(.q-drawer--mobile[data-model='true']) {
    background-color: #676767 !important;
    z-index: 3000 !important;
    transform: translateX(0) !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer__inner) {
    max-width: 250px !important;
    width: 250px !important;
    background-color: #676767 !important;
    color: white !important;
  }

  :deep(.q-drawer__content) {
    background-color: #676767 !important;
    color: white !important;
    visibility: visible !important;
    opacity: 1 !important;
  }

  :deep(.q-drawer *) {
    color: white !important;
  }

  :deep(.q-item) {
    background-color: #676767 !important;
    color: white !important;
  }

  :deep(.q-drawer .q-item) {
    color: white !important;
    background-color: #676767 !important;
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

  :deep(.gestion-page-container) {
    background: #676767 !important;
    padding: 20px 15px 100px 15px !important;
  }
}

.mobile-menu-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px;
  background-color: rgba(35, 42, 56, 0.95);
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

.mobile-menu-btn:hover {
  background-color: rgba(35, 42, 56, 1);
}

.mobile-menu-btn:active {
  background-color: rgba(35, 42, 56, 0.8);
}

.menu-label {
  font-size: 16px;
  font-weight: bold;
  color: white;
}

:deep(.custom-drawer) {
  background-color: #676767 !important;
}

:deep(.custom-drawer .q-drawer__content) {
  background-color: #676767 !important;
  overflow-x: hidden !important;
}

:deep(.custom-drawer) {
  overflow-x: hidden !important;
}

:deep(.custom-menu-item) {
  background-color: transparent !important;
  white-space: normal !important;
}
</style>

<style>
body:not(.body--dark) .gestion-layout-wrapper {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .gestion-page-container {
  background-color: #d1cbbb !important;
}

body:not(.body--dark) .q-drawer,
body:not(.body--dark) .custom-drawer,
body:not(.body--dark) .custom-drawer .q-drawer__content {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .q-drawer .text-subtitle1,
body:not(.body--dark) .q-drawer .menu-label,
body:not(.body--dark) .custom-drawer .text-subtitle1 {
  color: black !important;
}

body:not(.body--dark) .q-drawer .q-item,
body:not(.body--dark) .custom-menu-item {
  background-color: transparent !important;
  color: black !important;
}

body:not(.body--dark) .q-drawer .q-item-section,
body:not(.body--dark) .q-drawer .q-item__section {
  color: black !important;
}

body:not(.body--dark) .q-drawer .q-item--active {
  background-color: #5d4037 !important;
  color: white !important;
}

body:not(.body--dark) .q-drawer .q-item--active .q-item-section,
body:not(.body--dark) .q-drawer .q-item--active .q-item__section {
  color: white !important;
}

body:not(.body--dark) .q-drawer .q-item:hover {
  background-color: rgba(255, 255, 255, 0.2) !important;
}

body:not(.body--dark) .mobile-menu-btn {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .mobile-menu-btn .menu-label {
  color: black !important;
}

body:not(.body--dark) .mobile-menu-btn .q-icon {
  color: black !important;
}
</style>
