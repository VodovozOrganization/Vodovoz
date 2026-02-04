using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vodovoz.Application.Payments.OnlinePayments.Builders;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <summary>
	/// Парсер выписок Юкассы
	/// </summary>
	public class PaymentsFromYookassaParser : IPaymentByCardOnlineParser
	{
		private readonly IList<PaymentByCardOnline> _parsedPayments = new List<PaymentByCardOnline>();
		/// <summary>
		/// 
		/// </summary>
		private const string _shopVodovozString = "https://shop.vodovoz-spb.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _vodovozString = "https://vodovoz-spb.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _vodovozString2 = "https://www.vodovoz-spb.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _vodovozString3 = "vodovoz-spb.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _vodovozPromoString = "promo2.vodovoz-spb.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _shopVodovozUberserverString = "https://shopvodovoz.uberserver.ru";
		/// <summary>
		/// 
		/// </summary>
		private const string _shopKulerSale = "kuler-sale.ru";
		/// <summary>
		/// Выписка Мира напитков
		/// </summary>
		private const string _yookassaBeveragesWorld = "НЭК.338376.01";
		/// <summary>
		/// Выписка ВВ Восток
		/// </summary>
		private const string _yookassaVvEast = "НЭК.385952.01";

		public IEnumerable<PaymentByCardOnline> ParsedPayments => _parsedPayments;

		public void Parse(string filename)
		{
			using(var reader = new StreamReader(filename, Encoding.GetEncoding(1251)))
			{
				string line;
				var count = 0;
				(IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData = (null, null);
				
				while((line = reader.ReadLine()) != null)
				{
					count++;
					if(line == string.Empty)
					{
						continue;
					}

					if(count == 1)
					{
						var paymentFrom = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						registerData = TryGetRegisterFormat(paymentFrom[3].Trim('.', '/'));

						if(registerData.PaymentFrom != null)
						{
							continue;
						}

						if(paymentFrom.Length > 4)
						{
							registerData = TryGetRegisterFormat(paymentFrom[4].Trim('.', '/'));
						}
					}
					
					if(registerData.PaymentFrom is null || registerData.Columns is null)
					{
						throw new ArgumentException("Невозможно определить откуда оплата");
					}

					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);

					if(!Guid.TryParse(data[0], out Guid result))
					{
						continue;
					}

					var payment = DefaultYookassaPaymentByCardOnlineBuilder
						.Create(registerData)
						.Build(data);
					
					_parsedPayments.Add(payment);
				}
			}
		}
		
		private (IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) TryGetRegisterFormat(string data)
		{
			PaymentByCardOnlineFrom? paymentFrom = null;
			switch(data)
			{
				case _yookassaBeveragesWorld:
					return (BwYookassaOnlinePaymentRegisterColumns.Create(), PaymentByCardOnlineFrom.FromVodovozWebSite);
				case _yookassaVvEast:
					return (VvEastYookassaOnlinePaymentRegisterColumns.Create(), PaymentByCardOnlineFrom.FromVodovozWebSite);
				case _vodovozString:
				case _vodovozString2:
				case _vodovozPromoString:
				case _shopVodovozUberserverString:
					paymentFrom = PaymentByCardOnlineFrom.FromSMS;
					break;
				case _shopKulerSale:
					paymentFrom = PaymentByCardOnlineFrom.FromEShop;
					break;
				case _shopVodovozString:
				case _vodovozString3:
					paymentFrom =  PaymentByCardOnlineFrom.FromMobileApp;
					break;
				default:
					return (null, null);
			}
			
			return (DefaultYookassaOnlinePaymentRegisterColumns.Create(),  paymentFrom);
		}
	}
}
