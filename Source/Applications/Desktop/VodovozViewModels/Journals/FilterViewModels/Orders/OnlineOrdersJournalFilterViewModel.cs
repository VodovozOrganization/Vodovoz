using QS.Project.Filter;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class OnlineOrdersJournalFilterViewModel : FilterViewModelBase<OnlineOrdersJournalFilterViewModel>
	{
		private OnlineOrderStatus? _onlineOrderStatus;

		public OnlineOrderStatus? OnlineOrderStatus
		{
			get => _onlineOrderStatus;
			set => UpdateFilterField(ref _onlineOrderStatus, value);
		}
	}
}
