import { Component } from '@angular/core';

interface Subfeature {
  name: string, 
  roles: string[], 
  isActive: boolean
}

interface Feature {
  subfeatures: Subfeature[]
}

@Component({
  selector: 'app-sidebar',
  imports: [],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {

}
