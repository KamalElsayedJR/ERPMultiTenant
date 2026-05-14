import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

export const permissionGuard: CanActivateFn = (route, _state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const toastr = inject(ToastrService);

    const requiredPermissions = (route.data?.['permissions'] as string[] | undefined) ?? [];

    const evaluatePermissions = (userPermissionsOk: boolean) => {
        if (requiredPermissions.length === 0) {
            return true;
        }

        if (!userPermissionsOk) {
            toastr.error('You do not have permission to access this page.');
            return router.createUrlTree(['/dashboard']);
        }

        return true;
    };

    return authService.waitForAuthReady().pipe(
        switchMap(() => {
            const snapshot = authService.getCurrentUserSnapshot();
            if (snapshot) {
                return of(evaluatePermissions(authService.hasPermission(requiredPermissions, snapshot)));
            }

            return authService.loadCurrentUser().pipe(
                map((user) => evaluatePermissions(authService.hasPermission(requiredPermissions, user)))
            );
        })
    );
};
