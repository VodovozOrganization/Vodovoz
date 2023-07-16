using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
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
							Startup.AppDIContainer.BeginLifetimeScope());
					journalViewModel.NavigationManager = _navigationManager;
					journalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
					return journalViewModel;
				});
		}

		public IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactoryForCarsExploitationReport(
			ILifetimeScope scope, bool multipleSelect = false)
		{
			return new EntityAutocompleteSelectorFactory<CarJournalViewModel>(typeof(Car),
				() =>
				{
					var filter = scope.Resolve<CarJournalFilterViewModel>();
					filter.SetAndRefilterAtOnce(
						x => x.Archive = false,
						x => x.VisitingMasters = false,
						x => x.RestrictedCarTypesOfUse =
							new List<CarTypeOfUse>(new[] { CarTypeOfUse.Largus, CarTypeOfUse.GAZelle }));
					
					filter.SetFilterSensitivity(false);
					filter.CanChangeRestrictedCarOwnTypes = true;

					var journalViewModel =
						scope.Resolve<CarJournalViewModel>(new TypedParameter(typeof(CarJournalFilterViewModel), filter));
					journalViewModel.NavigationManager = _navigationManager;
					journalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
					return journalViewModel;
				});
		}
	}
}
