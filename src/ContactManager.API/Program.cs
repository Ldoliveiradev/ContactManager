using ContactManager.API.Middleware;
using ContactManager.Infrastructure.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

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
