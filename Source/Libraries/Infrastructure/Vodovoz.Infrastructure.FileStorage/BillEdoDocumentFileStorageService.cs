using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EdoService.Library.Services;
using QS.Project.DB;
using Vodovoz.Core.Application.FileStorage;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class BillEdoDocumentFileStorageService : FileStorageByS3Base, IBillEdoDocumentFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public BillEdoDocumentFileStorageService(
			IS3FileStorageService s3FileStorageService,
			IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings
				?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-edo-containers";

		public async Task SaveAsync(Guid documentId, byte[] pdf, CancellationToken cancellationToken)
		{
			using(var stream = new MemoryStream(pdf, false))
			{
				var result = await CreateFileAsync($"bills/{documentId:D}.pdf", stream, cancellationToken);
				if(result.IsFailure)
				{
					throw new InvalidOperationException($"Не удалось сохранить PDF счета в S3: {result.GetErrorsString()}");
				}
			}
		}

		public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken)
		{
			var result = await DeleteFileAsync($"bills/{documentId:D}.pdf", cancellationToken);
			if(result.IsFailure)
			{
				throw new InvalidOperationException($"Не удалось удалить PDF счета из S3: {result.GetErrorsString()}");
			}
		}

		public async Task<byte[]> GetAsync(Guid documentId, CancellationToken cancellationToken)
		{
			var result = await GetFileAsync($"bills/{documentId:D}.pdf", cancellationToken);
			if(result.IsFailure)
			{
				throw new InvalidOperationException($"Не удалось загрузить PDF счета из S3: {result.GetErrorsString()}");
			}

			const int defaultBufferSize = 81920;
			
			using(var stream = result.Value)
			using(var buffer = new MemoryStream())
			{
				await stream.CopyToAsync(buffer, defaultBufferSize, cancellationToken);
				return buffer.ToArray();
			}
		}
	}
}
