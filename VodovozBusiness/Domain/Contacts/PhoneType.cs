using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "типы телефонов",
	Nominative = "тип телефона")]
	public class PhoneType : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Тип телефона")]
		public virtual string Name { 
			get => name;
			set => SetField(ref name, value, () => Name); 
		}

		private PhoneEnumType phoneEnumType;
		[Display(Name = "Дополнительный тип")]
		public virtual PhoneEnumType PhoneEnumType {
			get => phoneEnumType;
			set => SetField(ref phoneEnumType, value, () => PhoneEnumType);
		}

		public PhoneType()
		{
			Name = String.Empty;
		}
	}

	public enum PhoneEnumType
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts
	}

	public class PhoneEnumTypeStringType : NHibernate.Type.EnumStringType
	{
		public PhoneEnumTypeStringType() : base(typeof(PhoneEnumType))
		{
		}
	}
}
