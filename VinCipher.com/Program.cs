

using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinCario.Services;
using Infrastructure.APIs.VinRush.Models;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using Services.DataServices;
using Services.Interfaces;

using VinCipher.Model;
using VinCipher.Services;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

builder.Services.AddDbContext<PlaygroundDbContext>(options =>
    options.UseSqlite("Data Source=playground.db"));
builder.Services.AddSingleton<BaseApiProvider, VincarioProvider>(_ => new VincarioProvider(configuration));
builder.Services.AddSingleton<BaseApiProvider, VinAuditProvider>(_ => new VinAuditProvider(configuration));
builder.Services.AddHttpClient<VinRushScrapper>();
builder.Services.AddHttpClient<VinCypScrapper>();
builder.Services.AddHttpClient<FreeVinDecoderScrapper>();
builder.Services.AddSingleton<IScrappableSource, VinRusUrlGenerator>();
builder.Services.AddSingleton<TokensProvider>(_ => new TokensProvider(configuration));
builder.Services.AddSingleton<VinDecodeCache>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VincarioApiClient>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VinAuditApiClient>();
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

builder.Services.AddSingleton<AdminEventBus>();

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
        var rootPassword = configuration["Admin:RootPassword"]
                ?? throw new InvalidOperationException("Admin:RootPassword must be configured. Do not use a hardcoded default.");
        var rootUser = new VinCipher.Model.Playground.AdminUser
        {
            Id = Guid.NewGuid(),
            Username = configuration["Admin:RootUsername"] ?? "root",
            IsRoot = true
        };
        rootUser.SetPassword(rootPassword);
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
if (app.Environment.IsDevelopment())
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

// Block direct access to .html files in wwwroot
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true)
    {
        ctx.Response.StatusCode = 404;
        return;
    }
    await next();
});

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Static files with build-time compression (gzip + brotli), ETags and cache fingerprinting
app.MapStaticAssets();

app.Run();
