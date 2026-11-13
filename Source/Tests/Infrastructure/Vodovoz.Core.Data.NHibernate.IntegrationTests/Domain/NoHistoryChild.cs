using QS.DomainModel.Entity;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Сущность без атрибута истории.
	/// </summary>
	public class NoHistoryChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }
	}
}
