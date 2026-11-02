import { defineStore } from 'pinia'

export const useCountryStore = defineStore('country', {
  state: () => ({
    countries: null,
    allUserAddress: [],
  }),
})
