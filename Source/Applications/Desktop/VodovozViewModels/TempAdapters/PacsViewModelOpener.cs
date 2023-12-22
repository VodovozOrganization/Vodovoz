using QS.Navigation;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.ViewModels.Journals.JournalViewModels.Pacs;

namespace Vodovoz.ViewModels.TempAdapters
{
	public class PacsViewModelOpener : IPacsViewModelOpener
	{
		private readonly INavigationManager _navigationManager;

		public PacsViewModelOpener(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public void OpenOperatorsReferenceBook()
		{
			_navigationManager.OpenViewModel<OperatorsJournalViewModel>(null);
		}
	}
}
