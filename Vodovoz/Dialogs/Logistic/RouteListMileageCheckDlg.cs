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
using Vodovoz.Repository.Logistics;
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
using QS.Osm;
using QS.Osm.Osrm;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Parameters;
using RouteListRepository = Vodovoz.EntityRepositories.Logistic.RouteListRepository;

namespace Vodovoz
{
	public partial class RouteListMileageCheckDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		#region Поля

		bool editing = true;

		List<RouteListKeepingItemNode> items;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						OrderSingletonRepository.GetInstance(),
						EmployeeSingletonRepository.GetInstance(),
						new BaseParametersProvider(),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

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

			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();

			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
		
			referenceLogistican.RepresentationModel = new EmployeesVM();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoWGeneric);
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
				RecountMileage();
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

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			if(Entity.Status > RouteListStatus.OnClosing) {
				if(Entity.FuelOperationHaveDiscrepancy()) {
					if(!MessageDialogHelper.RunQuestionDialog("Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?")) {
						return false;
					}
				}
				Entity.UpdateFuelOperation();
			}
			
			if (Entity.Status == RouteListStatus.Delivered && HasChanges)
			{
				Entity.ChangeStatusAndCreateTask(RouteListStatus.MileageCheck, CallTaskWorker);
			}

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
			var track = TrackRepository.GetTrackForRouteList(UoW, Entity.Id);
			if(track == null) {
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно расчитать растояние, так как в маршрутном листе нет трека", "");
				return;
			}
			Entity.ConfirmedDistance = (decimal)track.TotalDistance.Value;
		}

		#endregion

        #region Обработка добавления долгов

        protected void OnFinesAdded(object sender, EventArgs e)
        {
	        HasChanges = true;
        }
        
        #endregion

		private void RecountMileage()
		{
			var pointsToRecalculate = new List<PointOnEarth>();
			var pointsToBase = new List<PointOnEarth>();
			var baseLat = (double)Entity.GeographicGroups.FirstOrDefault().BaseLatitude.Value;
			var baseLon = (double)Entity.GeographicGroups.FirstOrDefault().BaseLongitude.Value;

			decimal totalDistanceTrack = 0;

			IEnumerable<RouteListItem> completedAddresses = Entity.Addresses.Where(x => x.Status == RouteListItemStatus.Completed);

			if(!completedAddresses.Any()) {
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Для МЛ нет завершенных адресов, невозможно расчитать трек", "");
				return;
			}

			if(completedAddresses.Count() > 1) {
				foreach(RouteListItem address in Entity.Addresses.OrderBy(x => x.StatusLastUpdate)) {
					if(address.Status == RouteListItemStatus.Completed) {
						pointsToRecalculate.Add(new PointOnEarth((double)address.Order.DeliveryPoint.Latitude, (double)address.Order.DeliveryPoint.Longitude));
					}
				}

				var recalculatedTrackResponse = OsrmMain.GetRoute(pointsToRecalculate, false, GeometryOverview.Full);
				var recalculatedTrack = recalculatedTrackResponse.Routes.First();

				totalDistanceTrack = recalculatedTrack.TotalDistanceKm;
			} else {
				var point = Entity.Addresses.First(x => x.Status == RouteListItemStatus.Completed).Order.DeliveryPoint;
				pointsToRecalculate.Add(new PointOnEarth((double)point.Latitude, (double)point.Longitude));
			}

			pointsToBase.Add(pointsToRecalculate.Last());
			pointsToBase.Add(new PointOnEarth(baseLat, baseLon));
			pointsToBase.Add(pointsToRecalculate.First());

			var recalculatedToBaseResponse = OsrmMain.GetRoute(pointsToBase, false, GeometryOverview.Full);
			var recalculatedToBase = recalculatedToBaseResponse.Routes.First();

			Entity.RecalculatedDistance = decimal.Round(totalDistanceTrack + recalculatedToBase.TotalDistanceKm);
		}
	}
}

