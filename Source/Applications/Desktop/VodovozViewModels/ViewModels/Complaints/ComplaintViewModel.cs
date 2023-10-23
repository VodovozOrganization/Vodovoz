using Autofac;
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
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintViewModel : EntityTabViewModelBase<Complaint>, IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;
		private DelegateCommand _changeDeliveryPointCommand;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IComplaintResultsRepository _complaintResultsRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
		private readonly IComplaintParametersProvider _complaintParametersProvider;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ILifetimeScope _scope;
		private readonly IUserRepository _userRepository;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFileDialogService _fileDialogService;

		private readonly bool _canAddGuiltyInComplaintsPermissionResult;
		private readonly bool _canCloseComplaintsPermissionResult;

		private Employee _currentEmployee;
		private ComplaintDiscussionsViewModel _discussionsViewModel;
		private GuiltyItemsViewModel _guiltyItemsViewModel;
		private ComplaintFilesViewModel _filesViewModel;
		private List<ComplaintSource> _complaintSources;
		private IEnumerable<ComplaintResultOfCounterparty> _complaintResults;
		private IList<ComplaintKind> _complaintKindSource;

		public ComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory driverJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureJournalFactory,
			IEmployeeSettings employeeSettings,
			IComplaintResultsRepository complaintResultsRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IRouteListItemRepository routeListItemRepository,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			IComplaintParametersProvider complaintParametersProvider,
			ILifetimeScope scope)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_complaintResultsRepository = complaintResultsRepository ?? throw new ArgumentNullException(nameof(complaintResultsRepository));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			EmployeeJournalFactory = driverJournalFactory ?? throw new ArgumentNullException(nameof(driverJournalFactory));
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			SubdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_generalSettingsParametersProvider = generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			if(orderSelectorFactory == null)
			{
				throw new ArgumentNullException(nameof(orderSelectorFactory));
			}

			Entity.ObservableComplaintDiscussions.ElementChanged += ObservableComplaintDiscussions_ElementChanged;
			Entity.ObservableComplaintDiscussions.ListContentChanged += ObservableComplaintDiscussions_ListContentChanged;
			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;
			Entity.PropertyChanged += EntityPropertyChanged;

			if(uowBuilder.IsNewEntity)
			{
				AbortOpening("Невозможно создать новую рекламацию из текущего диалога, необходимо использовать диалоги создания");
			}

			if(CurrentEmployee == null)
			{
				AbortOpening("Невозможно открыть рекламацию так как к вашему пользователю не привязан сотрудник");
			}

			ConfigureEntityChangingRelations();

			CreateCommands();

			var driverEntryViewModel =
					new CommonEEVMBuilderFactory<Complaint>(this, Entity, UoW, NavigationManager, _scope)
					.ForProperty(x => x.Driver)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.RestrictCategory = EmployeeCategory.driver;
						}
					)
					.Finish();

			ComplaintDriverEntryViewModel = driverEntryViewModel;

			_complaintKinds = _complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			ComplaintObject = Entity.ComplaintKind?.ComplaintObject;

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			var complaintDetalizationEntryViewModelBuilder = new CommonEEVMBuilderFactory<Complaint>(this, Entity, UoW, NavigationManager, _scope);

			ComplaintDetalizationEntryViewModel = complaintDetalizationEntryViewModelBuilder
				.ForProperty(x => x.ComplaintDetalization)
				.UseViewModelDialog<ComplaintDetalizationViewModel>()
				.UseViewModelJournalAndAutocompleter<ComplaintDetalizationJournalViewModel, ComplaintDetalizationJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictComplaintObject = Entity.ComplaintKind?.ComplaintObject;
						filter.RestrictComplaintKind = Entity.ComplaintKind;
						filter.HideArchieve = true;
					}
				)
				.Finish();

			TabName = $"Рекламация №{Entity.Id} от {Entity.CreationDate.ToShortDateString()}";

			_canAddGuiltyInComplaintsPermissionResult = CommonServices.CurrentPermissionService
				.ValidatePresetPermission("can_add_guilty_in_complaints");
			_canCloseComplaintsPermissionResult = CommonServices.CurrentPermissionService
				.ValidatePresetPermission("can_close_complaints");

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

		public IEntityEntryViewModel ComplaintDriverEntryViewModel { get; }

		public IEntityEntryViewModel ComplaintDetalizationEntryViewModel { get; }

		private void EntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.ComplaintKind))
			{
				OnPropertyChanged(nameof(CanChangeDetalization));
			}

			if(e.PropertyName == nameof(Entity.Order))
			{
				if(Entity.Order is null)
				{
					Entity.Driver = null;
					return;
				}

				var routeList = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity.Order)?.RouteList;

				if(routeList is null)
				{
					Entity.Driver = null;
					return;
				}

				Entity.Driver = routeList.Driver;
			}
		}

		[PropertyChangedAlso(nameof(CanChangeDetalization))]
		public bool CanReadDetalization => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(ComplaintDetalization)).CanRead;

		public bool CanChangeDetalization => CanReadDetalization && Entity.ComplaintKind != null;

		public bool AskSaveOnClose => CanEdit;

		public Employee CurrentEmployee
		{
			get
			{
				if(_currentEmployee == null)
				{
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId);
				}
				return _currentEmployee;
			}
		}

		public virtual ComplaintStatuses Status
		{
			get => Entity.Status;
			set => Entity.SetStatus(value);
		}

		public ComplaintDiscussionsViewModel DiscussionsViewModel
		{
			get
			{
				if(_discussionsViewModel == null)
				{
					_scope.BeginLifetimeScope();
					_discussionsViewModel = _scope.Resolve<ComplaintDiscussionsViewModel>();
				}
				return _discussionsViewModel;
			}
		}

		public GuiltyItemsViewModel GuiltyItemsViewModel
		{
			get
			{
				if(_guiltyItemsViewModel == null)
				{
					_guiltyItemsViewModel =
						new GuiltyItemsViewModel(
							Entity,
							UoW,
							this,
							_scope,
							CommonServices,
							_subdivisionRepository,
							EmployeeJournalFactory,
							SubdivisionParametersProvider);
				}

				return _guiltyItemsViewModel;
			}
		}

		public ComplaintFilesViewModel FilesViewModel
		{
			get
			{
				if(_filesViewModel == null)
				{
					_filesViewModel = new ComplaintFilesViewModel(Entity, UoW, _fileDialogService, CommonServices, _userRepository);
				}
				return _filesViewModel;
			}
		}

		public string SubdivisionsInWork
		{
			get
			{
				string inWork = string.Join(", ", Entity.ComplaintDiscussions
					.Where(x => x.Status == ComplaintDiscussionStatuses.InProcess)
					.Where(x => !string.IsNullOrWhiteSpace(x.Subdivision?.ShortName))
					.Select(x => x.Subdivision.ShortName));
				string okk = (!Entity.ComplaintDiscussions.Any(x => x.Status == ComplaintDiscussionStatuses.InProcess)
							&& Status != ComplaintStatuses.Closed) ? "OKK" : null;
				string result;
				if(!string.IsNullOrWhiteSpace(inWork) && !string.IsNullOrWhiteSpace(okk))
				{
					result = string.Join(", ", inWork, okk);
				}
				else if(!string.IsNullOrWhiteSpace(inWork))
				{
					result = inWork;
				}
				else if(!string.IsNullOrWhiteSpace(okk))
				{
					result = okk;
				}
				else
				{
					return string.Empty;
				}
				return $"В работе у отд. {result}";
			}
		}

		public string ChangedByAndDate => $"Изм: {Entity.ChangedBy?.ShortName} {Entity.ChangedDate:g}";

		public string CreatedByAndDate => $"Созд: {Entity.CreatedBy?.ShortName} {Entity.CreationDate:g}";


		public IEnumerable<ComplaintSource> ComplaintSources
		{
			get
			{
				if(_complaintSources == null)
				{
					_complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return _complaintSources;
			}
		}

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

		public IEnumerable<ComplaintObject> ComplaintObjectSource =>
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().Where(x => !x.IsArchive).ToList());

		public ComplaintObject ComplaintObject
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

		public IList<ComplaintKind> ComplaintKindSource
		{
			get
			{
				if(Entity.ComplaintKind != null && Entity.ComplaintKind.IsArchive)
				{
					_complaintKindSource.Add(UoW.GetById<ComplaintKind>(Entity.ComplaintKind.Id));
				}

				return _complaintKindSource;
			}
			set => SetField(ref _complaintKindSource, value);
		}

		public IList<FineItem> FineItems => Entity.Fines.SelectMany(x => x.Items).OrderByDescending(x => x.Id).ToList();

		public bool IsInnerComplaint => Entity.ComplaintType == ComplaintType.Inner;

		public bool IsClientComplaint => Entity.ComplaintType == ComplaintType.Client;

		[PropertyChangedAlso(nameof(CanAddFine),
			nameof(CanAttachFine),
			nameof(CanSelectDeliveryPoint),
			nameof(CanAddGuilty),
			nameof(CanClose),
			nameof(CanChangeDetalization))]
		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanAddGuilty => CanEdit && _canAddGuiltyInComplaintsPermissionResult;

		public bool CanClose => CanEdit && _canCloseComplaintsPermissionResult;

		public bool CanSelectDeliveryPoint => CanEdit && Entity.Counterparty != null;

		public bool CanAddFine => CanEdit;

		public bool CanAttachFine => CanEdit;

		public bool CanAddArrangementComment => !string.IsNullOrWhiteSpace(NewArrangementCommentText);
		public bool CanAddResultComment => !string.IsNullOrWhiteSpace(NewResultCommentText);

		private string _newArrangementCommentText;
		[PropertyChangedAlso(nameof(CanAddArrangementComment))]
		public virtual string NewArrangementCommentText
		{
			get => _newArrangementCommentText;
			set => SetField(ref _newArrangementCommentText, value);
		}

		private string _newResultCommentText;
		[PropertyChangedAlso(nameof(CanAddResultComment))]
		public virtual string NewResultCommentText
		{
			get => _newResultCommentText;
			set => SetField(ref _newResultCommentText, value);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
			CreateAddResultCommentCommand();
			CreateAddArrangementCommentCommand();
		}

		#region AttachFineCommand

		public DelegateCommand AttachFineCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			AttachFineCommand = new DelegateCommand(
				() =>
				{
					var fineFilter = new FineFilterViewModel();
					fineFilter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
					var fineJournalViewModel = new FinesJournalViewModel(
						fineFilter,
						_employeeService,
						EmployeeJournalFactory,
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						CommonServices,
						_scope);

					fineJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fineJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
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
				t =>
				{
					FineViewModel fineViewModel = new FineViewModel(
						EntityUoWBuilder.ForCreate(),
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						_employeeService,
						EmployeeJournalFactory,
						CommonServices,
						NavigationManager);

					fineViewModel.FineReasonString = Entity.GetFineReason();
					fineViewModel.EntitySaved += (sender, e) =>
					{
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

		#region AddArrangementCommentCommand
		public DelegateCommand AddArrangementCommentCommand { get; private set; }

		private void CreateAddArrangementCommentCommand()
		{
			AddArrangementCommentCommand = new DelegateCommand(
				() => {
					var newComment = new ComplaintArrangementComment();
					if(CurrentEmployee == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
						return;
					}
					newComment.Complaint = Entity;
					newComment.Author = CurrentEmployee;
					newComment.Comment = NewArrangementCommentText;
					newComment.CreationTime = DateTime.Now;
					Entity.ObservableArrangementComments.Add(newComment);
					NewArrangementCommentText = string.Empty;
				},
				() => CanAddArrangementComment
			);
			AddArrangementCommentCommand.CanExecuteChangedWith(this, x => x.CanAddArrangementComment);
		}
		#endregion

		#region AddResultCommentCommand
		public DelegateCommand AddResultCommentCommand { get; private set; }

		private void CreateAddResultCommentCommand()
		{
			AddResultCommentCommand = new DelegateCommand(
				() => {
					var newComment = new ComplaintResultComment();
					if(CurrentEmployee == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
						return;
					}
					newComment.Complaint = Entity;
					newComment.Author = CurrentEmployee;
					newComment.Comment = NewResultCommentText;
					newComment.CreationTime = DateTime.Now;
					Entity.ObservableResultComments.Add(newComment);
					NewResultCommentText = string.Empty;
				},
				() => CanAddResultComment
			);
			AddResultCommentCommand.CanExecuteChangedWith(this, x => x.CanAddResultComment);
		}
		#endregion

		#endregion Commands

		public IEntityAutocompleteSelectorFactory OrderAutocompleteSelectorFactory { get; private set; }
		private IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }
		public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
		private IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }
		private INomenclatureJournalFactory NomenclatureJournalFactory { get; }
		private ISubdivisionParametersProvider SubdivisionParametersProvider { get; }

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

		protected void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				e => e.ComplaintType,
				() => IsInnerComplaint,
				() => IsClientComplaint);

			SetPropertyChangeRelation(
				e => e.Status,
				() => Status);

			SetPropertyChangeRelation(
				e => e.ChangedBy,
				() => ChangedByAndDate);

			SetPropertyChangeRelation(
				e => e.ChangedDate,
				() => ChangedByAndDate);

			SetPropertyChangeRelation(
				e => e.CreatedBy,
				() => CreatedByAndDate);

			SetPropertyChangeRelation(
				e => e.CreationDate,
				() => CreatedByAndDate);

			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => CanSelectDeliveryPoint);

			SetPropertyChangeRelation(
				e => e.ComplaintKind,
				() => CanChangeDetalization);
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

		public void ChangeComplaintStatus(ComplaintStatuses oldStatus, ComplaintStatuses newStatus)
		{
			if(newStatus == ComplaintStatuses.Closed)
			{
				var subdivisionsToInformComplaintHasNoDriver = _generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriver;

				var subdivisionNamesToInform =
					(from guilty in Entity.Guilties.Where(x => x.Subdivision != null)
						where subdivisionsToInformComplaintHasNoDriver.Contains(guilty.Subdivision.Id)
						select guilty.Subdivision.Name)
					.ToArray();

				if(Entity.ComplaintResultOfEmployees?.Id == _complaintParametersProvider.ComplaintResultOfEmployeesIsGuiltyId
					&& subdivisionNamesToInform.Any()
					&& Entity.Driver is null
					&& !AskQuestion($"Вы хотите закрыть рекламацию на отдел {string.Join(", ", subdivisionNamesToInform)} без указания водителя?",
						"Вы уверены?"))
				{
					Entity.SetStatus(oldStatus);
					return;
				}
			}

			var msg = Entity.SetStatus(newStatus);
			if(msg.Any())
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning, string.Join<string>("\n", msg), "Не удалось закрыть");
			}
			else
			{
				Entity.ActualCompletionDate = newStatus == ComplaintStatuses.Closed ? (DateTime?)DateTime.Now : null;
			}
			OnPropertyChanged(nameof(Status));
		}

		public override void Close(bool askSave, CloseSource source)
		{
			_logger.Debug("Вызываем {Method}()", nameof(Close));
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return;
			}

			base.Close(askSave, source);
		}

		public override bool Save(bool close)
		{
			_logger.Debug("Вызываем {Method}()", nameof(Save));
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			return base.Save(close);
		}

		public override void Dispose()
		{
			_logger.Debug("Вызываем {Method}()", nameof(Dispose));
			base.Dispose();
		}
	}
}
