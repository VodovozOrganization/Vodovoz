using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Заголовок записи истории содержит Id: проверяет, что берётся настоящий Id из БД.
	/// </summary>
	[HistoryTrace]
	public class HistoryIdTitleChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }

		public virtual string Title => $"Запись {Id}";
	}
}
