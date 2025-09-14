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
import { FormsModule } from '@angular/forms';
import { NgClass } from '@angular/common';

export interface PaginationEmitData {
  currentPage: number;
  pageSize: number;
}

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [NgFor, FormsModule, NgClass],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class Pagination implements OnChanges {
  @Input() count!: number;
  @Input() inCurrentPage!: number;
  @Input() inPageSize!: number
  @Output() paginationDataEmitter = new EventEmitter<PaginationEmitData>();

  pageNavDisplay: (number | string)[] = [];
  totalPage?: number;

  pageSizeOptions = [10, 50, 100]

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['count']) {
      this.totalPage = Math.ceil(this.count / this.inPageSize);
      this.pageNavDisplay = this.getPageNavDisplay(this.inCurrentPage, this.totalPage);
    }
  }

  onPageNavigate(newPage: number | string) {
    if (typeof newPage !== 'number') {
      return;
    }
    if (newPage < 1) {
      this.inCurrentPage = 1;
    } else if (newPage > this.totalPage!) {
      this.inCurrentPage = this.totalPage!;
    } else {
      this.inCurrentPage = newPage;
    }
    AppConsole.log(`PAGINATION: CurrentState: currentPage: ${this.inCurrentPage}, totalPage: ${this.totalPage}, pageSize: ${this.inPageSize}`)
    this.paginationDataEmitter.emit({
      currentPage: this.inCurrentPage,
      pageSize: this.inPageSize,
    });
  }

  onPageSizeChanged(event: Event) {
    const select = event.target as HTMLSelectElement
    const newPageSize = Number(select.value)
    this.inPageSize = newPageSize
    this.paginationDataEmitter.emit({
      currentPage: this.inCurrentPage, 
      pageSize: newPageSize
    })
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
