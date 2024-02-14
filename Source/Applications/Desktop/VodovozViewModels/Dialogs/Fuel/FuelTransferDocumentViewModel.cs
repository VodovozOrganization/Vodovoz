using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using QS.ViewModels;
using QS.Commands;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using Vodovoz.EntityRepositories.Fuel;
using QS.Project.Domain;
using QS.Services;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Services;
using Autofac;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTransferDocumentViewModel : EntityTabViewModelBase<FuelTransferDocument>
	{
		private readonly IEmployeeService employeeService;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IFuelRepository fuelRepository;
		private readonly IEmployeeJournalFactory employeeJournalFactory;
		private readonly ICarJournalFactory carJournalFactory;
		private readonly IReportViewOpener reportViewOpener;
		private readonly ILifetimeScope _lifetimeScope;

		public FuelTransferDocumentViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			IFuelRepository fuelRepository,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ICarJournalFactory carJournalFactory,
			IReportViewOpener reportViewOpener,
			ILifetimeScope lifetimeScope
			) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			this.employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			this.carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			this.reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			TabName = "Документ перемещения топлива";

			if(CurrentEmployee == null) {
				AbortOpening("К вашему пользователю не привязан сотрудник, невозможно открыть документ");
			}
			ConfigureEntityPropertyChanges();
			CreateCommands();

			FuelBalanceViewModel = new FuelBalanceViewModel(unitOfWorkFactory,subdivisionRepository, fuelRepository);

			UpdateCashSubdivisions();
			UpdateFuelTypes();
			UpdateBalanceCache();

			if(uoWBuilder.IsNewEntity) {
				Entity.CreationTime = DateTime.Now;
				Entity.Author = CurrentEmployee;
			}

			ConfigureEntries();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEdit
			);

			SetPropertyChangeRelation(e => e.CashSubdivisionFrom,
				() => CashSubdivisionFrom
			);

			SetPropertyChangeRelation(e => e.CashSubdivisionTo,
				() => CashSubdivisionTo
			);

			OnEntityPropertyChanged(UpdateSubdivisionsTo, e => e.CashSubdivisionFrom);
			OnEntityPropertyChanged(UpdateSubdivisionsFrom, e => e.CashSubdivisionTo);
			OnEntityPropertyChanged(UpdateBalanceCache,
				e => e.CashSubdivisionFrom,
				e => e.FuelType
			);

			OnEntityAnyPropertyChanged(() => { OnPropertyChanged(() => CanSave); });
		}

		private void ConfigureExternalUpdateSubscribes()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<FuelType>((changeEvent) => UpdateFuelTypes());
			NotifyConfiguration.Instance.BatchSubscribeOnEntity((changeEvent) => UpdateBalanceCache(),
				typeof(FuelTransferDocument),
				typeof(FuelWriteoffDocument),
				typeof(FuelWriteoffDocumentItem),
				typeof(FuelIncomeInvoice),
				typeof(FuelIncomeInvoiceItem)
			);

		}

		#region Properties

		public FuelBalanceViewModel FuelBalanceViewModel { get; }

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		public bool CanEdit => Entity.Status == FuelTransferDocumentStatuses.New;
		public bool CanSave => (CanEdit && HasChanges) || sendedNow || receivedNow;
		public bool CanPrint => true;

		private bool sendedNow;
		[PropertyChangedAlso(nameof(CanSave))]
		public bool CanSend => SendCommand.CanExecute();

		private bool receivedNow;
		[PropertyChangedAlso(nameof(CanSave))]
		public bool CanReceive => ReceiveCommand.CanExecute();

		#endregion Properties

		#region Entries

		private void ConfigureEntries()
		{
			DriverSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory(_lifetimeScope);
		}
		
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory;
		public IEntityAutocompleteSelectorFactory CarSelectorFactory;

		#endregion Entries

		#region Commands

		public DelegateCommand SendCommand { get; private set; }
		public DelegateCommand ReceiveCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		private void CreateCommands()
		{
			CreateSendCommand();
			CreateReceiveCommand();
			CreatePrintCommand();
		}

		private void CreateSendCommand()
		{
			SendCommand = new DelegateCommand(
				() => {
					if(!Validate()) {
						return;
					}
					Entity.Send(CurrentEmployee, fuelRepository);
					sendedNow = Entity.Status == FuelTransferDocumentStatuses.Sent;
					OnPropertyChanged(() => CanSave);
				},
				() => {
					return CurrentEmployee != null
						&& Entity.Status == FuelTransferDocumentStatuses.New
						&& Entity.Driver != null
						&& Entity.Car != null
						&& Entity.CashSubdivisionFrom != null
						&& Entity.CashSubdivisionTo != null
						&& Entity.TransferedLiters > 0;
				}
			);
			SendCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Driver,
				x => x.Car,
				x => x.CashSubdivisionFrom,
				x => x.CashSubdivisionTo,
				x => x.TransferedLiters
			);
			SendCommand.CanExecuteChanged += (sender, e) => { OnPropertyChanged(() => CanSend); };
		}

		private void CreateReceiveCommand()
		{
			ReceiveCommand = new DelegateCommand(
				() => {
					Entity.Receive(CurrentEmployee);
					receivedNow = Entity.Status == FuelTransferDocumentStatuses.Received;
					OnPropertyChanged(() => CanSave);
				},
				() => {
					return CurrentEmployee != null
						&& Entity.Status == FuelTransferDocumentStatuses.Sent
						&& availableSubdivisionsForUser.Contains(Entity.CashSubdivisionTo)
						&& Entity.Id != 0;
				}
			);
			ReceiveCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Id
			);
			ReceiveCommand.CanExecuteChanged += (sender, e) => { OnPropertyChanged(() => CanReceive); };
		}

		private void CreatePrintCommand()
		{
			PrintCommand = new DelegateCommand(
				() => {
					if((UoW.IsNew && Entity.Id == 0 || sendedNow || receivedNow) && (!AskQuestion("Сохранить изменения перед печатью?") || !Save()))
						return;

					var reportInfo = new QS.Report.ReportInfo {
						Title = string.Format($"Документ перемещения №{Entity.Id} от {Entity.CreationTime:d}"),
						Identifier = "Documents.FuelTransferDocument",
						Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } }
					};

					reportViewOpener.OpenReport(this, reportInfo);
				},
				() => true
			);
		}

		protected override void AfterSave()
		{
			receivedNow = sendedNow = false;
			base.AfterSave();
		}

		#endregion

		#region Настройка списков доступных подразделений кассы

		private IEnumerable<Subdivision> cashSubdivisions;
		private IEnumerable<Subdivision> availableSubdivisionsForUser;

		private List<Subdivision> subdivisionsFrom;
		public virtual List<Subdivision> SubdivisionsFrom {
			get => subdivisionsFrom;
			set => SetField(ref subdivisionsFrom, value, () => SubdivisionsFrom);
		}

		private List<Subdivision> subdivisionsTo;
		public virtual List<Subdivision> SubdivisionsTo {
			get => subdivisionsTo;
			set => SetField(ref subdivisionsTo, value, () => SubdivisionsTo);
		}

		private void UpdateCashSubdivisions()
		{
			availableSubdivisionsForUser = subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser);
			cashSubdivisions = subdivisionRepository.GetCashSubdivisions(UoW);
			if(Entity.CashSubdivisionTo == null) {
				SubdivisionsFrom = new List<Subdivision>(availableSubdivisionsForUser);
			} else {
				SubdivisionsFrom = new List<Subdivision>(availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo));
			}
			if(Entity.CashSubdivisionFrom == null) {
				SubdivisionsTo = new List<Subdivision>(cashSubdivisions);
			} else {
				SubdivisionsTo = new List<Subdivision>(cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom));
			}
			if(!CanEdit && !SubdivisionsFrom.Contains(CashSubdivisionFrom)) {
				SubdivisionsFrom.Add(CashSubdivisionFrom);
			}
			if(!CanEdit && !SubdivisionsTo.Contains(CashSubdivisionTo)) {
				SubdivisionsTo.Add(CashSubdivisionTo);
			}
		}


		private bool isUpdatingSubdivisions = false;



		public virtual Subdivision CashSubdivisionFrom {
			get => Entity.CashSubdivisionFrom;
			set {
				if(CanEdit) {
					Entity.CashSubdivisionFrom = value;
				}
			}
		}

		public virtual Subdivision CashSubdivisionTo {
			get => Entity.CashSubdivisionTo;
			set {
				if(CanEdit) {
					Entity.CashSubdivisionTo = value;
				}
			}
		}


		private void UpdateSubdivisionsFrom()
		{
			if(!CanEdit || isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonFrom = Entity.CashSubdivisionFrom;
			SubdivisionsFrom = new List<Subdivision>(availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo));
			if(SubdivisionsTo.Contains(currentSubdivisonFrom)) {
				Entity.CashSubdivisionFrom = currentSubdivisonFrom;
			}
			isUpdatingSubdivisions = false;
		}

		private void UpdateSubdivisionsTo()
		{
			if(!CanEdit || isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonTo = Entity.CashSubdivisionTo;
			SubdivisionsTo = new List<Subdivision>(cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom));
			if(SubdivisionsTo.Contains(currentSubdivisonTo)) {
				Entity.CashSubdivisionTo = currentSubdivisonTo;
			}
			isUpdatingSubdivisions = false;
		}

		#endregion Настройка списков доступных подразделений кассы

		#region FuelBalance

		private decimal fuelBalanceCache;
		public virtual decimal FuelBalanceCache {
			get => fuelBalanceCache;
			set => SetField(ref fuelBalanceCache, value, () => FuelBalanceCache);
		}

		private void UpdateBalanceCache()
		{
			if(Entity.CashSubdivisionFrom == null || Entity.FuelType == null) {
				return;
			}
			FuelBalanceCache = fuelRepository.GetFuelBalanceForSubdivision(UoW, Entity.CashSubdivisionFrom, Entity.FuelType);
			if(Entity.TransferedLiters > FuelBalanceCache && CanEdit) {
				Entity.TransferedLiters = FuelBalanceCache;
			}
		}

		#endregion FuelBalance

		#region FuelTypes

		private IEnumerable<FuelType> fuelTypes;
		public virtual IEnumerable<FuelType> FuelTypes {
			get => fuelTypes;
			set => SetField(ref fuelTypes, value, () => FuelTypes);
		}

		private void UpdateFuelTypes()
		{
			FuelTypes = UoW.GetAll<FuelType>();
		}

		#endregion FuelTypes

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
