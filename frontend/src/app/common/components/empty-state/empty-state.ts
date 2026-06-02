import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty-state">
      <div class="empty-title">{{ title }}</div>
      <div class="empty-desc">{{ description }}</div>
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 48px 24px;
      color: var(--t3);
    }
    .empty-title {
      font-size: 15px;
      font-weight: 600;
      margin-bottom: 6px;
      color: var(--t2);
    }
    .empty-desc {
      font-size: 13px;
    }
  `]
})
export class EmptyStateComponent {
  @Input() title = 'No results found';
  @Input() description = 'There are no items to display.';
}
