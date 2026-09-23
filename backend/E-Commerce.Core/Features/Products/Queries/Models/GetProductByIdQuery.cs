using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Products_;
using MediatR;

namespace E_commerce.Core.Features.Products.Queries.Models;

public sealed record GetProductByIdQuery(int Id) : IRequest<Response<ProductPageViewDto>>;
