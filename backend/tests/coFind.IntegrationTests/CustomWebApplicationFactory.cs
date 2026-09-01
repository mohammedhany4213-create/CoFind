using coFind.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace coFind.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtKey = "integration-test-key-must-be-at-least-32-bytes-long";
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly string? _previousJwtKey;
    private readonly string? _previousJwtIssuer;
    private readonly string? _previousJwtAudience;
    private readonly string? _previousRefreshTokenDays;

    public CustomWebApplicationFactory()
    {
        _previousJwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
        _previousJwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
        _previousJwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience");
        _previousRefreshTokenDays = Environment.GetEnvironmentVariable("Jwt__RefreshTokenDays");

        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "CoFind.Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "CoFind.Client");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", "30");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            _connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
            Environment.SetEnvironmentVariable("Jwt__Key", _previousJwtKey);
            Environment.SetEnvironmentVariable("Jwt__Issuer", _previousJwtIssuer);
            Environment.SetEnvironmentVariable("Jwt__Audience", _previousJwtAudience);
            Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", _previousRefreshTokenDays);
        }

        base.Dispose(disposing);
    }
}