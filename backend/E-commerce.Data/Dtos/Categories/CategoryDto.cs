namespace E_commerce.Data.Dtos.Categories;

/// <summary>Category fields returned by catalog endpoints.</summary>
public sealed record CategoryDto(int Id, string Name);

/// <summary>Fields accepted when creating or renaming a category.</summary>
public sealed record SaveCategoryDto(string Name);
