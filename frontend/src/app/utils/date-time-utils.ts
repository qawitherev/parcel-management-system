import { AppConsole } from "./app-console";

export function getTime(utcDateTime: string): string {
        const utcTime = new Date(utcDateTime)
        const gmtPlusEight = new Date(utcTime.getTime() + 8 * 60 * 60)

        const now = new Date()
        const diffInSeconds = Math.floor(now.getTime() - gmtPlusEight.getTime())

        AppConsole.log(`diffInSeconds is: ${diffInSeconds}`)

        if (diffInSeconds < 60) {
            return "Now"
        } else if (diffInSeconds < 60 * 60) {
            const minutesAgo = Math.floor(diffInSeconds / 60)
            return `${minutesAgo} minutes ago`
        } else if (diffInSeconds < 1 * 60 * 60) {
            const hoursAgo = Math.floor(diffInSeconds / 60 * 60)
            return `${hoursAgo} hours ago`
        } else {
            const formattedDate = new Intl.DateTimeFormat(
                'en-MY', 
                {
                    day: '2-digit', 
                    month: '2-digit', 
                    year: 'numeric', 
                    hour: '2-digit', 
                    minute: '2-digit',
                    hour12: true
                }
            ).format(gmtPlusEight)
            return formattedDate
        }
    }