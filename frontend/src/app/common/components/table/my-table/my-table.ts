import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MyButton } from "../../buttons/my-button/my-button";
import { Pagination, PaginationEmitData } from "../../pagination/pagination";

/**
 * Type-safe table column configuration
 * @template T - The type of data rows in the table
 */
export interface TableColumn<T = any> {
  key: keyof T | string;
  label: string;
  isActionColumn: boolean;
}

/**
 * Generic type-safe table component
 * @template T - The type of data rows in the table
 */
@Component({
  selector: 'app-my-table',
  imports: [MyButton, Pagination],
  templateUrl: './my-table.html',
  styleUrl: './my-table.css'
})
export class MyTable<T = any> {

  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  @Input() columns: TableColumn<T>[] = [];
  @Input() data: T[] = [];
  @Input() count: number = 0;

  @Output() actionClicked = new EventEmitter<T>();
  @Output() paginationClicked = new EventEmitter<PaginationEmitData>();

  /**
   * Get cell value from a row using dot notation for nested properties
   * @param row - The data row
   * @param key - The property key (supports dot notation for nested properties)
   * @returns The cell value
   */
  getCellValue(row: T, key: keyof T | string): any {
    const keys = String(key).split('.');
    const value = keys.reduce((val, currKey) => val?.[currKey], row as any);
    return value;
  }

  /**
   * Handle action button click
   * @param data - The row data
   */
  onActionClicked(data: T): void {
    this.actionClicked.emit(data);
  }

  /**
   * Handle pagination change
   * @param data - The pagination data
   */
  onPaginationClicked(data: PaginationEmitData): void {
    this.paginationClicked.emit(data);
  }
}
