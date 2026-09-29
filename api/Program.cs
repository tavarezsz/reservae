using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.Interfaces;
using Reservae.Repository;
using Reservae.Service;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reservae API",
        Version = "v1",
        Description = "API para gerenciamento de espaços, disponibilidades e reservas."
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Informe somente o accessToken retornado por /auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling =
            JsonUnmappedMemberHandling.Disallow;
    });
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection para o PostgreSQL.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Web", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
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
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<IBookingRepository>(services =>
    services.GetRequiredService<BookingRepository>());
builder.Services.AddScoped<IBaseRepository<Booking>>(services =>
    services.GetRequiredService<BookingRepository>());
builder.Services.AddScoped<SpaceService>();
builder.Services.AddScoped<AvailabilityRuleService>();
builder.Services.AddScoped<BookableSlotService>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Reservae API v1");
        options.DocumentTitle = "Reservae API";
        options.EnablePersistAuthorization();
        options.DisplayRequestDuration();
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Web");
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
