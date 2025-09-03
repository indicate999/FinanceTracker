import {HttpInterceptorFn} from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  let authToken: string | null = null;
  const userString = localStorage.getItem('currentUser');

  if (userString) {
    try {
      const user = JSON.parse(userString);
      authToken = user.token;
    } catch (e) {
      console.error('Error parsing user data from localStorage', e);
    }
  }

  if (authToken) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`,
      },
    });
    return next(authReq);
  }

  return next(req);
}
