# SwedenStart

SwedenStart uses Angular, ASP.NET Core, EF Core, and PostgreSQL. Local development uses the Homebrew PostgreSQL service.

## Native PostgreSQL on macOS

The local PostgreSQL 18.4 installation is used for this project. In this environment ports 5432 and 5433 were already occupied, so SwedenStart's native cluster listens only on `localhost:5434`. It uses the existing Homebrew data directory at `/opt/homebrew/var/postgresql@18`; the API uses the `ConnectionStrings:DefaultConnection` setting and no database connection is hardcoded in application code. Production can override it with `ConnectionStrings__DefaultConnection` from the environment or a secrets manager, including an AWS RDS endpoint.

Create a private local configuration file and set the database password, JWT signing key, and ResRobot API key:

```sh
cp .env.example .env
openssl rand -hex 32
chmod 600 .env
```

Replace `replace-with-a-private-password` with a generated password, `Jwt__Key` with a random key of at least 32 characters, and `ResRobot__ApiKey` with your ResRobot key. The double underscores map environment variables to nested .NET settings (`Jwt:Key` and `ResRobot:ApiKey`). `.env` is ignored by Git, and the Makefile loads and exports it for backend commands. The `psql` command started by `make db_shell` may prompt for the database password. `backend/appsettings.json` contains tracked, non-secret shared defaults; `backend/appsettings.example.json` mirrors those defaults. The ignored `backend/appsettings.Development.json` is only for development-specific overrides.

The native PostgreSQL server listens on loopback only. Its host authentication uses SCRAM password authentication; local administrative access uses the macOS account's PostgreSQL socket connection. The app uses the dedicated `sweden_start` role and database.

`make db_up`, `make db_down`, and `make db_restart` use `brew services` to manage the Homebrew `postgresql@18` service. PostgreSQL continues to use the existing Homebrew data directory and port `5434`.

Useful commands:

```sh
make db_up               # start PostgreSQL 18 through Homebrew services
make db_status           # check Homebrew service status
make db_shell            # connect with psql
make db_migrate          # apply pending EF Core migrations
make db_down             # stop PostgreSQL; data remains on disk
make dev                 # PostgreSQL + backend + Angular
make watch               # PostgreSQL + backend watch + Angular
```

The Makefile requires and exports `.env` for backend runtime, database shell, and migration commands. `make backend_build` does not require runtime secrets. You can also export `ConnectionStrings__DefaultConnection` directly for a manual `dotnet run` or `dotnet watch` command.

The production `Dockerfile` is separate from local database management. Any previously created Docker PostgreSQL container or volume is left untouched; it is no longer managed by this repository.

## EF Core migrations

The backend reads `ConnectionStrings:DefaultConnection`. For development, `.env` supplies `ConnectionStrings__DefaultConnection`; production should supply the same setting through its deployment environment or secrets manager. To inspect migrations and update the configured database:

```sh
cd backend
set -a; source ../.env; set +a
dotnet ef migrations list
cd ..
make db_migrate
```

Starting the API does not automatically modify the schema. Apply migrations explicitly after reviewing them.

## Production database

No AWS resources are provisioned by this repository. To use RDS later, set `ConnectionStrings__DefaultConnection` to the RDS connection string using an environment variable or secrets manager. The backend continues to use EF Core and Npgsql; no code changes are needed for the provider switch.

## Visit Sweden attraction catalogue

The Visit Sweden importer feeds the Explore feature. Its data path is:

1. **Fetch:** `VisitSwedenService` requests the public Visit Sweden search endpoint in pages of 100 records. It follows the source's reported result count and treats incomplete pages, invalid JSON, timeouts, and unsuccessful HTTP responses as sync failures.
2. **Map and validate:** Each source record is read as JSON-LD. The importer maps its stable context/entry IDs, name, descriptions, website, category, image, address, city, and coordinates. County is resolved first from `dcterms:spatial`; if that is missing, the importer uses the record's coordinates and the bundled county-boundary GeoJSON. County names and slugs are normalized to IDs such as `kalmar` and `uppsala`. A record is skipped if it cannot be mapped to a county, has no image, or has missing/invalid coordinates.
3. **Upsert:** Valid attractions are saved in PostgreSQL's `Attractions` table. The combined Visit Sweden context and entry ID is the external key: a new key inserts a row, while a key already in the table updates that row. Re-running a sync does not create duplicates, and records absent from a later source response are not automatically deleted.
4. **Explore:** The county list comes from the app's static county catalogue. The attractions endpoint filters stored rows by the selected county's canonical ID and supported aliases, then returns a paged response. Explore does not fetch Visit Sweden directly in a user request; it reads the local PostgreSQL catalogue.

### When synchronization runs

The startup background service waits briefly after API startup, then checks whether `Attractions` contains any rows. If the table is empty, it runs a sync and retries up to three times on failure. If even one attraction already exists, startup sync is skipped; it is not a scheduled refresh and does not fill gaps in a partially populated catalogue. A manual sync can be used to refresh/update records and add newly available rows.

To refresh the catalogue manually, an authenticated account with the Admin role can send `POST /api/visitsweden/sync`. Syncs are serialized so startup and manual requests do not run concurrently. The result and logs report source records fetched, records mapped, rows inserted/updated, and records skipped for missing image, coordinates, or county mapping. Sync failures are logged and returned as API errors. The source base URL is configured with `VisitSweden:BaseUrl` (default: `https://data.visitsweden.com/store/`, overridable with `VisitSweden__BaseUrl`). No Visit Sweden API key is required for the public search endpoint.

If a county's Explore page is empty, first check its attraction count in the configured PostgreSQL database. If matching rows exist, check the API's county filter and the county ID stored on those rows. If the table is empty, check startup or manual sync logs for source or mapping failures. The `Attractions` count can differ from the number of source records because records without usable media or coordinates are intentionally skipped.
