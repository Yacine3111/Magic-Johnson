<template>
  <aside class="card panel summary">
    <header class="panel-header">Résumé de commande</header>
    <ul class="totals">
      <li>
        <span class="totals-detail">Montant de la commande :</span>
        <b class="price-value">{{ orderAmount.toFixed(2) }} $</b>
      </li>
      <li v-if="discount != 0">
        <span class="totals-detail">Rabais total :</span>
        <b class="price-value">-{{ discount.toFixed(2) }} $</b>
      </li>
      <li>
        <span class="totals-detail">Nombre d'articles :</span><b>{{ itemCount }}</b>
      </li>
      <li>
        <span class="totals-detail">Livraison :</span>
        <b class="price-value">{{ shippingCost.toFixed(2) }} $</b>
      </li>
    </ul>

    <div class="grand">
      <span>Sous-total :</span>
      <b class="price-value">{{ totalAmount.toFixed(2) }} $</b>
    </div>

    <slot name="actions"></slot>
  </aside>
</template>

<script setup>
defineProps({
  orderAmount: {
    type: Number,
    required: true,
  },
  discount: {
    type: Number,
    default: 0,
  },
  itemCount: {
    type: Number,
    required: true,
  },
  shippingCost: {
    type: Number,
    required: true,
  },
  totalAmount: {
    type: Number,
    required: true,
  },
})
</script>

<style scoped>
.card {
  background: #1f2837;
  border-radius: 16px;
  color: #e6ecff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  border: 1px solid #2b3550;
}

.panel {
  overflow: hidden;
}

.panel-header {
  background: #2e3a5a;
  padding: 14px 18px;
  font-weight: 700;
  letter-spacing: 0.5px;
  border-bottom: 1px solid #3a466d;
  font-size: large;
}

.summary {
  padding-bottom: 12px;
}

.totals {
  list-style: none;
  margin: 0;
  padding: 10px 16px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.totals li {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #cfe1ff;
  white-space: nowrap;
}

.totals-detail {
  font-size: medium;
}

.grand {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 10px 16px;
  padding: 12px;
  background: #1b2231;
  border: 1px solid #3a466d;
  border-radius: 10px;
  font-weight: 800;
  letter-spacing: 0.3px;
  font-size: large;
  white-space: nowrap;
}

/* Light Mode  */
body:not(.body--dark) .card {
  background: #f8f5f0;
  border: 1px solid #000;
  color: #000 !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .panel-header {
  background: #a89c8d;
  color: #000;
  border-bottom: 1px solid #000;
}

body:not(.body--dark) .grand {
  background: #d1cbbb;
  color: #000;
  border: 1px solid #000;
}

body:not(.body--dark) .totals li {
  color: #000 !important;
}

body:not(.body--dark) .totals-detail {
  color: #000 !important;
}

body:not(.body--dark) .price-value {
  color: #000 !important;
}
</style>
