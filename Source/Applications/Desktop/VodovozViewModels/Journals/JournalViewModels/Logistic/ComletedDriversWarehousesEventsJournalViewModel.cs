using System;
using System.Linq;
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
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;

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
		
		public override string FooterInfo
		{
			get
			{
				var autoRefreshInfo = GetAutoRefreshInfo();
				return $"{autoRefreshInfo} | {base.FooterInfo}";
			}
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateStartAutoRefresh();
			CreateStopAutoRefresh();
			CreateExportAction();
		}
		
		private string GetAutoRefreshInfo()
		{
			return _autoRefreshEnabled ? $"Автообновление каждые {_autoRefreshInterval} сек." : "Автообновление выключено";
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
					OnPropertyChanged(nameof(FooterInfo));
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
					OnPropertyChanged(nameof(FooterInfo));
					UpdateJournalActions?.Invoke();
				}
			);
			
			NodeActionsList.Add(journalAction);
		}
		
		private void CreateExportAction()
		{
			var journalAction = new JournalAction(
				"Экспорт",
				objects => true,
				objects => true,
				objects =>
				{
					var rows = GetCompletedEvents(UoW).List<CompletedDriversWarehousesEventsJournalNode>();

					if(!rows.Any())
					{
						return;
					}
					
					var report = new CompletedDriversWarehousesEventsJournalReport(_scope.Resolve<IFileDialogService>());
					report.Export(rows);
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
				.Left.JoinAlias(ce => ce.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinAlias(ce => ce.DriverWarehouseEvent, () => eventAlias);

			var carModelWithNumber = CustomProjections.Concat(
				Projections.Property(() => carModelAlias.Name),
				Projections.Constant(" ("),
				Projections.Property(() => carAlias.RegistrationNumber),
				Projections.Constant(")"));

			var employeeProjection = EmployeeProjections.GetDriverFullNameProjection();

			if(_filterViewModel.CompletedEventId.HasValue)
			{
				query.Where(ce => ce.Id == _filterViewModel.CompletedEventId);
			}

			var startDate = _filterViewModel.StartDate;
			
			if(startDate.HasValue)
			{
				query.Where(ce => ce.CompletedDate >= startDate);
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
				if(_filterViewModel.DistanceCriterion == ComparisonSings.LessOrEqual)
				{
					query.Where(ce => ce.DistanceMetersFromScanningLocation <= _filterViewModel.DistanceFromScanning);
				}
				else
				{
					query.Where(ce => ce.DistanceMetersFromScanningLocation >= _filterViewModel.DistanceFromScanning);
				}
			}

			var selectedDrivers = _filterViewModel.GetSelectedDrivers();

			if(selectedDrivers.Any())
			{
				query.WhereRestrictionOn(() => driverAlias.Id).IsInG(selectedDrivers);
			}

			query.SelectList(list => list
				.Select(ce => ce.Id).WithAlias(() => resultAlias.Id)
				.Select(() => eventAlias.EventName).WithAlias(() => resultAlias.EventName)
				.Select(() => eventAlias.Type).WithAlias(() => resultAlias.EventType)
				.Select(() => eventAlias.DocumentType).WithAlias(() => resultAlias.DocumentType)
				.Select(ce => ce.DocumentId).WithAlias(() => resultAlias.DocumentNumber)
				.Select(employeeProjection).WithAlias(() => resultAlias.EmployeeName)
				.Select(carModelWithNumber).WithAlias(() => resultAlias.Car)
				.Select(ce => ce.CompletedDate).WithAlias(() => resultAlias.CompletedDate)
				.Select(ce => ce.DistanceMetersFromScanningLocation)
					.WithAlias(() => resultAlias.DistanceMetersFromScanningLocation))
				.TransformUsing(Transformers.AliasToBean<CompletedDriversWarehousesEventsJournalNode>());

			if(_filterViewModel.SortViewModel.RightItems.Any())
			{
				foreach(var node in _filterViewModel.SortViewModel.GetRightItems())
				{
					switch(node.GroupType)
					{
						case GroupingType.Employee:
							query.OrderBy(employeeProjection).Asc();
							break;
						case GroupingType.DriverWarehouseEvent:
							query.OrderBy(() => eventAlias.EventName).Asc();
							break;
						case GroupingType.DriverWarehouseEventDate:
							if(_filterViewModel.OrderByEventDateDesc)
							{
								query.OrderBy(ce => ce.Id).Desc();
							}
							else
							{
								query.OrderBy(ce => ce.Id).Asc();
							}
							break;
					}
				}
			}
			
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
