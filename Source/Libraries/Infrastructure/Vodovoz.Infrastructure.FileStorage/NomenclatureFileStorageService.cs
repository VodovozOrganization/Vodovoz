using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class NomenclatureFileStorageService : AttachedFilesOnlyFileStorageByS3Base<Nomenclature, NomenclatureFileInformation>, INomenclatureFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public NomenclatureFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings) : base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-nomenclature-attachments";
	}
}
