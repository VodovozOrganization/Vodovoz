using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
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
						new CarJournalViewModel(
							filter,
							UnitOfWorkFactory.GetDefaultFactory,
							ServicesConfig.CommonServices,
							MainClass.AppDIContainer.BeginLifetimeScope());
					journalViewModel.NavigationManager = _navigationManager;
					journalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
					return journalViewModel;
				});
		}

		public IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactoryForCarsExploitationReport(bool multipleSelect = false)
		{
			return new EntityAutocompleteSelectorFactory<CarJournalViewModel>(typeof(Car),
				() =>
				{
					var filter = new CarJournalFilterViewModel(new CarModelJournalFactory())
					{
						Archive = false,
						VisitingMasters = false,
						RestrictedCarTypesOfUse = new List<CarTypeOfUse>(new[] { CarTypeOfUse.Largus, CarTypeOfUse.GAZelle })
					};
					filter.SetFilterSensitivity(false);
					filter.CanChangeRestrictedCarOwnTypes = true;
					var journalViewModel =
						new CarJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					journalViewModel.NavigationManager = _navigationManager;
					journalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
					return journalViewModel;
				});
		}
	}
}
