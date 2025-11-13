using Application.Features.Examples.Categories.VMs;

namespace Application.Features.Examples.Products.VMs
{
    /// <summary>
    /// View Model for Product entities with full category information.
    /// Represents product data with nested category details for API responses.
    /// Immutable record type for better performance and thread safety.
    /// </summary>
    public record ProductWithCategoryVm(
        Guid Id,
        string? Title,
        string? Description,
        string? Image,
        double Price,
        Guid CategoryId,
        string? CategoryName,
        CategoryVm? Category
    );
}
