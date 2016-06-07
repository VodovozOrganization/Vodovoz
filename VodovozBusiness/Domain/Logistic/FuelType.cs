using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "виды топлива",
		Nominative = "вид топлива")]
	public class FuelType : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(20)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		public FuelType ()
		{
			Name = String.Empty;
		}
	}
}

