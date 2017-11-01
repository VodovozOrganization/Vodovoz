using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
	NominativePlural = "группы комментариев",
	Nominative = "группа комментариев")]
	public class CommentsGroups : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string color;

		[Display(Name = "Цвет")]
		public virtual string Color {
			get { return color; }
			set { SetField(ref color, value, () => Color); }
		}

		#endregion


		public CommentsGroups()
		{
			Name = String.Empty;
			Color = String.Empty;
		}
	}
}
