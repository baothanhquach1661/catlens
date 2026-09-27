# CatLens

CatLens is an ASP.NET Core MVC application for reviewing Shopify
catalog and inventory data.

It imports a Shopify inventory CSV into PostgreSQL, identifies variants
that may need manual verification, and exports review reports as CSV
files.

CatLens uses a read-only review workflow. It does not modify products
or inventory in Shopify.

## V1 Features

- Import a Shopify inventory CSV.
- Store the current catalog snapshot in PostgreSQL.
- Browse imported product variants and inventory by location.
- Find variants with missing SKUs.
- Find SKUs assigned to multiple variants.
- Flag unusually high inventory using a configurable threshold.
- Export each review result as a UTF-8 CSV file.
- Preserve the existing snapshot if an import fails.

High inventory is only a warning for human verification. It does not
mean the Shopify inventory value is incorrect.

## Technology Stack

- ASP.NET Core MVC
- C#
- .NET 10
- Entity Framework Core
- PostgreSQL 17
- Docker Desktop
- CsvHelper
- Bootstrap

The ASP.NET Core application runs directly on the development computer.
Only PostgreSQL runs inside Docker.

## Project Structure

```text
CatLens/
├── CatLens.slnx
├── README.md
└── src/
    └── CatLens.Web/
        ├── Configuration/
        ├── Controllers/
        ├── Data/
        ├── Models/
        ├── Services/
        ├── Views/
        ├── wwwroot/
        ├── Program.cs
        └── CatLens.Web.csproj
```

## Prerequisites

Install the following software:

- .NET 10 SDK
- Docker Desktop
- Git
- Visual Studio Code or another C# editor

Confirm the installations:

```bash
dotnet --version
docker --version
git --version
```

## PostgreSQL Setup

### Existing development environment

Start the existing PostgreSQL container:

```bash
docker start atn-postgres
```

Check its status:

```bash
docker ps --filter name=atn-postgres
```

Verify that PostgreSQL is accepting connections:

```bash
docker exec atn-postgres \
  pg_isready \
  -U atnapp \
  -d atn_commerce
```

Expected result:

```text
/var/run/postgresql:5432 - accepting connections
```

### Creating the container from scratch

Choose a local development password and replace
`YOUR_LOCAL_PASSWORD` below:

```bash
docker run \
  --name atn-postgres \
  -e POSTGRES_USER=atnapp \
  -e POSTGRES_PASSWORD=YOUR_LOCAL_PASSWORD \
  -e POSTGRES_DB=atn_commerce \
  -p 5432:5432 \
  -v atn-postgres-data:/var/lib/postgresql/data \
  -d postgres:17-alpine
```

The Docker volume preserves PostgreSQL data when the container is
stopped or restarted.

Do not commit the database password to Git.

## Database Connection

CatLens reads its connection string from:

```text
ConnectionStrings:CatLensDatabase
```

Use .NET User Secrets for the local database password.

Initialize User Secrets once:

```bash
dotnet user-secrets init \
  --project src/CatLens.Web/CatLens.Web.csproj
```

Set the local connection string:

```bash
dotnet user-secrets set \
  "ConnectionStrings:CatLensDatabase" \
  "Host=localhost;Port=5432;Database=atn_commerce;Username=atnapp;Password=YOUR_LOCAL_PASSWORD" \
  --project src/CatLens.Web/CatLens.Web.csproj
```

User Secrets are stored outside the repository and are not committed
to Git.

## Restore and Build

From the repository root:

```bash
dotnet restore
dotnet build
```

Expected result:

```text
Build succeeded.
```

## Apply Database Migrations

Install the Entity Framework Core command-line tool if necessary:

```bash
dotnet tool install --global dotnet-ef
```

Apply the migrations:

```bash
dotnet ef database update \
  --project src/CatLens.Web/CatLens.Web.csproj \
  --startup-project src/CatLens.Web/CatLens.Web.csproj
```

This creates the CatLens tables in PostgreSQL.

## Run the Application

```bash
dotnet run \
  --project src/CatLens.Web/CatLens.Web.csproj
```

Open the localhost address displayed in the terminal.

CatLens intentionally uses the normal `dotnet run` workflow rather than
`dotnet watch`.

Stop the application with:

```text
Ctrl+C
```

## Import Workflow

1. Export inventory from Shopify Admin.
2. Choose all required locations and variants.
3. Download the inventory as a CSV file.
4. Open CatLens and select **Import**.
5. Select the CSV file.
6. Start the import.
7. Open **Catalog** to inspect the imported snapshot.
8. Open **Review** to inspect possible data issues.

The previous database snapshot is replaced only after the entire file
has been imported successfully.

Catalog files may contain business data and should not be committed to
the repository.

## Review Rules

### Missing SKU

A variant is included when its SKU is blank after normalization.

### Duplicate SKU

A SKU is included when it is assigned to more than one variant.

Duplicate comparison is case-insensitive.

### High Inventory

A variant-location inventory level is included when its on-hand
quantity is greater than the configured warning threshold.

The default threshold is:

```text
1000
```

The threshold is configuration, not a conclusion that the inventory
value is incorrect.

## CSV Reports

Each review page provides an **Export CSV** action.

Available reports:

- Missing SKU
- Duplicate SKU
- High Inventory

Reports use UTF-8 with a byte-order mark so they open correctly in
Excel and Numbers.

The exported reports are for review only and are not intended to be
imported directly back into Shopify.

## Docker Commands

Start PostgreSQL:

```bash
docker start atn-postgres
```

Stop PostgreSQL:

```bash
docker stop atn-postgres
```

View the container:

```bash
docker ps --filter name=atn-postgres
```

View PostgreSQL logs:

```bash
docker logs atn-postgres
```

Open a PostgreSQL shell:

```bash
docker exec -it atn-postgres \
  psql \
  -U atnapp \
  -d atn_commerce
```

## V1 Safety Boundaries

CatLens V1:

- Does not connect to a live Shopify store.
- Does not require a Shopify access token.
- Does not read customer or order data.
- Does not modify Shopify products.
- Does not modify Shopify inventory.
- Does not make automatic data-quality conclusions.
- Does not run background synchronization.

## Future Roadmap

Possible later phases:

- Manual read-only Shopify Admin GraphQL synchronization.
- A **Sync Now** button.
- Import history and snapshot comparison.
- Dead-stock reporting.
- Orders analysis, if approved.
- Authentication and role-based authorization.
- Automated tests.
- Deployment for internal use.

Background synchronization and retry behavior will only be considered
after manual synchronization is reliable.

## Development Status

The V1 workflow is functional:

```text
Import CSV → Store snapshot → Review issues → Export reports
```

Final V1 work focuses on validation, testing, documentation, and
release preparation.