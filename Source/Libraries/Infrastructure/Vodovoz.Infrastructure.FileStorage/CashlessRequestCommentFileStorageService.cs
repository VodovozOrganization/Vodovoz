using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CashlessRequestCommentFileStorageService : AttachedFilesOnlyFileStorageByS3Base<CashlessRequestComment, CashlessRequestCommentFileInformation>, ICashlessRequestCommentFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CashlessRequestCommentFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-cashless-request-comment-attachments";

	}
}
