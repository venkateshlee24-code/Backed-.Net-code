using MyWebApi.Application.Auth;
using MyWebApi.Application.Services;
using MyWebApi.Infrastructure.Data;
using MyWebApi.Infrastructure.Repositories;
using MyWebApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8017");

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("JWT key must be at least 32 characters.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:8000") // Angular URL
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter: Bearer {your JWT token}"
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
builder.Services.AddOutputCache();
builder.Services.AddProblemDetails();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationConstants.Policies.CanInitiateAp,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.ApInitiator));

    options.AddPolicy(
        AuthorizationConstants.Policies.CanAuthoriseAp,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.ApAuthoriser));

    options.AddPolicy(
        AuthorizationConstants.Policies.CanInitiateAr,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.ArInitiator));

    options.AddPolicy(
        AuthorizationConstants.Policies.CanAuthoriseAr,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.ArAuthoriser));

    options.AddPolicy(
        AuthorizationConstants.Policies.CanInitiateGl,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.GlInitiator));

    options.AddPolicy(
        AuthorizationConstants.Policies.CanAuthoriseGl,
        policy => policy.RequireClaim(
            AuthorizationConstants.ClaimTypes.Permission,
            AuthorizationConstants.Permissions.GlAuthoriser));
});

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAccountingRepository, AccountingRepository>();
builder.Services.AddScoped<IAccountingService, AccountingService>();

var app = builder.Build();
app.UseCors("AllowAngularApp");
app.UseExceptionHandler();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
