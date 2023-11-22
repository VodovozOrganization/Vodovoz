using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Factories
{
	public class CarModelJournalFactory : ICarModelJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarModelAutocompleteSelectorFactory(
			ILifetimeScope lifetimeScope,
			CarModelJournalFilterViewModel filter = null,
			bool multipleSelect = false)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			var carModelJournalViewModel = lifetimeScope.Resolve<CarModelJournalViewModel>();
			carModelJournalViewModel.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;

			return new EntityAutocompleteSelectorFactory<CarModelJournalViewModel>(
				typeof(CarModel),
				() => carModelJournalViewModel);
		}
	}
}
