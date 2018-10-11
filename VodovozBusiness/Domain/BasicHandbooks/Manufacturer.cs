using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "производители оборудования",
		Nominative = "производитель оборудования")]
	public class Manufacturer : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public Manufacturer ()
		{
			Name = String.Empty;
		}
	}
}
