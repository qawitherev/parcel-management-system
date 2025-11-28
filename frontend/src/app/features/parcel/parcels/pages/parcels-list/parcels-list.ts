import { Component, OnInit } from '@angular/core';
import {
  BehaviorSubject,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  Subject,
  switchMap,
  map,
  tap,
} from 'rxjs';
import { ParcelResponse, ParcelResponseList, ParcelsService } from '../../parcels-service';
import { AsyncPipe, CommonModule, NgFor } from '@angular/common';
import { PaginationEmitData } from '../../../../../common/components/pagination/pagination';
import { FormsModule } from '@angular/forms';
import { ListingQueryParams } from '../../../../../common/models/listing-query-params';
import { MyTable, TableColumn } from '../../../../../common/components/table/my-table/my-table';
import { MySearchbar } from "../../../../../common/components/searchbar/my-searchbar/my-searchbar";

@Component({
  selector: 'app-parcels-list',
  imports: [NgFor, AsyncPipe, CommonModule, FormsModule, MyTable, MySearchbar],
  templateUrl: './parcels-list.html',
  styleUrl: './parcels-list.css',
})
export class ParcelsList implements OnInit {
  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  tableColumns: TableColumn<ParcelResponse>[] = [
    { key: 'trackingNumber', label: 'Tracking Number', isActionColumn: false },
    { key: 'locker', label: 'Locker', isActionColumn: false },
    { key: 'weight', label: 'Weight', isActionColumn: false },
    { key: 'dimensions', label: 'Dimension', isActionColumn: false },
    { key: 'residentUnit', label: 'Resident Unit', isActionColumn: false },
    { key: 'status', label: 'Status', isActionColumn: false },
  ];

  searchKeyword = new BehaviorSubject<string>('');
  statusStream = new BehaviorSubject<string>('All');
  paginationParams = new BehaviorSubject<Omit<ListingQueryParams, 'searchKeyword'>>({
    page: this.paginationCurrentPage,
    take: this.paginationPageSize,
  });

  parcelList$ = combineLatest([
    this.searchKeyword.pipe(),
    this.statusStream.pipe(distinctUntilChanged()),
    this.paginationParams.pipe(distinctUntilChanged()),
  ]).pipe(
    map(([searchKeyword, status, pagination]) => ({
      searchKeyword,
      status,
      ...pagination,
    })),
    switchMap((params) => this.parcelService.getAllParcels(params))
  );

  availableStatus = ['All', 'AwaitingPickup', 'Claimed'];
  selectedStatus: string = 'All';

  constructor(private parcelService: ParcelsService) { }

  ngOnInit(): void {
    // do nothing
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.paginationCurrentPage = data.currentPage;
    this.paginationPageSize = data.pageSize;
    this.paginationParams.next({
      page: data.currentPage,
      take: data.pageSize,
    });
  }

  onSearch(searchKeyword: string) {
    this.searchKeyword.next(searchKeyword);
  }

  onStatusChanged(event: Event) {
    const status = event.target as HTMLSelectElement;
    this.statusStream.next(status.value.toString());
  }
}
