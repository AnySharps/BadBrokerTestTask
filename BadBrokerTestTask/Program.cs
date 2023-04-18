using BadBrokerTestTask.Interfaces;
using BadBrokerTestTask.Services;
using BadBrokerTestTask.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services
    .AddControllers()
    .Services
    .AddHttpClient()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddTransient<IRatesGatewayService, RatesGateWayService>()
    .AddTransient<IRevenueService, RevenueService>()
    .AddSingleton(builder.Configuration.GetSection(nameof(CurrencySettings)).Get<CurrencySettings>())
    .AddSingleton(builder.Configuration.GetSection(nameof(ExchangeRatesApiSettings)).Get<ExchangeRatesApiSettings>())
    .AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger().UseSwaggerUI();


app.UseHttpsRedirection().UseAuthorization();

app.MapControllers();

app.Run();


