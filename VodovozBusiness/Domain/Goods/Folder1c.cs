using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject(Gender = GrammaticalGender.Feminine,
	NominativePlural = "папки номенклатуры в 1с",
	Nominative = "папка номенклатуры в 1с")]
	public class Folder1c : DomainTreeNodeBase<Folder1c>
	{
		#region Свойства

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
		[Required(ErrorMessage = "Код 1с должен быть заполнен.")]
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
