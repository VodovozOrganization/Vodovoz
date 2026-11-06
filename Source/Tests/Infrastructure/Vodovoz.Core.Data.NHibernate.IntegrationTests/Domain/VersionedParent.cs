using QS.DomainModel.Entity;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Версионный родитель: блокировка увеличивает версию.
	/// </summary>
	public class VersionedParent : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int Version { get; set; }
		public virtual string Name { get; set; } = "versioned";
		public virtual IList<VersionedParentChild> Items { get; set; } = new List<VersionedParentChild>();
	}
}
