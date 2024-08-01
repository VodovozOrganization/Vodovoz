using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Errors;

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

		public async Task<Result> CreateFileAsync(string name, string content, CancellationToken cancellationToken)
		{
			return await _s3FileStorageService.CreateFileAsync(BucketName, name, content, cancellationToken);
		}

		public async Task<Result> DeleteFileAsync(string name, CancellationToken cancellationToken)
		{
			return await _s3FileStorageService.DeleteFileAsync(BucketName, name, cancellationToken);
		}

		public async Task<Result> GetFileAsync(string name, CancellationToken cancellationToken)
		{
			return await _s3FileStorageService.GetFileAsync(BucketName, name, cancellationToken);
		}

		public async Task<Result> UpdateFileAsync(string name, string content, CancellationToken cancellationToken)
		{
			return await _s3FileStorageService.UpdateFileAsync(BucketName, name, content, cancellationToken);
		}
	}
}
