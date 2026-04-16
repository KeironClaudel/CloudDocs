// System imports
using CloudDocs.API.Common;
// Global Error Handling Middleware
using CloudDocs.API.Extensions;
using CloudDocs.Application.Common.Interfaces.Persistence;
// Auth Imports
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
// Access Level Features
using CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevelById;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevels;
using CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;
// Auditting Features
using CloudDocs.Application.Features.AuditLogs.GetAuditLogs;
using CloudDocs.Application.Features.Auth.ChangePassword;
// Password management features
using CloudDocs.Application.Features.Auth.ForgotPassword;
using CloudDocs.Application.Features.Auth.Login;
using CloudDocs.Application.Features.Auth.Logout;
// Token management features
using CloudDocs.Application.Features.Auth.RefreshToken;
using CloudDocs.Application.Features.Auth.ResetPassword;
// Categories Features
using CloudDocs.Application.Features.Categories.CreateCategory;
using CloudDocs.Application.Features.Categories.DeactivateCategory;
using CloudDocs.Application.Features.Categories.GetCategories;
using CloudDocs.Application.Features.Categories.GetCategoryById;
using CloudDocs.Application.Features.Categories.ReactivateCategory;
using CloudDocs.Application.Features.Categories.UpdateCategory;
// Client features
using CloudDocs.Application.Features.Clients.CreateClient;
using CloudDocs.Application.Features.Clients.DeactivateClient;
using CloudDocs.Application.Features.Clients.GetClientById;
using CloudDocs.Application.Features.Clients.GetClients;
using CloudDocs.Application.Features.Clients.ReactivateClient;
using CloudDocs.Application.Features.Clients.SearchClients;
using CloudDocs.Application.Features.Clients.UpdateClient;
// Deparments Features
using CloudDocs.Application.Features.Departments.CreateDepartment;
using CloudDocs.Application.Features.Departments.DeactivateDepartment;
using CloudDocs.Application.Features.Departments.GetDepartmentById;
using CloudDocs.Application.Features.Departments.GetDepartments;
using CloudDocs.Application.Features.Departments.ReactivateDepartment;
using CloudDocs.Application.Features.Departments.UpdateDepartment;
// Documents Features
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.ReactivateDocument;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Application.Features.Documents.SendDocumentToClient;
using CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;
using CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;
//Email features
using CloudDocs.Application.Features.Email;
// Users Features
using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Application.Features.Users.UpdateUser;
using CloudDocs.Infrastructure.Persistence;
using CloudDocs.Infrastructure.Persistence.Repositories;
using CloudDocs.Infrastructure.Security;
using CloudDocs.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database SUPABASE configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger configuration for JWT Authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CloudDocs API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication configuration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader["Bearer ".Length..].Trim();
                return Task.CompletedTask;
            }

            var cookieSettings = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<AuthCookieSettings>>()
                .Value;

            var token = context.Request.Cookies[cookieSettings.AccessTokenCookieName];

            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});

// AuthCookies
builder.Services.Configure<AuthCookieSettings>(
    builder.Configuration.GetSection(AuthCookieSettings.SectionName));

builder.Services.AddScoped<AuthCookieHelper>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// User services
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddScoped<IGetUsersService, GetUsersService>();
builder.Services.AddScoped<IGetUserByIdService, GetUserByIdService>();
builder.Services.AddScoped<ICreateUserService, CreateUserService>();
builder.Services.AddScoped<IUpdateUserService, UpdateUserService>();
builder.Services.AddScoped<IDeactivateUserService, DeactivateUserService>();
builder.Services.AddScoped<IReactivateUserService, ReactivateUserService>();

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILoginService, LoginService>();

// Document services
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IUploadDocumentService, UploadDocumentService>();
builder.Services.AddScoped<ISearchDocumentsService, SearchDocumentsService>();
builder.Services.AddScoped<IGetDocumentByIdService, GetDocumentByIdService>();
builder.Services.AddScoped<IRenameDocumentService, RenameDocumentService>();
builder.Services.AddScoped<IDeactivateDocumentService, DeactivateDocumentService>();
builder.Services.AddScoped<IGetDocumentFileService, GetDocumentFileService>();
builder.Services.AddScoped<IDocumentVersionRepository, DocumentVersionRepository>();
builder.Services.AddScoped<IGetDocumentVersionsService, GetDocumentVersionsService>();
builder.Services.AddScoped<IUploadDocumentVersionService, UploadDocumentVersionService>();
builder.Services.AddScoped<IReactivateDocumentService, ReactivateDocumentService>();
builder.Services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
builder.Services.AddScoped<IAccessLevelRepository, AccessLevelRepository>();

builder.Services.AddScoped<IGetDocumentTypesService, GetDocumentTypesService>();
builder.Services.AddScoped<IGetDocumentTypeByIdService, GetDocumentTypeByIdService>();
builder.Services.AddScoped<ICreateDocumentTypeService, CreateDocumentTypeService>();
builder.Services.AddScoped<IUpdateDocumentTypeService, UpdateDocumentTypeService>();
builder.Services.AddScoped<IDeactivateDocumentTypeService, DeactivateDocumentTypeService>();
builder.Services.AddScoped<IReactivateDocumentTypeService, ReactivateDocumentTypeService>();
builder.Services.AddScoped<IAccessLevelRepository, AccessLevelRepository>();

builder.Services.AddScoped<IGetAccessLevelsService, GetAccessLevelsService>();
builder.Services.AddScoped<IGetAccessLevelByIdService, GetAccessLevelByIdService>();
builder.Services.AddScoped<ICreateAccessLevelService, CreateAccessLevelService>();
builder.Services.AddScoped<IUpdateAccessLevelService, UpdateAccessLevelService>();
builder.Services.AddScoped<IDeactivateAccessLevelService, DeactivateAccessLevelService>();
builder.Services.AddScoped<IReactivateAccessLevelService, ReactivateAccessLevelService>();
builder.Services.AddScoped<IUpdateDocumentVisibilityService, UpdateDocumentVisibilityService>();

// Category services
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IGetCategoriesService, GetCategoriesService>();
builder.Services.AddScoped<IGetCategoryByIdService, GetCategoryByIdService>();
builder.Services.AddScoped<ICreateCategoryService, CreateCategoryService>();
builder.Services.AddScoped<IUpdateCategoryService, UpdateCategoryService>();
builder.Services.AddScoped<IDeactivateCategoryService, DeactivateCategoryService>();
builder.Services.AddScoped<IReactivateCategoryService, ReactivateCategoryService>();

// Department services
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

// Document Access services
builder.Services.AddScoped<IDocumentAccessService, DocumentAccessService>();

// Auditing services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IGetAuditLogsService, GetAuditLogsService>();

// Password magagement services
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
builder.Services.AddScoped<IChangePasswordService, ChangePasswordService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

// Token management services
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ILogoutService, LogoutService>();

// Department services
builder.Services.AddScoped<IGetDepartmentsService, GetDepartmentsService>();
builder.Services.AddScoped<IGetDepartmentByIdService, GetDepartmentByIdService>();
builder.Services.AddScoped<ICreateDepartmentService, CreateDepartmentService>();
builder.Services.AddScoped<IUpdateDepartmentService, UpdateDepartmentService>();
builder.Services.AddScoped<IDeactivateDepartmentService, DeactivateDepartmentService>();
builder.Services.AddScoped<IReactivateDepartmentService, ReactivateDepartmentService>();

// Client services
builder.Services.AddScoped<IClientRepository, ClientRepository>();

builder.Services.AddScoped<IGetClientsService, GetClientsService>();
builder.Services.AddScoped<IGetClientByIdService, GetClientByIdService>();
builder.Services.AddScoped<ISearchClientsService, SearchClientsService>();
builder.Services.AddScoped<ICreateClientService, CreateClientService>();
builder.Services.AddScoped<IUpdateClientService, UpdateClientService>();
builder.Services.AddScoped<IDeactivateClientService, DeactivateClientService>();
builder.Services.AddScoped<IReactivateClientService, ReactivateClientService>();

builder.Services.Configure<FrontendSettings>(
    builder.Configuration.GetSection(FrontendSettings.SectionName));

// Email Service
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection(SmtpSettings.SectionName));

builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<ISendDocumentToClientService, SendDocumentToClientService>();

// Azure Blob Settings
builder.Services.Configure<AzureBlobSettings>(
    builder.Configuration.GetSection(AzureBlobSettings.SectionName));

// Demo policy service
builder.Services.Configure<DemoSettings>(
    builder.Configuration.GetSection(DemoSettings.SectionName));

builder.Services.AddScoped<IDemoPolicyService, DemoPolicyService>();

// File storage service configuration based on appsettings
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection(FileStorageSettings.SectionName));

builder.Services.Configure<AzureBlobSettings>(
    builder.Configuration.GetSection(AzureBlobSettings.SectionName));

var storageProvider = builder.Configuration["Storage:Provider"];

if (string.Equals(storageProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
}
else
{
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
}

// Health check services
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5174",
                "https://clouddocs-frontend.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();


// Middleware pipeline configuration
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seeder
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AppDbContextSeed.SeedAsync(dbContext);
}

app.Run();