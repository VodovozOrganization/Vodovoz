using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.TempAdapters
{
	public interface IOrderSelectorFactory
	{
		IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses);
		IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory(OrderJournalFilterViewModel filterViewModel = null);
		IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector();
		IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector();
	}
}
