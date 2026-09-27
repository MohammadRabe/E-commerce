using System.Security.Claims;
using E_commerce.Infrastructure.Data;
using E_commerce.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Api.Controllers.Likes;

[ApiController]
[Authorize]
[Route("api/v1/userLikes")]
public sealed class UserLikesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var productIds = await db.UserLikes.AsNoTracking()
            .Where(like => like.UserId == userId)
            .Select(like => like.ProductId)
            .ToListAsync(cancellationToken);

        return Ok(productIds);
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> AddLike(int productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        if (!await db.Products.AnyAsync(product => product.Id == productId, cancellationToken)) return NotFound();

        var exists = await db.UserLikes.AnyAsync(like => like.UserId == userId && like.ProductId == productId, cancellationToken);
        if (!exists)
        {
            db.UserLikes.Add(new UserLike { UserId = userId, ProductId = productId });
            await db.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveLike(int productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await db.UserLikes
            .Where(like => like.UserId == userId && like.ProductId == productId)
            .ExecuteDeleteAsync(cancellationToken);

        return NoContent();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
}
