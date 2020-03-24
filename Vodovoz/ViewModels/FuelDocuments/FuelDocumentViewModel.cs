using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repository.Logistics;
using Vodovoz.ViewModel;
using QS.Navigation;

namespace Vodovoz.ViewModels.FuelDocuments
{
	public class FuelDocumentViewModel : TabViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual IUnitOfWork UoW { get; set; }

		protected IFuelRepository fuelRepository { get; set; }
		protected ISubdivisionRepository subdivisionsRepository { get; }
		protected IEmployeeRepository employeeRepository { get; }
		protected ICommonServices commonServices { get; }

		private FuelDocument fuelDocument;
		[PropertyChangedAlso(nameof(Balance), nameof(FuelInfo), nameof(ResultInfo))]
		public virtual FuelDocument FuelDocument {
			get => fuelDocument;
			set => SetField(ref fuelDocument, value);
		}

		public RouteList RouteList { get; set; }

		private Employee cashier;
		[PropertyChangedAlso(nameof(CanEdit))]
		public virtual Employee Cashier {
			get => cashier;
			set => SetField(ref cashier, value);
		}

		private Track track;
		public virtual Track Track {
			get => track;
			set => SetField(ref track, value);
		}

		private bool canEdit = true;
		public virtual bool CanEdit {
			get => canEdit;
			set { SetField(ref canEdit, value);
				OnPropertyChanged(nameof(CanChangeDate));
			}
		}

		private bool autoCommit;
		public virtual bool AutoCommit {
			get => autoCommit;
			set => SetField(ref autoCommit, value);
		}

		private bool fuelInMoney;
		public virtual bool FuelInMoney {
			get => fuelInMoney;
			set => SetField(ref fuelInMoney, value);
		}

		private bool canOpenExpense;
		public virtual bool CanOpenExpense {
			get => canOpenExpense;
			set => SetField(ref canOpenExpense, value);
		}

		public bool CanEditFuelPrice { get { return FuelInMoney && CanEdit; } }

		public virtual string CashExpenseInfo { get { return UpdateCashExpenseInfo(); } }

		public virtual string BalanceState { get { return $"Доступно к выдаче: {Balance} л."; } }

		public virtual bool IsNewEditable { get { return FuelDocument.Id <= 0 && CanEdit; } }

		public virtual bool CanChangeDate { get => CanEdit &&
		commonServices.PermissionService.ValidateUserPresetPermission("can_change_fuel_card_number", commonServices.UserService.CurrentUserId);
		} 

		public virtual decimal Balance {
			get {
				if(FuelDocument.Subdivision != null && FuelDocument.Fuel != null)
					return fuelRepository?.GetFuelBalanceForSubdivision(UoW, FuelDocument.Subdivision, FuelDocument.Fuel) ?? 0m;
				return 0m;
			}
		}

		public virtual decimal NeedLiters { get { return fuelOutlayed - fuelBalance; } }

		private EmployeesVM employeeJournal;
		public virtual EmployeesVM EmployeeJournal {
			get 
			{
				if(employeeJournal != null)
					return employeeJournal;

				var filterDriver = new EmployeeFilterViewModel();
				filterDriver.SetAndRefilterAtOnce(
					x => x.RestrictCategory = EmployeeCategory.driver,
					x => x.Status = EmployeeStatus.IsWorking
				);
				employeeJournal = new EmployeesVM(filterDriver);

				return employeeJournal;
			}
			set => SetField(ref employeeJournal, value);
		}

		public IList<Subdivision> AvailableSubdivisionsForUser 
		{ 
			get 
				{
					var user = commonServices.UserService.GetCurrentUser(UoW);
					var employee = employeeRepository.GetEmployeesForUser(UoW, user.Id).FirstOrDefault();
					var subdivisions = subdivisionsRepository.GetCashSubdivisionsAvailableForUser(UoW, user).ToList();
					
					if(subdivisions.Any(x => x.Id == employee.Subdivision.Id))
						FuelDocument.Subdivision = employee.Subdivision;

					return subdivisions;
				} 
		}

		public string FuelInfo { get { return UpdateFuelInfo(); } set { } }

		public string ResultInfo { get { return UpdateResutlInfo(); } }

		decimal fuelBalance;
		decimal fuelOutlayed;

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
			INavigationManager navigationManager
		) : base(commonServices.InteractiveService, navigationManager)
		{
			this.commonServices = commonServices ?? throw new NotImplementedException(nameof(commonServices));
			this.subdivisionsRepository = subdivisionsRepository ?? throw new NotImplementedException(nameof(subdivisionsRepository));
			this.fuelRepository = fuelRepository ?? throw new NotImplementedException(nameof(fuelRepository));
			this.employeeRepository = employeeRepository ?? throw new NotImplementedException(nameof(employeeRepository));

			UoW = uow;
			FuelDocument = new FuelDocument();
			FuelDocument.UoW = UoW;
			autoCommit = false;
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
			INavigationManager navigationManager
		) : base(commonServices.InteractiveService, navigationManager)
		{
			this.commonServices = commonServices ?? throw new NotImplementedException(nameof(commonServices));
			this.subdivisionsRepository = subdivisionsRepository ?? throw new NotImplementedException(nameof(subdivisionsRepository));
			this.fuelRepository = fuelRepository ?? throw new NotImplementedException(nameof(fuelRepository));
			this.employeeRepository = employeeRepository ?? throw new NotImplementedException(nameof(employeeRepository));

			UoW = uow;
			FuelDocument = uow.GetById<FuelDocument>(fuelDocument.Id);
			FuelDocument.UoW = UoW;
			autoCommit = false;
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
			INavigationManager navigationManager
		) : base(commonServices.InteractiveService, navigationManager)
		{
			this.commonServices = commonServices ?? throw new NotImplementedException(nameof(commonServices));
			this.subdivisionsRepository = subdivisionsRepository ?? throw new NotImplementedException(nameof(subdivisionsRepository));
			this.fuelRepository = fuelRepository ?? throw new NotImplementedException(nameof(fuelRepository));
			this.employeeRepository = employeeRepository ?? throw new NotImplementedException(nameof(employeeRepository));

			var uow = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			UoW = uow;
			FuelDocument = uow.Root;
			FuelDocument.UoW = UoW;
			autoCommit = true;
			RouteList = UoW.GetById<RouteList>(rl.Id);

			Configure();
		}

		private void Configure()
		{
			if(!CarHasFuelType() || !InitActualCashier()) {
				AbortOpening();
				return;
			}

			TabName = "Выдача топлива";
			CreateCommands();
			Track = TrackRepository.GetTrackForRouteList(UoW, RouteList.Id);
			if(FuelDocument.Id == 0)
				FuelDocument.FillEntity(RouteList);
			FuelDocument.PropertyChanged += FuelDocument_PropertyChanged;
		}

		#endregion ctor

		private bool InitActualCashier()
		{
			Cashier = employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(Cashier == null) {
				ShowWarningMessage("Ваш пользователь не привязан к действующему сотруднику, Вы не можете выдавать денежные средства и топливо, так как некого указывать в качестве кассира.");
				return false;
			}

			var cashSubdivisions = subdivisionsRepository?.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			if(!cashSubdivisions?.Contains(Cashier.Subdivision) ?? true) {
				ShowWarningMessage("Выдать топливо может только сотрудник кассы");
				return false;
			}

			return true;
		}

		private bool CarHasFuelType()
		{
			if(RouteList.Car.FuelType == null) {
				ShowWarningMessage($"У машины {RouteList.Car.Model} {RouteList.Car.Title} отсутствует тип топлива");
				return false;
			}

			return true;
		}

		public bool SaveDocument()
		{
			if(FuelDocument.Author == null) {
				FuelDocument.Author = cashier;
			}

			FuelDocument.LastEditor = cashier;

			FuelDocument.LastEditDate = DateTime.Now;

			if(FuelDocument.FuelCashExpense != null) {
				FuelDocument.FuelCashExpense.Casher = cashier;
			}

			var valid = commonServices.ValidationService.Validate(FuelDocument, new ValidationContext(FuelDocument));

			if(!valid)
				return false;
			if(FuelDocument.Id == 0) 
			{
				FuelDocument.CreateOperations(fuelRepository);
				RouteList.ObservableFuelDocuments.Add(FuelDocument);
			} 
			else 
			{
				FuelDocument.UpdateFuelOperation(fuelRepository);
			}

			logger.Info("Сохраняем топливный документ...");

			if(autoCommit)
				UoW.Save();
			else 
				UoW.Save(FuelDocument);

			return true;
		}

		protected void SetRemain()
		{
			decimal litersBalance = 0;
			decimal litersGived = FuelDocument.FuelOperation?.LitersGived ?? default(decimal);

			litersBalance = fuelBalance + litersGived - fuelOutlayed;

			decimal moneyToPay = -litersBalance * FuelDocument.LiterCost;

			if(FuelDocument.PayedForFuel == null && moneyToPay > 0) 
				FuelDocument.PayedForFuel = 0;

			FuelDocument.PayedForFuel += moneyToPay;

			if(FuelDocument.PayedForFuel <= 0) 
				FuelDocument.PayedForFuel = null;
			
		}

		void FuelDocument_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
				return string.Empty;

			var text = new List<string>();
			decimal fc = (decimal)RouteList.Car.FuelConsumption;

			var track = TrackRepository.GetTrackForRouteList(UoW, RouteList.Id);
			bool hasTrack = track != null && track.Distance.HasValue;

			if(hasTrack)
				text.Add($"Расстояние по треку: {track.TotalDistance:f1}({track.Distance ?? 0:N1}+{track.DistanceToBase ?? 0:N1}) км.");

			text.Add($"Подтвержденное расстояние {RouteList.ConfirmedDistance}");

			if(RouteList.Car.FuelType != null) 
			{
				var fuelOtlayedOp = RouteList.FuelOutlayedOperation;
				var entityOp = FuelDocument.FuelOperation;

				text.Add($"Вид топлива: {RouteList.Car.FuelType.Name}");

				var exclude = new List<int>();
				if(entityOp != null && entityOp.Id != 0)
					exclude.Add(FuelDocument.FuelOperation.Id);

				if(fuelOtlayedOp != null && fuelOtlayedOp.Id != 0)
					exclude.Add(RouteList.FuelOutlayedOperation.Id);

				if(exclude.Count == 0) 
					exclude = null;

				Car car = RouteList.Car;
				Employee driver = RouteList.Driver;

				if(car.IsCompanyCar) 
					driver = null;
				else 
					car = null;

				fuelBalance = Repository.Operations.FuelRepository.GetFuelBalance(
					UoW, driver, car, RouteList.Car.FuelType, null, exclude?.ToArray());

				text.Add($"Остаток без документа {fuelBalance:F2} л.");
			} else {
				text.Add("Не указан вид топлива");
			}

			fuelOutlayed = fc / 100 * RouteList.ConfirmedDistance;

			text.Add($"Израсходовано топлива: {fuelOutlayed:f2} л. ({fc:f2} л/100км)");
			text.Add($"Номер топливной карты: {FuelDocument.FuelCardNumber}");

			return String.Join("\n", text);
		}

		protected virtual string UpdateResutlInfo () 
		{
			if(FuelDocument == null)
				return string.Empty;

			decimal litersGived = FuelDocument.FuelOperation?.LitersGived ?? default(decimal);

			var text = new List<string>();

			text.Add($"Итого выдано {litersGived:N2} литров");
			text.Add($"Баланс после выдачи {fuelBalance + litersGived - fuelOutlayed:N2}");

			return String.Join("\n", text);
		}

		protected virtual string UpdateCashExpenseInfo()
		{
			string cashExpenseInfo = string.Empty;
			if(FuelDocument.FuelCashExpense == null && !FuelDocument.PayedForFuel.HasValue) {
				CanOpenExpense = false;
				cashExpenseInfo = "";
			}
			if(FuelDocument.PayedForFuel.HasValue) {
				if(FuelDocument.FuelCashExpense != null && FuelDocument.FuelCashExpense.Id <= 0) {
					CanOpenExpense = false;
					cashExpenseInfo = "Расходный ордер будет создан";
				}
				if(FuelDocument.FuelCashExpense != null && FuelDocument.FuelCashExpense.Id > 0) {
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
		public DelegateCommand SetRemainCommand { get; private set;}

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
			CancelCommand = new DelegateCommand(() => { Close(false); }, () => true);
		}

		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand(
				() => 
				{ 
					if(SaveDocument())
						Close(false);
				},
				() => CanEdit
			);

			SaveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		#endregion Commands

		public override void Dispose()
		{
			if(UoW.RootObject is FuelDocument)
				UoW.Dispose();
			base.Dispose();
		}
	}
}
