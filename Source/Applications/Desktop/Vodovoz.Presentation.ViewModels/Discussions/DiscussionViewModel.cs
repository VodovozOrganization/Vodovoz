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
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using VodovozBusiness.Domain.Discussions;

namespace Vodovoz.Presentation.ViewModels.Discussions
{
	public abstract class DiscussionViewModel : WidgetViewModelBase
	{
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly IPermissionResult _containerEntiryPermissionResult;
		private readonly IPermissionResult _discussionPermissionResult;

		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;
		private readonly IUserRepository _userRepository;
		private readonly IEmployeeService _employeeService;
		private readonly IInteractiveService _interactiveService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private readonly IEntityFileStorageService<IDiscussionComment<FileInformation>> _entityFileStorageService;
		private readonly Func<IDiscussionComment<FileInformation>> _discussionCommentFactory;
		private string _newCommentText;
		private Employee _currentEmployee;

		private IDiscussionComment<FileInformation> _discussionComment;
		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;

		public DiscussionViewModel(
			IUnitOfWork unitOfWork,
			IDomainObject discussionContainer,
			IDiscussion<IDomainObject, IDiscussionComment<FileInformation>, FileInformation> discussion,
			IUserService userService,
			IUserRepository userRepository,
			IEmployeeService employeeService,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			IEntityFileStorageService<IDiscussionComment<FileInformation>> entityFileStorageService,
			Func<IDiscussionComment<FileInformation>> discussionCommentFactory)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			_containerEntiryPermissionResult =
				currentPermissionService.ValidateEntityPermission(discussionContainer.GetType());
			_discussionPermissionResult =
				currentPermissionService.ValidateEntityPermission(discussion.GetType());

			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_userService = userService
				?? throw new ArgumentNullException(nameof(userService));
			_userRepository = userRepository;
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));

			Discussion = discussion
				?? throw new ArgumentNullException(nameof(discussion));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory
				?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			_entityFileStorageService = entityFileStorageService ?? throw new ArgumentNullException(nameof(entityFileStorageService));
			_discussionCommentFactory = discussionCommentFactory ?? throw new ArgumentNullException(nameof(discussionCommentFactory));

			AddCommentCommand = new DelegateCommand(AddComment, () => CanAddComment);

			DiscussionComment = _discussionCommentFactory.Invoke();
			OpenFileCommand = new DelegateCommand<FileInformation>(OpenFile);

			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<IDiscussionComment<FileInformation>, FileInformation>(
				_unitOfWork,
				DiscussionComment,
				_entityFileStorageService,
				_cancellationTokenSource.Token,
				DiscussionComment.AddFileInformation,
				DiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		public IDiscussion<IDomainObject, IDiscussionComment<FileInformation>, FileInformation> Discussion { get; }

		public DelegateCommand AddCommentCommand { get; protected set; }
		public DelegateCommand<FileInformation> OpenFileCommand { get; protected set; }

		
		[PropertyChangedAlso(nameof(CanAddComment))]
		public virtual string NewCommentText
		{
			get => _newCommentText;
			set => SetField(ref _newCommentText, value);
		}

		public Employee CurrentEmployee
		{
			get
			{
				_currentEmployee ??= _employeeService.GetEmployeeForUser(
					_unitOfWork,
					_userService.CurrentUserId);

				return _currentEmployee;
			}
		}

		public IDiscussionComment<FileInformation> DiscussionComment
		{
			get => _discussionComment;
			set => SetField(ref _discussionComment, value);
		}

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel
		{
			get => _attachedFileInformationsViewModel;
			private set => SetField(ref _attachedFileInformationsViewModel, value);
		}

		public bool CanEdit => _discussionPermissionResult.CanUpdate
			&& _containerEntiryPermissionResult.CanUpdate;

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		private Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; } = new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		private void AddComment()
		{
			if(CurrentEmployee == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Невозможно добавить комментарий так как " +
					"к вашему пользователю не привязан сотрудник");

				return;
			}

			DiscussionComment.Author = CurrentEmployee;
			DiscussionComment.CreationTime = DateTime.Now;
			DiscussionComment.Comment = NewCommentText;

			Discussion.AddComment(DiscussionComment);
			NewCommentText = string.Empty;

			var newComment = DiscussionComment;

			FilesToUploadOnSave.Add(
				() => newComment.Id,
				AttachedFileInformationsViewModel.AttachedFiles
					.ToDictionary(kv => kv.Key, kv => kv.Value));

			DiscussionComment = _discussionCommentFactory.Invoke();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<IDiscussionComment<FileInformation>, FileInformation>(
				_unitOfWork,
				DiscussionComment,
				_entityFileStorageService,
				_cancellationTokenSource.Token,
				DiscussionComment.AddFileInformation,
				DiscussionComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		private void OpenFile(FileInformation fileInformation)
		{
			byte[] blob;

			if(fileInformation.Id == 0)
			{
				blob = FilesToUploadOnSave.FirstOrDefault(actionCommentIdToFile => actionCommentIdToFile.Value.ContainsKey(fileInformation.FileName))
					.Value[fileInformation.FileName];
			}
			else
			{
				var comment = Discussion.Comments.FirstOrDefault(cdc => cdc.Id == fileInformation.Id);

				var fileResult = _entityFileStorageService.GetFileAsync(comment, fileInformation.FileName, _cancellationTokenSource.Token)
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

			var vodovozUserTempDirectory = _userRepository.GetTempDirForCurrentUser(_unitOfWork);

			if(string.IsNullOrWhiteSpace(vodovozUserTempDirectory))
			{
				return;
			}

			var tempFilePath = Path.Combine(Path.GetTempPath(), vodovozUserTempDirectory, fileInformation.FileName);

			if(!File.Exists(tempFilePath))
			{
				File.WriteAllBytes(tempFilePath, blob);
			}

			var process = new Process
			{
				EnableRaisingEvents = true
			};

			process.StartInfo.FileName = Path.Combine(vodovozUserTempDirectory, fileInformation.FileName);

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
