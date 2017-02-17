using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using GMap.NET;
using System.Linq;
using GMap.NET.GtkSharp.Markers;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageCheckDlg : OrmGtkDialogBase<RouteList>
	{
		#region Поля

		private bool editing = true;

		List<RouteListKeepingItemNode> items;

		#endregion

		public RouteListMileageCheckDlg(int id, bool canEdit = true)
		{
			this.Build ();
			editing = canEdit;
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Контроль за километражом маршрутного листа №{0}",Entity.Id);
			ConfigureDlg ();
		}

		#region Настройка конфигураций

		public void ConfigureDlg(){
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = editing;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = editing;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = editing;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = editing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = editing;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = editing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			yspinConfirmedDistance.Binding.AddBinding(Entity, rl => rl.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinConfirmedDistance.Sensitive = editing;

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
				.RowCells ()
				.AddSetter<CellRenderer> ((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();

			items = new List<RouteListKeepingItemNode>();
			foreach (var item in Entity.Addresses)
				items.Add(new RouteListKeepingItemNode{RouteListItem=item});

			items.Sort((x, y) => {
				if(x.RouteListItem.StatusLastUpdate.HasValue && y.RouteListItem.StatusLastUpdate.HasValue){
					if(x.RouteListItem.StatusLastUpdate > y.RouteListItem.StatusLastUpdate) return 1;
					if(x.RouteListItem.StatusLastUpdate < y.RouteListItem.StatusLastUpdate) return -1;
				}
				return 0;
			} );

			ytreeviewAddresses.ItemsDataSource = items;
		}

		#endregion

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			UoWGeneric.Save();
			return true;
		}
		#endregion

		#region Обработка нажатий кнопок
		protected void OnButtonConfirmClicked (object sender, EventArgs e)
		{
			Entity.ConfirmedDistance = Entity.ActualDistance;
		}

		protected void OnButtonCloseRouteListClicked (object sender, EventArgs e)
		{
			var valid = new QSValidator<RouteList>(Entity, 
				             new Dictionary<object, object>
				{
					{ "NewStatus", RouteListStatus.Closed }
				});
			if (valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			if(Entity.ConfirmedDistance != Entity.ActualDistance)
			{
				Entity.RecalculateFuelOutlay();
			}

			yspinConfirmedDistance.Sensitive = false;
			buttonConfirm.Sensitive = false;
			buttonCloseRouteList.Sensitive = false;

			Entity.ConfirmMileage();
		}

		protected void OnButtonOpenMapClicked (object sender, EventArgs e)
		{
			var trackWnd = new TrackMapWnd(Entity.Id);
			trackWnd.Show();
		}

		#endregion
	}
}

