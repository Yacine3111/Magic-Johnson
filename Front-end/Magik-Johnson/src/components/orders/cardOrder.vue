<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  item: {
    type: Object,
    required: true,
  },
})

const router = useRouter()

const formatDate = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleDateString('fr-FR')
}

const formatTime = (dateString) => {
  if (!dateString || !dateString.includes('T')) return ''
  return dateString.split('T')[1].substring(0, 8)
}

const deliveryMode = computed(() => {
  if (props.item.deliveryMode === 'HomeDelivery') {
    return 'Livraison '
  } else if (props.item.deliveryMode === 'InStorePickup') {
    return 'Cueillette en magasin'
  }
  return props.item.deliveryMode
})

const allSteps = ref([])

if (props.item.orderStatus.name == 'Remboursée' || props.item.orderStatus.name == 'En attente') {
  allSteps.value = ['En préparation', 'En attente', 'Remboursée']
} else {
  allSteps.value = ['En préparation', 'Expédiée', 'Livrée']
}

const currentStepIndex = computed(() => {
  if (props.item.orderStatus.name == 'Partiellement remboursée') {
    return allSteps.value.indexOf('En préparation')
  }
  return allSteps.value.indexOf(props.item.orderStatus.name)
})

const visibleSteps = computed(() => {
  if (currentStepIndex.value === -1) return []
  return allSteps.value
})

const checkStatus = computed(() => {
  switch (props.item.orderStatus.name) {
    case 'En préparation':
      return 'status status-orange'
    case 'Expédiée':
      return 'status status-blue'
    case 'Livrée':
      return 'status status-green'
    case 'Annulée':
      return 'status status-red'
    case 'En attente':
      return 'status status-orange'
    case 'Remboursée':
      return 'status status-green'
    case 'Partiellement remboursée':
      return 'status status-green'
    default:
      return 'status status-default'
  }
})

const goToDetailsCommande = (orderId) => {
  router.push({ name: 'DetailsCommande', params: { orderId } })
}

const truncateId = (id) => {
  if (!id) return '—'
  return id.toString().substring(0, 8)
}
</script>

<template>
  <div class="full-width">
    <!-- Desktop Version -->
    <q-card class="row q-pa-sm full-width gt-sm">
      <div class="col-12 col-md column justify-center q-gutter-y-md q-pr-md-sm">
        <div>
          <p class="q-mb-xs"><b>Numéro de la commande :</b> #{{ truncateId(item.id) }}</p>
          <p v-if="item.totalQuantity" class="q-mt-sm q-mb-xs">
            <b>Nombre de cartes :</b> {{ item.totalQuantity }}
          </p>
          <p v-if="item.subTotal !== undefined" class="q-mt-sm no-margin">
            <b>Montant total :</b> {{ (item.subTotal + (item.taxeAmount || 0)).toFixed(2) }}$ CAD
          </p>
        </div>

        <div>
          <div class="row q-col-gutter-sm q-mb-md">
            <div class="col-6">
              <q-card class="q-pa-sm text-center full-height flex flex-center">
                <div class="text-caption" style="line-height: 1.2">
                  <strong>Commandé:</strong><br />
                  {{ formatDate(item.checkoutDate) }}<br />
                  {{ formatTime(item.checkoutDate) }}
                </div>
              </q-card>
            </div>
            <div class="col-6">
              <q-card class="q-pa-sm text-center full-height flex flex-center">
                <div class="text-caption" style="line-height: 1.2">
                  <strong>{{ deliveryMode }}:</strong><br />
                  {{ formatDate(item.deliveryDate) }}<br />
                  {{ formatTime(item.deliveryDate) }}
                </div>
              </q-card>
            </div>
          </div>

          <q-btn
            class="full-width"
            color="black"
            label="Détails"
            @click="goToDetailsCommande(item.id)"
          />
        </div>
      </div>

      <div
        class="col-12 col-md-auto column justify-start q-pl-md-sm q-mt-md q-mt-md-none"
        style="min-width: 280px"
      >
        <q-card class="q-pa-md round-card column">
          <p class="q-mb-md" style="white-space: nowrap">
            Statut de la commande : <span :class="checkStatus">{{ item.orderStatus.name }}</span>
          </p>

          <q-stepper
            vertical
            flat
            color="primary"
            class="tight-stepper"
            :model-value="currentStepIndex"
          >
            <q-step
              v-for="(step, index) in visibleSteps"
              :key="index"
              :name="index"
              :title="step"
              :done="index <= currentStepIndex"
              :active-icon="index === visibleSteps.length - 1 ? 'check' : 'edit'"
              :active="index === currentStepIndex && index !== visibleSteps.length - 1"
            />
          </q-stepper>
        </q-card>

        <div class="flex justify-center q-mt-md">
          <q-btn
            v-if="item.orderStatus.name == 'En préparation'"
            class="full-width btn-cancel q-py-xs"
            label="Annuler la commande"
            unelevated
            :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
          />

          <q-btn
            v-else-if="item.orderStatus.name == 'Livrée'"
            class="full-width q-py-xs"
            color="green"
            style="border-radius: 15px"
            label="Remboursement"
            :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
          />

          <q-btn
            v-else-if="item.orderStatus.name == 'Partiellement remboursée'"
            class="full-width q-py-xs"
            color="green"
            style="border-radius: 15px"
            label="Nouvelle demande de remboursement"
            :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
          />
        </div>
      </div>
    </q-card>

    <!-- Mobile Version -->
    <q-card class="lt-md q-pa-md bg-grey-13 full-width column q-gutter-y-sm">
      <!-- Header -->
      <div class="row justify-between items-center">
        <div class="text-subtitle1 text-weight-bold">#{{ truncateId(item.id) }}</div>
        <p class="no-margin" style="font-size: 0.8rem; white-space: nowrap">
          Statut de la commande : <span :class="checkStatus">{{ item.orderStatus.name }}</span>
        </p>
      </div>

      <!-- Info Grid -->
      <q-card class="q-pa-sm bg-white" flat bordered>
        <div class="row q-col-gutter-xs">
          <div class="col-6">
            <div class="text-caption text-grey-8">Articles</div>
            <div class="text-body2">{{ item.totalQuantity }} cartes</div>
          </div>
          <div class="col-6 text-right">
            <div class="text-caption text-grey-8">Montant</div>
            <div class="text-body2 text-weight-bold">
              {{ (item.subTotal + (item.taxeAmount || 0)).toFixed(2) }}$
            </div>
          </div>
          <div class="col-6 q-mt-xs">
            <div class="text-caption text-grey-8">Date commande</div>
            <div class="text-body2 text-weight-medium">{{ formatDate(item.checkoutDate) }}</div>
          </div>
          <div class="col-6 text-right q-mt-xs">
            <div class="text-caption text-grey-8">Livraison</div>
            <div class="text-body2">{{ formatDate(item.deliveryDate) || '—' }}</div>
          </div>
        </div>
      </q-card>

      <!-- Actions -->
      <div class="column q-gutter-y-sm q-mt-sm">
        <q-btn
          color="black"
          label="Voir les détails"
          class="full-width"
          outline
          @click="goToDetailsCommande(item.id)"
        />

        <q-btn
          v-if="item.orderStatus.name == 'En préparation'"
          class="full-width btn-cancel"
          label="Annuler"
          unelevated
          :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
        />

        <q-btn
          v-else-if="item.orderStatus.name == 'Livrée'"
          class="full-width"
          color="green"
          style="border-radius: 15px"
          label="Demander remboursement"
          :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
        />

        <q-btn
          v-else-if="item.orderStatus.name == 'Partiellement remboursée'"
          class="full-width"
          color="green"
          style="border-radius: 15px"
          label="Nouveau remboursement"
          :to="{ name: 'demandeRemboursement', params: { id: item.id } }"
        />
      </div>
    </q-card>
  </div>
</template>

<style scoped>
.status {
  padding: 4px 8px;
  border-radius: 4px;
  color: white;
  font-weight: bold;
}

.status-red {
  background-color: #d32f2f;
}
.status-orange {
  background-color: #f57c00;
}
.status-blue {
  background-color: #1976d2;
}
.status-green {
  background-color: #388e3c;
}
.status-default {
  background-color: #757575;
}

.round-card {
  border-radius: 25px;
}

.btn-cancel {
  background-color: #dc3545 !important;
  color: white;
  border-radius: 15px;
}

.tight-stepper {
  padding: 0;
  background: transparent;
}
:deep(.q-stepper__step-content) {
  display: none !important;
}
:deep(.q-stepper__header) {
  padding: 8px 12px;
  min-height: auto;
}
:deep(.q-stepper__step) {
  padding-bottom: 0 !important;
}
:deep(.q-stepper--vertical .q-stepper__step-inner) {
  padding: 0 !important;
}
</style>

<style>
.bg-grey-13 {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .q-card {
  background-color: #a89c8d !important;
  border: 1px solid black;
}

body:not(.body--dark) .q-card .q-card {
  background-color: #f0eada !important;
  color: black !important;
  border: none !important;
}

body.body--dark .bg-grey-13,
body.body--dark .bg-white,
body.body--dark .q-card {
  background-color: #1f2735 !important;
  color: white !important;
  box-shadow: 0 4px 6px rgba(255, 255, 255, 0.1) !important;
}

body.body--dark strong,
body.body--dark b,
body.body--dark p,
body.body--dark span,
body.body--dark div,
body.body--dark .text-subtitle1,
body.body--dark .q-stepper__title,
body.body--dark .q-stepper__caption {
  color: white !important;
}

body.body--dark .text-grey-8 {
  color: #ccc !important;
}

body.body--dark .text-caption {
  color: #ddd !important;
}

body.body--dark .text-caption {
  color: #ddd !important;
}

body:not(.body--dark)
  .q-stepper__tab:not(.q-stepper__tab--active):not(.q-stepper__tab--done)
  .q-stepper__title {
  color: black !important;
}
body:not(.body--dark)
  .q-stepper__tab:not(.q-stepper__tab--active):not(.q-stepper__tab--done)
  .q-stepper__dot {
  background-color: black !important;
}

body.body--dark .q-stepper__tab--active .q-stepper__dot,
body.body--dark .q-stepper__tab--done .q-stepper__dot {
  background-color: #1976d2 !important;
  color: white !important;
}

body.body--dark .q-stepper__tab--active .q-stepper__title,
body.body--dark .q-stepper__tab--done .q-stepper__title {
  color: #1976d2 !important;
  font-weight: bold;
}
</style>
