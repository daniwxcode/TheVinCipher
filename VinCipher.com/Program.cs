
using Domaine.Entities;

using VinCipher.Model;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinCario.Services;
using Infrastructure.APIs.VinRush.Models;
using Infrastructure.Contexts;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using Services.DataServices;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddDbContext<VinCipherContext>(option =>
{
    option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddDbContext<PlaygroundDbContext>(options =>
    options.UseSqlite("Data Source=playground.db"));
builder.Services.AddSingleton<BaseApiProvider, VincarioProvider>(_ => new VincarioProvider(configuration));
builder.Services.AddSingleton<BaseApiProvider, VinAuditProvider>(_ => new VinAuditProvider(configuration));
builder.Services.AddScoped<VinRushScrapper>();
builder.Services.AddSingleton<IScrappableSource, VinRusUrlGenerator>();
builder.Services.AddSingleton<TokensProvider>(_ => new TokensProvider(configuration));
builder.Services.AddSingleton<VinDecodeCache>();
builder.Services.AddScoped<ICrudServices, VinToSearchServices>();
builder.Services.AddScoped<ICarService, BaseCarServices>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VincarioApiClient>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VinAuditApiClient>();

builder.Services.AddHttpClient<IHttpConsumtionServices, DecodedCarBaseService>();

builder.Services.AddSingleton(_ =>
{
    var playground = configuration.GetSection("Playground");
    return new PlaygroundRateLimiter(
        playground.GetValue("MaxPerMinute", 5),
        playground.GetValue("MaxPerDay", 20));
});

builder.Services.AddSingleton(_ =>
{
    var maxPerDay = configuration.GetValue("VinDecoder:MaxPerDay", 50);
    return new VinDecoderRateLimiter(maxPerDay);
});

builder.Services.AddSingleton(_ =>
{
    var secret = configuration["Playground:HmacSecret"]
        ?? throw new InvalidOperationException("Playground:HmacSecret is not configured.");
    return new PlaygroundChallenge(secret);
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Learn more about configuring OpenAPI at https://learn.microsoft.com/aspnet/core/fundamentals/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var pgDb = scope.ServiceProvider.GetRequiredService<PlaygroundDbContext>();
    pgDb.Database.Migrate();

    // Seed root admin if not present
    if (!pgDb.AdminUsers.Any(a => a.IsRoot))
    {
        var rootUser = new VinCipher.Model.Playground.AdminUser
        {
            Id = Guid.NewGuid(),
            Username = configuration["Admin:RootUsername"] ?? "root",
            IsRoot = true
        };
        rootUser.SetPassword(configuration["Admin:RootPassword"] ?? "VinCipher@Admin2025!");
        pgDb.AdminUsers.Add(rootUser);
        pgDb.SaveChanges();
    }

    // Seed a sample pending access request if none exist
    if (!pgDb.AccessRequests.Any())
    {
        pgDb.AccessRequests.Add(new VinCipher.Model.Playground.AccessRequest
        {
            Id = Guid.NewGuid(),
            Name = "Jean Dupont",
            Email = "jean.dupont@example.com",
            Phone = "+221771234567",
            PhoneCode = "+221",
            Domain = "concessionnaire",
            Reason = "Je suis concessionnaire automobile à Dakar et je souhaite intégrer le décodage VIN dans mon système de gestion pour vérifier les véhicules importés.",
            Status = "pending"
        });
        pgDb.SaveChanges();
    }

    // Reload active playground tokens into in-memory TokensProvider
    var tokensProvider = scope.ServiceProvider.GetRequiredService<TokensProvider>();
    var activeTokens = pgDb.ApiTokens
        .Where(t => t.IsActive && t.ExpiresAtUtc > DateTime.UtcNow)
        .ToList();
    foreach (var t in activeTokens)
        tokensProvider.AddPlaygroundToken(t.Key, t.ExpiresAtUtc);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
        .WithTheme(ScalarTheme.Kepler)
        .HideDarkModeToggle()
        .WithClientButton(true);
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Block direct access to any remaining .html files in wwwroot
        if (ctx.File.Name.EndsWith(".html"))
        {
            ctx.Context.Response.StatusCode = 404;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
        }
    }
});

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
