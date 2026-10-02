using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservae.Models;
using Reservae.Models.Enums;

namespace Reservae.Data.Seed;

public sealed class DemoDataSeeder(
    ApplicationDbContext db,
    UserManager<User> users,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<DemoDataSeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
    };

    private string SeedDirectory => Path.Combine(environment.ContentRootPath, "Data", "Seed");
    private string WebRoot => environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");

    public async Task ExportAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken)
            : null;

        var data = new DemoSeedData(1, DateTime.UtcNow,
            await db.Users.AsNoTracking().OrderBy(x => x.Id).Select(x => new SeedUser(
                x.Id, x.Name, x.Email!, x.UserName!, x.EmailConfirmed, x.CreatedAt, x.UpdatedAt)).ToListAsync(cancellationToken),
            await db.Spaces.AsNoTracking().OrderBy(x => x.Id).Select(x => new SeedSpace(
                x.Id, x.OwnerId, x.Address, x.PricePerSpot, x.IsActive, x.Title, x.Description,
                (int)x.Category, x.CoverImagePath, x.CreatedAt, x.UpdatedAt)).ToListAsync(cancellationToken),
            await db.AvailabilityRules.AsNoTracking().OrderBy(x => x.Id).Select(x => new SeedRule(
                x.Id, x.SpaceId, (int)x.DayOfTheWeek, x.StartTime, x.EndTime, x.ValidFrom, x.ValidUntil,
                x.CustomPricePerSpot, x.IsActive, x.Capacity, x.SlotDurationMinutes, x.CreatedAt, x.UpdatedAt)).ToListAsync(cancellationToken),
            await db.BookableSlots.AsNoTracking().OrderBy(x => x.Id).Select(x => new SeedSlot(
                x.Id, x.AvailabilityRuleId, x.SpaceId, x.StartsAt, x.EndsAt, x.CustomPricePerSpot,
                x.Capacity, x.IsActive, x.CreatedAt, x.UpdatedAt)).ToListAsync(cancellationToken),
            await db.Bookings.AsNoTracking().OrderBy(x => x.Id).Select(x => new SeedBooking(
                x.Id, x.BookableSlotId, x.UserBookedId, (int)x.Status, x.Quantity, x.CreatedAt, x.UpdatedAt)).ToListAsync(cancellationToken));

        // Validate before replacing the snapshot; legacy invalid rules must be corrected at the source.
        Validate(data);
        Directory.CreateDirectory(SeedDirectory);
        foreach (var image in GetImagePaths(data))
        {
            var source = Path.Combine(WebRoot, "uploads", "images", image);
            if (!File.Exists(source))
                throw new InvalidOperationException($"Imagem do espaço não encontrada: {image}.");
            Directory.CreateDirectory(Path.Combine(SeedDirectory, "images"));
            File.Copy(source, Path.Combine(SeedDirectory, "images", image), overwrite: true);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var path = Path.Combine(SeedDirectory, "demo-data.json");
        await File.WriteAllTextAsync(path + ".tmp", json, cancellationToken);
        File.Move(path + ".tmp", path, overwrite: true);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Seed exportada: {Users} usuários, {Spaces} espaços, {Rules} regras, {Slots} horários e {Bookings} reservas.",
            data.Users.Count, data.Spaces.Count, data.AvailabilityRules.Count, data.BookableSlots.Count, data.Bookings.Count);
    }

    public async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (environment.IsProduction())
            throw new InvalidOperationException("A seed de demonstração não pode ser executada em Production.");

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        if (db.Database.IsNpgsql())
            // Serialize first-run seeding when multiple API instances start together.
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(7265736572766165)", cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken) || await db.Spaces.AnyAsync(cancellationToken)
            || await db.AvailabilityRules.AnyAsync(cancellationToken) || await db.BookableSlots.AnyAsync(cancellationToken)
            || await db.Bookings.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed ignorada: o banco já possui dados. Nenhum registro será alterado.");
            return false;
        }

        var json = await File.ReadAllTextAsync(Path.Combine(SeedDirectory, "demo-data.json"), cancellationToken);
        var data = JsonSerializer.Deserialize<DemoSeedData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Arquivo da seed vazio.");
        Validate(data);
        var password = configuration["DemoSeed:Password"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configure DemoSeed:Password para criar os usuários de demonstração.");
        foreach (var image in GetImagePaths(data))
            if (!File.Exists(Path.Combine(SeedDirectory, "images", image)))
                throw new InvalidOperationException($"Imagem ausente na seed: {image}.");

        foreach (var row in data.Users)
        {
            var user = new User
            {
                Id = row.Id, Name = row.Name, Email = row.Email, UserName = row.UserName,
                EmailConfirmed = row.EmailConfirmed, CreatedAt = row.CreatedAt, UpdatedAt = row.UpdatedAt
            };
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Não foi possível criar o usuário da seed: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        foreach (var row in data.Spaces)
        {
            var space = new Space(row.OwnerId, row.Address, row.Title, row.Description)
            {
                Id = row.Id, CreatedAt = row.CreatedAt, UpdatedAt = row.UpdatedAt
            };
            space.ChangePrice(row.PricePerSpot);
            space.ChangeCategory((CategoryEnum)row.Category);
            space.SetActive(row.IsActive);
            if (row.CoverImagePath is not null) space.ChangeCoverImage(row.CoverImagePath);
            db.Spaces.Add(space);
        }
        foreach (var row in data.AvailabilityRules)
        {
            var rule = CreateRule(row);
            rule.Id = row.Id;
            rule.CreatedAt = row.CreatedAt;
            rule.UpdatedAt = row.UpdatedAt;
            rule.SetActive(row.IsActive);
            db.AvailabilityRules.Add(rule);
        }
        foreach (var row in data.BookableSlots)
        {
            var slot = row.AvailabilityRuleId is int ruleId
                ? BookableSlot.FromRule(ruleId, row.SpaceId, row.StartsAt, row.EndsAt, row.CustomPricePerSpot, row.Capacity)
                : BookableSlot.CreateStandalone(row.SpaceId, row.StartsAt, row.EndsAt, row.CustomPricePerSpot, row.Capacity);
            slot.Id = row.Id;
            slot.CreatedAt = row.CreatedAt;
            slot.UpdatedAt = row.UpdatedAt;
            slot.SetActive(row.IsActive);
            db.BookableSlots.Add(slot);
        }
        foreach (var row in data.Bookings)
            db.Bookings.Add(new Booking(row.BookableSlotId, row.UserBookedId, (BookingStatusEnum)row.Status, row.Quantity)
            {
                Id = row.Id, CreatedAt = row.CreatedAt, UpdatedAt = row.UpdatedAt
            });

        await db.SaveChangesAsync(cancellationToken);
        if (db.Database.IsNpgsql())
            // Explicit seed IDs do not advance PostgreSQL identity sequences.
            await db.Database.ExecuteSqlRawAsync("""
                SELECT setval(pg_get_serial_sequence('"Spaces"', 'Id'), COALESCE(MAX("Id"), 1), COUNT(*) > 0) FROM "Spaces";
                SELECT setval(pg_get_serial_sequence('"AvailabilityRules"', 'Id'), COALESCE(MAX("Id"), 1), COUNT(*) > 0) FROM "AvailabilityRules";
                SELECT setval(pg_get_serial_sequence('"BookableSlots"', 'Id'), COALESCE(MAX("Id"), 1), COUNT(*) > 0) FROM "BookableSlots";
                SELECT setval(pg_get_serial_sequence('"Bookings"', 'Id'), COALESCE(MAX("Id"), 1), COUNT(*) > 0) FROM "Bookings";
                """, cancellationToken);

        foreach (var image in GetImagePaths(data))
        {
            var destination = Path.Combine(WebRoot, "uploads", "images", image);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            if (!File.Exists(destination)) File.Copy(Path.Combine(SeedDirectory, "images", image), destination);
        }
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Seed aplicada com os IDs e as datas originais. Usuários: {Users}; espaços: {Spaces}; regras: {Rules}; horários: {Slots}; reservas: {Bookings}.",
            data.Users.Count, data.Spaces.Count, data.AvailabilityRules.Count, data.BookableSlots.Count, data.Bookings.Count);
        return true;
    }

    private static AvailabilityRule CreateRule(SeedRule row)
        => new(row.SpaceId, (DayOfTheWeekEnum)row.DayOfTheWeek, row.StartTime, row.EndTime,
            row.ValidFrom, row.ValidUntil, row.CustomPricePerSpot, row.Capacity, row.SlotDurationMinutes);

    private static IEnumerable<string> GetImagePaths(DemoSeedData data)
    {
        foreach (var path in data.Spaces.Select(x => x.CoverImagePath).OfType<string>().Distinct())
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && (uri.Scheme == "https" || uri.Scheme == "http"))
                continue;
            const string prefix = "/uploads/images/";
            if (!path.StartsWith(prefix, StringComparison.Ordinal)
                || path[prefix.Length..].IndexOfAny(['/', '\\']) >= 0
                || string.IsNullOrWhiteSpace(path[prefix.Length..])
                || path[prefix.Length..] is "." or "..")
                throw new InvalidOperationException("Caminho de imagem inválido na seed.");
            yield return path[prefix.Length..];
        }
    }

    private static void Validate(DemoSeedData data)
    {
        if (data.Version != 1) throw new InvalidOperationException("Versão da seed não suportada.");
        var users = data.Users.Select(x => x.Id).ToHashSet();
        var spaces = data.Spaces.Select(x => x.Id).ToHashSet();
        var rules = data.AvailabilityRules.ToDictionary(x => x.Id);
        var slots = data.BookableSlots.Select(x => x.Id).ToHashSet();
        if (users.Count != data.Users.Count || spaces.Count != data.Spaces.Count
            || slots.Count != data.BookableSlots.Count || data.Bookings.Select(x => x.Id).Distinct().Count() != data.Bookings.Count)
            throw new InvalidOperationException("A seed contém IDs duplicados.");
        foreach (var user in data.Users)
            if (string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.UserName))
                throw new InvalidOperationException("Todo usuário da seed deve ter ID, e-mail e nome de usuário.");
        foreach (var space in data.Spaces)
            if (!users.Contains(space.OwnerId)) throw new InvalidOperationException($"Dono ausente para o espaço {space.Id}.");
        foreach (var rule in data.AvailabilityRules)
        {
            if (!spaces.Contains(rule.SpaceId)) throw new InvalidOperationException($"Espaço ausente para a regra {rule.Id}.");
            try { _ = CreateRule(rule); }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"Corrija a regra {rule.Id} antes de exportar/aplicar a seed: {exception.Message}", exception);
            }
        }
        foreach (var slot in data.BookableSlots)
            if (!spaces.Contains(slot.SpaceId) || (slot.AvailabilityRuleId is int ruleId
                && (!rules.TryGetValue(ruleId, out var rule) || rule.SpaceId != slot.SpaceId)))
                throw new InvalidOperationException($"Relação inválida para o horário {slot.Id}.");
        foreach (var booking in data.Bookings)
            if (!users.Contains(booking.UserBookedId) || !slots.Contains(booking.BookableSlotId))
                throw new InvalidOperationException($"Relação inválida para a reserva {booking.Id}.");
    }
}
