import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ThemeService } from '../../../../core/theme/theme-service';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-my-switch',
  imports: [NgClass],
  templateUrl: './my-switch.html',
  styleUrl: './my-switch.css'
})
export class MySwitch {
  @Input() labelPositive: string = "Positive"
  @Input() labelNegative: string = "Negative"
  @Input() isPositive: boolean = true;

  @Output() toggled = new EventEmitter<void>();

  onSwitchToggled() {
    this.isPositive = !this.isPositive
    this.toggled.emit();
  }

}
