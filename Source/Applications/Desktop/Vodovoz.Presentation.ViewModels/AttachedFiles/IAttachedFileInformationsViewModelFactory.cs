using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Presentation.ViewModels.AttachedFiles
{
	public interface IAttachedFileInformationsViewModelFactory
	{
		AttachedFileInformationsViewModel Create(IUnitOfWork unitOfWork, Action<string> addFileCallBack = null, Action<string> deleteFileCallback = null, IEnumerable<FileInformation> fileInformation = null);
		AttachedFileInformationsViewModel CreateAndInitialize<TEntity, TFileInformationType>(
			IUnitOfWork unitOfWork,
			TEntity entity,
			IEntityFileStorageService<TEntity> entityFileStorageService,
			CancellationToken cancellationToken,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null)
			where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformationType>
			where TFileInformationType : FileInformation;
	}
}
