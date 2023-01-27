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
		NominativePlural = "статьи дохода",
		Nominative = "статья дохода")]
	[EntityPermission]
	[HistoryTrace]
	public class IncomeCategory : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _nameLimit = 45;

		public IncomeCategory ()
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
			get => name;
			set => SetField (ref name, value);
		}
		
		private string numbering;
		[Display(Name = "Нумерация")]
		public virtual string Numbering {
			get => numbering;
			set => SetField(ref numbering, value);
		}
		
        
		Subdivision subdivision;
        [Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField (ref subdivision, value);
		}
		
		private bool isArchive;
		[Display(Name = "Категория архивирована")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}
        
        IncomeInvoiceDocumentType incomeDocumentType;
        /// <summary>
        /// Тип приходного ордера для котором возможно будет выбрать эту категорию
        /// </summary>
        [Required(ErrorMessage = "Должно быть заполнен тип приходного ордера.")]
        [Display(Name = "Тип приходного ордера")]
        public virtual IncomeInvoiceDocumentType IncomeDocumentType {
            get => incomeDocumentType;
			set => SetField(ref incomeDocumentType, value);
		}
		
        #endregion

		#region ParentChilds
		
		private IncomeCategory parent;
		[Display(Name = "Родительская группа")]
		public virtual IncomeCategory Parent {
			get => parent;
			set => SetField(ref parent, value);
		}
		
		private IList<IncomeCategory> childs;
		[Display(Name = "Дочерние группы")]
		public virtual IList<IncomeCategory> Childs {
			get => childs;
			set => SetField(ref childs, value);
		}
		
		#endregion

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
			if(Name?.Length > _nameLimit)
			{
				yield return new ValidationResult($"Длина названия статьи превышена на {Name.Length - _nameLimit}");
			}
		}
		
		#endregion IValidatableObject implementation

	}
}

