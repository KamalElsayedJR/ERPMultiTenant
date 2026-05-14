import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, _state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const toastr = inject(ToastrService);

    const requiredRoles = (route.data?.['roles'] as string[] | undefined) ?? [];

    const evaluateRole = (role: string | null | undefined) => {
        if (requiredRoles.length === 0) {
            return true;
        }

        const isAllowed = requiredRoles.some(
            (requiredRole) => requiredRole.toLowerCase() === (role ?? '').toLowerCase()
        );

        if (!isAllowed) {
            toastr.error('You do not have access to this page.');
            return router.createUrlTree(['/dashboard']);
        }

        return true;
    };

    return authService.waitForAuthReady().pipe(
        switchMap(() => {
            const snapshot = authService.getCurrentUserSnapshot();
            if (snapshot?.role) {
                return of(evaluateRole(snapshot.role));
            }

            return authService.loadCurrentUser().pipe(
                map((user) => evaluateRole(user?.role ?? null))
            );
        })
    );
};
