# Testing Fundamentals

This guide covers the fundamental principles and practices for writing unit tests in this repository, which uses Vitest as the test runner.

## Core Philosophy: Zone-Based & Async-Aware

This repository uses zone-based change detection with `zone.js`. Use `fixture.detectChanges()` to render the initial bindings and apply synchronous state changes, and `fixture.whenStable()` when waiting for asynchronous work.

**Do NOT** use `fixture.changeDetectorRef.markForCheck()` either: it marks only the host view and does not refresh the template of `OnPush` components (the default). Update state through signals, inputs, or DOM events instead.
Use the "Act, Render/Wait, Assert" pattern:

1.  **Act:** Update state or perform an action (e.g., set a component input, click a button).
2.  **Render/Wait:** Call `fixture.detectChanges()` to render synchronous changes, or use `await fixture.whenStable()` for asynchronous work.
3.  **Assert:** Verify the outcome.

### Basic Test Structure Example

```ts
import {ComponentFixture, TestBed} from '@angular/core/testing';
import {MyComponent} from './my.component';

describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let h1: HTMLElement;

  beforeEach(() => {
    TestBed.configureTestingModule({});

    // Create the component fixture
    fixture = TestBed.createComponent(MyComponent);
    fixture.detectChanges();
    component = fixture.componentInstance;
    h1 = fixture.nativeElement.querySelector('h1');
  });

  it('should display the default title', async () => {
    // ACT: (Implicit) Component is created with default state.
    // The initial bindings were rendered in beforeEach.
    await fixture.whenStable();
    // ASSERT the initial state.
    expect(h1.textContent).toContain('Default Title');
  });

  it('should display a different title after a change', async () => {
    // ACT: Change the component's title property.
    component.title.set('New Test Title');

    // Render the synchronous signal update.
    fixture.detectChanges();

    // ASSERT the DOM has been updated.
    expect(h1.textContent).toContain('New Test Title');
  });
});
```

## TestBed and ComponentFixture

- **`TestBed`**: The primary utility for creating a test-specific Angular module. Use `TestBed.configureTestingModule({...})` in your `beforeEach` to declare components, provide services, and set up imports needed for your test.
- **`ComponentFixture`**: A handle on the created component instance and its environment.
  - `fixture.componentInstance`: Access the component's class instance.
  - `fixture.nativeElement`: Access the component's root DOM element.
  - `fixture.debugElement`: An Angular-specific wrapper around the `nativeElement` that provides safer, platform-agnostic ways to query the DOM (e.g., `debugElement.query(By.css('p'))`).
