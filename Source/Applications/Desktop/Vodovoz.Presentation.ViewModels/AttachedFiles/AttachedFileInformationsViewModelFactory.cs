using Microsoft.Extensions.Options;
using QS.Attachments;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Common;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Presentation.ViewModels.AttachedFiles
{
	public class AttachedFileInformationsViewModelFactory : IAttachedFileInformationsViewModelFactory
	{
		private readonly IOptions<FileSecurityOptions> _fileSecurityOptions;
		private readonly IInteractiveService _interactiveService;
		private readonly IUserRepository _userRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IScanDialogService _scanDialogService;

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
			_scanDialogService = scanDialogService
				?? throw new ArgumentNullException(nameof(scanDialogService));
		}

		public AttachedFileInformationsViewModel Create(
			IUnitOfWork unitOfWork,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null,
			IEnumerable<FileInformation> fileInformation = null)
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

			if(fileInformation is null)
			{
				viewModel.FileInformations = new ObservableList<FileInformation>();
			}
			else
			{
				viewModel.FileInformations = fileInformation;
			}

			return viewModel;
		}

		public AttachedFileInformationsViewModel CreateAndInitialize<TEntity, TFileInformationType>(
			IUnitOfWork unitOfWork,
			TEntity entity,
			IEntityFileStorageService<TEntity> entityFileStorageService,
			CancellationToken cancellationToken,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null)
			where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformationType>
			where TFileInformationType : FileInformation
		{
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
					.GetFileAsync(entity, fileInformation.FileName, cancellationToken)
					.GetAwaiter()
					.GetResult();

				if(fileResult.IsSuccess)
				{
					using var ms = new MemoryStream();

					fileResult.Value.CopyTo(ms);
					
					//не использовать GetBuffer() для получения массива байтов, может быть получен мусор
					var fileContent = ms.ToArray();

					loadedFiles.Add(fileInformation.FileName, fileContent);
				}
			}

			viewModel.InitializeLoadedFiles(loadedFiles);

			return viewModel;
		}
	}
}
