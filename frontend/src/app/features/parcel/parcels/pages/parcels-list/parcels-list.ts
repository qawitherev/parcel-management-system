import { AfterViewInit, Component, OnInit, ViewChild, viewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { ParcelResponseList, ParcelsService } from '../../parcels-service';
import { AsyncPipe, CommonModule, NgFor } from '@angular/common';
import { Pagination, PaginationEmitData } from "../../../../../common/components/pagination/pagination";
import { AppConsole } from '../../../../../utils/app-console';

@Component({
  selector: 'app-parcels-list',
  imports: [NgFor, AsyncPipe, CommonModule, Pagination],
  templateUrl: './parcels-list.html',
  styleUrl: './parcels-list.css'
})
export class ParcelsList implements OnInit {
  parcelList$?: Observable<ParcelResponseList>
  paginationCurrentPage: number = 1

  constructor(private parcelService: ParcelsService) {}

  // Reads as:
  // “In this parent component, create a property called child of type ChildComponent. 
  // Angular will automatically assign it with the first <app-child> found in the template 
  // after the view is initialized.”
  // @ViewChild(Pagination) paginationChild !: Pagination

    ngOnInit(): void {
    this.parcelList$ = this.parcelService.getAllParcels(
          "", 
          "", 
          "", 
          1, 
          10
        )
  }

  onPaginationChanged(data: PaginationEmitData) {
    AppConsole.log(`PAGINATION: ReceivedPaginationData: ${JSON.stringify(data)}`)
    this.paginationCurrentPage = data.currentPage
    this.parcelList$ = this.parcelService.getAllParcels(
      "", 
      "", 
      "", 
      this.paginationCurrentPage, 
      data.pageSize
    )
  }
}
