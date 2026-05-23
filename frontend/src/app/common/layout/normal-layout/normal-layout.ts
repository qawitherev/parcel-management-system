import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from '../../components/navbar/navbar';

@Component({
  selector: 'app-normal-layout',
  standalone: true,
  imports: [RouterOutlet, Navbar],
  templateUrl: './normal-layout.html',
  styleUrls: ['./normal-layout.css']
})
export class NormalLayout implements OnInit {
  ngOnInit(): void {}
}
