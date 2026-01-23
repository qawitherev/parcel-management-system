import { HttpEvent, HttpHandler, HttpInterceptor, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { AppConsole } from "../../utils/app-console";
import { TOKEN_STORAGE_KEY } from "../../features/auth/auth-service";

export const AttachTokenInterceptor: HttpInterceptorFn = (req, next) => {
  AppConsole.log(`Intercepting request`)
  const token = localStorage.getItem(TOKEN_STORAGE_KEY)
  if (token && !req.url.includes('refresh') && !req.url.includes('login')) {
    const cloned = req.clone({
      headers: req.headers.set(`Authorization`, `Bearer ${token}`)
    })
    return next(cloned);
  }
  return next(req)
}
