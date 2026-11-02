export class resetPasswordRequest {
  constructor(email, resetCode, newPassword) {
    this.email = email
    ;((this.resetCode = resetCode), (this.newPassword = newPassword))
  }
}
