using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ComplaintDetalizationAutocompleteSelectorFactory : IComplaintDetalizationAutocompleteSelectorFactory
	{
		public IEntityAutocompleteSelectorFactory CreateComplaintDetalizationAutocompleteSelectorFactory(ComplaintDetalizationJournalFilterViewModel complainDetalizationFilter = null) =>
			new EntityAutocompleteSelectorFactory<ComplaintDetalizationJournalViewModel>(
				typeof(ComplaintDetalization),
				() => CreateComplaintDetalizationJournalViewModel(complainDetalizationFilter));

		private ComplaintDetalizationJournalViewModel CreateComplaintDetalizationJournalViewModel(ComplaintDetalizationJournalFilterViewModel filterViewModel = null) =>
			new ComplaintDetalizationJournalViewModel(
				filterViewModel,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices);
	}
}
