using FCG.Catalog.Api.Middleware;
using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Consumers;
using FCG.Catalog.Infrastructure;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(CreateGameCommandHandler).Assembly));

builder.Services.AddInfrastructure(
    builder.Configuration);

#region MassTransit
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PaymentProcessedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig =
            builder.Configuration.GetSection("RabbitMq");

        cfg.Host(rabbitMqConfig["Host"], "/", host =>
        {
            host.Username(
                rabbitMqConfig["Username"]!);

            host.Password(
                rabbitMqConfig["Password"]!);
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

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();