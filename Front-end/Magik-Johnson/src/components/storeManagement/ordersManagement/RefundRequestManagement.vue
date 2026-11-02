<script setup>
import { ref, onMounted, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { GetRefundRequest } from '@/api/payment/RefundRequestApi'
import { ProcessRefundRequest } from '@/api/payment/RefundRequestApi'
import { Loading, Notify } from 'quasar'

const router = useRouter()

const selectedStatus = ref('en attente')
const refundRequests = ref([])
const decisionToRefund = reactive({})
const comment = ref('')

const notify = (type, message) => {
  Notify.create({
    type,
    message,
    positino: 'top',
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
}

const loadRefundRequests = async () => {
  const data = await GetRefundRequest()
  refundRequests.value = data || { refunds: [] }
}

async function handleDecisionClick(item) {
  const decision = decisionToRefund[item.id]
  if (!decision) return

  try {
    Loading.show({ spinnerColor: 'primary' })

    await ProcessRefundRequest(item, decision)
    await loadRefundRequests()

    notify('positive', 'Statut de commande mis à jour')
  } catch (e) {
    notify('negative', 'Une erreur est survenue')
  } finally {
    Loading.hide()
  }
}

function showComment(itemComment) {
  comment.value = itemComment
}

onMounted(loadRefundRequests)

const filteredRequests = computed(() => {
  const arr =
    refundRequests.value && refundRequests.value.refunds ? refundRequests.value.refunds : []
  return arr.filter(
    (r) => (r.status || '').toLowerCase() === (selectedStatus.value || '').toLowerCase(),
  )
})
</script>
<template>
  <div class="order-container">
    <div class="title-section">
      <div class="text-h5 text-bold">Gestion des demandes</div>
    </div>
    <div class="navbar-container">
      <div class="segmented">
        <label class="segmented-button">
          <input
            type="radio"
            name="segmented"
            value="en attente"
            v-model="selectedStatus"
            checked
          />
          En attente
        </label>
        <label class="segmented-button">
          <input type="radio" name="segmented" value="acceptée" v-model="selectedStatus" />
          Acceptées
        </label>
        <label class="segmented-button">
          <input type="radio" name="segmented" value="refusée" v-model="selectedStatus" />
          Refusées
        </label>
      </div>
    </div>
    <table>
      <thead>
        <tr>
          <th>Id commande</th>
          <th>Remboursement</th>
          <th>Montant</th>
          <th>Type</th>
          <th>Motif</th>
          <th>Commentaire</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in filteredRequests" :key="item.id">
          <td>
            <a
              href="#"
              @click.prevent="router.push(`/details-commande/${item.orderId}`)"
              class="order-id-link"
            >
              {{ item.orderId.slice(0, 6) }}
            </a>
          </td>
          <td>{{ parseFloat(item.amountToRefund).toFixed(2) }} $</td>
          <td>{{ parseFloat(item.totalAmount).toFixed(2) }} $</td>
          <td>{{ item.isPartialRefund ? 'Partiel' : 'Complet' }}</td>
          <td>{{ item.refundJustification }}</td>
          <td>
            <button
              @click="showComment(item.comments)"
              type="button"
              class="btn comment-btn"
              data-bs-toggle="modal"
              data-bs-target="#staticBackdrop"
            >
              Voir
            </button>
          </td>

          <td class="d-flex justify-content-around">
            <label v-if="selectedStatus == 'en attente'" class="btn btn-success btn-sm">
              <input
                class="btn-check"
                type="radio"
                :name="`decision-${item.id}`"
                value="acceptée"
                v-model="decisionToRefund[item.id]"
                @change="handleDecisionClick(item)"
              />
              Accepter
            </label>

            <label v-if="selectedStatus == 'en attente'" class="btn btn-danger btn-sm">
              <input
                class="btn-check"
                type="radio"
                :name="`decision-${item.id}`"
                value="refusée"
                v-model="decisionToRefund[item.id]"
                @change="handleDecisionClick(item)"
              />
              Refuser
            </label>
          </td>
        </tr>

        <tr v-if="filteredRequests.length === 0">
          <td colspan="6">Aucun résultat pour "{{ selectedStatus }}"</td>
        </tr>
      </tbody>
    </table>
  </div>
  <!-- Modal -->
  <div
    class="modal fade"
    id="staticBackdrop"
    data-bs-backdrop="static"
    data-bs-keyboard="false"
    tabindex="-1"
    aria-labelledby="staticBackdropLabel"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="staticBackdropLabel">Commentaire</h5>
          <button
            type="button"
            class="btn-close"
            data-bs-dismiss="modal"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body">{{ comment ? comment : 'N/A' }}</div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Fermer</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.order-container {
  background-color: #676767;
  min-height: 100vh;
  padding-bottom: 20px;
}

.title-section {
  background-color: #959595;
  padding: 12px 16px;
  width: fit-content;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  flex-shrink: 0;
}

/*Css pour le tableau*/

hr {
  border: none;
  height: 4px;
  background-color: rgb(255, 255, 255);
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead th {
  padding: 10px;
  color: white;
}

tbody td {
  padding: 10px;
  font-size: small;
}

tbody tr {
  background-color: #9c9a9a;
  color: #fff;
  border-top: 2px solid #ffffff;
  border-bottom: 2px solid #ffffff;
  transition:
    opacity 0.3s ease,
    background-color 0.3s ease;
}

.profil {
  color: #89412b !important;
}

.btn-gerer {
  color: white !important;
  background: #5d4037 !important;
  font-weight: bold !important;
  width: fit-content !important;
  min-width: auto !important;
}

.comment-btn {
  font-size: 12px;
  color: white;
  border: 1px solid white;
  border-radius: 4px;
}

.order-id-link {
  color: #4a9eff;
  text-decoration: underline;
  cursor: pointer;
  font-weight: 500;
  transition: color 0.2s ease;
}

.order-id-link:hover {
  color: #6bb3ff;
  text-decoration: underline;
}

body:not(.body--dark) .order-id-link {
  color: #0066cc;
}

body:not(.body--dark) .order-id-link:hover {
  color: #0052a3;
}

/*Css + animation du bouton du status */

.navbar-container {
  display: grid;
  place-items: start;
}

.segmented {
  --padding: 0.25rem;
  display: flex;
  padding: var(--padding);
  border-radius: 10px;
  background-color: light-dark(#232a38, #1a1c20);
  width: fit-content;
  color: white;
}

.segmented-button {
  background-color: transparent;
  border: none;
  margin-bottom: 0px !important;
  border-radius: inherit;
  padding: 0.65rem 1.3rem;
  font-size: inherit;
  cursor: pointer;
  color: inherit;
  transition: all 0.2s ease;
  position: relative;
  z-index: 1;

  &:hover {
    background-color: light-dark(
      #232a38,
      #1a1c20
    ); /* Same as container to look like "hover" if needed, or slightly lighter */
    background-color: rgba(255, 255, 255, 0.1);
  }

  &:has(:checked) {
    background-color: light-dark(white, #333537);
    color: black;
    box-shadow:
      0 0 0 1px light-dark(#c7c8c8, #484a4c),
      0 1px 3px 0 rgb(0 0 0 / 0.1),
      0 1px 2px -1px rgb(0 0 0 / 0.1);
  }

  &:has(:focus-visible) {
    outline: 2px solid #007acc;
  }

  input {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border-width: 0;
  }
}

body:not(.body--dark) .order-container {
  background-color: #d1cbbb !important;
  color: black !important;
}

body:not(.body--dark) .title-section {
  background-color: #a89c8d !important;
}

body:not(.body--dark) .title-section .text-h5 {
  color: black !important;
}

body:not(.body--dark) .segmented {
  background-color: #a89c8d !important;
  color: black !important;
}

body:not(.body--dark) .segmented-button:hover {
  background-color: rgba(255, 255, 255, 0.3) !important;
}

body:not(.body--dark) .segmented-button:has(:checked) {
  background-color: white !important;
  color: black !important;
  box-shadow: 0 0 0 1px #ccc !important;
}

body:not(.body--dark) thead th {
  background-color: transparent !important;
  color: black !important;
  border-bottom: 1px solid black !important;
}

body:not(.body--dark) tbody tr {
  background-color: #efebe9 !important;
  color: black !important;
  border-top: 1px solid #e0e0e0 !important;
  border-bottom: 1px solid #e0e0e0 !important;
}

body:not(.body--dark) tbody td {
  color: black !important;
}

body:not(.body--dark) .comment-btn {
  color: black !important;
  border: 1px solid black !important;
  border-radius: 4px;
}

body.body--dark .segmented-button:has(:checked) {
  background-color: white !important;
  color: black !important;
  box-shadow: 0 0 0 1px #484a4c !important;
}

body.body--dark .segmented-button:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

body.body--dark .modal-content {
  background-color: #2d3748;
  color: white;
  border: 1px solid #4a5568;
}

body.body--dark .modal-header,
body.body--dark .modal-footer {
  border-color: rgba(255, 255, 255, 0.1);
}

body.body--dark .btn-close {
  filter: invert(1) grayscale(100%) brightness(200%);
}
</style>
