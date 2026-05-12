import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const toastr = inject(ToastrService);

    if (authService.isAuthenticated()) {
        return true;
    }

    toastr.warning('Please log in to continue.');
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const authChildGuard: CanActivateChildFn = (route, state) =>
    authGuard(route, state);
