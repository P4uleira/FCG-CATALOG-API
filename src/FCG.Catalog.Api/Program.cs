using FCG.Catalog.Api.Middleware;
using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Consumers;
using FCG.Catalog.Infrastructure;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

#region Controllers

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

        cfg.Host(
            rabbitMqConfig["Host"],
            "/",
            host =>
            {
                host.Username(rabbitMqConfig["Username"]!);
                host.Password(rabbitMqConfig["Password"]!);
            });

        cfg.ReceiveEndpoint(
            "catalog-payment-processed-event",
            endpoint =>
            {
                endpoint.ConfigureConsumer<
                    PaymentProcessedEventConsumer>(context);
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
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapGet("/health", () =>
    Results.Ok(new
    {
        service = "CatalogAPI",
        status = "Healthy"
    }));

#endregion

app.Run();