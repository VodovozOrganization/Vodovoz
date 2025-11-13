using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Application.FileStorage;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal abstract class AttachedFilesOnlyFileStorageByS3Base<TEntity, TFileInformationType> : FileStorageByS3Base
		where TEntity : IDomainObject, IHasAttachedFilesInformations<TFileInformationType>
		where TFileInformationType : FileInformation
	{
		protected AttachedFilesOnlyFileStorageByS3Base(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		public Task<Result> CreateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> CreateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);

		public Task<Result<Stream>> GetFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result> UpdateFileAsync(TEntity entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> UpdateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);

		public Task<Result> DeleteFileAsync(TEntity entity, string fileName, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{fileName}", cancellationToken);
	}
}
