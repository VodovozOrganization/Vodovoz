using Autofac;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReportGroupPrint : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;

		private readonly IInteractiveService _interactiveService;
		private Func<ReportInfo> _selectedReport;
		private IList<NamedDomainObjectNode> _availableSubdivisionsForOneDayGroupReport;
		private List<DriverSelectableNode> _availableDriversList = new List<DriverSelectableNode>();

		private ITdiTab _parentTab;
		private Car _car;
		private Organization _organization;

		public WayBillReportGroupPrint(
			IReportInfoFactory reportInfoFactory,
			ILifetimeScope lifetimeScope,
			IEmployeeJournalFactory employeeJournalFactory,
			IInteractiveService interactiveService,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));

			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			OrganizationViewModel = new LegacyEEVMBuilderFactory<WayBillReportGroupPrint>(ParentTab, this, UoW, navigationManager, _lifetimeScope)
				.ForProperty(x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			ConfigureSingleReport();
			ConfigureGroupReportForOneDay();

			frameSingleReport.Sensitive = true;
			frameOneDayGroupReport.Sensitive = false;

			_selectedReport = () => GetSingleReportInfo();

			ybuttonCreateReport.Clicked += OnButtonCreateRepotClicked;
			buttonInfoSingleReport.Clicked += OnButtonInfoSingleReportClicked;
			buttonInfoOneDayGroupReport.Clicked += OnButtonInfoOneDayGroupReportClicked;

			enumcheckCarTypeOfUseOneDayGroupReport.CheckStateChanged += EnumcheckCarTypeOfUseOneDayGroupReportCheckStateChanged;
			enumcheckCarOwnTypeOneDayGroupReport.CheckStateChanged += EnumcheckCarOwnTypeOneDayGroupReportCheckStateChanged;
		}

		#region Properties

		public IEntityEntryViewModel OrganizationViewModel { get; }

		public Car Car
		{
			get => _car;
			set
			{
				if(_car != value)
				{
					_car = value;

					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Car)));
				}
			}
		}

		public Organization Organization
		{
			get => _organization;
			set
			{
				if(_organization != value)
				{
					_organization = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Organization)));
				}
			}
		}

		public ITdiTab ParentTab
		{
			get => _parentTab;
			set
			{
				_parentTab = value;

				if(entityentryCar.ViewModel == null)
				{
					entityentryCar.ViewModel = BuildCarEntryViewModel();
				}
			}
		}
		#endregion Properties

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var navigationManager = _lifetimeScope.BeginLifetimeScope().Resolve<INavigationManager>();

			var viewModel = new LegacyEEVMBuilderFactory<WayBillReportGroupPrint>(ParentTab, this, UoW, navigationManager, _lifetimeScope)
			.ForProperty(x => x.Car)
			.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
				filter =>
				{
				})
			.UseViewModelDialog<CarViewModel>()
			.Finish();

			viewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		#region Конфигурация элементов управления виджета отчетов
		/// <summary>
		/// Первичная конфигурация одиночного отчета
		/// </summary>
		private void ConfigureSingleReport()
		{
			//Дата по умолчанию
			datepickerSingleReport.Date = DateTime.Today;

			//Время отправления по умолчанию
			timeHourEntrySingleReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntrySingleReport.Text = DateTime.Now.Minute.ToString("00.##");

			//Выбор водителя
			entityDriverSingleReport.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			yradiobuttonSingleReport.Clicked += OnRadiobuttonSingleReportClicked;
		}

		/// <summary>
		/// Первичная конфигурация группового отчета за один день для транспортного отдела
		/// </summary>
		private void ConfigureGroupReportForOneDay()
		{
			//Дата по умолчанию
			datepickerOneDayGroupReport.Date = DateTime.Today;
			datepickerOneDayGroupReport.DateChanged += OnDatepickerOneDayGroupReportDateChanged;

			// Тип автомобиля
			enumcheckCarTypeOfUseOneDayGroupReport.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUseOneDayGroupReport.AddEnumToHideList(CarTypeOfUse.Loader);
			SetChekBoxesInActive(new string[]{ CarTypeOfUse.Largus.ToString(), CarTypeOfUse.Minivan.ToString() }, ref enumcheckCarTypeOfUseOneDayGroupReport);

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayGroupReport.EnumType = typeof(CarOwnType);
			SetChekBoxesInActive(new string[] { CarOwnType.Company.ToString() }, ref enumcheckCarOwnTypeOneDayGroupReport);

			//Выбор подразделения
			comboSubdivisionsOneDayGroupReport.SetRenderTextFunc<NamedDomainObjectNode>(x => x.Name);
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Any();
			comboSubdivisionsOneDayGroupReport.Changed += OnComboSubdivisionsOneDayGroupReportChanged;

			//Время отправления по умолчанию
			timeHourEntryOneDayGroupReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntryOneDayGroupReport.Text = DateTime.Now.Minute.ToString("00.##");

			yradiobuttonOneDayGroupReport.Clicked += OnRadiobuttonOneDayGroupReportClicked;

			ytreeviewDrivers.ColumnsConfig = FluentColumnsConfig<DriverSelectableNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(d => d.IsSelected)
				.AddColumn("№").AddNumericRenderer(d => d.NodeNumber)
				.AddColumn("Водитель").AddTextRenderer(d => d.FullName)
				.AddColumn("Гос. номер").AddTextRenderer(d => d.CarRegistrationNumber)
				.Finish();
		}
		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetSingleReportInfo()
		{
			var parameters = new Dictionary<string, object>
				{
					{ "date", datepickerSingleReport.Date },
					{ "driver_id", (entityDriverSingleReport?.Subject as Employee)?.Id ?? -1 },
					{ "car_id", Car?.Id ?? -1 },
					{ "time", timeHourEntrySingleReport.Text + ":" + timeMinuteEntrySingleReport.Text },
					{ "need_date", !datepickerSingleReport.IsEmpty }
				};

			var reportInfo = _reportInfoFactory.Create("Logistic.WayBillReport", Title, parameters);
			return reportInfo;
		}

		private ReportInfo GetGroupReportInfoForOneDay()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "date", _date },
				{ "auto_types", _carTypesOfUse.Any() ? _carTypesOfUse : new[] { (object)0 } },
				{ "owner_types", _carOwnTypes.Any() ? _carOwnTypes : new[] { (object)0 } },
				{ "subdivisions", _subdivisionIds.Any() ? _subdivisionIds : new[] { -1 } },
				{ "exclude_drivers", _selectedDriversIds.Any() ? _selectedDriversIds : new[] { -1 } },
				{ "time", timeHourEntryOneDayGroupReport.Text + ":" + timeMinuteEntryOneDayGroupReport.Text },
				{ "need_date", !datepickerOneDayGroupReport.IsEmpty }
			};

			var reportInfo = _reportInfoFactory.Create("Logistic.WayBillReportOneDayGroupPrint", Title, parameters);
			return reportInfo;
		}

		private IList<NamedDomainObjectNode> GetAvailableSubdivisionsListInAccordingWithCarParameters()
		{

			var selectedCarTypeOfUses = _carTypesOfUse.Cast<CarTypeOfUse>().ToArray();
			var selectedCarOwnTypes = _carOwnTypes.Cast<CarOwnType>().ToArray();

			return _subdivisionRepository
				.GetAvailableSubdivisionsInAccordingWithCarTypeAndOwner(UoW, selectedCarTypeOfUses, selectedCarOwnTypes);
		}

		private static void SetChekBoxesInActive(string[] valuesToCheck, ref EnumCheckList checkList)
		{
			if(valuesToCheck.Length < 1) return;

			foreach(var check in checkList.Children.Cast<yCheckButton>())
			{
				if(valuesToCheck.Contains(check.Tag.ToString()))
				{
					check.Active = true;
				}
			}
		}

		private Enum[] _carTypesOfUse => enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues.ToArray();

		private Enum[] _carOwnTypes => enumcheckCarOwnTypeOneDayGroupReport.SelectedValues.ToArray();

		private int[] _subdivisionIds => 
			(comboSubdivisionsOneDayGroupReport.SelectedItem as NamedDomainObjectNode) != null
			? new[] { (comboSubdivisionsOneDayGroupReport.SelectedItem as NamedDomainObjectNode).Id }
			: _availableSubdivisionsForOneDayGroupReport.Select(s => s.Id).ToArray();

		private DateTime _date => 
			datepickerOneDayGroupReport.IsEmpty
			? DateTime.Now
			: datepickerOneDayGroupReport.Date;

		private int[] _selectedDriversIds => 
			_availableDriversList
			.Where(x => x.IsSelected)
			.Select(x => x.Id)
			.ToArray();

		private void FillDriversTreeView()
		{
			var selectedDriversIds = _selectedDriversIds;

			_availableDriversList = GetAvailableDrivers().ToList();

			for(int i = 0; i < _availableDriversList.Count; i++)
			{
				_availableDriversList[i].IsSelected = selectedDriversIds.Contains(_availableDriversList[i].Id);
			}

			ytreeviewDrivers.ItemsDataSource = _availableDriversList;
		}

		private IList<DriverSelectableNode> GetAvailableDrivers()
		{
			Car carAlias = null;
			Employee driverAlias = null;
			FuelType fuelTypeAlias = null;
			CarModel carModelAlias = null;
			CarVersion carVersionAlias = null;
			Subdivision subdivisionAlias = null;
			DriverSelectableNode driverNodeAlias = null;

			var drivers = UoW.Session.QueryOver(() => carAlias)
				.JoinAlias(() => carAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.FuelType, () => fuelTypeAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(() => carAlias.CarVersions, () => carVersionAlias)
				.Left.JoinAlias(() =>  driverAlias.Subdivision, () => subdivisionAlias)
				.Where(() => !carAlias.IsArchive)
				.Where(() => 
					driverAlias.Category == EmployeeCategory.driver
					&& driverAlias.Status == EmployeeStatus.IsWorking
					&& driverAlias.DateFired == null)
				.Where(Restrictions.In(Projections.Property(() => driverAlias.Subdivision.Id), _subdivisionIds))
				.Where(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse), _carTypesOfUse))
				.Where(Restrictions.In(Projections.Property(() => carVersionAlias.CarOwnType), _carOwnTypes))
				.Where(() => 
					carVersionAlias.StartDate <= _date
					&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate > _date))
				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => driverNodeAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => driverNodeAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => driverNodeAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => driverNodeAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => driverNodeAlias.CarRegistrationNumber))
				.TransformUsing(Transformers.AliasToBean<DriverSelectableNode>())
				.List<DriverSelectableNode>();

			drivers = drivers.OrderBy(d => d.LastName).ToList();
			for(int i = 0; i < drivers.Count; i++)
			{
				drivers[i].NodeNumber = i + 1;
			}

			return drivers;
		}

		protected void OnButtonInfoSingleReportClicked(object sender, EventArgs e)
		{
			var warning =
				"Формируется один путевой лист с данными из соответсвующих полей:" +
				$"\n\t'Дата' - дата выезда из гаража" +
				$"\n\t'Водитель' - информация о водителе" +
				$"\n\t'Автомобиль' - информация об автомобиле" +
				$"\n\t'Время' - время выезда из гаража" +
				$"\nДанные поля не являются обязательными к заполнению. При оставлении полей незаполненными (пустыми)," +
				$"\nв путевых листах будут отсутствовать соответствующие значения.";

			_interactiveService.ShowMessage(ImportanceLevel.Warning, warning, "Справка по работе с отчетом");
		}

		protected void OnButtonInfoOneDayGroupReportClicked(object sender, EventArgs e)
		{
			var info =
				"<b>1.</b> Формируется множество путевых листов для автомобилей, согласно установленным фильтрам" +
				$"\nпо типу и принадлежности автомобиля, а также выбранному значению подразделения." +
				$"\n" +
				$"\n<b>2.</b> При выборе пункта 'Все' в списке подразделений в выборку попадут автомобили," +
				$"\nудовлетворяющие условиям остальных фильтров, из всех подразделений." +
				$"\n" +
				$"\n<b>3.</b> Список подразделений обновляется при каждом изменении фильтра параметров автомобиля." +
				$"\nОтсутствие данных в списке подразделений означает, что автомобили, удовлетворяющие заданным" +
				$"\nусловиям не найдены ни в одном из подразделений." +
				$"\n" +
				$"\n<b>4.</b> Во всех путевых листах указываются одинаковые данные из следующих полей:" +
				$"\n\t'Дата' - дата выезда из гаража" +
				$"\n\t'Время' - время выезда из гаража" +
				$"\nДанные поля не являются обязательными к заполнению. При оставлении полей незаполненными (пустыми)," +
				$"\nв путевых листах будут отсутствовать соответствующие значения." +
				$"\nЕсли поле дата остается пустым, то в выборке будут присутствовать водители, соответствующие" +
				$"\nусловиям фильтра на текущую дату." +
				$"\n" +
				$"\n<b>5.</b> В выборку попадают только неархивные автомобили, имеющие \"привязанных\" водителей." +
				$"\nДанные водителя в каждый путевой лист вносятся автоматически." +
				$"\n" +
				$"\n<b>6.</b> Для исключения попадания водителя в выборку, в таблице \"Исключить из печати\" " +
				$"\nнеобходимо установить галочку в строке соответствующего водителя.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			if (yradiobuttonOneDayGroupReport.Active && 
				(_availableSubdivisionsForOneDayGroupReport.Count < 1 
				|| enumcheckCarOwnTypeOneDayGroupReport.SelectedValues.Count() < 1
				|| enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues.Count() < 1))
			{
				var info = "Недостаточно данных для отчета.";

				_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Ошибка формирования отчета");
				return;
			}

			if(yradiobuttonOneDayGroupReport.Active 
				&& _availableDriversList.Where(x => !x.IsSelected).Count() < 1)
			{
				var info = "Отсутствуют доступные водители";

				_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Ошибка формирования отчета");
				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(_selectedReport.Invoke(), true));
		}

		protected void OnRadiobuttonSingleReportClicked(object sender, EventArgs e)
		{
			_selectedReport = () => GetSingleReportInfo();
			frameSingleReport.Sensitive = true;
			frameOneDayGroupReport.Sensitive = false;
		}

		protected void OnRadiobuttonOneDayGroupReportClicked(object sender, EventArgs e)
		{
			_selectedReport = () => GetGroupReportInfoForOneDay();
			frameSingleReport.Sensitive = false;
			frameOneDayGroupReport.Sensitive = true;
			FillDriversTreeView();
		}

		private void OnDatepickerOneDayGroupReportDateChanged(object sender, EventArgs e)
		{
			FillDriversTreeView();
		}

		private void OnComboSubdivisionsOneDayGroupReportChanged(object sender, EventArgs e)
		{
			FillDriversTreeView();
		}

		private void EnumcheckCarTypeOfUseOneDayGroupReportCheckStateChanged(object sender, CheckStateChangedEventArgs e)
		{
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Any();
			FillDriversTreeView();
		}

		private void EnumcheckCarOwnTypeOneDayGroupReportCheckStateChanged(object sender, CheckStateChangedEventArgs e)
		{
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Any();
			FillDriversTreeView();
		}

		private class DriverSelectableNode
		{
			public int Id { get; set; }
			public int NodeNumber { get; set; }
			public string Name { get; set; }
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public string CarRegistrationNumber { get; set; }
			public bool IsSelected { get; set; } = false;
			public string FullName => LastName + " " + Name[0] + "." + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic[0] + ".");
		}
	}
}

