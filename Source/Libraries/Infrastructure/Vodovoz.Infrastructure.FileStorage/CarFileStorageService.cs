using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarFileStorageService : AttachedFilesAndPhotoFileStorageByS3Base<Car, CarFileInformation>, ICarFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CarFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings) : base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_","-")}-car-attachments";
	}
}
