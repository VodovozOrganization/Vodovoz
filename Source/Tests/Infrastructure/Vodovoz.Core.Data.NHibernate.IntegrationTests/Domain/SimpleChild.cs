using QS.DomainModel.Entity;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class SimpleChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
