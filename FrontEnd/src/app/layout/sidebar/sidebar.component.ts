import { AsyncPipe, CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { map } from 'rxjs/operators';

import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, AsyncPipe, RouterLink, RouterLinkActive],
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
    readonly canManageEmployees$ = this.authService.currentUser$.pipe(
        map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageEmployees', user))
    );
    readonly canManageDepartments$ = this.authService.currentUser$.pipe(
        map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageDepartments', user))
    );
    readonly canManageUsers$ = this.authService.currentUser$.pipe(
        map((user) => this.authService.isAdmin(user) || this.authService.hasPermission('ManageUsers', user))
    );

    constructor(private readonly authService: AuthService) { }
}
