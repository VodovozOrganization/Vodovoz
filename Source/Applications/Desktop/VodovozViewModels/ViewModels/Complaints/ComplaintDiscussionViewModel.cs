using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionViewModel : EntityWidgetViewModelBase<ComplaintDiscussion>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly IComplaintDiscussionCommentFileStorageService _complaintDiscussionCommentFileStorageService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private readonly bool _canCompleteComplaintDiscussionPermission;
		private readonly IPermissionResult _complaintPermissionResult;

		public ComplaintDiscussionViewModel(
			ComplaintDiscussion complaintDiscussion,
			IFileDialogService fileDialogService,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUnitOfWork uow,
			IUserRepository userRepository,
			IComplaintDiscussionCommentFileStorageService complaintDiscussionCommentFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(complaintDiscussion, commonServices)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_complaintDiscussionCommentFileStorageService = complaintDiscussionCommentFileStorageService ?? throw new ArgumentNullException(nameof(complaintDiscussionCommentFileStorageService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory ?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			newCommentFiles = new GenericObservableList<ComplaintFile>();
			_canCompleteComplaintDiscussionPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_complete_complaint_discussion");
			UoW = uow;
			_complaintPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Complaint));
			CreateCommands();
			ConfigureEntityPropertyChanges();

			AddCommentCommand = new DelegateCommand(AddCommentHandler, () => CanAddComment);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);

			ComplaintDiscussionComment = new ComplaintDiscussionComment();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>(
				UoW,
				ComplaintDiscussionComment,
				_complaintDiscussionCommentFileStorageService,
				ComplaintDiscussionComment.AddFileInformation,
				ComplaintDiscussionComment.DeleteFileInformation);
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEditStatus
			);
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

		private ComplaintDiscussionComment _complaintDiscussionComment;

		public ComplaintDiscussionComment ComplaintDiscussionComment
		{
			get => _complaintDiscussionComment;
			set => _complaintDiscussionComment = value;
		}


		private FilesViewModel filesViewModel;
		public FilesViewModel FilesViewModel {
			get {
				if(filesViewModel == null) {
					filesViewModel = new FilesViewModel(_fileDialogService, CommonServices.InteractiveService, UoW, _userRepository);
					filesViewModel.FilesList = NewCommentFiles;
				}

				return filesViewModel;
			}
		}

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel
		{
			get => _attachedFileInformationsViewModel;
			private set => SetField(ref _attachedFileInformationsViewModel, value);
		}

		public Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; }
			= new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		[PropertyChangedAlso(nameof(CanEditDate), nameof(CanEditStatus))]
		public bool CanEdit => PermissionResult.CanUpdate && _complaintPermissionResult.CanUpdate;

		public bool CanEditDate => CanEdit && CanCompleteDiscussion;

		public string SubdivisionShortName => string.IsNullOrWhiteSpace(Entity.Subdivision.ShortName) ? "?" : Entity.Subdivision.ShortName;

		#region Status

		public virtual ComplaintStatuses[] HiddenStatuses => new[] { ComplaintStatuses.Closed };
		public virtual ComplaintDiscussionStatuses[] HiddenDiscussionStatuses => new[] { ComplaintDiscussionStatuses.Closed };

		public bool CanEditStatus => CanEdit && Entity.Status != ComplaintDiscussionStatuses.Closed || (CanEdit && _canCompleteComplaintDiscussionPermission);

		//FIXME переделать репозиторий на зависимость
		public bool CanCompleteDiscussion => CanEditStatus && _canCompleteComplaintDiscussionPermission;

		#endregion Status

		private string newCommentText;
		[PropertyChangedAlso(nameof(CanAddComment))]
		public virtual string NewCommentText {
			get => newCommentText;
			set => SetField(ref newCommentText, value, () => NewCommentText);
		}

		private GenericObservableList<ComplaintFile> newCommentFiles;
		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;

		public virtual GenericObservableList<ComplaintFile> NewCommentFiles {
			get => newCommentFiles;
			set => SetField(ref newCommentFiles, value, () => NewCommentFiles);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateOpenFileCommand();
		}

		#region AddCommentCommand

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }

		private void AddCommentHandler()
		{
			if(CurrentEmployee == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
				return;
			}

			ComplaintDiscussionComment.Author = CurrentEmployee;
			ComplaintDiscussionComment.CreationTime = DateTime.Now;
			ComplaintDiscussionComment.Comment = NewCommentText;

			ComplaintDiscussionComment.ComplaintDiscussion = Entity;
			Entity.ObservableComments.Add(ComplaintDiscussionComment);
			NewCommentText = string.Empty;
			NewCommentFiles.Clear();

			var newComment = ComplaintDiscussionComment;
			FilesToUploadOnSave.Add(() => newComment.Id, AttachedFileInformationsViewModel.AttachedFiles.ToDictionary(kv => kv.Key, kv => kv.Value));

			ComplaintDiscussionComment = new ComplaintDiscussionComment();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>(
				UoW,
				ComplaintDiscussionComment,
				_complaintDiscussionCommentFileStorageService,
				ComplaintDiscussionComment.AddFileInformation,
				ComplaintDiscussionComment.DeleteFileInformation);
		}

		#endregion AddCommentCommand

		#region OpenFileCommand

		public DelegateCommand<ComplaintFile> OpenFileCommand { get; private set; }

		private void CreateOpenFileCommand()
		{
			OpenFileCommand = new DelegateCommand<ComplaintFile>(
				(file) => {
					FilesViewModel.OpenItemCommand.Execute(file);
				},
				(file) => file != null && FilesViewModel.OpenItemCommand.CanExecute(file)
			);
		}

		#endregion OpenFileCommand

		#endregion Commands
	}
}
