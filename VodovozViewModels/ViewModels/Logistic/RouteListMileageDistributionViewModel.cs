using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageDistributionViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		#region Поля

		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());
		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		private readonly ICallTaskRepository _callTaskRepository = new CallTaskRepository();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IErrorReporter _errorReporter;
		private readonly WageParameterService _wageParameterService/* = new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider))*/;

		private readonly bool _canEdit;

		List<RouteListKeepingNode> items;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker =>
			callTaskWorker ?? (callTaskWorker = new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				_callTaskRepository,
				_orderRepository,
				_employeeRepository,
				_baseParametersProvider,
				CommonServices.UserService,
				_errorReporter));



		#endregion

		public RouteListMileageDistributionViewModel(IEntityUoWBuilder ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			var currentPermissionService = CommonServices.CurrentPermissionService;			
			_canEdit = currentPermissionService.ValidatePresetPermission("logistican") && PermissionResult.CanUpdate;
			//UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = $"Контроль за километражем маршрутного листа №{Entity.Id}";
			var canConfirmMileage = currentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");
			_canEdit &= canConfirmMileage || !(Entity.GetCarVersion.IsCompanyCar && new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus }.Contains(Entity.Car.CarModel.CarTypeOfUse));
		}

		public bool AskSaveOnClose => _canEdit;
	}
}
