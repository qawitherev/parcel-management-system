import { Component, Input } from '@angular/core';

/**
 * Storing table model here cause for the vibe 
 * For now, we will just go for minimal version, 
 * enhance it later 🗿
 */
export interface TableColumn<T> {
  key: keyof T | string, 
  label: string
}

@Component({
  selector: 'app-my-table',
  imports: [],
  templateUrl: './my-table.html',
  styleUrl: './my-table.css'
})
export class MyTable<T> {
  @Input() columns: TableColumn<T>[] = [];
  @Input() data: T[] = [];

  getCellValue(row: any, key: string): any {
    const keys = key.split('.');
    const value = keys.reduce((val, currKey) => val?.[currKey], row);
    return value;
  }
}
