using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы e-mail",
		Nominative = "тип e-mail")]
	public class EmailType : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "E-mail")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private EmailAdditionalType emailAdditionalType;
		[Display(Name = "Дополнительный тип")]
		public virtual EmailAdditionalType EmailAdditionalType {
			get => emailAdditionalType;
			set => SetField(ref emailAdditionalType, value, () => EmailAdditionalType);
		}
		#endregion

		public EmailType()
		{
			Name = String.Empty;
		}
	}

	public enum EmailAdditionalType
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts,
		[Display(Name = "Для счетов")]
		ForBills
	}

	public class EmailAdditionalTypeStringType : NHibernate.Type.EnumStringType
	{
		public EmailAdditionalTypeStringType() : base(typeof(EmailAdditionalType))
		{
		}
	}
}
