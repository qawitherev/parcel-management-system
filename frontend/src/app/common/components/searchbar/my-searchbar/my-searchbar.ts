import { Component, EventEmitter, Input, Output } from '@angular/core';
import { debounce, debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'app-my-searchbar',
  imports: [],
  templateUrl: './my-searchbar.html',
  styleUrl: './my-searchbar.css'
})
export class MySearchbar {
  @Input() placeHolder: string = "Search..."
  @Input() delayTime: number = 300;

  @Output() onKeywordChange = new EventEmitter<string>();

  private searchSubject = new Subject<string>(); 

  constructor() {
    this.searchSubject.pipe(
      debounceTime(this.delayTime), 
      distinctUntilChanged()
    )
    .subscribe(keyword => this.onKeywordChange.emit(keyword));
  }

  onChange(data: string) {
    this.searchSubject.next(data);
  }

}
