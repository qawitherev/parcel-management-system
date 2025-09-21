import * as XLSX from 'xlsx';
import { CheckInPayload } from '../../features/parcel/check-in/check-in-service';
import { registerLocaleData } from '@angular/common';

export function excelToJson<T>(file: File, mapper: (data: any) => T): Promise<T[]> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e: any) => {
      const data = new Uint8Array(e.target.result);
      const workbook = XLSX.read(data, { type: 'array' });

      const worksheet = workbook.Sheets[0];
      const jsonData = XLSX.utils.sheet_to_json(worksheet);
      const converted = convertToObjects<T>(jsonData, mapper);
      resolve(converted);
    };

    reader.onerror = (err) => {
      reject(err);
    };

    reader.readAsArrayBuffer(file);
  });
}

function convertToObjects<T>(data: any[], mapper: (row: any) => T): T[] {
  return data.map(mapper);
}

function mapperCheckInPayload(data: any): CheckInPayload {
    return {
        trackingNumber: data.trackingNumber, 
        residentUnit: data.residentUnit, 
        weight: data.weight ? data.weight : undefined, 
        dimension: data.dimension ? data.dimension : undefined
    }
}
