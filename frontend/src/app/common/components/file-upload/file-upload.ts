import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { excelToJson, mapperCheckInPayload } from '../../../core/bulk-action/excel-to-json';
import { CheckInPayload } from '../../../features/parcel/check-in/check-in-service';
import { AppConsole } from '../../../utils/app-console';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MyButton } from '../buttons/my-button/my-button';

const EXCEL_FILE = 'excel'
const CSV_FILE = 'csv'

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [ReactiveFormsModule, MyButton],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css',
})
export class FileUpload {
  @Input() mapper!: (data: any) => any;
  @Input() uploadType: string = 'excel';
  @Output() dataEmitter = new EventEmitter<any[]>();
  @Output() cancelEmitter = new EventEmitter<void>();

  errorMessage: string | null = null;
  isLoading: boolean = false;

  formGroup: FormGroup;

  constructor(private fb: FormBuilder, private cdr: ChangeDetectorRef) {
    this.formGroup = fb.group({
      file: [null, [Validators.required]],
    });
  }

  onFileChange(event: any): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.formGroup.get('file')?.patchValue(file);
  }

  async onFileUploadParent(event: Event) {
    if (!this.formGroup.valid) {
      this.errorMessage = 'Not valid';
      return;
    }
    const file = this.formGroup.get('file')?.value;
    if (!file) {
      this.errorMessage = 'No file found';
      return;
    }
    this.uploadType === 'excel' ? await this.onFileUploadExcelOrCsv() : this.onFileUpload(file);
  }

  async onFileUpload(file: File) {
    throw Error('not implemented');
  }

  async onFileUploadExcelOrCsv(): Promise<void> {
    if (!this.formGroup.valid) {
      this.errorMessage = 'Not valid';
      return;
    }
    const file = this.formGroup.get('file')?.value;
    if (!file) {
      this.errorMessage = 'No file found';
      return;
    }

    const acceptedFormats: Record<string, 'excel' | 'csv'> = {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'excel', 
      'application/vnd.ms-excel': 'excel', 
      'text/csv': 'csv'
    }

    const fileType = acceptedFormats[file.type]
    if (!fileType) {
      this.errorMessage = 'File type not supported. Only upload xlsx or xls or csv file';
      return;
    }

    
  }

  private async processExcel(file: File) {
        try {
      this.isLoading = true;
      const converted = await excelToJson(file, this.mapper);
      AppConsole.log(`FILE-UPLOAD: converted: ${JSON.stringify(converted)}`);
      this.dataEmitter.emit(converted);
      this.cancelEmitter.emit();
    } catch (err) {
      AppConsole.log(`FILE-UPLOAD: inside catch block`);
      this.errorMessage = `Error parsing file. Please try again. Error details: ${err}`;
      this.cdr.detectChanges();
      AppConsole.error(`Parsing Error: ${err}`);
    } finally {
      this.isLoading = false;
    }
  }

  private processCsv() {

  }

  onCancel() {
    this.cancelEmitter.emit();
  }
}
