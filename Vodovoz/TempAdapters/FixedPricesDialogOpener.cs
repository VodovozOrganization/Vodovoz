using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.ViewModels.Counterparty;

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
	        var dpViewModel = new DeliveryPointViewModel(new UserRepository(), new GtkTabsOpener(), new PhoneRepository(), ContactParametersProvider.Instance,
		        new CitiesDataLoader(OsmWorker.GetOsmService()), new StreetsDataLoader(OsmWorker.GetOsmService()), new HousesDataLoader(OsmWorker.GetOsmService()),
		        new NomenclatureSelectorFactory(),
		        new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory(),
			        new WaterFixedPricesGenerator(new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())))),
		        EntityUoWBuilder.ForOpen(deliveryPointId), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
            TDIMain.MainNotebook.AddTab(dpViewModel);
            dpViewModel.OpenFixedPrices();
        }
    }
}
