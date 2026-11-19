using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Сущность с историей: все виды полей, которые умеет записывать трекер QS.
	/// </summary>
	[HistoryTrace]
	public class HistoryChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }
		public virtual string UniqueCode { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime OccurredAt { get; set; }
		public virtual int? NullableCount { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual BulkChildKind Kind { get; set; }
		public virtual BulkTag Tag { get; set; }
		public virtual DateTime CreatedAt { get; set; }

		[IgnoreHistoryTrace]
		public virtual string IgnoredNote { get; set; }

		public virtual IList<HistorySubItem> SubItems { get; set; } = new List<HistorySubItem>();

		public virtual string Title => $"Строка {Name}";
	}
}
