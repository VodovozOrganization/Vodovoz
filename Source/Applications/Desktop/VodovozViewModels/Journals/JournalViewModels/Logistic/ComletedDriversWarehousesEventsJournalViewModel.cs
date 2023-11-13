using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CompletedDriversWarehousesEventsJournalViewModel : JournalViewModelBase
	{
		private readonly ILifetimeScope _scope;
		private CompletedDriversWarehousesEventsJournalFilterViewModel _filterViewModel;

		public CompletedDriversWarehousesEventsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			Action<CompletedDriversWarehousesEventsJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			ConfigureLoader();
			CreateFilter(filterParams);

			TabName = "Журнал завершенных событий";
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
		}

		private void ConfigureLoader()
		{
			var loader = new ThreadDataLoader<CompletedDriversWarehousesEventsJournalNode>(UnitOfWorkFactory);
			loader.AddQuery(GetCompletedEvents);
			DataLoader = loader;
		}

		private IQueryOver<CompletedDriverWarehouseEvent> GetCompletedEvents(IUnitOfWork uow)
		{
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			DriverWarehouseEvent driverEventAlias = null;
			CompletedDriversWarehousesEventsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<CompletedDriverWarehouseEvent>()
				.JoinAlias(ce => ce.Employee, () => driverAlias)
				.JoinAlias(ce => ce.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinAlias(ce => ce.DriverWarehouseEvent, () => driverEventAlias);

			var carModelWithNumber = CustomProjections.Concat(
				Projections.Property(() => carModelAlias.Name),
				Projections.Constant(" ("),
				Projections.Property(() => carAlias.RegistrationNumber),
				Projections.Constant(")"));

			query.SelectList(list => list
				.Select(ce => ce.Id).WithAlias(() => resultAlias.Id)
				.Select(() => driverEventAlias.EventName).WithAlias(() => resultAlias.EventName)
				.Select(() => driverEventAlias.Type).WithAlias(() => resultAlias.Type)
				.Select(EmployeeProjections.GetDriverFullNameProjection()).WithAlias(() => resultAlias.DriverName)
				.Select(carModelWithNumber).WithAlias(() => resultAlias.Car)
				.Select(ce => ce.DistanceMetersFromScanningLocation)
					.WithAlias(() => resultAlias.DistanceMetersFromScanningLocation))
				.TransformUsing(Transformers.AliasToBean<CompletedDriversWarehousesEventsJournalNode>());
			
			return query;
		}

		private void CreateFilter(Action<CompletedDriversWarehousesEventsJournalFilterViewModel> filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(DialogViewModelBase), this),
				new TypedParameter(typeof(Action<CompletedDriversWarehousesEventsJournalFilterViewModel>), filterParams)
			};

			_filterViewModel = _scope.Resolve<CompletedDriversWarehousesEventsJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
