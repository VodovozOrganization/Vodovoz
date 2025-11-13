using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Infrastructure.FileStorage
{
	public abstract class FileStorageByS3Base : IFileStorageService
	{
		private readonly IS3FileStorageService _s3FileStorageService;

		protected FileStorageByS3Base(IS3FileStorageService s3FileStorageService)
		{
			_s3FileStorageService = s3FileStorageService;
		}

		protected abstract string BucketName { get; }

		public async Task<Result> CreateFileAsync(string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> await _s3FileStorageService.CreateFileAsync(BucketName, fileName, inputStream, cancellationToken);

		public async Task<Result<Stream>> GetFileAsync(string fileName, CancellationToken cancellationToken)
			=> await _s3FileStorageService.GetFileAsync(BucketName, fileName, cancellationToken);

		public async Task<Result> UpdateFileAsync(string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> await _s3FileStorageService.UpdateFileAsync(BucketName, fileName, inputStream, cancellationToken);

		public async Task<Result> DeleteFileAsync(string fileName, CancellationToken cancellationToken)
			=> await _s3FileStorageService.DeleteFileAsync(BucketName, fileName, cancellationToken);
	}
}
