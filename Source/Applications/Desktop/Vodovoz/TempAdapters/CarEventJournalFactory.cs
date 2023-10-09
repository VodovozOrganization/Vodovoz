using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.JournalViewers;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class CarEventJournalFactory : ICarEventJournalFactory
	{
		private readonly INavigationManager _navigationManager;

		public CarEventJournalFactory(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public IEntityAutocompleteSelectorFactory CreateCarEventAutocompleteSelectorFactory()
		{
			ICarJournalFactory carJournalFactory = new CarJournalFactory(_navigationManager);
			IEmployeeJournalFactory employeeFactory = new EmployeeJournalFactory(_navigationManager);
			ICarEventTypeJournalFactory carEventTypeJournalFactory = new CarEventTypeJournalFactory();
			ICarEventJournalFactory carEventJournalFactory = new CarEventJournalFactory(_navigationManager);

			return new EntityAutocompleteSelectorFactory<CarEventJournalViewModel>(typeof(CarEvent), () => new CarEventJournalViewModel(
				new CarEventFilterViewModel(
					carJournalFactory,
					carEventTypeJournalFactory,
					new EmployeeJournalFactory(_navigationManager)),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				carJournalFactory,
				carEventTypeJournalFactory,
				carEventJournalFactory,
				VodovozGtkServicesConfig.EmployeeService,
				employeeFactory,
				new EmployeeSettings(new ParametersProvider()),
				new CarEventSettings(new ParametersProvider())));
		}
	}
}
