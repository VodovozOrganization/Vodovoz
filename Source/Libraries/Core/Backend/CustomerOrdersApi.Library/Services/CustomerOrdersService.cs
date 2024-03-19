using System;
using CustomerOrdersApi.Library.Dto.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.Services
{
	public class CustomerOrdersService : ICustomerOrdersService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly ISignatureManager _signatureManager;
		private readonly IConfigurationSection _signaturesSection;

		public CustomerOrdersService(
			ILogger<CustomerOrdersService> logger,
			ISignatureManager signatureManager,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));

			_signaturesSection = configuration.GetSection("Signatures");
		}

		public bool ValidateSignature(OnlineOrderInfoDto onlineOrderInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(onlineOrderInfoDto.Source);
			
			return _signatureManager.Validate(
				onlineOrderInfoDto.Signature,
				new SignatureParams
				{
					OrderId = onlineOrderInfoDto.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)onlineOrderInfoDto.OrderSum * 100,
					ShopId = (int)onlineOrderInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		private string GetSourceSign(Source source)
		{
			return _signaturesSection.GetValue<string>(source.ToString());
		}
	}
}
