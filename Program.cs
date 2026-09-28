using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.Interfaces;
using Reservae.Repository;
using Reservae.Service;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling =
            JsonUnmappedMemberHandling.Disallow;
    });
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection para o PostgreSQL.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddScoped(
    typeof(IBaseRepository<>),
    typeof(BaseRepository<>));
builder.Services.AddScoped<AvailabilityRuleRepository>();
builder.Services.AddScoped<IAvailabilityRuleRepository>(services =>
    services.GetRequiredService<AvailabilityRuleRepository>());
builder.Services.AddScoped<IBaseRepository<AvailabilityRule>>(services =>
    services.GetRequiredService<AvailabilityRuleRepository>());
builder.Services.AddScoped<BookableSlotRepository>();
builder.Services.AddScoped<IBookableSlotRepository>(services =>
    services.GetRequiredService<BookableSlotRepository>());
builder.Services.AddScoped<IBaseRepository<BookableSlot>>(services =>
    services.GetRequiredService<BookableSlotRepository>());
builder.Services.AddScoped<SpaceService>();
builder.Services.AddScoped<AvailabilityRuleService>();
builder.Services.AddScoped<BookableSlotService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGroup("/auth").MapIdentityApi<User>();
app.MapGet("/auth/me", async (System.Security.Claims.ClaimsPrincipal principal, UserManager<User> users) =>
{
    var user = await users.GetUserAsync(principal);
    return user is null
        ? Results.Unauthorized()
        : Results.Ok(new { user.Id, user.Name, user.Email });
}).RequireAuthorization();

app.Run();
