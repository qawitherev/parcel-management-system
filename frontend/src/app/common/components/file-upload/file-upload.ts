import { Component } from '@angular/core';
import { excelToJson, mapperCheckInPayload } from '../../../core/bulk-action/excel-to-json';
import { CheckInPayload } from '../../../features/parcel/check-in/check-in-service';

@Component({
  selector: 'app-file-upload',
  imports: [],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css'
})
export class FileUpload {
  
  async onFileUpload(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement
    const file = input?.files?.[0]
    if (!file) return
    const acceptedFormats = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // xlsx
      'application/vnd.ms-excel' // xls
    ]
    if (!acceptedFormats.includes(file.type)) return // will do the error message later 
    
    try {
      const converted = excelToJson<CheckInPayload>(file, mapperCheckInPayload)
      
    } catch(err) {

    }
  }
}
