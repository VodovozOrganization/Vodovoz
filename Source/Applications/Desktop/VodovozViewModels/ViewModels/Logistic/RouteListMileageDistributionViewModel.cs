using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMileageDistributionViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private DelegateCommand _saveDistributionCommand;
		private DelegateCommand _distributeCommand;
		private DelegateCommand<RouteListMileageDistributionNode> _acceptFineCommand;
		private readonly WageParameterService _wageParameterService;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IValidationContextFactory _validationContextFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ITdiTabParent _tabParent;
		private readonly ITdiTab _tdiTab;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;

		private readonly IEmployeeSettings _employeeSettings;
		private readonly IEmployeeService _employeeService;

		private RouteListMileageDistributionNode _selectedNode;

		public RouteListMileageDistributionViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			WageParameterService wageParameterService,
			ICallTaskWorker callTaskWorker,
			IValidationContextFactory validationContextFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeSettings employeeSettings,
			IEmployeeService employeeService,
			ITdiTabParent tabParent,
			ITdiTab tdiTab)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			TabName = $"Разнос километража";

			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_tabParent = tabParent ?? throw new ArgumentNullException(nameof(tabParent));
			_tdiTab = tdiTab ?? throw new ArgumentNullException(nameof(tdiTab));

			GenerateDistributionRows();
		}

		#region Private methods

		private void GenerateDistributionRows()
		{
			var driverRouteListsAtDay = _routeListRepository.GetDriverRouteLists(UoW, Entity.Driver, Entity.Date);

			foreach(var routeList in driverRouteListsAtDay)
			{
				try
				{
					routeList.RecountMileage();
				}
				catch(Exception ex)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
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
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не во всех МЛ есть пересчитанный километраж"," Не удалось разнести километраж ");
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

		private void AcceptDistribution(RouteList routeList)
		{
			var validationContext = _validationContextFactory.CreateNewValidationContext(routeList,
				new Dictionary<object, object>
					{
						{ nameof(IRouteListRepository), _routeListRepository},
						{ nameof(IRouteListItemRepository), _routeListItemRepository}
					});

			if(!CommonServices.ValidationService.Validate(routeList, validationContext))
			{
				return;
			}

			routeList.AcceptMileage(_callTaskWorker);

			if(routeList.Status > RouteListStatus.OnClosing)
			{
				if(routeList.FuelOperationHaveDiscrepancy())
				{
					if(!CommonServices.InteractiveService.Question(
						   "Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?"))
					{
						return;
					}
				}

				routeList.UpdateFuelOperation();
			}

			if(routeList.Status == RouteListStatus.Delivered)
			{
				ChangeStatusAndCreateTaskFromDelivered(routeList);
			}

			routeList.CalculateWages(_wageParameterService);
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

		#region Properties

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
		public bool AskSaveOnClose => CanEdit;

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

		public bool CanAcceptFine => SelectedNode != null && SelectedNode.IsRouteList;

		#endregion

		#region Commands

		public DelegateCommand SaveDistributionCommand =>
			_saveDistributionCommand ?? (_saveDistributionCommand = new DelegateCommand(() =>
				{
					if(!HasChanges)
					{
						return;
					}

					foreach(var distributionNode in Rows.Where(r => r.RouteList != null).ToList())
					{
						AcceptDistribution(distributionNode.RouteList);
						UoW.Save(distributionNode.RouteList);
					}

					UoW.Commit();

					Close(false, CloseSource.Save);

					_tabParent.ForceCloseTab(_tdiTab);
				},
				() => true
			));

		public DelegateCommand DistributeCommand =>
			_distributeCommand ?? (_distributeCommand = new DelegateCommand(() =>
				{
					DistributeMileage();
				}
			));

		public DelegateCommand<RouteListMileageDistributionNode> AcceptFineCommand =>
			_acceptFineCommand ?? (_acceptFineCommand = new DelegateCommand<RouteListMileageDistributionNode>((selectedItem) =>
				{
					var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

					page.ViewModel.SetRouteListById(selectedItem.RouteList.Id);
					page.ViewModel.FineReasonString = "Перерасход топлива";
				}
			));

		#endregion

		public override bool Save(bool close)
		{
			SaveDistributionCommand.Execute();
			return true;
		}
	}
}
