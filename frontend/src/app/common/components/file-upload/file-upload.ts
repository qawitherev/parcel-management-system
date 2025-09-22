import { Component, EventEmitter, Input, Output } from '@angular/core';
import { excelToJson, mapperCheckInPayload } from '../../../core/bulk-action/excel-to-json';
import { CheckInPayload } from '../../../features/parcel/check-in/check-in-service';
import { AppConsole } from '../../../utils/app-console';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css'
})
export class FileUpload {

  @Input() mapper!: (data: any) => any;
  @Output() dataEmitter = new EventEmitter<any[]>()
  @Output() cancelEmitter = new EventEmitter<void>()

  errorMessage: string | null = null
  isLoading: boolean = false

  formGroup: FormGroup

  constructor(private fb: FormBuilder) {
    this.formGroup = fb.group({
      file: [null, [Validators.required]]
    })
  }

  async onFileUpload(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement
    const file = this.formGroup.get('file')?.value
    if (!file) {
      this.errorMessage = "No file found"
      return
    }
    const acceptedFormats = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // xlsx
      'application/vnd.ms-excel' // xls
    ]
    if (!acceptedFormats.includes(file.type)) {
      this.errorMessage = "File type not supported. Only upload xlsx or xls file"
    }
    
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
