using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение")]
	public class Subdivision : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string name;

		[Display(Name = "Название подразделения")]
		[Required (ErrorMessage = "Название подразделения должно быть заполнено.")]
		public virtual string Name
		{
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		#endregion

		public Subdivision()
		{
		}
	}
}

