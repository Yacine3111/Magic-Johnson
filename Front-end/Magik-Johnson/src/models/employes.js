export class Employes {
  constructor(id, firstName, lastName, islock) {
    this.id = id
    this.firstName = firstName
    ;((this.lastName = lastName), (this.islock = islock))
  }
}
