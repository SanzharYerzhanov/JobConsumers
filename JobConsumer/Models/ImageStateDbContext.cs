using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace JobConsumer.Models;

public class ImageStateDbContext : SagaDbContext
{
    public ImageStateDbContext(DbContextOptions<ImageStateDbContext> options) : base(options) {}
    public DbSet<Image> Images { get; set; }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get
        {
            yield return new JobTypeSagaMap(true);
            yield return new JobSagaMap(true);
            yield return new JobAttemptSagaMap(true);
        }
    }
}