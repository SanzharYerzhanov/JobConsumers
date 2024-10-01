using JobConsumer.Models;
using MassTransit;
using MassTransit.MessageData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
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
    x.UsingRabbitMq((context, cfg) =>
    {
        var directoryInfo = new DirectoryInfo(@"C:/Program Files/temp");
        cfg.UseMessageData(new FileSystemMessageDataRepository(directoryInfo));
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username(builder.Configuration["UserSettings:Username"]!);
            h.Password(builder.Configuration["UserSettings:Password"]!);
        });
        cfg.ReceiveEndpoint("image-service", e =>
        {
            e.ConfigureConsumer<DownloadImageConsumer>(context);
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