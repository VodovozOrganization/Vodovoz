using Microsoft.Extensions.Options;
using QS.Attachments;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.EntityRepositories;
using VodovozBusiness.Common;
using VodovozBusiness.Domain.Common;

namespace Vodovoz.Presentation.ViewModels.AttachedFiles
{
	public class AttachedFileInformationsViewModelFactory : IAttachedFileInformationsViewModelFactory
	{
		private readonly IOptions<FileSecurityOptions> _fileSecurityOptions;
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IScanDialogService _scanDialogService;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public AttachedFileInformationsViewModelFactory(
			IOptions<FileSecurityOptions> fileSecurityOptions,
			IInteractiveService interactiveService,
			IUserRepository userRepository,
			IFileDialogService fileDialogService,
			IScanDialogService scanDialogService)
		{
			_fileSecurityOptions = fileSecurityOptions
				?? throw new ArgumentNullException(nameof(fileSecurityOptions));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_userRepository = userRepository
				?? throw new ArgumentNullException(nameof(userRepository));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_scanDialogService = scanDialogService ?? throw new ArgumentNullException(nameof(scanDialogService));
		}

		public AttachedFileInformationsViewModel Create(
			IUnitOfWork unitOfWork,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null)
		{
			var viewModel = new AttachedFileInformationsViewModel(
				_fileSecurityOptions,
				unitOfWork,
				_interactiveService,
				_userRepository,
				_fileDialogService,
				_scanDialogService);

			if(addFileCallBack != null)
			{
				viewModel.AddFileCallback = addFileCallBack;
			}

			if(deleteFileCallback != null)
			{
				viewModel.DeleteFileCallback = deleteFileCallback;
			}

			return viewModel;
		}

		public AttachedFileInformationsViewModel CreateAndInitialize<TEntity, TFileInformationType>(
			IUnitOfWork unitOfWork,
			TEntity entity,
			IEntityFileStorageService<TEntity> entityFileStorageService,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null)
			where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformationType>
			where TFileInformationType : FileInformation
		{
			if(_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = new CancellationTokenSource();
			}

			var viewModel = Create(unitOfWork, addFileCallBack, deleteFileCallback);
			viewModel.FileInformations = entity.AttachedFileInformations;

			if(entity.Id == 0)
			{
				return viewModel;
			}

			var loadedFiles = new Dictionary<string, byte[]>();

			foreach(var fileInformation in entity.AttachedFileInformations)
			{
				var fileResult = entityFileStorageService
					.GetFileAsync(entity, fileInformation.FileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();

				if(fileResult.IsSuccess)
				{
					using var ms = new MemoryStream();

					fileResult.Value.CopyTo(ms);
					var fileContent = ms.ToArray();

					loadedFiles.Add(fileInformation.FileName, fileContent);
				}
			}

			viewModel.InitializeLoadedFiles(loadedFiles);

			return viewModel;
		}
	}
}
