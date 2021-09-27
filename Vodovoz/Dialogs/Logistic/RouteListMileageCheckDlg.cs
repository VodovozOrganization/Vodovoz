using System;
using System.Collections.Generic;
using System.Linq;
using Dialogs.Logistic;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;
using QS.Project.Services;
using QS.Dialog;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Filters.ViewModels;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools;
using Vodovoz.JournalViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Parameters;

namespace Vodovoz
{
	public partial class RouteListMileageCheckDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		#region Поля

		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		bool editing = true;

		List<RouteListKeepingItemNode> items;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker =>
			callTaskWorker ?? (callTaskWorker = new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				new CallTaskRepository(),
				new OrderRepository(),
				new EmployeeRepository(),
				new BaseParametersProvider(new ParametersProvider()),
				ServicesConfig.CommonServices.UserService,
				SingletonErrorReporter.Instance));

		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));

		#endregion

		public RouteListMileageCheckDlg(int id)
		{
			this.Build();
			editing = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = string.Format("Контроль за километражем маршрутного листа №{0}", Entity.Id);
			var canConfirmMileage = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");
			editing &= canConfirmMileage || !(Entity.Car.TypeOfUse.HasValue && Entity.Car.IsCompanyCar && new[] { CarTypeOfUse.CompanyGAZelle, CarTypeOfUse.CompanyLargus }.Contains(Entity.Car.TypeOfUse.Value));

			ConfigureDlg();
		}

		#region Настройка конфигураций

		public void ConfigureDlg()
		{
			if(!editing) {
				MessageDialogHelper.RunWarningDialog("Не достаточно прав. Обратитесь к руководителю.");
				HasChanges = false;
				vbxMain.Sensitive = false;
			}
			
			buttonAcceptFine.Clicked += ButtonAcceptFineOnClicked;	

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);

			referenceDriver.RepresentationModel = new EmployeesVM();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();

			referenceForwarder.RepresentationModel = new EmployeesVM();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
		
			referenceLogistican.RepresentationModel = new EmployeesVM();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoWGeneric);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();

			yspinConfirmedDistance.Binding.AddBinding(Entity, rl => rl.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();

			yentryRecalculatedDistance.Binding.AddBinding(Entity, rl => rl.RecalculatedDistance, widget => widget.Text, new DecimalToStringConverter()).InitializeFromSource();

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())
				.AddColumn("Адрес")
					.AddTextRenderer(node => String.Format("{0} д.{1}", node.RouteListItem.Order.DeliveryPoint.Street, node.RouteListItem.Order.DeliveryPoint.Building))
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.Status).Editing(false)
				.AddColumn("Последнее редактирование")
					.AddTextRenderer(node => node.LastUpdate)
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();
			
			items = new List<RouteListKeepingItemNode>();
			foreach(var item in Entity.Addresses)
				items.Add(new RouteListKeepingItemNode { RouteListItem = item });

			items.Sort((x, y) => {
				if(x.RouteListItem.StatusLastUpdate.HasValue && y.RouteListItem.StatusLastUpdate.HasValue) {
					if(x.RouteListItem.StatusLastUpdate > y.RouteListItem.StatusLastUpdate) return 1;
					if(x.RouteListItem.StatusLastUpdate < y.RouteListItem.StatusLastUpdate) return -1;
				}
				return 0;
			});

			ytreeviewAddresses.ItemsDataSource = items;
			ytextviewMileageComment.Binding.AddBinding(Entity, x => x.MileageComment, w => w.Buffer.Text).InitializeFromSource();
			
			if(Entity.Status == RouteListStatus.Closed) {

				vboxRouteList.Sensitive = table2.Sensitive = false;
			}
			else
				Entity.RecountMileage();

			//Телефон
			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = MainClass.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();
		}

		#endregion

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var validationContext = new Dictionary<object, object> {
				{ nameof(IRouteListItemRepository), new EntityRepositories.Logistic.RouteListItemRepository() }
			};
			var valid = new QSValidator<RouteList>(Entity, validationContext);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
				return false;
			}
		
			if(Entity.Status > RouteListStatus.OnClosing) {
				if(Entity.FuelOperationHaveDiscrepancy()) {
					if(!MessageDialogHelper.RunQuestionDialog("Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?")) {
						return false;
					}
				}
				Entity.UpdateFuelOperation();
			}
			
			if(Entity.Status == RouteListStatus.Delivered) {
				Entity.ChangeStatusAndCreateTask(Entity.Car.IsCompanyCar && Entity.Car.TypeOfUse != CarTypeOfUse.CompanyTruck ? RouteListStatus.MileageCheck : RouteListStatus.OnClosing, CallTaskWorker);
			}
			Entity.CalculateWages(_wageParameterService);

			UoWGeneric.Save();

			return true;
		}
		#endregion

		#region Обработка нажатий кнопок

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			var validationContext = new Dictionary<object, object> {
				{ "NewStatus", RouteListStatus.Closed },
				{ nameof(IRouteListItemRepository), new EntityRepositories.Logistic.RouteListItemRepository() }
			};
			var valid = new QSValidator<RouteList>(Entity, validationContext);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
				return;
			}

			if(Entity.Status == RouteListStatus.Delivered) {
				Entity.ChangeStatusAndCreateTask(Entity.Car.IsCompanyCar && Entity.Car.TypeOfUse != CarTypeOfUse.CompanyTruck ? RouteListStatus.MileageCheck : RouteListStatus.OnClosing, CallTaskWorker);
			}
			Entity.AcceptMileage(CallTaskWorker);

			UpdateStates();
			SaveAndClose();
		}

		private void UpdateStates()
		{
			buttonAccept.Sensitive = Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck;
		}

		protected void OnButtonOpenMapClicked(object sender, EventArgs e)
		{
			var trackWnd = new TrackOnMapWnd(UoWGeneric);
			trackWnd.Show();
		}

		protected void OnButtonFromTrackClicked(object sender, EventArgs e)
		{
			var track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);
			if(track == null) {
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно расчитать растояние, так как в маршрутном листе нет трека", "");
				return;
			}
			Entity.ConfirmedDistance = (decimal)track.TotalDistance.Value;
		}
		
		private void ButtonAcceptFineOnClicked(object sender, EventArgs e)
		{
			string fineReason = "Перерасход топлива";

			var fineDlg = new FineDlg(0, Entity, fineReason, Entity.Date, Entity.Driver);
			fineDlg.Entity.FineType = FineTypes.FuelOverspending;
			fineDlg.EntitySaved += OnFinesAdded;
			
			TabParent.AddSlaveTab(this, fineDlg);
		}

		#endregion

        #region Обработка добавления долгов

        protected void OnFinesAdded(object sender, EventArgs e)
        {
	        HasChanges = true;
        }
        
        #endregion
	}
}

