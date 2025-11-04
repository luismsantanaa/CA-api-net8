namespace Domain.Base
{
    public class TraceableEntity : AuditableEntity
    {
        public static bool Traceable => true;
    }
}
