import { Component, EventEmitter, Input, Output } from '@angular/core';
import { excelToJson, mapperCheckInPayload } from '../../../core/bulk-action/excel-to-json';
import { CheckInPayload } from '../../../features/parcel/check-in/check-in-service';
import { AppConsole } from '../../../utils/app-console';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css'
})
export class FileUpload {

  @Input() mapper!: (data: any) => any;
  @Output() dataEmitter = new EventEmitter<any[]>()
  @Output() cancelEmitter = new EventEmitter<void>()

  errorMessage: string | null = null
  isLoading: boolean = false

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
      this.isLoading = true
      const converted = await excelToJson<CheckInPayload>(file, mapperCheckInPayload)
      this.dataEmitter.emit(converted)
    } catch(err) {
      this.errorMessage = `Error parsing file. Please try again`
      AppConsole.error(`Parsing Error: ${JSON.stringify(err)}`)
    } finally {
      this.isLoading = false
      input.value = ''
    }
  }

  onCancel() {
    this.cancelEmitter.emit()
  }
}
