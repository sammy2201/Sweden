import { ApplicationConfig, provideZoneChangeDetection } from "@angular/core";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
import { provideRouter } from "@angular/router";
import { definePreset } from "@primeng/themes";
import Aura from "@primeng/themes/aura";
import { providePrimeNG } from "primeng/config";

import { routes } from "./app.routes";

const SwedenPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: "var(--app-blue-50)",
      100: "var(--app-blue-100)",
      200: "var(--app-blue-200)",
      300: "var(--app-blue-300)",
      400: "var(--app-blue-400)",
      500: "var(--app-blue-500)",
      600: "var(--app-primary)",
      700: "var(--app-primary-hover)",
      800: "var(--app-primary-dark)",
      900: "var(--app-blue-900)",
      950: "var(--app-blue-950)",
    },
    colorScheme: {
      light: {
        primary: {
          color: "var(--app-primary)",
          inverseColor: "var(--app-white)",
          hoverColor: "var(--app-primary-hover)",
          activeColor: "var(--app-primary-dark)",
        },
      },
    },
    focusRing: {
      width: "var(--border-width-strong)",
      style: "solid",
      color: "var(--app-focus-ring-color)",
      offset: "var(--app-focus-offset)",
    },
    formField: {
      borderRadius: "var(--radius-control)",
      focusRing: {
        width: "var(--border-width-strong)",
        style: "solid",
        color: "var(--app-focus-ring-color)",
        offset: "var(--app-focus-offset)",
      },
    },
  },
  components: {
    button: {
      root: {
        borderRadius: "var(--radius-control)",
        paddingX: "var(--space-4)",
        paddingY: "var(--space-2-5)",
        transitionDuration: "var(--app-motion-duration)",
        focusRing: {
          width: "var(--border-width-strong)",
          style: "solid",
          offset: "var(--app-focus-offset)",
        },
      },
    },
    inputtext: {
      root: {
        borderRadius: "var(--radius-control)",
        paddingX: "var(--space-4)",
        paddingY: "var(--space-2-5)",
        transitionDuration: "var(--app-motion-duration)",
      },
    },
    select: {
      root: {
        borderRadius: "var(--radius-control)",
        paddingX: "var(--space-4)",
        paddingY: "var(--space-2-5)",
        transitionDuration: "var(--app-motion-duration)",
      },
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: SwedenPreset,
        options: {
          darkModeSelector: false,
        },
      },
    }),
  ],
};
