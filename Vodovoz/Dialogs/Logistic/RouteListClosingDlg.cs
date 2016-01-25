using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Gtk;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;
using QSProjectsLib;
using Vodovoz.Repository.Logistics;
using System.IO;
using QSReport;
using QSTDI;
using Gamma.Utilities;
using NHibernate.Criterion;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public RouteListClosingDlg ()
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

		public RouteListClosingDlg (RouteList sub) : this (sub.Id)
		{
		}

		public RouteListClosingDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			referenceCar.SubjectType = typeof(Car);
			referenceCar.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = false;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = false;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = false;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoWGeneric);
			speccomboShift.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = false;

			yspinPlannedDistance.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.PlannedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinPlannedDistance.Sensitive = false;

			yspinActualDistance.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.IsEditable = true;

			datePickerDate.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = false;

			routelistclosingitemsview1.RouteListUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			UoWGeneric.Root.Status = RouteListStatus.Closed;
			foreach (var item in UoWGeneric.Root.Addresses)
			{
				//item.Order.OrderStatus = Vodovoz.Domain.Orders.OrderStatus.Closed; //??
			}
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}			

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			Save();
		}

	}
}
