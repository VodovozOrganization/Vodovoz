using QS.DomainModel.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal abstract class AttachedFilesAndPhotoFileStorageByS3Base<TEntity, TFileInformation> : FileStorageByS3Base
		where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformation>, IHasPhoto
		where TFileInformation : FileInformation
	{
		protected AttachedFilesAndPhotoFileStorageByS3Base(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		public Task<Result> CreatePhotoAsync(TEntity entity, string filename, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.AttachedFileInformations.Any(ati => ati.FileName == filename))
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.PhotoMatchesAttachedFileFileName));
			}

			return CreateFileAsync($"{entity.Id}/{filename}", inputStream, cancellationToken);
		}

		public Task<Result<Stream>> GetPhotoAsync(TEntity entity, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{entity.PhotoFileName}", cancellationToken);

		public Task<Result> UpdatePhotoAsync(TEntity entity, string filename, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.AttachedFileInformations.Any(ati => ati.FileName == filename))
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.PhotoMatchesAttachedFileFileName));
			}

			if(string.IsNullOrWhiteSpace(entity.PhotoFileName))
			{
				return CreatePhotoAsync(entity, filename, inputStream, cancellationToken);
			}

			if(entity.PhotoFileName == filename)
			{
				return UpdateFileAsync($"{entity.Id}/{filename}", inputStream, cancellationToken);
			}

			DeletePhotoAsync(entity, cancellationToken);

			return CreatePhotoAsync(entity, filename, inputStream, cancellationToken);
		}

		public Task<Result> DeletePhotoAsync(TEntity entity, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{entity.PhotoFileName}", cancellationToken);

		public Task<Result> CreateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.PhotoFileName == fileName)
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.AttachedFileMatchesPhotoFileName));
			}

			return CreateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);
		}

		public Task<Result<Stream>> GetFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result> UpdateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.PhotoFileName == fileName)
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.AttachedFileMatchesPhotoFileName));
			}

			return UpdateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);
		}

		public Task<Result> DeleteFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{fileName}", cancellationToken);
	}
}
