<script setup>
import { onMounted } from 'vue'

onMounted(() => {
  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible')
        }
      })
    },
    { threshold: 0.1 },
  )

  const elements = document.querySelectorAll('.animate-on-scroll')
  elements.forEach((element) => observer.observe(element))
})
</script>
<template>
  <div class="about-page">
    <!-- Hero Section -->
    <div class="hero-section">
      <div class="hero-content">
        <h1 class="hero-title animate-on-scroll">À propos de L'Arène des Stratèges</h1>
        <p class="hero-subtitle animate-on-scroll">
          Votre destination premium pour Magic: The Gathering depuis 1996
        </p>
      </div>
    </div>

    <!-- Main Content -->
    <div class="main-content">
      <!-- Section Notre histoire -->
      <section class="story-section animate-on-scroll">
        <div class="section-number">01</div>
        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Notre histoire</h2>
            <div class="divider"></div>
            <p class="section-text">
              Depuis 1996, L'Arène des Stratèges est votre destination de choix pour les jeux de
              société et les cartes à collectionner dans la grande région de Montréal. Tout a
              commencé avec l'ouverture de notre première succursale à Verdun, animée par une
              passion commune pour l'univers du jeu et la volonté de créer un espace où les
              passionnés pourraient se rassembler.
            </p>
          </div>
          <div class="content-icon">
            <div class="icon-wrapper">
              <span class="material-symbols-outlined icon"> history </span>
            </div>
          </div>
        </div>
      </section>

      <!-- Section Fondateurs -->
      <section class="founders-section animate-on-scroll">
        <div class="section-number">02</div>
        <div class="section-content reverse">
          <div class="content-icon">
            <div class="icon-wrapper">
              <span class="material-symbols-outlined icon"> group </span>
            </div>
          </div>
          <div class="content-text">
            <h2 class="section-title">Nos fondateurs</h2>
            <div class="divider"></div>
            <div class="founders-list">
              <div class="founder-item">
                <h3 class="founder-name">Jean-François Bédard</h3>
                <p class="founder-role">Co-fondateur & Président</p>
                <p class="founder-desc">
                  Guide notre vision d'entreprise avec une approche centrée sur la communauté et
                  l'excellence du service client.
                </p>
              </div>
              <div class="founder-item">
                <h3 class="founder-name">Fabrice Lemieux</h3>
                <p class="founder-role">Co-fondateur & Expert Technique</p>
                <p class="founder-desc">
                  Ancien joueur professionnel, il apporte une expertise technique inégalée et une
                  compréhension profonde des mécaniques de jeu.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Section Expansion -->
      <section class="expansion-section animate-on-scroll">
        <div class="section-number">03</div>
        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Notre expansion</h2>
            <div class="divider"></div>
            <p class="section-text">
              Fort de notre succès initial, nous avons élargi nos horizons en ouvrant une deuxième
              succursale à Chambly, permettant ainsi de servir une clientèle encore plus large.
              Notre entrepôt de Montréal nous permet de maintenir un inventaire diversifié et de
              répondre rapidement aux demandes de nos clients, qu'ils soient collectionneurs
              occasionnels ou joueurs compétitifs.
            </p>
          </div>
          <div class="content-icon">
            <div class="icon-wrapper">
              <span class="material-symbols-outlined icon"> location_on </span>
            </div>
          </div>
        </div>
      </section>

      <!-- Section Expertise -->
      <section class="expertise-section animate-on-scroll">
        <div class="section-number">04</div>
        <div class="section-content reverse">
          <div class="content-icon">
            <div class="icon-wrapper">
              <span class="material-symbols-outlined icon"> auto_awesome </span>
            </div>
          </div>
          <div class="content-text">
            <h2 class="section-title">Notre expertise Magic: The Gathering</h2>
            <div class="divider"></div>
            <p class="section-text">
              Notre équipe suit de près l'évolution du marché, les tendances de prix et les
              nouvelles sorties pour offrir à nos clients un service informé et des prix
              compétitifs. Que vous recherchiez une carte rare pour compléter votre collection ou
              que vous souhaitiez construire le deck parfait pour votre prochaine compétition, nous
              sommes là pour vous accompagner. Notre mission va au-delà de la simple vente : nous
              voulons nourrir la passion du jeu et contribuer à l'épanouissement de la communauté
              des joueurs de cartes à collectionner au Québec.
            </p>
          </div>
        </div>
      </section>

      <!-- appelle a Action -->
      <div class="cta-section">
        <div class="cta-content">
          <h2 class="cta-title">Rejoignez l'aventure</h2>
          <p class="cta-text">Découvrez notre collection et vivez la passion de Magic</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="../css/AboutPage.css" scoped></style>
