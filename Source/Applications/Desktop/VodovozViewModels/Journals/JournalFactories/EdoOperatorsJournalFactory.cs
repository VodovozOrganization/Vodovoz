using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class EdoOperatorsJournalFactory : IEdoOperatorsJournalFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EdoOperatorsJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateEdoOperatorsAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EdoOperatorsJournalViewModel>(typeof(EdoOperator), () =>
			{
				return new EdoOperatorsJournalViewModel(_uowFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
