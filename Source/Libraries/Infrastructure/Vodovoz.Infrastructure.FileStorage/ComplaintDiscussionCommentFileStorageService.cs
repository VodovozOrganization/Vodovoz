using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class ComplaintDiscussionCommentFileStorageService
		: AttachedFilesOnlyFileStorageByS3Base<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>,
		IComplaintDiscussionCommentFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public ComplaintDiscussionCommentFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-complaint-discussion-comment-attachments";
	}
}
