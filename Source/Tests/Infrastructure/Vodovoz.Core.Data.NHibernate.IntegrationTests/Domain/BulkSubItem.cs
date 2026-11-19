using QS.DomainModel.Entity;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkSubItem : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual BulkChild Child { get; set; }
	}
}
