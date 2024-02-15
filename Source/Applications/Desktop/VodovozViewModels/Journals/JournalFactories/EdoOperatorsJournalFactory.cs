using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class EdoOperatorsJournalFactory : IEdoOperatorsJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateEdoOperatorsAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EdoOperatorsJournalViewModel>(typeof(EdoOperator), () =>
			{
				return new EdoOperatorsJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
