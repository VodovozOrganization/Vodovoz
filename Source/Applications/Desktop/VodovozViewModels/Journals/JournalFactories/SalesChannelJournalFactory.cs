using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Retail;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class SalesChannelJournalFactory : ISalesChannelJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSalesChannelAutocompleteSelectorFactory()
		{
			return new DefaultEntityAutocompleteSelectorFactory<SalesChannel, SalesChannelJournalViewModel,
				SalesChannelJournalFilterViewModel>(ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices);
		}
	}
}
