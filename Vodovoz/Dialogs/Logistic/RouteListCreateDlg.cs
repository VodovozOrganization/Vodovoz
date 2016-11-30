using System;
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
		public bool transfer;

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

		public RouteListCreateDlg (RouteList routeList, IEnumerable<RouteListItem> addresses)
		{
			this.Build();
			transfer = true;
			buttonAccept.Sensitive = false;
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList>();

			Entity.Forwarder = routeList.Forwarder;
			Entity.Shift = routeList.Shift;
			Entity.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.Status = routeList.Status;
			for (var address = addresses.GetEnumerator(); address.MoveNext();)
			{
				var newAddress = Entity.AddAddressFromOrder(address.Current.Order);
				newAddress.Comment = address.Current.Comment;
			}
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
			subjectAdaptor.Target = UoWGeneric.Root;

			dataRouteList.DataSource = subjectAdaptor;

			referenceCar.SubjectType = typeof(Car);

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.PropertyMapping<RouteList> (r => r.Driver);
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.PropertyMapping<RouteList> (r => r.Forwarder);
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			referenceLogistican.PropertyMapping<RouteList> (r => r.Logistican);
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			speccomboShift.Mappings = Entity.GetPropertyName (r => r.Shift);
			speccomboShift.ColumnMappings = PropertyUtil.GetName<DeliveryShift> (s => s.Name);
			speccomboShift.ItemsDataSource = DeliveryShiftRepository.ActiveShifts (UoW);

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
			IsEditable (UoWGeneric.Root.Status == RouteListStatus.New || transfer);

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments) e.ItemEnum);

			buttonChangeToEnRoute.Sensitive = (Entity.Status == RouteListStatus.New
											|| Entity.Status == RouteListStatus.InLoading);
		}

		private void PrintSelectedDocument (RouteListPrintableDocuments choise)
		{
			QSReport.ReportInfo document = null;

			switch (choise)
			{
				case RouteListPrintableDocuments.All:
					PrintRouteListHelper.Print(UoW, Entity.Id, this);
					break;
				case RouteListPrintableDocuments.LoadDocument:
					document = PrintRouteListHelper.GetRDLLoadDocument(Entity.Id);
					break;
				case RouteListPrintableDocuments.RouteList:
					document = PrintRouteListHelper.GetRDLRouteList(UoW, Entity.Id);
					break;
				case RouteListPrintableDocuments.TimeList:
					document = PrintRouteListHelper.GetRDLTimeList(Entity.Id);
					break;
			}

			if (document != null)
			{
				this.TabParent.OpenTab(
					QSTDI.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => new QSReport.ReportViewDlg(document, true));
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

		private void IsEditable (bool val = false)
		{
			speccomboShift.Sensitive = val;
			datepickerDate.Sensitive = referenceCar.Sensitive = referenceForwarder.Sensitive = val;
			spinPlannedDistance.Sensitive = val;
			createroutelistitemsview1.IsEditable (val);
		}

		private void UpdateButtonStatus()
		{
			if(Entity.Status == RouteListStatus.New)
			{
				IsEditable (true);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				enumPrint.Sensitive = false;
				buttonAccept.Label = "Подтвердить";
			}
			if(Entity.Status == RouteListStatus.InLoading)
			{
				IsEditable (transfer);
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
				var valid = new QSValidator<RouteList> (UoWGeneric.Root, 
					            new Dictionary<object, object> {
						{ "NewStatus", RouteListStatus.InLoading }
					});
				if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
					return;

				UoWGeneric.Root.ChangeStatus(RouteListStatus.InLoading);

				foreach (var address in UoWGeneric.Root.Addresses)
				{
					address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.OnLoading);
				}
				Save();

				//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
				var forShipment = Repository.Store.WarehouseRepository.WarehouseForShipment (UoW, Entity.Id);
				if(forShipment.Count == 0)
				{
					if (MessageDialogWorks.RunQuestionDialog("Для маршрутного листа, нет необходимости грузится на складе. Перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.EnRoute.GetEnumTitle()))
					{
						valid = new QSValidator<RouteList> (UoWGeneric.Root, 
							new Dictionary<object, object> {
							{ "NewStatus", RouteListStatus.EnRoute }
						});
						if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
						{
							Entity.ChangeStatus(RouteListStatus.New);
						}
						else
						{
							Entity.ChangeStatus(RouteListStatus.EnRoute);
						}
					}
					else
					{
						Entity.ChangeStatus(RouteListStatus.New);
					}
					Save();
					UpdateButtonStatus();
					return;
				}

				UpdateButtonStatus();
				return;
			}
			if (UoWGeneric.Root.Status == RouteListStatus.InLoading) {
				UoWGeneric.Root.ChangeStatus(RouteListStatus.New);
				UpdateButtonStatus();
				return;
			}
		}

		protected void OnButtonChangeToEnRouteClicked (object sender, EventArgs e)
		{
			bool status = Entity.Status == RouteListStatus.New || Entity.Status == RouteListStatus.InLoading;

			if (status)
			{
				if (Entity.Status == RouteListStatus.New)
					Entity.ChangeStatus(RouteListStatus.InLoading);
				if (Entity.Status == RouteListStatus.InLoading)
					Entity.ChangeStatus(RouteListStatus.EnRoute);
			}
		}
	}
}