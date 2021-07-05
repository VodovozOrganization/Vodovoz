using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class CarEventTypeJournalFactory : ICarEventTypeJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarEventTypeAutocompleteSelectorFactory()
		{
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
			
			return new EntityAutocompleteSelectorFactory<CarEventTypeJournalViewModel>(typeof(CarEventType),
				() => new CarEventTypeJournalViewModel(journalActions, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}
	}
}
