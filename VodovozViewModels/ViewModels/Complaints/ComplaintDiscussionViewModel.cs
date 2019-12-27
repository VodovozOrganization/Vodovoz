using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using System.Linq;
using System.Collections.Generic;
using QS.Project.Repositories;
using System;
using QS.Project.Services;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using QS.Dialog;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionViewModel : EntityWidgetViewModelBase<ComplaintDiscussion>
	{
		private readonly IFilePickerService filePickerService;
		private readonly IEmployeeService employeeService;
		private readonly ICommonServices commonServices;

		public ComplaintDiscussionViewModel(
			ComplaintDiscussion complaintDiscussion,
			IFilePickerService filePickerService,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUnitOfWork uow
			) : base(complaintDiscussion, commonServices)
		{
			this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			newCommentFiles = new GenericObservableList<ComplaintFile>();
			UoW = uow;
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
					currentEmployee = employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		private FilesViewModel filesViewModel;
		public FilesViewModel FilesViewModel {
			get {
				if(filesViewModel == null) {
					filesViewModel = new FilesViewModel(filePickerService, UoW);
					filesViewModel.FilesList = NewCommentFiles;
				}

				return filesViewModel;
			}
		}

		[PropertyChangedAlso(nameof(CanEditDate), nameof(CanEditStatus))]
		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanEditDate => CanEdit && CanCompleteDiscussion;

		public string SubdivisionShortName => string.IsNullOrWhiteSpace(Entity.Subdivision.ShortName) ? "?" : Entity.Subdivision.ShortName;

		#region Status

		public virtual ComplaintStatuses[] HiddenStatuses => new[] { ComplaintStatuses.Closed };

		public bool CanEditStatus => CanEdit && Entity.Status != ComplaintStatuses.Closed || (CanEdit && UserPermissionRepository.CurrentUserPresetPermissions["can_complete_complaint_discussion"]);

		//FIXME переделать репозиторий на зависимость
		public bool CanCompleteDiscussion => CanEditStatus && UserPermissionRepository.CurrentUserPresetPermissions["can_complete_complaint_discussion"];

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
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
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
