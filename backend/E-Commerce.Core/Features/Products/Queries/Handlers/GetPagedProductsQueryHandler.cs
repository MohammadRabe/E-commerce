using E_commerce.Core.Bases;
using E_commerce.Core.Features.Products.Queries.Models;
using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Wrappers;
using E_commerce.Service.Abstraction;
using MediatR;

namespace E_commerce.Core.Features.Products.Queries.Handlers;

public sealed class GetPagedProductsQueryHandler
    : ResponseHandler, IRequestHandler<GetPagedProductsQuery, Response<PagedList<ProductListDto>>>
{

    private readonly IProductService _prodService;

    public GetPagedProductsQueryHandler(IProductService prodService)
    {
        _prodService = prodService;
    }

    public async Task<Response<PagedList<ProductListDto>>> Handle(
        GetPagedProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await _prodService.GetPagedListAsync(request.PageNumber, request.PageSize, request.Search, cancellationToken,
            product => product.Category,prod => prod.ImagePaths);



        return Success(result);
    }
}
