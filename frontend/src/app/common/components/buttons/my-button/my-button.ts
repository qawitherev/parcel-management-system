import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-my-button',
  imports: [],
  templateUrl: './my-button.html',
  styleUrl: './my-button.css'
})
export class MyButton {
  @Input() variant: 'primary' | 'secondary' | 'success' | 'danger' = 'primary'
  @Input() label: string = "Button"
  @Input() disabled: boolean = false
  @Input() type: 'button' | 'submit' | 'reset' = 'button'
  @Input() isLoading: boolean = false

  @Output() clicked = new EventEmitter<MouseEvent>

  onClick(event: MouseEvent) {
    if(!this.disabled) {
      this.clicked.emit(event);
    }
  }
}
