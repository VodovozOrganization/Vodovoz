using System;
using System.Collections.Generic;
using Dialogs.Logistic;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListMileageCheckDlg : OrmGtkDialogBase<RouteList>
	{
		#region Поля

		private bool editing = true;
		private bool editingAdmin = true;

		List<RouteListKeepingItemNode> items;

		#endregion

		public RouteListMileageCheckDlg(int id)
		{
			this.Build();
			editing = QSMain.User.Permissions["logistican"];
			editingAdmin = QSMain.User.Permissions["logistic_admin"];
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Контроль за километражом маршрутного листа №{0}", Entity.Id);
			ConfigureDlg();
		}

		#region Настройка конфигураций

		public void ConfigureDlg()
		{
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.Sensitive = false;

			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.Sensitive = false;

			var filterLogistican = new EmployeeFilter(UoW);
			filterLogistican.RestrictFired = false;
			referenceLogistican.RepresentationModel = new EmployeesVM(filterLogistican);
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.Sensitive = editing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoWGeneric);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = editing;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = false;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			yspinConfirmedDistance.Binding.AddBinding(Entity, rl => rl.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinConfirmedDistance.Sensitive = editing && Entity.Status != RouteListStatus.Closed;
			buttonConfirm.Sensitive = editing && Entity.Status != RouteListStatus.Closed;

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



			if(Entity.Status == RouteListStatus.MileageCheck) {
				buttonCloseRouteList.Sensitive = editing;
			} else if(editingAdmin) {
				buttonCloseRouteList.Sensitive = true;
			} else
				buttonCloseRouteList.Sensitive = false;
		}

		#endregion

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			if(Entity.Status > RouteListStatus.OnClosing) {
				if(Entity.FuelOperationHaveDiscrepancy()) {
					if(!MessageDialogWorks.RunQuestionDialog("Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?")) {
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
		protected void OnButtonConfirmClicked(object sender, EventArgs e)
		{
			Entity.ConfirmedDistance = Entity.ActualDistance;
		}

		protected void OnButtonCloseRouteListClicked(object sender, EventArgs e)
		{
			var valid = new QSValidator<RouteList>(Entity,
							 new Dictionary<object, object>
				{
					{ "NewStatus", RouteListStatus.Closed }
				});
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			if(Entity.ConfirmedDistance < Entity.ActualDistance && !Entity.Car.IsCompanyHavings) {
				decimal excessKM = Entity.ActualDistance - Entity.ConfirmedDistance;
				decimal redundantPayForFuel = Entity.GetLitersOutlayed(excessKM) * Entity.Car.FuelType.Cost;
				string fineReason = "Перевыплата топлива";
				var fine = new Fine();
				fine.Fill(redundantPayForFuel, Entity, fineReason, DateTime.Today, Entity.Driver);
				fine.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
				fine.UpdateWageOperations(UoWGeneric);
				UoWGeneric.Save(fine);
			} else if(Entity.ConfirmedDistance > Entity.ActualDistance) {
				Entity.RecalculateFuelOutlay();
			}

			yspinConfirmedDistance.Sensitive = false;
			buttonConfirm.Sensitive = false;
			buttonCloseRouteList.Sensitive = false;

			Entity.ConfirmMileage(UoWGeneric);
		}

		protected void OnButtonOpenMapClicked(object sender, EventArgs e)
		{
			var trackWnd = new TrackOnMapWnd(UoWGeneric);
			trackWnd.Show();
		}

		#endregion
	}
}

