using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Store
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "фуры",
		Nominative = "фура")]
	public class MovementWagon : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название фуры должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public MovementWagon ()
		{
			Name = String.Empty;
		}
	}
}