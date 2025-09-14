import { AfterViewInit, Component, OnInit, ViewChild, viewChild } from '@angular/core';
import {
  BehaviorSubject,
  debounceTime,
  distinctUntilChanged,
  filter,
  Subject,
  switchMap,
} from 'rxjs';
import { ParcelResponseList, ParcelsService } from '../../parcels-service';
import { AsyncPipe, CommonModule, NgFor } from '@angular/common';
import {
  Pagination,
  PaginationEmitData,
} from '../../../../../common/components/pagination/pagination';
import { AppConsole } from '../../../../../utils/app-console';
import { FormsModule } from '@angular/forms';

interface ParcelFilter {
  search: string;
  status: string;
}

@Component({
  selector: 'app-parcels-list',
  imports: [NgFor, AsyncPipe, CommonModule, Pagination, FormsModule],
  templateUrl: './parcels-list.html',
  styleUrl: './parcels-list.css',
})
export class ParcelsList implements OnInit {

  parcelList$ = new BehaviorSubject<ParcelResponseList | null>(null);
  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;
  filterState: ParcelFilter = {
    search: '',
    status: '',
  };
  private searchSubject = new Subject<string>();
  availableStatus = ["All", "AwaitingPickup", "Claimed"]
  selectedStatus: string = "All"

  constructor(private parcelService: ParcelsService) {}

  // Reads as:
  // “In this parent component, create a property called child of type ChildComponent.
  // Angular will automatically assign it with the first <app-child> found in the template
  // after the view is initialized.”
  // @ViewChild(Pagination) paginationChild !: Pagination

  ngOnInit(): void {
    this.parcelService
      .getAllParcels('', '', '', this.paginationCurrentPage, this.paginationPageSize)
      .subscribe((result) => {
        this.parcelList$?.next(result);
      });
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((searchKeyword) =>
          this.parcelService.getAllParcels(
            this.filterState.search,
            '',
            '',
            this.paginationCurrentPage,
            this.paginationPageSize
          )
        )
      )
      .subscribe((result) => {
        this.parcelList$?.next(result)
      });
  }

  onPaginationChanged(data: PaginationEmitData) {
    AppConsole.log(`PAGINATION: ReceivedPaginationData: ${JSON.stringify(data)}`);
    this.paginationCurrentPage = data.currentPage;
    this.paginationPageSize = data.pageSize;
    this.parcelService.getAllParcels(
      this.filterState.search,
      this.filterState.status,
      '',
      this.paginationCurrentPage,
      this.paginationPageSize
    ).subscribe((result) => {this.parcelList$.next(result)})
  }

  onSearch(value: string) {
    this.filterState.search = value
    this.searchSubject.next(this.filterState.search);
  }

  onStatusChanged(event: Event) {
    const status = event.target as HTMLSelectElement
    this.filterState.status = status.value.toString()
    this.parcelService.getAllParcels(
      this.filterState.search,
      this.filterState.status, 
      "", 
      this.paginationCurrentPage, 
      this.paginationPageSize
    ).subscribe(res => {
      this.parcelList$.next(res)
    })
  }
}
