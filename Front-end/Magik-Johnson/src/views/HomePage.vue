<script setup>
import PromotionsSection from '@/components/promotionsSection.vue'
import trendySection from '@/components/trendySection.vue'
import { ref } from 'vue'
import MostSoldSection from '@/components/mostSoldSection.vue'

const Unelevated_button_text = ref('Voir le catalogue de cartes')
const hasPromotions = ref(false)

const onPromotionsUpdated = (hasPromos) => {
  hasPromotions.value = hasPromos
}
</script>

<template>
  <div class="main_container">
    <div class="homeBanner">
      <div class="bannerCurtain"></div>
      <div class="bannerContent">
        <p class="welcomeText">BIENVENUE SUR LE SITE DE VENTE OFFICIEL DE</p>
        <p class="compagnyTitle">L'ARÈNE DES STRATÈGES</p>
      </div>

      <div class="ticker-container">
        <div class="ticker-content">
          <span
            ><span>
              Votre destination premium pour Magic: The Gathering depuis 1996 • Promotions variées
              et rabais exclusifs toutes les semaines • Les derniers prix compétitifs sur des
              milliers de cartes • Cartes rares et nouvelles sorties • Livraison gratuite dès 100$
              d'achat • Votre communauté MTG au Québec •
            </span>
          </span>
        </div>
      </div>
    </div>
    <PromotionsSection @promotions-updated="onPromotionsUpdated" />
    <RouterLink to="/Catalogue"
      ><button unelevated class="catalog_button">{{ Unelevated_button_text }}</button></RouterLink
    >
    <hr />
    <trendySection />
    <MostSoldSection />
  </div>
</template>

<style scoped>
.main_container {
  min-height: 100vh;
  width: 100%;
  background-color: #f0eada;
  overflow-x: hidden;
  transition: background-color 0s;
}

.homeBanner {
  background-image: url('@/assets/Logo/homeBanner.png');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  image-rendering: optimizeQuality;
  height: 60vh;
  min-height: 400px;
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
.bannerContent {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  color: white;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 100%;
  max-width: 100%;
  padding: 15px;
  box-sizing: border-box;
  z-index: 2;
}
.bannerContent p {
  flex-basis: auto;
  padding: 10px 15px;
  text-align: center;
  margin: 0;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  word-wrap: break-word;
  overflow-wrap: break-word;
}
.bannerCurtain {
  width: 100%;
  height: 100%;
  position: absolute;
  top: 0;
  left: 0;
  background-color: black;
  opacity: 40%;
  z-index: 1;
}
.compagnyTitle {
  font-size: 100px;
  line-height: 1.1;
  font-weight: bold;
}
.welcomeText {
  font-size: 30px;
  line-height: 1.3;
}

.ticker-container {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  overflow: hidden;
  padding: 8px 0;
  z-index: 10;
}

.ticker-content {
  display: flex;
  white-space: nowrap;
  animation: scroll-left 30s linear infinite;
}

.ticker-content span {
  color: white;
  font-size: 14px;
  font-weight: 500;
  padding-right: 50px;
  letter-spacing: 0.5px;
}

@keyframes scroll-left {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-50%);
  }
}

.ticker-container:hover .ticker-content {
  animation-play-state: paused;
}

.catalog_button {
  margin: 8px;
  width: fit-content;
  font-size: large;
  background-color: #708090;
  border-radius: 5px;
  color: white;
  padding: 10px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.15);
  transition: box-shadow 0.5s ease-in-out;
}
.catalog_button:hover {
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.5);
}
.catalog_button:active {
  transform: scale(1.04);
}

/* Media queries pour mobile */
@media (max-width: 768px) {
  .homeBanner {
    height: 50vh;
    min-height: 350px;
    background-size: cover;
    background-position: center;
  }

  .bannerContent {
    padding: 10px;
    top: 45%;
  }

  .compagnyTitle {
    font-size: 32px;
    line-height: 1.2;
    padding: 5px 10px;
  }

  .welcomeText {
    font-size: 14px;
    line-height: 1.4;
    padding: 5px 10px;
  }

  .bannerContent p {
    padding: 5px 10px;
  }

  .ticker-content span {
    font-size: 11px;
    padding-right: 30px;
  }
}

@media (max-width: 480px) {
  .homeBanner {
    height: 45vh;
    min-height: 300px;
  }

  .bannerContent {
    padding: 8px;
    top: 40%;
  }

  .compagnyTitle {
    font-size: 24px;
    line-height: 1.2;
    padding: 3px 8px;
  }

  .welcomeText {
    font-size: 12px;
    line-height: 1.3;
    padding: 3px 8px;
  }

  .bannerContent p {
    padding: 3px 8px;
  }

  .ticker-container {
    padding: 6px 0;
  }

  .ticker-content span {
    font-size: 10px;
    padding-right: 25px;
  }
}
</style>

<style>
body.body--dark .main_container {
  background-color: rgb(35, 42, 56) !important;
}

body.body--dark .titre_promotion {
  color: white !important;
}
</style>
