using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Cash
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "статьи расхода",
		Nominative = "статья расхода")]
	[EntityPermission]
	[HistoryTrace]
	public class ExpenseCategory : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public ExpenseCategory ()
		{
			Name = String.Empty;
		}
		
		
		#region Свойства
		
        public virtual int Id { get; }

        public virtual string Title => $"{Name}";

		string name;
		[Required (ErrorMessage = "Название статьи должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}
        
        
		ExpenseInvoiceDocumentType expenseDocumentType;
		/// <summary>
		/// Тип расходного ордера для которого возможно будет выбрать эту категорию
		/// </summary>
		[Required(ErrorMessage = "Должно быть заполнен тип расходного ордера.")]
		[Display(Name = "Тип расходного ордера")]
		public virtual ExpenseInvoiceDocumentType ExpenseDocumentType {
			get { return expenseDocumentType; }
			set { SetField(ref expenseDocumentType, value, () => ExpenseDocumentType); }
		}
		
		Subdivision subdivision;
        [Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField (ref subdivision, value); }
		}
		
		private bool isArchive;
		[Display(Name = "Категория архивирована")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}
		
		private string numbering;
		[Display(Name = "Нумерация")]
		public virtual string Numbering {
			get => numbering;
			set => SetField(ref numbering, value);
		}


        #region Levels

        // private List<ExpenseCategory> expenseCategories = null;
        
        // private void fillExpenseCategoriesList()
        // {
	       //  expenseCategories = new List<ExpenseCategory>();
	       //  
	       //  ExpenseCategory iter = this;
	       //  while (iter.Parent != null)
	       //  {
		      //   expenseCategories.Insert(0, iter);
		      //   iter = iter.Parent;
	       //  }
        // }
        
        // public virtual string Level5 => GetLevel(5);
        // public virtual string Level4 => GetLevel(4);
        // public virtual string Level3 => GetLevel(3);
        // public virtual string Level2 => GetLevel(2);
        // public virtual string Level1 => GetLevel(1);

        // private string GetLevel(int n)
        // {
	       //  if (expenseCategories == null) fillExpenseCategoriesList();
	       //  
	       //  if (expenseCategories.Count < n)
		      //   return "";
	       //  else
		      //   return expenseCategories[n-1].Name;
        // }

        #endregion //Levels
        
		#region ParentChilds
		private ExpenseCategory parent;
		[Display(Name = "Родительская группа")]
		public virtual ExpenseCategory Parent {
			get => parent;
			set => SetField(ref parent, value);
		}
		
		private IList<ExpenseCategory> childs;
		[Display(Name = "Дочерние группы")]
		public virtual IList<ExpenseCategory> Childs {
			get => childs;
			set => SetField(ref childs, value);
		}
        #endregion
		
		#endregion // свойства

		#region Функции

		public virtual void SetIsArchiveRecursively(bool value)
		{
			IsArchive = value;
			foreach (var child in Childs)
				child.SetIsArchiveRecursively(value);
		}
		
		private bool isChildsFetched = false;
		public virtual void FetchChilds(IUnitOfWork uow)
		{
			if(isChildsFetched)
				return;
			
			uow.Session.QueryOver<ExpenseCategory>().Fetch(SelectMode.Fetch, x => x.Childs).List();
			isChildsFetched = true;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;
		}

		#endregion IValidatableObject implementation

    }
}

