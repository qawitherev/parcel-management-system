import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-success-card',
  standalone: true,
  template: `
    @if (visible) {
      <div class="success-card">
        {{ message }}
      </div>
    }
  `,
  styles: [`
    .success-card {
      background: #f0fdf4;
      border: 1px solid #22c55e;
      padding: 14px 18px;
      margin-top: 12px;
      font-size: 13px;
      color: #166534;
    }
  `]
})
export class SuccessCardComponent implements OnInit {
  @Input() message = 'Operation completed successfully.';
  @Input() duration = 4000;

  visible = true;

  ngOnInit(): void {
    if (this.duration > 0) {
      setTimeout(() => { this.visible = false; }, this.duration);
    }
  }
}
