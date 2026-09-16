using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.Interfaces;
using Reservae.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("/auth").MapIdentityApi<User>();
app.MapGet("/auth/me", async (System.Security.Claims.ClaimsPrincipal principal, UserManager<User> users) =>
{
    var user = await users.GetUserAsync(principal);
    return user is null
        ? Results.Unauthorized()
        : Results.Ok(new { user.Id, user.Name, user.Email });
}).RequireAuthorization();

app.MapGet("/test/spaces", async (
    IBaseRepository<Space> repository) =>
{
    var test = new Space(
        "faf079b7-2389-44a2-833c-c08614d3b674",
        "Endereço de teste",
        "Casa Aurora",
        "Espaço criado para testar o repositório",
        "/images/test-space.jpg");

    test.ChangeCategory(Reservae.Models.Enums.CategoryEnum.Outros);
    test.ChangePrice(100m);

    var addedSpace = await repository.AddAsync(test);
    var all = await repository.GetAllAsync();
    return Results.Ok(all);
});

app.Run();
