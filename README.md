# 📂 CloudDocs API

Document management backend built with .NET 8 and Clean Architecture. The API handles secure document storage, configurable visibility rules, document versioning, client delivery flows, and administrative catalogs for enterprise-style document operations.

---

## 🚀 Overview

CloudDocs allows organizations to:

- Store and version documents securely
- Manage users, departments, clients, categories, document types, and access levels
- Control visibility with configurable access levels and department-based restrictions
- Deliver documents to clients by email
- Authenticate users with JWT + refresh tokens stored in cookies
- Audit security-sensitive and business-critical actions

Recent updates reflected in this version:

- Rate limiting is already implemented for `POST /api/auth/login` and `POST /api/auth/forgot-password`
- Document types and access levels are configurable database entities
- Departments and clients are part of the document flow
- Document visibility can be updated after upload
- File storage supports both local disk and Azure Blob Storage
- Document search is paginated server-side
- Document previews/downloads and uploads use streaming
- Read/write document audit logging uses a background queue for hot paths
- File storage now groups documents by `yyyy/mm/clients/client/category`

---

## 🧠 Architecture

The solution follows Clean Architecture principles:

```text
CloudDocs
│
├── CloudDocs.API            # Controllers, middleware, configuration
├── CloudDocs.Application    # Use cases, validators, DTOs, service contracts
├── CloudDocs.Domain         # Entities and domain rules
├── CloudDocs.Infrastructure # EF Core, repositories, storage, email, security
└── CloudDocs.Tests          # Unit tests
```

### Patterns used

- Clean Architecture
- Repository Pattern
- Unit of Work
- Dependency Injection
- Global exception handling middleware
- FluentValidation for request validation
- CQRS-style service separation

---

## 🛠️ Tech Stack

- .NET 8
- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL support
- JWT Authentication + Refresh Tokens
- Cookie-based auth token handling
- BCrypt password hashing
- ASP.NET Core Rate Limiting
- MailKit for email delivery
- Azure Blob Storage or local file storage
- xUnit + Moq + FluentAssertions
- Swagger / OpenAPI

---

## 🔐 Authentication & Security

- JWT-based authentication
- Refresh token rotation flow
- Auth cookies for access and refresh tokens
- Password hashing with BCrypt
- Account lockout after repeated failed logins
- Role-based authorization
- Fine-grained document visibility rules
- Rate limiting for login and forgot-password endpoints

### Rate limiting

The API configures an `AuthStrict` fixed-window policy in [`Program.cs`](C:/Users/Keiron/source/repos/CloudDocs/CloudDocs.API/Program.cs) with:

- 5 requests per minute
- Partitioning by client IP
- HTTP `429 Too Many Requests` on rejection
- `Retry-After` response header when available
- JSON error response for throttled requests

This policy is currently applied to:

- `POST /api/auth/login`
- `POST /api/auth/forgot-password`

---

## 📂 Core Features

### 👤 Users

- Create, update, deactivate, and reactivate users
- Role assignment
- Department assignment
- Admin password reset support

### 🏢 Departments

- Create, list, update, deactivate, and reactivate departments

### 🤝 Clients

- Create, list, search, update, deactivate, and reactivate clients

### 🗂️ Catalogs

- Manage categories
- Manage document types
- Manage access levels

These catalogs are stored in the database and can be managed at runtime.

### 📄 Documents

- Upload PDF documents with metadata
- Associate documents with categories, clients, document types, and access levels
- Attach department visibility rules
- Rename, deactivate, reactivate, and query documents
- Search documents with server-side pagination
- Update visibility after upload
- Download or preview the latest file or a specific version
- Send documents to clients by email

The upload contract currently accepts:

- `CategoryId`
- `ClientId`
- `DocumentTypeId`
- `AccessLevelId`
- `DepartmentIds`
- Expiration metadata

### 🧾 Document Versioning

- Automatic initial version on upload
- Upload new versions
- Retrieve full version history
- Retrieve paginated version history
- Download and preview by specific version

### 🔍 Access Control

Access levels are configurable entities, and the seeded defaults are:

- `INTERNAL_PUBLIC`
- `ADMIN_ONLY`
- `OWNER_ONLY`
- `DEPARTMENT_ONLY`

Documents can also be restricted to specific departments through the `DocumentDepartment` relation.

### 📊 Audit Logs

Tracks relevant actions such as:

- Authentication events
- Document operations
- Security-sensitive flows

The API includes read-only audit log queries for administration/reporting scenarios.

### ❤️ Health Checks

- `GET /health`

---

## ⚙️ Configuration

Main configuration currently lives in [`CloudDocs.API/appsettings.json`](C:/Users/Keiron/source/repos/CloudDocs/CloudDocs.API/appsettings.json).

Key sections:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings`
- `Storage`
- `FileStorage`
- `AzureBlob`
- `Frontend`
- `AuthCookies`
- `Demo`

### Storage providers

The API supports two storage modes:

- `Local`
- `AzureBlob`

`Storage:Provider` selects which implementation is registered at runtime.

Current logical storage layout for uploaded files is:

```text
yyyy/mm/clients/{client-slug}/{category-slug}/{guid}.pdf
```

Version uploads are stored under the same hierarchy, inside a `versions/{document-id}` branch.

---

## ⚙️ Setup & Run

### 1. Clone repository

```bash
git clone https://github.com/KeironClaudel/CloudDocs.git
cd CloudDocs
```

### 2. Configure application settings

Set the required values in:

```text
CloudDocs.API/appsettings.json
```

At minimum:

- PostgreSQL connection string
- JWT secret
- Frontend base URL
- Email settings if you will use client delivery flows
- Azure Blob settings if using `Storage:Provider = AzureBlob`

### 3. Apply migrations

```bash
dotnet ef database update \
  --project CloudDocs.Infrastructure \
  --startup-project CloudDocs.API
```

### 4. Run the API

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

---

## 🔑 Example Endpoints

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

---

## 🧪 Testing

The solution includes unit tests with:

- xUnit
- Moq
- FluentAssertions

Current test suite size:

- 31 test files
- 128 `[Fact]` test cases

Coverage includes:

- Authentication flows
- Users, clients, departments, and catalogs
- Document upload, search, versioning, visibility, and delivery
- Audit log queries

Run tests with:

```bash
dotnet test
```

---

## 🧱 Key Design Decisions

### ✅ Configurable catalogs

Access levels and document types are stored as entities instead of hardcoded enums so they can evolve without code changes.

### ✅ Visibility model

Document visibility combines access level rules with department-specific restrictions.

### ✅ Paged document listing

`GET /api/documents` already supports `page` and `pageSize`, so frontend consumers should request the first page and paginate incrementally instead of loading the full corpus at once.

### ✅ Global exception handling

Errors are normalized through middleware for consistent API responses.

### ✅ Pluggable storage

The API can run with local storage in development and Azure Blob Storage in cloud scenarios.

### ✅ Streaming file pipeline

Uploads, previews, and downloads avoid loading full files into memory, which keeps the API more stable as document volume grows.

### ✅ Defensive auth flow

Authentication combines lockout logic, refresh tokens, cookies, and rate limiting.

---

## 📈 Future Improvements

- Integration and end-to-end API tests
- Distributed caching
- More granular authorization policies
- Background processing for heavier email/document workflows
- Operational dashboards and metrics

---

## 👨‍💻 Author

Developed by **Keiron**  
Backend Developer focused on .NET, APIs, and Data Systems

---

## ⭐ Final Notes

This project demonstrates:

- Secure backend architecture
- Configurable business rules
- Real-world document management workflows
- Maintainable and testable .NET code
