using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.Project.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Application.Services.Logistics;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Profitability;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private ILifetimeScope _lifetimeScope;
		private ILogger<RouteListCreateDlg> _logger;

		private BaseParametersProvider _baseParametersProvider;
		private IAdditionalLoadingModel _additionalLoadingModel;
		private IRouteListService _routeListService;
		private IRouteListRepository _routeListRepository;
		private IGenericRepository<RouteListSpecialConditionType> _routeListSpecialConditionTypeRepository;
		private INomenclatureParametersProvider _nomenclatureParametersProvider;
		private IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;

		private IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory;
		private IEmployeeRepository _employeeRepository;
		private IDeliveryShiftRepository _deliveryShiftRepository;
		private ITrackRepository _trackRepository;
		private IWageParameterService _wageParameterService;

		private IRouteListProfitabilityController _routeListProfitabilityController;

		private AdditionalLoadingItemsView _additionalLoadingItemsView;

		private bool _canClose = true;
		private Employee _oldDriver;
		private DateTime _previousSelectedDate;
		private bool _isLogistican;
		private bool _canСreateRoutelistInPastPeriod;
		private GenericObservableList<RouteListProfitability> _routeListProfitabilities;

		public RouteListCreateDlg()
		{
			ResolveDependencies();

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
			ResolveDependencies();

			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);

			ConfigureDlg();
		}

		public bool CanEditFixedPrice { get; set; }
		public bool AskSaveOnClose => permissionResult.CanCreate && Entity.Id == 0 || permissionResult.CanUpdate;

		private void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			_logger = _lifetimeScope.Resolve<ILogger<RouteListCreateDlg>>();
			_baseParametersProvider = _lifetimeScope.Resolve<BaseParametersProvider>();
			_additionalLoadingModel = _lifetimeScope.Resolve<IAdditionalLoadingModel>();
			_routeListRepository = _lifetimeScope.Resolve<IRouteListRepository>();
			_routeListSpecialConditionTypeRepository = _lifetimeScope.Resolve<IGenericRepository<RouteListSpecialConditionType>>();
			_routeListService = _lifetimeScope.Resolve<IRouteListService>();
			_nomenclatureParametersProvider = _lifetimeScope.Resolve<INomenclatureParametersProvider>();
			_deliveryRulesParametersProvider = _lifetimeScope.Resolve<IDeliveryRulesParametersProvider>();
			_entityDocumentsPrinterFactory = _lifetimeScope.Resolve<IEntityDocumentsPrinterFactory>();
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_deliveryShiftRepository = _lifetimeScope.Resolve<IDeliveryShiftRepository>();
			_trackRepository = _lifetimeScope.Resolve<ITrackRepository>();
			_wageParameterService = _lifetimeScope.Resolve<IWageParameterService>();
			_routeListProfitabilityController = _lifetimeScope.Resolve<IRouteListProfitabilityController>();
		}

		private void ConfigureDlg()
		{
			createroutelistitemsview1.NavigationManager = Startup.MainWin.NavigationManager;
			createroutelistitemsview1.Container = this;

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

			ylblSpecialConditionsConfirmed.Binding
				.AddBinding(Entity, e => e.SpecialConditionsAccepted, w => w.Visible)
				.InitializeFromSource();

			ylblSpecialConditionsConfirmedDateTime.Binding
				.AddBinding(Entity, e => e.SpecialConditionsAccepted, w => w.Visible)
				.AddFuncBinding(e => e.SpecialConditionsAcceptedAt.HasValue ? e.SpecialConditionsAcceptedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "", w => w.Text)
				.InitializeFromSource();

			var specialConditions = _routeListService.GetSpecialConditionsFor(UoW, Entity.Id);

			var specialConditionsTypesIds = specialConditions.Select(x => x.RouteListSpecialConditionTypeId);

			var specialConditionsTypes = specialConditionsTypesIds.Any() ? _routeListSpecialConditionTypeRepository.Get(UoW, x => specialConditionsTypesIds.Contains(x.Id)) : Enumerable.Empty<RouteListSpecialConditionType>();

			radioBtnSprcialConditions.Visible = specialConditions.Any();
			radioBtnSprcialConditions.Toggled += OnButtonSpecialConditionsToggled;

			ytreeviewSpecialConditions.CreateFluentColumnsConfig<RouteListSpecialCondition>()
				.AddColumn("Название")
				.AddTextRenderer(x => specialConditionsTypes
					.Where(sct => sct.Id == x.RouteListSpecialConditionTypeId)
					.Select(sct => sct.Name)
					.FirstOrDefault() ?? "")
				.AddColumn("Принято")
				.AddTextRenderer(x => x.Accepted ? "Да" : "Нет")
				.AddColumn("Создано")
				.AddTextRenderer(x => x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
				.AddColumn("")
				.Finish();

			ytreeviewSpecialConditions.ItemsDataSource = specialConditions;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(Startup.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory(_lifetimeScope));
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
			var driverFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, driverFilter);
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
			var forwarderFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, forwarderFilter);
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
			
			var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
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
				new AdditionalLoadingItemsViewModel(UoW, this, new NomenclatureJournalFactory(_lifetimeScope), ServicesConfig.InteractiveService);
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

			_logger.LogDebug("Пересчитываем рентабельность МЛ");
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			createroutelistitemsview1.UpdateProfitabilityInfo();
			_logger.LogDebug("Закончили пересчет рентабельности МЛ");
			
			_routeListProfitabilities = new GenericObservableList<RouteListProfitability> { Entity.RouteListProfitability };
			ConfigureTreeRouteListProfitability();

			#endregion

			btnCopyEntityId.Sensitive = Entity.Id > 0;
			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		private void OnButtonSpecialConditionsToggled(object sender, EventArgs e)
		{
			if(radioBtnSprcialConditions.Active)
			{
				ynotebook1.Page = 2;
			}
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
			_logger.LogInformation("Вызван метод сохранения МЛ {RouteListId}...", Entity.Id);

			if(!Entity.IsDriversDebtInPermittedRangeVerification())
			{
				return false;
			}

			var contextItems = new Dictionary<object, object>
			{
				{nameof(IRouteListItemRepository), new RouteListItemRepository()}
			};

			var context = new ValidationContext(Entity, null, contextItems);
			var validator = new ObjectValidator(new GtkValidationViewFactory());

			if(!validator.Validate(Entity, context))
			{
				return false;
			}

			if(Entity.AdditionalLoadingDocument != null && !Entity.AdditionalLoadingDocument.Items.Any())
			{
				UoW.Delete(Entity.AdditionalLoadingDocument);
				Entity.AdditionalLoadingDocument = null;
			}

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

			UoW.Session.Flush();

			_logger.LogDebug("Пересчитываем рентабельность МЛ");

			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			_logger.LogDebug("Закончили пересчет рентабельности МЛ");
			UoW.Save(Entity.RouteListProfitability);

			_logger.LogInformation("Сохраняем маршрутный лист {RouteListId}...", Entity.Id);
			UoWGeneric.Save();
			_logger.LogInformation("Ok");
			
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
			if(!Save())
			{
				return;
			}

			using(var transaction = UoW.Session.BeginTransaction())
			{
				try
				{
					SetSensetivity(false);

					var isAcceptMode = buttonAccept.Label == "Подтвердить";

					var result = _routeListService.TryAcceptOrEditRouteList(UoW, Entity, isAcceptMode, DisableItemsUpdate, ServicesConfig.CommonServices);

					result.Match(() =>
					{
						if(result.Value != RouteListAcceptStatus.Accepted)
						{
							return;
						}

						transaction.Commit();
						GlobalUowEventsTracker.OnPostCommit((IUnitOfWorkTracked)UoW);
						createroutelistitemsview1.SubscribeOnChanges();
						UpdateAdditionalLoadingWidgets();
					},
					(errors) =>
					{
						var errorsStrings = errors.Select(x => $"{x.Message} : {x.Code}");
						ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, string.Join("\n", errorsStrings));
					}
					);

					UpdateButtonStatus();
					SetSensetivity(true);
					UpdateDlg(_isLogistican);
				}
				catch(Exception ex)
				{
					if(!transaction.WasCommitted
					   && !transaction.WasRolledBack
					   && transaction.IsActive
					   && UoW.Session.Connection.State == ConnectionState.Open)
					{
						try
						{
							transaction.Rollback();
						}
						catch { }
					}

					transaction.Dispose();

					_logger.LogError(ex, "Произошла ошибка во время подтверждения МЛ {RouteListId}: {Message}.", Entity.Id, ex.Message);

					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						$"Возникла ошибка при подтверждении МЛ {Entity.Id}, МЛ был сохранён, но не подтверждён.\n" +
						$"Будет произведена попытка переоткрытия вкладки.\n" +
						$"Ошибка: {ex.Message}\n{ex.StackTrace}");

					OnCloseTab(false);

					TabParent.OpenTab(() => new RouteListCreateDlg(Entity.Id));
				}
			}
		}

		private void DisableItemsUpdate(bool isDisable) => createroutelistitemsview1.DisableColumnsUpdate = isDisable;

		protected static readonly RouteListStatus[] NotLoadedRouteListStatuses =
			{ RouteListStatus.New, RouteListStatus.Confirmed, RouteListStatus.InLoading };

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}
