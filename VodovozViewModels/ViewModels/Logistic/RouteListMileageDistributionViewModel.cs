using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMileageDistributionViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private DelegateCommand _saveDistributionCommand;
		private DelegateCommand _distributeCommand;
		private readonly WageParameterService _wageParameterService;
		private readonly CallTaskWorker _callTaskWorker;
		private readonly IValidationContextFactory _validationContextFactory;
		private IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private decimal? _totalConfirmedDistanceAtDay;
		private readonly IList<RouteList> _driverRouteListsAtDay;

		public RouteListMileageDistributionViewModel(IEntityUoWBuilder uowBuilder,
			ICommonServices commonServices,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			WageParameterService wageParameterService,
			CallTaskWorker callTaskWorker,
			IValidationContextFactory validationContextFactory)
			: base(uowBuilder, commonServices)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));

			TabName = $"Разнос километража";

			//DistributionItems = GetItems(routeListRepository);

			//RouteListsForDistribution = new GenericObservableList<RouteList>(routeListRepository.GetDriverRouteLists(uow, Entity.Driver, Entity.Date));
			//MileageDistribution = new RouteListMileageDistribution
			//{
			//RouteListsForDistribution = routeListRepository.GetDriverRouteLists(UoW, Entity.Driver, Entity.Date);
			//};
			_driverRouteListsAtDay = routeListRepository.GetDriverRouteLists(UoW, Entity.Driver, Entity.Date);
			GenerateDistributionNodes();

		}

		private void GenerateDistributionNodes()
		{
			RouteListMileageDistributions = new List<RouteListMileageDistributionNode>(_driverRouteListsAtDay.Select(x =>
				new RouteListMileageDistributionNode
				{
					RouteList = x
				}));

			RouteListMileageDistributions.Add(new RouteListMileageDistributionNode
			{
				CustomForwarderColumn = "Итого:",
				CustomRecalculatedDistanceColumn = TotalRecalculatedDistanceAtDay
			});

			RouteListMileageDistributions.Add(new RouteListMileageDistributionNode
			{
				CustomForwarderColumn = "Разница:",
				CustomRecalculatedDistanceColumn = SubtractDistance
			});
		}

		public IList<RouteList> DriverRouteListsAtDay { get; set; }


		//private IList<RouteListMileageDistributionNode> GetItems(IRouteListRepository routeListRepository)
		//{
		//	var driverRouteList = routeListRepository.GetDriverRouteLists(UoW, Entity.Driver, Entity.Date);

		//	RouteListsForDistribution = driverRouteList;

		//	return driverRouteList.Select(routeList => new RouteListMileageDistributionNode
		//		{
		//			Id = routeList.Id,
		//			Driver = routeList.Driver?.FullName,
		//			ForwarderColumn = routeList.ForwarderColumn?.FullName,
		//			DeliveryShift = routeList.Shift?.Name,
		//			ConfirmedDistance = routeList.ConfirmedDistance,
		//			RecalculatedDistanceColumn = routeList.RecalculatedDistanceColumn
		//		})
		//		.ToList();
		//}

		#region Commands

		public DelegateCommand SaveDistributionCommand =>
			_saveDistributionCommand ?? (_saveDistributionCommand = new DelegateCommand(() =>
				{
					if(!HasChanges)
					{
						return;
					}

					foreach(var distributionNode in RouteListMileageDistributions.Where(n => n.RouteList != null).ToList())
					{
						AcceptDistribution(distributionNode.RouteList);
						UoW.Save(distributionNode.RouteList);
					}

					UoW.Commit();
				},
				() => true
			));

		public DelegateCommand DistributeCommand =>
			_distributeCommand ?? (_distributeCommand = new DelegateCommand(() =>
				{
					
				},
				() => true
			));

		#endregion

		//public RouteListMileageDistribution MileageDistribution { get; set; }
		//public class RouteListMileageDistribution
		//{
		public IList<RouteListMileageDistributionNode> RouteListMileageDistributions { get; set; }
		//public IList<RouteList> RouteListsForDistribution { get; set; }
		public decimal? TotalRecalculatedDistanceAtDay => RouteListMileageDistributions.Sum(n => n.RouteList?.RecalculatedDistance);
		
		public decimal? TotalConfirmedDistanceAtDay
		{
			get => _totalConfirmedDistanceAtDay;
			set
			{
				SetField(ref _totalConfirmedDistanceAtDay, value);
				GenerateDistributionNodes();
			}
		}

		public decimal? SubtractDistance => TotalConfirmedDistanceAtDay - TotalRecalculatedDistanceAtDay;

		//}

		//public GenericObservableList<RouteList> RouteListsForDistribution { get; private set; }

		public bool CanEdit { get; set; } = true;

		public bool AskSaveOnClose => CanEdit;

		public override bool Save(bool close)
		{
			SaveDistributionCommand.Execute();
			return true;
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

	}
}
