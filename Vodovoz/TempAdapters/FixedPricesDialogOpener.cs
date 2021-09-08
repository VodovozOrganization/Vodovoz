using QS.Tdi;
using Vodovoz.Factories;

namespace Vodovoz.TempAdapters
{
	public class FixedPricesDialogOpener : IFixedPricesDialogOpener
	{
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory = new DeliveryPointViewModelFactory();

		public void OpenFixedPricesForSelfDelivery(int counterpartyId)
		{
			CounterpartyDlg counterpartyDlg = new CounterpartyDlg(counterpartyId);
			counterpartyDlg.OpenFixedPrices();
			TDIMain.MainNotebook.AddTab(counterpartyDlg);
		}

		public void OpenFixedPricesForDeliveryPoint(int deliveryPointId)
		{
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(deliveryPointId);
			TDIMain.MainNotebook.AddTab(dpViewModel);
			dpViewModel.OpenFixedPrices();
		}
	}
}
