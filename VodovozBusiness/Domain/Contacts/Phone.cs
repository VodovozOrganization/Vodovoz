using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using QS.Utilities.Numeric;
using Vodovoz.Services;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	public class Phone : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string number;
		public virtual string Number {
			get => number;
			set {
				var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
				string phone = formatter.FormatString(value);
				SetField(ref number, phone, () => Number);
				DigitsNumber = value;
			}
		}

		private string digitsNumber;
		[Display(Name = "Только цифры")]
		public virtual string DigitsNumber {
			get => digitsNumber; 
			protected set {
				var formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
				string phone = formatter.FormatString(value);
				SetField(ref digitsNumber, phone, () => DigitsNumber);
			}
		}

		public virtual string Additional { get; set; }

		private PhoneType phoneType;
		public virtual PhoneType PhoneType {
			get => phoneType;
			set { SetField(ref phoneType, value, () => PhoneType); }
		}

		private string name;
		[Display(Name = "Имя")]
		public virtual string Name {
			get => name;
			set { SetField(ref name, value, () => Name); }
		}

		#endregion

		#region Рассчетные

		public virtual string LongText {
			get {
				return PhoneType?.Name
					 + (String.IsNullOrWhiteSpace(Number) ? "" : " +7 " + Number)
					 + (String.IsNullOrWhiteSpace(Additional) ? "" : " доп." + Additional)
					 + (String.IsNullOrWhiteSpace(Name) ? "" : String.Format(" [{0}]", Name));
			}
		}

		#endregion

		/// <summary>
		/// Обязательно вызвать <see cref="Init(IContactsParameters)"/> после вызова конструктора
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
		public Phone(string number,string name = null)
		{
			var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
			string phone = formatter.FormatString(number);
			this.number = phone;

			formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			phone = formatter.FormatString(number);
			this.digitsNumber = phone;

			this.name = name;
		}

		public virtual Phone Init(IContactsParameters contactsParameters)
		{
			//I-2566 Отключено за ненадобностью
			//if(String.IsNullOrWhiteSpace(contactsParameters.DefaultCityCode))
			//	Number = String.Empty;
			//else
			//Number = String.Format("({0})", contactsParameters.DefaultCityCode);

			Number = String.Empty;
			Additional = String.Empty;
			return this;
		}

		public override string ToString()
		{
			return "+7 " + Number;
		}
	}
}
