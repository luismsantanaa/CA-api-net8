namespace Application.Features.Examples.Products.VMs
{
    /// <summary>
    /// View Model for Product entities.
    /// Represents product data for API responses.
    /// Immutable record type for better performance and thread safety.
    /// </summary>
    public record ProductVm(
        Guid Id,
        string? Title,
        string? Description,
        string? Image,
        double Price,
        Guid CategoryId,
        string? CategoryName
    );
}
