using System.Security.Claims;
using System.Text;
using FCG.Catalog.Api.Middleware;
using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Consumers;
using FCG.Catalog.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

#region Controllers

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#endregion

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    const string bearerScheme = "Bearer";

    options.AddSecurityDefinition(
        bearerScheme,
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Informe o token JWT recebido no login."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                bearerScheme,
                document)] = []
        });
});

#endregion

#region Authentication

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "A configuração Jwt:Key não foi encontrada.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "A configuração Jwt:Issuer não foi encontrada.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "A configuração Jwt:Audience não foi encontrada.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

#endregion

#region MediatR

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(
        typeof(CreateGameCommandHandler).Assembly);
});

#endregion

#region Infrastructure

builder.Services.AddInfrastructure(
    builder.Configuration);

#endregion

#region MassTransit

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PaymentProcessedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig =
            builder.Configuration.GetSection("RabbitMq");

        var rabbitMqHost =
            rabbitMqConfig["Host"]
            ?? throw new InvalidOperationException(
                "A configuração RabbitMq:Host não foi encontrada.");

        var rabbitMqUsername =
            rabbitMqConfig["Username"]
            ?? throw new InvalidOperationException(
                "A configuração RabbitMq:Username não foi encontrada.");

        var rabbitMqPassword =
            rabbitMqConfig["Password"]
            ?? throw new InvalidOperationException(
                "A configuração RabbitMq:Password não foi encontrada.");

        cfg.Host(
            rabbitMqHost,
            "/",
            host =>
            {
                host.Username(rabbitMqUsername);
                host.Password(rabbitMqPassword);
            });

        cfg.ReceiveEndpoint(
            "catalog-payment-processed-event",
            endpoint =>
            {
                endpoint.ConfigureConsumer<
                    PaymentProcessedEventConsumer>(
                    context);
            });
    });
});

#endregion

var app = builder.Build();

#region Pipeline

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var isRunningInContainer =
    string.Equals(
        Environment.GetEnvironmentVariable(
            "DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
        "/health",
        () => Results.Ok(new
        {
            service = "CatalogAPI",
            status = "Healthy"
        }))
    .AllowAnonymous();

#endregion

app.Run();