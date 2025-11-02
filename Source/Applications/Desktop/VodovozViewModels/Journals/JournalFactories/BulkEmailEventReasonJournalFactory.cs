using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class BulkEmailEventReasonJournalFactory : IBulkEmailEventReasonJournalFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public BulkEmailEventReasonJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateBulkEmailEventReasonAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<BulkEmailEventReasonJournalViewModel>(typeof(BulkEmailEventReason), () =>
			{
				return new BulkEmailEventReasonJournalViewModel(_uowFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
