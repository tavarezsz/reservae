using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reservae.Data;
using Reservae.Models;

namespace Reservae.Tests;

public sealed class AuthorizationApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=unused;Database=unused;Username=unused;Password=unused");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultForbidScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        });
    }

    public HttpClient ClientFor(string? userId)
    {
        var client = CreateClient();
        if (userId is not null) client.DefaultRequestHeaders.Add("X-Test-User", userId);
        return client;
    }

    public AuthorizationApiFactory()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = "owner", UserName = "Owner" }, new User { Id = "client", UserName = "Client" }, new User { Id = "stranger", UserName = "Stranger" });
        db.Spaces.AddRange(new Space("owner", "Address", "Space", "Description") { Id = 1 }, new Space("stranger", "Address", "Other", "Description") { Id = 2 });
        var date = new DateTime(2030, 1, 7, 0, 0, 0, DateTimeKind.Utc);
        db.AvailabilityRules.Add(new AvailabilityRule(1, DayOfTheWeekEnum.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0), date, date.AddDays(30), null, 10, 60) { Id = 1 });
        db.BookableSlots.Add(BookableSlot.CreateStandalone(1, date.AddHours(10), date.AddHours(11), null, 10));
        db.SaveChanges();
        db.Bookings.AddRange(new Booking(1, "client", BookingStatusEnum.Confirmado, 1) { Id = 1 }, new Booking(1, "client", BookingStatusEnum.Confirmado, 1) { Id = 2 });
        db.SaveChanges();
    }
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var userId))
            return Task.FromResult(AuthenticateResult.NoResult());
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
