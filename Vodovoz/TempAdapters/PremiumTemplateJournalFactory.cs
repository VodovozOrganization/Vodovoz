using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
    public class PremiumTemplateJournalFactory : IPremiumTemplateJournalFactory
	{
        public IEntityAutocompleteSelectorFactory CreatePremiumTemplateAutocompleteSelectorFactory()
        {
			return new DefaultEntityAutocompleteSelectorFactory<PremiumTemplate, PremiumTemplateJournalViewModel,
				PremiumTemplateJournalFilterViewModel>(ServicesConfig.CommonServices);
        }
    }
}