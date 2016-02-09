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
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		ExpenseCategory parent;

		[Display (Name = "Родитель")]
		public virtual ExpenseCategory Parent {
			get { return parent; }
			set {
				if(NHibernate.NHibernateUtil.IsInitialized(Parent))
				{
					if (parent != null)
						parent.Childs.Remove(this);
					if (value != null && value.CheckCircle(this))
					{
						logger.Warn("Родитель не назначен, так как возникает зацикливание дерева.");
						return;
					}
				}
					
				SetField (ref parent, value, () => Parent); 

				if (parent != null && NHibernate.NHibernateUtil.IsInitialized(Parent))
					parent.Childs.Add(this);
			}
		}

		IList<ExpenseCategory> childs = new List<ExpenseCategory>();

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

		#region Внутренние

		private bool CheckCircle(ExpenseCategory category)
		{
			if (Parent == null)
				return false;
			
			if (Parent == category)
				return true;

			return Parent.CheckCircle(category);
		}
		#endregion

	}
}

