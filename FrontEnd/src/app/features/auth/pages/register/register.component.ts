import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        RouterLink
    ],
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss'
})
export class RegisterComponent {
    readonly form = this.fb.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(3)]],
        companyName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]]
    });

    loading = false;

    constructor(
        private readonly fb: FormBuilder,
        private readonly authService: AuthService,
        private readonly toastr: ToastrService,
        private readonly router: Router
    ) { }

    get fullNameControl() {
        return this.form.controls.fullName;
    }

    get emailControl() {
        return this.form.controls.email;
    }

    get companyNameControl() {
        return this.form.controls.companyName;
    }

    get passwordControl() {
        return this.form.controls.password;
    }

    onSubmit(): void {
        if (this.form.invalid || this.loading) {
            return;
        }

        this.loading = true;
        this.authService.register(this.form.getRawValue()).pipe(
            finalize(() => {
                this.loading = false;
            })
        ).subscribe({
            next: () => {
                this.toastr.success('Account created. You can now log in.');
                this.router.navigate(['/login']);
            },
            error: (error) => {
                const message = this.getErrorMessage(error, '');
                if (this.isEmailExistsMessage(message)) {
                    this.applyEmailExistsError();
                    this.toastr.error(message);
                    return;
                }
                const fallback = message || 'Registration failed. Please try again.';
                this.toastr.error(fallback);
            }
        });
    }

    clearEmailExistsError(): void {
        if (!this.emailControl.hasError('emailExists')) {
            return;
        }

        const errors = { ...(this.emailControl.errors ?? {}) };
        delete errors['emailExists'];
        this.emailControl.setErrors(Object.keys(errors).length ? errors : null);
    }

    private applyEmailExistsError(): void {
        const errors = { ...(this.emailControl.errors ?? {}) };
        errors['emailExists'] = true;
        this.emailControl.setErrors(errors);
    }

    private getErrorMessage(error: unknown, fallback: string): string {
        const message = (error as { error?: { message?: string } })?.error?.message;
        return typeof message === 'string' && message.trim().length > 0 ? message : fallback;
    }

    private isEmailExistsMessage(message: string): boolean {
        return message.trim().toLowerCase() === 'email already exists';
    }
}
