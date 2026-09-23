using CleanArch.Api.Controllers.Base;
using E_commerce.Core.Features.Categories.Models;
using E_commerce.Data.Dtos.Categories;
using CleanArch.Data.Routing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Api.Controllers.Category;

[ApiController]
public sealed class CategoryController(IMediator mediator) : AppControllerBase(mediator)
{
    [HttpGet(Router.Version1.Category.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new GetCategoriesQuery(), cancellationToken));

    [HttpGet(Router.Version1.Category.GetById)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new GetCategoryQuery(id), cancellationToken));

    [Authorize(Roles = "Admin"), HttpPost(Router.Version1.Category.Create)]
    public async Task<IActionResult> Create(SaveCategoryDto request, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken));

    [Authorize(Roles = "Admin"), HttpPut(Router.Version1.Category.Update)]
    public async Task<IActionResult> Update(int id, SaveCategoryDto request, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new UpdateCategoryCommand(id, request.Name), cancellationToken));

    [Authorize(Roles = "Admin"), HttpDelete(Router.Version1.Category.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken));
}
