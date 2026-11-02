<template>
  <div class="not-found-container">
    <div class="not-found-card">
      <h1>404</h1>
      <h2>Page introuvable</h2>
      <p>La page que vous recherchez n'existe pas ou a été déplacée.</p>

      <router-link to="/" class="return-btn"> Retourner à l'accueil </router-link>
    </div>
  </div>
</template>

<script setup></script>

<style scoped>
.not-found-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  background: linear-gradient(135deg, #f7f7f7, #ececec);
  padding: 20px;
}

.not-found-card {
  background: white;
  padding: 50px 40px;
  max-width: 500px;
  width: 100%;
  border-radius: 20px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
  text-align: center;
  animation: fadeIn 0.5s ease-in-out;
}

h1 {
  font-size: 80px;
  margin-bottom: 10px;
  color: #e44d26;
  font-weight: 800;
}

h2 {
  font-size: 26px;
  margin-bottom: 20px;
  color: #333;
}

p {
  font-size: 16px;
  color: #666;
  margin-bottom: 40px;
}

.return-btn {
  display: inline-block;
  padding: 12px 25px;
  background-color: #e44d26;
  color: white;
  border-radius: 8px;
  text-decoration: none;
  font-size: 16px;
  box-shadow: 0 5px 14px rgba(228, 77, 38, 0.3);
  transition: 0.2s ease;
}

.return-btn:hover {
  background-color: #c23f21;
  transform: translateY(-2px);
  box-shadow: 0 8px 18px rgba(228, 77, 38, 0.35);
}

/* Petite animation */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
