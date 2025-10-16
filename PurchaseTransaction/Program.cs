using Microsoft.EntityFrameworkCore;
using Polly;
using PurchaseTransaction.Data;
using PurchaseTransaction.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PTDbContext>(option =>
    option.UseSqlite("Data Source=app.db"));  //Once we move to production, we can change this to SQL Server or other RDBMS

// Add services to the container.

builder.Services.AddControllers();

var treasuryApiKey = builder.Configuration["TreasuryApi"] ?? "https://fiscal.treasury.gov";


builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri(treasuryApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(new[]
{
    TimeSpan.FromMilliseconds(200),
    TimeSpan.FromMilliseconds(500),
    TimeSpan.FromSeconds(1)
}));

builder.Services.AddScoped<IPurchaseService, PurchaseService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PTDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
