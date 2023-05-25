using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gamma.Widgets.Additions;
using Gdk;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class AtWorksDlg : TdiTabBase, ITdiDialog, ISingleUoWDialog
	{
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());
		
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IAuthorizationService _authorizationService = new AuthorizationServiceFactory().CreateNewAuthorizationService();
		private readonly IEmployeeWageParametersFactory _employeeWageParametersFactory = new EmployeeWageParametersFactory();
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory = new SubdivisionJournalFactory();
		private readonly IEmployeePostsJournalFactory _employeePostsJournalFactory = new EmployeePostsJournalFactory();
		private readonly ICashDistributionCommonOrganisationProvider _cashDistributionCommonOrganisationProvider =
			new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
		private readonly ISubdivisionParametersProvider _supSubdivisionParametersProvider =
			new SubdivisionParametersProvider(new ParametersProvider());
		private readonly IWageCalculationRepository _wageCalculationRepository  = new WageCalculationRepository();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly IPhonesViewModelFactory _phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly ICarRepository _carRepository = new CarRepository();
		private readonly IGeographicGroupRepository _geographicGroupRepository = new GeographicGroupRepository();
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository = new ScheduleRestrictionRepository();
		private readonly IWarehouseRepository _warehouseRepository = new WarehouseRepository();
        private readonly IRouteListRepository _routeListRepository = new RouteListRepository(new StockRepository(), _baseParametersProvider);
        private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory = new AttachmentsViewModelFactory();
        private readonly EmployeeFilterViewModel _forwarderFilter;
		private IList<RouteList> _routelists = new List<RouteList>();
		private readonly AtWorkFilterViewModel  _filterViewModel;

		public AtWorksDlg(
			IDefaultDeliveryDayScheduleSettings defaultDeliveryDayScheduleSettings,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			if(defaultDeliveryDayScheduleSettings == null)
			{
				throw new ArgumentNullException(nameof(defaultDeliveryDayScheduleSettings));
			}

			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_filterViewModel = new AtWorkFilterViewModel(UoW, _geographicGroupRepository, CheckAndSaveBeforeСontinue);

			Build();

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding
				.AddBinding(_filterViewModel, vm => vm.SelectedCarTypesOfUse, w => w.SelectedValuesList, 
					new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding
				.AddBinding(_filterViewModel, vm => vm.SelectedCarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			enumcheckDriverStatus.EnumType = typeof(AtWorkDriver.DriverStatus);
			enumcheckDriverStatus.Binding
				.AddBinding(_filterViewModel, vm => vm.SelectedDriverStatuses, w => w.SelectedValuesList, new EnumsListConverter<AtWorkDriver.DriverStatus>())
				.InitializeFromSource();

			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing().ToggledEvent(OnGeographicGroupSelected)
				.AddColumn("Район города").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();

			ytreeviewGeographicGroup.Binding
				.AddBinding(_filterViewModel, vm => vm.GeographicGroupNodes, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeviewGeographicGroup.HeadersVisible = false;

			yenumComboBoxSortBy.ItemsEnum = typeof(SortAtWorkDriversType);
			yenumComboBoxSortBy.Binding
				.AddBinding(_filterViewModel, vm => vm.SortType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ydateAtWorks.Binding.AddBinding(_filterViewModel, vm => vm.AtDate, w => w.Date).InitializeFromSource();

			_filterViewModel.OnFiltered += (s, a) =>
			{
				FillDialogAtDay();
				SetButtonClearDriverScreenSensitive();
				SetButtonCreateEmptyRouteListsSensitive();
			};

			var geographicGroups =
				_geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);
			var colorWhite = new Color(0xff, 0xff, 0xff);
			var colorLightRed = new Color(0xff, 0x66, 0x66);
			ytreeviewAtWorkDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>.Create()
				.AddColumn("Приоритет")
					.AddNumericRenderer(x => x.PriorityAtDay)
					.Editing(new Gtk.Adjustment(6, 1, 10, 1, 1, 1))
				.AddColumn("Статус")
					.AddTextRenderer(x => x.Status.GetEnumTitle())
				.AddColumn("Причина")
					.AddTextRenderer(x => x.Reason)
						.AddSetter((cell, driver) => cell.Editable = driver.Status == AtWorkDriver.DriverStatus.NotWorking)
				.AddColumn("Водитель")
					.AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Скор.")
					.AddTextRenderer(x => x.Employee.DriverSpeed.ToString("P0"))
				.AddColumn("График работы")
					.AddComboRenderer(x => x.DaySchedule)
					.SetDisplayFunc(x => x.Name)
					.FillItems(GetDeliveryDaySchedules())
					.Editing()
				.AddColumn("Оконч. работы")
					.AddTextRenderer(x => x.EndOfDayText).Editable()
				.AddColumn("Экспедитор")
					.AddComboRenderer(x => x.WithForwarder)
					.SetDisplayFunc(x => x.Employee.ShortName).Editing().Tag(Columns.Forwarder)
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x => x.Car != null && x.Car.GetActiveCarVersionOnDate(x.Date).CarOwnType == CarOwnType.Company ? vodovozCarIcon : null)
					.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
				.AddColumn("База")
					.AddComboRenderer(x => x.GeographicGroup)
					.SetDisplayFunc(x => x.Name)
					.FillItems(geographicGroups)
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.GeographicGroup == null
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Грузоп.")
					.AddTextRenderer(x => x.Car != null ? x.Car.CarModel.MaxWeight.ToString("D") : null)
				.AddColumn("Районы доставки")
					.AddTextRenderer(x => string.Join(", ", x.DistrictsPriorities.Select(d => d.District.DistrictName)))
				.AddColumn("")
				.AddColumn("Комментарий")
					.AddTextRenderer(x => x.Comment)
						.Editable(true)
				.AddColumn("Принадлежность\nавто")
					.AddTextRenderer(x => x.CarOwnTypeDisplayName)
				.AddColumn("Тип\nавто")
					.AddTextRenderer(x => x.CarTypeOfUseDisplayName)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.Status == AtWorkDriver.DriverStatus.NotWorking? "gray": "black")
				.Finish();

			ytreeviewAtWorkDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewAtWorkDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;

			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>.Create()
				.AddColumn("Экспедитор").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Едет с водителем").AddTextRenderer(x => RenderForwaderWithDriver(x))
				.Finish();
			ytreeviewOnDayForwarders.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewOnDayForwarders.Selection.Changed += YtreeviewForwarders_Selection_Changed;
			
			int currentUserId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			canReturnDriver = ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_return_driver_to_work", currentUserId);

			this.defaultDeliveryDaySchedule =
				UoW.GetById<DeliveryDaySchedule>(defaultDeliveryDayScheduleSettings.GetDefaultDeliveryDayScheduleId());

			_forwarderFilter = new EmployeeFilterViewModel();
			_forwarderFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.CanChangeStatus = true,
				x => x.Status = EmployeeStatus.IsWorking);

			ybuttonCreateRouteLists.Clicked += YbuttonCreateRouteListsClicked;

			buttonDriverSelectAuto.Visible = false;

			hideForwaders.Label = "Экспедиторы на работе";
			hideForwaders.Toggled += OnHideForwadersToggled;

			_filterViewModel.Update();
		}

		private void OnGeographicGroupSelected(object o, ToggledArgs args)
		{
			Application.Invoke((s, e) =>
			{
				var selectedNode = ytreeviewGeographicGroup.GetSelectedObject<GeographicGroupNode>();

				if(selectedNode == null)
				{
					return;
				}

				_filterViewModel.UpdateOrRollBackGeographicGroup(selectedNode);
			});
		}

		private bool CheckAndSaveBeforeСontinue(string question)
		{
			if(!_hasNewDrivers && !HasChanges)
			{
				return true;
			}

			if(ServicesConfig.InteractiveService.Question(question)
				&& Save())
			{
				return true;
			}

			return false;
		}

		private List<DeliveryDaySchedule> GetDeliveryDaySchedules()
		{
			var deliveryDaySchedules = UoW.GetAll<DeliveryDaySchedule>().ToList();
			return deliveryDaySchedules;
		}


		private void YbuttonCreateRouteListsClicked(object sender, EventArgs e)
		{
			if(!CheckAndSaveBeforeСontinue("Перед созданием МЛ необходимо сохранить изменения.\nВы хотите сохранить изменения и продолжить?"))
			{
				return;
			}

			var workedDrivers = DriversAtDay.Where(d => d.Status == AtWorkDriver.DriverStatus.IsWorking);
			var routeListGenerator = new EmptyRouteListGenerator(_routeListRepository, workedDrivers);
			var valid = ServicesConfig.ValidationService.Validate(routeListGenerator, new ValidationContext(routeListGenerator));
			if(!valid)
			{
				return;
			}
			_routelists = routeListGenerator.Generate();

			if(ServicesConfig.InteractiveService.Question($"Будут созданы {_routelists.Count} маршрутных листов.\nПродолжить?"))
			{
				SaveAndClose();
			}
			else
			{
				_routelists.Clear();
			}
		}

		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly Gdk.Pixbuf vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		private IList<AtWorkDriver> driversAtDay;
		private IList<AtWorkForwarder> forwardersAtDay;
		private HashSet<AtWorkDriver> driversWithCommentChanged = new HashSet<AtWorkDriver>();
		private GenericObservableList<AtWorkDriver> observableDriversAtDay;
		private GenericObservableList<AtWorkForwarder> observableForwardersAtDay;
		private bool _hasNewDrivers;
		private readonly bool canReturnDriver;
		private readonly DeliveryDaySchedule defaultDeliveryDaySchedule;

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		public bool HasChanges => UoW.HasChanges;
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		#region Properties

		private IList<AtWorkDriver> DriversAtDay {
			set {
				driversAtDay = value;
				observableDriversAtDay = new GenericObservableList<AtWorkDriver>(driversAtDay);
				ytreeviewAtWorkDrivers.SetItemsSource(observableDriversAtDay);
				observableDriversAtDay.PropertyOfElementChanged += (sender, args) => driversWithCommentChanged.Add(sender as AtWorkDriver);
			}
			get => driversAtDay;
		}

		private IList<AtWorkForwarder> ForwardersAtDay {
			set {
				forwardersAtDay = value;
				if(observableForwardersAtDay != null)
					observableForwardersAtDay.ListChanged -= ObservableForwardersAtDay_ListChanged;
				observableForwardersAtDay = new GenericObservableList<AtWorkForwarder>(forwardersAtDay);
				observableForwardersAtDay.ListChanged += ObservableForwardersAtDay_ListChanged;
				ytreeviewOnDayForwarders.SetItemsSource(observableForwardersAtDay);
				ObservableForwardersAtDay_ListChanged(null);
			}
			get => forwardersAtDay;
		}

		public override string TabName {
			get => $"Работают {_filterViewModel.AtDate:d}";
			protected set => throw new InvalidOperationException("Установка протеворечит логике работы.");
		}

		#endregion

		#region Events

		#region Buttons
		protected void OnButtonSaveChangesClicked(object sender, EventArgs e)
		{
			this.Save();
		}
		protected void OnButtonCancelChangesClicked(object sender, EventArgs e)
		{
			this.OnCloseTab(false, CloseSource.Cancel);
		}
		
		protected void OnButtonAddWorkingDriversClicked(object sender, EventArgs e)
		{
			var workDriversAtDay = _employeeRepository.GetWorkingDriversAtDay(UoW, _filterViewModel.AtDate);
			var onlyNewDrivers = new List<AtWorkDriver>();

			if(workDriversAtDay.Count > 0) 
			{
				foreach(var driver in workDriversAtDay) 
				{
					if(driversAtDay.Any(x => x.Employee.Id == driver.Id)) {
						logger.Warn($"Водитель {driver.ShortName} уже добавлен. Пропускаем...");
						continue;
					}

					var car = _carRepository.GetCarByDriver(UoW, driver);
					var daySchedule = GetDriverWorkDaySchedule(driver);

					var atwork = new AtWorkDriver(driver, _filterViewModel.AtDate, car, daySchedule);
					GetDefaultForwarder(driver, atwork);

					onlyNewDrivers.Add(atwork);
				}
			}

			_hasNewDrivers = onlyNewDrivers.Any();

			DriversAtDay = DriversAtDay
				.Union(onlyNewDrivers)
				.OrderBy(x => x.Employee.ShortName)
				.ToList();

			SetButtonCreateEmptyRouteListsSensitive();
			SetButtonClearDriverScreenSensitive();
		}

		protected void OnButtonAddDriverClicked(object sender, EventArgs e)
		{
			var selectDrivers = _employeeJournalFactory.CreateWorkingDriverEmployeeJournal();
			selectDrivers.SelectionMode = JournalSelectionMode.Multiple;
			selectDrivers.TabName = "Водители";
			
			selectDrivers.OnEntitySelectedResult += SelectDrivers_OnEntitySelectedResult;
			TabParent.AddSlaveTab(this, selectDrivers);
		}

		protected void OnButtonRemoveDriverClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			
			foreach(var driver in toDel)
			{
				if (driver.Id > 0)
				{
					ChangeButtonAddRemove(driver.Status == AtWorkDriver.DriverStatus.IsWorking);
					if (driver.Status == AtWorkDriver.DriverStatus.NotWorking)
					{
						if (canReturnDriver)
						{
							driver.Status = AtWorkDriver.DriverStatus.IsWorking;
						}
					}
					else
					{
						driver.Status = AtWorkDriver.DriverStatus.NotWorking;
						driver.AuthorRemovedDriver = _employeeRepository.GetEmployeeForCurrentUser(UoW);
						driver.RemovedDate = DateTime.Now;
					}
				}
				observableDriversAtDay.OnPropertyChanged(nameof(driver.Status));
			}
		}

		protected void OnButtonClearDriverScreenClicked(object sender, EventArgs e)
		{
			if (MessageDialogHelper.RunQuestionWithTitleDialog("ВНИМАНИЕ!!!",
				$"Список работающих и снятых водителей на дату: { _filterViewModel.AtDate.ToShortDateString()} будет очищен\n\n" +
				"Вы действительно хотите продолжить?"))
			{
				DriversAtDay.ToList().ForEach(x => UoW.Delete(x));
				observableDriversAtDay.Clear();
				SetButtonClearDriverScreenSensitive();
				SetButtonCreateEmptyRouteListsSensitive();
			}
		}
		
		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			throw new NotSupportedException("Отключено до востребования изменения логики работы логистики");

			var driver = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>().FirstOrDefault();

			if(driver == null)
			{
				MessageDialogHelper.RunWarningDialog("Не выбран водитель!");
				return;
			}
			
			var filter = new CarJournalFilterViewModel(new CarModelJournalFactory());
			filter.SetAndRefilterAtOnce(
				x => x.Archive = false,
				x => x.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company }
			);
			var journal = new CarJournalViewModel(
				filter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				MainClass.AppDIContainer.BeginLifetimeScope());
			journal.SelectionMode = JournalSelectionMode.Single;
			journal.OnEntitySelectedResult += (o, args) =>
			{
				var car = UoW.GetById<Car>(args.SelectedNodes.First().Id);
				driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x => x.Car = null);
				driver.Car = car;
			};
			TabParent.AddSlaveTab(this, journal);
		}
		
		protected void OnButtonAppointForwardersClicked(object sender, EventArgs e)
		{
			var toAdd = new List<AtWorkForwarder>();
			foreach(var forwarder in ForwardersAtDay.Where(f => DriversAtDay.All(d => d.WithForwarder != f))) {
				var defaulDriver = DriversAtDay.FirstOrDefault(d => d.WithForwarder == null && d.Employee.DefaultForwarder?.Id == forwarder.Employee.Id);
				if(defaulDriver != null)
					defaulDriver.WithForwarder = forwarder;
				else
					toAdd.Add(forwarder);
			}

			if(toAdd.Count == 0)
				return;

			var orders = _scheduleRestrictionRepository.OrdersCountByDistrict(UoW, _filterViewModel.AtDate, 12);
			var districtsBottles = orders.GroupBy(x => x.DistrictId).ToDictionary(x => x.Key, x => x.Sum(o => o.WaterCount));

			foreach(var forwarder in toAdd) {
				var driversToAdd = DriversAtDay.Where(x =>
					x.WithForwarder == null
					&& x.Car != null
					&& !(x.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
						&& x.Car.GetActiveCarVersionOnDate(x.Date).CarOwnType == CarOwnType.Company)
				).ToList();

				if(driversToAdd.Count == 0) {
					logger.Warn("Не осталось водителей для добавленя экспедиторов.");
					break;
				}

				int ManOnDistrict(int districtId) => driversAtDay
					.Where(dr =>
						dr.Car != null
						&& dr.DistrictsPriorities.Any(dd2 => dd2.District.Id == districtId)
						&& !(dr.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
							&& dr.Car.GetActiveCarVersionOnDate(dr.Date).CarOwnType == CarOwnType.Company)
					)
					.Sum(dr => dr.WithForwarder == null ? 1 : 2);

				var driversToAddWithWithActivePrioritySets =
					driversToAdd.Where(x => x.Employee.DriverDistrictPrioritySets.Any(p => p.IsActive));

				if(!districtsBottles.Any())
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Нет заказов на день для определения приоритета водителя.");
					return;
				}
				var driver = driversToAddWithWithActivePrioritySets
					.OrderByDescending(x => districtsBottles
						.Where(db => x.Employee.DriverDistrictPrioritySets
							.First(s => s.IsActive).DriverDistrictPriorities
							.Any(dd => dd.District.Id == db.Key))
						.Max(db => (double)db.Value / ManOnDistrict(db.Key)))
					.FirstOrDefault();

				if(driver != null) {
					driver.WithForwarder = forwarder;
				}
			}

			MessageDialogHelper.RunInfoDialog("Готово.");
		}

		protected void OnButtonOpenCarClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>().First();

			var uowFactory = UnitOfWorkFactory.GetDefaultFactory;
			var commonServices = ServicesConfig.CommonServices;
			var warehouseJournalFactory = new WarehouseJournalFactory();
			var employeeService = new EmployeeService();
			var geoGroupVersionsModel = new GeoGroupVersionsModel(commonServices.UserService, employeeService);
			var geoGroupJournalFactory = new GeoGroupJournalFactory(uowFactory, commonServices, _subdivisionJournalFactory, warehouseJournalFactory, geoGroupVersionsModel);

			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Car>(selected.Car.Id),
				() => new CarViewModel(
					EntityUoWBuilder.ForOpen(selected.Car.Id),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					_employeeJournalFactory,
					_attachmentsViewModelFactory,
					new CarModelJournalFactory(),
					new CarVersionsViewModelFactory(ServicesConfig.CommonServices),
					new OdometerReadingsViewModelFactory(ServicesConfig.CommonServices),
					new RouteListsWageController(new WageParameterService(new WageCalculationRepository(),
						new BaseParametersProvider(new ParametersProvider()))),
					geoGroupJournalFactory,
					MainClass.MainWin.NavigationManager
				)
			);
		}
		
		protected void OnButtonEditDistrictsClicked(object sender, EventArgs e)
		{
			districtpriorityview1.Visible = !districtpriorityview1.Visible;
		}

		protected void OnButtonOpenDriverClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			
			foreach(var one in selected) 
			{
				MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<EmployeeViewModel, IEntityUoWBuilder>(
					this, EntityUoWBuilder.ForOpen(one.Employee.Id));
			}
		}
		
		protected void OnButtonAddForwarderClicked(object sender, EventArgs e)
		{
			var forwardersJournal = _employeeJournalFactory.CreateEmployeesJournal(_forwarderFilter);
			forwardersJournal.SelectionMode = JournalSelectionMode.Multiple;
			forwardersJournal.OnEntitySelectedResult += OnForwardersSelected;
			TabParent.AddSlaveTab(this, forwardersJournal);
		}
		
		protected void OnButtonRemoveForwarderClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewOnDayForwarders.GetSelectedObjects<AtWorkForwarder>();
			foreach(var forwarder in toDel) {
				if(forwarder.Id > 0)
					UoW.Delete(forwarder);
				observableForwardersAtDay.Remove(forwarder);
			}
		}
		#endregion

		#region YTreeView

		void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveDriver.Sensitive = buttonDriverSelectAuto.Sensitive
				= buttonOpenDriver.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() > 0;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() != 1 && districtpriorityview1.Visible)
				districtpriorityview1.Visible = false;

			buttonOpenCar.Sensitive = false;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1) {
				var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
				districtpriorityview1.ListParent = selected[0];
				districtpriorityview1.Districts = selected[0].ObservableDistrictsPriorities;
				buttonOpenCar.Sensitive = selected[0].Car != null;
				ChangeButtonAddRemove(selected[0].Status == AtWorkDriver.DriverStatus.NotWorking);
			}
			districtpriorityview1.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1;
		}
		
		void YtreeviewForwarders_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveForwarder.Sensitive = ytreeviewOnDayForwarders.Selection.CountSelectedRows() > 0;
		}

		void ObservableForwardersAtDay_ListChanged(object aList)
		{
			var renderer = ytreeviewAtWorkDrivers.ColumnsConfig.GetRendererMappingByTagGeneric<ComboRendererMapping<AtWorkDriver, AtWorkForwarder>>(Columns.Forwarder).First();
			renderer.FillItems(ForwardersAtDay, "без экспедитора");
		}
		#endregion
		
		protected void OnYdateAtWorksDateChanged(object sender, EventArgs e)
		{
			OnTabNameChanged();
		}

		void SelectDrivers_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var addDrivers = e.SelectedNodes;
			logger.Info("Получаем авто для водителей...");
			var onlyNew = addDrivers.Where(x => driversAtDay.All(y => y.Employee.Id != x.Id)).ToList();
			_hasNewDrivers = onlyNew.Any();
			var allCars = _carRepository.GetCarsByDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());

			foreach(var driver in addDrivers) {
				var drv = UoW.GetById<Employee>(driver.Id);

				if(driversAtDay.Any(x => x.Employee.Id == driver.Id)) {
					logger.Warn($"Водитель {drv.ShortName} уже добавлен. Пропускаем...");
					continue;
				}

				var daySchedule = GetDriverWorkDaySchedule(drv);
				var atwork = new AtWorkDriver(drv, _filterViewModel.AtDate, allCars.FirstOrDefault(x => x.Driver.Id == driver.Id), daySchedule);

				GetDefaultForwarder(drv, atwork);

				driversAtDay.Add(atwork);
			}
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
			logger.Info("Ок");
			SetButtonClearDriverScreenSensitive();
			SetButtonCreateEmptyRouteListsSensitive();
		}

		protected void OnHideForwadersToggled(object o, Gtk.ToggledArgs args)
		{
			vboxForwarders.Visible = hideForwaders.ArrowDirection == Gtk.ArrowType.Down;
		}

		private void OnForwardersSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var loaded = UoW.GetById<Employee>(e.SelectedNodes.Select(x => x.Id));
			foreach(var forwarder in loaded)
			{
				if(forwardersAtDay.Any(x => x.Employee.Id == forwarder.Id))
				{
					logger.Warn($"Экспедитор {forwarder.ShortName} пропущен так как уже присутствует в списке.");
					continue;
				}
				forwardersAtDay.Add(new AtWorkForwarder(forwarder, _filterViewModel.AtDate));
			}
			ForwardersAtDay = forwardersAtDay.OrderBy(x => x.Employee.ShortName).ToList();
		}

		#endregion

		#region Fuctions
		private void SetButtonClearDriverScreenSensitive()
		{
			if (_filterViewModel.AtDate < DateTime.Now.Date || !driversAtDay.Any())
			{
				buttonClearDriverScreen.Sensitive = false;
			}
			else
			{
				buttonClearDriverScreen.Sensitive = true;
			}
		}

		private void SetButtonCreateEmptyRouteListsSensitive()
		{
			ybuttonCreateRouteLists.Sensitive = driversAtDay.Any();
		}

		private void ChangeButtonAddRemove(bool needRemove)
		{
			if (!canReturnDriver)
			{
				return;
			}
			
			if (needRemove)
			{
				buttonRemoveDriver.Label = "Вернуть водителя";
				buttonRemoveDriver.Image = new Gtk.Image(){Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu)};
			}
			else
			{
				buttonRemoveDriver.Label = "Снять водителя";
				buttonRemoveDriver.Image = new Gtk.Image(){Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu)};
			}
		}

		public void SaveAndClose() 
		{
			if(Save())
			{
				OnCloseTab(false, CloseSource.Save);
			}
		}

		private DeliveryDaySchedule GetDriverWorkDaySchedule(Employee driver)
		{
			var driverWorkSchedule = driver
				.ObservableDriverWorkScheduleSets.SingleOrDefault(x => x.IsActive)
				?.ObservableDriverWorkSchedules.SingleOrDefault(x => (int)x.WeekDay == (int)_filterViewModel.AtDate.DayOfWeek);

			return driverWorkSchedule == null 
				? defaultDeliveryDaySchedule
				: driverWorkSchedule.DaySchedule;
		}

		private void GetDefaultForwarder(Employee driver, AtWorkDriver atwork)
		{
			if(driver.DefaultForwarder != null) {
				var forwarder = ForwardersAtDay.FirstOrDefault(x => x.Employee.Id == driver.DefaultForwarder.Id);

				if(forwarder == null) {
					if(MessageDialogHelper.RunQuestionDialog($"Водитель {driver.ShortName} обычно ездит с экспедитором {driver.DefaultForwarder.ShortName}. Он отсутствует в списке экспедиторов. Добавить его в список?")) {
						forwarder = new AtWorkForwarder(driver.DefaultForwarder, _filterViewModel.AtDate);
						observableForwardersAtDay.Add(forwarder);
					}
				}

				if(forwarder != null && DriversAtDay.All(x => x.WithForwarder != forwarder)) {
					atwork.WithForwarder = forwarder;
				}
			}
		}
		
		public bool Save()
		{
			// В случае, если вкладка сохраняется, а в списке есть Снятые водители, сделать проверку, что у каждого из них заполнена причина.
			var NotWorkingDrivers = DriversAtDay.ToList()
				.Where(driver => driver.Status == AtWorkDriver.DriverStatus.NotWorking);
			
			if (NotWorkingDrivers.Count() != 0)
				foreach (var atWorkDriver in NotWorkingDrivers)
				{
					if (!String.IsNullOrEmpty(atWorkDriver.Reason)) continue;
					MessageDialogHelper.RunWarningDialog("Не у всех снятых водителей указаны причины!");
					return false;
				}

			foreach(var driver in DriversAtDay)
			{
				if(driver.GeographicGroup == null)
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не у всех водителей указана база!");
					return false;
				}

				if(driver.Car == null)
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не у всех водителей указан авто!");
					return false;
				}
			}

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			// Сохранение изменившихся за этот раз авторов и дат комментариев
			foreach (var atWorkDriver in driversWithCommentChanged)
			{
				atWorkDriver.CommentLastEditedAuthor = currentEmployee;
				atWorkDriver.CommentLastEditedDate = DateTime.Now;
			}
			driversWithCommentChanged.Clear();
			ForwardersAtDay.ToList().ForEach(x => UoW.Save(x));
			DriversAtDay.ToList().ForEach(x => UoW.Save(x));

			foreach(var routeList in _routelists)
			{
				routeList.Logistician = currentEmployee;
				UoW.Save(routeList);
			}

			UoW.Commit();

			_hasNewDrivers = false;

			return true;
		}
		
		string RenderForwaderWithDriver(AtWorkForwarder atWork)
		{
			return string.Join(", ", driversAtDay.Where(x => x.WithForwarder == atWork).Select(x => x.Employee.ShortName));
		}

		void FillDialogAtDay()
		{
			logger.Info("Загружаем экспедиторов на {0:d}...", _filterViewModel.AtDate);
			ForwardersAtDay = new EntityRepositories.Logistic.AtWorkRepository().GetForwardersAtDay(UoW, _filterViewModel.AtDate);

			logger.Info("Загружаем водителей на {0:d}...", _filterViewModel.AtDate);
			var selectedGeographicGroupIds = _filterViewModel
				.GeographicGroupNodes
				.Where(ggn => ggn.Selected)
				.Select(ggn => ggn.GeographicGroup.Id)
				.ToArray();
			DriversAtDay = new EntityRepositories.Logistic.AtWorkRepository().GetDriversAtDay(UoW, _filterViewModel.AtDate,
				driverStatuses: _filterViewModel.SelectedDriverStatuses, carTypesOfUse: _filterViewModel.SelectedCarTypesOfUse,
				carOwnTypes: _filterViewModel.SelectedCarOwnTypes, geoGroupIds: selectedGeographicGroupIds);

			switch(_filterViewModel.SortType)
			{
				case SortAtWorkDriversType.ByName:
					{
						DriversAtDay = DriversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
						break;
					}
				case SortAtWorkDriversType.ByCarOwn:
					{
						DriversAtDay = DriversAtDay.OrderBy(x => x.CarOwnTypeDisplayName).ToList();
						break;
					}
			}

			logger.Info("Ок");

			CheckAndCorrectDistrictPriorities();
			SetButtonClearDriverScreenSensitive();
			SetButtonCreateEmptyRouteListsSensitive();
		}

		//Если дата диалога >= даты активации набора районов и есть хотя бы один район у водителя, который не принадлежит активному набору районов
		private void CheckAndCorrectDistrictPriorities() {
			var activeDistrictsSet = UoW.Session.QueryOver<DistrictsSet>().Where(x => x.Status == DistrictsSetStatus.Active).SingleOrDefault();
			if(activeDistrictsSet == null) {
				throw new ArgumentNullException(nameof(activeDistrictsSet), @"Не найдена активная версия районов");
			}
			if(activeDistrictsSet.DateActivated == null) {
				throw new ArgumentNullException(nameof(activeDistrictsSet), @"У активной версии районов не проставлена дата активации");
			}
			if(_filterViewModel.AtDate.Date >= activeDistrictsSet.DateActivated.Value.Date) {
				var outDatedpriorities = DriversAtDay.SelectMany(x => x.DistrictsPriorities.Where(d => d.District.DistrictsSet.Id != activeDistrictsSet.Id)).ToList();
				if(!outDatedpriorities.Any()) 
					return;
				
				int deletedCount = 0;
				foreach (var priority in outDatedpriorities) {
					var newDistrict = activeDistrictsSet.ObservableDistricts.FirstOrDefault(x => x.CopyOf == priority.District);
					if(newDistrict == null) {
						priority.Driver.ObservableDistrictsPriorities.Remove(priority);
						UoW.Delete(priority);
						deletedCount++;
					}
					else {
						priority.District = newDistrict;
						UoW.Save(priority);
					}
				}
				MessageDialogHelper.RunInfoDialog($"Были найдены и исправлены устаревшие приоритеты районов.\nУдалено приоритетов, ссылающихся на несуществующий район: {deletedCount}");
				ytreeviewAtWorkDrivers.YTreeModel.EmitModelChanged();
			}
		}
		#endregion
		
		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
		
		
		enum Columns
		{
			Forwarder
		}
			   
	}
}
