
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
builder.Services.AddSingleton<BaseApiProvider, VincarioProvider>(_ => new VincarioProvider(configuration));
builder.Services.AddSingleton<BaseApiProvider, VinAuditProvider>(_ => new VinAuditProvider(configuration));
builder.Services.AddScoped<VinRushScrapper>();
builder.Services.AddSingleton<IScrappableSource, VinRusUrlGenerator>();
builder.Services.AddSingleton<TokensProvider>(_ => new TokensProvider(configuration));
builder.Services.AddScoped<ICrudServices, VinToSearchServices>();
builder.Services.AddScoped<ICarService, BaseCarServices>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VincarioApiClient>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VinAuditApiClient>();

builder.Services.AddHttpClient<IHttpConsumtionServices, DecodedCarBaseService>();
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://learn.microsoft.com/aspnet/core/fundamentals/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html"))
        {
            ctx.Context.Response.ContentType = "text/html; charset=utf-8";
        }
    }
});

app.UseAuthorization();

app.MapControllers();

app.Run();
