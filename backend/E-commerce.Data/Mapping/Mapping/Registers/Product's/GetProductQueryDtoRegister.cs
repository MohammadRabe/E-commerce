using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;
using Mapster;

namespace E_commerce.Data.Mapping.Registers.Products_;

public sealed class GetProductQueryDtoRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductListDto>()
            .MapWith(source => new ProductListDto(
                source.Id,
                source.Title,
                source.Price,
                source.Discount,
                source.Category.Name,
                source.Rating,
                source.ImagePaths.OrderBy(image => image.Id).Select(image => image.Url).FirstOrDefault()));
    }
}
