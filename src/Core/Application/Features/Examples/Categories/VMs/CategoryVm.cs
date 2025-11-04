namespace Application.Features.Examples.Categories.VMs
{
    /// <summary>
    /// View Model for Category entities.
    /// Represents category data for API responses.
    /// Immutable record type for better performance and thread safety.
    /// </summary>
    public record CategoryVm(
        Guid Id,
        string Name,
        string? Image
    );
}
