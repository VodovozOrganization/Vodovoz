using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Родитель без истории: его записи не должны мешать проверке истории дочерних сущностей.
	/// </summary>
	public class HistoryParent : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; } = "родитель";
		public virtual IObservableList<HistoryChild> Items { get; set; } = new ObservableList<HistoryChild>();
		public virtual IList<HistoryIdTitleChild> IdTitleItems { get; set; } = new List<HistoryIdTitleChild>();
		public virtual IList<NoHistoryChild> NoHistoryItems { get; set; } = new List<NoHistoryChild>();
	}
}
