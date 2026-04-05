// System imports
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CloudDocs.Infrastructure.Persistence;
using CloudDocs.Infrastructure.Persistence.Repositories;
using CloudDocs.Infrastructure.Security;
using CloudDocs.Infrastructure.Services;
using CloudDocs.Application.Common.Models;


// Global Error Handling Middleware
using CloudDocs.API.Extensions;
using CloudDocs.Application.Common.Interfaces.Persistence;

// Auth Imports
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;

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

// Documents Features
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.ReactivateDocument;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;
using CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;

// Access Level Features
using CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevelById;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevels;
using CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;

// Users Features
using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Application.Features.Users.UpdateUser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

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

// Category services
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IGetCategoriesService, GetCategoriesService>();
builder.Services.AddScoped<IGetCategoryByIdService, GetCategoryByIdService>();
builder.Services.AddScoped<ICreateCategoryService, CreateCategoryService>();
builder.Services.AddScoped<IUpdateCategoryService, UpdateCategoryService>();
builder.Services.AddScoped<IDeactivateCategoryService, DeactivateCategoryService>();
builder.Services.AddScoped<IReactivateCategoryService, ReactivateCategoryService>();

// Document services
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection(FileStorageSettings.SectionName));

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
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

// Email service
builder.Services.Configure<FrontendSettings>(
    builder.Configuration.GetSection(FrontendSettings.SectionName));

builder.Services.AddScoped<IEmailService, EmailService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5007",
                "https://localhost:7121"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline configuration
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seeder
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AppDbContextSeed.SeedAsync(dbContext);
}

app.Run();