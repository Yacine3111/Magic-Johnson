<template>
  <q-dialog
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    persistent
    class="custom-confirm-dialog"
  >
    <q-card class="custom-dialog-card q-dialog-plugin">
      <q-card-section class="confirm-modal-content">
        <div class="confirm-icon-wrapper">
          <q-icon name="help_outline" class="confirm-icon" />
        </div>
        <div class="confirm-title">Confirmation d'adresse</div>
        <div class="confirm-text">
          <slot>
            Voulez-vous enregistrer cette adresse pour le client
            <strong>{{ clientName }}</strong> ?
          </slot>
        </div>
      </q-card-section>

      <q-card-actions class="confirm-actions">
        <q-btn
          label="NON, UTILISER SEULEMENT POUR CETTE COMMANDE"
          class="confirm-btn-no"
          flat
          unelevated
          no-caps
          v-close-popup
          @click="$emit('cancel')"
        />
        <q-btn
          label="OUI, ENREGISTRER L'ADRESSE"
          class="confirm-btn-yes"
          flat
          unelevated
          no-caps
          v-close-popup
          @click="$emit('confirm')"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
  clientName: {
    type: String,
    default: '',
  },
})

defineEmits(['update:modelValue', 'confirm', 'cancel'])
</script>

<style>
.custom-dialog-card {
  background-color: #1e2636;
  color: white;
  border-radius: 8px;
  padding: 24px;
  max-width: 900px;
  min-width: 800px;
  width: 100%;
}

.confirm-modal-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 0;
}

.confirm-icon-wrapper {
  margin-bottom: 16px;
}

.confirm-icon {
  font-size: 64px;
  color: white;
  border: 4px solid white;
  border-radius: 50%;
  padding: 8px;
}

.confirm-title {
  font-size: 20px;
  font-weight: 500;
  margin-bottom: 8px;
}

.confirm-text {
  font-size: 16px;
  margin-bottom: 24px;
  line-height: 1.4;
}

.confirm-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  padding: 0;
}

.confirm-btn-yes {
  background-color: #1976d2 !important;
  color: white !important;
  font-weight: bold;
  padding: 10px 20px;
  border-radius: 4px;
  font-size: 14px;
  white-space: nowrap;
}

.confirm-btn-no {
  background-color: #c62828 !important;
  color: white !important;
  font-weight: bold;
  padding: 10px 20px;
  border-radius: 4px;
  font-size: 14px;
  white-space: nowrap;
}

/* Light Mode */
body:not(.body--dark) .custom-dialog-card {
  background-color: #a89c8d !important;
  color: white !important;
}

body:not(.body--dark) .confirm-icon {
  color: #1976d2 !important;
  border-color: #1976d2 !important;
}
</style>
