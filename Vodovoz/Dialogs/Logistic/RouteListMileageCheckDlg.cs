using System;
using System.Collections.Generic;
using System.Linq;
using Dialogs.Logistic;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSValidation;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListMileageCheckDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		#region Поля

		private bool editing = true;

		List<RouteListKeepingItemNode> items;

		#endregion

		public RouteListMileageCheckDlg(int id)
		{
			this.Build();
			editing = UserPermissionRepository.CurrentUserPresetPermissions["logistican"];
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = string.Format("Контроль за километражом маршрутного листа №{0}", Entity.Id);
			var canConfirmMileage = UserPermissionRepository.CurrentUserPresetPermissions["can_confirm_mileage_for_our_GAZelles_Larguses"];
			editing &= canConfirmMileage || !(Entity.Car.TypeOfUse.HasValue && Entity.Car.IsCompanyHavings && new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus }.Contains(Entity.Car.TypeOfUse.Value));

			ConfigureDlg();
		}

		#region Настройка конфигураций

		public void ConfigureDlg()
		{
			if(!editing) {
				MessageDialogHelper.RunWarningDialog("Не достаточно прав. Обратитесь к руководителю.");
				UoWGeneric.CanCheckIfDirty = false;
				HasChanges = false;
				vbxMain.Sensitive = false;
			}
			referenceCar.SubjectType = typeof(Car);
			referenceCar.ItemsQuery = CarRepository.ActiveCarsQuery();
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();

			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();

			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();

			var filterLogistican = new EmployeeFilter(UoW) {
				ShowFired = false
			};
			referenceLogistican.RepresentationModel = new EmployeesVM(filterLogistican);
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoWGeneric);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();

			yspinConfirmedDistance.Binding.AddBinding(Entity, rl => rl.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();

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
			entryMileageComment.Binding.AddBinding(Entity, x => x.MileageComment, w => w.Text).InitializeFromSource();
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

			UoWGeneric.Save();

			return true;
		}
		#endregion

		#region Обработка нажатий кнопок

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			var validationContext = new Dictionary<object, object> {
				{ "NewStatus", RouteListStatus.Closed }
			};
			var valid = new QSValidator<RouteList>(Entity, validationContext);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
				return;
			}

			Entity.AcceptMileage();

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
			Entity.ConfirmedDistance = (decimal)track.TotalDistance.Value;
		}

		#endregion
	}
}

