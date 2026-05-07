using Vodovoz.Core.Application.FileStorage;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarEventFileStorageService : FileStorageByS3Base, ICarEventFileStorageService
	{
		public CarEventFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => "car-event-attachments";
	}
}
