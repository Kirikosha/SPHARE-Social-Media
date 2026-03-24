import { HttpInterceptorFn } from '@angular/common/http';
import { map, catchError, throwError } from 'rxjs';
import { ApiResponse } from '../_models/shared/apiResponse';

export const apiEnvelopeInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event.type === 4 && event.hasOwnProperty('body')) {
        const response = event as any;
        const body: ApiResponse<any> = response.body;
        
        if (body?.isSuccess && body.value !== undefined) {
          return response.clone({ body: body.value });
        }
        
        if (!body?.isSuccess) {
          throw new Error(body.error ?? 'API request failed');
        }
      }
      return event;
    }),
    catchError(err => {
      if (err?.error?.isSuccess === false) {
        return throwError(() => new Error(err.error.error ?? 'API error'));
      }
      return throwError(() => err);
    })
  );
};
