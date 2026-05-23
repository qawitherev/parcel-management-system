import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  template: `<div class="skeleton" [style.width]="width" [style.height]="height"></div>`,
  styles: [`
    .skeleton {
      background: var(--bdr);
      animation: skeletonPulse 1.5s ease-in-out infinite;
    }
    @keyframes skeletonPulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.4; }
    }
  `]
})
export class SkeletonComponent {
  @Input() width = '100%';
  @Input() height = '16px';
}
