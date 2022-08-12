using QS.Dialog;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections;
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
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageCheckViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		#region Поля

		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly ICallTaskRepository _callTaskRepository = new CallTaskRepository();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IErrorReporter _errorReporter;
		private readonly WageParameterService _wageParameterService/* = new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider))*/;
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeJournalFactory _employeeFactory;
		private bool _canEdit;

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory LogisticianSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }


		public bool CanEdit
		{
			get => _canEdit;
			set
			{
				SetField(ref _canEdit, value);
				OnPropertyChanged(nameof(_canEdit));
			}
		}

		public bool IsAcceptAvailable => Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck;

		//public bool CanEditRouteList => CanEdit && Entity.Status == RouteListStatus.Closed;

		List<RouteListKeepingItemNode> items;

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

		public RouteListMileageCheckViewModel(IEntityUoWBuilder ctorParam,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			IEmployeeJournalFactory employeeFactory,
			IDeliveryShiftRepository deliveryShiftRepository)
			: base(ctorParam, commonServices)
		{
			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory();
			LogisticianSelectorFactory = employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();
			ForwarderSelectorFactory = employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();
			DeliveryShifts = deliveryShiftRepository.ActiveShifts(UoW);

			RouteListItems = GenerateRouteListItems();


			var currentPermissionService = CommonServices.CurrentPermissionService;
			CanEdit = currentPermissionService.ValidatePresetPermission("logistican") && PermissionResult.CanUpdate;
			//UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = $"Контроль за километражем маршрутного листа №{Entity.Id}";
			var canConfirmMileage = currentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");
			CanEdit &= canConfirmMileage || !(Entity.GetCarVersion.IsCompanyCar && new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus }.Contains(Entity.Car.CarModel.CarTypeOfUse));

			if(!CanEdit)
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не достаточно прав. Обратитесь к руководителю.");
				UpdateSensitivity();
			}

			if(CanEdit && Entity.Status != RouteListStatus.Closed)
			{
				if(!Entity.RecountMileage())
				{
					FailInitialize = true;
					return;
				}
			}
		}

		private IList<RouteListKeepingItemNode> GenerateRouteListItems()
		{
			var items = new List<RouteListKeepingItemNode>();
			foreach(var item in Entity.Addresses)
			{
				items.Add(new RouteListKeepingItemNode { RouteListItem = item });
			}

			items.Sort((x, y) => {
				if(x.RouteListItem.StatusLastUpdate.HasValue && y.RouteListItem.StatusLastUpdate.HasValue)
				{
					if(x.RouteListItem.StatusLastUpdate > y.RouteListItem.StatusLastUpdate) return 1;
					if(x.RouteListItem.StatusLastUpdate < y.RouteListItem.StatusLastUpdate) return -1;
				}
				return 0;
			});

			return items;
		}

		private void UpdateSensitivity()
		{
			//if(CanEdit && Entity.Status == RouteListStatus.Closed)
			//{
			//	vboxRouteList.Sensitive = table2.Sensitive = false;
			//}
			//else
			//{
			//	buttonSave.Sensitive = false;
			//	table2.Sensitive = false;
			//	hboxMileageComment.Sensitive = false;
			//	ytreeviewAddresses.Sensitive = false;
			//	hbox9.Sensitive = false;
			//}
		}

		public bool AskSaveOnClose => CanEdit;

		public IEnumerable DeliveryShifts { get; }

		public IList<RouteListKeepingItemNode> RouteListItems { get; }

		protected override bool BeforeSave()
		{
			if(Entity.Status > RouteListStatus.OnClosing)
			{
				if(Entity.FuelOperationHaveDiscrepancy())
				{
					if(!_commonServices.InteractiveService.Question("Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?"))
					{
						return false;
					}
				}
				Entity.UpdateFuelOperation();
			}

			if(Entity.Status == RouteListStatus.Delivered)
			{
				ChangeStatusAndCreateTaskFromDelivered();
			}

			Entity.CalculateWages(_wageParameterService);

			return base.BeforeSave();
		}

		private void ChangeStatusAndCreateTaskFromDelivered()
		{
			Entity.ChangeStatusAndCreateTask(
				Entity.GetCarVersion.IsCompanyCar && Entity.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck
					? RouteListStatus.MileageCheck
					: RouteListStatus.OnClosing,
				CallTaskWorker
			);
		}
	}
}
