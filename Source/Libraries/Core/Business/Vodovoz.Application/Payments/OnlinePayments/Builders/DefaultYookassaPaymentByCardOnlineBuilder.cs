using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments.Builders
{
	/// <summary>
	/// Билдер для создания онлайн платежей <see cref="PaymentByCardOnline"/>
	/// </summary>
	public class DefaultYookassaPaymentByCardOnlineBuilder : IPaymentByCardOnlineBuilder
	{
		private readonly CultureInfo _formatProvider;
		protected readonly (IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) RegisterData;
		protected readonly PaymentByCardOnline Payment;

		protected DefaultYookassaPaymentByCardOnlineBuilder(
			(IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom)? registerData)
		{
			RegisterData = registerData ?? throw new ArgumentNullException(nameof(registerData));
			Payment = new PaymentByCardOnline();

			_formatProvider = CultureInfo.CreateSpecificCulture("ru-RU");
			_formatProvider.NumberFormat.NumberDecimalSeparator = ".";
		}

		/// <summary>
		/// Добавление суммы платежа
		/// </summary>
		/// <returns></returns>
		protected internal virtual DefaultYookassaPaymentByCardOnlineBuilder Sum(string paymentSum)
		{
			if(!decimal.TryParse(paymentSum, NumberStyles.Any, _formatProvider.NumberFormat, out var sum))
			{
				sum = 0m;
			}

			Payment.PaymentRUR = sum;
			return this;
		}

		/// <summary>
		/// Добавление даты и времени платежа
		/// </summary>
		/// <returns></returns>
		protected internal virtual DefaultYookassaPaymentByCardOnlineBuilder DateAndTime(string dateTime)
		{
			Payment.DateAndTime = ParseDate(dateTime.Trim());
			return this;
		}
		
		/// <summary>
		/// Добавление номера платежа и источника
		/// </summary>
		/// <returns></returns>
		protected internal virtual DefaultYookassaPaymentByCardOnlineBuilder PaymentNumberAndSource(string number, PaymentByCardOnlineFrom paymentFrom)
		{
			int.TryParse(GetNumberFromDescription(number, ref paymentFrom), out var paymentNr);
			
			//Проверяем дополнительно здесь, т.к. по одной из касс прилетают оплаты трех форматов
			if(paymentNr < 1000000 && paymentFrom == PaymentByCardOnlineFrom.FromSMS)
			{
				paymentFrom = PaymentByCardOnlineFrom.FromVodovozWebSite;
			}
			
			Payment.PaymentNr = paymentNr;
			Payment.PaymentByCardFrom = paymentFrom;
			return this;
		}
		
		/// <summary>
		/// Добавление email
		/// </summary>
		/// <returns></returns>
		protected virtual DefaultYookassaPaymentByCardOnlineBuilder Email(string email)
		{
			Payment.Email = string.IsNullOrWhiteSpace(email) ? null : GetEmailFromDescription(email);
			return this;
		}

		/// <summary>
		/// Возврат созданного онлайн платежа
		/// </summary>
		/// <returns></returns>
		public virtual PaymentByCardOnline Build(string[] data)
		{
			Sum(data[RegisterData.Columns.PaymentSumColumn])
				.DateAndTime(data[RegisterData.Columns.DateAndTimeColumn])
				.PaymentNumberAndSource(data[RegisterData.Columns.PaymentNumberColumn], RegisterData.PaymentFrom.Value)
				.Email(data[RegisterData.Columns.EmailColumn.Value]);
			
			Payment.PaymentStatus = PaymentStatus.CONFIRMED;
			return Payment;
		}

		public static IPaymentByCardOnlineBuilder Create((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData)
		{
			switch (registerData.Columns)
			{
				case BwYookassaOnlinePaymentRegisterColumns _:
					return new BwYookassaPaymentByCardOnlineBuilder(registerData);
				case VvEastYookassaOnlinePaymentRegisterColumns _:
					return new VvEastYookassaPaymentByCardOnlineBuilder(registerData);
				default:
					return new DefaultYookassaPaymentByCardOnlineBuilder(registerData);
			}
		}
		
		private string GetNumberFromDescription(string description, ref PaymentByCardOnlineFrom paymentFrom)
		{
			var pattern1 = @"№([0-9]{1,})";
			var pattern2 = @"№[\s|-]([0-9]{1,})";
			var pattern3 = @"[\s|-]([0-9]{1,})";

			MatchCollection matches;

			switch(paymentFrom)
			{
				case PaymentByCardOnlineFrom.FromEShop:
					matches = Regex.Matches(description, pattern1);
					return matches[matches.Count - 1].Groups[1].Value;
				case PaymentByCardOnlineFrom.FromVodovozWebSite:
					matches = Regex.Matches(description, pattern3);
					return matches[0].Groups[1].Value;
				case PaymentByCardOnlineFrom.FromMobileApp:
					matches = Regex.Matches(description, pattern2);
					return matches[0].Groups[1].Value;
				case PaymentByCardOnlineFrom.FromSMS:
					matches = Regex.Matches(description, pattern2); // Проверяем по паттерну (№ заказа)

					if(matches.Count != 0)
					{
						return matches[matches.Count - 1].Groups[1].Value;
					}

					// Если нет - проверяем по 1 паттерну(№заказа)
					matches = Regex.Matches(description, pattern1);

					if(matches.Count != 0)
					{
						paymentFrom = PaymentByCardOnlineFrom.FromEShop;
						return matches[matches.Count - 1].Groups[1].Value;
					}
					
					// Если снова нет - проверяем по 3 паттерну(число_пробел_число, оплата с сайта, берем первое число)
					matches = Regex.Matches(description, pattern3);
					paymentFrom = PaymentByCardOnlineFrom.FromVodovozWebSite;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return matches[0].Groups[1].Value;
		}

		private DateTime ParseDate(string dateStr)
		{
			if(DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime result))
			{
				return result;
			}

			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm:ss", null, DateTimeStyles.None, out result))
			{
				return result;
			}

			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out result))
			{
				return result;
			}

			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy H:mm", null, DateTimeStyles.None, out result))
			{
				return result;
			}

			throw new FormatException("Неправильный формат выгрузки");
		}
		
		private string GetEmailFromDescription(string description)
		{
			var pattern = @"[0-9a-zA-Z]+[\.a-zA-Z0-9_-]*[a-zA-Z0-9]+@[a-zA-Z]+\.[a-zA-Z]+";
			var matches = Regex.Matches(description, pattern);

			if(matches.Count <= 0)
			{
				return string.Empty;
			}

			var str = new string[matches.Count];

			for(var i = 0; i < matches.Count; i++)
			{
				str[i] = matches[i].Value;
			}

			return str.FirstOrDefault();
		}
	}
}
