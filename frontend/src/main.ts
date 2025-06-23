import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './app/services/auth.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Get the auth token from the service
  const authToken = authService.getToken();
  
  // Clone the request and add the authorization header if token exists
  let authReq = req;
  if (authToken) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`
      }
    });
  }

  // Handle the request and catch any auth errors
  return next(authReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        // Token is invalid or expired
        authService.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
}).catch(err => console.error(err));