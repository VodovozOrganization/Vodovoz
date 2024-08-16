using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Cash;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CashlessRequestFileStorageService : AttachedFilesOnlyFileStorageByS3Base<CashlessRequest, CashlessRequestFileInformation>, ICashlessRequestFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CashlessRequestFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-cashless-request-attachments";
	}
}
