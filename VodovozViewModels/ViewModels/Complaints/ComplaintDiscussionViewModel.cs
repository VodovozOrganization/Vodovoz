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

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionViewModel : EntityWidgetViewModelBase<ComplaintDiscussion>
	{
		public ComplaintDiscussionViewModel(
			ComplaintDiscussion complaintDiscussion,
			ICommonServices commonServices
			) : base(complaintDiscussion, commonServices)
		{
			newCommentFiles = new GenericObservableList<ComplaintFile>();
			SubscribeFileListChanged();
			CreateCommands();
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEditStatus
			);
		}

		//FIXME переделать репозиторий на зависимость
		public bool CanCompleteDiscussion => UserPermissionRepository.CurrentUserPresetPermissions["can_complete_complaint_discussion"];

		[PropertyChangedAlso(nameof(CanEditDate), nameof(CanEditStatus))]
		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanEditDate => CanEdit && CanCompleteDiscussion;

		#region Status

		public virtual ComplaintStatuses[] HiddenStatuses => new[] { ComplaintStatuses.Closed };

		public bool CanEditStatus => CanEdit && CanCompleteDiscussion && Entity.Status != ComplaintStatuses.Closed;

		private IEnumerable<ComplaintStatuses> availableStatuses;
		public IEnumerable<ComplaintStatuses> AvailableStatuses {
			get {
				if(availableStatuses == null) {
					if(!CanCompleteDiscussion) {
						availableStatuses = new[] { ComplaintStatuses.InProcess, ComplaintStatuses.Checking };
					}
					availableStatuses = Enum.GetValues(typeof(ComplaintStatuses)).Cast<ComplaintStatuses>();
				}
				return availableStatuses;
			}
		}

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
			set {
				if(SetField(ref newCommentFiles, value, () => NewCommentFiles)) {
					SubscribeFileListChanged();
				}
			}
		}

		private void SubscribeFileListChanged()
		{
			newCommentFiles.ListChanged += (aList) => OnPropertyChanged(() => CanClearFiles);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddFilesCommand();
			CreateClearFilesCommand();
			CreateAddCommentCommand();
			CreateOpenFileCommand();
		}

		#region AddFilesCommand

		public bool CanAddFiles => CanAddComment;

		public DelegateCommand AddFilesCommand { get; private set; }

		private void CreateAddFilesCommand()
		{
			AddFilesCommand = new DelegateCommand(
				() => { },
				() => CanAddFiles
			);
			AddFilesCommand.CanExecuteChangedWith(this, x => x.CanAddFiles);
		}

		#endregion AddFilesCommand

		#region ClearFilesCommand

		public bool CanClearFiles => NewCommentFiles.Any();

		public DelegateCommand ClearFilesCommand { get; private set; }

		private void CreateClearFilesCommand()
		{
			ClearFilesCommand = new DelegateCommand(
				() => { },
				() => CanClearFiles
			);
			ClearFilesCommand.CanExecuteChangedWith(this, x => x.CanClearFiles);
		}

		#endregion ClearFilesCommand

		#region AddCommentCommand

		[PropertyChangedAlso(nameof(CanAddFiles))]
		public bool CanAddComment => string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }

		private void CreateAddCommentCommand()
		{
			AddCommentCommand = new DelegateCommand(
				() => {
					var newComment = new ComplaintDiscussionComment();
					newComment.Comment = NewCommentText;
					foreach(ComplaintFile file in newCommentFiles) {
						newComment.ObservableFiles.Add(file);
					}
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
					//код открытия файла
				},
				(file) => file != null
			);
		}

		#endregion OpenFileCommand

		#endregion Commands
	}
}
