using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Service;
using Reservae.Service.Mappers;

namespace Reservae.Tests;

public sealed class AvailabilityDurationTests
{
    private static readonly DateTime Date = new(2030, 1, 7, 0, 0, 0, DateTimeKind.Utc);

    private static AvailabilityRule CreateRule(int duration = 60)
        => new(1, DayOfTheWeekEnum.Monday, TimeOnly.MinValue, new TimeOnly(23, 59),
            Date, Date, null, 10, duration);

    [Theory]
    [InlineData(1440)]
    [InlineData(2880)]
    public void CreationRejectsDurationLongerThanWindow(int duration)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CreateRule(duration));

    [Fact]
    public void DurationUpdateRejectsLongerThanWindow()
    {
        var rule = CreateRule();
        Assert.Throws<ArgumentOutOfRangeException>(() => rule.ChangeSlotDuration(1440));
        Assert.Equal(60, rule.SlotDurationMinutes);
    }

    [Fact]
    public void ScheduleUpdateRejectsWindowShorterThanExistingDuration()
    {
        var rule = CreateRule();
        Assert.Throws<ArgumentOutOfRangeException>(() => rule.ChangeSchedule(
            DayOfTheWeekEnum.Monday, new TimeOnly(9, 0), new TimeOnly(9, 30), Date, Date));
        Assert.Equal(TimeOnly.MinValue, rule.StartTime);
    }

    [Fact]
    public void ScheduleAndDurationCanBeReducedTogether()
    {
        var rule = CreateRule();
        new UpdateAvailabilityRuleDTO
        {
            StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30), SlotDurationMinutes = 30
        }.ApplyUpdate(rule);
        Assert.Equal(30, rule.SlotDurationMinutes);
        Assert.Equal(new TimeOnly(9, 0), rule.StartTime);
    }

    [Fact]
    public async Task EndpointsReturnBadRequestForInvalidDurationAndWindow()
    {
        using var factory = new AuthorizationApiFactory();
        using var client = factory.ClientFor("owner");
        var response = await client.PostAsJsonAsync("/api/availability-rule", new
        {
            spaceId = 1, dayOfTheWeek = 0, startTime = "00:00:00", endTime = "23:59:00",
            validFrom = Date, validUntil = Date, capacity = 10, slotDurationMinutes = 1440
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("intervalo", await response.Content.ReadAsStringAsync());

        response = await client.PutAsJsonAsync("/api/availability-rule/1", new { slotDurationMinutes = 1440 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        response = await client.PutAsJsonAsync("/api/availability-rule/1", new { endTime = "09:30:00" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1439, 1)]
    [InlineData(60, 23)]
    [InlineData(30, 47)]
    [InlineData(1440, 0)]
    public async Task AvailabilityStopsAtEndOfDayIncludingLegacyInvalidRules(int duration, int expectedCount)
    {
        using var factory = new AuthorizationApiFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.AvailabilityRules.RemoveRange(db.AvailabilityRules);
        var rule = CreateRule(Math.Min(duration, 1439));
        // Simulate an invalid record saved before the new validation existed.
        if (duration == 1440)
            typeof(AvailabilityRule).GetProperty(nameof(AvailabilityRule.SlotDurationMinutes))!.SetValue(rule, duration);
        db.AvailabilityRules.Add(rule);
        await db.SaveChangesAsync();

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var service = scope.ServiceProvider.GetRequiredService<SpaceService>();
        var slots = await service.GetAvailabilityAsync(1, DateOnly.FromDateTime(Date),
            DateOnly.FromDateTime(Date), cancellationToken: cancellation.Token);
        var generated = slots.Where(slot => slot.AvailabilityRuleId == rule.Id).ToList();
        Assert.Equal(expectedCount, generated.Count);
        Assert.All(generated, slot => Assert.True(slot.EndsAt <= Date.AddHours(23).AddMinutes(59)));
    }
}
