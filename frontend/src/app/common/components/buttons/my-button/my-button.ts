import { NgClass } from '@angular/common';
import { Component, EventEmitter, Input, input, Output } from '@angular/core';

@Component({
  selector: 'app-my-button',
  imports: [NgClass],
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

  get buttonClasses() : string {
    const base = " text-sm px-4 py-2 rounded-lg transition-all duration-200 text-[var(--clr-surface-a0)] w-full"
    const variants : any = {
      primary : 'bg-[var(--clr-primary-a20)] hover:bg-[var(--clr-primary-a0)]', 
      secondary: 'bg-[var(--clr-surface-a20)] hover:bg-[var(--clr-surface-a10)]',
      success: 'bg-[var(--clr-success-a20)] hover:bg-[var(--clr-success-a10)]',
      danger: 'bg-[var(--clr-danger-a0)] hover:bg-[var(--clr-danger-a10)]'
    }
    const disabled = this.disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer';
    return `${base} ${variants[this.variant]} ${disabled}`
  }

  onClick(event: MouseEvent) {
    if(!this.disabled) {
      this.clicked.emit(event);
    }
  }
  
}
