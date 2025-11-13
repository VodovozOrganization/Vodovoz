using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Settings.Contacts;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	[HistoryTrace]
	public class Phone : Core.Domain.Contacts.PhoneEntity
	{
		private string _number;
		private string _digitsNumber;
		private PhoneType _phoneType;
		private string _comment;
		private bool _isArchive;
		private DeliveryPoint _deliveryPoint;
		private Counterparty _counterparty;
		private RoboAtsCounterpartyName _roboAtsCounterpartyName;
		private RoboAtsCounterpartyPatronymic _roboAtsCounterpartyPatronymic;
		private PhoneType _phoneType;
		private Employee _employee;

		public virtual new PhoneType PhoneType
		{
			get => _phoneType;
			set => SetField(ref _phoneType, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
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

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
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
					 + (string.IsNullOrWhiteSpace(Number) ? "" : " +7 " + Number)
					 + (string.IsNullOrWhiteSpace(Additional) ? "" : " доп." + Additional)
					 + (string.IsNullOrWhiteSpace(Comment) ? "" : $"\n[{Comment}]");
			}
		}

		#endregion

		/// <summary>
		/// Обязательно вызвать <see cref="Init(IContactSettings)"/> после вызова конструктора
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
			var phone = formatter.FormatString(number);
			_number = phone;

			formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			phone = formatter.FormatString(number);
			_digitsNumber = phone;

			_comment = comment;
		}

		public virtual Phone Init(IContactSettings contactsParameters)
		{
			Number = string.Empty;
			Additional = string.Empty;
			return this;
		}

		public override string ToString()
		{
			return "+7 " + Number;
		}

		public virtual string Title => $"{ ToString() }, { DeliveryPoint?.Title ?? Counterparty?.Name }";
	}
}
