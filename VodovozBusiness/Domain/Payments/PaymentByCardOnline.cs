using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи по карте",
		Nominative = "платёж по карте",
		Prepositional = "платеже по карте",
		PrepositionalPlural = "платежах по карте"
	)]
	public class PaymentByCardOnline : BusinessObjectBase<PaymentByCardOnline>, IDomainObject
	{
		#region конструкторы

		public PaymentByCardOnline() { }

		public PaymentByCardOnline(string[] data, bool fromTinkoff)
		{
			if (fromTinkoff) {
				CreateInstanceFromTinkoff(data);
			}
			else {
				CreateInstanceFromYookassa(data);
			}
		} 

		/// <summary>
		/// Создать объект из массива строк
		/// </summary>
		/// <param name="data">Поля объекта</param>
		void CreateInstanceFromTinkoff(string[] data)
		{
			//Для разбора дат и сумм.
			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";

			if(!int.TryParse(data[0], out paymentNr))
				paymentNr = 0;

			DateAndTime = ParseDate(data[1]);

			Shop = data[2];

			if(!decimal.TryParse(data[3], NumberStyles.AllowDecimalPoint, culture.NumberFormat, out paymentRUR))
				paymentRUR = 0m;

			switch(data[4]) {
				case "3DS_CHECKING":
					paymentStatus = PaymentStatus.CHECKING;
					break;
				case "3DS_CHECKED":
					paymentStatus = PaymentStatus.CHECKED;
					break;
				default:
					if(!Enum.TryParse(data[4].ToUpper(), out paymentStatus))
						paymentStatus = PaymentStatus.Unacceptable;
					break;
			}

			Email = data[6];

			Phone = data[7];
		}
		
		void CreateInstanceFromYookassa(string[] data)
		{
			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";
			
			if(!decimal.TryParse(data[1].Trim(), NumberStyles.AllowDecimalPoint, culture.NumberFormat, out paymentRUR))
				paymentRUR = 0m;
			
			DateAndTime = ParseDate(data[4].Trim());

			if(!int.TryParse(GetNumberFromDescription(data[6]), out paymentNr))
				paymentNr = 0;

			PaymentStatus = PaymentStatus.CONFIRMED;

			Email = GetEmailFromDescription(data[6]);
		}

		private DateTime ParseDate(string dateStr)
		{
			if(DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime result)) {
				return result;
			}
			
			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm:ss", null, DateTimeStyles.None, out result)) {
				return result;
			}

			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out result)) {
				return result;
			}

			if(DateTime.TryParseExact(dateStr, "dd.MM.yyyy H:mm", null, DateTimeStyles.None, out result)) {
				return result;
			}

			throw new FormatException("Не правильный формат выгрузки");
		}

		#endregion

		#region свойства для мапинга

		public virtual int Id { get; set; }

		PaymentStatus paymentStatus;
		[Display(Name = "Статус оплаты")]
		public virtual PaymentStatus PaymentStatus {
			get => paymentStatus;
			set => SetField(ref paymentStatus, value, () => PaymentStatus);
		}

		DateTime dateAndTime;
		[Display(Name = "Дата и время операции")]
		public virtual DateTime DateAndTime {
			get => dateAndTime;
			set => SetField(ref dateAndTime, value, () => DateAndTime);
		}

		int paymentNr;
		[Display(Name = "Номер операции")]
		public virtual int PaymentNr {
			get => paymentNr;
			set => SetField(ref paymentNr, value, () => PaymentNr);
		}

		decimal paymentRUR;
		[Display(Name = "Сумма операции")]
		public virtual decimal PaymentRUR {
			get => paymentRUR;
			set => SetField(ref paymentRUR, value, () => PaymentRUR);
		}

		string email = string.Empty;
		[Display(Name = "Адрес электронной почты")]
		public virtual string Email {
			get => email;
			set => SetField(ref email, value, () => Email);
		}

		string phone = string.Empty;
		[Display(Name = "Номер телефона")]
		public virtual string Phone {
			get => phone;
			set => SetField(ref phone, value, () => Phone);
		}

		string shop = string.Empty;
		[Display(Name = "Магазин")]
		public virtual string Shop {
			get => shop;
			set => SetField(ref shop, value, () => Shop);
		}

		#endregion

		bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value, () => Selected);
		}

		public virtual bool Selectable { get; set; }
		public virtual bool IsDuplicate { get; set; }
		public virtual string Color { get; set; }
		
		private string GetNumberFromDescription(string description)
		{
			string pattern = @"(\s|№)([0-9]{1,})";

			var matches = Regex.Matches(description, pattern);
			string[] str = new string[matches.Count];

			for(int i = 0; i < matches.Count; i++) {
				str[i] = matches[i].Groups[2].Value;
			}
			
			return str.FirstOrDefault();
		}

		private string GetEmailFromDescription(string description)
		{
			string pattern = @"[a-zA-Z]+[\.a-zA-Z0-9_-]*[a-zA-Z0-9]+@[a-zA-Z]+\.[a-zA-Z]+";

			var matches = Regex.Matches(description, pattern);

			if (matches.Count > 0)
			{
				string[] str = new string[matches.Count];

				for (int i = 0; i < matches.Count; i++)
				{
					str[i] = matches[i].Value;
				}

				return str.FirstOrDefault();
			}

			return string.Empty;
		}
	}

	/// <summary>
	/// Статус оплаты.
	/// Все кроме "Unacceptable" из выгрузки и их не редактировать.
	/// </summary>
	public enum PaymentStatus
	{
		[Display(Name = "Платёж создан")]
		NEW = 0,
		[Display(Name = "Отмена платежа")]
		CANCELED = 1,
		[Display(Name = "Перенаправление на страницу оплаты")]
		FORMSHOWED = 2,
		[Display(Name = "Истек срок платежа")]
		DEADLINE_EXPIRED = 3,
		[Display(Name = "Система начала обработку оплаты платежа")]
		AUTHORIZING = 4,
		[Display(Name = "Покупатель начал аутентификацию по 3-D Secure")]
		CHECKING = 5,
		[Display(Name = "Покупатель завершил проверку 3-D Secure")]
		CHECKED = 6,
		[Display(Name = "Ошибка платежа. Остались попытки оплаты")]
		AUTH_FAIL = 7,
		[Display(Name = "Средства заблокированы, но не списаны")]
		AUTHORIZED = 8,
		[Display(Name = "Начало отмены блокировки средств")]
		REVERSING = 9,
		[Display(Name = "Денежные средства разблокированы")]
		REVERSED = 10,
		[Display(Name = "Начало списания денежных средств")]
		CONFIRMING = 11,
		[Display(Name = "Денежные средства успешно списаны")]
		CONFIRMED = 12,
		[Display(Name = "Начало возврата денежных средств")]
		REFUNDING = 13,
		[Display(Name = "Произведен частичный возврат денежных средств")]
		PARTIAL_REFUNDED = 14,
		[Display(Name = "Произведен возврат денежных средств")]
		REFUNDED = 15,
		[Display(Name = "Ошибка платежа. Истекли попытки оплаты")]
		REJECTED = 16,
		[Display(Name = "Отсутствует в ДВ")]
		Unacceptable = 1024,
	}
}