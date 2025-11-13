using System.ComponentModel.DataAnnotations.Schema;

namespace Security.Entities
{
    [Table("VW_EMPLOYEE_NIC")]
    public class VwEmployee
    {
        public int Id { get; set; }
        public required string Codigo { get; set; }
        public required string Nombre { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string? Departamento { get; set; }
        public required string? CargoNombre { get; set; }
        public required string? Oficina { get; set; }
    }
}
