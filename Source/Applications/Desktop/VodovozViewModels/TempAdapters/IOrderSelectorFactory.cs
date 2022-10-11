using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IOrderSelectorFactory
	{
		IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses);
		IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector();
		IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector();
	}
}
