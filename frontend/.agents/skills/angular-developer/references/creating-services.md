# Creating and Using Services

Services in Angular are reusable pieces of code that handle data fetching, business logic, or state management that multiple components or other services need to access.

## Creating a Service

You can generate a service using the Angular CLI:

```bash
ng generate service my-data
```

Or you can manually create a TypeScript class and decorate it with `@Injectable({ providedIn: 'root' })`. For reactive state management, store data in a private `signal()` and expose it publicly via `.asReadonly()`:

```ts
import {Injectable, signal} from '@angular/core';

@Injectable({ providedIn: 'root' })
export class BasicDataStore {
  private readonly dataSignal = signal<string[]>([]);

  // Expose state as a read-only signal to prevent direct external mutation
  readonly data = this.dataSignal.asReadonly();

  addData(item: string): void {
    this.dataSignal.update((items) => [...items, item]);
  }
}
```

### The `@Injectable` decorator

Using `@Injectable({ providedIn: 'root' })` is the recommended approach for most services. It tells Angular to:

- **Create a single instance (singleton)** for the entire application.
- **Make it available everywhere** automatically, without needing to list it in any `providers` array.
- **Enable tree-shaking**, meaning the service is only included in the final JavaScript bundle if it is actually injected somewhere.

#### Scoped service instances

For an instance scoped to a component, use `@Injectable()` and provide the service in the component's `providers` array.

## Injecting a Service

Once a service is created, you can inject it into components, directives, or other services using the `inject()` function.

### Injecting into a Component

```ts
import {Component, inject} from '@angular/core';
import {BasicDataStore} from './basic-data-store.service';

@Component({
  selector: 'app-example',
  template: `
    <div>
      <p>Data items: {{ dataStore.data().length }}</p>
      <button (click)="dataStore.addData('New Item')">Add Item</button>
    </div>
  `,
})
export class Example {
  // Inject the service as a class field
  readonly dataStore = inject(BasicDataStore);
}
```

### Injecting into Another Service

Services can inject other services in the exact same way. Use `computed()` to derive values from injected services reactively:

```ts
import {Injectable, computed, inject, signal} from '@angular/core';
import {AdvancedDataStore} from './advanced-data-store.service';

@Injectable({ providedIn: 'root' })
export class CombinedDataStore {
  // Injecting another service
  private readonly advancedDataStore = inject(AdvancedDataStore);
  private readonly dataSignal = signal<string[]>([]);

  // Combine reactive state from this service and the injected service
  readonly allData = computed(() => [...this.dataSignal(), ...this.advancedDataStore.data()]);
}
```

## Advanced Service Patterns

While `@Injectable({ providedIn: 'root' })` covers most scenarios, you may sometimes need:

- **Component-specific instances**: Use `@Injectable()` and provide the service directly in the component's `@Component({ providers: [MyService] })` array.
- **Factory providers**: For dynamic creation.
- **Value providers**: For injecting configuration objects.
