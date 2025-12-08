using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using VodovozBusiness.Domain.Complaints;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryDiscussionViewModel : EntityWidgetViewModelBase<UndeliveryDiscussion>
	{
		private readonly bool _canCompleteUndeliveryDiscussionPermission;
		private readonly IPermissionResult _undeliveryPermissionResult;
		private readonly IUserRepository _userRepository;
		private readonly IUndeliveryDiscussionCommentFileStorageService _undeliveryDiscussionCommentFileStorageService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private string _newCommentText;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;
		private UndeliveryDiscussionComment _undeliveryDiscussionComment;

		public UndeliveryDiscussionViewModel(
			UndeliveryDiscussion undeliveryDiscussion,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUnitOfWork uow,
			IUserRepository userRepository,
			IUndeliveryDiscussionCommentFileStorageService undeliveryDiscussionCommentFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(undeliveryDiscussion, commonServices)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_undeliveryDiscussionCommentFileStorageService = undeliveryDiscussionCommentFileStorageService ?? throw new ArgumentNullException(nameof(undeliveryDiscussionCommentFileStorageService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory ?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));

			_undeliveryPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(UndeliveredOrder));
			_canCompleteUndeliveryDiscussionPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Core.Domain.Permissions.OrderPermissions.UndeliveredOrder.CanCompleteUndeliveryDiscussion);

			UoW = uow;
			CurrentEmployee = employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId);
			ConfigureEntityPropertyChanges();

			UndeliveryDiscussionComment = new UndeliveryDiscussionComment();

			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<UndeliveryDiscussionComment, UndeliveryDiscussionCommentFileInformation>(
				UoW,
				UndeliveryDiscussionComment,
				_undeliveryDiscussionCommentFileStorageService,
				_cancellationTokenSource.Token,
				UndeliveryDiscussionComment.AddFileInformation,
				UndeliveryDiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;

			AddCommentCommand = new DelegateCommand(AddComment, () => CanAddComment);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);

			OpenFileCommand = new DelegateCommand<UndeliveryDiscussionCommentFileInformation>(OpenFile);
		}

		public UndeliveryDiscussionComment UndeliveryDiscussionComment
		{
			get => _undeliveryDiscussionComment;
			set => _undeliveryDiscussionComment = value;
		}

		public Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; }
			= new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status, () => CanEditStatus);
		}

		private void AddComment()
		{
			if(CurrentEmployee == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
				return;
			}

			UndeliveryDiscussionComment.Author = CurrentEmployee;
			UndeliveryDiscussionComment.Comment = NewCommentText;
			UndeliveryDiscussionComment.UndeliveryDiscussion = Entity;
			UndeliveryDiscussionComment.CreationTime = DateTime.Now;
			Entity.ObservableComments.Add(UndeliveryDiscussionComment);
			NewCommentText = string.Empty;

			var newComment = UndeliveryDiscussionComment;
			FilesToUploadOnSave.Add(() => newComment.Id, AttachedFileInformationsViewModel.AttachedFiles.ToDictionary(kv => kv.Key, kv => kv.Value));

			UndeliveryDiscussionComment = new UndeliveryDiscussionComment();

			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<UndeliveryDiscussionComment, UndeliveryDiscussionCommentFileInformation>(
				UoW,
				UndeliveryDiscussionComment,
				_undeliveryDiscussionCommentFileStorageService,
				_cancellationTokenSource.Token,
				UndeliveryDiscussionComment.AddFileInformation,
				UndeliveryDiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel
		{
			get => _attachedFileInformationsViewModel;
			private set => SetField(ref _attachedFileInformationsViewModel, value);
		}

		public Employee CurrentEmployee { get; }

		[PropertyChangedAlso(nameof(CanEditDate), nameof(CanEditStatus))]
		public bool CanEdit => PermissionResult.CanUpdate && _undeliveryPermissionResult.CanUpdate;

		public bool CanEditDate => CanEdit && CanCompleteDiscussion;

		public string SubdivisionShortName => string.IsNullOrWhiteSpace(Entity.Subdivision.ShortName) ? "?" : Entity.Subdivision.ShortName;

		#region Status

		public virtual UndeliveryDiscussionStatus[] HiddenDiscussionStatuses => new[] { UndeliveryDiscussionStatus.Closed };

		public bool CanEditStatus => CanEdit && Entity.Status != UndeliveryDiscussionStatus.Closed || (CanEdit && _canCompleteUndeliveryDiscussionPermission);

		public bool CanCompleteDiscussion => CanEditStatus && _canCompleteUndeliveryDiscussionPermission;

		#endregion Status

		#region Comment

		[PropertyChangedAlso(nameof(CanAddComment))]
		public virtual string NewCommentText
		{
			get => _newCommentText;
			set => SetField(ref _newCommentText, value);
		}

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }
		public DelegateCommand<UndeliveryDiscussionCommentFileInformation> OpenFileCommand { get; }

		#endregion

		public void OpenFile(UndeliveryDiscussionCommentFileInformation complaintDiscussionCommentFileInformation)
		{
			byte[] blob;

			if(complaintDiscussionCommentFileInformation.UndeliveryDiscussionCommentId == 0)
			{
				blob = FilesToUploadOnSave.FirstOrDefault(actionCommentIdToFile => actionCommentIdToFile.Value.ContainsKey(complaintDiscussionCommentFileInformation.FileName))
					.Value[complaintDiscussionCommentFileInformation.FileName];
			}
			else
			{
				var comment = Entity.Comments.FirstOrDefault(cdc => cdc.Id == complaintDiscussionCommentFileInformation.UndeliveryDiscussionCommentId);

				var fileResult = _undeliveryDiscussionCommentFileStorageService.GetFileAsync(comment, complaintDiscussionCommentFileInformation.FileName, _cancellationTokenSource.Token)
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
