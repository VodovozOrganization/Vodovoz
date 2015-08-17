using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "смена доставки",
		Nominative = "смена доставки")]
	public class DeliveryShift : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(45)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		public DeliveryShift ()
		{
			Name = String.Empty;
		}
	}
}

