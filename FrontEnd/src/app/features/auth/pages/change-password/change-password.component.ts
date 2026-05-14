import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent {
  private readonly strongPasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;

  readonly form = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required, Validators.minLength(6)]],
    newPassword: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(this.strongPasswordPattern)
      ]
    ],
    confirmPassword: ['', [Validators.required]]
  }, { validators: [this.matchPasswordsValidator] });

  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly toastr: ToastrService
  ) { }

  get currentPasswordControl() {
    return this.form.controls.currentPassword;
  }

  get newPasswordControl() {
    return this.form.controls.newPassword;
  }

  get confirmPasswordControl() {
    return this.form.controls.confirmPassword;
  }

  get hasPasswordMismatch(): boolean {
    return this.form.hasError('passwordMismatch') && this.confirmPasswordControl.touched;
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) {
      return;
    }

    this.loading = true;
    const payload = {
      currentPassword: this.currentPasswordControl.value,
      newPassword: this.newPasswordControl.value,
      confirmPassword: this.confirmPasswordControl.value
    };

    this.authService.changePassword(payload).pipe(
      finalize(() => {
        this.loading = false;
      })
    ).subscribe({
      next: () => {
        this.toastr.success('Password updated successfully.');
        this.form.reset({
          currentPassword: '',
          newPassword: '',
          confirmPassword: ''
        });
      },
      error: (error) => {
        const message = this.getErrorMessage(error, 'Unable to update password. Please try again.');
        this.toastr.error(message);
      }
    });
  }

  private matchPasswordsValidator(control: AbstractControl): ValidationErrors | null {
    const group = control as { get: (key: string) => AbstractControl | null };
    const newPassword = group.get('newPassword')?.value as string | undefined;
    const confirmPassword = group.get('confirmPassword')?.value as string | undefined;
    if (!newPassword || !confirmPassword) {
      return null;
    }
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const message = (error as { error?: { message?: string } })?.error?.message;
    return typeof message === 'string' && message.trim().length > 0 ? message : fallback;
  }
}
