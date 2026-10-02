using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Reservae.Data;
using Reservae.Data.Seed;
using Reservae.Models;

namespace Reservae.Tests;

public sealed class DemoDataSeederTests
{
    private const string DemoPassword = "ReservaeDemo123!";

    [Fact]
    public async Task SnapshotRestoresRelationshipsImagesAndLoginWithoutChangingExistingDataOnRerun()
    {
        await using var setup = await SeedTestSetup.CreateAsync();
        var db = setup.Services.GetRequiredService<ApplicationDbContext>();
        var seed = setup.Services.GetRequiredService<DemoDataSeeder>();
        Assert.True(await seed.ApplyAsync());
        var snapshot = setup.Snapshot;
        Assert.Equal(snapshot.Users.Count, await db.Users.CountAsync());
        Assert.Equal(snapshot.Spaces.Count, await db.Spaces.CountAsync());
        Assert.Equal(snapshot.AvailabilityRules.Count, await db.AvailabilityRules.CountAsync());
        Assert.Equal(snapshot.BookableSlots.Count, await db.BookableSlots.CountAsync());
        Assert.Equal(snapshot.Bookings.Count, await db.Bookings.CountAsync());

        var users = setup.Services.GetRequiredService<UserManager<User>>();
        foreach (var row in snapshot.Users)
        {
            var user = await users.FindByIdAsync(row.Id);
            Assert.NotNull(user);
            Assert.Equal(row.Email, user.Email);
            Assert.True(await users.CheckPasswordAsync(user, DemoPassword));
        }
        foreach (var row in snapshot.Spaces)
        {
            var space = await db.Spaces.Include(x => x.Owner).SingleAsync(x => x.Id == row.Id);
            Assert.Equal(row.OwnerId, space.Owner.Id);
            Assert.Equal(row.CoverImagePath, space.CoverImagePath);
            if (row.CoverImagePath?.StartsWith("/uploads/images/") == true)
                Assert.True(File.Exists(Path.Combine(setup.Root, "wwwroot", row.CoverImagePath.TrimStart('/'))));
        }
        foreach (var row in snapshot.Bookings)
        {
            var booking = await db.Bookings.Include(x => x.UserBooked).Include(x => x.BookableSlot)
                .SingleAsync(x => x.Id == row.Id);
            Assert.Equal(row.UserBookedId, booking.UserBooked.Id);
            Assert.Equal(row.BookableSlotId, booking.BookableSlot.Id);
        }
        var firstSpace = await db.Spaces.OrderBy(x => x.Id).FirstAsync();
        firstSpace.Title = "Título alterado depois da seed";
        await db.SaveChangesAsync();
        Assert.False(await seed.ApplyAsync());
        Assert.Equal("Título alterado depois da seed", (await db.Spaces.FindAsync(firstSpace.Id))!.Title);
        Assert.Equal(snapshot.Bookings.Count, await db.Bookings.CountAsync());

        // The PostgreSQL run checks the identity sequences after explicit seed IDs.
        var newSpace = new Space(snapshot.Users[0].Id, "Novo endereço", "Novo espaço", "Descrição");
        db.Spaces.Add(newSpace);
        await db.SaveChangesAsync();
        Assert.True(newSpace.Id > snapshot.Spaces.Max(x => x.Id));
        var newRule = new AvailabilityRule(newSpace.Id, DayOfTheWeekEnum.Monday,
            new TimeOnly(9, 0), new TimeOnly(10, 0), DateTime.UtcNow, DateTime.UtcNow.AddYears(1), null, 5, 60);
        db.AvailabilityRules.Add(newRule);
        await db.SaveChangesAsync();
        Assert.True(newRule.Id > snapshot.AvailabilityRules.Max(x => x.Id));
        var newSlot = BookableSlot.CreateStandalone(newSpace.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), null, 5);
        db.BookableSlots.Add(newSlot);
        await db.SaveChangesAsync();
        Assert.True(newSlot.Id > snapshot.BookableSlots.Max(x => x.Id));
        var newBooking = new Booking(newSlot.Id, snapshot.Users[0].Id, BookingStatusEnum.Confirmado, 1);
        db.Bookings.Add(newBooking);
        await db.SaveChangesAsync();
        Assert.True(newBooking.Id > snapshot.Bookings.Max(x => x.Id));
    }

    [Fact]
    public async Task ExportExcludesCredentialsAndKeepsDatesAndIds()
    {
        await using var setup = await SeedTestSetup.CreateAsync();
        var seed = setup.Services.GetRequiredService<DemoDataSeeder>();
        await seed.ApplyAsync();
        await seed.ExportAsync();
        var json = await File.ReadAllTextAsync(Path.Combine(setup.Root, "Data", "Seed", "demo-data.json"));
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("securityStamp", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        var exported = JsonSerializer.Deserialize<DemoSeedData>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(setup.Snapshot.Users, exported.Users);
        Assert.Equal(setup.Snapshot.Spaces, exported.Spaces);
        Assert.Equal(setup.Snapshot.AvailabilityRules, exported.AvailabilityRules);
        Assert.Equal(setup.Snapshot.BookableSlots, exported.BookableSlots);
        Assert.Equal(setup.Snapshot.Bookings, exported.Bookings);
    }

    [Fact]
    public async Task MissingPasswordDoesNotPopulateDatabase()
    {
        await using var setup = await SeedTestSetup.CreateAsync(password: null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => setup.Services.GetRequiredService<DemoDataSeeder>().ApplyAsync());
        Assert.False(await setup.Services.GetRequiredService<ApplicationDbContext>().Users.AnyAsync());
    }

    [Fact]
    public async Task ProductionRejectsDemoSeed()
    {
        await using var setup = await SeedTestSetup.CreateAsync(environment: "Production");
        await Assert.ThrowsAsync<InvalidOperationException>(() => setup.Services.GetRequiredService<DemoDataSeeder>().ApplyAsync());
        Assert.False(await setup.Services.GetRequiredService<ApplicationDbContext>().Users.AnyAsync());
    }

    private sealed class SeedTestSetup : IAsyncDisposable
    {
        private readonly ServiceProvider provider;
        private readonly AsyncServiceScope scope;
        public IServiceProvider Services => scope.ServiceProvider;
        public string Root { get; }
        public DemoSeedData Snapshot { get; }

        private SeedTestSetup(ServiceProvider provider, AsyncServiceScope scope, string root, DemoSeedData snapshot)
            => (this.provider, this.scope, Root, Snapshot) = (provider, scope, root, snapshot);

        public static async Task<SeedTestSetup> CreateAsync(string? password = DemoPassword, string environment = "Development")
        {
            var source = Path.Combine(AppContext.BaseDirectory, "Data", "Seed");
            var root = Path.Combine(Path.GetTempPath(), "reservae-seed-test-" + Guid.NewGuid().ToString("N"));
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(root, "Data", "Seed", Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }
            var snapshot = JsonSerializer.Deserialize<DemoSeedData>(await File.ReadAllTextAsync(Path.Combine(source, "demo-data.json")),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["DemoSeed:Password"] = password }).Build());
            services.AddSingleton<IWebHostEnvironment>(new SeedEnvironment(root, environment));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connection = Environment.GetEnvironmentVariable("RESERVAE_SEED_TEST_CONNECTION");
                if (string.IsNullOrWhiteSpace(connection)) options.UseInMemoryDatabase(root);
                else
                {
                    var builder = new NpgsqlConnectionStringBuilder(connection)
                    {
                        Database = "reservae_seed_test_" + Guid.NewGuid().ToString("N")
                    };
                    options.UseNpgsql(builder.ConnectionString);
                }
            });
            services.AddIdentityCore<User>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<DemoDataSeeder>();
            var provider = services.BuildServiceProvider();
            var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (db.Database.IsRelational()) await db.Database.MigrateAsync();
            else await db.Database.EnsureCreatedAsync();
            return new SeedTestSetup(provider, scope, root, snapshot);
        }

        public async ValueTask DisposeAsync()
        {
            var db = Services.GetRequiredService<ApplicationDbContext>();
            // Each setup creates its own database; never reuse the source database.
            await db.Database.EnsureDeletedAsync();
            await scope.DisposeAsync();
            await provider.DisposeAsync();
            Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class SeedEnvironment(string root, string environment) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environment;
        public string ApplicationName { get; set; } = "Reservae";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(root, "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
