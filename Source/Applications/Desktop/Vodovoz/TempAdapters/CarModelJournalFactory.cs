using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Factories;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class CarModelJournalFactory : ICarModelJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarModelAutocompleteSelectorFactory(
			CarModelJournalFilterViewModel filter = null, bool multipleSelect = false)
		{
			return new EntityAutocompleteSelectorFactory<CarModelJournalViewModel>(
				typeof(CarModel),
				() =>
				{
					var parametersProvider = new ParametersProvider();
					
					return new CarModelJournalViewModel(
						filter ?? new CarModelJournalFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices,
						new CarManufacturerJournalFactory(), new RouteListProfitabilityController(
							new RouteListProfitabilityFactory(), new NomenclatureParametersProvider(parametersProvider),
							new ProfitabilityConstantsRepository(), new RouteListProfitabilityRepository(),
							new RouteListRepository(new StockRepository(), new BaseParametersProvider(parametersProvider))))
					{
						SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
					};
				});
		}
	}
}
