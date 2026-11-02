<script setup>
import '@/assets/gestionCompte.css'
import { ref, nextTick, onMounted } from 'vue'
import { Loading, Notify } from 'quasar'
import { AddAddress, getAddress, UpdateAddress, DeleteAddress } from '@/api/address/AddressApi'
import AddressModal from '../shared/AddressModal.vue'

const userAddresses = ref([])
const highlightedAddressId = ref(null)

const isModalOpen = ref(false)
const isEditMode = ref(false)
const modalAddressData = ref(null)

onMounted(async () => {
  await loadAddresses()
})

async function loadAddresses() {
  try {
    userAddresses.value = await getAddress()
  } catch (error) {
    console.error('Erreur lors du chargement des adresses:', error)
    userAddresses.value = []
  }
}

async function highlightNewAddress(newAddressData) {
  await nextTick()
  await new Promise((resolve) => setTimeout(resolve, 100))

  const newAddress = userAddresses.value.find(
    (addr) =>
      String(addr.doorNb) === String(newAddressData.doorNb) &&
      addr.street.trim().toLowerCase() === newAddressData.street.trim().toLowerCase() &&
      addr.city.trim().toLowerCase() === newAddressData.city.trim().toLowerCase() &&
      addr.postalCode.replace(/\s/g, '').toLowerCase() ===
        newAddressData.postalCode.replace(/\s/g, '').toLowerCase(),
  )

  if (newAddress) {
    highlightedAddressId.value = newAddress.id

    await nextTick()
    await new Promise((resolve) => setTimeout(resolve, 500))
    await nextTick()
    await new Promise((resolve) => setTimeout(resolve, 300))

    const anchorId = `address-${newAddress.id}`

    const scrollToAnchor = () => {
      const addressElement = document.getElementById(anchorId)
      if (addressElement) {
        window.location.hash = anchorId

        setTimeout(() => {
          addressElement.scrollIntoView({
            behavior: 'smooth',
            block: 'center',
          })
        }, 100)

        return true
      }
      return false
    }

    let attempts = 0
    const maxAttempts = 10
    const tryScroll = () => {
      attempts++
      if (!scrollToAnchor() && attempts < maxAttempts) {
        setTimeout(tryScroll, 150)
      }
    }
    tryScroll()

    setTimeout(() => {
      highlightedAddressId.value = null

      if (window.location.hash === `#${anchorId}`) {
        history.replaceState('', document.title, window.location.pathname)
      }
    }, 5000)
  }
}

function formatAddress(address) {
  if (!address) return ''
  const parts = []
  if (address.doorNb) parts.push(address.doorNb.toString())
  if (address.unit) parts.push('Apt ' + address.unit)
  if (address.street) parts.push(address.street)
  if (address.city) parts.push(address.city)
  if (address.province) parts.push(address.province)
  if (address.postalCode) parts.push(address.postalCode)
  if (address.country) parts.push(address.country)
  return parts.join(', ')
}

function openAddModal() {
  isEditMode.value = false
  modalAddressData.value = null
  isModalOpen.value = true
}

async function openEditModal(address) {
  isEditMode.value = true
  modalAddressData.value = {
    ...address,
  }
  isModalOpen.value = true
}

function closeModal() {
  isModalOpen.value = false
  modalAddressData.value = null
}

async function handleModalSave(data) {
  const payload = {
    addressNumber: parseInt(data.doorNb),
    street: data.street,
    city: data.city,
    postalCode: data.postalCode,
    unit: data.unit || null,
    state: data.provinceId,
    country: data.countryId,
  }

  try {
    Loading.show({
      message: isEditMode.value ? 'Mise à jour...' : 'Enregistrement...',
      spinnerColor: 'primary',
    })

    if (isEditMode.value && modalAddressData.value?.id) {
      await UpdateAddress(modalAddressData.value.id, payload)
      Notify.create({
        type: 'positive',
        message: 'Adresse modifiée avec succès',
        position: 'top',
      })
      await loadAddresses()
    } else {
      await AddAddress(payload)
      Notify.create({
        type: 'positive',
        message: 'Adresse ajoutée avec succès',
        position: 'top',
      })

      await loadAddresses()

      const newAddressData = {
        doorNb: parseInt(data.doorNb),
        street: data.street,
        city: data.city,
        postalCode: data.postalCode,
      }

      await highlightNewAddress(newAddressData)
    }

    closeModal()
  } catch (e) {
    Notify.create({
      type: 'negative',
      message: isEditMode.value
        ? "Impossible de modifier l'adresse"
        : "Impossible d'ajouter l'adresse",
      position: 'top',
    })
  } finally {
    Loading.hide()
  }
}

const showDeleteConfirmation = (message) => {
  return new Promise((resolve) => {
    const notification = Notify.create({
      type: 'warning',
      message,
      position: 'top',
      timeout: 0,
      actions: [
        {
          label: 'Oui',
          color: 'negative',
          handler: () => {
            notification()
            resolve(true)
          },
        },
        {
          label: 'Non',
          color: 'black',
          handler: () => {
            notification()
            resolve(false)
          },
        },
      ],
    })
  })
}

async function deleteAddress(address) {
  const confirmed = await showDeleteConfirmation(
    `Voulez-vous vraiment supprimer l'adresse : ${formatAddress(address)} ?`,
  )

  if (!confirmed) {
    return
  }

  try {
    Loading.show({
      message: 'Suppression...',
      spinnerColor: 'primary',
    })

    await DeleteAddress(address.id)

    Notify.create({
      type: 'positive',
      message: 'Adresse supprimée avec succès',
      position: 'top',
    })

    await loadAddresses()
  } catch (e) {
    Notify.create({
      type: 'negative',
      message: "Impossible de supprimer l'adresse",
      position: 'top',
    })
  } finally {
    Loading.hide()
  }
}
</script>

<template>
  <q-page class="fit q-pa-lg">
    <q-card class="row q-ma-md q-pa-sm text-align-center title-card">
      <h5>Adresse</h5>
    </q-card>
    <q-card class="row q-ma-md info-card">
      <q-card-section
        class="title-2-card"
        style="display: flex; justify-content: space-between; align-items: center; width: 100%"
      >
        <h6>Adresses enregistrées</h6>
        <q-btn
          label="Créer adresse"
          class="edit-btn voir-adresse-btn"
          @click="openAddModal"
          no-caps
        />
      </q-card-section>

      <!-- Liste des adresses enregistrées -->
      <q-card-section class="q-pa-md">
        <div v-if="!userAddresses || userAddresses.length === 0" class="text-center q-pa-lg">
          <p style="font-size: 1.1rem; color: inherit">Aucune adresse enregistrée</p>
        </div>
        <q-list v-else separator>
          <q-item
            v-for="address in userAddresses"
            :key="address.id"
            :id="`address-${address.id}`"
            :data-address-id="address.id"
            :class="[
              'q-pa-md address-item',
              { 'address-highlight': highlightedAddressId === address.id },
            ]"
          >
            <q-item-section clickable @click="openEditModal(address)">
              <q-item-label class="text-body1 text-weight-medium">
                {{ formatAddress(address) }}
              </q-item-label>
            </q-item-section>
            <q-item-section side class="delete-icon-section">
              <q-icon
                name="delete"
                class="delete-address-icon"
                size="md"
                @click.stop="deleteAddress(address)"
              >
                <q-tooltip>Supprimer l'adresse</q-tooltip>
              </q-icon>
            </q-item-section>
          </q-item>
        </q-list>
      </q-card-section>
    </q-card>

    <AddressModal
      v-model="isModalOpen"
      :initial-address="modalAddressData"
      :is-edit-mode="isEditMode"
      @save="handleModalSave"
      @cancel="closeModal"
    />
  </q-page>
</template>
<style scoped>
.voir-adresse-btn {
  white-space: nowrap !important;
  min-width: fit-content !important;
}

.voir-adresse-btn :deep(.q-btn__content) {
  white-space: nowrap !important;
}

.address-item {
  cursor: pointer;
  background-color: #f0eada !important;
  transition: all 0.3s ease;
  border-radius: 8px;
  margin-bottom: 8px;
  border: 1px solid transparent !important;
}

.address-item:hover {
  background-color: transparent !important;
  border: 1px solid rgba(0, 0, 0, 0.2) !important;
}

.delete-icon-section {
  display: flex;
  align-items: center;
  justify-content: center;
}

.delete-address-icon {
  color: #676767 !important;
  cursor: pointer !important;
  transition: all 0.3s ease !important;
  font-size: 28px !important;
}

.delete-address-icon:hover {
  color: #f44336 !important;
  transform: scale(1.2) rotate(5deg);
  filter: drop-shadow(0 2px 4px rgba(244, 67, 54, 0.4));
}

.delete-address-icon:active {
  transform: scale(1.1) rotate(-5deg);
  transition: all 0.1s ease !important;
}

.address-highlight {
  animation: highlightBorder 5s ease-in-out;
  border: 3px solid #4caf50 !important;
  border-radius: 4px;
  position: relative;
}

@keyframes highlightBorder {
  0% {
    border-color: #4caf50;
    border-width: 4px;
    box-shadow: 0 0 20px rgba(76, 175, 80, 0.6);
  }
  50% {
    border-color: #66bb6a;
    border-width: 3px;
    box-shadow: 0 0 15px rgba(76, 175, 80, 0.4);
  }
  100% {
    border-color: #81c784;
    border-width: 2px;
    box-shadow: 0 0 0px rgba(76, 175, 80, 0);
  }
}

body.body--dark h5,
body.body--dark h6,
body.body--dark .field-label,
body.body--dark .q-item__label,
body.body--dark span,
body.body--dark p,
body.body--dark label.field span {
  color: white !important;
}

body.body--dark .address-item {
  background-color: rgba(255, 255, 255, 0.1) !important;
  transition: background-color 0.3s ease;
  border-radius: 8px;
  margin-bottom: 8px;
}

body.body--dark .address-item:hover {
  background-color: transparent !important;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

body.body--dark .delete-address-icon {
  color: #9ca3af !important;
}

body.body--dark .delete-address-icon:hover {
  color: #f87171 !important;
}
</style>
