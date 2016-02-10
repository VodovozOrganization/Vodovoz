using System;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain;
using Vodovoz.Repository.Logistics;
using Gamma.GtkWidgets;
using System.Collections.Generic;
using Gtk;
using System.Globalization;
using System.Linq;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListKeepingDlg : OrmGtkDialogBase<RouteList>
	{		
		public RouteListKeepingDlg(int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Ведение маршрутного листа №{0}",Entity.Id);
			ConfigureDlg ();
		}

		List<RouteListKeepingItemNode> items;

		public void ConfigureDlg(){
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = false;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = false;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = false;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = false;

			yspinPlannedDistance.Binding.AddBinding(Entity, rl => rl.PlannedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinPlannedDistance.Sensitive = false;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = false;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = false;

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())					
				.AddColumn("Адрес")
					.AddTextRenderer(node => String.Format("{0} д.{1}", node.RouteListItem.Order.DeliveryPoint.Street, node.RouteListItem.Order.DeliveryPoint.Building))					
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)					
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.Status).Editing(true)					
				.AddColumn("Последнее редактирование")
					.AddTextRenderer(node => node.LastUpdate)
				.RowCells ()
					.AddSetter<CellRenderer> ((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();
			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			ytreeviewAddresses.Selection.Changed += OnSelectionChanged;
			UpdateNodes();
		}

		public void UpdateNodes(){
			items = new List<RouteListKeepingItemNode>();
			foreach (var item in Entity.Addresses)
				items.Add(new RouteListKeepingItemNode{ RouteListItem = item });
			ytreeviewAddresses.ItemsDataSource = items;
		}

		public void OnSelectionChanged(object sender, EventArgs args){
			buttonNewRouteList.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() > 0;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			foreach (var address in items.Where(item=>item.HasChanged).Select(item=>item.RouteListItem))
			{
				switch (address.Status)
				{
					case RouteListItemStatus.Canceled:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.Canceled);
						break;
					case RouteListItemStatus.Completed:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.Shipped);
						break;
					case RouteListItemStatus.EnRoute:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.OnTheWay);
						break;
					case RouteListItemStatus.Overdue:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.NotDelivered);
						break;
				}
				UoWGeneric.Save(address.Order);
			}
			UoWGeneric.Save();
			return true;
		}
		#endregion

		protected void OnButtonNewRouteListClicked (object sender, EventArgs e)
		{
			if (TabParent.CheckClosingSlaveTabs(this))
				return;
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(RouteList)))
			{
				if (!Save())
					return;
			}
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			var selectedAddreses = selectedObjects
				.Cast<RouteListKeepingItemNode>()
				.Select(item=>item.RouteListItem)
				.Where(item=>item.Status==RouteListItemStatus.EnRoute);

			var dlg = new RouteListCreateDlg(Entity, selectedAddreses);
			dlg.EntitySaved += OnNewRouteListCreated;
			TabParent.AddSlaveTab(this,dlg);
		}

		public void OnNewRouteListCreated(object sender, EntitySavedEventArgs args){
			var newRouteList = args.Entity as RouteList;
			foreach (var address in newRouteList.Addresses)
			{
				var transferedAddress = Entity.ObservableAddresses.FirstOrDefault(item => item.Order.Id == address.Order.Id);
				if (transferedAddress != null)
					Entity.RemoveAddress(transferedAddress);
			}
			UpdateNodes();
			Save();
		}
	}	

	public class RouteListKeepingItemNode {
		public bool HasChanged=false;
		public Gdk.Color RowColor{
			get{
				switch (RouteListItem.Status){						
					case RouteListItemStatus.Overdue:							
						return new Gdk.Color(0xee,0x66,0x66);
					case RouteListItemStatus.Completed:
						return new Gdk.Color(0x66,0xee,0x66);
					case RouteListItemStatus.Canceled:
						return new Gdk.Color(0xaf,0xaf,0xaf);
					default:
						return new Gdk.Color(0xff,0xff,0xff);
				}
			}
		}

		public RouteListItemStatus Status{
			get{
				return RouteListItem.Status;
			}
			set{
				RouteListItem.Status = value;
				HasChanged = true;
				RouteListItem.StatusLastUpdate = DateTime.Now;
			}
		}

		public string LastUpdate {
			get{
				var maybeLastUpdate = RouteListItem.StatusLastUpdate;
				if (maybeLastUpdate.HasValue)
				{
					if (maybeLastUpdate.Value.Date == DateTime.Today)
					{
						return maybeLastUpdate.Value.ToShortTimeString();
					}
					else
						return maybeLastUpdate.Value.ToString();
				}
				else
				{
					return "";
				}
			}
		}

		public RouteListItem RouteListItem{get;set;}
	}
}

