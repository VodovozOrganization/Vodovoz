using QS.Project.DB;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class EdoContainerFileStorageService : FileStorageByS3Base, IEdoContainerFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public EdoContainerFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-edo-containers";

		public Task<Result> CreateContainerAsync(EdoContainer entity, Stream inputStream, CancellationToken cancellationToken)
			=> CreateFileAsync($"{entity.Id}.zip", inputStream, cancellationToken);

		public Task<Result<Stream>> GetContainerAsync(EdoContainer entity, CancellationToken cancellationToken)
			=> GetFileAsync($"{entity.Id}.zip", cancellationToken);

		public Task<Result> UpdateContainerAsync(EdoContainer entity, Stream inputStream, CancellationToken cancellationToken) =>
			UpdateFileAsync($"{entity.Id}.zip", inputStream, cancellationToken);

		public Task<Result> DeleteContainerAsync(EdoContainer entity, CancellationToken cancellationToken)
			=> DeleteFileAsync($"{entity.Id}.zip", cancellationToken);
	}
}
