using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class RoboAtsCounterpartyJournalFactory : IRoboAtsCounterpartyJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateRoboAtsCounterpartyNameAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboAtsCounterpartyNameJournalViewModel>(typeof(RoboAtsCounterpartyName), () =>
			{
				return new RoboAtsCounterpartyNameJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}

		public IEntityAutocompleteSelectorFactory CreateRoboAtsCounterpartyPatronymicAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboAtsCounterpartyPatronymicJournalViewModel>(typeof(RoboAtsCounterpartyPatronymic), () =>
			{
				return new RoboAtsCounterpartyPatronymicJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
