using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "статьи расхода",
		Nominative = "статья расхода",
		Accusative = "статью расхода")]
	[EntityPermission]
	[HistoryTrace]
	public class ExpenseCategory : PropertyChangedBase, IDomainObject, IValidatableObject, IFinancialCategory
	{
		private const int _maxNameLength = 45;

		private string _name;
		private ExpenseInvoiceDocumentType _expenseDocumentType;
		private Subdivision _subdivision;
		private bool _isArchive;
		private string _numbering;
		private ExpenseCategory _parent;
		private IList<ExpenseCategory> _childs;
		private bool _isChildsFetched = false;
		private int? _financialCategoryGroupId;

		public ExpenseCategory()
		{
			Name = string.Empty;
		}

		public virtual int Id { get; }

		public virtual string Title => $"{Name}";

		[Required(ErrorMessage = "Название статьи должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Тип расходного ордера для которого возможно будет выбрать эту категорию
		/// </summary>
		[Required(ErrorMessage = "Должно быть заполнен тип расходного ордера.")]
		[Display(Name = "Тип расходного ордера")]
		public virtual ExpenseInvoiceDocumentType ExpenseDocumentType
		{
			get => _expenseDocumentType;
			set => SetField(ref _expenseDocumentType, value);
		}

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		[Display(Name = "Категория архивирована")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Нумерация")]
		public virtual string Numbering
		{
			get => _numbering;
			set => SetField(ref _numbering, value);
		}

		[Display(Name = "Родительская группа")]
		public virtual ExpenseCategory Parent
		{
			get => _parent;
			set => SetField(ref _parent, value);
		}

		[Display(Name = "Дочерние группы")]
		public virtual IList<ExpenseCategory> Childs
		{
			get => _childs;
			set => SetField(ref _childs, value);
		}

		public virtual FinancialCategoryTypeEnum FinancialCategoryType => FinancialCategoryTypeEnum.ExpenseCategory;

		public virtual int? FinancialCategoryGroupId
		{
			get => _financialCategoryGroupId;
			set => SetField(ref _financialCategoryGroupId, value);
		}

		public virtual void SetIsArchiveRecursively(bool value)
		{
			IsArchive = value;
			foreach(var child in Childs)
			{
				child.SetIsArchiveRecursively(value);
			}
		}

		public virtual void FetchChilds(IUnitOfWork uow)
		{
			if(_isChildsFetched)
			{
				return;
			}

			uow.Session.QueryOver<ExpenseCategory>().Fetch(SelectMode.Fetch, x => x.Childs).List();
			_isChildsFetched = true;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Name?.Length > _maxNameLength)
			{
				yield return new ValidationResult($"Длина названия статьи превышена на {Name.Length - _maxNameLength}");
			}
		}

		#endregion IValidatableObject implementation
	}
}
