export class EmployeeRegistrationRequest {
  constructor(
    firstName,
    name,
    email,
    phoneNumber,
    password,
    passwordConfirmation,
    isAccountEnabled,
    hiringDate,
  ) {
    this.firstName = firstName
    this.name = name
    this.email = email
    this.phoneNumber = phoneNumber
    this.password = password
    this.passwordConfirmation = passwordConfirmation
    this.isAccountEnabled = isAccountEnabled
    this.hiringDate = hiringDate
  }
}
