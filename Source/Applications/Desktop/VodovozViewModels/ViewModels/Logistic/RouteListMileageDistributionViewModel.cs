using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Validation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMileageDistributionViewModel : DialogViewModelBase, IAskSaveOnCloseViewModel, IDisposable
	{
		private const string _title = "Разнос километража";
		private readonly WageParameterService _wageParameterService;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IValidationContextFactory _validationContextFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IValidator _validator;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IUnitOfWork _uow;
		
		private int _driverId;
		private RouteListMileageDistributionNode _selectedNode;

		public RouteListMileageDistributionViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IValidator validator,
			INavigationManager navigationManager,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			WageParameterService wageParameterService,
			ICallTaskWorker callTaskWorker,
			IValidationContextFactory validationContextFactory)
			: base(navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}
			
			Title = _title;

			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot(_title);
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));

			InitializeCommands();
			AskSaveOnClose = CanEdit;
		}

		#region Properties

		public event EventHandler Distributed;
		public bool CanAcceptFine => SelectedNode != null && SelectedNode.IsRouteList;
		public string Car { get; private set; }
		public DateTime Date { get; private set; }
		public GenericObservableList<RouteListMileageDistributionNode> Rows { get; set; }
		public decimal? TotalConfirmedDistanceAtDay { get; set; }
		public decimal? TotalRecalculatedDistanceAtDay => Rows.Sum(r => r.RouteList?.RecalculatedDistance);
		public decimal? SubtractDistance => (TotalConfirmedDistanceAtDay - TotalRecalculatedDistanceAtDay) ?? SubtractDistanceAutomatically;

		public decimal? SubtractDistanceAutomatically
		{
			get
			{
				var routeListRows = Rows.Where(r => r.IsRouteList).ToList();

				var totalConfirmedDistance = routeListRows.All(r => r.ConfirmedDistance > 0)
					? routeListRows.Sum(r => r.RouteList?.ConfirmedDistance)
					: null;

				if(totalConfirmedDistance.HasValue && TotalRecalculatedDistanceAtDay.HasValue)
				{
					return totalConfirmedDistance - TotalRecalculatedDistanceAtDay;
				}

				return null;
			}
		}
		
		public bool CanEdit { get; set; } = true;
		public bool AskSaveOnClose { get; private set; }

		public RouteListMileageDistributionNode SelectedNode
		{
			get => _selectedNode;
			set
			{
				if(SetField(ref _selectedNode, value))
				{
					OnPropertyChanged(nameof(CanAcceptFine));
				}
			}
		}
		
		private bool HasChanges => _uow.HasChanges;
		
		#region Commands

		public DelegateCommand SaveDistributionCommand { get; private set; }
		public DelegateCommand DistributeCommand { get; private set; }
		public DelegateCommand AcceptFineCommand { get; private set; }

		#endregion

		#endregion

		public void Configure(int driverId, DateTime date, string car)
		{
			Date = date;
			Car = car;
			_driverId = driverId;
			
			GenerateDistributionRows();
		}

		#region Private methods
		
		private void InitializeCommands()
		{
			SaveDistributionCommand = new DelegateCommand(SaveDistribution);
			AcceptFineCommand = new DelegateCommand(AcceptFine);
			DistributeCommand = new DelegateCommand(DistributeMileage);
		}

		private void GenerateDistributionRows()
		{
			var driverRouteListsAtDay = _routeListRepository.GetDriverRouteLists(_uow, _driverId, Date);

			foreach(var routeList in driverRouteListsAtDay)
			{
				try
				{
					routeList.RecountMileage();
				}
				catch(Exception ex)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error,
						$"Не удалось пересчитать километраж для МЛ { routeList.Id }:\n{ ex.Message }", "Ошибка при подготовке к разносу километража");

					return;
				}
			}

			Rows = new GenericObservableList<RouteListMileageDistributionNode>(driverRouteListsAtDay.Select(x =>
				new RouteListMileageDistributionNode
				{
					DistributionNodeType = RouteListDistributionNodeType.RouteList,
					RouteList = x
				}).ToList());

			Rows.Add(new RouteListMileageDistributionNode
			{
				DistributionNodeType = RouteListDistributionNodeType.Total,
				ForwarderColumn = $"{RouteListDistributionNodeType.Total.GetEnumTitle()}:",
				RecalculatedDistanceColumn = TotalRecalculatedDistanceAtDay
			});

			Rows.Add(new RouteListMileageDistributionNode
			{
				DistributionNodeType = RouteListDistributionNodeType.Substract,
				ForwarderColumn = $"{RouteListDistributionNodeType.Substract.GetEnumTitle()}:",
				RecalculatedDistanceColumn = SubtractDistance
			});
		}
		
		private void SaveDistribution()
		{
			if(!HasChanges)
			{
				return;
			}

			foreach(var distributionNode in Rows.Where(r => r.RouteList != null).ToList())
			{
				if(!AcceptDistribution(distributionNode.RouteList))
				{
					AskSaveOnClose = false;
					return;
				}
				_uow.Save(distributionNode.RouteList);
			}

			_uow.Commit();

			Close(false, CloseSource.Save);
			Distributed?.Invoke(this, EventArgs.Empty);
		}
		
		private void AcceptFine()
		{
			NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
				this,
				EntityUoWBuilder.ForCreate(),
				OpenPageOptions.AsSlave,
				conf =>
				{
					conf.SetRouteListById(SelectedNode.RouteList.Id);
					conf.FineReasonString = "Перерасход топлива";
				});
		}

		private void DistributeMileage()
		{
			if(!TotalConfirmedDistanceAtDay.HasValue || TotalConfirmedDistanceAtDay == 0)
			{
				return;
			}

			var routeLists = Rows.Where(r => r.IsRouteList);

			if(!routeLists.Any())
			{
				return;
			}

			if(routeLists.Count() == 1)
			{
				routeLists.Single().ConfirmedDistance = TotalConfirmedDistanceAtDay.Value;
			}
			else
			{
				if(routeLists.Any(rl => !rl.RecalculatedDistanceColumn.HasValue))
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Error,
						"Не во всех МЛ есть пересчитанный километраж"," Не удалось разнести километраж ");
					return;
				}

				var firstRouteList = routeLists.First();
				var lastRouteList = routeLists.Last();

				firstRouteList.ConfirmedDistance = firstRouteList.RecalculatedDistanceColumn.Value + SubtractDistance.Value / 2;
				lastRouteList.ConfirmedDistance = lastRouteList.RecalculatedDistanceColumn.Value + SubtractDistance.Value / 2;

				foreach(var routeList in routeLists)
				{
					if(routeList != firstRouteList && routeList != lastRouteList && routeList.RecalculatedDistanceColumn != null)
					{
						routeList.ConfirmedDistance = routeList.RecalculatedDistanceColumn.Value;
					}
				}
			}

			var substructRow = Rows.SingleOrDefault(r => r.DistributionNodeType == RouteListDistributionNodeType.Substract);
			
			if(substructRow != null)
			{
				substructRow.RecalculatedDistanceColumn = SubtractDistance;
			}

		}

		private bool AcceptDistribution(RouteList routeList)
		{
			var validationContext = _validationContextFactory.CreateNewValidationContext(routeList,
				new Dictionary<object, object>
					{
						{ nameof(IRouteListRepository), _routeListRepository},
						{ nameof(IRouteListItemRepository), _routeListItemRepository}
					});

			if(!_validator.Validate(routeList, validationContext))
			{
				return false;
			}

			if(!routeList.AcceptMileage(_callTaskWorker, _validator))
			{
				return false;
			}

			if(routeList.Status > RouteListStatus.OnClosing)
			{
				if(routeList.FuelOperationHaveDiscrepancy())
				{
					if(!_interactiveService.Question(
						"Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений." +
						" Продолжить сохранение?"))
					{
						return false;
					}
				}

				routeList.UpdateFuelOperation();
			}

			if(!routeList.TryValidateFuelOperation(_validator))
			{
				return false;
			}

			if(routeList.Status == RouteListStatus.Delivered)
			{
				ChangeStatusAndCreateTaskFromDelivered(routeList);
			}

			routeList.CalculateWages(_wageParameterService);
			return true;
		}

		private void ChangeStatusAndCreateTaskFromDelivered(RouteList routeList)
		{
			routeList.ChangeStatusAndCreateTask(
				routeList.GetCarVersion.IsCompanyCar && routeList.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck
					? RouteListStatus.MileageCheck
					: RouteListStatus.OnClosing,
				_callTaskWorker
			);
		}

		#endregion
		
		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
