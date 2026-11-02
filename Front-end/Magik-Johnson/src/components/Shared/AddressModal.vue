<template>
  <q-dialog
    :model-value="modelValue"
    persistent
    transition-show="scale"
    transition-hide="scale"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <q-card class="address-dialog-card">
      <q-card-section class="text-h6 modal-title">
        {{ isEditMode ? "Modifier l'adresse" : 'Nouvelle adresse' }}
      </q-card-section>

      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">No civique</label>
            <input
              type="text"
              class="form-control"
              v-model="addressData.doorNb"
              placeholder="1234"
              @blur="validateField('doorNb')"
              :class="{ 'field-error': !!errors.doorNb }"
            />
            <span class="error-message" v-if="errors.doorNb">{{ errors.doorNb }}</span>
          </div>

          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">Rue</label>
            <input
              type="text"
              class="form-control"
              v-model="addressData.street"
              placeholder="Rue Principale"
              @blur="validateField('street')"
              :class="{ 'field-error': !!errors.street }"
            />
            <span class="error-message" v-if="errors.street">{{ errors.street }}</span>
          </div>

          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">Ville</label>
            <input
              type="text"
              class="form-control"
              v-model="addressData.city"
              placeholder="Granby"
              @blur="validateField('city')"
              :class="{ 'field-error': !!errors.city }"
            />
            <span class="error-message" v-if="errors.city">{{ errors.city }}</span>
          </div>

          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">Unité</label>
            <input
              type="text"
              class="form-control"
              v-model="addressData.unit"
              placeholder="Apt 2"
            />
          </div>

          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">Pays</label>
            <input type="text" class="form-control" v-model="countryName" disabled />
          </div>

          <div class="col-6 mb-3">
            <label class="form-label text-adaptive">Code postal</label>
            <input
              type="text"
              class="form-control"
              v-model="addressData.postalCode"
              placeholder="J2G 5C7"
              v-mask="'A#A #A#'"
              style="text-transform: uppercase"
              @blur="validateField('postalCode')"
              :class="{ 'field-error': !!errors.postalCode }"
            />
            <span class="error-message" v-if="errors.postalCode">{{ errors.postalCode }}</span>
          </div>

          <div class="col-12 mb-3">
            <label class="form-label text-adaptive">Province</label>
            <div class="dropdown">
              <button
                class="btn dropdown-toggle form-control d-flex justify-content-between align-items-center text-start"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
                :class="{ 'field-error': !!errors.state }"
              >
                <span class="text-truncate">{{
                  addressData.state ? addressData.state.name : 'Sélectionnez une province'
                }}</span>
              </button>
              <ul class="dropdown-menu w-100 p-0">
                <li v-for="prov in provinces" :key="prov.id">
                  <a class="dropdown-item py-2" href="#" @click.prevent="selectProvince(prov)">
                    {{ prov.name }}
                  </a>
                </li>
              </ul>
            </div>
            <span class="error-message" v-if="errors.state">{{ errors.state }}</span>
          </div>
        </div>
      </q-card-section>

      <q-card-actions align="right" class="q-pa-md">
        <q-btn label="Annuler" color="negative" no-caps @click="emit('cancel')" />
        <q-btn label="Confirmer" color="primary" no-caps @click="submit" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import '@/assets/gestionCompte.css'
import { ref, computed, watch, onMounted } from 'vue'
import { getCanada } from '@/api/address/AddressApi'
import { ValidateStreet, ValidateCity, ValidatePostalCode } from '@/validators/addressValidator'
import { Notify } from 'quasar'

const props = defineProps({
  modelValue: Boolean,
  initialAddress: {
    type: Object,
    default: () => null,
  },
  isEditMode: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:modelValue', 'save', 'cancel'])

const addressData = ref({
  doorNb: '',
  street: '',
  city: '',
  unit: '',
  postalCode: '',
  state: null,
})

const countryData = ref(null)
const errors = ref({
  doorNb: '',
  street: '',
  city: '',
  postalCode: '',
  state: '',
  country: '',
})

const provinces = computed(() => countryData.value?.provinces || [])
const countryName = computed(() => countryData.value?.name || 'Canada')

onMounted(async () => {
  countryData.value = await getCanada()
})

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      if (props.initialAddress) {
        addressData.value = {
          doorNb: props.initialAddress.doorNb || '',
          street: props.initialAddress.street || '',
          city: props.initialAddress.city || '',
          unit: props.initialAddress.unit || '',
          postalCode: props.initialAddress.postalCode || '',
          state: null,
        }

        if (provinces.value.length > 0) {
          let found = null

          if (props.initialAddress.provinceId) {
            found = provinces.value.find((p) => p.id === props.initialAddress.provinceId)
          }

          if (!found && (props.initialAddress.province || props.initialAddress.stateName)) {
            found = provinces.value.find(
              (p) =>
                p.name === props.initialAddress.province ||
                p.name === props.initialAddress.stateName,
            )
          }

          if (found) addressData.value.state = found
        }
      } else {
        addressData.value = {
          doorNb: '',
          street: '',
          city: '',
          unit: '',
          postalCode: '',
          state: null,
        }
      }

      Object.keys(errors.value).forEach((k) => (errors.value[k] = ''))
    }
  },
)

watch(provinces, (newVal) => {
  if (props.initialAddress && newVal.length > 0) {
    let found = null

    if (props.initialAddress.provinceId) {
      found = newVal.find((p) => p.id === props.initialAddress.provinceId)
    }

    if (!found && (props.initialAddress.province || props.initialAddress.stateName)) {
      found = newVal.find(
        (p) =>
          p.name === props.initialAddress.province || p.name === props.initialAddress.stateName,
      )
    }

    if (found) addressData.value.state = found
  }
})

function validateField(field) {
  let isValid = true

  if (field === 'street' || !field) {
    const streetRef = ref(addressData.value.street)
    const errRef = ref(false)
    const msgRef = ref('')
    isValid = ValidateStreet(streetRef, errRef, msgRef) && isValid
    errors.value.street = msgRef.value
  }

  if (field === 'city' || !field) {
    const cityRef = ref(addressData.value.city)
    const errRef = ref(false)
    const msgRef = ref('')
    isValid = ValidateCity(cityRef, errRef, msgRef) && isValid
    errors.value.city = msgRef.value
  }

  if (field === 'postalCode' || !field) {
    const pcRef = ref(addressData.value.postalCode)
    const errRef = ref(false)
    const msgRef = ref('')
    isValid = ValidatePostalCode(pcRef, errRef, msgRef) && isValid
    errors.value.postalCode = msgRef.value
  }

  if (field === 'doorNb' || !field) {
    if (!addressData.value.doorNb) {
      errors.value.doorNb = 'Ce champ est requis'
      isValid = false
    } else {
      errors.value.doorNb = ''
    }
  }

  if (field === 'state' || !field) {
    if (!addressData.value.state) {
      errors.value.state = 'Veuillez sélectionner une province'
      isValid = false
    } else {
      errors.value.state = ''
    }
  }

  return isValid
}

function selectProvince(prov) {
  addressData.value.state = prov
  validateField('state')
}

function submit() {
  const fields = ['street', 'city', 'postalCode', 'doorNb', 'state']
  let allValid = true
  fields.forEach((f) => {
    if (!validateField(f)) allValid = false
  })

  if (allValid) {
    emit('save', {
      ...addressData.value,
      country: countryData.value,
      provinceId: addressData.value.state?.id || props.initialAddress?.provinceId,
      countryId: countryData.value?.id || props.initialAddress?.countryId,
    })
  } else {
    Notify.create({
      type: 'negative',
      message: 'Veuillez corriger les erreurs dans le formulaire',
      position: 'top',
    })
  }
}
</script>

<style scoped>
.address-dialog-card {
  width: 100%;
  max-width: 600px;
  background: #1f2837;
  overflow-x: hidden !important;
}

.modal-title {
  color: white;
}

.form-control {
  border-radius: 4px;
  padding: 0 12px;
  font-weight: 500;
  width: 100%;
  height: 42px;
  line-height: 40px;
  display: inline-block;
  vertical-align: middle;
}

.form-label {
  text-align: left;
  width: 100%;
  display: block;
  margin-bottom: 0.5rem;
}

.error-message {
  color: red !important;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.field-error {
  border: 2px solid red !important;
}

/* Light Mode */
body:not(.body--dark) .address-dialog-card {
  background-color: #e8e4d9 !important;
  color: black !important;
}

body:not(.body--dark) .modal-title {
  color: black !important;
}

body:not(.body--dark) .form-control {
  background-color: white;
  color: black;
  border: 1px solid #ccc;
}

body:not(.body--dark) .text-adaptive {
  color: black !important;
}

/* Dark Mode */
body.body--dark .address-dialog-card {
  background-color: #1f2837 !important;
}

body.body--dark .form-control {
  background-color: #455a64;
  color: white;
  border: 1px solid #455a64;
}

body.body--dark .form-select {
  background-color: #455a64;
  color: white;
  border: 1px solid #455a64;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='white' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M2 5l6 6 6-6'/%3e%3c/svg%3e");
}

body.body--dark .text-adaptive {
  color: white !important;
}

body.body--dark .field-error {
  border: 2px solid #ef4444 !important;
}

.mb-3 {
  margin-bottom: 1rem !important;
}

.dropdown-toggle {
  background-color: white;
  color: black;
  border: 1px solid #ccc;
}

.dropdown-toggle::after {
  margin-left: auto;
}

.dropdown-menu {
  max-height: 200px;
  overflow-y: auto;
  border-radius: 4px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.dropdown-menu::-webkit-scrollbar {
  width: 8px;
}

.dropdown-menu::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

.dropdown-menu::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 4px;
}

.dropdown-menu::-webkit-scrollbar-thumb:hover {
  background: #555;
}

body.body--dark .dropdown-toggle {
  background-color: #455a64;
  color: white;
  border: 1px solid #455a64;
}

body.body--dark .dropdown-menu {
  background-color: #1f2837;
  border: 1px solid #455a64;
}

body.body--dark .dropdown-item {
  color: white;
}

body.body--dark .dropdown-item:hover {
  background-color: #2d3c49;
}

body.body--dark .dropdown-menu::-webkit-scrollbar-track {
  background: #1f2837;
}

body.body--dark .dropdown-menu::-webkit-scrollbar-thumb {
  background: #4a5568;
}

body.body--dark .dropdown-menu::-webkit-scrollbar-thumb:hover {
  background: #607d8b;
}
</style>
