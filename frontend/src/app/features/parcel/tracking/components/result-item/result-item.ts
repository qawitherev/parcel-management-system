import { Component, Input, OnInit } from '@angular/core';
import { ParcelHistoryItem } from '../../pages/search-result/search-result';
import { getRelativeTime } from '../../../../../utils/date-time-utils';

@Component({
  selector: 'app-result-item',
  imports: [],
  templateUrl: './result-item.html',
  styleUrl: './result-item.css'
})
export class ResultItem implements OnInit {
  formattedTime?: string

  @Input() parcelHistoryItem!: ParcelHistoryItem

  ngOnInit(): void {
    this.formattedTime = getRelativeTime(this.parcelHistoryItem.eventTime.toString())
  }
}
