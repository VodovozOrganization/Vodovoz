using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "цвета оборудования",
		Nominative = "цвет оборудования")]
	public class EquipmentColors : PropertyChangedBase, IDomainObject
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

		public EquipmentColors ()
		{
			Name = String.Empty;
		}
	}
}
