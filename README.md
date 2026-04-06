# 📂 CloudDocs API

A robust document management system (DMS) built with .NET 8, following Clean Architecture, designed to handle secure document storage, versioning, and access control in enterprise environments.

---

## 🚀 Overview

CloudDocs is a backend API that allows organizations to:

- Store and manage documents securely  
- Maintain version history of documents  
- Control access based on roles, ownership, and departments  
- Track actions through audit logs  
- Authenticate users using JWT with refresh tokens  

This project is designed as a scalable and maintainable backend system, suitable for real-world business use cases.

Recent changes (short):
- Document types and access levels were moved to configurable database entities so they can be managed at runtime.
- Departments are modelled as entities and documents support a many-to-many relation to departments for visibility.

---

## 🧠 Architecture

The solution follows **Clean Architecture principles**:

```
CloudDocs
│
├── CloudDocs.API            # Presentation layer (Controllers, Middleware)
├── CloudDocs.Application    # Business logic (Use Cases, Services, DTOs)
├── CloudDocs.Domain         # Core entities and enums
├── CloudDocs.Infrastructure # Persistence, external services (EF Core, Storage)
└── CloudDocs.Tests          # Unit tests (xUnit, Moq)
```

### Patterns used

- Clean Architecture  
- Repository Pattern  
- Unit of Work  
- Dependency Injection  
- Middleware for exception handling  
- CQRS-style service separation  

---

Notes about the updated domain model:
- `AccessLevelEntity`, `DocumentTypeEntity`, and `Department` are domain entities under `CloudDocs.Domain.Entities` and persisted via EF in `CloudDocs.Infrastructure`.
- The document visibility model changed: `Document` now relates to departments via `DocumentDepartment` (many-to-many). This impacts repositories and upload flow.

## 🛠️ Tech Stack

- .NET 8  
- C#  
- Entity Framework Core  
- SQL Server  
- JWT Authentication + Refresh Tokens  
- BCrypt (Password Hashing)  
- xUnit + Moq (Testing)  
- FluentAssertions  
- Swagger / OpenAPI  

---

## 🔐 Authentication & Security

- JWT-based authentication  
- Refresh token mechanism  
- Password hashing with BCrypt  
- Account lockout after multiple failed attempts  
- Role-based authorization (Admin / User)  
- Fine-grained document access control  

---

## 📂 Core Features

### 👤 Users

- Create, update, deactivate users  
- Role assignment  
- Department support  

### 📄 Documents

- Upload PDF documents  
- Metadata support (category, type, expiration, etc.)  
- Search with filters:
  - Name  
  - Category  
  - Month / Year  

Update notes (documents):
- The upload contract now accepts `DocumentTypeId` and `AccessLevelId` (both GUIDs) and `DepartmentIds` (List<Guid>) when appropriate.
- Access levels and document types are now configurable from the database rather than fixed enums.

### 🧾 Document Versioning

- Automatic version 1 on upload  
- Upload new versions  
- Full version history per document  

Response update: `DocumentResponse` now includes `AccessLevelId`, `AccessLevelName`, `AccessLevelCode` and `VisibleDepartments` (list of departments allowed to see the document).

### 🔍 Access Control

Documents support different access levels:

- **InternalPublic** → All authenticated users  
- **Private** → Owner only  
- **OwnerOnly** → Owner only  
- **AdminOnly** → Admin users only  
- **DepartmentOnly** → Same department or admin  

### 📊 Audit Logs

Tracks:

- Login attempts  
- Document actions  
- Token usage  

Read-only audit queries included.

---

## ⚙️ Setup & Run

### 1. Clone repository

```bash
git clone https://github.com/your-username/CloudDocs.git
cd CloudDocs
```

### 2. Configure database

Update your connection string in:

```
CloudDocs.API/appsettings.json
```

### 3. Apply migrations

```bash
dotnet ef database update \
--project CloudDocs.Infrastructure \
--startup-project CloudDocs.API
```

If you changed the model to move AccessLevel or DocumentType from enum to entity, create a migration first:

```pwsh
dotnet ef migrations add ReplaceAccessLevelEnumWithEntity \
  --project .\CloudDocs.Infrastructure\CloudDocs.Infrastructure.csproj \
  --startup-project .\CloudDocs.API\CloudDocs.API.csproj \
  --output-dir Persistence/Migrations
```

### 4. Run the API

```bash
dotnet run --project CloudDocs.API
```

Swagger will be available at:

```
https://localhost:xxxx/swagger
```

---

## 🔑 Example Endpoints

### Auth

- `POST /api/auth/login`  
- `POST /api/auth/refresh-token`  
- `POST /api/auth/logout`  

### Documents

- `POST /api/documents/upload`  
- `GET /api/documents`  
- `GET /api/documents/{id}`  
- `GET /api/documents/{id}/preview`  
- `GET /api/documents/{id}/download`  

### Versions

- `GET /api/documents/{id}/versions`  
- `POST /api/documents/{id}/versions`  

### Users

- `POST /api/users`  
- `PUT /api/users/{id}`  
- `DELETE /api/users/{id}`  

---

## 🧪 Testing

Unit tests are implemented using:

- xUnit  
- Moq  
- FluentAssertions  

### Covered services

- LoginService  
- CreateUserService  
- ChangePasswordService  
- RefreshTokenService  
- UploadDocumentService  
- DocumentAccessService  

Run tests with:

```bash
dotnet test
```
---

## 🧱 Key Design Decisions

### ✅ UnitOfWork

Ensures atomic operations across repositories and improves consistency.

### ✅ Global Exception Handler

Centralized error handling with consistent API responses.

### ✅ Logging

Structured logging using `ILogger` for:

- Warnings (validation/auth issues)  
- Errors (unexpected failures)  

### ✅ Document Versioning

Prevents data loss and enables auditability of file changes.

---

## 📈 Future Improvements

- Pagination at database level with access filters  
- File storage integration (AWS S3 / Azure Blob)  
- Role-based policy authorization  
- Caching (Redis)  
- Rate limiting  
- Full integration tests  

---

## 👨‍💻 Author

Developed by **Keiron**  
Backend Developer focused on .NET, APIs, and Data Systems  

---

## ⭐ Final Notes

This project demonstrates:

- Strong backend architecture design  
- Secure authentication flows  
- Real-world business logic implementation  
- Clean and testable codebase  
