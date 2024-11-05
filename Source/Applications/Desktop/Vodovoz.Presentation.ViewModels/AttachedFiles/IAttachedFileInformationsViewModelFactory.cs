using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Application.FileStorage;
using VodovozBusiness.Common;
using VodovozBusiness.Domain.Common;

namespace Vodovoz.Presentation.ViewModels.AttachedFiles
{
	public interface IAttachedFileInformationsViewModelFactory
	{
		AttachedFileInformationsViewModel Create(IUnitOfWork unitOfWork, Action<string> addFileCallBack = null, Action<string> deleteFileCallback = null);
		AttachedFileInformationsViewModel CreateAndInitialize<TEntity, TFileInformationType>(
			IUnitOfWork unitOfWork,
			TEntity entity,
			IEntityFileStorageService<TEntity> entityFileStorageService,
			Action<string> addFileCallBack = null,
			Action<string> deleteFileCallback = null)
			where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformationType>
			where TFileInformationType : FileInformation;
	}
}
