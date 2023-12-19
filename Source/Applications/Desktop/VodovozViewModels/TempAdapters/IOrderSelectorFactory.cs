using System;
using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public interface IOrderSelectorFactory
	{
		IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses);
		IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory(OrderJournalFilterViewModel filterViewModel = null);
		IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector();
		IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector(Func<GeoGroup> geoGroupRestrictionFunc);
		OrderJournalViewModel CreateOrderJournalViewModel(OrderJournalFilterViewModel filterViewModel = null);
	}
}
