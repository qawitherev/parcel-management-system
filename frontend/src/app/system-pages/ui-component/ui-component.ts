import { Component } from '@angular/core';
import { MyButton } from "../../common/components/buttons/my-button/my-button";
import { AppConsole } from '../../utils/app-console';

@Component({
  selector: 'app-ui-component',
  imports: [MyButton],
  templateUrl: './ui-component.html',
  styleUrl: './ui-component.css'
})
export class UiComponent {

  onClick() {
    AppConsole.log("TEST: hello")
  }
}
