import { NgClass } from '@angular/common';
import { Component, EventEmitter, Input, input, Output } from '@angular/core';

@Component({
  selector: 'app-my-button',
  imports: [NgClass],
  templateUrl: './my-button.html',
  styleUrl: './my-button.css'
})
export class MyButton {
  /**
   * base 
   * variant 
   * disable 
   * 
   */
  @Input() variant: 'primary' | 'secondary' | 'success' | 'danger' = 'primary'
  @Input() label: string = "Button"
  @Input() disabled: boolean = false

  @Output() clicked = new EventEmitter<MouseEvent>

  get buttonClasses() : string {
    const base = "px-4 py-2 rounded-lg transition-all duration-200 text-[var(--clr-dark-a0)]"
    const variants : any = {
      primary : 'bg-[var(--clr-primary-a20)] hover:bg-[var(--clr-primary-a10)]', 
      secondary: 'bg-[var(--clr-surface-a20)] hover:bg-[var(--clr-surface-a10)]',
      success: 'bg-[var(--clr-success-a20)] hover:bg-[var(--clr-success-a10)]',
      danger: 'bg-[var(--clr-danger-a20)] hover:bg-[var(--clr-danger-a10)]'
    }
    const disabled = this.disabled ? 'opacity-50 cursor-not-allowed' : '';
    return `${base} ${variants[this.variant]} ${disabled}`
  }

  onClick(event: MouseEvent) {
    if(!this.disabled) {
      this.clicked.emit(event);
    }
  }
  
}
