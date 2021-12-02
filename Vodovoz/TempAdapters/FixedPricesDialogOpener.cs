using Fias.Service;
using QS.Tdi;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.TempAdapters
{
	public class FixedPricesDialogOpener : IFixedPricesDialogOpener
	{
		public void OpenFixedPricesForSelfDelivery(int counterpartyId)
		{
			CounterpartyDlg counterpartyDlg = new CounterpartyDlg(counterpartyId);
			counterpartyDlg.OpenFixedPrices();
			TDIMain.MainNotebook.AddTab(counterpartyDlg);
		}

		public void OpenFixedPricesForDeliveryPoint(int deliveryPointId)
		{
			IParametersProvider parametersProvider = new ParametersProvider();
			IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
			IFiasService fiasService = new FiasService(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken);
			var deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(fiasService);
			var dpViewModel = deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(deliveryPointId);
			TDIMain.MainNotebook.AddTab(dpViewModel);
			dpViewModel.OpenFixedPrices();
		}
	}
}
