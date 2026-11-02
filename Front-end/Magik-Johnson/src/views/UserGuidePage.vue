<script setup>
import { onMounted, computed } from 'vue'

import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()

const isManager = computed(() => {
  return authStore.role?.includes('manager')
})

const isEmployee = computed(() => {
  return authStore.isUserEmployee || isManager.value
})

const section3Prefix = computed(() => (isManager.value ? '3' : '1'))

const section4Prefix = computed(() => (isManager.value ? '4' : isEmployee.value ? '2' : '1'))

const section5Prefix = computed(() => (isManager.value ? '5' : isEmployee.value ? '3' : '2'))

onMounted(async () => {
  if (!authStore.role) {
    await authStore.fetchRole()
  }

  // Animations douces au scroll
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
  <div class="guide-page">
    <!-- Hero Section -->

    <div class="hero-section">
      <div class="hero-content">
        <h1 id="guide-utilisation" class="hero-title animate-on-scroll">Guide d'utilisation</h1>

        <p class="hero-subtitle animate-on-scroll">
          Retrouvez ici les explications détaillées pour utiliser le site officiel de L'Arène des
          Stratèges.
        </p>
      </div>
    </div>

    <!-- Main Content -->

    <div class="main-content">
      <!-- Table des matières -->

      <section class="toc-section animate-on-scroll">
        <div class="section-number">00</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Table des matières</h2>

            <div class="divider"></div>

            <nav class="toc-nav">
              <div v-if="isManager">
                <h3 class="toc-category">Pour les managers</h3>

                <ul class="toc-list">
                  <li>
                    <a href="#creation-employe" class="toc-link">1. Créer un nouvel employé</a>
                    <ul class="toc-sublist">
                      <li>
                        <a href="#creation-employe-acceder" class="toc-link"
                          >1.1 Accéder à la page de création d'employé</a
                        >
                      </li>
                      <li>
                        <a href="#creation-employe-remplir" class="toc-link"
                          >1.2 Remplir le formulaire</a
                        >
                      </li>
                      <li>
                        <a href="#creation-employe-soumettre" class="toc-link"
                          >1.3 Soumettre le formulaire</a
                        >
                      </li>
                    </ul>
                  </li>

                  <li>
                    <a href="#creation-promotion" class="toc-link"
                      >2. Créer une nouvelle promotion</a
                    >
                    <ul class="toc-sublist">
                      <li>
                        <a href="#creation-promotion-acceder" class="toc-link"
                          >2.1 Accéder à la page de création de promotion</a
                        >
                      </li>
                      <li>
                        <a href="#creation-promotion-informations" class="toc-link"
                          >2.2 Informations de base de la promotion</a
                        >
                      </li>
                      <li>
                        <a href="#creation-promotion-periode" class="toc-link"
                          >2.3 Période de validité</a
                        >
                      </li>
                      <li>
                        <a href="#creation-promotion-rabais" class="toc-link">2.4 Rabais</a>
                      </li>
                      <li>
                        <a href="#creation-promotion-cartes" class="toc-link"
                          >2.5 Ajouter des cartes à la promotion</a
                        >
                      </li>
                      <li>
                        <a href="#creation-promotion-description" class="toc-link"
                          >2.6 Description</a
                        >
                      </li>
                      <li>
                        <a href="#creation-promotion-sauvegarder" class="toc-link"
                          >2.7 Sauvegarder ou annuler</a
                        >
                      </li>
                    </ul>
                  </li>
                </ul>
              </div>

              <div v-if="isEmployee">
                <h3 class="toc-category">Pour les employés</h3>

                <ul class="toc-list">
                  <li>
                    <a href="#gestion-remboursement" class="toc-link"
                      >{{ section3Prefix }}. Gestion des demandes de remboursement</a
                    >
                    <ul class="toc-sublist">
                      <li>
                        <a href="#gestion-remboursement-acceder" class="toc-link"
                          >{{ section3Prefix }}.1 Accéder à la page de gestion des demandes</a
                        >
                      </li>
                      <li>
                        <a href="#gestion-remboursement-tableau" class="toc-link"
                          >{{ section3Prefix }}.2 Comprendre le tableau des demandes</a
                        >
                      </li>
                      <li>
                        <a href="#gestion-remboursement-commentaire" class="toc-link"
                          >{{ section3Prefix }}.3 Consulter le commentaire détaillé</a
                        >
                      </li>
                      <li>
                        <a href="#gestion-remboursement-accepter-refuser" class="toc-link"
                          >{{ section3Prefix }}.4 Accepter ou refuser une demande (statut « en
                          attente »)</a
                        >
                      </li>
                      <li>
                        <a href="#gestion-remboursement-notes" class="toc-link"
                          >{{ section3Prefix }}.5 Notes importantes</a
                        >
                      </li>
                    </ul>
                  </li>
                </ul>
              </div>

              <h3 class="toc-category">Pour les clients</h3>

              <ul class="toc-list">
                <li>
                  <a href="#recherche-avancee" class="toc-link"
                    >{{ section4Prefix }}. Recherche avancée</a
                  >
                  <ul class="toc-sublist">
                    <li>
                      <a href="#recherche-avancee-acceder" class="toc-link"
                        >{{ section4Prefix }}.1 Accéder à la recherche avancée</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-barre" class="toc-link"
                        >{{ section4Prefix }}.2 Utiliser la barre de recherche</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-filtres" class="toc-link"
                        >{{ section4Prefix }}.3 Utiliser les filtres avancés</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-trier" class="toc-link"
                        >{{ section4Prefix }}.4 Trier les résultats</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-combiner" class="toc-link"
                        >{{ section4Prefix }}.5 Combiner et réinitialiser les filtres</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-consulter" class="toc-link"
                        >{{ section4Prefix }}.6 Consulter les résultats</a
                      >
                    </li>
                    <li>
                      <a href="#recherche-avancee-notes" class="toc-link"
                        >{{ section4Prefix }}.7 Notes importantes</a
                      >
                    </li>
                  </ul>
                </li>

                <li>
                  <a href="#passer-commande" class="toc-link"
                    >{{ section5Prefix }}. Passer une commande</a
                  >
                  <ul class="toc-sublist">
                    <li>
                      <a href="#passer-commande-catalogue" class="toc-link"
                        >{{ section5Prefix }}.1 Accéder au catalogue et choisir des cartes</a
                      >
                    </li>
                    <li>
                      <a href="#passer-commande-panier" class="toc-link"
                        >{{ section5Prefix }}.2 Vérifier et modifier son panier</a
                      >
                    </li>
                    <li>
                      <a href="#passer-commande-livraison" class="toc-link"
                        >{{ section5Prefix }}.3 Saisir l'adresse de livraison et choisir le mode
                        d'expédition</a
                      >
                    </li>
                    <li>
                      <a href="#passer-commande-payer" class="toc-link"
                        >{{ section5Prefix }}.4 Valider le récapitulatif et payer</a
                      >
                    </li>
                    <li>
                      <a href="#passer-commande-confirmation" class="toc-link"
                        >{{ section5Prefix }}.5 Confirmation et suivi de commande</a
                      >
                    </li>
                    <li>
                      <a href="#passer-commande-notes" class="toc-link"
                        >{{ section5Prefix }}.6 Notes importantes</a
                      >
                    </li>
                  </ul>
                </li>
              </ul>
            </nav>
          </div>
        </div>
      </section>

      <!-- Section 1: Création d'employé -->

      <section v-if="isManager" id="creation-employe" class="guide-section animate-on-scroll">
        <div class="section-number">01</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Créer un nouvel employé</h2>

            <div class="divider"></div>

            <h3 id="creation-employe-acceder" class="subsection-title">
              1.1 Accéder à la page de création d'employé
            </h3>

            <p class="section-text">
              Pour commencer, repérez le bouton <strong>« Créer un nouvel employé »</strong> dans la
              page de gestion des employés, et cliquez dessus.
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_ajouter_employe.png"
                alt="Bouton Créer un nouvel employé dans la page de gestion"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Vous serez redirigé vers la page de création d'employé suivante :
            </p>

            <p class="section-text"><strong>Formulaire de création d'employé</strong></p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_formulaire_employe.png"
                alt="Formulaire de création d'employé"
                class="guide-image"
              />
            </div>

            <p class="section-text">Le formulaire contient les champs suivants :</p>

            <ul class="guide-list">
              <li><strong>Prénom</strong> : Entrez le prénom de l'employé (obligatoire)</li>

              <li><strong>Nom</strong> : Entrez le nom de famille de l'employé (obligatoire)</li>

              <li>
                <strong>Adresse courriel</strong> : Entrez l'adresse email de l'employé
                (obligatoire, format email valide requis)
              </li>

              <li>
                <strong>Numéro de téléphone</strong> : Entrez le numéro de téléphone au format (###)
                ### - #### (obligatoire)
              </li>

              <li>
                <strong>Date d'embauche</strong> : Sélectionnez la date d'embauche de l'employé
                (obligatoire)
              </li>

              <li>
                <strong>Mot de passe</strong> : Créez un mot de passe pour le compte (obligatoire)

                <ul>
                  <li>Doit contenir au moins 8 caractères</li>

                  <li>
                    Doit contenir au moins une minuscule, une majuscule, un chiffre et un caractère
                    spécial
                  </li>
                </ul>
              </li>

              <li>
                <strong>Confirmer mot de passe</strong> : Entrez à nouveau le mot de passe pour
                confirmation (obligatoire)
              </li>

              <li>
                <strong>Cocher pour activer le compte dès sa création</strong> : Cochez cette case
                si vous souhaitez que l'employé puisse se connecter immédiatement après la création
                de son compte. Si la case n'est pas cochée, le compte sera créé mais verrouillé
                jusqu'à activation manuelle.
              </li>
            </ul>

            <h3 id="creation-employe-remplir" class="subsection-title">
              1.2 Remplir le formulaire
            </h3>

            <ol class="guide-list">
              <li>
                Remplissez tous les champs obligatoires (marqués par un astérisque * ou par la
                validation automatique).
              </li>

              <li>
                Les champs sont validés automatiquement lorsque vous quittez le champ (événement
                @blur).
              </li>

              <li>
                Si un champ contient une erreur, un message d'erreur s'affichera en rouge sous le
                champ concerné.
              </li>
            </ol>

            <h3 id="creation-employe-soumettre" class="subsection-title">
              1.3 Soumettre le formulaire
            </h3>

            <p class="section-text">Une fois tous les champs remplis correctement :</p>

            <ol class="guide-list">
              <li>
                Cliquez sur le bouton <strong>« Ajouter l'employé »</strong> en bas du formulaire.
              </li>

              <li>
                Si la création réussit, vous serez redirigé vers la liste des employés et une
                notification de succès s'affichera.
              </li>

              <li>
                Si l'adresse courriel est déjà utilisée, une notification d'erreur s'affichera et
                vous devrez utiliser une autre adresse email.
              </li>
            </ol>
          </div>
        </div>
      </section>

      <!-- Section 2: Création de promotion -->

      <section v-if="isManager" id="creation-promotion" class="guide-section animate-on-scroll">
        <div class="section-number">02</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Créer une nouvelle promotion</h2>

            <div class="divider"></div>

            <h3 id="creation-promotion-acceder" class="subsection-title">
              2.1 Accéder à la page de création de promotion
            </h3>

            <p class="section-text">
              Pour commencer, repérez le bouton

              <strong>« Créer une nouvelle promotion »</strong> dans la page de gestion des
              promotions, et cliquez dessus.
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_creer_promotion.png"
                alt="Bouton Créer une nouvelle promotion dans la page de gestion des promotions"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Vous serez redirigé vers le formulaire de création de promotion suivant :
            </p>

            <p class="section-text"><strong>Formulaire de création de promotion</strong></p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_formulaire_creer_promotion.png"
                alt="Formulaire de création de promotion"
                class="guide-image"
              />
            </div>

            <p class="section-text">Le formulaire contient les sections suivantes :</p>

            <h3 id="creation-promotion-informations" class="subsection-title">
              2.2 Informations de base de la promotion
            </h3>

            <ol class="guide-list">
              <li>
                <strong>Nom promotion *</strong> : Entrez un nom descriptif pour la promotion
                (obligatoire, marqué par *)
              </li>

              <li>
                <strong>Statut</strong> : Sélectionnez le statut de la promotion

                <ul>
                  <li>
                    <strong>Active</strong> : La promotion est active et s'appliquera aux cartes
                    sélectionnées
                  </li>

                  <li>
                    <strong>Inactive</strong> : La promotion est désactivée et ne s'appliquera pas
                  </li>
                </ul>
              </li>
            </ol>

            <h3 id="creation-promotion-periode" class="subsection-title">
              2.3 Période de validité
            </h3>

            <ol class="guide-list">
              <li>
                <strong>Date début *</strong> : Sélectionnez la date de début de la promotion
                (obligatoire, marqué par *)
              </li>

              <li>
                <strong>Date expiration *</strong> : Sélectionnez la date de fin de la promotion
                (obligatoire, marqué par *)

                <ul>
                  <li>
                    <strong>Note importante</strong> : La date de fin doit être postérieure à la
                    date de début
                  </li>
                </ul>
              </li>
            </ol>

            <h3 id="creation-promotion-rabais" class="subsection-title">2.4 Rabais</h3>

            <ul class="guide-list">
              <li>
                <strong>Rabais (%) *</strong> : Entrez le pourcentage de rabais à appliquer
                (obligatoire, marqué par *)

                <ul>
                  <li>Doit être un nombre positif</li>

                  <li>Ne peut pas être 0</li>

                  <li>Ne peut pas dépasser 100%</li>
                </ul>
              </li>
            </ul>

            <h3 id="creation-promotion-cartes" class="subsection-title">
              2.5 Ajouter des cartes à la promotion
            </h3>

            <p class="section-text">
              Vous avez deux options pour ajouter des cartes à la promotion :
            </p>

            <p class="section-text">
              <strong
                >Option 1 : Ajouter une série complète (recommandé pour les promotions de
                masse)</strong
              >
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_formulaire_creer_promotion_avec_ajout_serie.png"
                alt="Formulaire de création de promotion avec menu déroulant Série"
                class="guide-image"
              />
            </div>

            <ol class="guide-list">
              <li>
                Dans le menu déroulant <strong>« Série (optionnel) »</strong>, sélectionnez la série
                de cartes que vous souhaitez inclure dans la promotion.
              </li>

              <li>Cliquez sur le bouton <strong>« Ajouter la série »</strong>.</li>

              <li>
                Toutes les cartes de la série sélectionnée seront automatiquement ajoutées à la
                promotion.
              </li>
            </ol>

            <p class="section-text"><strong>Option 2 : Ajouter des cartes individuelles</strong></p>

            <ol class="guide-list">
              <li>
                Après avoir rempli les informations de base, utilisez les filtres et la recherche
                pour trouver les cartes spécifiques que vous souhaitez inclure.
              </li>

              <li>Sélectionnez les cartes désirées.</li>

              <li>Les cartes sélectionnées seront ajoutées à la promotion.</li>
            </ol>

            <p class="section-text">
              <strong>Note importante</strong> : Au moins une carte doit être ajoutée à la promotion
              pour pouvoir la sauvegarder.
            </p>

            <h3 id="creation-promotion-description" class="subsection-title">2.6 Description</h3>

            <ul class="guide-list">
              <li>
                <strong>Description *</strong> : Entrez une description détaillée de la promotion
                (obligatoire, marqué par *)

                <ul>
                  <li>Maximum 300 caractères</li>

                  <li>Un compteur de caractères s'affiche en bas du champ</li>
                </ul>
              </li>
            </ul>

            <h3 id="creation-promotion-sauvegarder" class="subsection-title">
              2.7 Sauvegarder ou annuler
            </h3>

            <ol class="guide-list">
              <li>
                <strong>Sauvegarder</strong> : Cliquez sur le bouton

                <strong>« Sauvegarder »</strong> (vert) pour créer la promotion.

                <ul>
                  <li>Le système validera tous les champs obligatoires.</li>

                  <li>
                    Si des erreurs sont détectées, des messages d'erreur s'afficheront sous les
                    champs concernés.
                  </li>

                  <li>
                    Si la validation réussit, la promotion sera créée et vous serez redirigé vers la
                    liste des promotions.
                  </li>
                </ul>
              </li>

              <li>
                <strong>Annuler</strong> : Cliquez sur le bouton <strong>« Annuler »</strong> (gris)
                pour annuler la création et retourner à la liste des promotions sans sauvegarder.
              </li>
            </ol>
          </div>
        </div>
      </section>

      <!-- Section 3: Gestion des remboursements -->

      <section v-if="isEmployee" id="gestion-remboursement" class="guide-section animate-on-scroll">
        <div class="section-number">{{ isManager ? '03' : '01' }}</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Gestion des demandes de remboursement (employé)</h2>

            <div class="divider"></div>

            <h3 id="gestion-remboursement-acceder" class="subsection-title">
              {{ section3Prefix }}.1 Accéder à la page de gestion des demandes
            </h3>

            <p class="section-text">
              <strong>{{ section3Prefix }}.1.1 Ouvrir la page</strong>
            </p>

            <p class="section-text">
              Pour commencer, dans le menu de gestion du magasin, repérez l'entrée

              <strong>« Gestion des demandes de remboursement »</strong> et cliquez dessus.
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_gestion_demande_remboursement.png"
                alt="Page de gestion des demandes de remboursement avec filtres"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Vous arriverez sur la page listant toutes les demandes de remboursement en cours et
              traitées.
            </p>

            <p class="section-text">
              <strong>{{ section3Prefix }}.1.2 Filtrer les demandes par statut</strong>
            </p>

            <p class="section-text">
              En haut de la page, un bandeau permet de filtrer les demandes selon leur statut :
            </p>

            <ul class="guide-list">
              <li>
                <strong>En attente</strong> : affiche uniquement les demandes qui n'ont pas encore
                été traitées.
              </li>

              <li><strong>Acceptées</strong> : affiche les demandes déjà approuvées.</li>

              <li><strong>Refusées</strong> : affiche les demandes déjà refusées.</li>
            </ul>

            <p class="section-text">
              Cliquez sur le bouton correspondant au statut souhaité pour mettre à jour la liste.
            </p>

            <h3 id="gestion-remboursement-tableau" class="subsection-title">
              {{ section3Prefix }}.2 Comprendre le tableau des demandes
            </h3>

            <p class="section-text">
              Le tableau présente une ligne par demande de remboursement avec les informations
              suivantes :
            </p>

            <ul class="guide-list">
              <li>
                <strong>Id commande</strong> : les premiers caractères de l'identifiant de la
                commande associée à la demande.
              </li>

              <li><strong>Remboursement</strong> : montant à rembourser pour cette demande.</li>

              <li><strong>Montant</strong> : montant total de la commande concernée.</li>

              <li><strong>Type</strong> : type de remboursement demandé (complet ou partiel).</li>

              <li>
                <strong>Motif</strong> : raison principale fournie par le client (sélectionnée dans
                le formulaire de demande).
              </li>

              <li>
                <strong>Commentaire</strong> : commentaire libre saisi par le client (facultatif).
              </li>
            </ul>

            <h3 id="gestion-remboursement-commentaire" class="subsection-title">
              {{ section3Prefix }}.3 Consulter le commentaire détaillé
            </h3>

            <p class="section-text">Pour lire le commentaire complet associé à une demande :</p>

            <ol class="guide-list">
              <li>
                Dans la colonne <strong>« Commentaire »</strong>, cliquez sur le bouton

                <strong>« Voir »</strong>.
              </li>

              <li>
                Une fenêtre modale s'affiche au centre de l'écran avec le texte du commentaire.
              </li>

              <li>Si aucun commentaire n'a été fourni, <strong>« N/A »</strong> est affiché.</li>

              <li>Cliquez sur <strong>« Fermer »</strong> pour refermer la fenêtre.</li>
            </ol>

            <h3 id="gestion-remboursement-accepter-refuser" class="subsection-title">
              {{ section3Prefix }}.4 Accepter ou refuser une demande (statut « en attente »)
            </h3>

            <p class="section-text">
              Cette action n'est possible que lorsque le filtre <strong>« En attente »</strong> est
              sélectionné.
            </p>

            <p class="section-text">
              <strong>{{ section3Prefix }}.4.1 Choisir une décision</strong>
            </p>

            <p class="section-text">Dans la dernière colonne de la ligne de la demande :</p>

            <ul class="guide-list">
              <li>
                <strong>Bouton « Accepter »</strong> :

                <ul>
                  <li>
                    Sélectionnez cette option pour approuver la demande de remboursement (total ou
                    partiel, selon ce qui a été demandé).
                  </li>
                </ul>
              </li>

              <li>
                <strong>Bouton « Refuser »</strong> :

                <ul>
                  <li>Sélectionnez cette option pour refuser la demande de remboursement.</li>
                </ul>
              </li>
            </ul>

            <p class="section-text">
              Dès que vous cliquez sur <strong>« Accepter »</strong> ou

              <strong>« Refuser »</strong> :
            </p>

            <ol class="guide-list">
              <li>La décision est envoyée au système.</li>

              <li>La demande est mise à jour avec le nouveau statut.</li>

              <li>La liste des demandes est rechargée automatiquement.</li>

              <li>
                Une notification s'affiche en haut de l'écran pour confirmer la bonne prise en
                compte de la décision ou signaler une erreur en cas de problème.
              </li>
            </ol>

            <p class="section-text">
              <strong>{{ section3Prefix }}.4.2 Vérifier le résultat</strong>
            </p>

            <p class="section-text">Après avoir pris une décision, vous pouvez :</p>

            <ol class="guide-list">
              <li>
                Changer le filtre de statut (par exemple, passer sur

                <strong>« Acceptées »</strong> ou <strong>« Refusées »</strong>).
              </li>

              <li>
                Vérifier que la demande apparaît désormais dans la bonne section avec le statut mis
                à jour.
              </li>
            </ol>

            <h3 id="gestion-remboursement-notes" class="subsection-title">
              {{ section3Prefix }}.5 Notes importantes
            </h3>

            <ul class="guide-list">
              <li>
                Seules les demandes <strong>« en attente »</strong> peuvent être acceptées ou
                refusées.
              </li>

              <li>Les montants affichés sont arrondis à deux décimales.</li>

              <li>
                Le tableau est mis à jour automatiquement après chaque décision pour refléter l'état
                réel des demandes.
              </li>

              <li>
                En cas de message d'erreur lors de l'acceptation ou du refus, aucune modification
                n'est appliquée tant que la demande n'a pas été traitée avec succès.
              </li>
            </ul>
          </div>
        </div>
      </section>

      <!-- Section 4: Recherche avancée -->

      <section id="recherche-avancee" class="guide-section animate-on-scroll">
        <div class="section-number">{{ isEmployee ? '04' : '01' }}</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Recherche avancée pour le client</h2>

            <div class="divider"></div>

            <h3 id="recherche-avancee-acceder" class="subsection-title">
              {{ section4Prefix }}.1 Accéder à la recherche avancée
            </h3>

            <p class="section-text">
              <strong>{{ section4Prefix }}.1.1 Ouvrir la page catalogue</strong>
            </p>

            <p class="section-text">
              Pour commencer, connectez-vous à votre compte puis cliquez sur l'entrée

              <strong>« Catalogue »</strong> dans le menu principal.
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_recherche_avancee_catalogue.png"
                alt="Page catalogue avec filtres de recherche avancée"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Vous arrivez sur la page listant les cartes disponibles à la vente.
            </p>

            <p class="section-text">
              <strong>{{ section4Prefix }}.1.2 Afficher les filtres de recherche avancée</strong>
            </p>

            <p class="section-text">Sur la page catalogue :</p>

            <ul class="guide-list">
              <li>
                Sur ordinateur, les filtres sont généralement visibles dans une colonne à gauche de
                la liste des cartes.
              </li>

              <li>
                Sur mobile, cliquez sur le bouton <strong>« Filtres »</strong> ou

                <strong>« Recherche avancée »</strong> pour afficher le panneau des filtres.
              </li>
            </ul>

            <h3 id="recherche-avancee-barre" class="subsection-title">
              {{ section4Prefix }}.2 Utiliser la barre de recherche
            </h3>

            <p class="section-text">
              <strong>{{ section4Prefix }}.2.1 Rechercher par nom de carte</strong>
            </p>

            <p class="section-text">
              En haut de la liste des cartes, saisissez le nom complet ou partiel d'une carte dans
              la barre de recherche.
            </p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_recherche_avancee_par_nom.png"
                alt="Barre de recherche avec résultats de recherche par nom"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Dès que vous validez (touche Entrée ou icône de recherche), la liste se met à jour
              pour n'afficher que les cartes correspondant à votre texte.
            </p>

            <h3 id="recherche-avancee-filtres" class="subsection-title">
              {{ section4Prefix }}.3 Utiliser les filtres avancés
            </h3>

            <p class="section-text">
              Les filtres vous permettent de cibler plus précisément les cartes que vous cherchez.
            </p>

            <p class="section-text">Vous pouvez combiner plusieurs filtres en même temps.</p>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_recherche_avance_filtre_selectionnes.png"
                alt="Panneau de filtres avec plusieurs options sélectionnées"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.1 Filtrer par série</strong>
            </p>

            <ul class="guide-list">
              <li>
                Utilisez la section <strong>« Série »</strong> pour sélectionner une ou plusieurs
                séries (extensions) de cartes.
              </li>

              <li>Seules les cartes appartenant aux séries choisies seront affichées.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.2 Filtrer par rareté</strong>
            </p>

            <ul class="guide-list">
              <li>
                Dans la section <strong>« Rareté »</strong>, sélectionnez une ou plusieurs raretés
                (Commune, Inhabituelle, Rare, Mythique, etc.).
              </li>

              <li>La liste affichera uniquement les cartes correspondant aux raretés choisies.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.3 Filtrer par type et sous-type</strong>
            </p>

            <ul class="guide-list">
              <li>
                La section <strong>« Type »</strong> permet de choisir des types généraux (Créature,
                Rituel, Éphémère, Terrain, etc.).
              </li>

              <li>
                La section <strong>« Sous-type »</strong> permet de préciser encore davantage (par
                exemple : Humain, Sorcier, Dragon).
              </li>

              <li>Vous pouvez sélectionner plusieurs types et sous-types si nécessaire.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.4 Filtrer par couleur</strong>
            </p>

            <ul class="guide-list">
              <li>
                Dans la section <strong>« Couleur »</strong>, cochez les couleurs souhaitées (Blanc,
                Bleu, Noir, Rouge, Vert, Multicolore, Incolore).
              </li>

              <li>Seules les cartes correspondant aux couleurs sélectionnées seront affichées.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.5 Filtrer par langue</strong>
            </p>

            <ul class="guide-list">
              <li>
                La section <strong>« Langue »</strong> permet de choisir la ou les langues
                d'impression (Français, Anglais, etc.).
              </li>

              <li>Ce filtre limite les résultats aux cartes disponibles dans ces langues.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.6 Filtrer par disponibilité</strong>
            </p>

            <ul class="guide-list">
              <li>
                Le filtre <strong>« Disponibilité »</strong> permet par exemple de n'afficher que
                les cartes <strong>« En stock »</strong>.
              </li>

              <li>Si vous choisissez ce filtre, les cartes en rupture de stock seront masquées.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.3.7 Filtrer par prix</strong>
            </p>

            <ul class="guide-list">
              <li>
                Utilisez les champs <strong>« Prix minimum »</strong> et

                <strong>« Prix maximum »</strong> pour définir une fourchette de prix.
              </li>

              <li>Seules les cartes dont le prix se situe dans cet intervalle seront affichées.</li>
            </ul>

            <h3 id="recherche-avancee-trier" class="subsection-title">
              {{ section4Prefix }}.4 Trier les résultats
            </h3>

            <p class="section-text">
              <strong>{{ section4Prefix }}.4.1 Choisir un critère de tri</strong>
            </p>

            <p class="section-text">
              Un menu de tri vous permet d'ordonner les résultats selon différents critères, par
              exemple :
            </p>

            <ul class="guide-list">
              <li>Prix croissant ou décroissant</li>

              <li>Nom de la carte (A–Z ou Z–A)</li>

              <li>Rareté</li>

              <li>Popularité ou nombre de clics (si disponible)</li>
            </ul>

            <p class="section-text">
              Sélectionnez l'option qui vous convient pour réorganiser la liste.
            </p>

            <h3 id="recherche-avancee-combiner" class="subsection-title">
              {{ section4Prefix }}.5 Combiner et réinitialiser les filtres
            </h3>

            <p class="section-text">
              <strong>{{ section4Prefix }}.5.1 Combiner plusieurs critères</strong>
            </p>

            <p class="section-text">Vous pouvez :</p>

            <ul class="guide-list">
              <li>Saisir un texte dans la barre de recherche</li>

              <li>Sélectionner une ou plusieurs séries, raretés, types, couleurs, langues</li>

              <li>Définir une plage de prix</li>

              <li>Choisir un tri</li>
            </ul>

            <p class="section-text">Tout cela en même temps.</p>

            <p class="section-text">
              La liste des cartes affichera uniquement les résultats correspondant à l'ensemble des
              critères choisis.
            </p>

            <p class="section-text">
              <strong>{{ section4Prefix }}.5.2 Réinitialiser les filtres</strong>
            </p>

            <p class="section-text">
              Si les résultats deviennent trop restreints ou ne correspondent plus à ce que vous
              souhaitez :
            </p>

            <ul class="guide-list">
              <li>
                Utilisez le bouton <strong>« Réinitialiser »</strong> ou

                <strong>« Effacer les filtres »</strong> (selon l'intitulé de votre interface).
              </li>

              <li>
                Tous les filtres sont remis à zéro (séries, raretés, types, couleurs, langues, prix,
                disponibilité).
              </li>

              <li>La liste des cartes revient à l'affichage par défaut.</li>
            </ul>

            <h3 id="recherche-avancee-consulter" class="subsection-title">
              {{ section4Prefix }}.6 Consulter les résultats
            </h3>

            <p class="section-text">
              <strong>{{ section4Prefix }}.6.1 Navigation dans les pages</strong>
            </p>

            <p class="section-text">Si le nombre de cartes trouvées est important :</p>

            <ul class="guide-list">
              <li>
                Utilisez les boutons de pagination (page suivante, page précédente, numéros de page)
                pour parcourir les résultats.
              </li>

              <li>Le nombre total de pages et la page actuelle sont indiqués sous la liste.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section4Prefix }}.6.2 Détails d'une carte</strong>
            </p>

            <p class="section-text">Pour voir les détails d'une carte particulière :</p>

            <ul class="guide-list">
              <li>
                Cliquez sur la carte ou sur un bouton dédié (par exemple

                <strong>« Voir plus »</strong>).
              </li>

              <li>
                Vous accéderez à une fiche détaillée avec l'illustration, le texte de règle, le
                prix, la quantité disponible et éventuellement les variantes.
              </li>
            </ul>

            <h3 id="recherche-avancee-notes" class="subsection-title">
              {{ section4Prefix }}.7 Notes importantes
            </h3>

            <ul class="guide-list">
              <li>
                Plus vous ajoutez de filtres, plus le nombre de résultats diminue. Si aucun résultat
                n'apparaît, essayez d'enlever un ou plusieurs filtres.
              </li>

              <li>
                Les quantités et la disponibilité sont mises à jour en fonction du stock réel : une
                carte peut ne plus apparaître si elle est entièrement vendue.
              </li>

              <li>
                La recherche par nom est généralement tolérante aux débuts de mots, mais évitez les
                fautes d'orthographe pour de meilleurs résultats.
              </li>
            </ul>
          </div>
        </div>
      </section>

      <!-- Section 5: Passer une commande -->

      <section id="passer-commande" class="guide-section animate-on-scroll">
        <div class="section-number">{{ isEmployee ? '05' : '02' }}</div>

        <div class="section-content">
          <div class="content-text">
            <h2 class="section-title">Passer une commande (client)</h2>

            <div class="divider"></div>

            <h3 id="passer-commande-catalogue" class="subsection-title">
              {{ section5Prefix }}.1 Accéder au catalogue et choisir des cartes
            </h3>

            <p class="section-text">
              <strong>{{ section5Prefix }}.1.1 Ouvrir le catalogue</strong>
            </p>

            <p class="section-text">
              Pour commencer, connectez-vous à votre compte client puis cliquez sur

              <strong>« Catalogue »</strong> dans le menu principal.
            </p>

            <p class="section-text">La liste des cartes disponibles à l'achat s'affiche.</p>

            <p class="section-text">
              <strong>{{ section5Prefix }}.1.2 Rechercher et filtrer les cartes</strong>
            </p>

            <p class="section-text">Vous pouvez :</p>

            <ul class="guide-list">
              <li>Saisir le nom d'une carte dans la barre de recherche.</li>

              <li>
                Utiliser les filtres (série, rareté, type, couleur, langue, prix, disponibilité)
                pour affiner les résultats.
              </li>

              <li>Changer le tri (par exemple : prix croissant, rareté, ordre alphabétique).</li>
            </ul>

            <p class="section-text">
              Parcourez les pages jusqu'à trouver les cartes que vous souhaitez acheter.
            </p>

            <p class="section-text">
              <strong>{{ section5Prefix }}.1.3 Ajouter des cartes au panier</strong>
            </p>

            <p class="section-text">Sur chaque carte, vous disposez d'actions permettant de :</p>

            <ul class="guide-list">
              <li>Choisir la version (foil / non foil, si disponible).</li>

              <li>
                Sélectionner la langue (par exemple Français ou Anglais), si plusieurs langues
                existent.
              </li>

              <li>Indiquer la quantité désirée (dans la limite du stock disponible).</li>

              <li>
                Cliquer sur <strong>« Ajouter au panier »</strong> pour ajouter la sélection à votre
                panier.
              </li>
            </ul>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_passer_commande_catalogue_ajouter_au_panier.png"
                alt="Page catalogue avec bouton Ajouter au panier"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              Répétez ces étapes pour toutes les cartes que vous souhaitez commander.
            </p>

            <h3 id="passer-commande-panier" class="subsection-title">
              {{ section5Prefix }}.2 Vérifier et modifier son panier
            </h3>

            <p class="section-text">
              <strong>{{ section5Prefix }}.2.1 Accéder au panier</strong>
            </p>

            <p class="section-text">Une fois vos cartes choisies :</p>

            <ul class="guide-list">
              <li>
                Cliquez sur l'icône de panier ou sur le lien <strong>« Panier »</strong> dans le
                menu.
              </li>

              <li>
                La liste de toutes les cartes ajoutées s'affiche avec : nom de la carte, version
                foil ou non, langue, quantité et prix unitaire.
              </li>
            </ul>

            <div class="guide-image-container">
              <img
                src="@/assets/images/guide_utilisation_passer_commande.png"
                alt="Page panier avec articles et bouton Passer à la commande"
                class="guide-image"
              />
            </div>

            <p class="section-text">
              <strong>{{ section5Prefix }}.2.2 Modifier le contenu du panier</strong>
            </p>

            <p class="section-text">Dans le panier, vous pouvez :</p>

            <ul class="guide-list">
              <li>Augmenter ou diminuer la quantité d'une carte.</li>

              <li>Supprimer une carte du panier si vous ne la souhaitez plus.</li>

              <li>Vérifier le sous-total des articles (hors livraison et taxes).</li>
            </ul>

            <p class="section-text">
              Lorsque le contenu du panier vous convient, cliquez sur le bouton

              <strong>« Passer à la commande »</strong> ou <strong>« Commander »</strong>.
            </p>

            <h3 id="passer-commande-livraison" class="subsection-title">
              {{ section5Prefix }}.3 Saisir l'adresse de livraison et choisir le mode d'expédition
            </h3>

            <p class="section-text">
              <strong>{{ section5Prefix }}.3.1 Adresse de livraison</strong>
            </p>

            <p class="section-text">Sur la page d'adresse :</p>

            <ul class="guide-list">
              <li>
                Remplissez les champs obligatoires (nom, prénom, adresse, ville, province, code
                postal, téléphone, etc.).
              </li>

              <li>
                Si une adresse est déjà enregistrée, vous pouvez la vérifier et la modifier au
                besoin.
              </li>
            </ul>

            <p class="section-text">
              <strong>{{ section5Prefix }}.3.2 Choisir le mode de livraison</strong>
            </p>

            <p class="section-text">
              Vous devez ensuite choisir l'un des modes suivants (si proposés) :
            </p>

            <ul class="guide-list">
              <li>
                <strong>Cueillette en magasin</strong> : vous récupérez la commande directement en
                boutique, sans frais de livraison.
              </li>

              <li>
                <strong>Livraison à domicile</strong> : la commande est livrée à l'adresse indiquée,
                avec des frais de transport qui s'ajoutent au total (par exemple 10$ pour les
                commandes de moins de 100$).
              </li>
            </ul>

            <p class="section-text">
              Validez l'adresse et le mode de livraison pour passer à l'étape suivante.
            </p>

            <h3 id="passer-commande-payer" class="subsection-title">
              {{ section5Prefix }}.4 Valider le récapitulatif et payer
            </h3>

            <p class="section-text">
              <strong>{{ section5Prefix }}.4.1 Vérifier le récapitulatif</strong>
            </p>

            <p class="section-text">Avant de payer, un récapitulatif complet s'affiche :</p>

            <ul class="guide-list">
              <li>Liste des cartes commandées (quantité, prix unitaire, total par ligne).</li>

              <li>Sous-total des articles.</li>

              <li>Frais de livraison (si applicables).</li>

              <li>Montant des taxes (TPS + TVQ).</li>

              <li>Total final à payer.</li>
            </ul>

            <p class="section-text">Relisez ces informations attentivement.</p>

            <p class="section-text">
              Si nécessaire, utilisez les boutons de retour pour corriger l'adresse, le panier ou le
              mode de livraison.
            </p>

            <p class="section-text">
              <strong>{{ section5Prefix }}.4.2 Procéder au paiement</strong>
            </p>

            <p class="section-text">Lorsque tout est correct :</p>

            <ol class="guide-list">
              <li>
                Cliquez sur le bouton de paiement (par exemple <strong>« Payer »</strong> ou

                <strong>« Confirmer la commande »</strong>).
              </li>

              <li>
                Saisissez vos informations de paiement sur la page sécurisée (carte bancaire ou
                autre mode proposé).
              </li>

              <li>Validez le paiement.</li>
            </ol>

            <p class="section-text">
              Une fois le paiement accepté, votre commande est créée et un numéro de commande vous
              est attribué.
            </p>

            <h3 id="passer-commande-confirmation" class="subsection-title">
              {{ section5Prefix }}.5 Confirmation et suivi de commande
            </h3>

            <p class="section-text">
              <strong>{{ section5Prefix }}.5.1 Confirmation</strong>
            </p>

            <p class="section-text">Après validation :</p>

            <ul class="guide-list">
              <li>
                Une page de confirmation s'affiche avec le numéro de commande et un résumé de votre
                achat.
              </li>

              <li>Un courriel de confirmation est envoyé à l'adresse associée à votre compte.</li>
            </ul>

            <p class="section-text">
              <strong>{{ section5Prefix }}.5.2 Consulter l'historique de vos commandes</strong>
            </p>

            <p class="section-text">Pour suivre l'état de vos commandes :</p>

            <ul class="guide-list">
              <li>
                Allez dans la section <strong>« Mon compte »</strong> puis

                <strong>« Historique des commandes »</strong> ou équivalent.
              </li>

              <li>
                Vous pouvez y voir la liste de vos commandes, leur statut (en préparation, expédiée,
                livrée, remboursée, etc.) et accéder au détail de chacune.
              </li>
            </ul>

            <h3 id="passer-commande-notes" class="subsection-title">
              {{ section5Prefix }}.6 Notes importantes
            </h3>

            <ul class="guide-list">
              <li>
                Le stock est mis à jour en temps réel : si un article n'est plus disponible, vous ne
                pourrez pas l'ajouter au panier ou la quantité sera limitée.
              </li>

              <li>
                Pour les commandes sous un certain montant (par exemple 100$), des frais de
                livraison peuvent s'appliquer.
              </li>

              <li>
                Assurez-vous que votre adresse et votre courriel sont corrects pour éviter les
                problèmes de livraison et de confirmation.
              </li>

              <li>
                Si vous repérez une erreur après validation de la commande, utilisez votre
                historique ou contactez le support pour demander une modification ou un
                remboursement.
              </li>
            </ul>
          </div>
        </div>
      </section>
    </div>

    <!-- Bouton pour revenir en haut de la page -->
    <a
      href="#guide-utilisation"
      class="scroll-to-top-button"
      aria-label="Revenir en haut de la page"
      title="Revenir en haut"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      >
        <path d="M18 15l-6-6-6 6" />
      </svg>
    </a>
  </div>
</template>

<style scoped>
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(40px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.guide-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a202c 0%, #2d3748 100%);
  color: #f7fafc;
  scroll-behavior: smooth;
}

html {
  scroll-behavior: smooth;
}

.hero-section {
  position: relative;
  min-height: 40vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background:
    linear-gradient(135deg, rgba(162, 210, 255, 0.1) 0%, rgba(98, 182, 203, 0.1) 100%),
    url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320"><path fill="%232d3748" opacity="0.3" d="M0,96L48,112C96,128,192,160,288,160C384,160,480,128,576,112C672,96,768,96,864,112C960,128,1056,160,1152,160C1248,160,1344,128,1392,112L1440,96L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path></svg>')
      no-repeat bottom;
  background-size: cover;
  overflow: visible;
  padding: 2rem 1rem;
}

.hero-content {
  text-align: center;
  z-index: 1;
  padding: 2rem 1rem;
  max-width: 100%;
  width: 100%;
  box-sizing: border-box;
}

.hero-title {
  font-size: 3rem;
  font-weight: 800;
  margin-bottom: 1rem;
  background: linear-gradient(135deg, #a2d2ff 0%, #62b6cb 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  word-wrap: break-word;
  overflow-wrap: break-word;
  line-height: 1.2;
}

#guide-utilisation {
  scroll-margin-top: 120px;
}

.hero-subtitle {
  font-size: 1.3rem;
  font-weight: 300;
  color: rgba(255, 255, 255, 0.9);
  letter-spacing: 0.5px;
  line-height: 1.6;
  word-wrap: break-word;
  overflow-wrap: break-word;
  max-width: 100%;
}

.main-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 3rem;
}

.guide-section {
  position: relative;
  margin-bottom: 4rem;
  padding: 4rem;
  background: rgba(255, 255, 255, 0.02);
  border-radius: 20px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(226, 232, 240, 0.12);
  box-shadow:
    0 10px 25px rgba(0, 0, 0, 0.25),
    0 0 0 1px rgba(148, 163, 184, 0.2);
  scroll-margin-top: 120px;
  scroll-padding-top: 120px;
}

.toc-section {
  position: relative;
  margin-bottom: 4rem;
  padding: 4rem;
  background: rgba(162, 210, 255, 0.05);
  border-radius: 20px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(162, 210, 255, 0.2);
  box-shadow:
    0 10px 25px rgba(0, 0, 0, 0.25),
    0 0 0 1px rgba(162, 210, 255, 0.3);
}

.section-number {
  position: absolute;
  top: -1.2rem;
  left: 2rem;
  font-size: 0.9rem;
  letter-spacing: 4px;
  text-transform: uppercase;
  color: #a0aec0;
  background: linear-gradient(135deg, #1a202c 0%, #2d3748 100%);
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
}

.section-content {
  display: flex;
  align-items: flex-start;
  gap: 2rem;
}

.content-text {
  flex: 1;
}

.section-title {
  font-size: 1.8rem;
  font-weight: 700;
  font-style: italic;
  margin-bottom: 1rem;
  color: #f7fafc;
}

.subsection-title {
  font-size: 1.3rem;
  font-weight: 600;
  font-style: italic;
  margin-top: 2.5rem;
  margin-bottom: 1.25rem;
  color: #e2e8f0;
  scroll-margin-top: 120px;
}

.divider {
  width: 80px;
  height: 3px;
  background: linear-gradient(90deg, #a2d2ff, #62b6cb);
  border-radius: 9999px;
  margin-bottom: 1.5rem;
}

.section-text {
  font-size: 1rem;
  line-height: 1.9;
  color: #e2e8f0;
  margin-bottom: 1rem;
}

.guide-list {
  font-size: 1rem;
  line-height: 1.9;
  color: #e2e8f0;
  margin-left: 1.5rem;
  margin-bottom: 1.5rem;
}

.guide-list li {
  margin-bottom: 0.75rem;
}

.guide-list ul {
  margin-top: 0.5rem;
  margin-left: 1.5rem;
}

.toc-nav {
  margin-top: 1rem;
}

.toc-category {
  font-size: 1.2rem;
  font-weight: 600;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  color: #a2d2ff;
}

.toc-category:first-child {
  margin-top: 0;
}

.toc-list {
  list-style: none;
  margin-left: 0;
  margin-bottom: 1rem;
}

.toc-list li {
  margin-bottom: 0.5rem;
}

.toc-sublist {
  list-style: none;
  margin-left: 1.5rem;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}

.toc-sublist li {
  margin-bottom: 0.375rem;
}

.toc-link {
  color: #e2e8f0;
  text-decoration: none;
  transition:
    color 0.3s ease,
    transform 0.2s ease;
  display: inline-block;
  cursor: pointer;
}

.toc-link:hover {
  color: #a2d2ff;
  transform: translateX(5px);
}

.guide-image-container {
  margin: 2rem 0;
  text-align: center;
  width: 100%;
}

.guide-image {
  max-width: 100%;
  height: auto;
  border-radius: 12px;
  box-shadow:
    0 8px 20px rgba(0, 0, 0, 0.3),
    0 0 0 1px rgba(148, 163, 184, 0.2);
  border: 1px solid rgba(226, 232, 240, 0.15);
  background: rgba(255, 255, 255, 0.05);
  padding: 0.5rem;
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease;
}

.guide-image:hover {
  transform: scale(1.02);
  box-shadow:
    0 12px 30px rgba(0, 0, 0, 0.4),
    0 0 0 1px rgba(162, 210, 255, 0.3);
}

.animate-on-scroll {
  opacity: 0;
  transform: translateY(20px);
  transition:
    opacity 0.6s ease-out,
    transform 0.6s ease-out;
}

.animate-on-scroll.visible {
  opacity: 1;
  transform: translateY(0);
  animation: fadeInUp 0.6s ease-out;
}

@media (max-width: 768px) {
  .hero-section {
    min-height: 35vh;
    padding: 1.5rem 1rem;
    overflow: visible;
  }

  .hero-content {
    padding: 1rem 0.5rem;
  }

  .hero-title {
    font-size: 2rem;
    margin-bottom: 0.75rem;
    line-height: 1.3;
    word-wrap: break-word;
  }

  #guide-utilisation {
    scroll-margin-top: 90px;
  }

  .hero-subtitle {
    font-size: 1rem;
    padding: 0;
    line-height: 1.5;
    word-wrap: break-word;
  }

  .main-content {
    max-width: 100%;
    padding: 2rem 1.25rem;
  }

  .guide-section,
  .toc-section {
    padding: 2rem 1.5rem;
    margin-bottom: 2.5rem;
    border-radius: 16px;
    scroll-margin-top: 90px;
    scroll-padding-top: 90px;
  }

  .section-number {
    font-size: 0.75rem;
    padding: 0.2rem 0.5rem;
    top: -0.8rem;
    left: 1.5rem;
  }

  .section-title {
    font-size: 1.5rem;
    font-style: italic;
    margin-bottom: 0.75rem;
    line-height: 1.3;
  }

  .subsection-title {
    font-size: 1.1rem;
    font-style: italic;
    margin-top: 2rem;
    margin-bottom: 1rem;
    line-height: 1.4;
    scroll-margin-top: 90px;
  }

  .divider {
    width: 60px;
    height: 2px;
    margin-bottom: 1.25rem;
  }

  .section-text {
    font-size: 0.95rem;
    line-height: 1.8;
    margin-bottom: 0.875rem;
  }

  .guide-list {
    font-size: 0.95rem;
    line-height: 1.8;
    margin-left: 1.25rem;
    margin-bottom: 1.25rem;
  }

  .guide-list li {
    margin-bottom: 0.625rem;
  }

  .guide-list ul {
    margin-left: 1.25rem;
    margin-top: 0.5rem;
  }

  .toc-nav {
    margin-top: 0.75rem;
  }

  .toc-category {
    font-size: 1.1rem;
    margin-top: 1.25rem;
    margin-bottom: 0.625rem;
  }

  .toc-list {
    margin-bottom: 0.875rem;
  }

  .toc-list li {
    margin-bottom: 0.5rem;
  }

  .toc-sublist {
    margin-left: 1.25rem;
    margin-top: 0.375rem;
  }

  .toc-sublist li {
    margin-bottom: 0.25rem;
  }

  .toc-link {
    font-size: 0.95rem;
    padding: 0.25rem 0;
    display: block;
  }

  .section-content {
    flex-direction: column;
    gap: 1.5rem;
  }

  .guide-image-container {
    margin: 1.5rem 0;
  }

  .guide-image {
    border-radius: 10px;
    padding: 0.25rem;
  }
}

@media (max-width: 480px) {
  .hero-section {
    min-height: 30vh;
    padding: 1rem 0.75rem;
  }

  .hero-content {
    padding: 0.75rem 0.25rem;
  }

  .hero-title {
    font-size: 1.75rem;
    line-height: 1.2;
    margin-bottom: 0.5rem;
  }

  .hero-subtitle {
    font-size: 0.9rem;
    line-height: 1.4;
    padding: 0;
  }

  .main-content {
    padding: 1.5rem 1rem;
  }

  .guide-section,
  .toc-section {
    padding: 1.75rem 1.25rem;
    margin-bottom: 2rem;
    border-radius: 12px;
  }

  .section-title {
    font-size: 1.35rem;
    font-style: italic;
  }

  .subsection-title {
    font-size: 1rem;
    font-style: italic;
    margin-top: 1.75rem;
    scroll-margin-top: 90px;
  }

  .section-text,
  .guide-list {
    font-size: 0.9rem;
  }

  .toc-link {
    font-size: 0.9rem;
  }

  .guide-image {
    border-radius: 8px;
    padding: 0.25rem;
  }
}

/* Light Mode  */
body:not(.body--dark) .guide-page {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  color: #2d3748;
}

body:not(.body--dark) .hero-section {
  background:
    linear-gradient(135deg, rgba(162, 210, 255, 0.2) 0%, rgba(98, 182, 203, 0.2) 100%),
    url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320"><path fill="%23f7fafc" opacity="0.3" d="M0,96L48,112C96,128,192,160,288,160C384,160,480,128,576,112C672,96,768,96,864,112C960,128,1056,160,1152,160C1248,160,1344,128,1392,112L1440,96L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path></svg>')
      no-repeat bottom;
  background-size: cover;
}

body:not(.body--dark) .hero-title {
  background: linear-gradient(135deg, #2c3e50 0%, #4ca1af 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

body:not(.body--dark) .hero-subtitle {
  color: #4a5568;
}

body:not(.body--dark) .guide-section {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .toc-section {
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(162, 210, 255, 0.4);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .section-number {
  background: linear-gradient(135deg, #e2e8f0 0%, #cbd5e0 100%);
  color: #4a5568;
}

body:not(.body--dark) .section-title {
  color: #2d3748;
}

body:not(.body--dark) .subsection-title {
  color: #4a5568;
}

body:not(.body--dark) .section-text,
body:not(.body--dark) .guide-list {
  color: #4a5568;
}

body:not(.body--dark) .toc-category {
  color: #3182ce;
}

body:not(.body--dark) .toc-link {
  color: #4a5568;
}

body:not(.body--dark) .toc-link:hover {
  color: #3182ce;
}

body:not(.body--dark) .guide-image {
  background: #fff;
  border-color: rgba(0, 0, 0, 0.1);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

body:not(.body--dark) .guide-image:hover {
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.15);
}

/* Bouton pour revenir en haut de la page */
.scroll-to-top-button {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: 50px;
  padding: 0;
  background: linear-gradient(135deg, #a2d2ff 0%, #62b6cb 100%);
  color: #1a202c;
  border: none;
  border-radius: 50%;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  text-decoration: none;
  box-shadow:
    0 4px 12px rgba(162, 210, 255, 0.4),
    0 0 0 1px rgba(162, 210, 255, 0.2);
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease,
    opacity 0.3s ease;
  z-index: 9999;
  opacity: 1;
  visibility: visible;
}

.scroll-to-top-button:hover {
  transform: translateY(-3px);
  box-shadow:
    0 6px 20px rgba(162, 210, 255, 0.5),
    0 0 0 1px rgba(162, 210, 255, 0.3);
}

.scroll-to-top-button:active {
  transform: translateY(-1px);
}

.scroll-to-top-button svg {
  width: 24px;
  height: 24px;
}

body:not(.body--dark) .scroll-to-top-button {
  background: linear-gradient(135deg, #3182ce 0%, #2c5282 100%);
  color: #fff;
  box-shadow:
    0 4px 12px rgba(49, 130, 206, 0.3),
    0 0 0 1px rgba(49, 130, 206, 0.2);
}

body:not(.body--dark) .scroll-to-top-button:hover {
  box-shadow:
    0 6px 20px rgba(49, 130, 206, 0.4),
    0 0 0 1px rgba(49, 130, 206, 0.3);
}

@media (max-width: 768px) {
  .scroll-to-top-button {
    width: 45px;
    height: 45px;
    bottom: 1.5rem;
    right: 1.5rem;
  }

  .scroll-to-top-button svg {
    width: 20px;
    height: 20px;
  }
}
</style>
