using EdoService.Library.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using QS.Report;
using Taxcom.Client.Api;
using TaxcomEdoApi.Config;
using TaxcomEdoApi.Converters;
using TaxcomEdoApi.Factories;
using TaxcomEdoApi.Services;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Tools.Orders;

namespace TaxcomEdoApi
{
	public static class TaxcomEdoApiExtensions
	{
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<WarrantOptions>(config.GetSection(WarrantOptions.Position))
				.Configure<TaxcomEdoApiOptions>(config.GetSection(TaxcomEdoApiOptions.Position));
			return services;
		}

		public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services.AddHostedService<AutoSendReceiveService>()
				.AddHostedService<ContactsUpdaterService>()
				.AddHostedService<DocumentFlowService>()

				.AddInfrastructure()

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
				.AddSingleton(provider =>
				{
					var apiOptions = provider.GetRequiredService<IOptions<TaxcomEdoApiOptions>>().Value;
					var certificate = provider.GetRequiredService<X509Certificate2>();

					return new Factory().CreateApi(
						apiOptions.BaseUrl,
						true,
						apiOptions.IntegratorId,
						certificate.RawData,
						apiOptions.EdxClientId);
				})
				.AddSingleton<IEdoUpdFactory, EdoUpdFactory>()
				.AddSingleton<IEdoBillFactory, EdoBillFactory>()
				.AddSingleton<IReportInfoFactory, DefaultReportInfoFactory>()
				.AddSingleton<PrintableDocumentSaver>()
				.AddSingleton<IParticipantDocFlowConverter, ParticipantDocFlowConverter>()
				.AddSingleton<IEdoContainerMainDocumentIdParser, EdoContainerMainDocumentIdParser>()
				.AddSingleton<IUpdProductConverter, UpdProductConverter>()
				.AddSingleton<IContactStateConverter, ContactStateConverter>();

			return services;
		}
	}
}
