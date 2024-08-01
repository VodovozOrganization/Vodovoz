using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Errors;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class CarFileStorageService : FileStorageByS3Base, ICarFileStorageService
	{
		public CarFileStorageService(IS3FileStorageService s3FileStorageService) : base(s3FileStorageService)
		{
		}

		protected override string BucketName => "vodovoz-car-attachments";
	}
}
