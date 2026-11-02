export class ClientRegistrationRequest {
  constructor(firstName, name, email, phoneNumber, password, passwordConfirmation) {
    this.firstName = firstName
    this.name = name
    this.email = email
    this.phoneNumber = phoneNumber
    this.password = password
    this.passwordConfirmation = passwordConfirmation
  }
}
