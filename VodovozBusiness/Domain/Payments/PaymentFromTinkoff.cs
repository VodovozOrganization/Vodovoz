using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи",
		Nominative = "платёж",
		Prepositional = "платеже",
		PrepositionalPlural = "платежах"
	)]
	public class PaymentFromTinkoff : BusinessObjectBase<PaymentFromTinkoff>, IDomainObject
	{
		#region конструкторы

		public PaymentFromTinkoff() { }

		public PaymentFromTinkoff(string[] data) => CreateInstance(data);

		/// <summary>
		/// Создать объект из массива строк
		/// </summary>
		/// <param name="data">Поля объекта</param>
		void CreateInstance(string[] data)
		{
			//Для разбора дат и сумм.
			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";

			switch(data[0]) {
				case "3DS_CHECKING":
					paymentStatus = PaymentStatus.CHECKING;
					break;
				default:
					if(!Enum.TryParse(data[0], out paymentStatus))
						paymentStatus = PaymentStatus.Unacceptable;
					//else
						//Selectable = Selected = PaymentStatus == PaymentStatus.CONFIRMED;
					break;
			}

			DateAndTime = DateTime.ParseExact(
				data[1],
				"yyyy-MM-dd HH:mm:ss",
				null
			);

			if(!Int32.TryParse(data[2], out paymentNr))
				paymentNr = 0;

			if(!Decimal.TryParse(data[3], NumberStyles.AllowDecimalPoint, culture.NumberFormat, out paymentRUR))
				paymentRUR = 0m;

			Email = data[4];

			Phone = data[5];

			Shop = data[6];
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

		string email = String.Empty;
		[Display(Name = "Адрес электронной почты")]
		public virtual string Email {
			get => email;
			set => SetField(ref email, value, () => Email);
		}

		string phone = String.Empty;
		[Display(Name = "Номер телефона")]
		public virtual string Phone {
			get => phone;
			set => SetField(ref phone, value, () => Phone);
		}

		string shop = String.Empty;
		[Display(Name = "Магазин какой-то")]
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
	}

	/// <summary>
	/// Статус оплаты.
	/// Все кроме "Unacceptable" из выгрузки и их не редактировать.
	/// </summary>
	public enum PaymentStatus
	{
		[Display(Name = "Новый")]
		NEW = 0,
		[Display(Name = "Подтверждён")]
		CONFIRMED = 1,
		[Display(Name = "Показанная форма")]
		FORM_SHOWED = 2,
		[Display(Name = "Ошибка авторизации")]
		AUTH_FAIL = 3,
		[Display(Name = "3DS_CHECKING")]
		CHECKING = 4,
		[Display(Name = "Превышение срока")]
		DEADLINE_EXPIRED = 5,
		[Display(Name = "Отвергнут")]
		REJECTED = 6,
		[Display(Name = "Отсутствует в ДВ")]
		Unacceptable = 1024
	}
}