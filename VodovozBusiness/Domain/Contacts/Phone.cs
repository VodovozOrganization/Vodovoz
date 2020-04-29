using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
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
				if(SetField(ref number, value, () => Number))
					DigitsNumber = Regex.Replace(Number, "[^0-9]", "");
			}
		}

		private string digitsNumber;
		[Display(Name = "Только цифры")]
		public virtual string DigitsNumber {
			get => digitsNumber; 
			protected set { SetField(ref digitsNumber, value, () => DigitsNumber); }
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
