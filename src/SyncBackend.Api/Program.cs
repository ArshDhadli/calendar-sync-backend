using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SyncBackend.Api.Auth;
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

// Bind the "Jwt" config section (merged from appsettings.json + .Development.json)
// to a typed JwtSettings object, and register the service that issues tokens.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtTokenService>();

// Read the same settings here so the *validation* middleware trusts tokens signed
// with our key. This is the "verify" half that makes [Authorize] actually work.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Every rule here must pass or the token is rejected before your controller runs.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true, // reject expired tokens
            ValidateIssuerSigningKey = true, // the crux: verify the signature
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        };
    });

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

// Order matters: authentication (who are you? — reads/validates the token) must run
// before authorization (are you allowed? — enforces [Authorize]).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
