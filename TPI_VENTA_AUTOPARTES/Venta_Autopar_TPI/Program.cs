
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Venta_Autopar_TPI.Core.Repositories.Implementations;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Implementations;
using Venta_Autopar_TPI.Core.Services.Interfaces;
using Venta_Autopar_TPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Registrar controladores
builder.Services.AddControllers();

// Configurar CORS para permitir el acceso desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7283")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Inyectar dependencias de autopartes
builder.Services.AddScoped<IAutoparteRepository, AutoparteRepository>();
builder.Services.AddScoped<IAutoparteService, AutoparteService>();
builder.Services.AddScoped<IHistorialPrecioRepository, HistorialPrecioRepository>();

// Inyectar dependencias de facturas
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

// Inyectar dependencias de dashboards
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Inyectar dependencias de autenticación
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
//dependencias de clientes
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
//dependencias de movimiento stock
builder.Services.AddScoped<IMovimientoStockRepository, MovimientoStockRepository>();
builder.Services.AddScoped<IMovimientoStockService, MovimientoStockService>();

// Registrar el contexto de Entity Framework
builder.Services.AddDbContext<AutopartesContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Obtener la clave secreta de JWT
var secretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "No se configuró la clave secreta de JWT.");
}

var keyBytes = Convert.FromBase64String(secretKey);

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ClockSkew = TimeSpan.Zero
    };
});

// Configurar Swagger con autenticación JWT
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el Access Token generado al iniciar sesión."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configurar Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitar CORS antes de autenticación y autorización
app.UseCors("FrontendPolicy");

// Configurar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
