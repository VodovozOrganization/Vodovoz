using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Entity;
using TaxcomEdoApi.Library;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi
{
	public static class TaxcomEdoApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<WarrantOptions>(config.GetSection(WarrantOptions.Path))
				.Configure<TaxcomEdoApiOptions>(config.GetSection(TaxcomEdoApiOptions.Path));
			
			return services;
		}

		public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services
				.AddSingleton(provider =>
				{
					var apiOptions = provider.GetRequiredService<IOptions<TaxcomEdoApiOptions>>().Value;
					var certificateThumbprint = apiOptions.CertificateThumbprint.ToUpper();
					var certificate =
						CertificateLogic.GetAvailableCertificates().SingleOrDefault(x => x.Thumbprint == certificateThumbprint);

					if(certificate is null)
					{
						throw new InvalidOperationException("Не найден сертификат в личном хранилище пользователя");
					}

					return certificate;
				})
				.AddScoped(provider =>
				{
					var apiOptions = provider.GetRequiredService<IOptions<TaxcomEdoApiOptions>>().Value;
					var certificate = provider.GetRequiredService<X509Certificate2>();
					var apiCryptographicMode = apiOptions.CryptographicMode.TryParseAsEnum<CryptographicMode>();

					if(apiCryptographicMode is null)
					{
						return new Factory().CreateApi(
							apiOptions.BaseUrl,
							true,
							apiOptions.IntegratorId,
							certificate.RawData,
							apiOptions.EdxClientId);
					}
					
					return new Factory().CreateApi(
						apiOptions.BaseUrl,
						true,
						apiOptions.IntegratorId,
						certificate.RawData,
						apiOptions.EdxClientId,
						new TaxcomApiUserSettings
						{
							CryptographicMode = apiCryptographicMode.Value
						});
				})
				.AddTaxcomEdoApiLibrary();

			return services;
		}
	}
}
