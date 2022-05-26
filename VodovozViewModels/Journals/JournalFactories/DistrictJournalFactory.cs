using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class DistrictJournalFactory : IDistrictJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateDistrictAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () =>
			{
				var filter = new DistrictJournalFilterViewModel();
				return new DistrictJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
