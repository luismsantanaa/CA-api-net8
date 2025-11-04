using System.Text.Json;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Persistence.Constants;

namespace Persistence.InternalModels
{
    internal class AuditEntry
    {
        private readonly DateTime _localTime;
        public AuditEntry(EntityEntry entry, DateTime localTime)
        {
            Entry = entry;
            _localTime = localTime;
        }

        public EntityEntry Entry { get; }
        public required Guid UserId { get; set; }
        public required string TableName { get; set; }
        public Dictionary<string, object>? KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>? OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>? NewValues { get; } = new Dictionary<string, object>();
        public required AuditType AuditType { get; set; }
        public List<string>? ChangedColumns { get; } = new List<string>();

        public AuditLog ToAudit()
        {
            var audit = new AuditLog
            {
                UserId = UserId,
                Type = AuditType.ToString(),
                TableName = TableName,
                DateTime = _localTime,
                PrimaryKey = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues!.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues!.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
                AffectedColumns = ChangedColumns!.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns)
            };
            return audit;
        }
    }
}
