using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.FuelDocuments
{
	public class FuelDocumentViewModel : TabViewModelBase
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly ITrackRepository _trackRepository;

		private readonly CashDistributionCommonOrganisationProvider _commonOrganisationProvider =
			new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));

		private FuelCashOrganisationDistributor _fuelCashOrganisationDistributor;

		private FuelDocument _fuelDocument;
		private Employee _cashier;
		private Track _track;
		private bool _canEdit = true;
		private bool _autoCommit;
		private bool _fuelInMoney;
		private bool _canOpenExpense;
		private decimal _fuelBalance;
		private decimal _fuelOutlayed;

		public virtual IUnitOfWork UoW { get; set; }

		protected IFuelRepository FuelRepository { get; set; }
		protected ISubdivisionRepository SubdivisionsRepository { get; }
		protected IEmployeeRepository EmployeeRepository { get; }
		protected ICommonServices CommonServices { get; }

		[PropertyChangedAlso(nameof(Balance), nameof(FuelInfo), nameof(ResultInfo))]
		public virtual FuelDocument FuelDocument
		{
			get => _fuelDocument;
			set => SetField(ref _fuelDocument, value);
		}

		public RouteList RouteList { get; set; }

		[PropertyChangedAlso(nameof(CanEdit))]
		public virtual Employee Cashier
		{
			get => _cashier;
			set => SetField(ref _cashier, value);
		}

		public virtual Track Track
		{
			get => _track;
			set => SetField(ref _track, value);
		}

		public virtual bool CanEdit
		{
			get => _canEdit;
			set
			{
				SetField(ref _canEdit, value);
				OnPropertyChanged(nameof(CanChangeDate));
			}
		}

		public virtual bool FuelInMoney
		{
			get => _fuelInMoney;
			set => SetField(ref _fuelInMoney, value);
		}

		public virtual bool CanOpenExpense
		{
			get => _canOpenExpense;
			set => SetField(ref _canOpenExpense, value);
		}

		public virtual string CashExpenseInfo => UpdateCashExpenseInfo();

		public virtual string BalanceState => $"Доступно к выдаче: {Balance} л.";

		public virtual bool IsNewEditable => FuelDocument.Id <= 0 && CanEdit;

		public virtual bool CanChangeDate =>
			CanEdit
			&& CommonServices.PermissionService.ValidateUserPresetPermission("can_change_fuel_card_number",
				CommonServices.UserService.CurrentUserId);

		public virtual decimal Balance
		{
			get
			{
				if(FuelDocument.Subdivision != null && FuelDocument.Fuel != null)
				{
					return FuelRepository?.GetFuelBalanceForSubdivision(UoW, FuelDocument.Subdivision, FuelDocument.Fuel) ?? 0m;
				}

				return 0m;
			}
		}

		public IList<Subdivision> AvailableSubdivisionsForUser
		{
			get
			{
				var user = CommonServices.UserService.GetCurrentUser(UoW);
				var employee = EmployeeRepository.GetEmployeesForUser(UoW, user.Id).FirstOrDefault();
				var subdivisions = SubdivisionsRepository.GetCashSubdivisionsAvailableForUser(UoW, user).ToList();

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
		public IEntityAutocompleteSelectorFactory CarAutocompleteSelector { get; }

		#region ctor

		/// <summary>
		/// Открывает диалог выдачи топлива, с коммитом изменений в родительском UoW
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
			ICarJournalFactory carJournalFactory) : base(commonServices?.InteractiveService, navigationManager)
		{
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			SubdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			FuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			CarAutocompleteSelector =
				(carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory)))
				.CreateCarAutocompleteSelectorFactory();

			UoW = uow;
			FuelDocument = new FuelDocument();
			FuelDocument.UoW = UoW;
			_autoCommit = false;
			RouteList = rl;

			Configure();
		}

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
			ICarJournalFactory carJournalFactory) : base(commonServices?.InteractiveService, navigationManager)
		{
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			SubdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			FuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			CarAutocompleteSelector =
				(carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory)))
				.CreateCarAutocompleteSelectorFactory();

			UoW = uow;
			FuelDocument = uow.GetById<FuelDocument>(fuelDocument.Id);
			FuelDocument.UoW = UoW;
			_autoCommit = false;
			RouteList = FuelDocument.RouteList;

			Configure();
		}

		/// <summary>
		/// Открывает диалог выдачи топлива, с автоматическим коммитом всех изменений
		/// </summary>
		public FuelDocumentViewModel
		(
			RouteList rl,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionsRepository,
			IEmployeeRepository employeeRepository,
			IFuelRepository fuelRepository,
			INavigationManager navigationManager,
			ITrackRepository trackRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			ICarJournalFactory carJournalFactory) : base(commonServices?.InteractiveService, navigationManager)
		{
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			SubdivisionsRepository = subdivisionsRepository ?? throw new ArgumentNullException(nameof(subdivisionsRepository));
			FuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			EmployeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			CarAutocompleteSelector =
				(carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory)))
				.CreateCarAutocompleteSelectorFactory();

			var uow = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			UoW = uow;
			FuelDocument = uow.Root;
			FuelDocument.UoW = UoW;
			_autoCommit = true;
			RouteList = UoW.GetById<RouteList>(rl.Id);

			Configure();
		}

		private void Configure()
		{
			if(!CarHasFuelType() || !InitActualCashier())
			{
				AbortOpening();
				return;
			}

			TabName = "Выдача топлива";
			_fuelCashOrganisationDistributor = new FuelCashOrganisationDistributor(_commonOrganisationProvider);
			CreateCommands();
			Track = _trackRepository.GetTrackByRouteListId(UoW, RouteList.Id);

			if(FuelDocument.Id == 0)
			{
				FuelDocument.FillEntity(RouteList);
			}

			FuelDocument.PropertyChanged += FuelDocument_PropertyChanged;

			OpenExpenseCommand = new DelegateCommand(OpenExpense);
		}

		#endregion ctor

		private bool InitActualCashier()
		{
			Cashier = EmployeeRepository.GetEmployeeForCurrentUser(UoW);

			if(Cashier == null)
			{
				ShowWarningMessage("Ваш пользователь не привязан к действующему сотруднику, Вы не можете выдавать денежные средства и топливо, так как некого указывать в качестве кассира.");
				return false;
			}

			var cashSubdivisions = SubdivisionsRepository?.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			if(!cashSubdivisions?.Contains(Cashier.Subdivision) ?? true)
			{
				ShowWarningMessage("Выдать топливо может только сотрудник кассы");
				return false;
			}

			return true;
		}

		private bool CarHasFuelType()
		{
			if(RouteList.Car.FuelType == null)
			{
				ShowWarningMessage($"У машины {RouteList.Car.CarModel.Name} {RouteList.Car.Title} отсутствует тип топлива");
				return false;
			}

			return true;
		}

		public bool SaveDocument()
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

			var valid = CommonServices.ValidationService.Validate(FuelDocument, new ValidationContext(FuelDocument));

			if(!valid)
			{
				return false;
			}

			if(FuelDocument.Id == 0)
			{
				FuelDocument.CreateOperations(FuelRepository, _commonOrganisationProvider, _financialCategoriesGroupsSettings);
				RouteList.ObservableFuelDocuments.Add(FuelDocument);

				if(FuelInMoney && FuelDocument.FuelPaymentType == FuelPaymentType.Cash)
				{
					_fuelCashOrganisationDistributor.DistributeCash(UoW, FuelDocument);
				}
			}
			else
			{
				FuelDocument.UpdateFuelOperation();
			}

			_logger.Info("Сохраняем топливный документ...");

			if(_autoCommit)
			{
				UoW.Save();
			}
			else
			{
				UoW.Save(FuelDocument);
			}

			return true;
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
			if(e.PropertyName == nameof(FuelDocument.FuelCoupons))
			{
				OnPropertyChanged(nameof(ResultInfo));
				OnPropertyChanged(nameof(CashExpenseInfo));
			}

			if(e.PropertyName == nameof(FuelDocument.Subdivision) || e.PropertyName == nameof(FuelDocument.Fuel))
			{
				OnPropertyChanged(nameof(Balance));
				OnPropertyChanged(nameof(BalanceState));
			}
		}

		public void UpdateInfo()
		{
			OnPropertyChanged(nameof(ResultInfo));
			OnPropertyChanged(nameof(CashExpenseInfo));
			OnPropertyChanged(nameof(Balance));
			OnPropertyChanged(nameof(BalanceState));
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

				if(carVersion.IsCompanyCar)
				{
					driver = null;
				}
				else
				{
					car = null;
				}

				_fuelBalance = FuelRepository.GetFuelBalance(UoW, driver, car, null, exclude?.ToArray());

				text.Add($"Остаток без документа {_fuelBalance:F2} л.");
			}
			else
			{
				text.Add("Не указан вид топлива");
			}

			_fuelOutlayed = fc / 100 * RouteList.ConfirmedDistance;

			text.Add($"Израсходовано топлива: {_fuelOutlayed:f2} л. ({fc:f2} л/100км)");
			text.Add($"Номер топливной карты: {FuelDocument.FuelCardNumber}");

			return String.Join("\n", text);
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

		private void OpenExpense()
		{
			if(FuelDocument.FuelCashExpense?.Id > 0)
			{
				NavigationManager.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(FuelDocument.FuelCashExpense.Id));
			}
		}

		private void CreateCommands()
		{
			CreateSaveCommand();
			CreateCancelCommand();
			CreateSetRemainCommand();
		}

		private void CreateSetRemainCommand()
		{
			SetRemainCommand = new DelegateCommand(SetRemain, () => true);
		}

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(() => { Close(true, CloseSource.Cancel); }, () => true);
		}

		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand(
				() =>
				{
					if(SaveDocument())
					{
						Close(false, CloseSource.Save);
					}
				},
				() => CanEdit
			);

			SaveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		#endregion Commands

		public override void Dispose()
		{
			if(UoW.RootObject is FuelDocument)
			{
				UoW.Dispose();
			}

			base.Dispose();
		}
	}
}
