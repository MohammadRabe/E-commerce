using E_commerce.Data.Entities;
using E_commerce.Infrastructure.Abstraction;
using E_commerce.Infrastructure.Data;

namespace E_commerce.Infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext context)
    : BaseRepository<Category>(context), ICategoryRepository
{
}
