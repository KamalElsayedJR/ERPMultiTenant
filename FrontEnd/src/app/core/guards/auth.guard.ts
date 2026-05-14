import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.waitForAuthReady().pipe(
        map(() => {
            if (authService.isAuthenticated()) {
                return true;
            }

            return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
        })
    );
};

export const authChildGuard: CanActivateChildFn = (route, state) =>
    authGuard(route, state);
