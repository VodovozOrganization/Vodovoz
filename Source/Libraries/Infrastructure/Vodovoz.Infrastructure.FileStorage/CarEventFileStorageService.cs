using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Application.FileStorage;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarEventFileStorageService : FileStorageByS3Base, ICarEventFileStorageService
	{
		public CarEventFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		// Должен быть убран при деплое в продакшен, так как S3 недоступен в dev-среде.
		public new Task<Result> CreateFileAsync(string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			return Task.FromResult(Result.Failure(Core.Application.Errors.S3.ServiceUnavailable));
		}

		protected override string BucketName => "car-event-attachments";
	}
}
