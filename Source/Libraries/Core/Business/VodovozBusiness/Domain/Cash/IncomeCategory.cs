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
		NominativePlural = "статьи дохода",
		Nominative = "статья дохода",
		Accusative = "статью дохода")]
	[EntityPermission]
	[HistoryTrace]
	public class IncomeCategory : PropertyChangedBase, IDomainObject, IValidatableObject, IFinancialCategory
	{
		private const int _maxNameLength = 45;

		private string _name;
		private string _numbering;
		private Subdivision _subdivision;
		private bool _isArchive;
		private IncomeInvoiceDocumentType _incomeDocumentType;
		private IncomeCategory _parent;
		private IList<IncomeCategory> _childs;
		private bool _isChildsFetched = false;
		private int? _financialCategoryGroupId;

		public IncomeCategory()
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

		[Display(Name = "Нумерация")]
		public virtual string Numbering
		{
			get => _numbering;
			set => SetField(ref _numbering, value);
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

		/// <summary>
		/// Тип приходного ордера для котором возможно будет выбрать эту категорию
		/// </summary>
		[Required(ErrorMessage = "Должно быть заполнен тип приходного ордера.")]
		[Display(Name = "Тип приходного ордера")]
		public virtual IncomeInvoiceDocumentType IncomeDocumentType
		{
			get => _incomeDocumentType;
			set => SetField(ref _incomeDocumentType, value);
		}

		[Display(Name = "Родительская группа")]
		public virtual IncomeCategory Parent
		{
			get => _parent;
			set => SetField(ref _parent, value);
		}

		[Display(Name = "Дочерние группы")]
		public virtual IList<IncomeCategory> Childs
		{
			get => _childs;
			set => SetField(ref _childs, value);
		}

		public virtual FinancialCategoryTypeEnum FinancialCategoryType => FinancialCategoryTypeEnum.IncomeCategory;

		public virtual int? FinancialIncomeCategoryId
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
