import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map, catchError, throwError } from 'rxjs';
import { ApiResponse } from '../_models/shared/apiResponse';

export const apiEnvelopeInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body) {
        const body = event.body;
        
        const isEnveloped = 
          typeof body === 'object' && 
          body !== null && 
          'isSuccess' in body && 
          'value' in body;
        
        if (isEnveloped) {
          const apiResponse = body as ApiResponse<any>;
          
          if (apiResponse.isSuccess && apiResponse.value !== undefined) {
            return event.clone({ body: apiResponse.value });
          }
          
          if (!apiResponse.isSuccess) {
            throw new Error(apiResponse.error ?? 'API request failed');
          }
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
