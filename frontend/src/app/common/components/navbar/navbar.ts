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
  selector: 'app-navbar',
  imports: [],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {

}
