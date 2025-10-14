import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

type AlertVariant = 'default' | 'destructive';

@Component({
  selector: 'ui-alert',
  standalone: true,
  imports: [NgClass],
  template: `
    <div
      [ngClass]="alertClasses"
      class="relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-foreground"
      role="alert"
    >
      <ng-content></ng-content>
    </div>
  `,
})
export class AlertComponent {
  @Input() variant: AlertVariant = 'default';

  get alertClasses(): string {
    const variantClasses = {
      default: 'bg-background text-foreground',
      destructive:
        'border-destructive/50 text-destructive dark:border-destructive [&>svg]:text-destructive',
    };

    return variantClasses[this.variant];
  }
}

@Component({
  selector: 'ui-alert-title',
  standalone: true,
  template: `
    <h5 class="mb-1 font-medium leading-none tracking-tight">
      <ng-content></ng-content>
    </h5>
  `,
})
export class AlertTitleComponent {}

@Component({
  selector: 'ui-alert-description',
  standalone: true,
  template: `
    <div class="text-sm [&_p]:leading-relaxed">
      <ng-content></ng-content>
    </div>
  `,
})
export class AlertDescriptionComponent {}
