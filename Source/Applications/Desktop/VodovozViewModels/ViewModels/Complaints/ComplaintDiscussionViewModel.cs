using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using System;
using QS.Project.Services;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using QS.Dialog;
using Vodovoz.EntityRepositories;
using QS.Project.Services.FileDialog;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionViewModel : EntityWidgetViewModelBase<ComplaintDiscussion>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly bool _canCompleteComplaintDiscussionPermission;
		private readonly IPermissionResult _complaintPermissionResult;

		public ComplaintDiscussionViewModel(
			ComplaintDiscussion complaintDiscussion,
			IFileDialogService fileDialogService,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUnitOfWork uow,
			IUserRepository userRepository
			) : base(complaintDiscussion, commonServices)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			newCommentFiles = new GenericObservableList<ComplaintFile>();
			_canCompleteComplaintDiscussionPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_complete_complaint_discussion");
			UoW = uow;
			_complaintPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Complaint));
			CreateCommands();
			ConfigureEntityPropertyChanges();
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

		public virtual GenericObservableList<ComplaintFile> NewCommentFiles {
			get => newCommentFiles;
			set => SetField(ref newCommentFiles, value, () => NewCommentFiles);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddCommentCommand();
			CreateOpenFileCommand();
		}

		#region AddCommentCommand

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }

		private void CreateAddCommentCommand()
		{
			AddCommentCommand = new DelegateCommand(
				() => {
					var newComment = new ComplaintDiscussionComment();
					if(CurrentEmployee == null) {
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
						return;
					}
					newComment.Author = CurrentEmployee;
					newComment.CreationTime = DateTime.Now;
					newComment.Comment = NewCommentText;
					foreach(ComplaintFile file in newCommentFiles) {
						file.ComplaintDiscussionComment = newComment;
						newComment.ObservableFiles.Add(file);
					}
					newComment.ComplaintDiscussion = Entity;
					Entity.ObservableComments.Add(newComment);
					NewCommentText = string.Empty;
					NewCommentFiles.Clear();
				},
				() => CanAddComment
			);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);
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
