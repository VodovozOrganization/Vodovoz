using System;
using Vodovoz.Settings.CashReceipt;

namespace CashReceiptApi.Client
{
	public class CashReceiptClientChannelFactory
	{
		private readonly ICashReceiptSettings _cashReceiptSettings;

		public CashReceiptClientChannelFactory(ICashReceiptSettings cashReceiptSettings)
		{
			_cashReceiptSettings = cashReceiptSettings ?? throw new ArgumentNullException(nameof(cashReceiptSettings));
		}

		public CashReceiptClientChannel OpenChannel(string url = null)
		{
			string serviceUrl = url ?? _cashReceiptSettings.CashReceiptApiUrl;
			return new CashReceiptClientChannel(serviceUrl, _cashReceiptSettings.CashReceiptApiKey);
		}
	}
}
