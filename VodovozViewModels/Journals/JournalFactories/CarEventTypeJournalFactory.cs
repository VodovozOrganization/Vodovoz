using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class CarEventTypeJournalFactory : ICarEventTypeJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarEventTypeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<CarEventTypeJournalViewModel>(typeof(CarEventType), () =>
			{
				return new CarEventTypeJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
