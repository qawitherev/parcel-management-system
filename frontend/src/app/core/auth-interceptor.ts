import { HttpEvent, HttpHandler, HttpInterceptor, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { AppConsole } from "../utils/app-console";

export const AttachTokenInterceptor: HttpInterceptorFn = (req, next) => {
  AppConsole.log(`Intercepting request`)
  const token = localStorage.getItem('parcel-management-system-token')
  if (token) {
    AppConsole.log(`Token is ${token}`)
    const cloned = req.clone({
      headers: req.headers.set(`Authorization`, `Bearer ${token}`)
    })
    return next(cloned);
  }
  return next(req)
}
