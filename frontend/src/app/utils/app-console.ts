import { environment } from "../../environment/environment"

export class AppConsole {
    static log(...args: any[]) {
        if (environment.enabledLogging) {
            console.log(...args)
        }
    }

    static error(...args: any[]) {
        if (environment.enabledLogging) {
            console.error(...args)
        }
    }
}