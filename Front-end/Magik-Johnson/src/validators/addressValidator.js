export function ValidateStreet(name, errorStatus, errorMessage) {
  if (!/^[\p{L} ,.'-]+$/u.test(name.value)) {
    errorStatus.value = true
    errorMessage.value = 'Rue invalide'
    return false
  } else {
    errorStatus.value = false
    errorMessage.value = ''
    return true
  }
}

export function ValidateCity(name, errorStatus, errorMessage) {
  if (!/^[\p{L} ,.'-]+$/u.test(name.value)) {
    errorStatus.value = true
    errorMessage.value = 'Ville invalide'
    return false
  } else {
    errorStatus.value = false
    errorMessage.value = ''
    return true
  }
}

export function ValidatePostalCode(name, errorStatus, errorMessage) {
  if (!/^[A-Z][0-9][A-Z] [0-9][A-Z][0-9]$/.test(name.value)) {
    errorStatus.value = true
    errorMessage.value = 'Code postal invalide'
    return false
  } else {
    errorStatus.value = false
    errorMessage.value = ''
    return true
  }
}

export function ValidateState(name, errorStatus, errorMessage) {
  if (name.value == '') {
    errorStatus.value = true
    errorMessage.value = 'Province requise'
    return false
  } else {
    errorStatus.value = false
    errorMessage.value = ''
    return true
  }
}
