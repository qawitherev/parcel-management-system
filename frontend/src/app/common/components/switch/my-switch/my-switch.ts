import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ThemeService } from '../../../../core/theme/theme-service';
import { NgClass } from '@angular/common';
import { AppConsole } from '../../../../utils/app-console';

@Component({
  selector: 'app-my-switch',
  imports: [NgClass],
  templateUrl: './my-switch.html',
  styleUrl: './my-switch.css'
})
export class MySwitch {
  @Input() labelOn: string = "On"
  @Input() labelOff: string = "Off"
  @Input() isPositive: boolean = true;
  @Input() willShowLabel: boolean = true;

  @Output() toggled = new EventEmitter<boolean>();

  onSwitchToggled(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    this.isPositive = checked;
    this.toggled.emit();
  }

}
