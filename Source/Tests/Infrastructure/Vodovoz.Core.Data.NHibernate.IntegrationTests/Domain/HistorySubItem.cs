using QS.DomainModel.Entity;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HistorySubItem : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryChild Child { get; set; }
	}
}
