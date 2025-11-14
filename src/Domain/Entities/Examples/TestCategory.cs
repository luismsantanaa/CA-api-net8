using Domain.Base;

namespace Domain.Entities.Examples
{
    public class TestCategory : AuditableEntity
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

        // NOTA: La colección de navegación Products fue removida debido a un bug conocido
        // en EF Core 8/9 (NullReferenceException en FindCollectionMapping).
        // La relación existe en la base de datos mediante foreign key (CategoryId en TestProduct).
        // Para obtener los productos de una categoría, usar queries explícitas.
    }
}
