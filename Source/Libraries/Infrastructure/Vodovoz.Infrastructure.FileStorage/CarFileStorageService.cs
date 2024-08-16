using QS.Project.DB;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Logistic.Cars;
using VodovozBusiness.Domain.Logistic.Cars;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarFileStorageService : AttachedFilesAndPhotoFileStorageByS3Base<Car, CarFileInformation>, ICarFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CarFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings) : base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings;
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_","-")}-car-attachments";
	}
}
