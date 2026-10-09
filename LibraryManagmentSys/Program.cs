using System.Text;
using Library.Application.Interfaces;
using Library.Infrastructure.Auth;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Services;
using LibraryManagmentSys.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LibraryManagmentSys.Swagger;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management System API", Version = "v1" });


    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});


builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));



builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();



var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = "role",
        NameClaimType = "unique_name",
    };
});

builder.Services.AddAuthorization();


const string CorsPolicyName = "FrontendPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://library.isd-inapp.com"];


builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//starts
var app = builder.Build();



app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);



app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Library Management System API is running.")
    .WithTags("LibraryApi")
    .AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithTags("LibraryApi")
    .AllowAnonymous();

app.MapControllers();

// best-effort warm-up: pay the one-time JIT / EF model-build cost now, not on the first login
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Users.AsNoTracking()
        .Where(u => u.Id == 0)
        .Select(u => new { u.Id, u.Username, u.Password, Roles = u.UserRoleBridges.Select(b => b.UserRole!.Role).ToList() })
        .FirstOrDefaultAsync();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    hasher.Verify("warm-up", hasher.Hash("warm-up"));

    scope.ServiceProvider.GetRequiredService<IJwtTokenService>()
        .GenerateAccessToken(new JwtPrincipalInfo(0, "warm-up", []));
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Startup warm-up skipped.");
}

app.Run();
