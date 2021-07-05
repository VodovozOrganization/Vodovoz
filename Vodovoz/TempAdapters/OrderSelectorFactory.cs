using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		public IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses)
		{
			OrderForMovDocJournalFilterViewModel orderFilterVM = new OrderForMovDocJournalFilterViewModel
			{
				IsOnlineStoreOrders = IsOnlineStoreOrders, OrderStatuses = orderStatuses
			};
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			OrderForMovDocJournalViewModel vm = new OrderForMovDocJournalViewModel(
				journalActions,
				orderFilterVM,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			) 
			{
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}
	}
}
