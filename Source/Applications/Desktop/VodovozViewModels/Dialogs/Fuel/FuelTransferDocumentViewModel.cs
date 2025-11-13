using Autofac;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTransferDocumentViewModel : EntityTabViewModelBase<FuelTransferDocument>
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IReportInfoFactory _reportInfoFactory;
		private Employee _currentEmployee;
		private bool _sendedNow;
		private bool _receivedNow;
		private IEnumerable<Subdivision> _cashSubdivisions;
		private IEnumerable<Subdivision> _availableSubdivisionsForUser;
		private List<Subdivision> _subdivisionsFrom;
		private List<Subdivision> _subdivisionsTo;
		private bool _isUpdatingSubdivisions = false;
		private decimal _fuelBalanceCache;
		private IEnumerable<FuelType> _fuelTypes;

		public FuelTransferDocumentViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			IFuelRepository fuelRepository,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeJournalFactory employeeJournalFactory,
			IReportViewOpener reportViewOpener,
			ILifetimeScope lifetimeScope,
			IReportInfoFactory reportInfoFactory
			) : base(uoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			TabName = "Документ перемещения топлива";

			if(CurrentEmployee == null)
			{
				AbortOpening("К вашему пользователю не привязан сотрудник, невозможно открыть документ");
			}
			ConfigureEntityPropertyChanges();
			CreateCommands();

			FuelBalanceViewModel = new FuelBalanceViewModel(unitOfWorkFactory,subdivisionRepository, fuelRepository);

			UpdateCashSubdivisions();
			UpdateFuelTypes();
			UpdateBalanceCache();

			if(uoWBuilder.IsNewEntity)
			{
				Entity.CreationTime = DateTime.Now;
				Entity.Author = CurrentEmployee;
			}

			DriverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CarEntryViewModel = BuildCarEntryViewModel();
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

		public Employee CurrentEmployee
		{
			get
			{
				if(_currentEmployee == null)
				{
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return _currentEmployee;
			}
		}

		public bool CanEdit => Entity.Status == FuelTransferDocumentStatuses.New;
		public bool CanSave => (CanEdit && HasChanges) || _sendedNow || _receivedNow;
		public bool CanPrint => true;

		[PropertyChangedAlso(nameof(CanSave))]
		public bool CanSend => SendCommand.CanExecute();

		[PropertyChangedAlso(nameof(CanSave))]
		public bool CanReceive => ReceiveCommand.CanExecute();

		#endregion Properties

		#region Entries

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityEntryViewModel CarEntryViewModel { get; }

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<FuelTransferDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

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
				() =>
				{
					if(!Validate())
					{
						return;
					}
					Entity.Send(CurrentEmployee, _fuelRepository);
					_sendedNow = Entity.Status == FuelTransferDocumentStatuses.Sent;
					OnPropertyChanged(() => CanSave);
				},
				() =>
				{
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
				() =>
				{
					Entity.Receive(CurrentEmployee);
					_receivedNow = Entity.Status == FuelTransferDocumentStatuses.Received;
					OnPropertyChanged(() => CanSave);
				},
				() =>
				{
					return CurrentEmployee != null
						&& Entity.Status == FuelTransferDocumentStatuses.Sent
						&& _availableSubdivisionsForUser.Contains(Entity.CashSubdivisionTo)
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
				() =>
				{
					if((UoW.IsNew && Entity.Id == 0 || _sendedNow || _receivedNow) && (!AskQuestion("Сохранить изменения перед печатью?") || !Save()))
					{
						return;
					}

					var reportInfo = _reportInfoFactory.Create();
					reportInfo.Title = string.Format($"Документ перемещения №{Entity.Id} от {Entity.CreationTime:d}");
					reportInfo.Identifier = "Documents.FuelTransferDocument";
					reportInfo.Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } };

					_reportViewOpener.OpenReport(this, reportInfo);
				},
				() => true
			);
		}

		protected override void AfterSave()
		{
			_receivedNow = _sendedNow = false;
			base.AfterSave();
		}

		#endregion

		#region Настройка списков доступных подразделений кассы

		public virtual List<Subdivision> SubdivisionsFrom
		{
			get => _subdivisionsFrom;
			set => SetField(ref _subdivisionsFrom, value, () => SubdivisionsFrom);
		}

		public virtual List<Subdivision> SubdivisionsTo
		{
			get => _subdivisionsTo;
			set => SetField(ref _subdivisionsTo, value, () => SubdivisionsTo);
		}

		private void UpdateCashSubdivisions()
		{
			_availableSubdivisionsForUser = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser);
			_cashSubdivisions = _subdivisionRepository.GetCashSubdivisions(UoW);
			if(Entity.CashSubdivisionTo == null)
			{
				SubdivisionsFrom = new List<Subdivision>(_availableSubdivisionsForUser);
			}
			else
			{
				SubdivisionsFrom = new List<Subdivision>(_availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo));
			}
			if(Entity.CashSubdivisionFrom == null)
			{
				SubdivisionsTo = new List<Subdivision>(_cashSubdivisions);
			}
			else
			{
				SubdivisionsTo = new List<Subdivision>(_cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom));
			}
			if(!CanEdit && !SubdivisionsFrom.Contains(CashSubdivisionFrom))
			{
				SubdivisionsFrom.Add(CashSubdivisionFrom);
			}
			if(!CanEdit && !SubdivisionsTo.Contains(CashSubdivisionTo))
			{
				SubdivisionsTo.Add(CashSubdivisionTo);
			}
		}

		public virtual Subdivision CashSubdivisionFrom
		{
			get => Entity.CashSubdivisionFrom;
			set
			{
				if(CanEdit)
				{
					Entity.CashSubdivisionFrom = value;
				}
			}
		}

		public virtual Subdivision CashSubdivisionTo
		{
			get => Entity.CashSubdivisionTo;
			set
			{
				if(CanEdit)
				{
					Entity.CashSubdivisionTo = value;
				}
			}
		}


		private void UpdateSubdivisionsFrom()
		{
			if(!CanEdit || _isUpdatingSubdivisions)
			{
				return;
			}
			_isUpdatingSubdivisions = true;
			var currentSubdivisonFrom = Entity.CashSubdivisionFrom;
			SubdivisionsFrom = new List<Subdivision>(_availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo));
			if(SubdivisionsTo.Contains(currentSubdivisonFrom))
			{
				Entity.CashSubdivisionFrom = currentSubdivisonFrom;
			}
			_isUpdatingSubdivisions = false;
		}

		private void UpdateSubdivisionsTo()
		{
			if(!CanEdit || _isUpdatingSubdivisions)
			{
				return;
			}
			_isUpdatingSubdivisions = true;
			var currentSubdivisonTo = Entity.CashSubdivisionTo;
			SubdivisionsTo = new List<Subdivision>(_cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom));
			if(SubdivisionsTo.Contains(currentSubdivisonTo))
			{
				Entity.CashSubdivisionTo = currentSubdivisonTo;
			}
			_isUpdatingSubdivisions = false;
		}

		#endregion Настройка списков доступных подразделений кассы

		#region FuelBalance

		public virtual decimal FuelBalanceCache
		{
			get => _fuelBalanceCache;
			set => SetField(ref _fuelBalanceCache, value, () => FuelBalanceCache);
		}

		private void UpdateBalanceCache()
		{
			if(Entity.CashSubdivisionFrom == null || Entity.FuelType == null)
			{
				return;
			}
			FuelBalanceCache = _fuelRepository.GetFuelBalanceForSubdivision(UoW, Entity.CashSubdivisionFrom, Entity.FuelType);
			if(Entity.TransferedLiters > FuelBalanceCache && CanEdit)
			{
				Entity.TransferedLiters = FuelBalanceCache;
			}
		}

		#endregion FuelBalance

		#region FuelTypes

		public virtual IEnumerable<FuelType> FuelTypes
		{
			get => _fuelTypes;
			set => SetField(ref _fuelTypes, value, () => FuelTypes);
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
