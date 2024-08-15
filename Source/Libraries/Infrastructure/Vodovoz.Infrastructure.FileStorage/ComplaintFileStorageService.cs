using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class ComplaintFileStorageService : AttachedFilesOnlyFileStorageByS3Base<Complaint, ComplaintFileInformation>, IComplaintFileStorageService
	{
		public ComplaintFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => "vodovoz-complaint-attachments";
	}
}
