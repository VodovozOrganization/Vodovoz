using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class BulkEmailEventReasonJournalFactory : IBulkEmailEventReasonJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateBulkEmailEventReasonAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<BulkEmailEventReasonJournalViewModel>(typeof(BulkEmailEventReason), () =>
			{
				return new BulkEmailEventReasonJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
