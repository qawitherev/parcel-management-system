import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
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

  constructor(private fb: FormBuilder, private cdr: ChangeDetectorRef) {
    this.formGroup = fb.group({
      file: [null, [Validators.required]]
    })
  }

  onFileChange(event: any): void {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0]
    this.formGroup.get('file')?.patchValue(file)
  }

  async onFileUpload(event: Event): Promise<void> {
    if(!this.formGroup.valid) {
      this.errorMessage = 'Not valid'
      return
    }
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
      return
    }
    
    try {
      this.isLoading = true
      const converted = await excelToJson<CheckInPayload>(file, mapperCheckInPayload)
      AppConsole.log(`FILE-UPLOAD: converted: ${JSON.stringify(converted)}`)
      this.dataEmitter.emit(converted)
      this.cancelEmitter.emit()
    } catch(err) {
      AppConsole.log(`FILE-UPLOAD: inside catch block`)
      this.errorMessage = `Error parsing file. Please try again. Error details: ${err}`
      this.cdr.detectChanges()
      AppConsole.error(`Parsing Error: ${err}`)
    } finally {
      this.isLoading = false
    }
  }

  onCancel() {
    this.cancelEmitter.emit()
  }
}
