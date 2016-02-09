using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "статьи расхода",
		Nominative = "статья расхода")]
	public class ExpenseCategory : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		ExpenseCategory parent;

		[Display (Name = "Родитель")]
		public virtual ExpenseCategory Parent {
			get { return parent; }
			set { SetField (ref parent, value, () => Parent); }
		}

		IList<ExpenseCategory> childs;

		[Display (Name = "Дочерние категории")]
		public virtual IList<ExpenseCategory> Childs {
			get { return childs; }
			set { SetField (ref childs, value, () => Childs); }
		}

		string name;

		[Required (ErrorMessage = "Название статьи должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public ExpenseCategory ()
		{
			Name = String.Empty;
		}
	}
}

