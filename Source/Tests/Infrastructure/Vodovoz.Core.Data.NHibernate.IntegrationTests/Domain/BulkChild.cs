using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Основная дочерняя сущность: все виды колонок, значения от БД, ссылки и собственная коллекция.
	/// </summary>
	public class BulkChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual BulkParent Parent { get; set; }
		public virtual string Name { get; set; }
		public virtual string UniqueCode { get; set; }
		public virtual string Notes { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime OccurredAt { get; set; }
		public virtual int? NullableCount { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual BulkChildKind Kind { get; set; }
		public virtual BulkAddress Address { get; set; }
		public virtual BulkTag Tag { get; set; }
		public virtual DateTime CreatedAt { get; set; }
		public virtual int NameLength { get; set; }
		public virtual IList<BulkSubItem> SubItems { get; set; } = new List<BulkSubItem>();
	}
}
