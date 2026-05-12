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
                this.toastr.error(this.getErrorMessage(error, 'Registration failed. Please try again.'));
            }
        });
    }

    private getErrorMessage(error: unknown, fallback: string): string {
        const message = (error as { error?: { message?: string } })?.error?.message;
        return typeof message === 'string' && message.trim().length > 0 ? message : fallback;
    }
}
