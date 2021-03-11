using QS.Project.Journal;
using System;

namespace Vodovoz.JournalNodes
{
    public class HistoryTraceObjectNode : JournalEntityNodeBase
    {
        protected HistoryTraceObjectNode(Type entityType) : base(entityType)
        {
            ObjectType = entityType;
        }

        public HistoryTraceObjectNode(Type entityType, string displayName) : this(entityType)
        {
            DisplayName = displayName;
        }

        public override string Title => DisplayName;

        public string ObjectName { get; set; }
        public Type ObjectType { get; set; }
        public string DisplayName { get; set; }
    }
}
