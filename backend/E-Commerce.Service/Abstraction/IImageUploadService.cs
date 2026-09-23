namespace E_commerce.Service.Abstraction;

public interface IImageUploadService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}
