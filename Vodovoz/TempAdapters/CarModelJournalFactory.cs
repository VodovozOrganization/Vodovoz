using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.JournalViewModels;
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
				() => new CarModelJournalViewModel(filter ?? new CarModelJournalFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices, new CarManufacturerJournalFactory())
				{
					SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
				}
			);
		}
	}
}
