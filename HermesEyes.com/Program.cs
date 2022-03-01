
using Domaine.Entities;

using HermesEyes.com.Model;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.VinCario.Services;
using Infrastructure.Contexts;

using Microsoft.EntityFrameworkCore;

using Services.DataServices;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddDbContext<HermesContext>(option =>
{
    option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddSingleton<BaseApiProvider, VincarioProvider>(_ => new VincarioProvider(configuration));
builder.Services.AddSingleton<TokensProvider>(_ => new TokensProvider(configuration));
builder.Services.AddScoped<ICrudServices, VinToSearchServices>();
builder.Services.AddScoped<ICarService, BaseCarServices>();
builder.Services.AddHttpClient<BaseApiProviderClient<CarBase>, VincarioApiClient>();
builder.Services.AddHttpClient<IHttpConsumtionServices, DecodedCarBaseService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
