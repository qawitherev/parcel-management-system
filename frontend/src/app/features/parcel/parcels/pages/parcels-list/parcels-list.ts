import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { ParcelResponseList, ParcelsService } from '../../parcels-service';
import { AsyncPipe, CommonModule, NgFor } from '@angular/common';
@Component({
  selector: 'app-parcels-list',
  imports: [NgFor, AsyncPipe, CommonModule],
  templateUrl: './parcels-list.html',
  styleUrl: './parcels-list.css'
})
export class ParcelsList implements OnInit {
  parcelList$?: Observable<ParcelResponseList>

  constructor(private parcelService: ParcelsService) {}

  ngOnInit(): void {
    this.parcelList$ = this.parcelService.getAllParcels()
  }
  
}
