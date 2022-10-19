using System;
using QS.Tdi;
using Vodovoz.Domain.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IUndeliveredOrdersJournalOpener
	{
		void OpenFromFine(ITdiTab tab, Order oldOrder, DateTime? deliveryDate, UndeliveryStatus undeliveryStatus);
	}
}
