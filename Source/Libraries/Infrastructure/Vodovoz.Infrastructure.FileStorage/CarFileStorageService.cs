using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Logistic.Cars;
using VodovozBusiness.Domain.Logistic.Cars;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarFileStorageService : AttachedFilesAndPhotoFileStorageByS3Base<Car, CarFileInformation>, ICarFileStorageService
	{
		public CarFileStorageService(IS3FileStorageService s3FileStorageService) : base(s3FileStorageService)
		{
		}

		protected override string BucketName => "vodovoz-car-attachments";
	}
}
