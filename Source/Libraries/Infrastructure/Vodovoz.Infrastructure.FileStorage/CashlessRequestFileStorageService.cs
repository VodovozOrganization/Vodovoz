using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Cash;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CashlessRequestFileStorageService : AttachedFilesOnlyFileStorageByS3Base<CashlessRequest, CashlessRequestFileInformation>, ICashlessRequestFileStorageService
	{
		public CashlessRequestFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => throw new System.NotImplementedException();
	}
}
