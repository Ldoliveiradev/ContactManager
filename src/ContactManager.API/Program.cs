using System.Text;
using ContactManager.API.Middleware;
using ContactManager.Application;
using ContactManager.Infrastructure;
using ContactManager.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application + Infrastructure composition roots.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT bearer authentication. The signing key/issuer/audience come from the same
// Jwt config section the token generator uses, so issued tokens validate here.
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// CORS for the Angular dev server.
const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Only redirect to HTTPS when an HTTPS endpoint is actually configured. In the
// container we serve plain HTTP on 8080 (TLS is terminated upstream), so redirecting
// would just emit warnings and break requests. Locally, launchSettings provides one.
var httpsConfigured = builder.Configuration["ASPNETCORE_HTTPS_PORTS"] is not null
    || (builder.Configuration["ASPNETCORE_URLS"]?.Contains("https", StringComparison.OrdinalIgnoreCase) ?? false);
if (httpsConfigured)
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed so the integration test host (WebApplicationFactory) can reference the entry point.
public partial class Program { }
