import { Component, OnInit} from '@angular/core';
import {
  BehaviorSubject,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  Subject,
  switchMap,
  map
} from 'rxjs';
import { ParcelResponseList, ParcelsService } from '../../parcels-service';
import { AsyncPipe, CommonModule, NgFor } from '@angular/common';
import {
  Pagination,
  PaginationEmitData,
} from '../../../../../common/components/pagination/pagination';
import { AppConsole } from '../../../../../utils/app-console';
import { FormsModule } from '@angular/forms';
import { ListingQueryParams } from '../../../../../common/models/listing-query-params';

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

  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  searchKeyword = new BehaviorSubject<string>('')
  statusStream = new BehaviorSubject<string>('All')
  paginationParams = new BehaviorSubject<Omit<ListingQueryParams, 'searchKeyword'>>({
    page: this.paginationCurrentPage, 
    take: this.paginationPageSize
  })

  parcelList$ = combineLatest([
    this.searchKeyword.pipe(debounceTime(300), distinctUntilChanged()),
    this.statusStream.pipe(distinctUntilChanged()),
    this.paginationParams.pipe(distinctUntilChanged())
  ]).pipe(
    map(([searchKeyword, status, pagination]) => ({
      searchKeyword, 
      status,
      ...pagination
    })), 
    switchMap(params => this.parcelService.getAllParcels(params))
  )

  private searchSubject = new Subject<string>();
  availableStatus = ["All", "AwaitingPickup", "Claimed"]
  selectedStatus: string = "All"

  constructor(private parcelService: ParcelsService) {}


  ngOnInit(): void {
    // do nothing 
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.paginationCurrentPage = data.currentPage
    this.paginationPageSize = data.pageSize
    this.paginationParams.next({
      page: data.currentPage, 
      take: data.pageSize
    })
  }

  onSearch(value: string) {
    this.searchKeyword.next(value)
  }

  onStatusChanged(event: Event) {
    const status = event.target as HTMLSelectElement
    this.statusStream.next(status.value.toString())
  }
}
