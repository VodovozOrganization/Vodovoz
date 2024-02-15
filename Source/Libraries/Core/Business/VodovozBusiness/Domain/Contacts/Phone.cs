using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Client;
using Vodovoz.Services;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	[HistoryTrace]
	public class Phone : Core.Domain.Contacts.PhoneEntity
	{
		#region Свойства

		private DeliveryPoint _deliveryPoint;
		private Counterparty _counterparty;
		private RoboAtsCounterpartyName _roboAtsCounterpartyName;
		private RoboAtsCounterpartyPatronymic _roboAtsCounterpartyPatronymic;

		private PhoneType phoneType;
		public virtual PhoneType PhoneType
		{
			get => phoneType;
			set => SetField(ref phoneType, value);
		}

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Имя контрагента")]
		public virtual RoboAtsCounterpartyName RoboAtsCounterpartyName
		{
			get => _roboAtsCounterpartyName;
			set => SetField(ref _roboAtsCounterpartyName, value);
		}

		[Display(Name = "Отчество контрагента")]
		public virtual RoboAtsCounterpartyPatronymic RoboAtsCounterpartyPatronymic
		{
			get => _roboAtsCounterpartyPatronymic;
			set => SetField(ref _roboAtsCounterpartyPatronymic, value);
		}

		#endregion

		#region Рассчетные

		public virtual string LongText
		{
			get
			{
				return PhoneType?.Name
					 + (String.IsNullOrWhiteSpace(Number) ? "" : " +7 " + Number)
					 + (String.IsNullOrWhiteSpace(Additional) ? "" : " доп." + Additional)
					 + (String.IsNullOrWhiteSpace(Comment) ? "" : $"\n[{Comment}]");
			}
		}

		#endregion

		/// <summary>
		/// Обязательно вызвать <see cref="Init(IContactParametersProvider)"/> после вызова конструктора
		/// </summary>
		public Phone()
		{
		}

		/// <summary>
		/// Конструктор ,который преобразует любой вид телефона к стандартному виду
		/// Формат:
		/// 	Phone.Number = "(XXX) XXX - XX - XX" [здесь есть пробелы!]
		/// 	Phone.DigitsNumber = "XXXXXXXXXX" [10 цифр , без пробелов и без +7/7 ]
		/// 	Phone.Additional = [до 10 цифр]
		/// 	Phone.Name = [понятно]
		/// 	Phone.LonqText = [понятно]
		/// </summary>
		/// <param name="number">Number.</param>
		public Phone(string number, string comment = null)
		{
			var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
			string phone = formatter.FormatString(number);
			_number = phone;

			formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			phone = formatter.FormatString(number);
			_digitsNumber = phone;

			_comment = comment;
		}

		public virtual Phone Init(IContactParametersProvider contactsParameters)
		{
			Number = String.Empty;
			Additional = String.Empty;
			return this;
		}

		public override string ToString()
		{
			return "+7 " + Number;
		}

		public virtual bool IsValidPhoneNumber => IsValidPhoneNumberFormat();

		private bool IsValidPhoneNumberFormat()
		{
			if(Regex.IsMatch(_digitsNumber, "^[3 4 8 9]{1}[0-9]{9}"))
			{
				return true;
			}

			return false;
		}

		public virtual string Title => $"{ ToString() }, { DeliveryPoint?.Title ?? Counterparty?.Name }";
	}
}
