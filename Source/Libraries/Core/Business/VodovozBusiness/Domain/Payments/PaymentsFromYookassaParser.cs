using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
	public class PaymentsFromYookassaParser
	{
		private readonly string docPath;
		private const string _shopVodovozString = "https://shop.vodovoz-spb.ru";
		private const string _vodovozString = "https://vodovoz-spb.ru";
		private const string _vodovozString2 = "https://www.vodovoz-spb.ru";
		private const string _vodovozString3 = "vodovoz-spb.ru";
		private const string _vodovozPromoString = "promo2.vodovoz-spb.ru";
		private const string _shopVodovozUberserverString = "https://shopvodovoz.uberserver.ru";
		private const string _shopKulerSale = "kuler-sale.ru";

		public List<PaymentByCardOnline> PaymentsFromYookassa { get; set; } = new List<PaymentByCardOnline>();

		public PaymentsFromYookassaParser(string docPath)
		{
			this.docPath = docPath;
		}

		public void Parse()
		{
			using(var reader = new StreamReader(docPath, Encoding.GetEncoding(1251)))
			{
				string line;
				var count = 0;
				var paymentByCardFrom = PaymentByCardOnlineFrom.FromVodovozWebSite;
				
				while((line = reader.ReadLine()) != null)
				{
					count++;
					if(line == string.Empty) continue;
					
					if (count == 1) {
						var paymentFrom = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
						TryMatchPaymentFrom(paymentFrom[3].Trim('.', '/'), ref paymentByCardFrom);
					}
					
					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
				
					if (Guid.TryParse(data[0], out Guid result)) {
						var payment = new PaymentByCardOnline(data, paymentByCardFrom);
						PaymentsFromYookassa.Add(payment);
					}
				}
			}
		}

		private void TryMatchPaymentFrom(string data, ref PaymentByCardOnlineFrom paymentByCardFrom)
		{
			switch(data)
			{
				case _vodovozString:
				case _vodovozString2:
				case _vodovozPromoString:
				case _shopVodovozUberserverString:
					paymentByCardFrom = PaymentByCardOnlineFrom.FromSMS;
					break;
				case _shopKulerSale:
					paymentByCardFrom = PaymentByCardOnlineFrom.FromEShop;
					break;
				case _shopVodovozString:
				case _vodovozString3:
					paymentByCardFrom = PaymentByCardOnlineFrom.FromMobileApp;
					break;
				default:
					throw new ArgumentException("Невозможно определить откуда оплата.");
			}
		}
	}
}
