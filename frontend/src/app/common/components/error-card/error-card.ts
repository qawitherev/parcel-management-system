import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-error-card',
  standalone: true,
  template: `
    <div class="err-card">
      <div>
        <strong>{{ title }}</strong>
        @if (description) {
          <div style="margin-top: 4px;">{{ description }}</div>
        }
      </div>
      @if (retryLabel) {
        <button class="btn btn-s" (click)="retry.emit()">{{ retryLabel }}</button>
      }
    </div>
  `,
  styles: [`
    .err-card {
      background: #fef2f2;
      border: 1px solid var(--red);
      padding: 14px 18px;
      margin-bottom: 16px;
      font-size: 13px;
      color: var(--red);
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
    }
    .err-card strong { font-weight: 700; }
    .btn {
      padding: 8px 16px;
      border: 2px solid var(--red);
      font-size: 11px;
      font-weight: 600;
      cursor: pointer;
      font-family: var(--f);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      background: var(--red);
      color: #fff;
      white-space: nowrap;
      transition: all 0.15s;
    }
    .btn:hover {
      background: var(--red-d);
      border-color: var(--red-d);
    }
    .btn-s { padding: 6px 14px; font-size: 10px; }
  `]
})
export class ErrorCardComponent {
  @Input() title = 'Something went wrong';
  @Input() description = '';
  @Input() retryLabel = '';
  @Output() retry = new EventEmitter<void>();
}
