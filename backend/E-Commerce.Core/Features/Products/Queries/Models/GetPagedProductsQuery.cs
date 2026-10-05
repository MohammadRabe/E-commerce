using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Products_;
using E_commerce.Data.Entities;
using E_commerce.Data.Wrappers;
using MediatR;

namespace E_commerce.Core.Features.Products.Queries.Models;

public sealed record GetPagedProductsQuery(int PageNumber = 1, int PageSize = 20, string? Search = null)
    : IRequest<Response<PagedList<ProductListDto>>>;
