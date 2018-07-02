using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Feminine,
	NominativePlural = "папки номенклатуры в 1с",
	Nominative = "папка номенклатуры в 1с")]
	public class Folder1c : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[StringLength(100)]
		[Required(ErrorMessage = "Название папки должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string code1c;
		[Display(Name = "Код 1с")]
		public virtual string Code1c {
			get { return code1c; }
			set { SetField(ref code1c, value, () => Code1c); }
		}

#endregion

		public Folder1c()
		{
		}
	}
}
