using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Fuel;
using Vodovoz.Settings.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.FuelDocuments
{
	public class FuelDocumentViewModel : TabViewModelBase, ITDICloseControlTab
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrackRepository _trackRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly ISubdivisionRepository _subdivisionsRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IFuelApiService _fuelApiService;
		private readonly IFuelControlSettings _fuelControlSettings;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IYesNoCancelQuestionInteractive _yesNoCancelQuestionInteractive;
		private FuelCashOrganisationDistributor _fuelCashOrganisationDistributor;

		private FuelDocument _fuelDocument;
		private Employee _cashier;
		private RouteList _routeList;
		private bool _autoCommit;
		private bool _canOpenExpense;
		private decimal _fuelBalance;
		private decimal _fuelOutlayed;
		private int _fuelLimitTransactionsCount;
		private int _fuelLimitTransactionsCountMaxValue;
		private bool _isOnlyDocumentsCreation;
		private bool _isGiveFuelInMoneySelected;
		private bool _isDocumentSavingInProcess;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private int _fuelLimitMaxTransactionsCount;
		private decimal _maxDailyFuelLimitForCar;

		#region ctor
		/// <summary>
		/// Открывает диалог выдачи топлива, с коммитом изменений в родительском UoW
		/// Создание нового документа из диалого закрытия МЛ
		/// </summary>
		public FuelDocumentViewModel
		(
			IUnitOfWork uow,
			RouteList rl,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionsRepository,
			IEmployeeRepository employeeRepository,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager,
			ITrackRepository trackRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			IFuelApiService fuelApiService,
			IFuelControlSettings fuelControlSettings,
			ICarEventSettings carEventSettings,
			IGuiDispatcher guiDispatcher,
			IUserSettingsService userSettingsService,
			IYesNoCancelQuestionInteractive yesNoCancelQuestionInteractive,
			ILifetimeScope lifetimeScope)
			: base(commonServices?.InteractiveService, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_yesNoCancelQuestionInteractive = yesNoCancelQuestionInteractive ?? throw new ArgumentNullException(nameof(yesNoCancelQuestionInteractive));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			UoW = uow;
			FuelDocument = new FuelDocument();
			FuelDocument.UoW = UoW;
			_autoCommit = false;
			RouteList = rl;

			CarEntryViewModel = BuildCarEntryViewModel(lifetimeScope);
			FuelTypeEntryViewModel = BuildFuelTypeEntryViewModel(lifetimeScope);

			Configure();
		}

		/// <summary>
		/// Открытие существующего документа из диалога закрытия МЛ
		/// </summary>
		public FuelDocumentViewModel
		(
			IUnitOfWork uow,
			FuelDocument fuelDocument,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionsRepository,
			IEmployeeRepository employeeRepository,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager,
			ITrackRepository trackRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			IFuelApiService fuelApiService,
			IFuelControlSettings fuelControlSettings,
			ICarEventSettings carEventSettings,
			IGuiDispatcher guiDispatcher,
			IUserSettingsService userSettingsService,
			IYesNoCancelQuestionInteractive yesNoCancelQuestionInteractive,
			ILifetimeScope lifetimeScope)
			: base(commonServices?.InteractiveService, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_yesNoCancelQuestionInteractive = yesNoCancelQuestionInteractive ?? throw new ArgumentNullException(nameof(yesNoCancelQuestionInteractive));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			UoW = uow;
			FuelDocument = uow.GetById<FuelDocument>(fuelDocument.Id);		
			
			FuelDocument.UoW = UoW;
			_autoCommit = false;
			RouteList = FuelDocument.RouteList;

			CarEntryViewModel = BuildCarEntryViewModel(lifetimeScope);
			FuelTypeEntryViewModel = BuildFuelTypeEntryViewModel(lifetimeScope);

			Configure();
		}

		/// <summary>
		/// Открывает диалог выдачи топлива, с автоматическим коммитом всех изменений
		/// Создание нового документа из журнала "Работа кассы с МЛ"
		/// </summary>
		public FuelDocumentViewModel
		(
			RouteList rl,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionsRepository,
			IEmployeeRepository employeeRepository,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager,
			ITrackRepository trackRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			IFuelApiService fuelApiService,
			IFuelControlSettings fuelControlSettings,
			ICarEventSettings carEventSettings,
			IGuiDispatcher guiDispatcher,
			IUserSettingsService userSettingsService,
			IYesNoCancelQuestionInteractive yesNoCancelQuestionInteractive,
			ILifetimeScope lifetimeScope)
			: base(commonServices?.InteractiveService, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_yesNoCancelQuestionInteractive = yesNoCancelQuestionInteractive ?? throw new ArgumentNullException(nameof(yesNoCancelQuestionInteractive));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			var uow = _uowFactory.CreateWithNewRoot<FuelDocument>();
			UoW = uow;
			FuelDocument = uow.Root;
			FuelDocument.UoW = UoW;
			_autoCommit = true;
			RouteList = UoW.GetById<RouteList>(rl.Id);

			CarEntryViewModel = BuildCarEntryViewModel(lifetimeScope);
			FuelTypeEntryViewModel = BuildFuelTypeEntryViewModel(lifetimeScope);

			Configure();
		}

		/// <summary>
		/// Открывает диалог выдачи топлива, с автоматическим коммитом всех изменений
		/// Создание нового документа из журнала "Журнал МЛ"
		/// </summary>
		public FuelDocumentViewModel
		(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEntityUoWBuilder entityUoWBuilder,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionsRepository,
			IEmployeeRepository employeeRepository,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager,
			ITrackRepository trackRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			IFuelApiService fuelApiService,
			IFuelControlSettings fuelControlSettings,
			ICarEventSettings carEventSettings,
			IGuiDispatcher guiDispatcher,
			IUserSettingsService userSettingsService,
			IYesNoCancelQuestionInteractive yesNoCancelQuestionInteractive,
			ILifetimeScope lifetimeScope)
			: base(commonServices?.InteractiveService, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}
			_uowFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_yesNoCancelQuestionInteractive = yesNoCancelQuestionInteractive ?? throw new ArgumentNullException(nameof(yesNoCancelQuestionInteractive));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			var uow = entityUoWBuilder.CreateUoW<FuelDocument>(_uowFactory);
			UoW = uow;
			FuelDocument = uow.Root;
			FuelDocument.UoW = UoW;
			_autoCommit = true;

			CarEntryViewModel = BuildCarEntryViewModel(lifetimeScope);
			FuelTypeEntryViewModel = BuildFuelTypeEntryViewModel(lifetimeScope);

			Configure();
		}

		private void Configure()
		{
			if(!InitActualCashier() || !IsCurrentCashierCanGiveFuel())
			{
				AbortOpening();
				return;
			}

            if(RouteList != null && !RouteList.HasAddressesOrAdditionalLoading)
            {
                AbortOpening("Запрещено выдавать топливо для МЛ без адресов или без погруженного запаса");
                return;
            }

            if(FuelDocument.Id == 0 && RouteList != null)
			{
				FuelDocument.FillEntity(RouteList);
			}

			if(RouteList != null)
			{
				if(!CarHasFuelType())
				{
					AbortOpening();
					return;
				}

				SetFuelLimitTransactionsCount();
			}

			TabName = "Выдача топлива";
			_fuelCashOrganisationDistributor = new FuelCashOrganisationDistributor(_organizationRepository);

			CreateCommands();

			IsGiveFuelInMoneySelected = FuelDocument?.FuelOperation?.PayedLiters > 0m;

			FuelDocument.PropertyChanged += FuelDocument_PropertyChanged;

			OpenExpenseCommand = new DelegateCommand(OpenExpense, () => CanOpenExpense);
		}

		#endregion ctor

		public virtual IUnitOfWork UoW { get; set; }

		[PropertyChangedAlso(nameof(FuelInfo), nameof(ResultInfo))]
		public virtual FuelDocument FuelDocument
		{
			get => _fuelDocument;
			set => SetField(ref _fuelDocument, value);
		}

		[PropertyChangedAlso(nameof(IsFuelLimitsCanBeEdited))]
		public RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[PropertyChangedAlso(nameof(IsDocumentCanBeEdited))]
		public virtual Employee Cashier
		{
			get => _cashier;
			set => SetField(ref _cashier, value);
		}

		public virtual bool CanOpenExpense
		{
			get => _canOpenExpense;
			set => SetField(ref _canOpenExpense, value);
		}

		public virtual int FuelLimitTransactionsCount
		{
			get => _fuelLimitTransactionsCount;
			set
			{
				if(value > _fuelLimitTransactionsCountMaxValue)
				{
					SetField(ref _fuelLimitTransactionsCount, _fuelLimitTransactionsCountMaxValue);
					return;
				}

				SetField(ref _fuelLimitTransactionsCount, value);
			}
		}

		public virtual int FuelLimitTransactionsCountMaxValue
		{
			get => _fuelLimitTransactionsCountMaxValue;
			set => SetField(ref _fuelLimitTransactionsCountMaxValue, value);
		}

		public virtual bool IsOnlyDocumentsCreation
		{
			get => _isOnlyDocumentsCreation;
			set => SetField(ref _isOnlyDocumentsCreation, value);
		}

		[PropertyChangedAlso(nameof(CanChangeDate), nameof(IsFuelLimitsCanBeEdited))]
		public virtual bool IsGiveFuelInMoneySelected
		{
			get => _isGiveFuelInMoneySelected;
			set => SetField(ref _isGiveFuelInMoneySelected, value);
		}

		[PropertyChangedAlso(nameof(IsDocumentCanBeSaved))]
		public virtual bool IsDocumentSavingInProcess
		{
			get => _isDocumentSavingInProcess;
			set => SetField(ref _isDocumentSavingInProcess, value);
		}

		public virtual bool IsUserCanGiveFuelLimits =>
			IsCurrentUserHasPermissonToGiveFuelLimit || IsUserWorkInCashSubdivisions;

		public virtual bool IsUserCanGiveFuelInMoney =>
			IsUserWorkInCashSubdivisions;

		[PropertyChangedAlso(nameof(CanChangeDate), nameof(IsDocumentCanBeSaved))]
		public virtual bool IsDocumentCanBeEdited =>
			UoW.IsNew || FuelDocument.FuelLimitLitersAmount == 0;

		public virtual bool IsDocumentCanBeSaved => IsDocumentCanBeEdited && !IsDocumentSavingInProcess;

		public virtual bool IsFuelLimitsCanBeEdited =>
			IsNewEditable
			&& !IsGiveFuelInMoneySelected
			&& IsUserCanGiveFuelLimits
			&& _autoCommit
			&& RouteList?.Date >= DateTime.Today;

		public virtual bool IsFuelInMoneyCanBeEdited =>
			IsNewEditable && IsUserCanGiveFuelInMoney;

		public virtual string CashExpenseInfo => UpdateCashExpenseInfo();

		public virtual bool IsNewEditable => FuelDocument.Id <= 0 && IsDocumentCanBeEdited;

		public virtual bool CanChangeDate =>
			IsDocumentCanBeEdited
			&& _commonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeFuelCardNumber,
				_commonServices.UserService.CurrentUserId)
			&& IsGiveFuelInMoneySelected;

		public IList<Subdivision> AvailableSubdivisionsForUser
		{
			get
			{
				var user = _commonServices.UserService.GetCurrentUser();
				var employee = _employeeRepository.GetEmployeesForUser(UoW, user.Id).FirstOrDefault();
				var subdivisions = _subdivisionsRepository.GetCashSubdivisionsAvailableForUser(UoW, user).ToList();

				if(subdivisions.Any(x => x.Id == employee.Subdivision.Id))
				{
					FuelDocument.Subdivision = employee.Subdivision;
				}

				return subdivisions;
			}
		}

		public string FuelInfo => UpdateFuelInfo();

		public string ResultInfo => UpdateResutlInfo();

		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelector { get; }
		public IEntityEntryViewModel CarEntryViewModel { get; }
		public IEntityEntryViewModel FuelTypeEntryViewModel { get; }

		private IEntityEntryViewModel BuildCarEntryViewModel(ILifetimeScope lifetimeScope)
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<FuelDocument>(this, FuelDocument, UoW, NavigationManager, lifetimeScope);

			var viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			viewModel.IsEditable = false;
			viewModel.CanViewEntity = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		private IEntityEntryViewModel BuildFuelTypeEntryViewModel(ILifetimeScope lifetimeScope)
		{
			var fuelTypeViewModelBuilder = new CommonEEVMBuilderFactory<FuelDocument>(this, FuelDocument, UoW, NavigationManager, lifetimeScope);

			var viewModel = fuelTypeViewModelBuilder
				.ForProperty(x => x.Fuel)
				.UseViewModelJournalAndAutocompleter<FuelTypeJournalViewModel>()
				.UseViewModelDialog<FuelTypeViewModel>()
				.Finish();

			viewModel.IsEditable = false;
			viewModel.CanViewEntity = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelType)).CanUpdate;

			return viewModel;
		}

		public void SetRouteListById(int routeListId)
		{
			RouteList = UoW.GetById<RouteList>(routeListId);

			if(UoW.IsNew)
			{
				FuelDocument.FillEntity(RouteList);
			}

			SetFuelLimitTransactionsCount();
			OnPropertyChanged(nameof(FuelInfo));
		}

		private bool InitActualCashier()
		{
			Cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(Cashier == null)
			{
				ShowWarningMessage("Ваш пользователь не привязан к действующему сотруднику, Вы не можете выдавать денежные средства и топливо, так как некого указывать в качестве кассира.");
				return false;
			}

			return true;
		}

		private bool IsCurrentCashierCanGiveFuel()
		{
			if(!IsUserWorkInCashSubdivisions && !IsCurrentUserHasPermissonToGiveFuelLimit)
			{
				ShowWarningMessage("Выдать топливо может только сотрудник кассы, либо должно иметься право на выдачу топливных лимитов");
				return false;
			}

			return true;
		}

		private void SetFuelLimitTransactionsCount()
		{
			SetFuelDispensingRestrictionsParameters();

			if(UoW.IsNew)
			{
				FuelLimitTransactionsCountMaxValue = _fuelLimitMaxTransactionsCount;
			}
			else
			{
				FuelLimitTransactionsCountMaxValue = FuelDocument?.FuelLimit?.TransctionsCount ?? 1;
			}

			FuelLimitTransactionsCount = FuelLimitTransactionsCountMaxValue;
		}

		private void SetFuelDispensingRestrictionsParameters()
		{
			int maxTransactionsCount;
			decimal maxDailyFuelLimit;

			switch (FuelDocument.Car?.CarModel?.CarTypeOfUse)
			{
				case CarTypeOfUse.Largus:
					maxTransactionsCount = _fuelControlSettings.LargusFuelLimitMaxTransactionsCount;
					maxDailyFuelLimit = _fuelControlSettings.LargusMaxDailyFuelLimit;
					break;
				case CarTypeOfUse.GAZelle:
					maxTransactionsCount = _fuelControlSettings.GAZelleFuelLimitMaxTransactionsCount;
					maxDailyFuelLimit = _fuelControlSettings.GAZelleMaxDailyFuelLimit;
					break;
				case CarTypeOfUse.Truck:
					maxTransactionsCount = _fuelControlSettings.TruckFuelLimitMaxTransactionsCount;
					maxDailyFuelLimit = _fuelControlSettings.TruckMaxDailyFuelLimit;
					break;
				case CarTypeOfUse.Loader:
					maxTransactionsCount = _fuelControlSettings.LoaderFuelLimitMaxTransactionsCount;
					maxDailyFuelLimit = _fuelControlSettings.LoaderMaxDailyFuelLimit;
					break;
				case CarTypeOfUse.Minivan:
					maxTransactionsCount = _fuelControlSettings.MinivanFuelLimitMaxTransactionsCount;
					maxDailyFuelLimit = _fuelControlSettings.MinivanMaxDailyFuelLimit;
					break;
				default:
					throw new InvalidOperationException("Невозможно определить максимальное допустимое значение количества транзакций. " +
					                                    "Возможные причины: не выбран авто, не указан модель авто, у модели авто не указан тип использования");
			}

			_fuelLimitMaxTransactionsCount = maxTransactionsCount;
			_maxDailyFuelLimitForCar = maxDailyFuelLimit;
		}

		private IEnumerable<Subdivision> CashSubdivisions =>
			_subdivisionsRepository?.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });

		private bool IsUserWorkInCashSubdivisions =>
			CashSubdivisions?.Contains(Cashier.Subdivision) ?? false;

		private bool IsCurrentUserHasPermissonToGiveFuelLimit =>
			_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Fuel.CanGiveFuelLimits);

		private bool CarHasFuelType()
		{
			if(RouteList.Car.FuelType == null)
			{
				ShowErrorMessage($"У машины {RouteList.Car.CarModel.Name} {RouteList.Car.Title} отсутствует тип топлива");
				return false;
			}

			return true;
		}

		private void CancelDocumentCreation()
		{
			if(!CanClose())
			{
				return;
			}

			Close(true, CloseSource.Cancel);
		}

		private void UpdateDependentDocumentsSaveAndClose()
		{
			if(FuelDocument.Id != 0 && FuelDocument.FuelLimitLitersAmount > 0)
			{
				ShowErrorMessage("Запрещено изменять документы, по которым выдавалось топливо лимитами");

				return;
			}

			UpdateDocumentEditionInfo();

			try
			{
				if(!IsFuelDocumentValid())
				{
					return;
				}

				if(IsMaxDailyFuelLimitExceededForCar())
				{
					return;
				}

				IsDocumentSavingInProcess = true;

				if(FuelDocument.Id == 0)
				{
					var isNeedToCreateFuelLimitOnServer =
						IsFuelLimitsCanBeEdited
						&& FuelDocument.FuelLimitLitersAmount > 0
						&& !IsOnlyDocumentsCreation;

					if(isNeedToCreateFuelLimitOnServer)
					{
						var isGazpromServiceAuthDataNotSet =
							string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiLogin)
							|| string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiPassword)
							|| string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiKey);

						if(isGazpromServiceAuthDataNotSet)
						{
							ShowErrorMessageInGuiThread("У Вас не указаны данные для авторизации в сервисе Газпром");
							IsDocumentSavingInProcess = false;
							return;
						}

						CreateFuelLimitFuelOperationSaveAndClose(_cancellationTokenSource.Token);
					}
					else
					{
						CreateFuelOperationSaveAndClose();
					}
				}
				else
				{
					FuelDocument.UpdateFuelOperation();
					SaveAndClose();
				}
			}
			catch(Exception ex)
			{
				LogAndShowExceptionMessageInGuiThread(ex);
			}
		}

		private void UpdateDocumentEditionInfo()
		{
			if(FuelDocument.Author == null)
			{
				FuelDocument.Author = _cashier;
			}

			FuelDocument.LastEditor = _cashier;

			FuelDocument.LastEditDate = DateTime.Now;

			if(FuelDocument.FuelCashExpense != null)
			{
				FuelDocument.FuelCashExpense.Casher = _cashier;
			}
		}

		private bool IsFuelDocumentValid()
		{
			var isValid = _commonServices.ValidationService.Validate(FuelDocument, new ValidationContext(FuelDocument));

			return isValid;
		}

		private async void CreateFuelLimitFuelOperationSaveAndClose(CancellationToken token)
		{
			var fuelCardId = _fuelRepository.GetFuelCardIdByNumber(UoW, FuelDocument.FuelCardNumber);

			if(fuelCardId == null)
			{
				return;
			}

			var fuelLimit = CreateFuelLimitForCard(fuelCardId);
			var existingLimits = Enumerable.Empty<FuelLimit>();
			var notUsedFuelLimits = Enumerable.Empty<FuelLimit>();

			try
			{
				existingLimits = await GetExistingFuleLimitsFromService(fuelCardId, token);
				notUsedFuelLimits = existingLimits.Where(l => l.UsedAmount < l.Amount);
			}
			catch(Exception ex)
			{
				LogAndShowExceptionMessageInGuiThread(ex);
				return;
			}

			_guiDispatcher.RunInGuiTread(async () =>
			{
				try
				{
					SummarizeNotUsedLimitsWithCurrentIfNeed(notUsedFuelLimits);

					UpdateExistingFuelDocumentsWithNotUsedLimits(notUsedFuelLimits);

					await RemoveFuelLimitsFromService(existingLimits.Select(l => l.LimitId), token);

					fuelLimit.Amount = FuelDocument.FuelLimitLitersAmount;
					fuelLimit.LimitId = await CreateNewFuelLimitInService(fuelLimit, token);
					fuelLimit.CreateDate = DateTime.Now;

					FuelDocument.FuelLimit = fuelLimit;

					CreateFuelOperationSaveAndClose();
				}
				catch(Exception ex)
				{
					LogAndShowExceptionMessageInGuiThread(ex);
				}
			});
		}

		private void LogAndShowExceptionMessageInGuiThread(Exception ex)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				IsDocumentSavingInProcess = false;
				_logger.Error(ex);

				ShowErrorMessage(ex.Message);
			});
		}

		private void ShowErrorMessageInGuiThread(string message)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				ShowErrorMessage(message);
			});
		}

		private bool IsMaxDailyFuelLimitExceededForCar()
		{
			var givedLitersOnDate = _fuelRepository.GetGivedFuelInLitersOnDate(UoW, FuelDocument.Car.Id, FuelDocument.Date);
			var totalFuelLitersAmount = givedLitersOnDate + FuelDocument.PayedLiters + FuelDocument.FuelLimitLitersAmount;

			var isLimitExceeded = totalFuelLitersAmount > _maxDailyFuelLimitForCar;

			if(isLimitExceeded)
			{
				ShowErrorMessage($"Выдать топливо нельзя! Достигнут максимальный лимит по выдаче топлива для авто.\n"
					+ $"На выбранную дату уже выдано топлива: {givedLitersOnDate} л.\n"
					+ $"Суточное ограничение по топливу: {_maxDailyFuelLimitForCar} л.");
			}

			return isLimitExceeded;
		}

		private void CreateFuelOperationSaveAndClose()
		{
			CreateFuelOperations();
			SaveAndClose();
		}

		private void CreateFuelOperations()
		{
			FuelDocument.CreateOperations(_fuelRepository, _organizationRepository, _financialCategoriesGroupsSettings);
			RouteList.ObservableFuelDocuments.Add(FuelDocument);

			if(IsGiveFuelInMoneySelected && FuelDocument.FuelPaymentType == FuelPaymentType.Cash)
			{
				_fuelCashOrganisationDistributor.DistributeCash(UoW, FuelDocument);
			}
		}

		private void SaveAndClose()
		{
			SaveDocument();

			IsDocumentSavingInProcess = false;

			_guiDispatcher.RunInGuiTread(() =>
			{
				Close(false, CloseSource.Save);
			});
		}

		private void SaveDocument()
		{
			_logger.Info("Сохраняем топливный документ...");

			if(_autoCommit)
			{
				UoW.Save();
			}
			else
			{
				UoW.Save(FuelDocument);
			}
		}

		private void SummarizeNotUsedLimitsWithCurrentIfNeed(IEnumerable<FuelLimit> notUsedFuelLimits)
		{
			var amountSum = notUsedFuelLimits.Select(l => l.Amount).Sum() ?? 0;
			var usedAmountSum = notUsedFuelLimits.Select(l => l.UsedAmount).Sum() ?? 0;
			var notUsedFuelLimitsSum = amountSum - usedAmountSum;

			if(notUsedFuelLimitsSum > 0)
			{
				var questionMessage = $"На сервере Газпром для данного авто имеется неиспользованные лимиты на {notUsedFuelLimitsSum} литров\n" +
					$"Выдать лимит с суммарным значением?\n\n" +
					$"\"Да\" - создастся лимит на {FuelDocument.FuelLimitLitersAmount + notUsedFuelLimitsSum} л.\n" +
					$"\"Нет\" - создастся лимит на {FuelDocument.FuelLimitLitersAmount} л.";

				var summarizeQuestionResult = _yesNoCancelQuestionInteractive.Question(questionMessage);

				if(summarizeQuestionResult == YesNoCancelQuestionResult.Yes)
				{
					FuelDocument.FuelLimitLitersAmount = FuelDocument.FuelLimitLitersAmount + notUsedFuelLimitsSum;
					return;
				}

				if(summarizeQuestionResult == YesNoCancelQuestionResult.No)
				{
					return;
				}

				if(summarizeQuestionResult == YesNoCancelQuestionResult.Cancel)
				{
					throw new Exception("Выдача топлива отменена пользователем!");
				}

				throw new InvalidOperationException("Неизвестный результат выбора действия");
			}
		}

		private void UpdateExistingFuelDocumentsWithNotUsedLimits(IEnumerable<FuelLimit> notUsedFuelLimits)
		{
			foreach(var limit in notUsedFuelLimits)
			{
				var fuelDocument = _fuelRepository.GetFuelDocumentByFuelLimitId(UoW, limit.LimitId);

				if(fuelDocument != null)
				{
					fuelDocument.FuelLimitLitersAmount = limit.UsedAmount ?? 0;
					fuelDocument.FuelOperation.LitersGived = fuelDocument.FuelLimitLitersAmount + fuelDocument.FuelOperation.PayedLiters;
					fuelDocument.FuelLimit.UsedAmount = limit.UsedAmount ?? 0;
					fuelDocument.FuelLimit.TransactionsOccured = limit.TransactionsOccured ?? 0;
					fuelDocument.FuelLimit.LastEditDate = limit.LastEditDate;
				}
			}
		}

		private FuelLimit CreateFuelLimitForCard(string fuelCardId)
		{
			return new FuelLimit
			{
				CardId = fuelCardId,
				ContractId = _fuelControlSettings.OrganizationContractId,
				ProductType = _fuelControlSettings.FuelProductTypeId,
				TermType = FuelLimitTermType.AllDays,
				Period = 1,
				PeriodUnit = FuelLimitPeriodUnit.OneTime,
				TransctionsCount = FuelLimitTransactionsCount
			};
		}

		private async Task<IEnumerable<FuelLimit>> GetExistingFuleLimitsFromService(string fuelCardId, CancellationToken cancellationToken)
		{
			var fuelLimits = await _fuelApiService.GetFuelLimitsByCardId(fuelCardId, cancellationToken);

			return fuelLimits;
		}

		private async Task RemoveFuelLimitsFromService(IEnumerable<string> limitIds, CancellationToken cancellationToken)
		{
			foreach(var limitId in limitIds)
			{
				await _fuelApiService.RemoveFuelLimitById(limitId, cancellationToken);
			}
		}

		private async Task<string> CreateNewFuelLimitInService(FuelLimit fuelLimit, CancellationToken cancellationToken)
		{
			var result = await _fuelApiService.SetFuelLimit(fuelLimit, cancellationToken);

			return result.First();
		}

		protected void SetRemain()
		{
			decimal litersGived = FuelDocument.FuelOperation?.LitersGived ?? default;

			decimal litersBalance = _fuelBalance + litersGived - _fuelOutlayed;

			decimal moneyToPay = -litersBalance * FuelDocument.LiterCost;

			if(FuelDocument.PayedForFuel == null && moneyToPay > 0)
			{
				FuelDocument.PayedForFuel = 0;
			}

			FuelDocument.PayedForFuel += moneyToPay;

			if(FuelDocument.PayedForFuel <= 0)
			{
				FuelDocument.PayedForFuel = null;
			}
		}

		private void FuelDocument_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(FuelDocument.FuelLimitLitersAmount))
			{
				OnPropertyChanged(nameof(ResultInfo));
				OnPropertyChanged(nameof(CashExpenseInfo));
			}

			if(e.PropertyName == nameof(FuelDocument.Date))
			{
				if(UoW.IsNew)
				{
					FuelDocument.SetFuelCardNumberByDocumentDate();
					OnPropertyChanged(nameof(FuelInfo));
				}
			}

			if(e.PropertyName == nameof(FuelDocument.FuelLimitLitersAmount))
			{
				OnPropertyChanged(nameof(IsDocumentCanBeEdited));
			}
		}

		public void UpdateInfo()
		{
			OnPropertyChanged(nameof(ResultInfo));
			OnPropertyChanged(nameof(CashExpenseInfo));
		}

		private void SetFuelDocumentTodayDateIfNeed()
		{
			if(UoW.IsNew && !IsGiveFuelInMoneySelected)
			{
				FuelDocument.Date = DateTime.Now;
			}
		}

		#region FuelInfo

		protected virtual string UpdateFuelInfo()
		{
			if(FuelDocument == null || RouteList == null)
			{
				return string.Empty;
			}

			var text = new List<string>();
			decimal fc = (decimal)RouteList.Car.FuelConsumption;

			var curTrack = _trackRepository.GetTrackByRouteListId(UoW, RouteList.Id);
			bool hasTrack = curTrack != null && curTrack.Distance.HasValue;

			if(hasTrack)
			{
				text.Add($"Расстояние по треку: {curTrack.TotalDistance:f1}({curTrack.Distance ?? 0:N1}+{curTrack.DistanceToBase ?? 0:N1}) км.");
			}

			text.Add($"Подтвержденное расстояние {RouteList.ConfirmedDistance}");

			if(RouteList.Car.FuelType != null)
			{
				var fuelOtlayedOp = RouteList.FuelOutlayedOperation;
				var entityOp = FuelDocument.FuelOperation;

				text.Add($"Вид топлива: {RouteList.Car.FuelType.Name}");

				var exclude = new List<int>();
				if(entityOp != null && entityOp.Id != 0)
				{
					exclude.Add(FuelDocument.FuelOperation.Id);
				}

				if(fuelOtlayedOp != null && fuelOtlayedOp.Id != 0)
				{
					exclude.Add(RouteList.FuelOutlayedOperation.Id);
				}

				if(exclude.Count == 0)
				{
					exclude = null;
				}

				var car = RouteList.Car;
				var carVersion = car.GetActiveCarVersionOnDate(RouteList.Date);
				var driver = RouteList.Driver;

				if(carVersion.IsCompanyCar || car.GetCurrentActiveFuelCardVersion() != null)
				{
					driver = null;
				}
				else
				{
					car = null;
				}

				_fuelBalance = _fuelRepository.GetFuelBalance(UoW, driver, car, _carEventSettings.FuelBalanceCalibrationCarEventTypeId, null, exclude?.ToArray());

				text.Add($"Остаток без документа {_fuelBalance:F2} л.");
			}
			else
			{
				text.Add("Не указан вид топлива");
			}

			_fuelOutlayed = fc / 100 * RouteList.ConfirmedDistance;

			text.Add($"Израсходовано топлива: {_fuelOutlayed:f2} л. ({fc:f2} л/100км)");
			text.Add($"Номер топливной карты: {FuelDocument.FuelCardNumber}");

			return string.Join("\n", text);
		}

		protected virtual string UpdateResutlInfo()
		{
			if(FuelDocument == null)
			{
				return string.Empty;
			}

			decimal litersGived = FuelDocument.FuelOperation?.LitersGived ?? default(decimal);

			var text = new List<string>();

			text.Add($"Итого выдано {litersGived:N2} литров");
			text.Add($"Баланс после выдачи {_fuelBalance + litersGived - _fuelOutlayed:N2}");

			return String.Join("\n", text);
		}

		protected virtual string UpdateCashExpenseInfo()
		{
			var cashExpenseInfo = string.Empty;

			if(FuelDocument.FuelCashExpense == null && !FuelDocument.PayedForFuel.HasValue)
			{
				CanOpenExpense = false;
				cashExpenseInfo = "";
			}

			if(FuelDocument.PayedForFuel.HasValue)
			{
				if(FuelDocument.FuelCashExpense != null && FuelDocument.FuelCashExpense.Id <= 0)
				{
					CanOpenExpense = false;
					cashExpenseInfo = "Расходный ордер будет создан";
				}
				if(FuelDocument.FuelCashExpense != null && FuelDocument.FuelCashExpense.Id > 0)
				{
					CanOpenExpense = true;
					cashExpenseInfo = "";
				}
			}

			return cashExpenseInfo;
		}

		#endregion FuelInfo

		#region Commands

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand SetRemainCommand { get; private set; }
		public DelegateCommand OpenExpenseCommand { get; private set; }
		public DelegateCommand SetFuelDocumentTodayDateIfNeedCommand { get; private set; }

		private void OpenExpense()
		{
			if(FuelDocument.FuelCashExpense?.Id > 0)
			{
				NavigationManager.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(FuelDocument.FuelCashExpense.Id));
			}
		}

		private void CreateCommands()
		{
			SaveCommand = new DelegateCommand(UpdateDependentDocumentsSaveAndClose, () => IsDocumentCanBeEdited);
			CancelCommand = new DelegateCommand(CancelDocumentCreation, () => true);
			SetRemainCommand = new DelegateCommand(SetRemain, () => true);
			SetFuelDocumentTodayDateIfNeedCommand = new DelegateCommand(SetFuelDocumentTodayDateIfNeed, () => true);
		}

		#endregion Commands

		public bool CanClose()
		{
			if(!IsDocumentSavingInProcess)
			{
				return true;
			}

			var message = "В данный момент выполняется запрос к сервису Газпромнефть.\n" +
				"Дождитесь заверешния операции и после этого закройте вкладку.";

			_guiDispatcher.RunInGuiTread(() => ShowWarningMessage(message));

			return false;
		}

		public override void Dispose()
		{
			if(UoW.RootObject is FuelDocument)
			{
				UoW.Dispose();
			}

			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
