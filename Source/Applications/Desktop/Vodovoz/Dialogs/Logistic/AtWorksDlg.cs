using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gamma.Widgets.Additions;
using Gdk;
using Gtk;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.Settings.Delivery;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class AtWorksDlg : TdiTabBase, ITdiDialog, ISingleUoWDialog
	{
		private readonly Gdk.Pixbuf _vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");
		private readonly Gtk.Adjustment _driversAtWorksPriorityAdjustment = new Gtk.Adjustment(6, 1, 10, 1, 1, 1);

		private readonly ILogger<AtWorksDlg> _logger;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICarRepository _carRepository;
		private readonly IGeographicGroupRepository _geographicGroupRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IAtWorkRepository _atWorkRepository;
		private readonly EmployeeFilterViewModel _forwarderFilter;
		private readonly AtWorkFilterViewModel _filterViewModel;

		private readonly Color _colorPrimaryBase = GdkColors.PrimaryBase;
		private readonly Color _colorPrimaryText = GdkColors.PrimaryText;
		private readonly Color _colorInsensitiveText = GdkColors.InsensitiveText;
		private readonly Color _colorLightRed = GdkColors.DangerBase;

		private IList<RouteList> _routelists = new List<RouteList>();
		private readonly HashSet<AtWorkDriver> _driversWithCommentChanged = new HashSet<AtWorkDriver>();
		private bool _hasNewDrivers;
		private readonly bool _canReturnDriver;
		private readonly DeliveryDaySchedule _defaultDeliveryDaySchedule;
		private readonly IList<GeoGroup> _cachedGeographicGroups;

		public AtWorksDlg(
			ILogger<AtWorksDlg> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IEmployeeJournalFactory employeeJournalFactory,
			IRouteListRepository routeListRepository,
			ICarRepository carRepository,
			IEmployeeRepository employeeRepository,
			IGeographicGroupRepository geographicGroupRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IUserService userService,
			IPermissionService permissionService,
			IInteractiveService interactiveService,
			IAtWorkRepository atWorkRepository)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("На работе");

			if(userService is null)
			{
				throw new ArgumentNullException(nameof(userService));
			}

			if(permissionService is null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_geographicGroupRepository = geographicGroupRepository ?? throw new ArgumentNullException(nameof(geographicGroupRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_atWorkRepository = atWorkRepository ?? throw new ArgumentNullException(nameof(atWorkRepository));

			_filterViewModel = new AtWorkFilterViewModel(UoW, _geographicGroupRepository, CheckAndSaveBeforeСontinue);

			int currentUserId = userService.CurrentUserId;
			_canReturnDriver = permissionService.ValidateUserPresetPermission("can_return_driver_to_work", currentUserId);

			_defaultDeliveryDaySchedule =
				UoW.GetById<DeliveryDaySchedule>(_deliveryScheduleSettings.DefaultDeliveryDayScheduleId);

			_cachedGeographicGroups = _geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);

			_forwarderFilter = new EmployeeFilterViewModel();

			DriversAtDay = new GenericObservableList<AtWorkDriver>();
			DriversAtDay.PropertyOfElementChanged += (sender, args) => _driversWithCommentChanged.Add(sender as AtWorkDriver);
			ForwardersAtDay = new GenericObservableList<AtWorkForwarder>();
			ForwardersAtDay.ListChanged += ObservableForwardersAtDay_ListChanged;

			Build();

			Initialize();

			ObservableForwardersAtDay_ListChanged(ForwardersAtDay);
		}

		#region Properties

		public IUnitOfWork UoW { get; }

		public bool HasChanges => UoW.HasChanges;

		public virtual bool HasCustomCancellationConfirmationDialog => false;

		public virtual Func<int> CustomCancellationConfirmationDialogFunc => null;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public override string TabName => $"Работают {_filterViewModel.AtDate:d}";

		public GenericObservableList<AtWorkDriver> DriversAtDay { get; }

		public GenericObservableList<AtWorkForwarder> ForwardersAtDay { get; }
		public INavigationManager NavigationManager { get; }

		#endregion Properties

		private void Initialize()
		{
			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
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

			ConfigureDriversAtWorksTreeView();
			ConfigureForwardersAtWorksTreeView();

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

		private void ConfigureForwardersAtWorksTreeView()
		{
			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>.Create()
				.AddColumn("Экспедитор").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Едет с водителем").AddTextRenderer(x => RenderForwaderWithDriver(x))
				.Finish();

			ytreeviewOnDayForwarders.ItemsDataSource = ForwardersAtDay;
			ytreeviewOnDayForwarders.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewOnDayForwarders.Selection.Changed += YtreeviewForwarders_Selection_Changed;
		}

		private void ConfigureDriversAtWorksTreeView()
		{
			ytreeviewAtWorkDrivers.CreateFluentColumnsConfig<AtWorkDriver>()
				.AddColumn("Приоритет")
					.AddNumericRenderer(x => x.PriorityAtDay)
					.Editing(_driversAtWorksPriorityAdjustment)
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
					.AddPixbufRenderer(x => x.Car != null && x.Car.GetActiveCarVersionOnDate(x.Date).CarOwnType == CarOwnType.Company ? _vodovozCarIcon : null)
					.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
				.AddColumn("База")
					.AddComboRenderer(x => x.GeographicGroup)
					.SetDisplayFunc(x => x.Name)
					.FillItems(_cachedGeographicGroups)
					.AddSetter(
						(c, n) =>
						{
							c.Editable = true;
							c.BackgroundGdk = n.GeographicGroup == null
								? _colorLightRed
								: _colorPrimaryBase;
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
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.Status == AtWorkDriver.DriverStatus.NotWorking
						? _colorInsensitiveText
						: _colorPrimaryText)
				.Finish();

			ytreeviewAtWorkDrivers.ItemsDataSource = DriversAtDay;
			ytreeviewAtWorkDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewAtWorkDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;
		}

		private void OnGeographicGroupSelected(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, e) =>
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

			if(_interactiveService.Question(question)
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

			if(_interactiveService.Question($"Будут созданы {_routelists.Count} маршрутных листов.\nПродолжить?"))
			{
				SaveAndClose();
			}
			else
			{
				_routelists.Clear();
			}
		}

		#region EventHandlers

		#region Buttons

		protected void OnButtonSaveChangesClicked(object sender, EventArgs e)
		{
			Save();
		}

		protected void OnButtonCancelChangesClicked(object sender, EventArgs e)
		{
			OnCloseTab(false, CloseSource.Cancel);
		}

		protected void OnButtonAddWorkingDriversClicked(object sender, EventArgs e)
		{
			if(!_interactiveService.Question("Будут добавлены все работающие водители, вы уверены?"))
			{
				return;
			}

			var workDriversAtDay = _employeeRepository.GetWorkingDriversAtDay(UoW, _filterViewModel.AtDate);
			var onlyNewDrivers = new List<AtWorkDriver>();

			if(workDriversAtDay.Count > 0)
			{
				foreach(var driver in workDriversAtDay)
				{
					if(DriversAtDay.Any(x => x.Employee.Id == driver.Id))
					{
						_logger.LogWarning("Водитель {DriverName} уже добавлен. Пропускаем...", driver.ShortName);
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

			RefillDriversAtDay(DriversAtDay.Union(onlyNewDrivers));

			SetButtonCreateEmptyRouteListsSensitive();
			SetButtonClearDriverScreenSensitive();
		}

		private void RefillDriversAtDay(IEnumerable<AtWorkDriver> atWorkDrivers)
		{
			IEnumerable<AtWorkDriver> sortedAtWorkDrivers = null;

			switch(_filterViewModel.SortType)
			{
				case SortAtWorkDriversType.ByName:
					sortedAtWorkDrivers = atWorkDrivers.OrderBy(x => x.Employee.ShortName);
					break;
				case SortAtWorkDriversType.ByCarOwn:
					sortedAtWorkDrivers = atWorkDrivers.OrderBy(x => x.CarOwnTypeDisplayName);
					break;
			}

			if(sortedAtWorkDrivers is null)
			{
				_logger.LogWarning("Не удалось отсортировать");
				return;
			}

			DriversAtDay.Clear();

			foreach(var driver in sortedAtWorkDrivers)
			{
				DriversAtDay.Add(driver);
			}
		}

		private void RefillForwardersAtDay(IEnumerable<AtWorkForwarder> atWorkForwarders)
		{
			var tempList = atWorkForwarders.OrderBy(x => x.Employee.ShortName).ToList();

			ForwardersAtDay.Clear();

			foreach(var forwarder in tempList)
			{
				ForwardersAtDay.Add(forwarder);
			}
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
				if(driver.Id <= 0)
				{
					DriversAtDay.Remove(driver);
					continue;
				}

				ChangeButtonAddRemove(driver.Status == AtWorkDriver.DriverStatus.IsWorking);

				if(driver.Status == AtWorkDriver.DriverStatus.NotWorking)
				{
					if(_canReturnDriver)
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

				DriversAtDay.OnPropertyChanged(nameof(driver.Status));
			}
		}

		protected void OnButtonClearDriverScreenClicked(object sender, EventArgs e)
		{
			if(_interactiveService.Question($"Список работающих и снятых водителей на дату: {_filterViewModel.AtDate.ToShortDateString()} будет очищен\n\n" +
				"Вы действительно хотите продолжить?",
				"ВНИМАНИЕ!!!"))
			{
				DriversAtDay.ToList().ForEach(x => UoW.Delete(x));
				DriversAtDay.Clear();
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
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран водитель!");
				return;
			}

			var carJournalPage = (NavigationManager as ITdiCompatibilityNavigation)
				.OpenViewModelOnTdi<CarJournalViewModel, Action<CarJournalFilterViewModel>>(this, filter =>
				{
					filter.Archive = false;
					filter.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company };
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += (o, args) =>
					{
						var car = UoW.GetById<Car>(args.SelectedObjects.Cast<Car>().First().Id);
						DriversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x => x.Car = null);
						driver.Car = car;
					};
				});
		}

		protected void OnButtonAppointForwardersClicked(object sender, EventArgs e)
		{
			var toAdd = new List<AtWorkForwarder>();

			foreach(var forwarder in ForwardersAtDay.Where(f => DriversAtDay.All(d => d.WithForwarder != f)))
			{
				var defaulDriver = DriversAtDay.FirstOrDefault(d => d.WithForwarder == null
					&& d.Employee.DefaultForwarder?.Id == forwarder.Employee.Id);

				if(defaulDriver != null)
				{
					defaulDriver.WithForwarder = forwarder;
				}
				else
				{
					toAdd.Add(forwarder);
				}
			}

			if(toAdd.Count == 0)
			{
				return;
			}

			var orders = _scheduleRestrictionRepository.OrdersCountByDistrict(UoW, _filterViewModel.AtDate, 12);
			var districtsBottles = orders.GroupBy(x => x.DistrictId).ToDictionary(x => x.Key, x => x.Sum(o => o.WaterCount));

			foreach(var forwarder in toAdd)
			{
				var driversToAdd = DriversAtDay.Where(x =>
						x.WithForwarder == null
						&& x.Car != null
						&& !(x.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
							&& x.Car.GetActiveCarVersionOnDate(x.Date).CarOwnType == CarOwnType.Company))
					.ToList();

				if(driversToAdd.Count == 0)
				{
					_logger.LogWarning("Не осталось водителей для добавленя экспедиторов.");
					break;
				}

				int ManOnDistrict(int districtId) => DriversAtDay
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
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет заказов на день для определения приоритета водителя.");
					return;
				}

				var driver = driversToAddWithWithActivePrioritySets
					.OrderByDescending(x => districtsBottles
						.Where(db => x.Employee.DriverDistrictPrioritySets
							.First(s => s.IsActive).DriverDistrictPriorities
							.Any(dd => dd.District.Id == db.Key))
						.Max(db => (double)db.Value / ManOnDistrict(db.Key)))
					.FirstOrDefault();

				if(driver != null)
				{
					driver.WithForwarder = forwarder;
				}
			}

			_interactiveService.ShowMessage(ImportanceLevel.Info, "Готово.");
		}

		protected void OnButtonOpenCarClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>().FirstOrDefault();

			if(selected is null)
			{
				return;
			}

			(NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<CarViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selected.Car.Id));
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
				(NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<EmployeeViewModel, IEntityUoWBuilder>(
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

			foreach(var forwarder in toDel)
			{
				if(forwarder.Id > 0)
				{
					UoW.Delete(forwarder);
				}

				ForwardersAtDay.Remove(forwarder);
			}
		}

		#endregion Buttons

		#region YTreeView

		private void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			var sensitiveness = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() > 0;

			buttonRemoveDriver.Sensitive = sensitiveness;
			buttonDriverSelectAuto.Sensitive = sensitiveness;
			buttonOpenDriver.Sensitive = sensitiveness;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() != 1 && districtpriorityview1.Visible)
			{
				districtpriorityview1.Visible = false;
			}

			buttonOpenCar.Sensitive = false;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1)
			{
				var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
				districtpriorityview1.ListParent = selected[0];
				districtpriorityview1.Districts = selected[0].ObservableDistrictsPriorities;
				buttonOpenCar.Sensitive = selected[0].Car != null;
				ChangeButtonAddRemove(selected[0].Status == AtWorkDriver.DriverStatus.NotWorking);
			}

			districtpriorityview1.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1;
		}

		private void YtreeviewForwarders_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveForwarder.Sensitive = ytreeviewOnDayForwarders.Selection.CountSelectedRows() > 0;
		}

		private void ObservableForwardersAtDay_ListChanged(object aList)
		{
			var renderer = ytreeviewAtWorkDrivers.ColumnsConfig.GetRendererMappingByTagGeneric<ComboRendererMapping<AtWorkDriver, AtWorkForwarder>>(Columns.Forwarder).First();
			renderer.FillItems(ForwardersAtDay, "без экспедитора");
		}

		#endregion YTreeView

		protected void OnYdateAtWorksDateChanged(object sender, EventArgs e)
		{
			OnTabNameChanged();
		}

		private void SelectDrivers_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var addDrivers = e.SelectedNodes;
			_logger.LogInformation("Получаем авто для водителей...");
			var onlyNewIds = addDrivers.Where(x => !DriversAtDay.Any(dat => dat.Employee.Id == x.Id)).Select(x => x.Id);
			_hasNewDrivers = onlyNewIds.Any();
			var allCars = _carRepository.GetCarsByDrivers(UoW, onlyNewIds.ToArray());

			var drivers = UoW.GetById<Employee>(onlyNewIds);

			var toAdd = new List<AtWorkDriver>();

			foreach(var driver in drivers)
			{
				var daySchedule = GetDriverWorkDaySchedule(driver);
				var atwork = new AtWorkDriver(driver, _filterViewModel.AtDate, allCars.FirstOrDefault(x => x.Driver.Id == driver.Id), daySchedule);

				GetDefaultForwarder(driver, atwork);

				toAdd.Add(atwork);
			}

			RefillDriversAtDay(DriversAtDay.Union(toAdd).ToArray());

			_logger.LogInformation("Ок");

			SetButtonClearDriverScreenSensitive();
			SetButtonCreateEmptyRouteListsSensitive();

			(sender as EmployeesJournalViewModel).OnEntitySelectedResult -= SelectDrivers_OnEntitySelectedResult;
		}

		protected void OnHideForwadersToggled(object o, Gtk.ToggledArgs args)
		{
			vboxForwarders.Visible = hideForwaders.ArrowDirection == Gtk.ArrowType.Down;
		}

		private void OnForwardersSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedForwardersIds = e.SelectedNodes.Select(x => x.Id)
				.Where(x => !ForwardersAtDay.Any(fad => fad.Employee.Id != x));

			var loaded = UoW.GetById<Employee>(selectedForwardersIds);

			var forwardersToAdd = new List<AtWorkForwarder>();

			foreach(var forwarder in loaded)
			{
				forwardersToAdd.Add(new AtWorkForwarder(forwarder, _filterViewModel.AtDate));
			}

			RefillForwardersAtDay(ForwardersAtDay.Union(forwardersToAdd));
		}

		#endregion EventHandlers

		#region Methods

		private void SetButtonClearDriverScreenSensitive()
		{
			if(_filterViewModel.AtDate < DateTime.Now.Date || !DriversAtDay.Any())
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
			ybuttonCreateRouteLists.Sensitive = DriversAtDay.Any();
		}

		private void ChangeButtonAddRemove(bool needRemove)
		{
			if(!_canReturnDriver)
			{
				return;
			}

			if(needRemove)
			{
				buttonRemoveDriver.Label = "Вернуть водителя";
				buttonRemoveDriver.Image = new Gtk.Image() { Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu) };
			}
			else
			{
				buttonRemoveDriver.Label = "Снять водителя";
				buttonRemoveDriver.Image = new Gtk.Image() { Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu) };
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
				? _defaultDeliveryDaySchedule
				: driverWorkSchedule.DaySchedule;
		}

		private void GetDefaultForwarder(Employee driver, AtWorkDriver atwork)
		{
			if(driver.DefaultForwarder == null)
			{
				return;
			}

			var forwarder = ForwardersAtDay.FirstOrDefault(x => x.Employee.Id == driver.DefaultForwarder.Id);

			if(forwarder == null
				&& _interactiveService.Question(
					  $"Водитель {driver.ShortName} обычно ездит с экспедитором {driver.DefaultForwarder.ShortName}. " +
					  $"Он отсутствует в списке экспедиторов. Добавить его в список?"))
			{
				forwarder = new AtWorkForwarder(driver.DefaultForwarder, _filterViewModel.AtDate);
				ForwardersAtDay.Add(forwarder);
			}

			if(forwarder != null && DriversAtDay.All(x => x.WithForwarder != forwarder))
			{
				atwork.WithForwarder = forwarder;
			}
		}

		public bool Save()
		{
			// В случае, если вкладка сохраняется, а в списке есть Снятые водители, сделать проверку, что у каждого из них заполнена причина.
			var NotWorkingDrivers = DriversAtDay
				.Where(driver => driver.Status == AtWorkDriver.DriverStatus.NotWorking);

			if(NotWorkingDrivers.Any())
			{
				foreach(var atWorkDriver in NotWorkingDrivers)
				{
					if(!string.IsNullOrEmpty(atWorkDriver.Reason))
					{
						continue;
					}

					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не у всех снятых водителей указаны причины!");
					return false;
				}
			}

			foreach(var driver in DriversAtDay)
			{
				if(driver.GeographicGroup == null)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, "Не у всех водителей указана база!");
					return false;
				}

				if(driver.Car == null)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, "Не у всех водителей указан авто!");
					return false;
				}
			}

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			// Сохранение изменившихся за этот раз авторов и дат комментариев

			foreach(var atWorkDriver in _driversWithCommentChanged)
			{
				atWorkDriver.CommentLastEditedAuthor = currentEmployee;
				atWorkDriver.CommentLastEditedDate = DateTime.Now;
			}

			_driversWithCommentChanged.Clear();
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

		private string RenderForwaderWithDriver(AtWorkForwarder atWork)
		{
			return string.Join(", ", DriversAtDay.Where(x => x.WithForwarder == atWork).Select(x => x.Employee.ShortName));
		}

		private void FillDialogAtDay()
		{
			_logger.LogInformation("Загружаем экспедиторов на {0:d}...", _filterViewModel.AtDate);
			RefillForwardersAtDay(_atWorkRepository.GetForwardersAtDay(UoW, _filterViewModel.AtDate));

			_logger.LogInformation("Загружаем водителей на {0:d}...", _filterViewModel.AtDate);

			var selectedGeographicGroupIds = _filterViewModel
				.GeographicGroupNodes
				.Where(ggn => ggn.Selected)
				.Select(ggn => ggn.GeographicGroup.Id)
				.ToArray();

			RefillDriversAtDay(_atWorkRepository.GetDriversAtDay(
				UoW,
				_filterViewModel.AtDate,
				driverStatuses: _filterViewModel.SelectedDriverStatuses,
				carTypesOfUse: _filterViewModel.SelectedCarTypesOfUse,
				carOwnTypes: _filterViewModel.SelectedCarOwnTypes,
				geoGroupIds: selectedGeographicGroupIds));

			_logger.LogInformation("Ок");

			CheckAndCorrectDistrictPriorities();
			SetButtonClearDriverScreenSensitive();
			SetButtonCreateEmptyRouteListsSensitive();
		}

		//Если дата диалога >= даты активации набора районов и есть хотя бы один район у водителя, который не принадлежит активному набору районов
		private void CheckAndCorrectDistrictPriorities()
		{
			var activeDistrictsSet = UoW.Session.QueryOver<DistrictsSet>().Where(x => x.Status == DistrictsSetStatus.Active).SingleOrDefault();

			if(activeDistrictsSet == null)
			{
				throw new InvalidOperationException("Не найдена активная версия районов");
			}

			if(activeDistrictsSet.DateActivated == null)
			{
				throw new InvalidOperationException("У активной версии районов не проставлена дата активации");
			}

			if(_filterViewModel.AtDate.Date >= activeDistrictsSet.DateActivated.Value.Date)
			{
				var outDatedpriorities = DriversAtDay.SelectMany(x => x.DistrictsPriorities.Where(d => d.District.DistrictsSet.Id != activeDistrictsSet.Id)).ToList();

				if(!outDatedpriorities.Any())
				{
					return;
				}

				int deletedCount = 0;

				foreach(var priority in outDatedpriorities)
				{
					var newDistrict = activeDistrictsSet.ObservableDistricts.FirstOrDefault(x => x.CopyOf == priority.District);

					if(newDistrict == null)
					{
						priority.Driver.ObservableDistrictsPriorities.Remove(priority);
						UoW.Delete(priority);
						deletedCount++;
					}
					else
					{
						priority.District = newDistrict;
						UoW.Save(priority);
					}
				}

				_interactiveService.ShowMessage(ImportanceLevel.Info, $"Были найдены и исправлены устаревшие приоритеты районов.\nУдалено приоритетов, ссылающихся на несуществующий район: {deletedCount}");

				ytreeviewAtWorkDrivers.YTreeModel.EmitModelChanged();
			}
		}

		#endregion Methods

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}

		public enum Columns
		{
			Forwarder
		}
	}
}
