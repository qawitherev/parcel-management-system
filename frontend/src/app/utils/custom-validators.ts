import { AbstractControl, ValidationErrors } from "@angular/forms"

export const passwordMatchValidator = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value
  const confirmPassword = control.get('confirmPassword')?.value
  return password === confirmPassword ? null : 
    { passwordMismatch: true, errorString: 'Passwords do not match' }
}