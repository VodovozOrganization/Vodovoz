using QS.Project.Journal;
using System;

namespace Vodovoz.JournalNodes
{
    public class HistoryTracePropertyNode : JournalEntityNodeBase
    {
        protected HistoryTracePropertyNode(Type entityType) : base(entityType)
        {
            ObjectType = entityType;
        }

        public HistoryTracePropertyNode(Type entityType, string displayName) : this(entityType)
        {
            PropertyName = displayName;
        }

        public override string Title => PropertyName;

        public string PropertyName { get; set; }

        public string PropertyPath { get; set; }

        public Type ObjectType { get; set; }
    }
}
