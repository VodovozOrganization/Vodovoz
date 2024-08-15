using System.IO;
using System.Linq;
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

		public Task<Result> CreatePhotoAsync(Car entity, string filename, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.AttachedFileInformations.Any(ati => ati.FileName == filename))
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.PhotoMatchesAttachedFileFileName));
			}

			return CreateFileAsync($"{entity.Id}/{filename}", inputStream, cancellationToken);
		}

		public Task<Result<Stream>> GetPhotoAsync(Car entity, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{entity.PhotoFileName}", cancellationToken);

		public Task<Result> UpdatePhotoAsync(Car entity, string filename, Stream inputStream, CancellationToken cancellationToken)
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

		public Task<Result> DeletePhotoAsync(Car entity, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{entity.PhotoFileName}", cancellationToken);

		public Task<Result> CreateFileAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.PhotoFileName == fileName)
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.AttachedFileMatchesPhotoFileName));
			}

			return CreateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);
		}

		public Task<Result<Stream>> GetFileAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}/{fileName}", cancellationToken);

		public Task<Result> UpdateFileAsync(Car entity, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(entity.PhotoFileName == fileName)
			{
				return Task.FromResult(Result.Failure(Application.Errors.FileStorage.AttachedFileMatchesPhotoFileName));
			}

			return UpdateFileAsync($"{entity.Id}/{fileName}", inputStream, cancellationToken);
		}

		public Task<Result> DeleteFileAsync(Car entity, string fileName, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}/{fileName}", cancellationToken);

	}
}
