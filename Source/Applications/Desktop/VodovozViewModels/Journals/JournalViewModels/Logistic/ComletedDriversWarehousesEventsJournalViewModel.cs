using System;
using System.Timers;
using Autofac;
using DateTimeHelpers;
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
		private bool _autoRefreshEnabled;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval = 30;

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
			StartAutoRefresh();
			CreateNodeActions();
			SearchEnabled = false;

			TabName = "Журнал завершенных событий";
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateStartAutoRefresh();
			CreateStopAutoRefresh();
		}

		private void CreateStartAutoRefresh()
		{
			var journalAction = new JournalAction(
				"Вкл автообновление",
				objects => true,
				objects => !_autoRefreshEnabled,
				objects =>
				{
					_autoRefreshTimer.Start();
					_autoRefreshEnabled = true;
					UpdateJournalActions?.Invoke();
				}
			);
			
			NodeActionsList.Add(journalAction);
		}
		
		private void CreateStopAutoRefresh()
		{
			var journalAction = new JournalAction(
				"Выкл автообновление",
				objects => true,
				objects => _autoRefreshEnabled,
				objects =>
				{
					_autoRefreshTimer.Stop();
					_autoRefreshEnabled = false;
					UpdateJournalActions?.Invoke();
				}
			);
			
			NodeActionsList.Add(journalAction);
		}
		
		private void StartAutoRefresh()
		{
			if(_autoRefreshEnabled)
			{
				return;
			}
			
			_autoRefreshTimer = new Timer(_autoRefreshInterval * 1000);
			_autoRefreshTimer.Elapsed += OnUpdated;
			_autoRefreshTimer.Start();
			_autoRefreshEnabled = true;
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
			DriverWarehouseEvent eventAlias = null;
			CompletedDriversWarehousesEventsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<CompletedDriverWarehouseEvent>()
				.JoinAlias(ce => ce.Employee, () => driverAlias)
				.JoinAlias(ce => ce.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinAlias(ce => ce.DriverWarehouseEvent, () => eventAlias);

			var carModelWithNumber = CustomProjections.Concat(
				Projections.Property(() => carModelAlias.Name),
				Projections.Constant(" ("),
				Projections.Property(() => carAlias.RegistrationNumber),
				Projections.Constant(")"));

			if(_filterViewModel.CompletedEventId.HasValue)
			{
				query.Where(ce => ce.Id == _filterViewModel.CompletedEventId);
			}

			if(_filterViewModel.StartDate.HasValue)
			{
				query.Where(ce => ce.CompletedDate >= _filterViewModel.StartDate);
			}

			var endDate = _filterViewModel.EndDate;

			if(endDate.HasValue)
			{
				query.Where(ce => ce.CompletedDate <= endDate.Value.LatestDayTime());
			}

			if(_filterViewModel.SelectedEventType.HasValue)
			{
				query.Where(() => eventAlias.Type == _filterViewModel.SelectedEventType);
			}

			if(_filterViewModel.DriverWarehouseEvent != null)
			{
				query.Where(ce => ce.DriverWarehouseEvent.Id == _filterViewModel.DriverWarehouseEvent.Id);
			}

			if(_filterViewModel.Car != null)
			{
				query.Where(ce => ce.Car.Id == _filterViewModel.Car.Id);
			}

			if(_filterViewModel.DistanceFromScanning.HasValue)
			{
				query.Where(ce => ce.DistanceMetersFromScanningLocation == _filterViewModel.DistanceFromScanning);
			}

			query.SelectList(list => list
				.Select(ce => ce.Id).WithAlias(() => resultAlias.Id)
				.Select(() => eventAlias.EventName).WithAlias(() => resultAlias.EventName)
				.Select(() => eventAlias.Type).WithAlias(() => resultAlias.EventType)
				.Select(EmployeeProjections.GetDriverFullNameProjection()).WithAlias(() => resultAlias.DriverName)
				.Select(carModelWithNumber).WithAlias(() => resultAlias.Car)
				.Select(ce => ce.CompletedDate).WithAlias(() => resultAlias.CompletedDate)
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
			_filterViewModel.OnFiltered += OnUpdated;
			JournalFilter = _filterViewModel;
		}

		private void OnUpdated(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Dispose()
		{
			if(_autoRefreshTimer != null)
			{
				_autoRefreshTimer.Stop();
				_autoRefreshTimer.Elapsed -= OnUpdated;
				_autoRefreshTimer.Close();
				_autoRefreshTimer = null;
			}
			base.Dispose();
		}
	}
}
