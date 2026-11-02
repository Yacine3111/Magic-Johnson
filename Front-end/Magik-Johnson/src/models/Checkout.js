export class Address {
  constructor() {
    this.email = ''
    this.civicNo = ''
    this.street = ''
    this.city = ''
    this.unit = ''
    this.province = ''
    this.country = 'Canada'
    this.postalCode = ''
  }
}

export class PaymentInfo {
  constructor() {
    this.cardNumberMasked = ''
    this.expMonth = ''
    this.expYear = ''
    this.cvvMasked = ''
  }
}
