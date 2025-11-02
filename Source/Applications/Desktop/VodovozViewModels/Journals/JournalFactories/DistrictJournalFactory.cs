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
		private readonly IUnitOfWorkFactory _uowFactory;

		public DistrictJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateDistrictAutocompleteSelectorFactory(DistrictJournalFilterViewModel districtJournalFilterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () =>
			{
				var filter = districtJournalFilterViewModel ?? new DistrictJournalFilterViewModel();
				return new DistrictJournalViewModel(filter, _uowFactory, ServicesConfig.CommonServices);
			});
		}

		public IEntityAutocompleteSelectorFactory CreateDistrictAutocompleteSelectorFactory(DistrictJournalFilterViewModel districtJournalFilterViewModel, bool enableDfaultButtons)
		{
			return new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () =>
			{
				var filter = districtJournalFilterViewModel ?? new DistrictJournalFilterViewModel();
				return new DistrictJournalViewModel(filter, _uowFactory, ServicesConfig.CommonServices)
				{
					EnableDeleteButton = enableDfaultButtons,
					EnableAddButton = enableDfaultButtons,
					EnableEditButton = enableDfaultButtons
				};
			});
		}
	}
}
