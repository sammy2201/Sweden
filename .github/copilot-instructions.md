# Copilot Instructions

These instructions apply to the entire Sweden Start repository.

## Project Shape

- The backend is an ASP.NET Core Web API targeting .NET 10 in `backend/`.
- The frontend is an Angular 20 application in `frontend/`, using TypeScript, PrimeNG, RxJS, and Angular signals.
- Local PostgreSQL runs natively through the Homebrew service. The backend and frontend run on the host during local development; `make dev` starts the database and both apps.
- The frontend is also packaged into the backend's static web root by the root `Dockerfile` for the production container.
- Backend code is organized around controllers, services, repositories, DTOs, models, and EF Core data access. Follow the nearest existing implementation when adding features.

## Working With This Repository

- Before editing, inspect the relevant implementation, caller, and nearby tests or configuration. State the local behavior you are changing and make the smallest coherent change.
- Preserve existing user changes. Do not revert, reformat, or edit unrelated files just to make a patch cleaner.
- Do not make speculative product or provider choices. Ask when the request leaves a consequential behavior, external integration, or security policy unclear.
- Do not add dependencies or broad abstractions when the existing platform or local pattern already solves the problem.
- Keep changes focused. Avoid unrelated refactors, generated output, and formatting churn.
- Never commit secrets, tokens, passwords, private keys, or local machine configuration. Do not print secret values in command output or responses.
- Do not run destructive database or Git commands without explicit user approval. Never delete or reset a PostgreSQL data directory or database volume as part of routine development.
- Do not claim tests passed unless they were actually run. Report unavailable test coverage and relevant existing warnings clearly.

## Backend Conventions

- Keep controllers focused on HTTP binding, authorization, status codes, and response mapping. Put business rules in services and persistence operations in repositories or the established EF Core data layer.
- Use DTOs at API boundaries; do not expose EF entities directly. Validate incoming request DTOs using the existing validation conventions.
- Use asynchronous APIs for database and network I/O, propagate cancellation tokens where the surrounding API supports them, and avoid blocking calls.
- Preserve established route, status-code, error-handling, authentication, and CORS patterns. Add the HTTP method to the configured CORS policy when introducing a new API method.
- Use the correct HTTP status code for each outcome (for example, `400` for invalid input, `401` for unauthenticated requests, `403` for forbidden access, `404` for missing resources, `409` for conflicts, `502` for an unavailable upstream service, and `500` for unexpected server errors). Do not return `200 OK` for failures.
- Format API failures as ASP.NET Core `ProblemDetails` / `ValidationProblemDetails` (`application/problem+json`). Preserve useful fields such as `status`, `title`, `detail`, validation `errors`, and stable machine-readable extension codes where clients need them. Do not return ad hoc `{ message: ... }` error objects or expose internal exception details for unexpected `500` errors.
- Keep successful response bodies as their existing typed response DTOs. Do not add a generic `{ message, data }` envelope; only introduce or change a response envelope when explicitly requested and coordinated across backend DTOs, OpenAPI metadata, and frontend types.
- Document non-success response types in OpenAPI using `ProblemDetails` or `ValidationProblemDetails` rather than `object` or anonymous response types.
- Add EF Core migrations only when the persisted schema changes. Do not create migrations for DTO-only or API-only changes.
- Read database and JWT configuration through ASP.NET Core configuration. Local secrets belong in ignored local configuration or environment variables, not committed files. The JWT key is supplied as `Jwt__Key` and must meet the application's minimum key-length requirement.

## Frontend Conventions

- Use the repository's existing HTTP client (Axios) through an injectable feature service for backend communication. Keep API calls out of templates and avoid duplicating calls already owned by a service.
- Use the configured API base URL and keep request and response types aligned with backend DTOs.
- Preserve the existing Axios configuration, including authentication headers, interceptors, and error-handling behavior. Do not migrate to Angular `HttpClient` unless explicitly requested.
- Use the configured API base URL from the environment files. Keep request and response types aligned with backend DTOs.
- Keep `HttpClient` response types aligned with the backend's direct success DTOs; do not expect a `{ message, data }` wrapper unless that API explicitly defines one.
- Route HTTP failures through the shared `getApiErrorMessage` utility in `frontend/src/app/utils/api-error.ts`. It understands Problem Details and validation errors and provides status-aware fallbacks. Do not duplicate backend error parsing or cast response errors to `any`; receive errors as `unknown` and narrow them safely.
- Present actionable request failures in the relevant UI instead of handling expected API errors only with `console.error`. Keep intentional fallback behavior (such as the roadmap's expected `404` when a user has not created one yet) intact.
- Follow the existing standalone Angular component, dependency-injection, signal, and RxJS patterns. Clean up long-lived subscriptions with `takeUntilDestroyed` where appropriate.
- Preserve the existing PrimeNG components and theme setup unless the task explicitly changes the design system.
- Put reusable visual values in CSS custom properties in `frontend/src/styles.css`. Component styles should reference those tokens for shared colors, spacing, typography, control sizes, borders, radii, and effects. Keep genuinely component-specific geometry local and preserve responsive behavior.
- Do not add visual instructions or helper copy to the UI unless the requested user workflow needs it.

## Local Commands And Verification

Run commands from the repository root unless a command specifies a working directory.

- `make build` builds the backend and frontend.
- `make backend_build` builds the .NET API.
- `make frontend_build` builds the Angular production app.
- `make dev` starts PostgreSQL and runs the backend and frontend locally.
- `make db_status` checks the Homebrew PostgreSQL 18 service.
- `make db_shell` opens a `psql` session against the development database on port 5434.
- Run the narrowest relevant build or test after editing, then broader checks when the change affects shared behavior. The frontend currently defines a production `build` script; check package scripts before assuming a test command exists.
- The backend build may report a known `NU1903` advisory for `Microsoft.OpenApi`. Do not perform a breaking package upgrade as part of unrelated work.

## Response Expectations

- Summarize the behavior changed and identify the important files.
- State the exact validation commands run and whether they passed.
- Call out material tradeoffs, configuration steps, or unverified behavior; keep the report concise and factual.
