using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы e-mail",
		Nominative = "тип e-mail")]
	[EntityPermission]
	public class EmailTypeEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;
		private EmailPurpose _emailPurpose;

		#region Свойства

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "E-mail")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Дополнительный тип")]
		public virtual EmailPurpose EmailPurpose
		{
			get => _emailPurpose;
			set => SetField(ref _emailPurpose, value);
		}
		#endregion

		public EmailTypeEntity()
		{
			Name = String.Empty;
		}
	}
}
