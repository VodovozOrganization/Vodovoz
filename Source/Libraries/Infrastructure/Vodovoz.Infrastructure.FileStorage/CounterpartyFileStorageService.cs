using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CounterpartyFileStorageService : AttachedFilesOnlyFileStorageByS3Base<Counterparty, CounterpartyFileInformation>, ICounterpartyFileStorageService
	{
		public CounterpartyFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => throw new NotImplementedException();
	}
}
