# Vercel Deployment

The `main` GitHub Actions workflow deploys the application as one Vercel Container Runtime service. Vercel builds `Dockerfile.vercel`, which the workflow creates from the repository's existing multi-stage `Dockerfile`. Angular is built once inside that Docker build and copied into the ASP.NET Core publish output. The workflow waits for deployment and verifies `/api/health`, including Supabase connectivity.

## One-time Vercel setup

1. Create a Vercel project for this repository and note its **Team ID** (or account org ID) and **Project ID**.
2. Add GitHub Actions repository variables `VERCEL_ORG_ID` and `VERCEL_PROJECT_ID` with those IDs.
3. Create a Vercel token with deployment access to the project and save it as the GitHub Actions repository secret `VERCEL_TOKEN`.
4. Set these Vercel **Production** environment variables:
   - `PORT`: `8080`. Vercel routes container requests to port 80 by default; this must match the image's `ASPNETCORE_HTTP_PORTS=8080`.
   - `ConnectionStrings__DefaultConnection`: the Supabase PostgreSQL connection string, including SSL settings required by the selected Supabase endpoint. Prefer Supabase's pooled connection endpoint for serverless workloads.
   - `Jwt__Key`: a randomly generated secret of at least 32 bytes. Do not commit it or put it in GitHub workflow YAML.
5. Ensure the production deployment URL permits unauthenticated `GET /api/health` so the workflow can verify it. The endpoint reports database health in its JSON body; the workflow requires both application and database status to be `Healthy`.
6. Disable Vercel's automatic Git production deployments if the repository is connected there. GitHub Actions is the single production deployment trigger; this avoids duplicate builds and deployments.
7. Push to `main`. The workflow's successful health check is the deployment verification. Configure a Vercel production domain separately if one is needed.

The Vercel project should use the repository root as its project root so `vercel.json`, `Dockerfile`, and backend/frontend sources are available to the service build.

## Runtime considerations

Vercel Container Runtime runs the .NET 10 image as a request-driven Function on Fluid compute. It supports the container's HTTP server and routes all paths to ASP.NET Core, where existing `/api/*` routes and Angular's `index.html` fallback remain in effect. Container Runtime and Services are currently Beta and inherit Vercel Function limits and pricing. Functions scale down when idle; they are not continuously running servers. On Hobby, requests have a maximum 300-second duration; paid plans allow longer limits subject to their configured maximums. Function request/response payloads are limited to 4.5 MB.

The existing `VisitSwedenStartupSyncService` is a best-effort background startup task, not a durable worker. Vercel can scale an idle instance down, so do not rely on that task to complete initialization or run continuously. Prepopulate Supabase if that data must be ready before serving traffic, or trigger and verify the existing authorized sync endpoint after deployment.

The container filesystem is ephemeral. Persistent application data must remain in Supabase; do not store uploads or durable state in the image/container filesystem.