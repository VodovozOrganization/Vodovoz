using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public class PremiumTemplateJournalFactory : IPremiumTemplateJournalFactory
	{
		private readonly EntitiesJournalActionsViewModel _journalActionsViewModel;

		public PremiumTemplateJournalFactory(EntitiesJournalActionsViewModel journalActionsViewModel)
		{
			_journalActionsViewModel = journalActionsViewModel ?? throw new ArgumentNullException(nameof(journalActionsViewModel));
		}
		
		public IEntityAutocompleteSelectorFactory CreatePremiumTemplateAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<PremiumTemplateJournalViewModel>(
				typeof(PremiumTemplate), 
				() => new PremiumTemplateJournalViewModel(
					_journalActionsViewModel, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}
	}
}
