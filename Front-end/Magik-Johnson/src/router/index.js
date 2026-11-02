import { createRouter, createWebHistory } from 'vue-router'
import Profil from '@/components/accountManagement/Profil.vue'
import Promotions from '@/components/storeManagement/Promotions.vue'
import ManageEmployee from '@/components/storeManagement/ManageEmployee.vue'
import AddEmployee from '@/components/storeManagement/AddEmployee.vue'
import { useAuthStore } from '@/stores/authStore'
import Clients from '@/components/storeManagement/Clients.vue'
import ManageClient from '@/components/storeManagement/ManageClient.vue'
import NotFound from '@/views/errors/NotFound.vue'
import OrderHistory from '@/components/accountManagement/OrderHistory.vue'
import Adress from '@/components/accountManagement/Adress.vue'
import Security from '@/components/accountManagement/Security.vue'
import OrderDetails from '@/components/storeManagement/ordersManagement/OrderDetails.vue'
import OrderManagement from '@/components/storeManagement/ordersManagement/OrderManagement.vue'
import RefundRequestManagement from '@/components/storeManagement/ordersManagement/RefundRequestManagement.vue'
import Inventory from '@/components/storeManagement/Inventory.vue'
import Employees from '@/components/storeManagement/Employees.vue'
import HomePage from '@/views/HomePage.vue'
import AboutPage from '@/views/AboutPage.vue'
import RefundRequestSuccess from '@/views/RefundRequestSuccess.vue'
import CatalogPage from '@/views/CatalogPage.vue'
import OrderPage from '@/views/OrderPage.vue'
import ConnectionPage from '@/views/ConnectionPage.vue'
import RefundRequestPage from '@/views/RefundRequestPage.vue'
import CardDetailsPage from '@/views/CardDetailsPage.vue'
import ManageAccountPage from '@/views/ManageAccountPage.vue'
import ManageStorePage from '@/views/ManageStorePage.vue'
import UserGuidePage from '@/views/UserGuidePage.vue'
import RegisterPage from '@/views/RegisterPage.vue'
import CartPage from '@/views/CartPage.vue'
import FrogotPasswordPage from '@/views/FrogotPasswordPage.vue'
import NewPasswordPage from '@/views/NewPasswordPage.vue'
import PrivacyPolicyPage from '@/views/PrivacyPolicyPage.vue'
import CardsPromotionPage from '@/views/CardsPromotionPage.vue'
import ResetCodeLoadingPage from '@/views/ResetCodeLoadingPage.vue'
import ResetCodeSentPage from '@/views/ResetCodeSentPage.vue'
import PaymentSuccessPage from '@/views/PaymentSuccessPage.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior() {
    document.getElementById('app').scrollIntoView()
  },
  routes: [
    { path: '/', name: 'default', component: HomePage },
    {
      path: '/catalogue',
      name: 'catalogue',
      component: CatalogPage,
    },
    {
      path: '/panier',
      name: 'MonPanier',
      component: CartPage,
    },

    {
      path: '/inscription',
      name: 'Inscription',
      component: RegisterPage,
    },
    {
      path: '/GestionCompte',
      name: 'Gestioncompte',
      component: ManageAccountPage,
      meta: { requiresAuth: true },
      redirect: (to) => {
        return { name: 'Profil' }
      },
      children: [
        {
          path: 'Profil',
          name: 'Profil',
          component: Profil,
        },
        {
          path: 'HistoriqueCommande',
          name: 'HistoriqueCommande',
          component: OrderHistory,
        },
        {
          path: 'Adresse',
          name: 'Adresse',
          component: Adress,
        },
        {
          path: 'Securite',
          name: 'Securite',
          component: Security,
        },
      ],
    },

    {
      path: '/Connexion',
      name: 'Connexion',
      component: ConnectionPage,
    },

    {
      path: '/ForgotPassword',
      name: 'Forgotpassword',
      component: FrogotPasswordPage,
    },
    {
      path: '/resetCodeSent',
      name: 'resetCodeSent',
      component: ResetCodeSentPage,
    },
    {
      path: '/resetCodeLoading',
      name: 'resetCodeLoading',
      component: ResetCodeLoadingPage,
      props: (route) => ({
        email: route.query.email,
        resetCode: route.query.resetCode,
      }),
    },
    { path: '/NewPassword', name: 'NewPassword', component: NewPasswordPage },
    {
      path: '/Cards/:id/',
      name: 'DetailsCarte',
      component: CardDetailsPage,
    },
    {
      path: '/promotion/:id/cartes',
      name: 'PromotionCartes',
      component: CardsPromotionPage,
    },
    {
      path: '/APropos',
      name: 'APropos',
      component: AboutPage,
    },
    {
      path: '/GuideUtilisation',
      name: 'GuideUtilisation',
      component: UserGuidePage,
    },
    {
      path: '/PolitiqueConfidentialite',
      name: 'PolitiqueConfidentialite',
      component: PrivacyPolicyPage,
    },
    {
      path: '/Commander',
      name: 'Commander',
      component: OrderPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/details-commande/:orderId',
      name: 'DetailsCommande',
      component: OrderDetails,
      meta: { requiresAuth: true },
      props: (route) => ({
        orderId: route.params.orderId,
      }),
    },
    {
      path: '/GestionMagasin',
      name: 'GestionMagasin',
      component: ManageStorePage,
      meta: { requiresAuth: true },
      redirect: (to) => {
        return { name: 'Inventaire' }
      },
      children: [
        {
          path: 'promotions',
          name: 'Promotions',
          component: Promotions,
        },
        {
          path: 'inventaire',
          name: 'Inventaire',
          component: Inventory,
        },
        {
          path: 'employes',
          name: 'Employes',
          component: Employees,
        },
        {
          path: 'manage-employee',
          name: 'Manage-employee',
          component: ManageEmployee,
          props: (route) => ({
            id: route.query.id,
          }),
        },
        {
          path: 'add-employee',
          name: 'Add-employee',
          component: AddEmployee,
        },
        {
          path: 'manage-client',
          name: 'Manage-client',
          component: ManageClient,
          props: (route) => ({
            id: route.query.id,
          }),
        },
        {
          path: 'clients',
          name: 'Clients',
          component: Clients,
        },

        {
          path: 'gestionCommandes',
          name: 'gestionCommandes',
          component: OrderManagement,
        },
        {
          path: 'gestionDemandesRemboursement',
          name: 'gestionDemandesRemboursement',
          component: RefundRequestManagement,
        },
      ],
    },
    {
      path: '/demande-remboursement/:id',
      name: 'demandeRemboursement',
      component: RefundRequestPage,
    },
    {
      path: '/PaymentSuccess',
      name: 'PaymentSuccess',
      component: PaymentSuccessPage,
    },
    {
      path: '/demande-remboursement-success',
      name: 'DemandeRemboursementSuccess',
      component: RefundRequestSuccess,
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: NotFound,
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  const auth = useAuthStore()
  const role = auth.role?.[0]
  if (
    (to.name === 'Inventaire' ||
      to.name === 'gestionCommandes' ||
      to.name === 'gestionDemandesRemboursement' ||
      to.name === 'Clients' ||
      to.name === 'Manage-client') &&
    to.meta.requiresAuth
  ) {
    if (role === 'employee' || role === 'manager') {
      next()
    } else {
      next('/connexion')
    }
  } else if (
    (to.name === 'Promotions' ||
      to.name === 'Employes' ||
      to.name === 'Manage-employee' ||
      to.name === 'Add-employee') &&
    to.meta.requiresAuth
  ) {
    if (role === 'manager') {
      next()
    } else {
      next('/connexion')
    }
  } else if (to.meta.requiresAuth && !auth.isAuthenticated) {
    next({ path: '/connexion', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})

export default router
