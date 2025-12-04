using Autofac;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.Complaints;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintViewModel : EntityTabViewModelBase<Complaint>, IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;
		private DelegateCommand _changeDeliveryPointCommand;
		private readonly IComplaintResultsRepository _complaintResultsRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGeneralSettings _generalSettingsSettings;
		private readonly IComplaintSettings _complaintSettings;
		private readonly IComplaintFileStorageService _complaintFileStorageService;
		private readonly IComplaintDiscussionCommentFileStorageService _complaintDiscussionCommentFileStorageService;
		private readonly IComplaintService _complaintService;
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFileDialogService _fileDialogService;

		private readonly bool _canAddGuiltyInComplaintsPermissionResult;
		private readonly bool _canCloseComplaintsPermissionResult;

		private Employee _currentEmployee;
		private ComplaintDiscussionsViewModel _discussionsViewModel;
		private GuiltyItemsViewModel _guiltyItemsViewModel;
		private List<ComplaintSource> _complaintSources;
		private IEnumerable<ComplaintResultOfCounterparty> _complaintResults;
		private IList<ComplaintKind> _complaintKindSource;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public ComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IUserRepository userRepository,
			IEmployeeJournalFactory driverJournalFactory,
			IComplaintResultsRepository complaintResultsRepository,
			ISubdivisionSettings subdivisionSettings,
			IRouteListItemRepository routeListItemRepository,
			IGeneralSettings generalSettingsSettings,
			IComplaintSettings complaintSettings,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			IComplaintFileStorageService complaintFileStorageService,
			IComplaintDiscussionCommentFileStorageService complaintDiscussionCommentFileStorageService,
			IComplaintService complaintService,
			ILifetimeScope scope)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_complaintResultsRepository = complaintResultsRepository ?? throw new ArgumentNullException(nameof(complaintResultsRepository));
			LifetimeScope = scope ?? throw new ArgumentNullException(nameof(scope));
			EmployeeJournalFactory = driverJournalFactory ?? throw new ArgumentNullException(nameof(driverJournalFactory));
			SubdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_generalSettingsSettings = generalSettingsSettings ?? throw new ArgumentNullException(nameof(generalSettingsSettings));
			_complaintSettings = complaintSettings ?? throw new ArgumentNullException(nameof(complaintSettings));
			_complaintFileStorageService = complaintFileStorageService ?? throw new ArgumentNullException(nameof(complaintFileStorageService));
			_complaintDiscussionCommentFileStorageService = complaintDiscussionCommentFileStorageService ?? throw new ArgumentNullException(nameof(complaintDiscussionCommentFileStorageService));
			_complaintService = complaintService ?? throw new ArgumentNullException(nameof(complaintService));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

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

			_complaintKinds = _complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			ComplaintObject = Entity.ComplaintKind?.ComplaintObject;

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

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

			InitializeEntryViewModels();

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory
				.CreateAndInitialize<Complaint, ComplaintFileInformation>(
					UoW,
					Entity,
					_complaintFileStorageService,
					_cancellationTokenSource.Token,
					Entity.AddFileInformation,
					Entity.RemoveFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		public ILifetimeScope LifetimeScope { get; private set; }
		public IEntityEntryViewModel ComplaintDriverEntryViewModel { get; private set; }
		public IEntityEntryViewModel ComplaintDetalizationEntryViewModel { get; private set; }
		public IEntityEntryViewModel OrderRatingEntryViewModel { get; private set; }

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
			
			if(e.PropertyName == nameof(Entity.ComplaintType))
			{
				OnComplaintTypeChanged();
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
					_discussionsViewModel = LifetimeScope.Resolve<ComplaintDiscussionsViewModel>(
						new TypedParameter(typeof(Complaint), Entity),
						new TypedParameter(typeof(IUnitOfWork), UoW),
						new TypedParameter(typeof(DialogViewModelBase), this));
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
							LifetimeScope,
							CommonServices,
							_subdivisionRepository,
							EmployeeJournalFactory,
							SubdivisionSettings);
				}

				return _guiltyItemsViewModel;
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
					var page = NavigationManager.OpenViewModel<FinesJournalViewModel, Action<FineFilterViewModel>>(
						this,
						filter =>
						{
							filter.CanEditFilter = false;
							filter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
						});

					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += (sender, e) =>
					{
						var selectedObject = e.SelectedObjects.FirstOrDefault();

						if(!(selectedObject is FineJournalNode selectedNode))
						{
							return;
						}

						Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
					};
				},
				() => CanAttachFine
			);
			AttachFineCommand.CanExecuteChangedWith(this, x => CanAttachFine);
		}

		#endregion AttachFineCommand

		#region AddFineCommand

		public DelegateCommand AddFineCommand { get; private set; }

		private void CreateAddFineCommand()
		{
			AddFineCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

					page.ViewModel.FineReasonString = Entity.GetFineReason();
					page.ViewModel.EntitySaved += (sender, e) =>
					{
						Entity.AddFine(e.Entity as Fine);
					};
				},
				() => CanAddFine
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

		private IEmployeeJournalFactory EmployeeJournalFactory { get; }
		private ISubdivisionSettings SubdivisionSettings { get; }
		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		public void ShowMessage(string message)
		{
			ShowInfoMessage(message);
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

		private void OnComplaintTypeChanged()
		{
			if(Entity.ComplaintType == ComplaintType.Client)
			{
				return;
			}
			
			Entity.Order = null;
			Entity.OrderRating = null;
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

		public void SetOrderRating(int orderRatingId)
		{
			var orderRating = UoW.GetById<OrderRating>(orderRatingId);
			Entity.OrderRating = orderRating;
		}

		public void ChangeComplaintStatus(ComplaintStatuses oldStatus, ComplaintStatuses newStatus)
		{
			if(newStatus == ComplaintStatuses.Closed)
			{
				var subdivisionsToInformComplaintHasNoDriver = _generalSettingsSettings.SubdivisionsToInformComplaintHasNoDriver;

				var subdivisionNamesToInform =
					(from guilty in Entity.Guilties.Where(x => x.Subdivision != null)
						where subdivisionsToInformComplaintHasNoDriver.Contains(guilty.Subdivision.Id)
						select guilty.Subdivision.Name)
					.ToArray();

				if(Entity.ComplaintResultOfEmployees?.Id == _complaintSettings.ComplaintResultOfEmployeesIsGuiltyId
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
		
		private void InitializeEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<Complaint>(this, Entity, UoW, NavigationManager, LifetimeScope);
			
			ComplaintDriverEntryViewModel =
				builder
					.ForProperty(x => x.Driver)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.RestrictCategory = EmployeeCategory.driver;
						}
					)
					.Finish();
			
			ComplaintDetalizationEntryViewModel =
				builder
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
			
			OrderRatingEntryViewModel =
				builder
					.ForProperty(x => x.OrderRating)
					.UseViewModelDialog<OrderRatingViewModel>()
					.UseViewModelJournalAndAutocompleter<OrdersRatingsJournalViewModel>()
					.Finish();
			OrderRatingEntryViewModel.IsEditable = false;
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

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!AttachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in AttachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _complaintFileStorageService.CreateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();

					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_complaintFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_complaintFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		public override bool Save(bool close)
		{
			_logger.Debug("Вызываем {Method}()", nameof(Save));

			if(!base.Save(false))
			{
				return false;
			}

			AddAttachedFilesIfNeeded();
			UpdateAttachedFilesIfNeeded();
			DeleteAttachedFilesIfNeeded();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
			AddDiscussionsCommentFilesIfNeeded();

			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			return base.Save(close);
		}

		private void AddDiscussionsCommentFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			do
			{
				foreach(var complaintDiscussionViewModel in DiscussionsViewModel.ObservableComplaintDiscussionViewModels)
				{
					foreach(var keyValuePair in complaintDiscussionViewModel.FilesToUploadOnSave)
					{
						var commentId = keyValuePair.Key.Invoke();

						var comment = Entity
							.ObservableComplaintDiscussions
							.FirstOrDefault(cd => cd.Comments.Any(c => c.Id == commentId))
							?.Comments
							?.FirstOrDefault(c => c.Id == commentId);

						foreach(var fileToUploadPair in keyValuePair.Value)
						{
							using(var ms = new MemoryStream(fileToUploadPair.Value))
							{
								var result = _complaintDiscussionCommentFileStorageService.CreateFileAsync(
									comment,
									fileToUploadPair.Key,
									ms,
									_cancellationTokenSource.Token)
									.GetAwaiter()
									.GetResult();

								if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
								{
									errors.Add(fileToUploadPair.Key, string.Join(", ", result.Errors.Select(e => e.Message)));
								}
							}
						}
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}
		
		protected override bool BeforeSave()
		{
			var checkDuplicatesFromDate = Entity.CreationDate.AddDays(-1);
			var checkDuplicatesToDate = Entity.CreationDate.AddDays(1);
			var canSave = _complaintService.CheckForDuplicateComplaint(UoW, Entity, checkDuplicatesFromDate, checkDuplicatesToDate);

			return canSave;
		}

		public override void Dispose()
		{
			_logger.Debug("Вызываем {Method}()", nameof(Dispose));
			LifetimeScope = null;
			base.Dispose();
		}
	}
}
