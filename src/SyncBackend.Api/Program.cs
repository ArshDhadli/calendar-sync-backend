using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncBackend.Api.Data;
using SyncBackend.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the database context. The connection string comes from configuration
// (appsettings.json / environment variables) under the key "Postgres".
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Password hashing. We borrow only this one vetted piece from ASP.NET Identity
// (correct salt + slow PBKDF2 hashing) rather than the whole Identity stack.
// Injected wherever we ask for IPasswordHasher<User>.
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
