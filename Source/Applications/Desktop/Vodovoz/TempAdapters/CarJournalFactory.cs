using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class CarJournalFactory : ICarJournalFactory
	{
		private readonly INavigationManager _navigationManager;

		public CarJournalFactory(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactory(bool multipleSelect = false)
		{
			return new EntityAutocompleteSelectorFactory<CarJournalViewModel>(typeof(Car),
				() =>
				{
					var filter = new CarJournalFilterViewModel(new CarModelJournalFactory());
					var journalViewModel =
						new CarJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					journalViewModel.NavigationManager = _navigationManager;
					journalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
					return journalViewModel;
				});
		}
	}
}
