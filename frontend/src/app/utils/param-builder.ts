import { HttpParams } from "@angular/common/http";

export function HttpParamsBuilder(request: Record<string, any>): HttpParams {
    // convert into json
    const convertedIntoJson = Object.entries(request)
    const filtered = convertedIntoJson.filter(([_, v]) => v != null && v != '')
    const convertBack = Object.fromEntries(filtered)
    return new HttpParams( { fromObject: convertBack })
}