import {
  Component,
  EventEmitter,
  Output,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { AppConsole } from '../../../utils/app-console';
import { NgFor } from '@angular/common';

export interface PaginationEmitData {
  currentPage: number;
  pageSize: number;
}

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [NgFor],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class Pagination implements OnChanges {
  @Input() count!: number;
  @Output() paginationDataEmitter = new EventEmitter<PaginationEmitData>();

  pageNavDisplay: (number | string)[] = [];
  currentPage: number = 1;
  totalPage?: number;
  pageSize: number = 1;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['count']) {
      this.totalPage = Math.ceil(this.count / this.pageSize);
      this.pageNavDisplay = this.getPageNavDisplay(this.currentPage, this.totalPage);
    }
  }

  onPageNavigate(newPage: number) {
    if (newPage < 1) {
      this.currentPage = 1;
    } else if (newPage > this.totalPage!) {
      this.currentPage = this.totalPage!;
    } else {
      this.currentPage = newPage;
    }
    this.paginationDataEmitter.emit({
      currentPage: this.currentPage,
      pageSize: this.pageSize,
    });
  }

  getPageNavDisplay(currentPage: number, totalPages: number): (number | string)[] {
    var temp = [];
    temp.push(1);

    const left = Math.max(2, currentPage - 1);
    const right = Math.min(totalPages - 1, currentPage + 1);

    if (left > 2) {
      temp.push('..');
    }

    for (let i = left; i < right + 1; i++) {
      temp.push(i);
    }

    if (right < totalPages - 1) {
      temp.push('..');
    }

    if (totalPages > 1) {
      temp.push(totalPages);
    }

    AppConsole.log(`temp is: ${temp}`);

    return temp;
  }
}
