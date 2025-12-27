import * as XLSX from 'xlsx';
import { CheckInPayload } from '../../features/parcel/check-in/check-in-service';
import { AppConsole } from '../../utils/app-console';
import { ClaimPayload } from '../../features/parcel/claim/claim-service';

export function excelToJson<T>(file: File, mapper: (data: any) => T): Promise<T[]> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e: any) => {
      const data = new Uint8Array(e.target.result);
      const workbook = XLSX.read(data, { type: 'array' });

      const worksheet = workbook.Sheets[workbook.SheetNames[0]];
      const jsonData = XLSX.utils.sheet_to_json(worksheet);
      try {
        const converted = convertToObjects<T>(jsonData, mapper);
        resolve(converted);
      } catch (err) {
        reject(err)
      }
      
    };

    reader.onerror = (err) => {
      reject(err);
    };

    reader.readAsArrayBuffer(file);
  });
}

function convertToObjects<T>(data: any[], mapper: (row: any) => T): T[] {
  return data.map((row, idx) => {
    try {
      return mapper(row)
    } catch (err) {
      throw new Error(`${err instanceof Error ? err.message : err}`)
    }
  });
}

export function mapperCheckInPayload(data: any): CheckInPayload{
    AppConsole.log(`MAPPER: data is ${JSON.stringify(data)}`)
    if (!data.TrackingNumber || !data.ResidentUnit) {
      throw new Error('Required fields missing value')
    }
    return {
        trackingNumber: data.TrackingNumber, 
        residentUnit: data.ResidentUnit, 
        locker: "", 
        weight: data.Weight ? data.Weight : undefined, 
        dimension: data.Dimension ? data.Dimension : undefined
    }
}

export function mapperClaimPayload(data: any): ClaimPayload {
  return {
    trackingNumber: data.trackingNumber
  }
}
