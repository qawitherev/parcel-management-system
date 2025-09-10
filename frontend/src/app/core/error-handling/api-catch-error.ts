import { Observable, of } from "rxjs";
import { AppConsole } from "../../utils/app-console";

export interface ApiError {
    error: boolean, 
    message: string
}

export function handleApiError(err: any): Observable<ApiError> {
    AppConsole.error(`Api Error: ${err.error.message}`)
    return of({error: true, message: err.error.message})
}