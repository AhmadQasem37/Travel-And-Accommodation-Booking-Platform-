using TAABP.Api;
using TAABP.Application;
using TAABP.Infrastructure;
using TAABP.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services from each layer
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

var app = builder.Build();

// Seed database
await DataSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline
app.UseApi();

app.Run();