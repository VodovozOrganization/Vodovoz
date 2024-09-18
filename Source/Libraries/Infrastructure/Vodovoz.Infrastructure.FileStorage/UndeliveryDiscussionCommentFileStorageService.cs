using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class UndeliveryDiscussionCommentFileStorageService
		: AttachedFilesOnlyFileStorageByS3Base<UndeliveryDiscussionComment, UndeliveryDiscussionCommentFileInformation>, IUndeliveryDiscussionCommentFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public UndeliveryDiscussionCommentFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-undelivery-discussion-comment-attachments";
	}
}
