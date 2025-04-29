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
		private const string _yookassaBeveragesWorld = "НЭК.338376.01";

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
				var newFormat = false;
				
				while((line = reader.ReadLine()) != null)
				{
					count++;
					if(line == string.Empty) continue;
					
					if (count == 1) {
						var paymentFrom = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
						var parsedPaymentFrom = TryMatchPaymentFrom(paymentFrom[3].Trim('.', '/'));

						if(parsedPaymentFrom != null)
						{
							paymentByCardFrom = parsedPaymentFrom.Value;
							continue;
						}
						
						if(paymentFrom.Length > 4)
						{
							parsedPaymentFrom = TryMatchPaymentFrom(paymentFrom[4].Trim('.', '/'));
						}

						if(parsedPaymentFrom is null)
						{
							throw new ArgumentException("Невозможно определить откуда оплата.");
						}
						
						paymentByCardFrom = parsedPaymentFrom.Value;

						if(parsedPaymentFrom.Value == PaymentByCardOnlineFrom.FromVodovozWebSite)
						{
							newFormat = true;
						}
					}
					
					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
				
					if (Guid.TryParse(data[0], out Guid result)) {
						var payment = new PaymentByCardOnline(data, paymentByCardFrom, newFormat);
						PaymentsFromYookassa.Add(payment);
					}
				}
			}
		}

		private PaymentByCardOnlineFrom? TryMatchPaymentFrom(string data)
		{
			switch(data)
			{
				case _yookassaBeveragesWorld:
					return PaymentByCardOnlineFrom.FromVodovozWebSite;
				case _vodovozString:
				case _vodovozString2:
				case _vodovozPromoString:
				case _shopVodovozUberserverString:
					return PaymentByCardOnlineFrom.FromSMS;
				case _shopKulerSale:
					return PaymentByCardOnlineFrom.FromEShop;
				case _shopVodovozString:
				case _vodovozString3:
					return PaymentByCardOnlineFrom.FromMobileApp;
				default:
					return null;
			}
		}
	}
}
