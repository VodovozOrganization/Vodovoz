using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CounterpartyFileStorageService : AttachedFilesOnlyFileStorageByS3Base<Counterparty, CounterpartyFileInformation>, ICounterpartyFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public CounterpartyFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-counterparty-attachments";
	}
}
