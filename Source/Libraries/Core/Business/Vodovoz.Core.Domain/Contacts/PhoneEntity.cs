﻿using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
		private PhoneTypeEntity _phoneType;

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

		[Display(Name = "Тип телефона")]
		public virtual PhoneTypeEntity PhoneType
		{
			get => _phoneType;
			set => SetField(ref _phoneType, value);
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
	}
}
