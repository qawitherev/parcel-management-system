import { Component, EventEmitter, Output } from '@angular/core';
import { AppConsole } from '../../../utils/app-console';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css'
})
export class Pagination {
  @Output() dataEmitter = new EventEmitter<string>()

  sendData() {
    this.dataEmitter.emit("hello from pagination")
  }
}
