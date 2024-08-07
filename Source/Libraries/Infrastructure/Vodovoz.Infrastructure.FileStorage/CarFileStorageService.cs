using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Errors;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarFileStorageService : FileStorageByS3Base, IFileStorageService, ICarFileStorageService
	{
		public CarFileStorageService(IS3FileStorageService s3FileStorageService) : base(s3FileStorageService)
		{
		}

		protected override string BucketName => "vodovoz-car-attachments";

		public Task<Result> CreateFileAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> CreateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);

		public Task<Result> CreatePhotoAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> CreateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);

		public Task<Result> DeleteFileAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result> DeletePhotoAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result<Stream>> GetFileAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result<Stream>> GetPhotoAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result> UpdateFileAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> UpdateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);

		public Task<Result> UpdatePhotoAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
			=> UpdateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);
	}
}
