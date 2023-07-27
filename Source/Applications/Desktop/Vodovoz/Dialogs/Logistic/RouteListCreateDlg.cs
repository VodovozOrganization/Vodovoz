using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using QS.Navigation;
using QS.ViewModels.Extension;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Additions.Printing;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Factories;
using Gamma.ColumnConfig;
using Vodovoz.Domain.Profitability;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(_parametersProvider);
		private static readonly IAdditionalLoadingModel _additionalLoadingModel = new AdditionalLoadingModel(new EmployeeRepository(),
			new FlyerRepository(), new DeliveryRulesParametersProvider(_parametersProvider), new StockRepository());
		private static readonly IRouteListRepository _routeListRepository =
			new RouteListRepository(new StockRepository(), _baseParametersProvider);
		private static readonly INomenclatureParametersProvider _nomenclatureParametersProvider =
			new NomenclatureParametersProvider(_parametersProvider);
		private static readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider = 
			new DeliveryRulesParametersProvider(_parametersProvider);

		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory =
			new EntityDocumentsPrinterFactory();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(_parametersProvider);
		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), _baseParametersProvider);

		private readonly RouteListProfitabilityController _routeListProfitabilityController = 
			new RouteListProfitabilityController(
				new RouteListProfitabilityFactory(),
				_nomenclatureParametersProvider,
				new ProfitabilityConstantsRepository(),
				new RouteListProfitabilityRepository(),
				_routeListRepository,
				new NomenclatureRepository(_nomenclatureParametersProvider));

		private AdditionalLoadingItemsView _additionalLoadingItemsView;

		private bool _canClose = true;
		private Employee _oldDriver;
		private DateTime _previousSelectedDate;
		private bool _isLogistican;
		private bool _canСreateRoutelistInPastPeriod;
		private GenericObservableList<RouteListProfitability> _routeListProfitabilities;
		private bool _driversDebtIsConfirmed = false;

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

			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public RouteListCreateDlg(RouteList sub) : this(sub.Id) { }

		public RouteListCreateDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);

			ConfigureDlg();
		}

		public bool CanEditFixedPrice { get; set; }
		public bool AskSaveOnClose => permissionResult.CanCreate && Entity.Id == 0 || permissionResult.CanUpdate;


		private void ConfigureDlg()
		{
			ynotebook1.ShowTabs = false;
			radioBtnInformation.Toggled += OnInformationToggled;
			radioBtnInformation.Active = true;
			
			var currentPermissionService = ServicesConfig.CommonServices.CurrentPermissionService;
			btnCancel.Clicked += OnCancelClicked;
			printTimeButton.Clicked += OnPrintTimeButtonClicked;
			ybuttonAddAdditionalLoad.Clicked += OnButtonAddAdditionalLoadClicked;
			ybuttonRemoveAdditionalLoad.Clicked += OnButtonRemoveAdditionalLoadClicked;

			datepickerDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			_previousSelectedDate = Entity.Date;
			datepickerDate.DateChangedByUser += OnDatepickerDateDateChangedByUser;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(Startup.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory());
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.ChangedByUser += (sender, e) =>
			{
				if(Entity.Car == null || Entity.Date == default)
				{
					evmeForwarder.IsEditable = true;
					ybuttonAddAdditionalLoad.Sensitive = false;
					return;
				}

				ybuttonAddAdditionalLoad.Sensitive = true;
				var isCompanyCar = Entity.GetCarVersion.IsCompanyCar;

				Entity.Driver = Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired
					? Entity.Car.Driver
					: null;
				evmeDriver.Sensitive = Entity.Driver == null || isCompanyCar;

				if(!isCompanyCar || Entity.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus && Entity.CanAddForwarder)
				{
					Entity.Forwarder = Entity.Forwarder;
					evmeForwarder.IsEditable = true;
				}
				else
				{
					Entity.Forwarder = null;
					evmeForwarder.IsEditable = false;
				}
			};

			CanEditFixedPrice = currentPermissionService.ValidatePresetPermission("can_change_route_list_fixed_price");

			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.CanChangeStatus = false);
			var driverFactory = new EmployeeJournalFactory(driverFilter);
			evmeDriver.Changed += (sender, args) => lblDriverComment.Text = Entity.Driver?.Comment;
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			hboxDriverComment.Binding
				.AddFuncBinding(Entity, e => e.Driver != null && !string.IsNullOrWhiteSpace(e.Driver.Comment), w => w.Visible)
				.InitializeFromSource();

			var forwarderFilter = new EmployeeFilterViewModel();
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.CanChangeStatus = false);
			var forwarderFactory = new EmployeeJournalFactory(forwarderFilter);
			evmeForwarder.SetEntityAutocompleteSelectorFactory(forwarderFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();
			evmeForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
				lblForwarderComment.Text = Entity.Forwarder?.Comment;
			};

			hboxForwarderComment.Binding
				.AddFuncBinding(Entity, e => e.Forwarder != null && !string.IsNullOrWhiteSpace(e.Forwarder.Comment), w => w.Visible)
				.InitializeFromSource();
			lblForwarderComment.Text = Entity.Forwarder?.Comment;
			
			var employeeFactory = new EmployeeJournalFactory();
			evmeLogistician.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeLogistician.Sensitive = false;
			evmeLogistician.Binding.AddBinding(Entity, e => e.Logistician, w => w.Subject).InitializeFromSource();

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			labelStatus.Binding.AddFuncBinding(Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();

			evmeDriver.Sensitive = false;
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

			_isLogistican = currentPermissionService.ValidatePresetPermission("logistican");
			createroutelistitemsview1.RouteListUoW = UoWGeneric;
			createroutelistitemsview1.SetPermissionParameters(permissionResult, _isLogistican);

			var additionalLoadingItemsViewModel =
				new AdditionalLoadingItemsViewModel(UoW, this, new NomenclatureJournalFactory(), ServicesConfig.InteractiveService);
			additionalLoadingItemsViewModel.BindWithSource(Entity, e => e.AdditionalLoadingDocument);
			additionalLoadingItemsViewModel.CanEdit = Entity.Status == RouteListStatus.New;
			_additionalLoadingItemsView = new AdditionalLoadingItemsView(additionalLoadingItemsViewModel);
			_additionalLoadingItemsView.WidthRequest = 300;
			_additionalLoadingItemsView.ShowAll();
			hboxAdditionalLoading.PackStart(_additionalLoadingItemsView, false, false, 0);

			buttonAccept.Visible = ybuttonAddAdditionalLoad.Visible = ybuttonRemoveAdditionalLoad.Visible =
				NotLoadedRouteListStatuses.Contains(Entity.Status);

			if(Entity.Status == RouteListStatus.InLoading || Entity.Status == RouteListStatus.Confirmed)
			{
				var icon = new Image
				{
					Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu)
				};
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			ggToStringWidget.UoW = UoW;
			ggToStringWidget.Label = "Район города:";
			ggToStringWidget.Binding.AddBinding(Entity, x => x.ObservableGeographicGroups, x => x.Items).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.SetVisibility(RouteListPrintableDocuments.TimeList, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.OrderOfAddresses, false);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintableDocuments)e.ItemEnum);

			//Телефон
			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = Startup.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			var hasAccessToDriverTerminal = _isLogistican || currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier);
			var baseDoc = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Entity.Driver);
			labelTerminalCondition.Visible = hasAccessToDriverTerminal &&
											 baseDoc is DriverAttachedTerminalGiveoutDocument &&
											 baseDoc.CreationDate.Date <= Entity?.Date;
			if(labelTerminalCondition.Visible)
			{
				labelTerminalCondition.LabelProp += $"{Entity.DriverTerminalCondition?.GetEnumTitle() ?? "неизвестно"}";
			}

			_canСreateRoutelistInPastPeriod = currentPermissionService.ValidatePresetPermission("can_create_routelist_in_past_period");

			fixPriceSpin.Binding
				.AddBinding(Entity, e => e.FixedShippingPrice, w => w.ValueAsDecimal)
				.AddBinding(Entity, e => e.HasFixedShippingPrice, w => w.Sensitive).InitializeFromSource();
			checkIsFixPrice.Binding.AddBinding(Entity, e => e.HasFixedShippingPrice, w => w.Active).InitializeFromSource();

			_oldDriver = Entity.Driver;
			UpdateDlg(_isLogistican);

			Entity.PropertyChanged += OnRouteListPropertyChanged;
			Entity.ObservableGeographicGroups.ListContentChanged += ObservableGeographicGroups_ListContentChanged;
			UpdateCashSubdivision();

			#region Рентабельность МЛ

			radioBtnProfitability.Sensitive = currentPermissionService.ValidatePresetPermission("can_read_route_list_profitability");
			radioBtnProfitability.Toggled += OnProfitabilityToggled;

			_logger.Debug("Пересчитываем рентабельность МЛ");
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			_logger.Debug("Закончили пересчет рентабельности МЛ");
			
			_routeListProfitabilities = new GenericObservableList<RouteListProfitability> { Entity.RouteListProfitability };
			ConfigureTreeRouteListProfitability();

			#endregion

			btnCopyEntityId.Sensitive = Entity.Id > 0;
			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = Entity.Id.ToString();
			}
		}

		private void OnInformationToggled(object sender, EventArgs e)
		{
			if(radioBtnInformation.Active)
			{
				ynotebook1.Page = 0;
			}
		}
		
		private void OnProfitabilityToggled(object sender, EventArgs e)
		{
			if(radioBtnProfitability.Active)
			{
				ynotebook1.Page = 1;
			}
		}

		private void ConfigureTreeRouteListProfitability()
		{
			treeRouteListProfitability.ColumnsConfig = FluentColumnsConfig<RouteListProfitability>.Create()
				.AddColumn("№ МЛ")
					.AddNumericRenderer(x => Entity.Id)
				.AddColumn("Фактический пробег,\nкм")
					.AddNumericRenderer(x => x.Mileage)
				.AddColumn("Амортизация,\nруб")
					.AddNumericRenderer(x => x.Amortisation)
					.Digits(2)
				.AddColumn("Ремонт,\nруб")
					.AddNumericRenderer(x => x.RepairCosts)
					.Digits(2)
				.AddColumn("Топливо,\nруб")
					.AddNumericRenderer(x => x.FuelCosts)
					.Digits(2)
				.AddColumn("Затраты ЗП\nвод + эксп, руб")
					.AddNumericRenderer(x => x.DriverAndForwarderWages)
					.Digits(2)
				.AddColumn("Оплата доставки\nклиентом: Доставка за\nчас, платная доставка, руб")
					.AddNumericRenderer(x => x.PaidDelivery)
					.Digits(2)
				.AddColumn("Затраты на МЛ,\nруб")
					.AddNumericRenderer(x => x.RouteListExpenses)
					.Digits(2)
				.AddColumn("Вывезено,\nкг")
					.AddNumericRenderer(x => x.TotalGoodsWeight)
					.Digits(2)
				.AddColumn("Затраты\nна кг")
					.AddNumericRenderer(x => x.RouteListExpensesPerKg)
					.Digits(2)
				.AddColumn("Сумма\nпродаж,\nруб")
					.AddNumericRenderer(x => x.SalesSum)
					.Digits(2)
				.AddColumn("Сумма затрат,\nруб")
					.AddNumericRenderer(x => x.ExpensesSum)
					.Digits(2)
				.AddColumn("Валовая\nмаржа,\nруб")
					.AddNumericRenderer(x => x.GrossMarginSum)
					.Digits(2)
				.AddColumn("Валовая маржа, %")
					.AddNumericRenderer(x => x.GrossMarginPercents)
					.Digits(2)
				.AddColumn("")
				.Finish();

			treeRouteListProfitability.ItemsDataSource = _routeListProfitabilities;
		}

		private void ObservableGeographicGroups_ListContentChanged(object sender, EventArgs e)
		{
			UpdateCashSubdivision();
		}

		private void UpdateCashSubdivision()
		{
			string subdivisionMessage = "Нет";
			if(Entity.ClosingSubdivision != null)
			{
				subdivisionMessage = Entity.ClosingSubdivision.Name;
			}
			label8.LabelProp = $"Сдается в кассу: {subdivisionMessage}";
		}

		private void OnRouteListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.AdditionalLoadingDocument))
			{
				UpdateAdditionalLoadingWidgets();
			}
		}

		private void UpdateAdditionalLoadingWidgets()
		{
			if(Entity.AdditionalLoadingDocument == null)
			{
				if(NotLoadedRouteListStatuses.Contains(Entity.Status))
				{
					ybuttonAddAdditionalLoad.Visible = true;
					ybuttonRemoveAdditionalLoad.Visible = false;
				}
				else
				{
					ybuttonAddAdditionalLoad.Visible = false;
					ybuttonRemoveAdditionalLoad.Visible = false;
				}

				_additionalLoadingItemsView.Visible = false;
			}
			else
			{
				if(NotLoadedRouteListStatuses.Contains(Entity.Status))
				{
					ybuttonAddAdditionalLoad.Visible = false;
					ybuttonRemoveAdditionalLoad.Visible = true;
				}
				else
				{
					ybuttonAddAdditionalLoad.Visible = false;
					ybuttonRemoveAdditionalLoad.Visible = false;
				}
				_additionalLoadingItemsView.Visible = true;
			}
		}

		private void OnButtonAddAdditionalLoadClicked(object sender, EventArgs args)
		{
			var document = _additionalLoadingModel.CreateAdditionLoadingDocument(UoW, Entity);
			if(document != null)
			{
				Entity.AdditionalLoadingDocument = document;
				createroutelistitemsview1.UpdateInfo();
			}
		}

		private void OnButtonRemoveAdditionalLoadClicked(object sender, EventArgs e)
		{
			UoW.Delete(Entity.AdditionalLoadingDocument);
			Entity.AdditionalLoadingDocument = null;
			createroutelistitemsview1.UpdateInfo();
		}

		private void OnDatepickerDateDateChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Date < DateTime.Today.AddDays(-1) && !_canСreateRoutelistInPastPeriod)
			{
				MessageDialogHelper.RunWarningDialog("Нельзя выставлять дату ранее вчерашнего дня!");
				Entity.Date = _previousSelectedDate;
			}
			else
			{
				_additionalLoadingModel.ReloadActiveFlyers(UoW, Entity, _previousSelectedDate);
				createroutelistitemsview1.UpdateInfo();
				_previousSelectedDate = Entity.Date;
			}
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false, CloseSource.Cancel);
		}
		
		private void UpdateDlg(bool logistician)
		{
			if(Entity.Status == RouteListStatus.New && logistician && (permissionResult.CanCreate && Entity.Id == 0 || permissionResult.CanUpdate))
			{
				UpdateElements(true);
			}
			else if(logistician && (permissionResult.CanUpdate))
			{
				UpdateElements(false);
			}
			else
			{
				var canOpenOrder = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Order)).CanRead;
				UpdateElements(false, canOpenOrder);
				buttonAccept.Sensitive = buttonSave.Sensitive = false;
			}
			UpdateAdditionalLoadingWidgets();
		}

		private void UpdateElements(bool isEditable, bool canOpenOrder = true)
		{
			speccomboShift.Sensitive = isEditable;
			ggToStringWidget.Sensitive = datepickerDate.Sensitive = entityviewmodelentryCar.Sensitive = evmeForwarder.Sensitive = isEditable;
			createroutelistitemsview1.IsEditable(isEditable, canOpenOrder);
			ybuttonAddAdditionalLoad.Sensitive = isEditable && Entity.Car != null;
			ybuttonRemoveAdditionalLoad.Sensitive = isEditable;
			fixPriceSpin.Sensitive = Entity.HasFixedShippingPrice && Entity.Status != RouteListStatus.Closed;
			checkIsFixPrice.Sensitive = CanEditFixedPrice && Entity.Status != RouteListStatus.Closed;
			_additionalLoadingItemsView.ViewModel.CanEdit = isEditable;
		}

		private void PrintSelectedDocument(RouteListPrintableDocuments choise)
		{
			TabParent.AddSlaveTab(this, CreateDocumentsPrinterDlg(choise));
		}

		private DocumentsPrinterViewModel CreateDocumentsPrinterDlg(RouteListPrintableDocuments choise)
		{
			var dlg = new DocumentsPrinterViewModel(
				UoW, _entityDocumentsPrinterFactory, Startup.MainWin.NavigationManager, Entity, choise, ServicesConfig.InteractiveService);
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
			_logger.Info("Вызван метод сохранения МЛ {EntityId}...", Entity.Id);

			if(!_driversDebtIsConfirmed && !Entity.IsDriversDebtInPermittedRangeVerification())
			{
				return false;
			}

			var valid = new QSValidator<RouteList>(Entity, new Dictionary<object, object>() { { nameof(IRouteListItemRepository), new RouteListItemRepository() } });
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
			{
				return false;
			}

			if(Entity.AdditionalLoadingDocument != null && !Entity.AdditionalLoadingDocument.Items.Any())
			{
				UoW.Delete(Entity.AdditionalLoadingDocument);
				Entity.AdditionalLoadingDocument = null;
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

			var commonFastDeliveryMaxDistance = (decimal)_deliveryRulesParametersProvider.GetMaxDistanceToLatestTrackPointKmFor(DateTime.Now);
			Entity.UpdateFastDeliveryMaxDistanceValue(commonFastDeliveryMaxDistance);

			_logger.Info("Сохраняем маршрутный лист {EntityId}...", Entity.Id);
			UoWGeneric.Save();
			_logger.Info("Ok");
			
			_logger.Debug("Пересчитываем рентабельность МЛ");
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			_logger.Debug("Закончили пересчет рентабельности МЛ");
			UoW.Save(Entity.RouteListProfitability);

			_logger.Info("Выполняем коммит изменений МЛ {EntityId}...", Entity.Id);
			UoW.Commit();
			_logger.Info("Коммит изменений {EntityId} выполнен", Entity.Id);

			return true;
		}

		private void UpdateButtonStatus()
		{
			buttonAccept.Visible = true;

			switch(Entity.Status)
			{
				case RouteListStatus.New:
					{
						UpdateElements(true);
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
						UpdateElements(false);
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
						UpdateElements(false);
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
			btnCancel.Sensitive = isSensetive;
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
				_driversDebtIsConfirmed = Entity.IsDriversDebtInPermittedRangeVerification();
				if(!_driversDebtIsConfirmed)
				{
					return;
				}

				SetSensetivity(false);
				var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					new OrderRepository(),
					_employeeRepository,
					_baseParametersProvider,
					ServicesConfig.CommonServices.UserService,
					ErrorReporter.Instance);

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

				if(Entity.HasReverseVolumeExcess())
				{
					warningMsg.Append($"\n\t- объём возвращаемого груза превышен на {Entity.ReverseVolumeExecess()} м<sup>3</sup>");
				}

				if(buttonAccept.Label == "Подтвердить" && (Entity.HasOverweight() || Entity.HasVolumeExecess() || Entity.HasReverseVolumeExcess()))
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
						RouteOptimizer optimizer = new RouteOptimizer(ServicesConfig.InteractiveService, new GeographicGroupRepository());
						var newRoute = optimizer.RebuidOneRoute(Entity);
						if(newRoute != null)
						{
							createroutelistitemsview1.DisableColumnsUpdate = true;
							newRoute.UpdateAddressOrderInRealRoute(Entity);
							//Рассчитываем расстояние
							using(var calc = new RouteGeometryCalculator())
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

					try
					{
						Save();
					}
					finally
					{
						_logger.Log(LogLevel.Info, "Создаём операции по свободным остаткам МЛ {EntityId}...", Entity.Id);

						var routeListKeepingDocumentController = new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureParametersProvider);

						foreach(var address in Entity.Addresses)
						{
							if(address.TransferedTo == null &&
							   (!address.WasTransfered || address.AddressTransferType != AddressTransferType.FromHandToHand))
							{
								routeListKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(
									UoW, address, DeliveryFreeBalanceType.Decrease, isFullRecreation: true, needRouteListUpdate: true);
							}
							else
							{
								routeListKeepingDocumentController.RemoveRouteListKeepingDocument(UoW, address, true);
							}
						}

						_logger.Log(LogLevel.Info, "Операции по свободным остаткакам МЛ {EntityId} созданы.", Entity.Id);
					}

					if(Entity.GetCarVersion.IsCompanyCar && Entity.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck && !Entity.NeedToLoad)
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
					UpdateAdditionalLoadingWidgets();

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
			catch(Exception ex)
			{
				_logger.Error("Произошла ошибка во время подтверждения МЛ {EntityId}: {Message}.", Entity.Id, ex.Message);

				throw ex;
			}
			finally
			{
				SetSensetivity(true);
			}
			UpdateDlg(_isLogistican);
		}

		protected static readonly RouteListStatus[] NotLoadedRouteListStatuses =
			{ RouteListStatus.New, RouteListStatus.Confirmed, RouteListStatus.InLoading };
	}
}
