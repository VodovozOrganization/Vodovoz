using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "статьи дохода",
		Nominative = "статья дохода",
		Accusative = "статью дохода",
		Genitive = "финансовой статьи дохода")]
	[EntityPermission]
	[HistoryTrace]
	public class FinancialIncomeCategory : PropertyChangedBase, IDomainObject
	{
		private int? _parentId;
		private string _title;
		private bool _isArchive;
		private TargetDocument _targetDocument;
		private int? _subdivisionId;
		private string _numbering;
		private bool _excludeFromCashFlowDds;
		private bool _isHiddenFromPublicAccess;

		[Display(Name = "Код")]
		public virtual int Id { get; }

		[Display(Name = "Родительская группа")]
		[HistoryIdentifier(TargetType = typeof(FinancialCategoriesGroup))]
		public virtual int? ParentId
		{
			get => _parentId;
			set => SetField(ref _parentId, value);
		}

		[Display(Name = "Название")]
		public virtual string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Тип документа")]
		public virtual TargetDocument TargetDocument
		{
			get => _targetDocument;
			set => SetField(ref _targetDocument, value);
		}

		[Display(Name = "Подразделение")]
		[HistoryIdentifier(TargetType = typeof(Subdivision))]
		public virtual int? SubdivisionId
		{
			get => _subdivisionId;
			set => SetField(ref _subdivisionId, value);
		}

		[Display(Name = "Нумерация")]
		[MaxLength(150)]
		public virtual string Numbering
		{
			get => _numbering;
			set => SetField(ref _numbering, value);
		}

		[Display(Name = "Тип группы")]
		public virtual GroupType GroupType => GroupType.Category;

		[Display(Name = "Приход/расход")]
		public virtual FinancialSubType FinancialSubtype => FinancialSubType.Income;

		[Display(Name = "Не включать в ДДС")]
		public virtual bool ExcludeFromCashFlowDds
		{
			get => _excludeFromCashFlowDds;
			set => SetField(ref _excludeFromCashFlowDds, value);
		}

		[Display(Name = "Скрыта из общего доступа")]
		public virtual bool IsHiddenFromPublicAccess
		{
			get => _isHiddenFromPublicAccess;
			set => SetField(ref _isHiddenFromPublicAccess, value);
		}
	}
}
