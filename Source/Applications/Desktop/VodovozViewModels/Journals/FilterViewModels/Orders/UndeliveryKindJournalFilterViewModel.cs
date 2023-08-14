using System.Collections.Generic;
using QS.Project.Filter;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveryKindJournalFilterViewModel : FilterViewModelBase<UndeliveryKindJournalFilterViewModel>
	{
		private UndeliveryObject _undeliveryObject;

		public UndeliveryKindJournalFilterViewModel()
		{
			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>().List();
		}

		public UndeliveryObject UndeliveryObject
		{
			get => _undeliveryObject;
			set => UpdateFilterField(ref _undeliveryObject, value);
		}

		public IList<UndeliveryObject> UndeliveryObjects { get; }
	}
}
