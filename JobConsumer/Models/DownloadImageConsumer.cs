using MassTransit;

namespace JobConsumer.Models;

public class DownloadImageConsumer : IJobConsumer<DownloadImage>
{
    private readonly ILogger<DownloadImageConsumer> _logger;
    private readonly ImageStateDbContext _db;
    public DownloadImageConsumer(ImageStateDbContext db, ILogger<DownloadImageConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task Run(JobContext<DownloadImage> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(6));
        _logger.LogInformation("the document was loaded");
        var image = new Image()
        {
            ImageId = context.Job.ImageId,
            ImageData = await context.Job.Image.Value
        };
        _db.Images.Add(image);
        await _db.SaveChangesAsync();
    }
}