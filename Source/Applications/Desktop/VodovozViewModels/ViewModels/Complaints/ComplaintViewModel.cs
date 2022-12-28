using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintViewModel : EntityTabViewModelBase<Complaint>, IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;
		private DelegateCommand _changeDeliveryPointCommand;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IComplaintResultsRepository _complaintResultsRepository;
		private readonly IUserRepository _userRepository;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFileDialogService _fileDialogService;

		private readonly bool _canAddGuiltyInComplaintsPermissionResult;
		private readonly bool _canCloseComplaintsPermissionResult;

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public ComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory driverJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelector,
			IEmployeeSettings employeeSettings,
			IComplaintResultsRepository complaintResultsRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider) : base(uowBuilder, uowFactory, commonServices)
		{
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_complaintResultsRepository = complaintResultsRepository ?? throw new ArgumentNullException(nameof(complaintResultsRepository));
			NomenclatureSelector = nomenclatureSelector ?? throw new ArgumentNullException(nameof(nomenclatureSelector));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			EmployeeJournalFactory = driverJournalFactory ?? throw new ArgumentNullException(nameof(driverJournalFactory));
			_employeeSelectorFactory = EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			SubdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			
			if(orderSelectorFactory == null)
			{
				throw new ArgumentNullException(nameof(orderSelectorFactory));
			}

			Entity.ObservableComplaintDiscussions.ElementChanged += ObservableComplaintDiscussions_ElementChanged;
			Entity.ObservableComplaintDiscussions.ListContentChanged += ObservableComplaintDiscussions_ListContentChanged;
			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;

			if(uowBuilder.IsNewEntity) {
				AbortOpening("Невозможно создать новую рекламацию из текущего диалога, необходимо использовать диалоги создания");
			}

			if(CurrentEmployee == null) {
				AbortOpening("Невозможно открыть рекламацию так как к вашему пользователю не привязан сотрудник");
			}

			ConfigureEntityChangingRelations();

			CreateCommands();

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			ComplaintObject = Entity.ComplaintKind?.ComplaintObject;

			TabName = $"Рекламация №{Entity.Id} от {Entity.CreationDate.ToShortDateString()}";

			_canAddGuiltyInComplaintsPermissionResult = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_guilty_in_complaints");
			_canCloseComplaintsPermissionResult =  CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_complaints");

			if(Entity.ComplaintResultOfEmployees != null && Entity.ComplaintResultOfEmployees.IsArchive)
			{
				ComplaintResultsOfEmployees =
					_complaintResultsRepository.GetActiveResultsOfEmployeesWithSelectedResult(UoW, Entity.ComplaintResultOfEmployees.Id);
			}
			else
			{
				ComplaintResultsOfEmployees = _complaintResultsRepository.GetActiveResultsOfEmployees(UoW);
			}
			
			InitializeOrderAutocompleteSelectorFactory(orderSelectorFactory);
		}

		private void InitializeOrderAutocompleteSelectorFactory(IOrderSelectorFactory orderSelectorFactory)
		{
			var orderFilter =
				new OrderJournalFilterViewModel(CounterpartyJournalFactory, DeliveryPointJournalFactory, EmployeeJournalFactory);
			
			if(Entity.Counterparty != null)
			{
				orderFilter.RestrictCounterparty = Entity.Counterparty;
			}
			
			OrderAutocompleteSelectorFactory =
				(orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory)))
				.CreateOrderAutocompleteSelectorFactory(orderFilter);
		}

		public bool AskSaveOnClose => CanEdit;

		protected void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.ComplaintType,
				() => IsInnerComplaint,
				() => IsClientComplaint
			);

			SetPropertyChangeRelation(e => e.Status,
				() => Status
			);

			SetPropertyChangeRelation(
				e => e.ChangedBy,
				() => ChangedByAndDate
			);

			SetPropertyChangeRelation(
				e => e.ChangedDate,
				() => ChangedByAndDate
			);

			SetPropertyChangeRelation(
				e => e.CreatedBy,
				() => CreatedByAndDate
			);

			SetPropertyChangeRelation(
				e => e.CreationDate,
				() => CreatedByAndDate
			);

			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => CanSelectDeliveryPoint
			);
		}

		private void ObservableComplaintDiscussions_ElementChanged(object aList, int[] aIdx)
		{
			OnDiscussionsChanged();
		}

		private void ObservableComplaintDiscussions_ListContentChanged(object sender, EventArgs e)
		{
			OnDiscussionsChanged();
		}

		private void OnDiscussionsChanged()
		{
			OnPropertyChanged(() => SubdivisionsInWork);
			Entity.UpdateComplaintStatus();
		}

		private void ObservableFines_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(() => FineItems);
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		public virtual ComplaintStatuses Status
		{
			get => Entity.Status;
			set => Entity.SetStatus(value);
		}

		private ComplaintDiscussionsViewModel discussionsViewModel;
		public ComplaintDiscussionsViewModel DiscussionsViewModel {
			get {
				if(discussionsViewModel == null) {
					discussionsViewModel = new ComplaintDiscussionsViewModel(
						Entity,
						this,
						UoW,
						_fileDialogService,
						_employeeService,
						CommonServices,
						_employeeSelectorFactory,
						_salesPlanJournalFactory,
						NomenclatureSelector,
						_userRepository
					);
				}
				return discussionsViewModel;
			}
		}

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel
		{
			get
			{
				if(guiltyItemsViewModel == null)
				{
					guiltyItemsViewModel =
						new GuiltyItemsViewModel(Entity, UoW, CommonServices, _subdivisionRepository, _employeeSelectorFactory, SubdivisionParametersProvider);
				}

				return guiltyItemsViewModel;
			}
		}


		private ComplaintFilesViewModel filesViewModel;
		public ComplaintFilesViewModel FilesViewModel
		{
			get
			{
				if(filesViewModel == null)
				{
					filesViewModel = new ComplaintFilesViewModel(Entity, UoW, _fileDialogService, CommonServices, _userRepository);
				}
				return filesViewModel;
			}
		}

		public string SubdivisionsInWork {
			get {
				string inWork = string.Join(", ", Entity.ComplaintDiscussions
					.Where(x => x.Status == ComplaintDiscussionStatuses.InProcess)
					.Where(x => !string.IsNullOrWhiteSpace(x.Subdivision?.ShortName))
					.Select(x => x.Subdivision.ShortName));
				string okk = (!Entity.ComplaintDiscussions.Any(x => x.Status == ComplaintDiscussionStatuses.InProcess)
							&& Status != ComplaintStatuses.Closed) ? "OKK" : null;
				string result;
				if(!string.IsNullOrWhiteSpace(inWork) && !string.IsNullOrWhiteSpace(okk)) {
					result = string.Join(", ", inWork, okk);
				} else if(!string.IsNullOrWhiteSpace(inWork)) {
					result = inWork;
				} else if(!string.IsNullOrWhiteSpace(okk)) {
					result = okk;
				} else {
					return string.Empty;
				}
				return $"В работе у отд. {result}";
			}
		}

		public string ChangedByAndDate => string.Format("Изм: {0} {1}", Entity.ChangedBy?.ShortName, Entity.ChangedDate.ToString("g"));
		public string CreatedByAndDate => string.Format("Созд: {0} {1}", Entity.CreatedBy?.ShortName, Entity.CreationDate.ToString("g"));

		private List<ComplaintSource> complaintSources;
		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(complaintSources == null) {
					complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return complaintSources;
			}
		}

		private IEnumerable<ComplaintResultOfCounterparty> _complaintResults;
		public IEnumerable<ComplaintResultOfCounterparty> ComplaintResultsOfCounterparty
		{
			get
			{
				if(_complaintResults == null)
				{
					if(Entity.ComplaintResultOfCounterparty != null && Entity.ComplaintResultOfCounterparty.IsArchive)
					{
						_complaintResults =
							_complaintResultsRepository.GetActiveResultsOfCounterpartyWithSelectedResult(
								UoW, Entity.ComplaintResultOfCounterparty.Id);
					}
					else
					{
						_complaintResults = _complaintResultsRepository.GetActiveResultsOfCounterparty(UoW);
					}
				}
				return _complaintResults;
			}
		}

		public IEnumerable<ComplaintResultOfEmployees> ComplaintResultsOfEmployees { get; }

		private IList<ComplaintKind> complaintKindSource;

		public IList<ComplaintKind> ComplaintKindSource {
			get {
				if(Entity.ComplaintKind != null && Entity.ComplaintKind.IsArchive)
					complaintKindSource.Add(UoW.GetById<ComplaintKind>(Entity.ComplaintKind.Id));

				return complaintKindSource;
			}
			set
			{
				SetField(ref complaintKindSource, value);
			}
		}

		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(SetField(ref _complaintObject, value))
				{
					ComplaintKindSource = value == null ? _complaintKinds : _complaintKinds.Where(x => x.ComplaintObject == value).ToList();
				}
			}
		}

		public IEnumerable<ComplaintObject> ComplaintObjectSource =>
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().Where(x => !x.IsArchive).ToList());

		public IList<FineItem> FineItems => Entity.Fines.SelectMany(x => x.Items).OrderByDescending(x => x.Id).ToList();

		public bool IsInnerComplaint => Entity.ComplaintType == ComplaintType.Inner;
		public bool IsClientComplaint => Entity.ComplaintType == ComplaintType.Client;
		[PropertyChangedAlso(nameof(CanAddFine), nameof(CanAttachFine), nameof(CanSelectDeliveryPoint),
			nameof(CanAddGuilty), nameof(CanClose))]
		public bool CanEdit => PermissionResult.CanUpdate;
		public bool CanAddGuilty => CanEdit && _canAddGuiltyInComplaintsPermissionResult;
		public bool CanClose => CanEdit && _canCloseComplaintsPermissionResult;
		public bool CanSelectDeliveryPoint => CanEdit && Entity.Counterparty != null;
		public bool CanAddFine => CanEdit;
		public bool CanAttachFine => CanEdit;

		#region Commands

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
		}

		#region AttachFineCommand

		public DelegateCommand AttachFineCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			AttachFineCommand = new DelegateCommand(
				() => {
					var fineFilter = new FineFilterViewModel();
					fineFilter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
					var fineJournalViewModel = new FinesJournalViewModel(
						fineFilter,
						_undeliveryViewOpener,
						_employeeService,
						_employeeSelectorFactory,
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						_employeeSettings,
						CommonServices
					);
					fineJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fineJournalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null) {
							return;
						}
						Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, fineJournalViewModel);
				},
				() => CanAttachFine
			);
			AttachFineCommand.CanExecuteChangedWith(this, x => CanAttachFine);
		}

		#endregion AttachFineCommand

		#region AddFineCommand

		public DelegateCommand<ITdiTab> AddFineCommand { get; private set; }

		private void CreateAddFineCommand()
		{
			AddFineCommand = new DelegateCommand<ITdiTab>(
				t => {
					FineViewModel fineViewModel = new FineViewModel(
						EntityUoWBuilder.ForCreate(),
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						_undeliveryViewOpener,
						_employeeService,
						_employeeSelectorFactory,
						_employeeSettings,
						CommonServices
					);
					fineViewModel.FineReasonString = Entity.GetFineReason();
					fineViewModel.EntitySaved += (sender, e) => {
						Entity.AddFine(e.Entity as Fine);
					};
					t.TabParent.AddSlaveTab(t, fineViewModel);
				},
				t => CanAddFine
			);
			AddFineCommand.CanExecuteChangedWith(this, x => CanAddFine);
		}

		#endregion AddFineCommand

		#region ChangeDeliveryPointCommand

		public DelegateCommand ChangeDeliveryPointCommand => _changeDeliveryPointCommand ?? (_changeDeliveryPointCommand =
			new DelegateCommand(() =>
				{
					if(Entity.Order?.DeliveryPoint != null)
					{
						Entity.DeliveryPoint = Entity.Order.DeliveryPoint;
					}
				},
				() => true
			));

		#endregion ChangeDeliveryPointCommand

		#endregion Commands

		public IEntityAutocompleteSelectorFactory OrderAutocompleteSelectorFactory { get; private set; }
		private IEmployeeJournalFactory EmployeeJournalFactory { get; }
		private ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
		private IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }
		private INomenclatureJournalFactory NomenclatureSelector { get; }
		private ISubdivisionParametersProvider SubdivisionParametersProvider { get; }

		public void CloseComplaint(ComplaintStatuses status)
		{
			var msg = Entity.SetStatus(status);
			if(!msg.Any())
			{
				Entity.ActualCompletionDate = status == ComplaintStatuses.Closed ? (DateTime?)DateTime.Now : null;
			}
			else
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,string.Join<string>("\n", msg), "Не удалось закрыть");
			}
			OnPropertyChanged(nameof(Status));
		}

		public override void Close(bool askSave, CloseSource source)
		{
			_logger.Debug("Вызываем Close()");
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return;
			}

			base.Close(askSave, source);
		}

		public override bool Save(bool close)
		{
			_logger.Debug("Вызываем Save()");
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			return base.Save(close);
		}

		public override void Dispose()
		{
			_logger.Debug("Вызываем Dispose()");
			base.Dispose();
		}
	}
}
