using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Providers;
using TaxcomEdoApi.Library.Services.Interfaces;
using TaxcomEdoApi.Library.Services.SignProcessors;

namespace TaxcomEdoApi.Library.Factories
{
	/// <inheritdoc/>
	public class SignProcessorFactory : ISignProcessorFactory
	{
		private readonly ISignFilenameProvider _signFilenameProvider;
		private readonly ICertificateSearcher _certificateSearcher;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public SignProcessorFactory(
			ISignFilenameProvider signFilenameProvider,
			ICertificateSearcher certificateSearcher,
			IServiceScopeFactory serviceScopeFactory)
		{
			_signFilenameProvider = signFilenameProvider ?? throw new ArgumentNullException(nameof(signFilenameProvider));
			_certificateSearcher = certificateSearcher ?? throw new ArgumentNullException(nameof(certificateSearcher));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}
		
		/// <inheritdoc/>
		public ISignProcessor CreateSignProcessor(SignMode mode)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			
			switch(mode)
			{
				case SignMode.NotSign:
					return new NullSignProcessor();
				case SignMode.UseSpecifiedCertificate:
					//TODO изменить инициализацию
					return new SpecifiedCertificateSignProcessor(
						scope.ServiceProvider.GetRequiredService<ILogger<SpecifiedCertificateSignProcessor>>(),
						_signFilenameProvider,
						_certificateSearcher);
				case SignMode.UseSpecifiedSignature:
					return new SpecifiedSignatureSignProcessor(_signFilenameProvider);
				default:
					throw new ArgumentException($"Неизвестный режим подписи документов : {mode}");
			}
		}
	}
}
