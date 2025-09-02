import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AuthEndpoints } from "./endpoints/auth-endpoints";
import { Observable } from "rxjs";

@Injectable()
export class AttachTokenInterceptor implements HttpInterceptor {
  private excludeEndpoints = [AuthEndpoints.register, AuthEndpoints.register]


  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // if auth endpoints, no need to attach token 
    if(this.excludeEndpoints.some(url => req.url.includes(url))) {
      return next.handle(req);
    }
    
    const token = localStorage.getItem('parcel-management-system-token')
    if(token) {
      const cloned = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
      return next.handle(cloned)
    } else {
      // Throw an error observable if token is missing
      return new Observable<HttpEvent<any>>((observer) => {
        observer.error(new Error('Authentication token is missing'));
      });
    }
    
  }
}
