using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.TempAdapters
{
	public class CarModelJournalFactory : ICarModelJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarModelAutocompleteSelectorFactory()
		{
			var carJournalFilterViewModel = new CarModelJournalFilterViewModel();
			
			return new EntityAutocompleteSelectorFactory<CarModelJournalViewModel>(
				typeof(CarModel),
				() => new CarModelJournalViewModel(carJournalFilterViewModel, UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices)
			);
		}
	}
}
