import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppConsole } from '../../../../utils/app-console';
import { MyButton } from "../../buttons/my-button/my-button";
import { Pagination, PaginationEmitData } from "../../pagination/pagination";

/**
 * Storing table model here cause for the vibe 
 * For now, we will just go for minimal version, 
 * enhance it later 🗿
 */
export interface TableColumn<T> {
  key: string, 
  label: string, 
  isActionColumn: boolean
}

@Component({
  selector: 'app-my-table',
  imports: [MyButton, Pagination],
  templateUrl: './my-table.html',
  styleUrl: './my-table.css'
})
export class MyTable<T> {

  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  @Input() columns: TableColumn<T>[] = [];
  @Input() data: T[] = [];

  @Output() actionClicked = new EventEmitter<T>();
  @Output() paginationClicked = new EventEmitter<PaginationEmitData>();

  getCellValue(row: any, key: string): any {
    const keys = key.split('.');
    const value = keys.reduce((val, currKey) => val?.[currKey], row);
    return value;
  }

  onActionClicked(data: T) {
    this.actionClicked.emit(data);
  }

  onPaginationClicked(data: PaginationEmitData) {
    this.paginationClicked.emit(data);
  }
}
