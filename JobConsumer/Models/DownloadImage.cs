using MassTransit;

namespace JobConsumer.Models;

public class DownloadImage
{
    public Guid ImageId { get; set; }
    public MessageData<byte[]> Image { get; set; }
}