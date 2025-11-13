using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class CarEventTypeJournalFactory : ICarEventTypeJournalFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public CarEventTypeJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateCarEventTypeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<CarEventTypeJournalViewModel>(typeof(CarEventType), () =>
			{
				return new CarEventTypeJournalViewModel(_uowFactory, ServicesConfig.CommonServices, new CarEventTypeFilterViewModel());
			});
		}
	}
}
