using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class ComplaintDiscussionCommentFileStorageService
		: AttachedFilesOnlyFileStorageByS3Base<ComplaintDiscussionComment, ComplaintDiscussionCommentFileInformation>,
		IComplaintDiscussionCommentFileStorageService
	{
		public ComplaintDiscussionCommentFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => "vodovoz-complaint-discussion-comment-attachments";
	}
}
