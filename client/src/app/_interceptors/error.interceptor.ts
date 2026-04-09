import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';
import { AccountService } from '../_services/account.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);
  
  return next(req).pipe(
    catchError(error => {
      console.log('HTTP Error:', error);
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error?.errors) {
              const modalStateErrors = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modalStateErrors.push(error.error.errors[key]);
                }
              }
              throw modalStateErrors.flat();
            } else {
              const errorMessage = error.error?.message || 'Bad Request';
              toastr.error(errorMessage, error.status.toString());
            }
            break;
          case 401:
            if (!req.url.includes('/account/refresh')) {
              localStorage.removeItem('user');
              toastr.error('Unauthorized - Please login', error.status.toString());
              router.navigateByUrl('/login');
            }
            break;
          case 403:
            toastr.error('Forbidden - You do not have access', '403');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExtras: NavigationExtras = { state: { error: error.error } };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          default:
            toastr.error('Something unexpected went wrong');
            break;
        }
      }
      throw error;
    })
  );
};