using System;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;

namespace Vodovoz.JournalViewers
{
	//FIXME Временно. Для возможности открывать старые диалоги из отдельного проекта для моделей представления
	public class UndeliveriesViewOpener : IUndeliveriesViewOpener
	{
		//отрытие журнала недовоза на конкретном недовозе из диалога штрафов
		public void OpenFromFine(ITdiTab tab, Order oldOrder, DateTime? deliveryDate, UndeliveryStatus undeliveryStatus)
		{
			UndeliveriesView dlg = new UndeliveriesView();
			dlg.HideFilterAndControls();
			dlg.UndeliveredOrdersFilter.SetAndRefilterAtOnce(
				x => x.RestrictOldOrder = oldOrder,
				x => x.RestrictOldOrderStartDate = deliveryDate,
				x => x.RestrictOldOrderEndDate = deliveryDate,
				x => x.RestrictUndeliveryStatus = undeliveryStatus
			);
			tab.TabParent.AddSlaveTab(tab, dlg);
		}
	}
}
