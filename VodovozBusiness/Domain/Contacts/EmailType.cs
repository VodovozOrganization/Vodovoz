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
		#endregion

		public EmailType()
		{
			Name = String.Empty;
		}
	}
}
