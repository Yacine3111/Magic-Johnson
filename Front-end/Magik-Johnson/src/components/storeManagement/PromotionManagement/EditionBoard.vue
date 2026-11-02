<template>
  <div class="edit-form q-mb-md">
    <div class="text-h6 q-mb-md">
      {{ isNewPromotion ? 'Créer une nouvelle promotion' : 'Modifier la promotion' }}
    </div>
    <div class="row q-gutter-md q-mb-md">
      <div class="col form-field">
        <label class="form-label">Nom promotion *</label>
        <input
          :value="editingPromotion.nom"
          @input="
            $emit('update:editingPromotion', { ...editingPromotion, nom: $event.target.value })
          "
          class="form-input"
          :class="{ 'has-error': !!validationErrors?.nom }"
          placeholder="Ex: Soldes d'été"
          maxlength="60"
        />
        <span v-if="validationErrors?.nom" class="error-msg">{{ validationErrors.nom }}</span>
      </div>
      <div class="col status-selector">
        <div class="text-subtitle2 q-mb-xs text-dark">Statut</div>
        <div class="row q-gutter-sm">
          <q-btn
            :color="editingPromotion.statut !== 'Inactif' ? 'green' : 'grey-5'"
            :text-color="editingPromotion.statut !== 'Inactif' ? 'white' : 'black'"
            label="Active"
            unelevated
            no-caps
            class="status-btn"
            @click="$emit('update:editingPromotion', { ...editingPromotion, statut: 'Actif' })"
          />
          <q-btn
            :color="editingPromotion.statut === 'Inactif' ? 'red' : 'grey-5'"
            :text-color="editingPromotion.statut === 'Inactif' ? 'white' : 'black'"
            label="Inactive"
            unelevated
            no-caps
            class="status-btn"
            @click="$emit('update:editingPromotion', { ...editingPromotion, statut: 'Inactif' })"
          />
        </div>
      </div>
    </div>
    <div class="row q-gutter-md q-mb-md">
      <div class="col form-field">
        <label class="form-label">Date début *</label>
        <input
          type="date"
          :value="editingPromotion.debut"
          @input="
            $emit('update:editingPromotion', { ...editingPromotion, debut: $event.target.value })
          "
          class="form-input"
          :class="{ 'has-error': !!validationErrors?.debut }"
        />
        <span v-if="validationErrors?.debut" class="error-msg">{{ validationErrors.debut }}</span>
      </div>
      <div class="col form-field">
        <label class="form-label">Date expiration *</label>
        <input
          type="date"
          :value="editingPromotion.fin"
          @input="
            $emit('update:editingPromotion', { ...editingPromotion, fin: $event.target.value })
          "
          class="form-input"
          :class="{ 'has-error': !!validationErrors?.fin }"
        />
        <span v-if="validationErrors?.fin" class="error-msg">{{ validationErrors.fin }}</span>
      </div>
    </div>
    <div class="row q-gutter-md q-mb-md">
      <div class="col form-field">
        <label class="form-label">Rabais (%) *</label>
        <input
          type="number"
          :value="editingPromotion.rabais"
          @input="
            $emit('update:editingPromotion', {
              ...editingPromotion,
              rabais: Number($event.target.value),
            })
          "
          class="form-input"
          min="0"
          max="100"
          :class="{ 'has-error': !!validationErrors?.rabais }"
        />
        <span v-if="validationErrors?.rabais" class="error-msg">{{ validationErrors.rabais }}</span>
      </div>
    </div>
    <div class="row q-gutter-md q-mb-md">
      <div class="col serie-select-wrapper">
        <label class="text-subtitle2 q-mb-xs text-dark">Série (optionnel)</label>
        <div class="serie-select-container">
          <select
            :value="editingPromotion.selectedSerie || ''"
            @change="
              $emit('update:editingPromotion', {
                ...editingPromotion,
                selectedSerie: $event.target.value || null,
              })
            "
            class="form-select form-select-sm serie-select"
          >
            <option value="">Sélectionner une série</option>
            <option v-for="serie in seriesOptions" :key="serie" :value="serie">
              {{ serie }}
            </option>
          </select>
          <q-btn
            v-if="editingPromotion.selectedSerie"
            label="Ajouter la série"
            color="primary"
            unelevated
            class="add-series-btn"
            @click="$emit('add-series')"
          />
        </div>
        <div class="text-caption q-mt-xs text-grey-7">
          Sélectionner une série pour ajouter toutes ses cartes à la promotion
        </div>
      </div>
    </div>
    <div class="q-mb-md">
      <div class="form-field">
        <label class="form-label">Description *</label>
        <textarea
          :value="editingPromotion.description"
          @input="
            $emit('update:editingPromotion', {
              ...editingPromotion,
              description: $event.target.value,
            })
          "
          class="form-textarea full-width no-resize"
          rows="4"
          placeholder="Entrez une description détaillée de la promotion..."
          maxlength="300"
          :class="{ 'has-error': !!validationErrors?.description }"
        ></textarea>
        <div class="row justify-between items-start">
          <span v-if="validationErrors?.description" class="error-msg">{{
            validationErrors.description
          }}</span>
          <span v-else class="col"></span>

          <div class="text-caption text-grey-7 q-mt-xs">
            {{ editingPromotion.description ? editingPromotion.description.length : 0 }} / 300
          </div>
        </div>
      </div>
    </div>
    <div class="row q-gutter-md justify-end">
      <q-btn color="green-8" label="Sauvegarder" class="text-white" @click="$emit('save')" />
      <q-btn color="grey-8" label="Annuler" class="text-white" @click="$emit('cancel')" />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  editingPromotion: Object,
  validationErrors: Object,
  seriesOptions: {
    type: Array,
    default: () => [],
  },
})

defineEmits(['save', 'cancel', 'update:editingPromotion', 'add-series'])

const isNewPromotion = computed(() => {
  return !props.editingPromotion || props.editingPromotion.id === null
})
</script>

<style scoped>
.edit-form {
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  flex-shrink: 0;
}

/* Light Mode  */
body:not(.body--dark) .edit-form {
  background-color: #f5f5f5;
  border: 1px solid #d1c0a8;
}

body:not(.body--dark) .edit-form .text-h6 {
  color: #5d4037;
}

body:not(.body--dark) .serie-select {
  background-color: white;
  color: #333;
  border: 1px solid #ccc;
}

body:not(.body--dark) .serie-select option {
  background-color: white;
  color: #333;
}

/* Dark Mode  */
body.body--dark .edit-form {
  background-color: #2d3748;
  border: 1px solid #4a5568;
  color: #e2e8f0;
}

body.body--dark .edit-form .text-h6 {
  color: #e2e8f0;
}

body.body--dark .serie-select {
  background-color: #1a202c;
  color: #e2e8f0;
  border: 1px solid #4a5568;
}

body.body--dark .serie-select option {
  background-color: #1a202c;
  color: #e2e8f0;
}

body.body--dark .text-dark {
  color: #e2e8f0 !important;
}

body.body--dark .text-grey-7 {
  color: #a0aec0 !important;
}

/*  Styles commun */
.edit-form .text-h6 {
  margin-top: 0;
}

.full-width {
  width: 100%;
}

.no-resize {
  resize: none !important;
}

.status-btn {
  min-width: 80px;
  border-radius: 4px;
  font-weight: 500;
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
}

.status-btn:hover {
  transform: scale(1.02);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.status-btn:active {
  transform: scale(0.98);
  transition: transform 0.1s ease;
}

.row .col {
  min-width: 0;
  flex: 1 1 0;
}

.status-selector {
  flex: 0 0 auto;
  min-width: 200px;
}

.serie-select-wrapper {
  width: 100%;
}

.serie-select-container {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.serie-select {
  flex: 1;
  min-width: 0;
  padding: 8px;
  border-radius: 4px;
  font-size: 14px;
  cursor: pointer;
}

.serie-select:focus,
.serie-select:active,
.serie-select:hover {
  outline: none;
}

body:not(.body--dark) .serie-select:focus {
  border-color: #1976d2;
}

body.body--dark .serie-select:focus {
  border-color: #63b3ed;
}

.serie-select option:checked {
  color: #333;
  background-color: #f0f0f0;
}

.add-series-btn {
  min-width: 200px;
  height: 40px;
  white-space: nowrap;
  flex-shrink: 0;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}

.form-label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

body.body--dark .form-label {
  color: #e2e8f0;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 10px 12px;
  border-radius: 4px;
  border: 1px solid #ccc;
  font-size: 14px;
  background-color: white;
  color: #333;
  transition:
    border-color 0.2s,
    box-shadow 0.2s;
  outline: none;
}

body.body--dark .form-input,
body.body--dark .form-textarea {
  background-color: #1a202c;
  color: #e2e8f0;
  border-color: #4a5568;
}

.form-input:focus,
.form-textarea:focus {
  border-color: #1976d2;
  box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.2);
}

body.body--dark .form-input:focus,
body.body--dark .form-textarea:focus {
  border-color: #63b3ed;
  box-shadow: 0 0 0 2px rgba(99, 179, 237, 0.2);
}

.form-input.has-error,
.form-textarea.has-error {
  border-color: #ff4444;
}

.error-msg {
  font-size: 12px;
  color: #ff4444;
  margin-top: 4px;
  font-weight: 500;
}

.serie-select-wrapper {
  width: 100%;
}
</style>
