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

		private PhoneAdditionalType phoneAdditionalType;
		[Display(Name = "Дополнительный тип")]
		public virtual PhoneAdditionalType PhoneAdditionalType {
			get => phoneAdditionalType;
			set => SetField(ref phoneAdditionalType, value, () => PhoneAdditionalType);
		}

		public PhoneType()
		{
			Name = String.Empty;
		}
	}

	public enum PhoneAdditionalType
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts
	}

	public class PhoneAdditionalTypeStringType : NHibernate.Type.EnumStringType
	{
		public PhoneAdditionalTypeStringType() : base(typeof(PhoneAdditionalType))
		{
		}
	}
}
