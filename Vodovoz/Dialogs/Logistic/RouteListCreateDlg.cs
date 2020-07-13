using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private IWarehouseRepository warehouseRepository = new WarehouseRepository();
		private ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
		private IEmployeeRepository employeeRepository = EmployeeSingletonRepository.GetInstance();

		WageParameterService wageParameterService = new WageParameterService(WageSingletonRepository.GetInstance(), new BaseParametersProvider());

		bool isEditable;

		protected bool IsEditable {
			get => isEditable;
			set {
				isEditable = value;
				speccomboShift.Sensitive = isEditable;
				ggToStringWidget.Sensitive = datepickerDate.Sensitive = entityviewmodelentryCar.Sensitive = referenceForwarder.Sensitive = yspeccomboboxCashSubdivision.Sensitive = isEditable;
				createroutelistitemsview1.IsEditable(isEditable);
			}
		}

		public RouteListCreateDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList>();
			Entity.Logistican = employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Logistican == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}

			if(ConfigSubdivisionCombo()) {
				Entity.Date = DateTime.Now;
				ConfigureDlg();
			}
		}

		public RouteListCreateDlg(RouteList sub) : this(sub.Id) { }

		public RouteListCreateDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);

			if(ConfigSubdivisionCombo())
				ConfigureDlg();
		}

		private bool ConfigSubdivisionCombo()
		{
			var subdivisions = subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			if(!subdivisions.Any()) {
				MessageDialogHelper.RunErrorDialog("Не правильно сконфигурированы подразделения кассы, невозможно будет указать подразделение в которое будут сдаваться маршрутные листы");
				FailInitialize = true;
				return false;
			}
			yspeccomboboxCashSubdivision.ShowSpecialStateNot = true;
			yspeccomboboxCashSubdivision.ItemsList = subdivisions;
			yspeccomboboxCashSubdivision.SelectedItem = SpecialComboState.Not;
			yspeccomboboxCashSubdivision.ItemSelected += YspeccomboboxCashSubdivision_ItemSelected;

			if(Entity.ClosingSubdivision != null && subdivisions.Any(x => x.Id == Entity.ClosingSubdivision.Id))
				yspeccomboboxCashSubdivision.SelectedItem = Entity.ClosingSubdivision;

			return true;
		}

		private void ConfigureDlg()
		{
			datepickerDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.ChangedByUser += (sender, e) => {
				if(Entity.Car != null) {
					Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired) ? Entity.Car.Driver : null;
					referenceDriver.Sensitive = Entity.Driver == null || Entity.Car.IsCompanyCar;
					//Водители на Авто компании катаются без экспедитора
					Entity.Forwarder = Entity.Car.IsCompanyCar ? null : Entity.Forwarder;
					referenceForwarder.IsEditable = !Entity.Car.IsCompanyCar;
				}
			};

			var filterDriver = new EmployeeFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.CanChangeStatus = false
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.CanChangeStatus = false
			);
			referenceForwarder.RepresentationModel = new ViewModel.EmployeesVM(filter);
			referenceForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();
			referenceForwarder.Changed += (sender, args) => {
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			referenceLogistican.RepresentationModel = new EmployeesVM();
			referenceLogistican.Binding.AddBinding(Entity, e => e.Logistican, w => w.Subject).InitializeFromSource();

			speccomboShift.ItemsList = Repository.Logistics.DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();

			referenceDriver.Sensitive = false;
			enumPrint.Sensitive = Entity.Status != RouteListStatus.New;

			if(Entity.Id > 0) {
				//Нужно только для быстрой загрузки данных диалога. Проверено на МЛ из 200 заказов. Разница в скорости в несколько раз.
				var orders = UoW.Session.QueryOver<RouteListItem>()
								.Where(x => x.RouteList == Entity)
								.Fetch(x => x.Order).Eager
								.Fetch(x => x.Order.OrderItems).Eager
								.List();
			}

			createroutelistitemsview1.RouteListUoW = UoWGeneric;

			buttonAccept.Visible = Entity.Status == RouteListStatus.New || Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed;
			if(Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed) {
				var icon = new Image {
					Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
				};
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			IsEditable = Entity.Status == RouteListStatus.New && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");

			ggToStringWidget.UoW = UoW;
			ggToStringWidget.Label = "Район города:";
			ggToStringWidget.Binding.AddBinding(Entity, x => x.ObservableGeographicGroups, x => x.Items).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.SetVisibility(RouteListPrintableDocuments.LoadSofiyskaya, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.TimeList, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.OrderOfAddresses, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.LoadDocument, !(Entity.Status == RouteListStatus.Confirmed));
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments)e.ItemEnum);
			CheckCarLoadDocuments();
		}

		void YspeccomboboxCashSubdivision_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			Entity.ClosingSubdivision = yspeccomboboxCashSubdivision.SelectedItem as Subdivision;
		}

		void CheckCarLoadDocuments()
		{
			if(Entity.Id > 0 && new RouteListRepository().GetCarLoadDocuments(UoW, Entity.Id).Any())
				IsEditable = false;
		}

		void PrintSelectedDocument(RouteListPrintableDocuments choise)
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
			if(!Entity.Printed && e is EndPrintArgs printArgs) {
				if(printArgs.Args.Cast<IPrintableDocument>().Any(d => d.Name == RouteListPrintableDocuments.RouteList.GetEnumTitle())) {
					Entity.Printed = true;
					Save();
				}
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<RouteList>(Entity, new Dictionary<object, object>() { { nameof(IRouteListItemRepository), new RouteListItemRepository() } });
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем маршрутный лист...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		private void UpdateButtonStatus()
		{
			buttonAccept.Visible = true;

			switch(Entity.Status) {
				case RouteListStatus.New: {
						IsEditable = true;
						var icon = new Image {
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = false;
						buttonAccept.Label = "Подтвердить";
						break;
					}
				case RouteListStatus.Confirmed: {
						IsEditable = false;
						var icon = new Image {
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = true;
						buttonAccept.Label = "Редактировать";
						break;
					}
				case RouteListStatus.InLoading: {
						IsEditable = false;
						var icon = new Image {
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = true;
						buttonAccept.Label = "Редактировать";
						break;
					}
				default:
					buttonAccept.Visible = false;
					break;
			}
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения работы задачи и повторите");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
			buttonAccept.Sensitive = isSensetive;
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			try {
				SetSensetivity(false);
				var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					OrderSingletonRepository.GetInstance(),
					EmployeeSingletonRepository.GetInstance(),
					new BaseParametersProvider(),
					ServicesConfig.CommonServices.UserService,
					SingletonErrorReporter.Instance);

				if(Entity.Car == null) {
					MessageDialogHelper.RunWarningDialog("Не заполнен автомобиль");
					return;
				}
				StringBuilder warningMsg = new StringBuilder(string.Format("Автомобиль '{0}':", Entity.Car.Title));
				if(Entity.HasOverweight())
					warningMsg.Append(string.Format("\n\t- перегружен на {0} кг", Entity.Overweight()));
				if(Entity.HasVolumeExecess())
					warningMsg.Append(string.Format("\n\t- объём груза превышен на {0} м<sup>3</sup>", Entity.VolumeExecess()));

				if(buttonAccept.Label == "Подтвердить" && (Entity.HasOverweight() || Entity.HasVolumeExecess())) {
					if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_confirm_routelist_with_overweight")) {
						warningMsg.AppendLine("\nВы уверены что хотите подтвердить маршрутный лист?");
						if(!MessageDialogHelper.RunQuestionDialog(warningMsg.ToString())) {
							return;
						}
					} else {
						warningMsg.AppendLine("\nПодтвердить маршрутный лист нельзя.");
						MessageDialogHelper.RunWarningDialog(warningMsg.ToString());
						return;
					}
				}

				if(Entity.Status == RouteListStatus.New) {
					var valid = new QSValidator<RouteList>(Entity,
									new Dictionary<object, object> {
						{ "NewStatus", RouteListStatus.Confirmed },
						{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
						});
					if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
						return;
					}

					Entity.ChangeStatus(RouteListStatus.Confirmed, callTaskWorker);
					//Строим маршрут для МЛ.
					if(!Entity.Printed || MessageDialogHelper.RunQuestionWithTitleDialog("Перестроить маршрут?", "Этот маршрутный лист уже был когда-то напечатан. При новом построении маршрута порядок адресов может быть другой. При продолжении обязательно перепечатайте этот МЛ.\nПерестроить маршрут?")) {
						RouteOptimizer optimizer = new RouteOptimizer(ServicesConfig.InteractiveService);
						var newRoute = optimizer.RebuidOneRoute(Entity);
						if(newRoute != null) {
							createroutelistitemsview1.DisableColumnsUpdate = true;
							newRoute.UpdateAddressOrderInRealRoute(Entity);
							//Рассчитываем расстояние
							using(var calc = new RouteGeometryCalculator(DistanceProvider.Osrm)) {
								Entity.RecalculatePlanedDistance(calc);
							}
							createroutelistitemsview1.DisableColumnsUpdate = false;
							var noPlan = Entity.Addresses.Count(x => !x.PlanTimeStart.HasValue);
							if(noPlan > 0)
								MessageDialogHelper.RunWarningDialog($"Для маршрута незапланировано {noPlan} адресов.");
						} else {
							MessageDialogHelper.RunWarningDialog($"Маршрут не был перестроен.");
						}
					}

					Save();

					if(Entity.Car.TypeOfUse == CarTypeOfUse.CompanyTruck) {
						if(MessageDialogHelper.RunQuestionDialog("Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.OnClosing.GetEnumTitle())) {
							Entity.CompleteRoute(wageParameterService, callTaskWorker);
						}
					} else {
						//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
						var forShipment = warehouseRepository.WarehouseForShipment(UoW, Entity.Id);
						if(!forShipment.Any()) {
							if(MessageDialogHelper.RunQuestionDialog("Для маршрутного листа, нет необходимости грузится на складе. Перевести машрутный лист сразу в статус '{0}'?", RouteListStatus.EnRoute.GetEnumTitle())) {
								valid = new QSValidator<RouteList>(
									Entity,
									new Dictionary<object, object> {
										{ "NewStatus", RouteListStatus.EnRoute },
										{ nameof(IRouteListItemRepository), new RouteListItemRepository()}
									});
								if(!valid.IsValid)
									return;
								Entity.ChangeStatus(valid.RunDlgIfNotValid((Window)this.Toplevel) ? RouteListStatus.New : RouteListStatus.EnRoute, callTaskWorker);
							} else {
								Entity.ChangeStatus(RouteListStatus.New, callTaskWorker);
							}
						}
					}
					Save();
					UpdateButtonStatus();
					return;
				}
				if(Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed) {
					if(new RouteListRepository().GetCarLoadDocuments(UoW, Entity.Id).Any()) {
						MessageDialogHelper.RunErrorDialog("Для маршрутного листа были созданы документы погрузки. Сначала необходимо удалить их.");
					} else {
						Entity.ChangeStatus(RouteListStatus.New, callTaskWorker);
					}
					UpdateButtonStatus();
					return;
				}
			} finally {
				SetSensetivity(true);
			}
		}

		protected void OnReferenceCarChanged(object sender, EventArgs e)
		{
			createroutelistitemsview1.UpdateWeightInfo();
			createroutelistitemsview1.UpdateVolumeInfo();
		}

		protected void OnReferenceCarChangedByUser(object sender, EventArgs e)
		{
			while(Entity.ObservableGeographicGroups.Any())
				Entity.ObservableGeographicGroups.Remove(Entity.ObservableGeographicGroups.FirstOrDefault());

			if(Entity.Car != null)
				foreach(var group in Entity.Car.GeographicGroups)
					Entity.ObservableGeographicGroups.Add(group);
		}
	}
}