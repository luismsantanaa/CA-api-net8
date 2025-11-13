namespace Application.Features.Utilities.UploadFiles.Queries.Vm
{
    /// <summary>
    /// View Model for UploadedFile entities.
    /// Represents uploaded file information for API responses.
    /// Immutable record type for better performance and thread safety.
    /// </summary>
    public record UploadedFileVm(
        string? Id,
        string? Name,
        string? Type,
        string? Extension,
        decimal Size,
        string? Path,
        string? Reference,
        string? Comment
    );
}