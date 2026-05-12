import { AsyncPipe, CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-navbar',
    standalone: true,
    imports: [CommonModule, AsyncPipe, MatToolbarModule, MatButtonModule, RouterLink, RouterLinkActive],
    templateUrl: './navbar.component.html',
    styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
    readonly isAuthenticated$ = this.authService.isAuthenticated$;
    readonly currentUser$ = this.authService.currentUser$;
    readonly role$ = this.authService.role$;
    loggingOut = false;

    constructor(
        private readonly authService: AuthService,
        private readonly toastr: ToastrService,
        private readonly router: Router
    ) { }

    onLogout(): void {
        if (this.loggingOut) {
            return;
        }

        this.loggingOut = true;
        this.authService.logout().subscribe({
            next: () => {
                this.toastr.success('You have been logged out.');
                this.router.navigate(['/login']);
            },
            error: () => {
                this.toastr.error('Logout failed. Please try again.');
            },
            complete: () => {
                this.loggingOut = false;
            }
        });
    }
}
