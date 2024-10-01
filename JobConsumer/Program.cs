using JobConsumer.Models;
using MassTransit;
using MassTransit.MessageData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddDbContext<ImageStateDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("ImageService"));
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DownloadImageConsumer>();
    x.AddJobSagaStateMachines()
        .EntityFrameworkRepository(e =>
        {
            e.ExistingDbContext<ImageStateDbContext>();
        });
    x.AddDelayedMessageScheduler();
    x.UsingRabbitMq((context, cfg) =>
    {
        var directoryInfo = new DirectoryInfo(@"C:/Program Files/temp");
        cfg.UseMessageData(new FileSystemMessageDataRepository(directoryInfo));
        cfg.UseDelayedMessageScheduler();
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username(builder.Configuration["UserSettings:Username"]!);
            h.Password(builder.Configuration["UserSettings:Password"]!);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();
app.MapGet("/", async ([FromServices] IPublishEndpoint endpoint) =>
{
    var bytes = File.ReadAllBytes(@"C:/laptop.jpg");
    await endpoint.Publish<DownloadImage>(new
    {
        ImageId = NewId.NextGuid(),
        Image = bytes
    });
});
app.Run();