using QS.Project.DB;
using Vodovoz.Core.Application.FileStorage;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarEventFileStorageService : FileStorageByS3Base, ICarEventFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CarEventFileStorageService(
			IS3FileStorageService s3FileStorageService,
			IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new System.ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-car-event-attachments";
	}
}
