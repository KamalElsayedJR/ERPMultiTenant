import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { ToastrService } from 'ngx-toastr';
import { finalize, map } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { UsersService } from '../../../../core/services/users.service';
import { TenantUser, UserRole } from '../../../../core/models/user-management.models';

@Component({
    selector: 'app-users-list',
    standalone: true,
    imports: [
        CommonModule,
        AsyncPipe,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './users-list.component.html',
    styleUrl: './users-list.component.scss'
})
export class UsersListComponent implements OnInit {
    readonly roleOptions: UserRole[] = ['Admin', 'Manager', 'Employee', 'Accountant'];
    readonly defaultInviteRole: UserRole = 'Employee';
    readonly inviteForm = this.fb.nonNullable.group({
        fullName: ['', [Validators.required]],
        email: ['', [Validators.required, Validators.email]],
        role: [this.defaultInviteRole, [Validators.required]]
    });

    readonly canManageUsers$ = this.authService.currentUser$.pipe(
        map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageUsers', user))
    );

    users: TenantUser[] = [];
    usersLoading = false;
    usersError = '';
    adminCount = 0;
    lastAdminBlockedUserId: string | null = null;
    inviteOpen = false;
    inviteLoading = false;
    invitePasswordOpen = false;
    invitePassword = '';
    invitePasswordEmail = '';
    roleUpdating = new Set<string>();
    deletingUsers = new Set<string>();
    readonly roleSelections: Record<string, UserRole> = {};

    constructor(
        private readonly usersService: UsersService,
        private readonly authService: AuthService,
        private readonly fb: FormBuilder,
        private readonly toastr: ToastrService
    ) { }

    ngOnInit(): void {
        this.loadUsers();
    }

    get emailControl() {
        return this.inviteForm.controls.email;
    }

    get fullNameControl() {
        return this.inviteForm.controls.fullName;
    }

    get roleControl() {
        return this.inviteForm.controls.role;
    }

    openInvite(): void {
        this.inviteOpen = true;
    }

    closeInvite(): void {
        this.inviteOpen = false;
        this.inviteForm.reset({ fullName: '', email: '', role: this.defaultInviteRole });
    }

    closeInvitePassword(): void {
        this.invitePasswordOpen = false;
        this.invitePassword = '';
        this.invitePasswordEmail = '';
    }

    submitInvite(): void {
        if (this.inviteForm.invalid || this.inviteLoading) {
            return;
        }

        if (!this.roleOptions.includes(this.roleControl.value)) {
            this.toastr.error('Select a valid role before sending the invite.');
            return;
        }

        const inviteEmail = this.emailControl.value;
        this.inviteLoading = true;
        this.usersService.inviteUser(this.inviteForm.getRawValue()).pipe(
            finalize(() => {
                this.inviteLoading = false;
            })
        ).subscribe({
            next: (response) => {
                this.toastr.success('Invitation sent successfully.');
                const tempPassword = response?.data?.temporaryPassword ?? response?.data?.tempPassword;
                if (tempPassword) {
                    this.invitePassword = tempPassword;
                    this.invitePasswordEmail = inviteEmail;
                    this.invitePasswordOpen = true;
                }
                this.closeInvite();
                this.loadUsers();
            },
            error: (error) => {
                const message = this.getErrorMessage(error, '');
                if (this.isEmailExistsMessage(message)) {
                    this.applyInviteEmailExistsError();
                    this.toastr.error(message);
                    return;
                }
                const fallback = message || 'Invite failed. Please try again.';
                this.toastr.error(fallback);
            }
        });
    }

    loadUsers(): void {
        if (this.usersLoading) {
            return;
        }

        this.usersLoading = true;
        this.usersError = '';
        this.adminCount = 0;

        this.usersService.getTenantUsers().pipe(
            finalize(() => {
                this.usersLoading = false;
            })
        ).subscribe({
            next: (users) => {
                this.users = users;
                this.adminCount = users.filter((user) => user.role === 'Admin').length;
                const knownIds = new Set(users.map((user) => user.userId));
                Object.keys(this.roleSelections).forEach((userId) => {
                    if (!knownIds.has(userId)) {
                        delete this.roleSelections[userId];
                    }
                });
                users.forEach((user) => {
                    this.roleSelections[user.userId] = user.role;
                });
                if (this.lastAdminBlockedUserId) {
                    const blockedUser = users.find((user) => user.userId === this.lastAdminBlockedUserId);
                    if (!blockedUser || !this.isLastAdmin(blockedUser)) {
                        this.lastAdminBlockedUserId = null;
                    }
                }
            },
            error: () => {
                this.usersError = 'Unable to load users for this tenant.';
            }
        });
    }

    getRoleSelection(user: TenantUser): UserRole {
        return this.roleSelections[user.userId] ?? user.role;
    }

    onRoleSelectionChange(user: TenantUser, nextRole: UserRole): void {
        if (!user?.userId) {
            return;
        }
        if (!this.roleOptions.includes(nextRole)) {
            return;
        }
        if (this.isLastAdmin(user) && nextRole !== 'Admin') {
            this.lastAdminBlockedUserId = user.userId;
            this.toastr.error('Last admin cannot be downgraded.');
            this.roleSelections[user.userId] = 'Admin';
            return;
        }
        this.roleSelections[user.userId] = nextRole;
    }

    updateRole(user: TenantUser): void {
        if (!user.userId) {
            console.error('Cannot update role without userId.', user);
            return;
        }
        const selectedRole = this.getRoleSelection(user) as UserRole;
        if (!selectedRole || selectedRole === user.role) {
            return;
        }

        if (this.isLastAdmin(user) && selectedRole !== 'Admin') {
            this.lastAdminBlockedUserId = user.userId;
            this.toastr.error('Last admin cannot be downgraded.');
            this.roleSelections[user.userId] = user.role;
            return;
        }

        if (!this.roleOptions.includes(selectedRole)) {
            this.toastr.error('Select a valid role before updating.');
            return;
        }

        if (this.roleUpdating.has(user.userId)) {
            return;
        }

        const previousRole = user.role;
        const requestBody = { userId: user.userId, role: selectedRole };
        const requestUrl = `/api/users/${user.userId}/role`;
        this.roleUpdating.add(user.userId);
        console.log('Updating user role request.', {
            url: requestUrl,
            body: requestBody
        });
        this.usersService.updateUserRole(user.userId, requestBody).pipe(
            finalize(() => {
                this.roleUpdating.delete(user.userId);
            })
        ).subscribe({
            next: (response) => {
                const responseRole = this.coerceRole(response?.data?.role);
                const updatedRole = responseRole ?? selectedRole;
                user.role = updatedRole;
                this.roleSelections[user.userId] = updatedRole;
                this.toastr.success('Role updated successfully.');
            },
            error: (error) => {
                console.error('User role update failed.', {
                    userId: user.userId,
                    selectedRole,
                    status: (error as { status?: number })?.status,
                    validation: (error as { error?: unknown })?.error,
                    responseBody: (error as { error?: unknown })?.error,
                    error
                });
                this.roleSelections[user.userId] = previousRole;
                const message = this.getErrorMessage(error, '');
                if (this.isLastAdminErrorMessage(message)) {
                    this.lastAdminBlockedUserId = user.userId;
                    this.toastr.error(message);
                    return;
                }
                const fallback = message || 'Unable to update role.';
                this.toastr.error(fallback);
            }
        });
    }

    deleteUser(user: TenantUser): void {
        if (!user.userId) {
            console.error('Cannot delete user without userId.', user);
            return;
        }

        if (this.deletingUsers.has(user.userId)) {
            return;
        }

        if (this.isLastAdmin(user)) {
            this.lastAdminBlockedUserId = user.userId;
            this.toastr.error('Last admin cannot be deleted.');
            return;
        }

        const confirmed = window.confirm(`Delete ${user.fullName || user.email}? This cannot be undone.`);
        if (!confirmed) {
            return;
        }

        this.deletingUsers.add(user.userId);
        this.usersService.deleteUser(user.userId).pipe(
            finalize(() => {
                this.deletingUsers.delete(user.userId);
            })
        ).subscribe({
            next: () => {
                this.users = this.users.filter((item) => item.userId !== user.userId);
                delete this.roleSelections[user.userId];
                this.toastr.success('User deleted successfully.');
            },
            error: (error) => {
                console.error('User deletion failed.', {
                    userId: user.userId,
                    error
                });
                const message = this.getErrorMessage(error, '');
                if (this.isLastAdminErrorMessage(message)) {
                    this.lastAdminBlockedUserId = user.userId;
                    this.toastr.error(message);
                    return;
                }
                const fallback = message || 'Unable to delete user.';
                this.toastr.error(fallback);
            }
        });
    }

    clearInviteEmailExistsError(): void {
        if (!this.emailControl.hasError('emailExists')) {
            return;
        }
        const errors = { ...(this.emailControl.errors ?? {}) };
        delete errors['emailExists'];
        this.emailControl.setErrors(Object.keys(errors).length ? errors : null);
    }

    isLastAdmin(user: TenantUser): boolean {
        return user.role === 'Admin' && this.adminCount === 1;
    }

    isRoleUpdating(userId: string): boolean {
        return this.roleUpdating.has(userId);
    }

    isDeletingUser(userId: string): boolean {
        return this.deletingUsers.has(userId);
    }

    trackByUserId(index: number, user: TenantUser): string {
        return user.userId ?? `${index}`;
    }

    copyInvitePassword(): void {
        if (!this.invitePassword) {
            return;
        }

        if (navigator?.clipboard?.writeText) {
            navigator.clipboard.writeText(this.invitePassword).then(
                () => this.toastr.success('Temporary password copied.'),
                () => this.toastr.error('Unable to copy the temporary password.')
            );
            return;
        }

        const copied = this.fallbackCopy(this.invitePassword);
        if (copied) {
            this.toastr.success('Temporary password copied.');
        } else {
            this.toastr.error('Unable to copy the temporary password.');
        }
    }

    private getErrorMessage(error: unknown, fallback: string): string {
        const message = (error as { error?: { message?: string; error?: string }; message?: string })?.error?.message
            ?? (error as { error?: { message?: string; error?: string }; message?: string })?.error?.error
            ?? (error as { message?: string })?.message;
        return typeof message === 'string' && message.trim().length > 0 ? message : fallback;
    }

    private applyInviteEmailExistsError(): void {
        const errors = { ...(this.emailControl.errors ?? {}) };
        errors['emailExists'] = true;
        this.emailControl.setErrors(errors);
    }

    private isEmailExistsMessage(message: string): boolean {
        return message.trim().toLowerCase() === 'email already exists';
    }

    private isLastAdminErrorMessage(message: string): boolean {
        return message.trim().toLowerCase() === 'cannot delete last admin';
    }

    private fallbackCopy(text: string): boolean {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();

        let copied = false;
        try {
            copied = document.execCommand('copy');
        } catch {
            copied = false;
        }

        document.body.removeChild(textarea);
        return copied;
    }

    private coerceRole(value: unknown): UserRole | null {
        return this.roleOptions.includes(value as UserRole) ? (value as UserRole) : null;
    }
}
