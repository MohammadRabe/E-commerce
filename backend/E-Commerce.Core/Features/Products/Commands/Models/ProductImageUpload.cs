namespace E_commerce.Core.Features.Products.Commands.Models;

public sealed record ProductImageUpload(Stream Content, string FileName, string ContentType);
