import { Component } from '@angular/core';

@Component({
  selector: 'ui-card',
  standalone: true,
  template: `
    <div class="rounded-lg border bg-card text-card-foreground shadow-sm">
      <ng-content></ng-content>
    </div>
  `,
})
export class CardComponent {}

@Component({
  selector: 'ui-card-header',
  standalone: true,
  template: `
    <div class="flex flex-col space-y-1.5 p-6">
      <ng-content></ng-content>
    </div>
  `,
})
export class CardHeaderComponent {}

@Component({
  selector: 'ui-card-title',
  standalone: true,
  template: `
    <h3 class="text-2xl font-semibold leading-none tracking-tight">
      <ng-content></ng-content>
    </h3>
  `,
})
export class CardTitleComponent {}

@Component({
  selector: 'ui-card-description',
  standalone: true,
  template: `
    <p class="text-sm text-muted-foreground">
      <ng-content></ng-content>
    </p>
  `,
})
export class CardDescriptionComponent {}

@Component({
  selector: 'ui-card-content',
  standalone: true,
  template: `
    <div class="p-6 pt-0">
      <ng-content></ng-content>
    </div>
  `,
})
export class CardContentComponent {}

@Component({
  selector: 'ui-card-footer',
  standalone: true,
  template: `
    <div class="flex items-center p-6 pt-0">
      <ng-content></ng-content>
    </div>
  `,
})
export class CardFooterComponent {}
