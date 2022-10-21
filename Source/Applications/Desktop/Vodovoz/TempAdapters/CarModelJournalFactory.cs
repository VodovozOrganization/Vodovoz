using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Profitability;
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
				() => new CarModelJournalViewModel(
					filter ?? new CarModelJournalFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices,
					new CarManufacturerJournalFactory(), new RouteListProfitabilityController(
						new RouteListProfitabilityFactory(), new NomenclatureParametersProvider(new ParametersProvider()),
						new ProfitabilityConstantsRepository(), new RouteListProfitabilityRepository()))
				{
					SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
				}
			);
		}
	}
}
