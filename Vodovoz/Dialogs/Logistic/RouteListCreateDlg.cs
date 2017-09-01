﻿using System;
using System.Collections.Generic;
using Gamma.Utilities;
using Gtk;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using Vodovoz.Additions.Logistic;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable;

		protected bool IsEditable{
			get { return isEditable;}
			set{
				isEditable = value;
				speccomboShift.Sensitive = isEditable;
				datepickerDate.Sensitive = referenceCar.Sensitive = referenceForwarder.Sensitive = isEditable;
				createroutelistitemsview1.IsEditable (isEditable);
			}
		}


		public RouteListCreateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (Entity.Logistican == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListCreateDlg (RouteList sub) : this(sub.Id) {}

		public RouteListCreateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datepickerDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			referenceCar.SubjectType = typeof(Car);
			referenceCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			referenceCar.ChangedByUser += (sender, e) => {
				Entity.Driver = Entity.Car.Driver;
				referenceDriver.Sensitive = Entity.Driver == null || Entity.Car.IsCompanyHavings ? true : false;
				//Водители на Авто компании катаются без экспедитора
				Entity.Forwarder = Entity.Car.IsCompanyHavings ? null : Entity.Forwarder;
				referenceForwarder.Sensitive = !Entity.Car.IsCompanyHavings;
			};

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			//SubjectType не подхватывается автоматически
			referenceLogistican.SubjectType = typeof(Employee);
			referenceLogistican.Binding.AddBinding(Entity, e => e.Logistican, w => w.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts (UoW);
			speccomboShift.Binding.AddBinding(Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();

			referenceDriver.Sensitive = false;
			enumPrint.Sensitive = UoWGeneric.Root.Status != RouteListStatus.New;
			createroutelistitemsview1.RouteListUoW = UoWGeneric;

			buttonAccept.Visible = (UoWGeneric.Root.Status == RouteListStatus.New || UoWGeneric.Root.Status == RouteListStatus.InLoading);
			if (UoWGeneric.Root.Status == RouteListStatus.InLoading) {
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			IsEditable = UoWGeneric.Root.Status == RouteListStatus.New && QSMain.User.Permissions ["logistican"];

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments) e.ItemEnum);
		}

		private void PrintSelectedDocument (RouteListPrintableDocuments choise)
		{
			QSReport.ReportInfo document = PrintRouteListHelper.GetRDL(Entity, choise, UoW);

			if (document != null)
			{
				this.TabParent.OpenTab(
					QSTDI.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => new QSReport.ReportViewDlg(document));
			}
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем маршрутный лист...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		private void UpdateButtonStatus()
		{
			if(Entity.Status == RouteListStatus.New)
			{
				IsEditable = (true);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				enumPrint.Sensitive = false;
				buttonAccept.Label = "Подтвердить";
			}
			if(Entity.Status == RouteListStatus.InLoading)
			{
				IsEditable = (false);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				enumPrint.Sensitive = true;
				buttonAccept.Label = "Редактировать";
			}
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{

			if (UoWGeneric.Root.Status == RouteListStatus.New) {
				var valid = new QSValidator<RouteList>(UoWGeneric.Root,
								new Dictionary<object, object> {
						{ "NewStatus", RouteListStatus.InLoading }
					});
				if(valid.RunDlgIfNotValid((Window)this.Toplevel))
					return;

				UoWGeneric.Root.ChangeStatus(RouteListStatus.InLoading);

				foreach(var address in UoWGeneric.Root.Addresses) {
					if(address.Order.OrderStatus < Domain.Orders.OrderStatus.OnLoading)
						address.Order.ChangeStatus(Domain.Orders.OrderStatus.OnLoading);
				}
				Save();

				//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
				var forShipment = Repository.Store.WarehouseRepository.WarehouseForShipment(UoW, Entity.Id);
				if((forShipment.Count == 0 || TransferredReloadCheck(forShipment)) && UoWGeneric.Root.Car.TypeOfUse != CarTypeOfUse.Truck) {
					if(MessageDialogWorks.RunQuestionDialog("Для маршрутного листа, нет необходимости грузится на складе. Перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.EnRoute.GetEnumTitle())) {
						valid = new QSValidator<RouteList>(UoWGeneric.Root,
							new Dictionary<object, object> {
							{ "NewStatus", RouteListStatus.EnRoute }
						});
						if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
							Entity.ChangeStatus(RouteListStatus.New);
						} else {
							Entity.ChangeStatus(RouteListStatus.EnRoute);
						}
					} else {
						Entity.ChangeStatus(RouteListStatus.New);
					}
				} else if(UoWGeneric.Root.Car.TypeOfUse == CarTypeOfUse.Truck && MessageDialogWorks.RunQuestionDialog("Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.OnClosing.GetEnumTitle())) {
					Entity.ChangeStatus(RouteListStatus.OnClosing);
					foreach(var item in UoWGeneric.Root.Addresses) {
						item.Order.OrderStatus = Domain.Orders.OrderStatus.OnTheWay;
					}
					Entity.CompleteRoute();

				}
				Save();
				UpdateButtonStatus();
				return;
			}
			if (UoWGeneric.Root.Status == RouteListStatus.InLoading) {
				UoWGeneric.Root.ChangeStatus(RouteListStatus.New);
				UpdateButtonStatus();
				return;
			}
		}

		bool TransferredReloadCheck(IList<Domain.Store.Warehouse> forShipment)
		{
			if(forShipment.Count == 0)
				return false;
			
			foreach(var adress in UoWGeneric.Root.Addresses)  
				if(adress.Order.OrderStatus == Domain.Orders.OrderStatus.OnTheWay && adress.NeedToReload == true)
					return false;
			 
			return true;
		}
	}
}