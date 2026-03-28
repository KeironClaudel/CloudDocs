// System imports
using System.Text;
using CloudDocs.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using CloudDocs.Application.Common.Interfaces.Services;

// Auth Imports
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Auth.Login;
using CloudDocs.Infrastructure.Persistence;
using CloudDocs.Infrastructure.Persistence.Repositories;
using CloudDocs.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Users Features
using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Application.Features.Users.UpdateUser;

// Categories Features
using CloudDocs.Application.Features.Categories.CreateCategory;
using CloudDocs.Application.Features.Categories.GetCategoryById;
using CloudDocs.Application.Features.Categories.GetCategories;
using CloudDocs.Application.Features.Categories.UpdateCategory;
using CloudDocs.Application.Features.Categories.DeactivateCategory;
using CloudDocs.Application.Features.Categories.ReactivateCategory;

// Documents Features
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;

// Auditting Features
using CloudDocs.Infrastructure.Services;
using CloudDocs.Application.Features.AuditLogs.GetAuditLogs;

// Password management features
using CloudDocs.Application.Features.Auth.ForgotPassword;
using CloudDocs.Application.Features.Auth.ResetPassword;
using CloudDocs.Application.Features.Auth.ChangePassword;

// Tpken management features
using CloudDocs.Application.Features.Auth.RefreshToken;
using CloudDocs.Application.Features.Auth.Logout;

// Global Error Handling Middleware
using CloudDocs.API.Extensions;

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

// Token management services
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ILogoutService, LogoutService>();

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