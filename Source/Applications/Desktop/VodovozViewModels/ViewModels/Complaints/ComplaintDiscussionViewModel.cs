using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Complaints;
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

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
			_canCompleteComplaintDiscussionPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_complete_complaint_discussion");
			UoW = uow;
			_complaintPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Complaint));
			ConfigureEntityPropertyChanges();

			AddCommentCommand = new DelegateCommand(AddCommentHandler, () => CanAddComment);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);

			OpenFileCommand = new DelegateCommand<ComplaintDiscussionCommentFileInformation>(OpenFile);

			ComplaintDiscussionComment = new ComplaintDiscussionComment();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>(
				UoW,
				ComplaintDiscussionComment,
				_complaintDiscussionCommentFileStorageService,
				_cancellationTokenSource.Token,
				ComplaintDiscussionComment.AddFileInformation,
				ComplaintDiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;

			UpdateFilesMissingOnStorage();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => Status);

			SetPropertyChangeRelation(e => e.Status,
				() => CanEditStatus);
		}

		public ComplaintDiscussionStatuses Status => Entity.Status;

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

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel
		{
			get => _attachedFileInformationsViewModel;
			private set => SetField(ref _attachedFileInformationsViewModel, value);
		}

		public Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; }
			= new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		public List<string> FilesMissingOnStorage { get; } = new List<string>();

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

		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;

		#region Commands

		#region AddCommentCommand

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }

		public void UpdateFilesMissingOnStorage()
		{
			FilesMissingOnStorage.Clear();

			foreach(var comment in Entity.ObservableComments)
			{
				var loadedFiles = GetLoadedFilesForComment(comment);

				foreach(var fileInformation in comment.AttachedFileInformations)
				{
					if(!loadedFiles.ContainsKey(fileInformation.FileName))
					{
						FilesMissingOnStorage.Add(fileInformation.FileName);
					}
				}
			}
		}

		private Dictionary<string, byte[]> GetLoadedFilesForComment(ComplaintDiscussionComment complaintDiscussionComment)
		{
			var loadedFiles = new Dictionary<string, byte[]>();
			foreach(var fileInformation in complaintDiscussionComment.AttachedFileInformations)
			{
				var fileResult = _complaintDiscussionCommentFileStorageService
					.GetFileAsync(complaintDiscussionComment, fileInformation.FileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();

				if(fileResult.IsSuccess)
				{
					using(var ms = new MemoryStream())
					{
						fileResult.Value.CopyTo(ms);
						var fileContent = ms.ToArray();
						loadedFiles.Add(fileInformation.FileName, fileContent);
					}
				}
			}

			return loadedFiles;
		}

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

			var newComment = ComplaintDiscussionComment;
			FilesToUploadOnSave.Add(() => newComment.Id, AttachedFileInformationsViewModel.AttachedFiles.ToDictionary(kv => kv.Key, kv => kv.Value));

			ComplaintDiscussionComment = new ComplaintDiscussionComment();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>(
				UoW,
				ComplaintDiscussionComment,
				_complaintDiscussionCommentFileStorageService,
				_cancellationTokenSource.Token,
				ComplaintDiscussionComment.AddFileInformation,
				ComplaintDiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		#endregion AddCommentCommand

		#region OpenFileCommand

		public DelegateCommand<ComplaintDiscussionCommentFileInformation> OpenFileCommand { get; }

		public void OpenFile(ComplaintDiscussionCommentFileInformation complaintDiscussionCommentFileInformation)
		{
			byte[] blob;

			if(complaintDiscussionCommentFileInformation.ComplaintDiscussionCommentId == 0)
			{
				blob = FilesToUploadOnSave.FirstOrDefault(actionCommentIdToFile => actionCommentIdToFile.Value.ContainsKey(complaintDiscussionCommentFileInformation.FileName))
					.Value[complaintDiscussionCommentFileInformation.FileName];
			}
			else
			{
				var comment = Entity.Comments.FirstOrDefault(cdc => cdc.Id == complaintDiscussionCommentFileInformation.ComplaintDiscussionCommentId);

				var fileResult = _complaintDiscussionCommentFileStorageService.GetFileAsync(comment, complaintDiscussionCommentFileInformation.FileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();

				if(fileResult.IsFailure)
				{
					return;
				}

				using(var ms = new MemoryStream())
				{
					fileResult.Value.CopyTo(ms);

					blob = ms.ToArray();
				}
			}

			var vodovozUserTempDirectory = _userRepository.GetTempDirForCurrentUser(UoW);

			if(string.IsNullOrWhiteSpace(vodovozUserTempDirectory))
			{
				return;
			}

			var tempFilePath = Path.Combine(Path.GetTempPath(), vodovozUserTempDirectory, complaintDiscussionCommentFileInformation.FileName);

			if(!File.Exists(tempFilePath))
			{
				File.WriteAllBytes(tempFilePath, blob);
			}

			var process = new Process
			{
				EnableRaisingEvents = true
			};

			process.StartInfo.FileName = Path.Combine(vodovozUserTempDirectory, complaintDiscussionCommentFileInformation.FileName);

			process.Exited += OnProcessExited;
			process.Start();
		}

		#endregion OpenFileCommand

		#endregion Commands

		private void OnProcessExited(object sender, EventArgs e)
		{
			if(sender is Process process)
			{
				File.Delete(process.StartInfo.FileName);
				process.Exited -= OnProcessExited;
			}
		}
	}
}
