using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class UndeliveryDetalizationJournalFactory : IUndeliveryDetalizationJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateUndeliveryDetalizationAutocompleteSelectorFactory(UndeliveryDetalizationJournalFilterViewModel filterViewModel)
		{
			return new EntityAutocompleteSelectorFactory<UndeliveryDetalizationJournalViewModel>(typeof(UndeliveryDetalization), () =>
				new UndeliveryDetalizationJournalViewModel(filterViewModel, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}
	}
}
