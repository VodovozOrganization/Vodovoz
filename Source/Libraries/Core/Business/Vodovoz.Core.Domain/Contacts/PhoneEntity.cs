using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	[HistoryTrace]
	public class PhoneEntity : PropertyChangedBase, IDomainObject
	{
		protected int _id;
		protected string _number;
		protected string _digitsNumber;
		protected string _additional;
		protected string _comment;
		protected bool _isArchive;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Номер")]
		public virtual string Number
		{
			get => _number;
			set
			{
				var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
				string phone = formatter.FormatString(value);
				SetField(ref _number, phone);
				DigitsNumber = value;
			}
		}

		[Display(Name = "Только цифры")]
		public virtual string DigitsNumber
		{
			get => _digitsNumber;
			protected set
			{
				var formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
				string phone = formatter.FormatString(value);
				SetField(ref _digitsNumber, phone, () => DigitsNumber);
			}
		}

		[Display(Name = "Добавочный")]
		public virtual string Additional
		{
			get => _additional;
			set => SetField(ref _additional, value);
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
	}
}
