export function checkEmptyFieldSecurity(
  currentPassword,
  currentPasswordErrorMessage,
  newPassword,
  newPasswordErrorMessage,
  passwordConfirmation,
  passwordConfirmationErrorMessage,
) {
  let triggered = false
  if (currentPassword.value.trim() == '') {
    currentPasswordErrorMessage.value = 'Mot de passe actuel requis.'
    triggered = true
  }
  if (newPassword.value.trim() == '') {
    newPasswordErrorMessage.value = 'Nouveau mot de passe requis'
    triggered = true
  }
  if (passwordConfirmation.value.trim() == '') {
    passwordConfirmationErrorMessage.value = 'Mot de passe de confirmation requis'
    triggered = true
  }

  if (triggered == true) {
    return false
  }
  return true
}

export function checkEmptyFieldAddress(
  addressNumber,
  addressNumberErrorMessage,
  street,
  streetErrorMessage,
  city,
  cityErrorMessage,
  postalCode,
  postalCodeErrorMessage,
  country,
  countryErrorMessage,
  state,
  stateErrorMessage,
) {
  let triggered = false
  if (addressNumber.value.trim() == '') {
    addressNumberErrorMessage.value = 'Adresse requise.'
    triggered = true
  }
  if (street.value.trim() == '') {
    streetErrorMessage.value = 'Rue requise'
    triggered = true
  }
  if (city.value.trim() == '') {
    cityErrorMessage.value = 'Ville requise'
    triggered = true
  }
  if (postalCode.value.trim() == '') {
    postalCodeErrorMessage.value = 'Code postal requis'
    triggered = true
  }
  if (country.value == '') {
    countryErrorMessage.value = 'Pays requis'
    triggered = true
  }
  if (state.value == '') {
    stateErrorMessage.value = 'Province requise'
    triggered = true
  }

  if (triggered == true) {
    return false
  }
  return true
}
