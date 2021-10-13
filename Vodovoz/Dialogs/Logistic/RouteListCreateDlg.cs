﻿using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Additions.Printing;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalFilters;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(_parametersProvider);

		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory =
			new EntityDocumentsPrinterFactory();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly IRouteListRepository _routeListRepository = new RouteListRepository(new StockRepository(), _baseParametersProvider);
		private readonly ITrackRepository _trackRepository = new TrackRepository();

		private IWarehouseRepository _warehouseRepository = new WarehouseRepository();
		private ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(_parametersProvider);
		private WageParameterService _wageParameterService = new WageParameterService(new WageCalculationRepository(), _baseParametersProvider);

		private bool _isEditable;
		private bool _canClose = true;
		private Employee _oldDriver;

		protected bool IsEditable
		{
			get => _isEditable;
			set
			{
				_isEditable = value;
				speccomboShift.Sensitive = _isEditable;
				ggToStringWidget.Sensitive = datepickerDate.Sensitive = entityviewmodelentryCar.Sensitive = referenceForwarder.Sensitive = yspeccomboboxCashSubdivision.Sensitive = _isEditable;
				createroutelistitemsview1.IsEditable(_isEditable);
			}
		}

		public RouteListCreateDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList>();
			Entity.Logistician = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Logistician == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}

			if(ConfigSubdivisionCombo())
			{
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
			{
				ConfigureDlg();
			}
		}

		private bool ConfigSubdivisionCombo()
		{
			var subdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			if(!subdivisions.Any())
			{
				MessageDialogHelper.RunErrorDialog(
					"Неправильно сконфигурированы подразделения кассы, невозможно будет указать подразделение в которое будут сдаваться маршрутные листы");
				FailInitialize = true;
				return false;
			}
			yspeccomboboxCashSubdivision.ShowSpecialStateNot = true;
			yspeccomboboxCashSubdivision.ItemsList = subdivisions;
			yspeccomboboxCashSubdivision.SelectedItem = SpecialComboState.Not;
			yspeccomboboxCashSubdivision.ItemSelected += YspeccomboboxCashSubdivision_ItemSelected;

			if(Entity.ClosingSubdivision != null && subdivisions.Any(x => x.Id == Entity.ClosingSubdivision.Id))
			{
				yspeccomboboxCashSubdivision.SelectedItem = Entity.ClosingSubdivision;
			}

			return true;
		}

		private void ConfigureDlg()
		{
			datepickerDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.ChangedByUser += (sender, e) =>
			{
				if(Entity.Car != null)
				{
					Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired) ? Entity.Car.Driver : null;
					referenceDriver.Sensitive = Entity.Driver == null || Entity.ActiveCarVersion.IsCompanyCar;
					//Водители на Авто компании катаются без экспедитора
					Entity.Forwarder = Entity.ActiveCarVersion.IsCompanyCar ? null : Entity.Forwarder;
					referenceForwarder.IsEditable = !Entity.ActiveCarVersion.IsCompanyCar;
				}
			};

			var filterDriver = new EmployeeRepresentationFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.CanChangeStatus = false
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			var filter = new EmployeeRepresentationFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.CanChangeStatus = false
			);
			referenceForwarder.RepresentationModel = new ViewModel.EmployeesVM(filter);
			referenceForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();
			referenceForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			referenceLogistican.RepresentationModel = new EmployeesVM();
			referenceLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Subject).InitializeFromSource();

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();

			referenceDriver.Sensitive = false;
			enumPrint.Sensitive = Entity.Status != RouteListStatus.New;

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

			buttonAccept.Visible = Entity.Status == RouteListStatus.New || Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed;
			if(Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed)
			{
				var icon = new Image
				{
					Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
				};
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			var logistician = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			IsEditable = Entity.Status == RouteListStatus.New && logistician;

			ggToStringWidget.UoW = UoW;
			ggToStringWidget.Label = "Район города:";
			ggToStringWidget.Binding.AddBinding(Entity, x => x.ObservableGeographicGroups, x => x.Items).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.SetVisibility(RouteListPrintableDocuments.LoadSofiyskaya, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.TimeList, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.OrderOfAddresses, false);
			bool IsLoadDocumentPrintable = ServicesConfig.CommonServices.CurrentPermissionService
											.ValidatePresetPermission("can_print_car_load_document");
			enumPrint.SetVisibility(RouteListPrintableDocuments.LoadDocument, IsLoadDocumentPrintable
																			  && !(Entity.Status == RouteListStatus.Confirmed));
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments)e.ItemEnum);
			CheckCarLoadDocuments();

			//Телефон
			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = MainClass.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			var hasAccessToDriverTerminal = logistician ||
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
			var baseDoc = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Entity.Driver);
			labelTerminalCondition.Visible = hasAccessToDriverTerminal &&
											 baseDoc is DriverAttachedTerminalGiveoutDocument &&
											 baseDoc.CreationDate.Date <= Entity?.Date;
			if(labelTerminalCondition.Visible)
			{
				labelTerminalCondition.LabelProp += $"{Entity.DriverTerminalCondition?.GetEnumTitle() ?? "неизвестно"}";
			}

			_oldDriver = Entity.Driver;
		}

		private void YspeccomboboxCashSubdivision_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			Entity.ClosingSubdivision = yspeccomboboxCashSubdivision.SelectedItem as Subdivision;
		}

		private void CheckCarLoadDocuments()
		{
			if(Entity.Id > 0 && _routeListRepository.GetCarLoadDocuments(UoW, Entity.Id).Any())
			{
				IsEditable = false;
			}
		}

		private void PrintSelectedDocument(RouteListPrintableDocuments choise)
		{
			TabParent.AddSlaveTab(this, CreateDocumentsPrinterDlg(choise));
		}

		private DocumentsPrinterViewModel CreateDocumentsPrinterDlg(RouteListPrintableDocuments choise)
		{
			var dlg = new DocumentsPrinterViewModel(
				UoW, _entityDocumentsPrinterFactory, MainClass.MainWin.NavigationManager, Entity, choise, ServicesConfig.InteractiveService);
			dlg.DocumentsPrinted += Dlg_DocumentsPrinted;
			return dlg;
		}

		private void Dlg_DocumentsPrinted(object sender, EventArgs e)
		{
			if(e is EndPrintArgs printArgs)
			{
				if(printArgs.Args.Cast<IPrintableDocument>().Any(d => d.Name == RouteListPrintableDocuments.RouteList.GetEnumTitle()))
				{
					Entity.AddPrintHistory();
					Save();
				}
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<RouteList>(Entity, new Dictionary<object, object>() { { nameof(IRouteListItemRepository), new RouteListItemRepository() } });
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
			{
				return false;
			}

			Entity.CalculateWages(_wageParameterService);

			if(_oldDriver != Entity.Driver)
			{
				if(_oldDriver != null)
				{
					var selfDriverTerminalTransferDocument = _routeListRepository.GetSelfDriverTerminalTransferDocument(UoW, _oldDriver, Entity);

					if(selfDriverTerminalTransferDocument != null)
					{
						UoW.Delete(selfDriverTerminalTransferDocument);
					}
				}

				_oldDriver = Entity.Driver;
			}

			_logger.Info("Сохраняем маршрутный лист...");
			UoWGeneric.Save();
			_logger.Info("Ok");
			return true;
		}

		private void UpdateButtonStatus()
		{
			buttonAccept.Visible = true;

			switch(Entity.Status)
			{
				case RouteListStatus.New:
					{
						IsEditable = true;
						var icon = new Image
						{
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = false;
						buttonAccept.Label = "Подтвердить";
						break;
					}
				case RouteListStatus.Confirmed:
					{
						IsEditable = false;
						var icon = new Image
						{
							Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
						};
						buttonAccept.Image = icon;
						enumPrint.Sensitive = true;
						buttonAccept.Label = "Редактировать";
						break;
					}
				case RouteListStatus.InLoading:
					{
						IsEditable = false;
						var icon = new Image
						{
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

		public bool CanClose()
		{
			if(!_canClose)
			{
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения работы задачи и повторите");
			}

			return _canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			_canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
			buttonAccept.Sensitive = isSensetive;
		}

		private void OnPrintTimeButtonClicked(object sender, EventArgs e)
		{
			var history = _routeListRepository.GetPrintsHistory(UoW, Entity);
			if(history?.Any() ?? false)
			{
				var message = "<b>№\t| Дата и время печати\t| Тип документа</b>";
				for(var i = 0; i < history.Count; i++)
				{
					var item = history[i];
					message += $"\n{i + 1}\t| { item.PrintingTime.ToShortDateString() }" +
							   $" { item.PrintingTime.ToShortTimeString() }\t\t| { item.DocumentType.GetEnumShortTitle() }";
				}
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, message, $"История печати МЛ №: {Entity.Id}");
			}
			else
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, "МЛ не печатался ранее");
			}
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			try
			{
				SetSensetivity(false);
				var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					new OrderRepository(),
					_employeeRepository,
					_baseParametersProvider,
					ServicesConfig.CommonServices.UserService,
					SingletonErrorReporter.Instance);

				if(Entity.Car == null)
				{
					MessageDialogHelper.RunWarningDialog("Не заполнен автомобиль");
					return;
				}
				StringBuilder warningMsg = new StringBuilder($"Автомобиль '{ Entity.Car.Title }':");
				if(Entity.HasOverweight())
				{
					warningMsg.Append($"\n\t- перегружен на { Entity.Overweight() } кг");
				}

				if(Entity.HasVolumeExecess())
				{
					warningMsg.Append($"\n\t- объём груза превышен на { Entity.VolumeExecess() } м<sup>3</sup>");
				}

				if(buttonAccept.Label == "Подтвердить" && (Entity.HasOverweight() || Entity.HasVolumeExecess()))
				{
					if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_confirm_routelist_with_overweight"))
					{
						warningMsg.AppendLine("\nВы уверены что хотите подтвердить маршрутный лист?");
						if(!MessageDialogHelper.RunQuestionDialog(warningMsg.ToString()))
						{
							return;
						}
					}
					else
					{
						warningMsg.AppendLine("\nПодтвердить маршрутный лист нельзя.");
						MessageDialogHelper.RunWarningDialog(warningMsg.ToString());
						return;
					}
				}

				if(Entity.Status == RouteListStatus.New)
				{
					var valid = new QSValidator<RouteList>(Entity,
									new Dictionary<object, object> {
						{ "NewStatus", RouteListStatus.Confirmed },
						{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
						});
					if(valid.RunDlgIfNotValid((Window)this.Toplevel))
					{
						return;
					}

					Entity.ChangeStatusAndCreateTask(RouteListStatus.Confirmed, callTaskWorker);
					//Строим маршрут для МЛ.
					if((!Entity.PrintsHistory?.Any() ?? true) || MessageDialogHelper.RunQuestionWithTitleDialog("Перестроить маршрут?", "Этот маршрутный лист уже был когда-то напечатан. При новом построении маршрута порядок адресов может быть другой. При продолжении обязательно перепечатайте этот МЛ.\nПерестроить маршрут?"))
					{
						RouteOptimizer optimizer = new RouteOptimizer(ServicesConfig.InteractiveService);
						var newRoute = optimizer.RebuidOneRoute(Entity);
						if(newRoute != null)
						{
							createroutelistitemsview1.DisableColumnsUpdate = true;
							newRoute.UpdateAddressOrderInRealRoute(Entity);
							//Рассчитываем расстояние
							using(var calc = new RouteGeometryCalculator(DistanceProvider.Osrm))
							{
								Entity.RecalculatePlanedDistance(calc);
							}
							createroutelistitemsview1.DisableColumnsUpdate = false;
							var noPlan = Entity.Addresses.Count(x => !x.PlanTimeStart.HasValue);
							if(noPlan > 0)
							{
								MessageDialogHelper.RunWarningDialog($"Для маршрута незапланировано { noPlan } адресов.");
							}
						}
						else
						{
							MessageDialogHelper.RunWarningDialog($"Маршрут не был перестроен.");
						}
					}

					Save();

					if(Entity.ActiveCarVersion.IsCompanyCar && Entity.Car.CarModel.TypeOfUse == CarTypeOfUse.Truck && !Entity.NeedToLoad)
					{
						if(MessageDialogHelper.RunQuestionDialog(
							"Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{0}'?",
							RouteListStatus.OnClosing.GetEnumTitle()))
						{
							Entity.CompleteRouteAndCreateTask(_wageParameterService, callTaskWorker, _trackRepository);
						}
					}
					else
					{
						//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
						var needTerminal = Entity.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

						if(!Entity.NeedToLoad && !needTerminal)
						{
							if(MessageDialogHelper.RunQuestionDialog("Для маршрутного листа, нет необходимости грузится на складе. Перевести маршрутный лист сразу в статус '{0}'?", RouteListStatus.EnRoute.GetEnumTitle()))
							{
								valid = new QSValidator<RouteList>(
									Entity,
									new Dictionary<object, object>
									{
										{ "NewStatus", RouteListStatus.EnRoute },
										{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
									});
								if(!valid.IsValid)
								{
									return;
								}

								Entity.ChangeStatusAndCreateTask(valid.RunDlgIfNotValid((Window)this.Toplevel) ? RouteListStatus.New : RouteListStatus.EnRoute, callTaskWorker);
							}
							else
							{
								Entity.ChangeStatusAndCreateTask(RouteListStatus.New, callTaskWorker);
							}
						}
					}
					Save();
					UpdateButtonStatus();
					createroutelistitemsview1.SubscribeOnChanges();

					return;
				}
				if(Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed)
				{
					if(_routeListRepository.GetCarLoadDocuments(UoW, Entity.Id).Any())
					{
						MessageDialogHelper.RunErrorDialog("Для маршрутного листа были созданы документы погрузки. Сначала необходимо удалить их.");
					}
					else
					{
						Entity.ChangeStatusAndCreateTask(RouteListStatus.New, callTaskWorker);
					}
					UpdateButtonStatus();
					return;
				}
			}
			finally
			{
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
			{
				Entity.ObservableGeographicGroups.Remove(Entity.ObservableGeographicGroups.FirstOrDefault());
			}

			if(Entity.Car != null)
			{
				foreach(var group in Entity.Car.GeographicGroups)
				{
					Entity.ObservableGeographicGroups.Add(group);
				}
			}
		}
	}
}
