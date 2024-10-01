using MassTransit;
using Serilog;
using Serilog.Core;

namespace JobConsumer.Models;

public class DownloadImageConsumer : IJobConsumer<DownloadImage>
{
    private readonly ImageStateDbContext _db;
    public DownloadImageConsumer(ImageStateDbContext db)
    {
        _db = db;
    }
    public async Task Run(JobContext<DownloadImage> context)
    {
        Log.Information("The document was loaded");
        var image = new Image()
        {
            ImageId = context.Job.ImageId,
            ImageData = await context.Job.Image.Value
        };
        _db.Images.Add(image);
        await _db.SaveChangesAsync();
        Log.Information("SAVED The document was saved to db");
    }
}