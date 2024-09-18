﻿using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vodovoz.Application.FileStorage;
using Vodovoz.Application.Options;

namespace Vodovoz.Infrastructure.S3
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructureS3(this IServiceCollection services)
			=> services
				.AddScoped<IS3FileStorageService, S3FileStorageService>()
				.AddScoped<AmazonS3Client>(sp =>
				{
					var options = sp.GetRequiredService<IOptions<S3Options>>();

					var config = new AmazonS3Config
					{
						ServiceURL = options.Value.ServiceUrl,
						ForcePathStyle = true,
					};

					return new AmazonS3Client(
						options.Value.AccessKey,
						options.Value.SecretKey,
						config);
				});
	}
}
