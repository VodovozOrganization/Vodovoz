using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
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
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
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
			referenceCar.ItemsQuery = CarRepository.ActiveCarsQuery();
			referenceCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			referenceCar.ChangedByUser += (sender, e) => {
				if(Entity.Car != null) {
					Entity.Driver = (Entity.Car.Driver != null && !Entity.Car.Driver.IsFired) ? Entity.Car.Driver : null;
					referenceDriver.Sensitive = Entity.Driver == null || Entity.Car.IsCompanyHavings;
					//Водители на Авто компании катаются без экспедитора
					Entity.Forwarder = Entity.Car.IsCompanyHavings ? null : Entity.Forwarder;
					referenceForwarder.IsEditable = !Entity.Car.IsCompanyHavings;
				}
			};

			var filterDriver = new EmployeeFilter(UoW, false);
			filterDriver.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			var filter = new EmployeeFilter(UoW, false);
			filter.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.forwarder);
			referenceForwarder.RepresentationModel = new ViewModel.EmployeesVM(filter);
			referenceForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();
			referenceForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			var filterLogistican = new EmployeeFilter(UoW);
			referenceLogistican.RepresentationModel = new EmployeesVM(filterLogistican);
			referenceLogistican.Binding.AddBinding(Entity, e => e.Logistican, w => w.Subject).InitializeFromSource();

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts (UoW);
			speccomboShift.Binding.AddBinding(Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();

			referenceDriver.Sensitive = false;
			enumPrint.Sensitive = UoWGeneric.Root.Status != RouteListStatus.New;

			if(Entity.Id > 0)
			{
				//Нужно только для быстрой загрузки данных диалога. Проверено на МЛ из 200 заказов. Разница в скорости в несколько раз.
				var orders = UoW.Session.QueryOver<RouteListItem>()
								.Where(x => x.RouteList == Entity)
								.Fetch(x => x.Order).Eager
				                .Fetch(x => x.Order.OrderItems).Eager
				                .List();
			}

			createroutelistitemsview1.RouteListUoW = UoWGeneric;

			buttonAccept.Visible = (UoWGeneric.Root.Status == RouteListStatus.New || UoWGeneric.Root.Status == RouteListStatus.InLoading);
			if (UoWGeneric.Root.Status == RouteListStatus.InLoading) {
				var icon = new Image {
					Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
				};
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			IsEditable = UoWGeneric.Root.Status == RouteListStatus.New && QSMain.User.Permissions ["logistican"];

			//FIXME костыли, необходимо избавится от этого кода когда решим проблему с сессиями и flush nhibernate
			HasChanges = true;
			UoW.CanCheckIfDirty = false;

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.SetVisibility(RouteListPrintableDocuments.LoadSofiyskaya, false);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments) e.ItemEnum);
			CheckCarLoadDocuments();
		}

		void CheckCarLoadDocuments()
		{
			if(Entity.Id > 0 && RouteListRepository.GetCarLoadDocuments(UoW, Entity.Id).Any())
				IsEditable = false;
		}

		void PrintSelectedDocument (RouteListPrintableDocuments choise)
		{
			TabParent.OpenTab(
				QS.Dialog.Gtk.TdiTabBase.GenerateHashName<DocumentsPrinterDlg>(),
				() => CreateDocumentsPrinterDlg(choise)
			);
		}

		DocumentsPrinterDlg CreateDocumentsPrinterDlg(RouteListPrintableDocuments choise)
		{
			var dlg = new DocumentsPrinterDlg(UoW, Entity, choise);
			dlg.DocumentsPrinted += Dlg_DocumentsPrinted;
			return dlg;
		}

		void Dlg_DocumentsPrinted(object sender, EventArgs e)
		{
			if(!Entity.Printed && e is EndPrintArgs) {
				var printArgs = e as EndPrintArgs;
				if(printArgs.Args.Cast<IPrintableDocument>().Any(d => d.Name == RouteListPrintableDocuments.RouteList.GetEnumTitle())) {
					Entity.Printed = true;
					Save();
				}
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
			switch(Entity.Status) {
				case RouteListStatus.New: {
						IsEditable = (true);
						var icon = new Image {
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = false;
						buttonAccept.Label = "Подтвердить";
						break;
					}
				case RouteListStatus.InLoading: {
						IsEditable = (false);
						var icon = new Image {
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = true;
						buttonAccept.Label = "Редактировать";
						break;
					}
			}
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			if(buttonAccept.Label == "Подтвердить" && Entity.HasOverweight()) {
				if(QSMain.User.Permissions["can_confirm_routelist_with_overweight"]) {
					if(
						!MessageDialogHelper.RunQuestionDialog(
							String.Format(
								"Вы перегрузили '{0}' на {1} кг.\nВы уверены что хотите подтвердить маршрутный лист?",
								Entity.Car.Title,
								Entity.Overweight()
							)
						)
					)
						return;
				} else {
					MessageDialogHelper.RunWarningDialog(
						String.Format(
							"Вы перегрузили '{0}' на {1} кг.\nПодтвердить маршрутный лист нельзя.",
							Entity.Car.Title,
							Entity.Overweight()
						)
					);
					return;
				}
			}

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

				//Строим маршрут для МЛ.
				if(!Entity.Printed || MessageDialogHelper.RunQuestionWithTitleDialog("Перестроить маршрут?", "Этот маршрутный лист уже был когда-то напечатан. При новом построении маршрута порядок адресов может быть другой. При продолжении обязательно перепечатайте этот МЛ.\nПерестроить маршрут?")) {
					RouteOptimizer optimizer = new RouteOptimizer();
					var newRoute = optimizer.RebuidOneRoute(Entity);
					if(newRoute != null) {
						createroutelistitemsview1.DisableColumnsUpdate = true;
						newRoute.UpdateAddressOrderInRealRoute(Entity);
						//Рассчитываем расстояние
						Entity.RecalculatePlanedDistance(new Tools.Logistic.RouteGeometryCalculator(Tools.Logistic.DistanceProvider.Osrm));
						createroutelistitemsview1.DisableColumnsUpdate = false;
						var noPlan = Entity.Addresses.Count(x => !x.PlanTimeStart.HasValue);
						if(noPlan > 0)
							MessageDialogHelper.RunWarningDialog($"Для маршрута незапланировано {noPlan} адресов.");
					} else {
						MessageDialogHelper.RunWarningDialog($"Маршрут не был перестроен.");
					}
				}

				Save();

				if(UoWGeneric.Root.Car.TypeOfUse == CarTypeOfUse.Truck)
				{
					if(MessageDialogHelper.RunQuestionDialog("Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.OnClosing.GetEnumTitle()))
					{
						Entity.ChangeStatus(RouteListStatus.OnClosing);
						foreach(var item in UoWGeneric.Root.Addresses) {
							item.Order.OrderStatus = Domain.Orders.OrderStatus.OnTheWay;
						}
						Entity.CompleteRoute();
					}
				}
				else
				{
					//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
					var forShipment = Repository.Store.WarehouseRepository.WarehouseForShipment(UoW, Entity.Id);
					if(forShipment.Count == 0) {
						if(MessageDialogHelper.RunQuestionDialog("Для маршрутного листа, нет необходимости грузится на складе. Перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.EnRoute.GetEnumTitle())) {
							valid = new QSValidator<RouteList>(
								UoWGeneric.Root,
								new Dictionary<object, object> {
									{ "NewStatus", RouteListStatus.EnRoute }
								}
							);

							Entity.ChangeStatus(valid.RunDlgIfNotValid((Window)this.Toplevel) ? RouteListStatus.New : RouteListStatus.EnRoute);
						} else {
							Entity.ChangeStatus(RouteListStatus.New);
						}
					}
				}
				Save();
				UpdateButtonStatus();
				return;
			}
			if (UoWGeneric.Root.Status == RouteListStatus.InLoading) {
				if(RouteListRepository.GetCarLoadDocuments(UoW, Entity.Id).Any()) {
					MessageDialogHelper.RunErrorDialog("Для маршрутного листа были созданы документы погрузки. Сначала необходимо удалить их.");
				}else {
					UoWGeneric.Root.ChangeStatus(RouteListStatus.New);
				}
				UpdateButtonStatus();
				return;
			}
		}

		protected void OnReferenceCarChanged(object sender, EventArgs e)
		{
			createroutelistitemsview1.UpdateWeightInfo();
		}
	}
}