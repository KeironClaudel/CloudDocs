# CloudDocs API

Document management backend built with .NET 8 and Clean Architecture. The API handles secure document storage, configurable visibility rules, document versioning, client delivery flows, and administrative catalogs for enterprise-style document operations.

## Overview

CloudDocs allows organizations to:

- Store and version PDF documents securely
- Manage users, departments, clients, categories, document types, and access levels
- Control visibility with configurable access levels and department-based restrictions
- Deliver documents or specific document versions to clients by email
- Authenticate users with JWT access tokens plus rotating refresh tokens stored in HTTP-only cookies
- Audit security-sensitive and business-critical actions

The current backend already includes:

- Rate limiting for `POST /api/auth/login` and `POST /api/auth/forgot-password`
- Configurable document types and access levels stored in the database
- Department-based visibility updates after upload
- Local disk and Azure Blob Storage implementations
- Server-side paginated document search
- Streamed upload, preview, and download flows
- Background-queued audit logging for hot document paths
- Client-aware storage paths grouped by year, month, category, and document type

## Architecture

The solution follows Clean Architecture principles:

```text
CloudDocs
|
|-- CloudDocs.API            # Controllers, middleware, configuration
|-- CloudDocs.Application    # Use cases, validators, DTOs, service contracts
|-- CloudDocs.Domain         # Entities and domain rules
|-- CloudDocs.Infrastructure # EF Core, repositories, storage, email, security
`-- CloudDocs.Tests          # Unit tests
```

Patterns used:

- Clean Architecture
- Repository Pattern
- Unit of Work
- Dependency Injection
- Global exception handling middleware
- FluentValidation
- CQRS-style service separation

## Tech Stack

- .NET 8
- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- HTTP-only auth cookies
- BCrypt password hashing
- ASP.NET Core Rate Limiting
- MailKit SMTP email delivery
- Azure Blob Storage or local file storage
- xUnit, Moq, FluentAssertions
- Swagger / OpenAPI

## Authentication And Security

CloudDocs supports bearer auth and cookie-based auth at the same time:

- Access tokens are validated from the `Authorization: Bearer ...` header when present
- If there is no bearer header, the API falls back to the configured access-token cookie
- Refresh tokens are stored in a dedicated HTTP-only cookie and rotated on refresh

Current defaults from [`CloudDocs.API/appsettings.json`](./CloudDocs.API/appsettings.json):

- JWT access token expiration: `15` minutes
- Access token cookie expiration: `15` minutes
- Refresh token cookie expiration: `7` days
- `SameSite=None`
- `Secure=true`

Cookie behavior is implemented in [`CloudDocs.API/Common/AuthCookieHelper.cs`](./CloudDocs.API/Common/AuthCookieHelper.cs):

- Access token cookie path: `/`
- Refresh token cookie path: `/api/auth`
- Both cookies use `HttpOnly`
- Both cookies use `Expires` plus `MaxAge`

Other security features:

- Password hashing with BCrypt
- Account lockout after repeated failed logins
- Role-based authorization
- Fine-grained document visibility rules
- Rate limiting on auth-sensitive endpoints

### Rate Limiting

The API configures an `AuthStrict` fixed-window policy in [`CloudDocs.API/Program.cs`](./CloudDocs.API/Program.cs) with:

- 5 requests per minute
- Partitioning by client IP
- HTTP `429 Too Many Requests` on rejection
- `Retry-After` response header when available
- JSON error response for throttled requests

This policy is applied to:

- `POST /api/auth/login`
- `POST /api/auth/forgot-password`

## Core Features

### Users

- Create, update, deactivate, and reactivate users
- Assign role and department
- Query users by id or list

### Departments

- Create, list, update, deactivate, and reactivate departments

### Clients

- Create, list, search, update, deactivate, and reactivate clients

### Catalogs

- Categories
- Document types
- Access levels

These catalogs are stored in the database and can be managed at runtime.

### Documents

- Upload PDF documents with metadata
- Associate documents with category, client, document type, and access level
- Attach visible departments for `DEPARTMENT_ONLY` documents
- Rename, deactivate, reactivate, and query documents
- Search documents with server-side pagination
- Update visibility after upload
- Download or preview the latest file or a specific version
- Send the current file or a selected version to the assigned client by email

The upload contract currently includes:

- `CategoryId`
- `ClientId`
- `DocumentTypeId`
- `AccessLevelId`
- `DepartmentIds`
- `ExpirationDate`
- `ExpirationDatePendingDefinition`

Upload rules currently enforced:

- PDF only
- Max file size from configuration
- Demo-mode limits when `Demo:Enabled=true`

### Document Versioning

- Automatic initial version on document upload
- Upload new versions for existing documents
- Retrieve full version history
- Retrieve paginated version history
- Download and preview by specific version
- Send a selected version to a client by providing `VersionId`

### Access Control

Access levels are configurable entities, and the seeded defaults are:

- `INTERNAL_PUBLIC`
- `ADMIN_ONLY`
- `OWNER_ONLY`
- `DEPARTMENT_ONLY`

Documents can also be restricted to specific departments through the `DocumentDepartment` relation.

### Audit Logs

Tracks relevant actions such as:

- Authentication events
- Document uploads, versions, visibility changes, and delivery events
- Security-sensitive flows

The API exposes read-only audit log queries for administration/reporting scenarios.

### Health Checks

- `GET /health`

## Storage Model

Main configuration lives in [`CloudDocs.API/appsettings.json`](./CloudDocs.API/appsettings.json).

Key sections:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings`
- `Storage`
- `FileStorage`
- `AzureBlob`
- `Frontend`
- `AuthCookies`
- `Demo`

### Storage Providers

The API supports two storage modes:

- `Local`
- `AzureBlob`

`Storage:Provider` selects which implementation is registered at runtime.

### Logical File Layout

The current logical storage layout for new documents is:

```text
YYYY/MM/Client/{client-slug}/{category-slug}/{DOCUMENT-TYPE}/{guid}.pdf
```

Examples:

```text
2026/04/Client/contoso/contracts/CONTRACT/7f5....pdf
2026/04/Client/keiron-claudel/prueba/PLAN-DE-ESTUDIO-PRUEBA/versions/{documentId}/{guid}.pdf
```

Notes:

- `Client` is stored as a literal path segment
- Client and category names are slugified in lowercase
- Document type is slugified and stored in uppercase
- New versions follow the same base directory and append `versions/{documentId}`
- The version path builder still supports a fallback path strategy for older documents stored under previous layouts

## Setup And Run

### 1. Clone Repository

```bash
git clone https://github.com/KeironClaudel/CloudDocs.git
cd CloudDocs
```

### 2. Configure Application Settings

Set the required values in:

```text
CloudDocs.API/appsettings.json
```

At minimum:

- PostgreSQL connection string
- JWT secret
- Frontend base URL
- Auth cookie settings appropriate for the environment
- Email settings if you will use client delivery flows
- Azure Blob settings if using `Storage:Provider=AzureBlob`

### 3. Apply Migrations

```bash
dotnet ef database update \
  --project CloudDocs.Infrastructure \
  --startup-project CloudDocs.API
```

### 4. Run The API

```bash
dotnet run --project CloudDocs.API
```

Swagger is available at:

```text
https://localhost:xxxx/swagger
```

Health checks are available at:

```text
https://localhost:xxxx/health
```

## API Surface

### Auth

- `GET /api/auth/me`
- `POST /api/auth/login`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/change-password`
- `POST /api/auth/refresh-token`
- `POST /api/auth/logout`

### Users

- `POST /api/users`
- `GET /api/users`
- `GET /api/users/{id}`
- `PUT /api/users/{id}`
- `PATCH /api/users/{id}/deactivate`
- `PATCH /api/users/{id}/reactivate`

### Departments

- `POST /api/departments`
- `GET /api/departments`
- `GET /api/departments/{id}`
- `PUT /api/departments/{id}`
- `PATCH /api/departments/{id}/deactivate`
- `PATCH /api/departments/{id}/reactivate`

### Clients

- `POST /api/clients`
- `GET /api/clients`
- `GET /api/clients/search?term=...`
- `GET /api/clients/{id}`
- `PUT /api/clients/{id}`
- `PATCH /api/clients/{id}/deactivate`
- `PATCH /api/clients/{id}/reactivate`

### Categories

- `POST /api/categories`
- `GET /api/categories`
- `GET /api/categories/{id}`
- `PUT /api/categories/{id}`
- `PATCH /api/categories/{id}/deactivate`
- `PATCH /api/categories/{id}/reactivate`

### Document Types

- `POST /api/document-types`
- `GET /api/document-types`
- `GET /api/document-types/{id}`
- `PUT /api/document-types/{id}`
- `PATCH /api/document-types/{id}/deactivate`
- `PATCH /api/document-types/{id}/reactivate`

### Access Levels

- `POST /api/access-levels`
- `GET /api/access-levels`
- `GET /api/access-levels/{id}`
- `PUT /api/access-levels/{id}`
- `PATCH /api/access-levels/{id}/deactivate`
- `PATCH /api/access-levels/{id}/reactivate`

### Documents

- `POST /api/documents/upload`
- `GET /api/documents`
- `GET /api/documents/{id}`
- `GET /api/documents/{id}/preview`
- `GET /api/documents/{id}/download`
- `PUT /api/documents/{id}/rename`
- `GET /api/documents/{id}/versions`
- `GET /api/documents/{id}/versions/paged`
- `POST /api/documents/{id}/versions`
- `PATCH /api/documents/{id}/deactivate`
- `PATCH /api/documents/{id}/reactivate`
- `PATCH /api/documents/{id}/visibility`
- `POST /api/documents/{id}/send-to-client`

### Audit Logs

- `GET /api/audit-logs`

## Testing

The solution currently includes unit tests with:

- xUnit
- Moq
- FluentAssertions

Current test suite size:

- 40 test files
- 129 `[Fact]` test cases

Coverage includes:

- Authentication flows
- Users, clients, departments, categories, document types, and access levels
- Document upload, search, versioning, visibility, and client delivery
- Audit log queries
- Document access and storage-path behavior

Run tests with:

```bash
dotnet test
```

## Key Design Decisions

### Configurable Catalogs

Access levels and document types are stored as entities instead of hardcoded enums so they can evolve without code changes.

### Visibility Model

Document visibility combines access-level rules with department-specific restrictions.

### Paged Document Listing

`GET /api/documents` supports `page` and `pageSize`, so consumers should paginate incrementally instead of loading the full corpus at once.

### Global Exception Handling

Errors are normalized through middleware for consistent API responses.

### Pluggable Storage

The API can run with local storage in development and Azure Blob Storage in cloud scenarios.

### Streaming File Pipeline

Uploads, previews, and downloads avoid loading full files into memory, which keeps the API more stable as document volume grows.

### Defensive Auth Flow

Authentication combines lockout logic, refresh tokens, HTTP-only cookies, and rate limiting.

## Future Improvements

- Integration and end-to-end API tests
- Distributed caching
- More granular authorization policies
- Background processing for heavier email/document workflows
- Operational dashboards and metrics

## Author

Developed by **Keiron**  
Backend Developer focused on .NET, APIs, and Data Systems

## Final Notes

This project demonstrates:

- Secure backend architecture
- Configurable business rules
- Real-world document management workflows
- Maintainable and testable .NET code
