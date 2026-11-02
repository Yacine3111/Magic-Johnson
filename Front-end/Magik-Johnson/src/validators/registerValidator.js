export function validateFirstName(firstname, errorSatus, errorMessage) {
  if (firstname.value.trim() == '') {
    errorMessage.value = 'Prénom requis.'
    errorSatus.value = true
    return false
  } else if (!/^[\p{L} ,.'-]+$/u.test(firstname.value)) {
    errorMessage.value = 'Prénom invalide.'
    errorSatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorSatus.value = false
    return true
  }
}

export function validateName(name, errorSatus, errorMessage) {
  if (name.value.trim() == '') {
    errorMessage.value = 'Nom requis.'
    errorSatus.value = true
    return false
  } else if (!/^[\p{L} ,.'-]+$/u.test(name.value)) {
    errorMessage.value = 'Nom invalide.'
    errorSatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorSatus.value = false
    return true
  }
}

export function validateEmail(email, errorSatus, errorMessage) {
  if (email.value.trim() == '') {
    errorMessage.value = 'Email requis.'
    errorSatus.value = true
    return false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    errorMessage.value = 'Email invalide.'
    errorSatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorSatus.value = false
    return true
  }
}

export function validatePhoneNumber(phoneNumber, errorSatus, errorMessage) {
  if (phoneNumber.value.trim() == '') {
    errorMessage.value = 'Numéro de téléphone requis.'
    errorSatus.value = true
    return false
  } else if (
    !/^[\+]?[(]?[0-9]{3}[)]?\s*[-\s\.]?\s*[0-9]{3}\s*[-\s\.]?\s*[0-9]{4,6}$/.test(
      phoneNumber.value,
    )
  ) {
    errorMessage.value = 'Numéro de téléphone invalide.'
    errorSatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorSatus.value = false
    return true
  }
}

export function validatePassword(password, errorStatus, errorMessage) {
  if (password.value.trim() == '') {
    errorMessage.value = 'Mot de passe requis.'
    errorStatus.value = true
    return false
  } else if (password.value.length < 8) {
    errorMessage.value = 'Le mot de passe doit contenir au moins 8 caractères.'
    errorStatus.value = true
    return false
  } else if (
    !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;"'<>,.?/~`]).{8,}$/.test(
      password.value,
    )
  ) {
    errorMessage.value =
      'Le mot de passe doit contenir au moins une minuscule, une majuscule, un chiffre et un caractère spécial.'
    errorStatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorStatus.value = false
    return true
  }
}

export function validateConfirmationPassword(
  password,
  confirmationPassword,
  errorStatus,
  errorMessage,
) {
  if (password.value.localeCompare(confirmationPassword.value) != 0) {
    errorMessage.value = 'Les mots de passe ne correspondent pas.'
    errorStatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorStatus.value = false
    return true
  }
}

export function validatePasswordForLogin(password, errorStatus, errorMessage) {
  if (password.value.trim() === '') {
    errorMessage.value = 'Mot de passe requis.'
    errorStatus.value = true
    return false
  } else {
    errorMessage.value = ''
    errorStatus.value = false
    return true
  }
}

export function validateCardNumber(cardNumber, errorStatus) {
  if (!/^\d{4} \d{4} \d{4} \d{4}$/.test(cardNumber.value)) {
    errorStatus.value = true
    return false
  } else {
    errorStatus.value = false
    return true
  }
}
