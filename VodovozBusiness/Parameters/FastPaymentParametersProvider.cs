using System;

namespace Vodovoz.Parameters
{
	public class FastPaymentParametersProvider : IFastPaymentParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public FastPaymentParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int GetQRLifetime => _parametersProvider.GetIntValue("fast_payment_qr_lifetime");
		public int GetPayUrlLifetime => _parametersProvider.GetIntValue("fast_payment_pay_url_lifetime");
		public string GetFastPaymentBackUrl => _parametersProvider.GetStringValue("fast_payment_back_url");
		public string GetFastPaymentApiBaseUrl => _parametersProvider.GetStringValue("fast_payment_api_base_url");
		public string GetAvangardFastPayBaseUrl => _parametersProvider.GetStringValue("avangard_fast_pay_base_url");
		public string GetVodovozFastPayBaseUrl => _parametersProvider.GetStringValue("vodovoz_fast_pay_base_url");
	}
}
